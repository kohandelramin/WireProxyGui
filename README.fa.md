# WireProxy GUI

یک رابط گرافیکی ساده برای ویندوز جهت اجرا و مدیریت `wireproxy` با روند راه‌اندازی راحت‌تر.

## این برنامه چه کاری انجام می‌دهد

WireProxy GUI به کاربر کمک می‌کند تا:

- فایل WireGuard با پسوند `.conf` را انتخاب کند
- تنظیمات Bind IP و Port برای HTTP و SOCKS5 را انجام دهد
- بدون استفاده از ترمینال، `wireproxy` را اجرا یا متوقف کند
- لاگ‌ها را به صورت زنده داخل خود برنامه ببیند
- فرایند را با دکمه‌های Run ،Restart و Terminate کنترل کند

## نکته مهم

این برنامه فقط یک رابط گرافیکی برای `wireproxy` است.

این GUI به تنهایی و به صورت مستقل کار نمی‌کند.
برای اجرا، لازم است فایل `wireproxy.exe` کنار فایل اجرایی برنامه قرار داشته باشد، مگر اینکه بعدا ساختار پروژه را تغییر دهید و آن قابلیت را داخل برنامه ادغام کنید.

پس بسته نهایی مناسب برای کاربران باید شامل این دو فایل باشد:

- `WireProxyGui.exe`
- `wireproxy.exe`

## قابلیت‌ها

- انتخاب فایل WireGuard با پسوند `.conf`
- تنظیم Bind IP و Port برای HTTP
- تنظیم Bind IP و Port برای SOCKS5
- دکمه **My IP** برای پر کردن خودکار IP سیستم
- دکمه‌های Run ،Restart و Terminate
- نمایش لاگ زنده داخل برنامه
- سورس متن باز برای توسعه و تغییرات بعدی

## اسکرین‌شات

بعد از قرار دادن تصویر در ریپو، این بخش را استفاده کنید.

```md
![تصویر برنامه](./docs/screenshot.png)
```

## آیکن برنامه

مسیر پیشنهادی:

```text
src/WireProxyGui/Assets/app.ico
```

## ساختار پروژه

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

## پیش‌نیاز توسعه محلی

- ویندوز
- .NET 8 SDK
- فایل `wireproxy.exe`

## اجرای محلی

از ریشه پروژه این دستورات را اجرا کنید:

```powershell
dotnet restore .\src\WireProxyGui\WireProxyGui.csproj
dotnet build .\src\WireProxyGui\WireProxyGui.csproj -c Release
dotnet run --project .\src\WireProxyGui\WireProxyGui.csproj
```

## ساخت خروجی قابل حمل ویندوز به صورت محلی

```powershell
dotnet publish .\src\WireProxyGui\WireProxyGui.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

خروجی نهایی معمولا در این مسیر ساخته می‌شود:

```text
src\WireProxyGui\bin\Release\net8.0-windows\win-x64\publish\
```

## بسته پیشنهادی برای انتشار

چون این رابط گرافیکی به `wireproxy.exe` وابسته است، پیشنهاد می‌شود فایل نهایی برای کاربر به صورت ZIP منتشر شود، مثلا:

```text
WireProxyGui-win-x64.zip
├─ WireProxyGui.exe
├─ wireproxy.exe
├─ README.txt
```

این بهترین مدل انتشار است، چون:

- کاربر فقط یک ZIP دانلود می‌کند
- فایل را از حالت فشرده خارج می‌کند
- `WireProxyGui.exe` را اجرا می‌کند
- نیازی به build یا استفاده از ترمینال ندارد

## پیشنهاد برای GitHub Release

برای انتشار عمومی، بهتر است **فایل ZIP** منتشر کنید، نه فقط فایل EXE تنها.

### عنوان پیشنهادی ریلیز

```text
v1.0.0
```

### فایل پیشنهادی برای ریلیز

- `WireProxyGui-win-x64.zip`

### متن کوتاه پیشنهادی برای توضیحات ریلیز

```text
اولین نسخه عمومی WireProxy GUI

فایل‌های داخل بسته:
- WireProxyGui.exe
- wireproxy.exe

قابلیت‌ها:
- انتخاب فایل WireGuard .conf
- تنظیم HTTP و SOCKS5
- دکمه My IP
- دکمه‌های Run ،Restart و Terminate
- نمایش لاگ زنده داخل برنامه

روش استفاده:
فایل ZIP را از حالت فشرده خارج کنید و WireProxyGui.exe را اجرا کنید.
```

## راه‌اندازی Git

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

## ساخت تگ نسخه

```bash
git tag v1.0.0
git push origin v1.0.0
```

## هدف متن باز پروژه

ساختار این پروژه به شکلی در نظر گرفته شده که:

- کاربر عادی بتواند از خروجی EXE قابل حمل استفاده کند
- توسعه‌دهنده بتواند ریپو را clone کند و بعدا کد را تغییر دهد

## لایسنس

لایسنس متن باز موردنظر خودتان را اینجا اضافه کنید، مثلا MIT.

## سازنده و ارجاع

- RK Github: https://github.com/kohandelramin
- `wireproxy` یک وابستگی جداگانه است که این رابط گرافیکی از آن استفاده می‌کند
