param(
    [string]$Configuration = "Debug",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repoRoot "TravelMate.sln"
$mobileProject = Join-Path $repoRoot "src\TravelMate.Mobile\TravelMate.Mobile.csproj"

Write-Host "TravelMate mobile smoke build" -ForegroundColor Cyan
Write-Host "Repository: $repoRoot"

dotnet restore $solution

if (-not $SkipTests) {
    dotnet test $solution --configuration $Configuration --no-restore
}

dotnet build $mobileProject --configuration $Configuration --no-restore

Write-Host ""
Write-Host "Manual smoke checklist:" -ForegroundColor Cyan
Write-Host "1. Start TravelMate.Api locally."
Write-Host "2. Launch the MAUI app on Windows or Android."
Write-Host "3. Tap Start demo trip, then Next stop through the Hyderabad route."
Write-Host "4. Confirm multiple story cards load at each stop."
Write-Host "5. Play a story and confirm in-app playback starts."
Write-Host "6. Let playback finish and confirm the completion status message appears."
Write-Host "7. Tap Not interested, reload the same stop, and confirm that story is hidden."
Write-Host "8. Submit a contribution and confirm it appears in the admin moderation queue."
