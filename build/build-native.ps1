<#
.SYNOPSIS
    Clones harehare/mq at a pinned tag, builds the mq-ffi native library,
    and copies the binary for local development and testing.

.DESCRIPTION
    On Windows ARM64, this script builds a win-x64 DLL using the x86_64-pc-windows-msvc
    Rust toolchain (runs via x64 emulation). Native win-arm64 binaries are produced by
    GitHub Actions (windows-latest runners have full ARM64 MSVC tooling).

.PARAMETER MqTag
    The mq release tag to build (default: v0.5.31)
#>
param(
    [string]$MqTag = "v0.5.36"
)

$ErrorActionPreference = "Stop"

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir    = Join-Path $scriptDir ".."
$repoDir    = [System.IO.Path]::GetFullPath((Join-Path $rootDir ".mq"))
$localNativeDir = [System.IO.Path]::GetFullPath((Join-Path $rootDir "native"))

if ($IsWindows -or (-not ($IsLinux -or $IsMacOS))) {
    # On Windows ARM64: build x64 DLL (runs via emulation). ARM64 DLL is built in CI.
    $rid         = "win-x64"
    $libName     = "mq_ffi.dll"
    $cargoTarget = "x86_64-pc-windows-msvc"
    $rustToolchain = "+1.95.0-x86_64-pc-windows-msvc"
} elseif ($IsMacOS) {
    $arch = if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture `
                -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "arm64" } else { "x64" }
    $rid         = "osx-$arch"
    $libName     = "libmq_ffi.dylib"
    $cargoTarget = if ($arch -eq "arm64") { "aarch64-apple-darwin" } else { "x86_64-apple-darwin" }
    $rustToolchain = ""
} else {
    $arch = if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture `
                -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "arm64" } else { "x64" }
    $rid         = "linux-$arch"
    $libName     = "libmq_ffi.so"
    $cargoTarget = if ($arch -eq "arm64") { "aarch64-unknown-linux-gnu" } else { "x86_64-unknown-linux-gnu" }
    $rustToolchain = ""
}

Write-Host "Target RID : $rid"
Write-Host "Cargo target: $cargoTarget"

# Clone or update mq repository
if (-not (Test-Path (Join-Path $repoDir ".git"))) {
    Write-Host "Cloning harehare/mq at tag $MqTag..."
    git clone --depth=1 --branch $MqTag https://github.com/harehare/mq.git $repoDir
} else {
    Write-Host "mq repo already present at $repoDir — skipping clone."
}

# Build mq-ffi
Write-Host "Building mq-ffi (release) for $cargoTarget ..."
Push-Location $repoDir
try {
    $cargoArgs = @("build", "--release", "-p", "mq-ffi", "--target", $cargoTarget)
    if ($rustToolchain) {
        & cargo $rustToolchain @cargoArgs
    } else {
        & cargo @cargoArgs
    }
    if ($LASTEXITCODE -ne 0) { throw "cargo build failed with exit code $LASTEXITCODE" }
} finally {
    Pop-Location
}

$srcBinary = Join-Path $repoDir "target" $cargoTarget "release" $libName
if (-not (Test-Path $srcBinary)) {
    throw "Build succeeded but binary not found at: $srcBinary"
}

# Copy to native/<rid>/ for local dev / testing
$destDir = Join-Path $localNativeDir $rid
New-Item -ItemType Directory -Force -Path $destDir | Out-Null
Copy-Item -Path $srcBinary -Destination (Join-Path $destDir $libName) -Force
Write-Host "Copied to: $destDir\$libName"

Write-Host ""
Write-Host "Done. Run 'dotnet test' to verify."
