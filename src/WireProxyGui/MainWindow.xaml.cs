using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using WireProxyGui.Models;
using WireProxyGui.Services;

namespace WireProxyGui;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly ProcessService _processService;
    private readonly DispatcherTimer _logPollTimer;
    private readonly DispatcherTimer _metricsTimer;
    private readonly string _baseDirectory;
    private readonly string _generatedConfigPath;
    private readonly string _wireProxyExePath;

    public ObservableCollection<LogEntry> Logs { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        _baseDirectory = AppContext.BaseDirectory;
        _generatedConfigPath = Path.Combine(_baseDirectory, "WireproxySetting.conf");
        _wireProxyExePath = Path.Combine(_baseDirectory, "wireproxy.exe");

        _settingsService = new SettingsService(_baseDirectory);
        _processService = new ProcessService(_baseDirectory);
        _processService.LogReceived += OnLogReceived;
        _processService.ProcessExited += OnProcessExited;

        _logPollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _logPollTimer.Tick += (_, _) => _processService.ReadNewLogLines();

        _metricsTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _metricsTimer.Tick += async (_, _) => await RefreshMetricsAsync();

        LogsDataGrid.ItemsSource = Logs;

        LoadSettings();
        UpdateUiState();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Load();
        ConfigPathTextBox.Text = settings.ConfigPath;
        HttpIpTextBox.Text = string.IsNullOrWhiteSpace(settings.HttpIp) ? ProcessService.GetBestLocalIpv4() : settings.HttpIp;
        HttpPortTextBox.Text = settings.HttpPort.ToString();
        SocksIpTextBox.Text = string.IsNullOrWhiteSpace(settings.SocksIp) ? ProcessService.GetBestLocalIpv4() : settings.SocksIp;
        SocksPortTextBox.Text = settings.SocksPort.ToString();

        AddInfo("Loaded previous settings.");
    }

    private void SaveSettings()
    {
        _settingsService.Save(new AppSettings
        {
            ConfigPath = ConfigPathTextBox.Text.Trim(),
            HttpIp = HttpIpTextBox.Text.Trim(),
            HttpPort = int.TryParse(HttpPortTextBox.Text, out var httpPort) ? httpPort : 25345,
            SocksIp = SocksIpTextBox.Text.Trim(),
            SocksPort = int.TryParse(SocksPortTextBox.Text, out var socksPort) ? socksPort : 25344
        });
    }

    private void UpdateUiState()
    {
        var running = _processService.IsRunning;

        ConfigPathTextBox.IsEnabled = !running;
        BrowseButton.IsEnabled = !running;
        HttpIpTextBox.IsEnabled = !running;
        HttpPortTextBox.IsEnabled = !running;
        SocksIpTextBox.IsEnabled = !running;
        SocksPortTextBox.IsEnabled = !running;
        HttpMyIpButton.IsEnabled = !running;
        SocksMyIpButton.IsEnabled = !running;

        RunButton.IsEnabled = !running;
        RestartButton.IsEnabled = true;
        TerminateButton.IsEnabled = running;

        StatusTextBlock.Text = running ? "Running" : "Stopped";
        DownloadTextBlock.Text = running ? DownloadTextBlock.Text : "0 B";
        UploadTextBlock.Text = running ? UploadTextBlock.Text : "0 B";
        TotalTextBlock.Text = running ? TotalTextBlock.Text : "0 B";
    }

    private bool TryValidateInputs(out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(ConfigPathTextBox.Text) || !File.Exists(ConfigPathTextBox.Text))
        {
            error = "Please select a valid .conf file.";
            return false;
        }

        if (!int.TryParse(HttpPortTextBox.Text, out var httpPort) || httpPort is < 1 or > 65535)
        {
            error = "HTTP port must be between 1 and 65535.";
            return false;
        }

        if (!int.TryParse(SocksPortTextBox.Text, out var socksPort) || socksPort is < 1 or > 65535)
        {
            error = "SOCKS5 port must be between 1 and 65535.";
            return false;
        }

        return true;
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TryValidateInputs(out var error))
            {
                MessageBox.Show(error, "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveSettings();
            _processService.KillOtherWireProxyProcesses();

            var config = _processService.BuildConfig(
                ConfigPathTextBox.Text.Trim(),
                HttpIpTextBox.Text.Trim(),
                int.Parse(HttpPortTextBox.Text),
                SocksIpTextBox.Text.Trim(),
                int.Parse(SocksPortTextBox.Text));

            _processService.Start(_wireProxyExePath, _generatedConfigPath, config);
            _logPollTimer.Start();
            _metricsTimer.Start();
            UpdateUiState();

            await RefreshMetricsAsync();
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            MessageBox.Show(ex.ToString(), "Run failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _processService.Stop();
            _logPollTimer.Stop();
            _metricsTimer.Stop();
            UpdateUiState();
            RunButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            MessageBox.Show(ex.ToString(), "Restart failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TerminateButton_Click(object sender, RoutedEventArgs e)
    {
        _processService.Stop();
        _logPollTimer.Stop();
        _metricsTimer.Stop();
        DownloadTextBlock.Text = "0 B";
        UploadTextBlock.Text = "0 B";
        TotalTextBlock.Text = "0 B";
        UpdateUiState();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "WireGuard Config (*.conf)|*.conf|All files (*.*)|*.*",
            Title = "Select WireGuard config file"
        };

        if (dialog.ShowDialog(this) == true)
        {
            ConfigPathTextBox.Text = dialog.FileName;
            AddInfo($"Selected config: {dialog.FileName}");
        }
    }

    private void HttpMyIpButton_Click(object sender, RoutedEventArgs e)
    {
        var ip = ProcessService.GetBestLocalIpv4();
        HttpIpTextBox.Text = ip;
        AddInfo($"HTTP Bind IP set to {ip}");
    }

    private void SocksMyIpButton_Click(object sender, RoutedEventArgs e)
    {
        var ip = ProcessService.GetBestLocalIpv4();
        SocksIpTextBox.Text = ip;
        AddInfo($"SOCKS5 Bind IP set to {ip}");
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _baseDirectory,
            UseShellExecute = true
        });
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void HelpHyperlink_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "1. Select your WireGuard .conf file.\n" +
            "2. Set HTTP and SOCKS5 bind IP and port if needed.\n" +
            "3. Click Run.\n" +
            "4. Configure your browser or app to use the proxy.\n" +
            "5. Use Restart or Terminate when needed.",
            "Help",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void OnLogReceived(LogEntry entry)
    {
        Dispatcher.Invoke(() =>
        {
            Logs.Add(entry);
            if (Logs.Count > 2000)
            {
                Logs.RemoveAt(0);
            }
            LogsDataGrid.ScrollIntoView(entry);
        });
    }

    private void OnProcessExited(int exitCode)
    {
        Dispatcher.Invoke(() =>
        {
            AddInfo($"wireproxy exited with code {exitCode}");
            _logPollTimer.Stop();
            _metricsTimer.Stop();
            DownloadTextBlock.Text = "0 B";
            UploadTextBlock.Text = "0 B";
            TotalTextBlock.Text = "0 B";
            UpdateUiState();
        });
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }

    private async Task RefreshMetricsAsync()
    {
        if (!_processService.IsRunning)
        {
            DownloadTextBlock.Text = "0 B";
            UploadTextBlock.Text = "0 B";
            TotalTextBlock.Text = "0 B";
            return;
        }

        var metrics = await _processService.GetMetricsAsync();
        if (!metrics.IsAvailable)
        {
            return;
        }

        DownloadTextBlock.Text = FormatBytes(metrics.RxBytes);
        UploadTextBlock.Text = FormatBytes(metrics.TxBytes);
        TotalTextBlock.Text = FormatBytes(metrics.RxBytes + metrics.TxBytes);
    }

    private void AddInfo(string message) => OnLogReceived(new LogEntry { Level = "Info", Message = message });
    private void AddError(string message) => OnLogReceived(new LogEntry { Level = "Error", Message = message });

    protected override void OnClosed(EventArgs e)
    {
        _logPollTimer.Stop();
        _metricsTimer.Stop();
        _processService.Stop();
        base.OnClosed(e);
    }
}
