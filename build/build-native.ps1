<#
.SYNOPSIS
    Clones harehare/mq at a pinned tag, builds the mq-ffi native library,
    and copies the binary for local development and testing.
.PARAMETER MqTag
    The mq release tag to build (default: v0.5.31)
#>
param(
    [string]$MqTag = "v0.5.31"
)

$ErrorActionPreference = "Stop"

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir    = Join-Path $scriptDir ".."
$repoDir    = [System.IO.Path]::GetFullPath((Join-Path $rootDir ".mq"))
$localNativeDir = [System.IO.Path]::GetFullPath((Join-Path $rootDir "src\MQNet\native"))

# Detect host RID
$arch = if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture `
            -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "arm64" } else { "x64" }

if ($IsWindows -or (-not ($IsLinux -or $IsMacOS))) {
    $rid     = "win-$arch"
    $libName = "mq_ffi.dll"
    $cargoTarget = if ($arch -eq "arm64") { "aarch64-pc-windows-msvc" } else { "x86_64-pc-windows-msvc" }
} elseif ($IsMacOS) {
    $rid     = "osx-$arch"
    $libName = "libmq_ffi.dylib"
    $cargoTarget = if ($arch -eq "arm64") { "aarch64-apple-darwin" } else { "x86_64-apple-darwin" }
} else {
    $rid     = "linux-$arch"
    $libName = "libmq_ffi.so"
    $cargoTarget = if ($arch -eq "arm64") { "aarch64-unknown-linux-gnu" } else { "x86_64-unknown-linux-gnu" }
}

Write-Host "Target RID : $rid"
Write-Host "Library    : $libName"

# Clone or update mq repository
if (-not (Test-Path (Join-Path $repoDir ".git"))) {
    Write-Host "Cloning harehare/mq at tag $MqTag..."
    git clone --depth=1 --branch $MqTag https://github.com/harehare/mq.git $repoDir
} else {
    Write-Host "mq repo already present at $repoDir — skipping clone."
}

# Build mq-ffi
Write-Host "Building mq-ffi (release)..."
Push-Location $repoDir
try {
    cargo build --release -p mq-ffi
} finally {
    Pop-Location
}

# Paths
$srcBinary = Join-Path $repoDir "target" "release" $libName
if (-not (Test-Path $srcBinary)) {
    throw "Build succeeded but binary not found at: $srcBinary"
}

# Copy to src/MQNet/native/ for local dev / testing
New-Item -ItemType Directory -Force -Path $localNativeDir | Out-Null
Copy-Item -Path $srcBinary -Destination (Join-Path $localNativeDir $libName) -Force
Write-Host "Copied to: $localNativeDir\$libName"

# Copy to runtime package directory for NuGet packaging
$runtimeNativeDir = [System.IO.Path]::GetFullPath(
    (Join-Path $rootDir "src\MQNet.Runtime\$rid\runtimes\$rid\native"))
New-Item -ItemType Directory -Force -Path $runtimeNativeDir | Out-Null
Copy-Item -Path $srcBinary -Destination (Join-Path $runtimeNativeDir $libName) -Force
Write-Host "Copied to: $runtimeNativeDir\$libName"

Write-Host ""
Write-Host "Done. Run 'dotnet test' to verify."
