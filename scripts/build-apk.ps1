param(
    [string]$Configuration = "Release",
    [string]$OutputName = "TravelMate.Mobile-release.apk",
    [string]$KeystorePath = "",
    [string]$KeystorePassword = "",
    [string]$KeyAlias = "",
    [string]$KeyPassword = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\TravelMate.Mobile\TravelMate.Mobile.csproj"
$artifacts = Join-Path $repoRoot "artifacts"

$publishArgs = @(
    "publish",
    $project,
    "-f",
    "net10.0-android",
    "-c",
    $Configuration,
    "-p:AndroidPackageFormat=apk"
)

if (-not [string]::IsNullOrWhiteSpace($KeystorePath)) {
    if (-not (Test-Path $KeystorePath)) {
        throw "Keystore file was not found: $KeystorePath"
    }

    if ([string]::IsNullOrWhiteSpace($KeystorePassword) -or [string]::IsNullOrWhiteSpace($KeyAlias)) {
        throw "KeystorePassword and KeyAlias are required when KeystorePath is supplied."
    }

    $publishArgs += "-p:AndroidKeyStore=true"
    $publishArgs += "-p:AndroidSigningKeyStore=$KeystorePath"
    $publishArgs += "-p:AndroidSigningStorePass=$KeystorePassword"
    $publishArgs += "-p:AndroidSigningKeyAlias=$KeyAlias"

    if (-not [string]::IsNullOrWhiteSpace($KeyPassword)) {
        $publishArgs += "-p:AndroidSigningKeyPass=$KeyPassword"
    }
}

New-Item -ItemType Directory -Force -Path $artifacts | Out-Null
dotnet @publishArgs

$apk = Get-ChildItem -Recurse -File (Join-Path $repoRoot "src\TravelMate.Mobile\bin\$Configuration\net10.0-android") -Include "*-Signed.apk" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $apk) {
    throw "APK was not generated."
}

$target = Join-Path $artifacts $OutputName
Copy-Item -LiteralPath $apk.FullName -Destination $target -Force

Write-Host "APK generated:" -ForegroundColor Cyan
Get-Item $target | Select-Object FullName,Length,LastWriteTime
