param(
    [Parameter(Mandatory = $true)]
    [string]$CertificateThumbprint,

    [string]$OutputDir = "",

    [switch]$MachineCertificateStore
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $projectRoot "artifacts/PixelMate-win-x64"
}
$archivePath = Join-Path $projectRoot "artifacts/PixelMate-1.0.1-win-x64.zip"
$offlineArchivePath = Join-Path $projectRoot "artifacts/PixelMate-1.0.1-win-x64-offline.zip"

if (-not (Test-Path $OutputDir -PathType Container)) {
    throw "Publish directory does not exist: $OutputDir"
}

$signTool = (Get-Command signtool.exe -ErrorAction SilentlyContinue).Source
if (-not $signTool) {
    $windowsKits = Join-Path ${env:ProgramFiles(x86)} "Windows Kits/10/bin"
    if (Test-Path $windowsKits) {
        $signTool = Get-ChildItem `
            -Path (Join-Path $windowsKits "*/x64/signtool.exe") `
            -File |
            Sort-Object FullName -Descending |
            Select-Object -First 1 -ExpandProperty FullName
    }
}
if (-not $signTool) {
    throw "signtool.exe was not found. Install the Windows SDK signing tools."
}

$normalizedThumbprint = $CertificateThumbprint -replace '[\s:-]', ''
if ($normalizedThumbprint.Length -lt 40) {
    throw "CertificateThumbprint does not look like a SHA-1 certificate thumbprint."
}

$targets = @(
    (Join-Path $OutputDir "PixelMate.exe"),
    (Join-Path $OutputDir "PixelMate.dll")
)
foreach ($target in $targets) {
    if (-not (Test-Path $target -PathType Leaf)) {
        throw "Signing target is missing: $target"
    }

    $signArguments = @(
        "sign",
        "/sha1", $normalizedThumbprint,
        "/fd", "SHA256",
        "/tr", "http://timestamp.digicert.com",
        "/td", "SHA256",
        "/v"
    )
    if ($MachineCertificateStore) {
        $signArguments += "/sm"
    }
    $signArguments += $target

    & $signTool @signArguments
    if ($LASTEXITCODE -ne 0) {
        throw "signtool sign failed for $target with exit code $LASTEXITCODE"
    }

    & $signTool verify /pa /all /v $target
    if ($LASTEXITCODE -ne 0) {
        throw "Authenticode verification failed for $target with exit code $LASTEXITCODE"
    }
}

if (Test-Path $archivePath) {
    Remove-Item $archivePath -Force
}
if (Test-Path $offlineArchivePath) {
    Remove-Item $offlineArchivePath -Force
}
Compress-Archive `
    -Path $OutputDir `
    -DestinationPath $archivePath `
    -CompressionLevel Optimal
Copy-Item $archivePath $offlineArchivePath

$archiveHash = (Get-FileHash $archivePath -Algorithm SHA256).Hash
Write-Host "Signed release archive: $archivePath"
Write-Host "Signed offline archive: $offlineArchivePath"
Write-Host "SHA-256: $archiveHash"
