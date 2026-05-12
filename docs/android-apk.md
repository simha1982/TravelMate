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

The APK targets Android 8.0/API 26 or newer because the in-app media playback package requires Android 26+. If the API is not reachable, the demo trip still shows a small offline Hyderabad/Nandi Hills story catalog so the app can open and demonstrate the flow.

## GitHub APK Artifact

The `Android APK` workflow builds the APK on every push and pull request:

```text
.github/workflows/android-apk.yml
```

After a workflow run completes, download:

```text
TravelMate.Mobile-release-apk
```

The artifact contains:

```text
TravelMate.Mobile-release.apk
```

## Optional Release Signing

For a real release APK, create an Android keystore outside the repository and add these GitHub Actions secrets:

```text
ANDROID_KEYSTORE_BASE64
ANDROID_KEYSTORE_PASSWORD
ANDROID_KEY_ALIAS
ANDROID_KEY_PASSWORD
```

`ANDROID_KEYSTORE_BASE64` should be the base64 text of the `.keystore` file. On Windows:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("travelmate-release.keystore")) | Set-Content travelmate-release.keystore.base64
```

Then run the `Android APK` workflow manually from GitHub Actions. It will produce a second artifact:

```text
TravelMate.Mobile-signed-release-apk
```

Local signed build example:

```powershell
.\scripts\build-apk.ps1 `
  -OutputName TravelMate.Mobile-signed-release.apk `
  -KeystorePath C:\secure\travelmate-release.keystore `
  -KeystorePassword "<store-password>" `
  -KeyAlias "<key-alias>" `
  -KeyPassword "<key-password>"
```
