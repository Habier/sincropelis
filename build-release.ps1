param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ".\release"
)

$ErrorActionPreference = "Stop"

Write-Host "Building SincroPelis..." -ForegroundColor Cyan

$projectPath = ".\SincroPelis\SincroPelis.csproj"

dotnet publish $projectPath -c $Configuration -o "$OutputDir\publish-full"

Write-Host "Filtering VLC plugins..." -ForegroundColor Cyan

$publishDir = "$OutputDir\publish-full"
$pluginsDir = "$publishDir\libvlc\win-x64\plugins"

$requiredPlugins = @(
    "audio_output",
    "video_output", 
    "video_filter",
    "demux",
    "codec",
    "video_splitter",
    "meta_engine",
    "video_chroma",
    "access",
    "access_output",
    "packetizer",
    "stream_filter",
    "text_renderer"
)

Get-ChildItem $pluginsDir -Directory | ForEach-Object {
    $pluginName = $_.Name
    if ($requiredPlugins -notcontains $pluginName) {
        Write-Host "  Removing: $pluginName" -ForegroundColor Gray
        Remove-Item $_.FullName -Recurse -Force
    }
}

$releaseDir = "$OutputDir\SincroPelis-v1.0"
if (Test-Path $releaseDir) {
    Remove-Item $releaseDir -Recurse -Force
}

Copy-Item "$publishDir\*" $releaseDir -Recurse

Write-Host "Creating ZIP..." -ForegroundColor Cyan

$zipPath = "$OutputDir\SincroPelis-v1.0.zip"
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
