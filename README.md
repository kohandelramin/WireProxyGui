# WireProxy GUI

A simple Windows desktop application for running a WireGuard configuration through HTTP and SOCKS5 proxies using `wireproxy`.

WireProxy GUI is designed for users who want to use a WireGuard connection with selected apps or browsers without routing the whole Windows system through a VPN.

## Features

- Windows desktop GUI
- WireGuard `.conf` file selection
- HTTP proxy support
- SOCKS5 proxy support
- Local IP detection with **My IP** buttons
- Run, Restart, and Terminate controls
- Real-time logs inside the app
- Download, Upload, and Total usage counters
- Portable release, no installation required
- No terminal or PowerShell usage required for normal users

## Screenshot

Add your screenshot here:

```md
![WireProxy GUI Screenshot](docs/screenshot.png)
```

## Requirements

- Windows 10 or Windows 11
- A valid WireGuard `.conf` file
- `wireproxy.exe`, included in the release package

## Download

Download the latest ZIP file from the **Releases** section.

Recommended package:

```text
WireProxyGui-win-x64.zip
├─ WireProxyGui.exe
├─ wireproxy.exe
└─ README.txt
```

## How to Use

1. Download the release ZIP.
2. Extract the ZIP file.
3. Run `WireProxyGui.exe`.
4. Select your WireGuard `.conf` file.
5. Set HTTP and SOCKS5 bind IP and port if needed.
6. Click **Run**.
7. Configure your browser or app to use the proxy.

Default proxy examples:

```text
HTTP   127.0.0.1:25345
SOCKS5 127.0.0.1:25344
```

## Using the Proxy in Apps

You can use the generated local proxy in apps that support HTTP or SOCKS5 proxy settings.

Examples:

- Browser proxy settings
- Firefox proxy settings
- Telegram proxy settings
- Proxy management tools
- Apps that support SOCKS5 or HTTP proxy

## Sharing Proxy on Local Network

If you want another device on your local network to use the proxy, set the bind IP to your computer local IP instead of `127.0.0.1`.

Example:

```text
192.168.1.10:25345
```

Then configure the other device to use that IP and port as its proxy.

Make sure your firewall allows the selected port.

## Logs and Usage

The app shows live logs and basic traffic counters.

Log colors:

- Green: resolve or DNS related logs
- Red: errors, timeouts, or handshake issues
- Blue: normal information

Usage counters:

- Download
- Upload
- Total

These counters represent traffic passing through the active `wireproxy` connection, not total Windows system traffic.

## Important Notes

- This app is not a full system VPN client.
- Only apps configured to use the HTTP or SOCKS5 proxy will use this connection.
- `wireproxy.exe` is required for the current version.
- Keep `WireProxyGui.exe` and `wireproxy.exe` in the same folder.
- Your WireGuard `.conf` file is selected locally and is not uploaded anywhere by this app.

## Project Structure

```text
WireProxyGui/
├─ src/
│  └─ WireProxyGui/
│     ├─ Models/
│     ├─ Services/
│     ├─ Assets/
│     ├─ App.xaml
│     ├─ App.xaml.cs
│     ├─ MainWindow.xaml
│     ├─ MainWindow.xaml.cs
│     ├─ WireProxyGui.csproj
│     └─ wireproxy.exe
├─ docs/
│  └─ screenshot.png
├─ README.md
└─ README.fa.md
```

## Build from Source

Requirements:

- Windows
- .NET 8 SDK

Build and publish:

```powershell
dotnet publish .\src\WireProxyGui\WireProxyGui.csproj -c Release
```

The published files will be created under:

```text
src\WireProxyGui\bin\Release\
```

## Credits

- wireproxy: https://github.com/windtf/wireproxy
- Built with WPF and .NET 8

## License

This project is open source. Check the repository license for details.
