# Android APK

Generate a local APK:

```powershell
.\scripts\build-apk.ps1
```

The APK is copied to:

```text
artifacts\TravelMate.Mobile-release.apk
```

## Local API From Android

The APK can edit its API URL from the `Connection` panel.

Use these common values:

- Android emulator: `http://10.0.2.2:5068/`
- Windows desktop app: `http://localhost:5068/`
- Physical phone on the same Wi-Fi: `http://<your-computer-ip>:5068/`
- Azure deployment later: `https://<your-api-host>/`

For a physical phone, start the API on all interfaces:

```powershell
dotnet run --project .\src\TravelMate.Api\TravelMate.Api.csproj --urls http://0.0.0.0:5068
```

Then allow port `5068` through Windows Firewall if the phone cannot connect.

## Install With ADB

```powershell
adb install -r .\artifacts\TravelMate.Mobile-release.apk
```

After install:

1. Open TravelMate.
2. Enter the API base URL in `Connection`.
3. Tap `Test connection`.
4. Use `Start demo trip`.
