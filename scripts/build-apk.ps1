param(
    [string]$Configuration = "Release",
    [string]$OutputName = "TravelMate.Mobile-release.apk"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\TravelMate.Mobile\TravelMate.Mobile.csproj"
$artifacts = Join-Path $repoRoot "artifacts"

New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

dotnet publish $project `
    -f net10.0-android `
    -c $Configuration `
    -p:AndroidPackageFormat=apk

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
