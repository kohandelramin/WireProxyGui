# WireProxy GUI

A simple Windows desktop GUI for running and managing `wireproxy` with a cleaner setup flow.

## What this app does

WireProxy GUI helps users:

- choose a WireGuard `.conf` file
- configure HTTP and SOCKS5 bind IP and port
- start and stop `wireproxy` without using terminal commands
- view live logs inside the app
- restart and terminate the running process from a simple Windows interface

## Important note

This GUI is a wrapper for `wireproxy`.

It **does not work standalone** by itself.
The app needs `wireproxy.exe` to be present beside the GUI executable, unless you redesign the project later to embed or reimplement that functionality.

So the normal release package for users should contain both:

- `WireProxyGui.exe`
- `wireproxy.exe`

## Features

- WireGuard `.conf` file selection
- HTTP proxy bind settings
- SOCKS5 proxy bind settings
- **My IP** helper buttons
- Run, Restart, and Terminate controls
- Live logs inside the GUI
- Open source codebase for future modification

## Screenshot

Add your screenshot after you place it in the repository.

```md
![App Screenshot](./docs/screenshot.png)
```

## App icon

Recommended path:

```text
src/WireProxyGui/Assets/app.ico
```

## Project structure

```text
WireProxyGui/
├─ src/
│  └─ WireProxyGui/
│     ├─ App.xaml
│     ├─ App.xaml.cs
│     ├─ MainWindow.xaml
│     ├─ MainWindow.xaml.cs
│     ├─ WireProxyGui.csproj
│     ├─ Models/
│     ├─ Services/
│     ├─ Assets/
│     │  └─ app.ico
│     └─ wireproxy.exe
├─ docs/
│  └─ screenshot.png
├─ README.md
├─ README.fa.md
└─ .github/
   └─ workflows/
      └─ build-release.yml
```

## Requirements for local development

- Windows
- .NET 8 SDK
- `wireproxy.exe`

## Run locally

From the project root:

```powershell
dotnet restore .\src\WireProxyGui\WireProxyGui.csproj
dotnet build .\src\WireProxyGui\WireProxyGui.csproj -c Release
dotnet run --project .\src\WireProxyGui\WireProxyGui.csproj
```

## Build a portable Windows release locally

```powershell
dotnet publish .\src\WireProxyGui\WireProxyGui.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The published files will usually be located in:

```text
src\WireProxyGui\bin\Release\net8.0-windows\win-x64\publish\
```

## Recommended release package

Since the GUI depends on `wireproxy.exe`, the recommended user download is a ZIP file such as:

```text
WireProxyGui-win-x64.zip
├─ WireProxyGui.exe
├─ wireproxy.exe
├─ README.txt
```

This is the best release model because:

- users download one ZIP
- users extract it anywhere
- users run `WireProxyGui.exe`
- no build tools or terminal usage are needed

## GitHub release recommendation

For public sharing, publish a **ZIP release**, not only the EXE by itself.

### Recommended release title

```text
v1.0.0
```

### Recommended release assets

- `WireProxyGui-win-x64.zip`

### Recommended short release description

```text
First public release of WireProxy GUI.

Included:
- WireProxyGui.exe
- wireproxy.exe

Features:
- WireGuard .conf file selection
- HTTP and SOCKS5 bind setup
- My IP helper buttons
- Run, Restart, and Terminate controls
- Live logs inside the GUI

Usage:
Extract the ZIP file and run WireProxyGui.exe.
```

## Git setup

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

## Create a release tag

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Open source goal

This project is structured so that:

- normal users can use a portable EXE package
- developers can clone the repository and modify the source code later

## License

Add your preferred open source license here, for example MIT.

## Credits

- RK Github: https://github.com/kohandelramin
- `wireproxy` is a separate dependency used by this GUI
