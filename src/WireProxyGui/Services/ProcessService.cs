using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using WireProxyGui.Models;

namespace WireProxyGui.Services;

public sealed class ProcessService
{
    private Process? _process;
    private readonly string _baseDirectory;
    private readonly string _runtimeLogPath;
    private long _lastPosition;
    private readonly HttpClient _httpClient = new();
    private readonly string _metricsBaseUrl = "http://127.0.0.1:9080";

    public event Action<LogEntry>? LogReceived;
    public event Action<int>? ProcessExited;

    public ProcessService(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _runtimeLogPath = Path.Combine(baseDirectory, "wireproxy-runtime.log");
    }

    public bool IsRunning => _process is { HasExited: false };

    public static string GetBestLocalIpv4()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            var props = ni.GetIPProperties();
            foreach (var ua in props.UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ua.Address) &&
                    !ua.Address.ToString().StartsWith("169.254."))
                {
                    return ua.Address.ToString();
                }
            }
        }

        return "127.0.0.1";
    }

    public void KillOtherWireProxyProcesses()
    {
        foreach (var process in Process.GetProcessesByName("wireproxy"))
        {
            try
            {
                process.Kill(true);
                WriteLog("Info", $"Terminated previous wireproxy instance, PID {process.Id}");
            }
            catch (Exception ex)
            {
                WriteLog("Error", $"Failed to terminate previous wireproxy instance, PID {process.Id}, {ex.Message}");
            }
        }
    }

    public string BuildConfig(string configPath, string httpIp, int httpPort, string socksIp, int socksPort)
    {
        return $"""
WGConfig = {configPath}

[HTTP]
BindAddress = {httpIp}:{httpPort}

[Socks5]
BindAddress = {socksIp}:{socksPort}
""";
    }

    public void Start(string wireProxyExePath, string generatedConfigPath, string configContent)
    {
        if (IsRunning)
        {
            return;
        }

        File.WriteAllText(generatedConfigPath, configContent);
        File.WriteAllText(_runtimeLogPath, string.Empty);
        _lastPosition = 0;

        var cmdArgs = $"/c \"\"{wireProxyExePath}\" -i 127.0.0.1:9080 -c \"{generatedConfigPath}\" 1>>\"{_runtimeLogPath}\" 2>>&1\"";

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = cmdArgs,
            WorkingDirectory = _baseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _process = new Process
        {
            StartInfo = psi,
            EnableRaisingEvents = true
        };

        _process.Exited += (_, _) =>
        {
            var code = _process?.ExitCode ?? -1;
            ReadNewLogLines();
            ProcessExited?.Invoke(code);
        };

        if (!_process.Start())
        {
            throw new InvalidOperationException("Failed to start wireproxy.");
        }

        WriteLog("Info", "wireproxy started.");
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            WriteLog("Info", "No running wireproxy process found.");
            return;
        }

        try
        {
            _process?.Kill(true);
            _process?.WaitForExit(3000);
            ReadNewLogLines();
            WriteLog("Info", "wireproxy terminated.");
        }
        catch (Exception ex)
        {
            WriteLog("Error", $"Failed to terminate wireproxy, {ex.Message}");
        }
    }

    public void ReadNewLogLines()
    {
        if (!File.Exists(_runtimeLogPath))
        {
            return;
        }

        using var fs = new FileStream(_runtimeLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (_lastPosition > fs.Length)
        {
            _lastPosition = 0;
        }

        fs.Seek(_lastPosition, SeekOrigin.Begin);
        using var sr = new StreamReader(fs);

        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (ShouldSkipLogLine(line))
            {
                continue;
            }

            var level = ClassifyLevel(line);
            WriteLog(level, line, useNow: true);
        }

        _lastPosition = fs.Position;
    }

	private static bool ShouldSkipLogLine(string line)
	{
		var trimmed = line.Trim();
		var lower = trimmed.ToLowerInvariant();

		if (string.IsNullOrWhiteSpace(trimmed))
		{
			return true;
		}

		if (lower.Contains("health metric request"))
		{
			return true;
		}

		return
			trimmed.StartsWith("private_key=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("public_key=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("preshared_key=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("listen_port=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("protocol_version=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("endpoint=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("last_handshake_time_sec=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("last_handshake_time_nsec=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("tx_bytes=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("rx_bytes=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("persistent_keepalive_interval=", StringComparison.OrdinalIgnoreCase) ||
			trimmed.StartsWith("allowed_ip=", StringComparison.OrdinalIgnoreCase);
	}

    public async Task<MetricsInfo> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var metricsText = await _httpClient.GetStringAsync(_metricsBaseUrl + "/metrics", cancellationToken);

            long rxBytes = 0;
            long txBytes = 0;

            foreach (var line in metricsText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (line.StartsWith("rx_bytes=", StringComparison.OrdinalIgnoreCase))
                {
                    rxBytes = ExtractKeyValueMetric(line);
                }
                else if (line.StartsWith("tx_bytes=", StringComparison.OrdinalIgnoreCase))
                {
                    txBytes = ExtractKeyValueMetric(line);
                }
            }

            return new MetricsInfo
            {
                IsAvailable = true,
                RxBytes = rxBytes,
                TxBytes = txBytes
            };
        }
        catch
        {
            return new MetricsInfo
            {
                IsAvailable = false,
                RxBytes = 0,
                TxBytes = 0
            };
        }
    }

    private static long ExtractKeyValueMetric(string line)
    {
        var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return 0;
        }

        return long.TryParse(parts[1], out var result) ? result : 0;
    }

    private static string ClassifyLevel(string line)
    {
        var lower = line.ToLowerInvariant();

        if (lower.Contains("resolving") || lower.Contains("resolve") || lower.Contains("resolved") || lower.Contains("dns"))
        {
            return "Resolve";
        }

        if (lower.Contains("handshake") || lower.Contains("timeout") || lower.Contains("timed out") || lower.Contains("error") || lower.Contains("failed"))
        {
            return "Error";
        }

        return "Info";
    }

    private void WriteLog(string level, string message, bool useNow = false)
    {
        LogReceived?.Invoke(new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message
        });
    }
}
