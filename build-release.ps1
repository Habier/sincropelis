param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ".\dist",
    [string]$Version
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Read-Host "Que version quieres usar (ej. 1.2.0)"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "La version no puede estar vacia."
}

$versionLabel = "v$Version"

Write-Host "Building SincroPelis..." -ForegroundColor Cyan

$projectPath = ".\SincroPelis\SincroPelis.csproj"

dotnet publish $projectPath -c $Configuration -o "$OutputDir\publish-full"

$releaseDir = "$OutputDir\SincroPelis-$versionLabel"
if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}

New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

Copy-Item "$OutputDir\publish-full\*" $releaseDir -Recurse

Write-Host "Removing libvlc folder..." -ForegroundColor Cyan
if (Test-Path "$releaseDir\libvlc") {
    Remove-Item "$releaseDir\libvlc" -Recurse -Force
}

Write-Host "Creating ZIP..." -ForegroundColor Cyan

$zipPath = "$OutputDir\SincroPelis-$versionLabel.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($releaseDir, $zipPath)

$zipSize = (Get-Item $zipPath).Length / 1MB
Write-Host ""
Write-Host "Done!" -ForegroundColor Green
Write-Host "  Release folder: $releaseDir" -ForegroundColor White
Write-Host "  ZIP file: $zipPath" -ForegroundColor White
Write-Host "  ZIP size: $([math]::Round($zipSize, 1)) MB" -ForegroundColor White
