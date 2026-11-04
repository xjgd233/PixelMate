$ErrorActionPreference = "Stop"

$env:AVALONIA_TELEMETRY_OPTOUT = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

$projectRoot = Split-Path -Parent $PSScriptRoot
$dotnet = if ($env:DOTNET_ROOT) {
    Join-Path $env:DOTNET_ROOT "dotnet.exe"
} else {
    (Get-Command dotnet -ErrorAction Stop).Source
}
$outputDir = Join-Path $projectRoot "artifacts/PixelMate-win-x64"
$archivePath = Join-Path $projectRoot "artifacts/PixelMate-1.0.0-win-x64.zip"

if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}

& $dotnet publish `
    (Join-Path $projectRoot "src/PixelMate/PixelMate.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    "-p:DebugType=None" `
    "-p:DebugSymbols=false" `
    -o $outputDir `
    -m:1 `
    -nodeReuse:false `
    "-p:UseSharedCompilation=false" `
    -v:minimal

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$requiredFiles = @(
    "PixelMate.exe",
    "PixelMate.dll",
    "libSkiaSharp.dll",
    "libHarfBuzzSharp.dll",
    "av_libglesv2.dll"
)
foreach ($requiredFile in $requiredFiles) {
    $path = Join-Path $outputDir $requiredFile
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "PixelMate publish output is missing $requiredFile"
    }
}

Get-ChildItem -Path $outputDir -File -Filter "*.pdb" | Remove-Item
Copy-Item (Join-Path $projectRoot "README.md") (Join-Path $outputDir "README.md")
$releaseDocsDir = Join-Path $outputDir "docs"
New-Item -ItemType Directory -Path $releaseDocsDir -Force | Out-Null
Copy-Item `
    (Join-Path $projectRoot "docs/phase-status.md") `
    (Join-Path $releaseDocsDir "phase-status.md")
Copy-Item `
    (Join-Path $projectRoot "docs/release-notes-1.0.0.md") `
    (Join-Path $releaseDocsDir "release-notes-1.0.0.md")
Copy-Item `
    (Join-Path $projectRoot "docs/windows-acceptance-checklist.md") `
    (Join-Path $releaseDocsDir "windows-acceptance-checklist.md")
Copy-Item `
    (Join-Path $projectRoot "docs/release-notes-1.0.0.md") `
    (Join-Path $outputDir "RELEASE-NOTES.md")
Copy-Item `
    (Join-Path $projectRoot "docs/windows-acceptance-checklist.md") `
    (Join-Path $outputDir "WINDOWS-ACCEPTANCE.md")

if (Test-Path $archivePath) {
    Remove-Item $archivePath -Force
}
Compress-Archive `
    -Path $outputDir `
    -DestinationPath $archivePath `
    -CompressionLevel Optimal

Write-Host "Windows x64 publish output: $outputDir"
Write-Host "Windows x64 release archive: $archivePath"
