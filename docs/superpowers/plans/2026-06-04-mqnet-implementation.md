# MQNet Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET library (MQNet) that wraps the mq-ffi native library to expose mq's Markdown query engine to .NET applications, distributed as NuGet packages.

**Architecture:** P/Invoke into the mq-ffi C shared library (built from Rust source). The managed layer is split into a thin FFI wrapper (`MqEngine`) and a fluent builder (`Mq.Query()`). Native binaries are distributed via six `MQNet.Runtime.<rid>` NuGet packages; consumers reference only `MQNet`.

**Tech Stack:** .NET 10 SDK, multi-target net8.0+net10.0, xUnit 2.x, Rust/Cargo (for building mq-ffi), GitHub Actions CI, NuGet.

---

## File Map

| File | Responsibility |
|---|---|
| `MQNet.slnx` | Solution file (slnx format) |
| `Directory.Build.props` | Shared version, authors, nullable, LangVersion |
| `.gitignore` | Exclude `.mq/`, `native/`, build artifacts |
| `src/MQNet/MQNet.csproj` | Main library, net8.0+net10.0, copies local native binary |
| `src/MQNet/Native/NativeMethods.cs` | All P/Invoke declarations + native structs |
| `src/MQNet/InputFormat.cs` | `InputFormat` enum + `ToNativeString()` extension |
| `src/MQNet/ConversionOptions.cs` | `ConversionOptions` POCO |
| `src/MQNet/MqException.cs` | `MqException : Exception` |
| `src/MQNet/MqResult.cs` | `MqResult` (immutable, wraps `IReadOnlyList<string>`) |
| `src/MQNet/MqEngine.cs` | `MqEngine : IDisposable` — core FFI wrapper |
| `src/MQNet/Mq.cs` | `Mq` static entry point + `MqQueryBuilder` |
| `src/MQNet.Runtime/win-x64/MQNet.Runtime.win-x64.csproj` | Packs `runtimes/win-x64/native/mq_ffi.dll` |
| `src/MQNet.Runtime/win-arm64/MQNet.Runtime.win-arm64.csproj` | Packs `runtimes/win-arm64/native/mq_ffi.dll` |
| `src/MQNet.Runtime/linux-x64/MQNet.Runtime.linux-x64.csproj` | Packs `runtimes/linux-x64/native/libmq_ffi.so` |
| `src/MQNet.Runtime/linux-arm64/MQNet.Runtime.linux-arm64.csproj` | Packs `runtimes/linux-arm64/native/libmq_ffi.so` |
| `src/MQNet.Runtime/osx-x64/MQNet.Runtime.osx-x64.csproj` | Packs `runtimes/osx-x64/native/libmq_ffi.dylib` |
| `src/MQNet.Runtime/osx-arm64/MQNet.Runtime.osx-arm64.csproj` | Packs `runtimes/osx-arm64/native/libmq_ffi.dylib` |
| `src/MQNet/MQNet.nuspec` | NuGet spec listing all 6 runtime packages as dependencies |
| `tests/MQNet.Tests/MQNet.Tests.csproj` | xUnit test project, net8.0+net10.0 |
| `tests/MQNet.Tests/MqEngineTests.cs` | Engine lifecycle + eval + HTML conversion tests |
| `tests/MQNet.Tests/MqResultTests.cs` | MqResult API tests (no native needed) |
| `tests/MQNet.Tests/MqQueryBuilderTests.cs` | Fluent builder tests |
| `build/build-native.ps1` | Clones mq, builds mq-ffi, copies binary to local native/ |
| `.github/workflows/ci.yml` | 6-RID matrix build + test + pack + publish |

---

## Task 1: Repository Scaffolding

**Files:**
- Create: `.gitignore`
- Create: `Directory.Build.props`
- Create: `MQNet.slnx`
- Create: `src/MQNet/` directory structure
- Create: `tests/MQNet.Tests/` directory
- Create: `build/` directory

- [ ] **Step 1: Create directory structure**

```powershell
New-Item -ItemType Directory -Force src\MQNet\Native
New-Item -ItemType Directory -Force src\MQNet.Runtime\win-x64
New-Item -ItemType Directory -Force src\MQNet.Runtime\win-arm64
New-Item -ItemType Directory -Force src\MQNet.Runtime\linux-x64
New-Item -ItemType Directory -Force src\MQNet.Runtime\linux-arm64
New-Item -ItemType Directory -Force src\MQNet.Runtime\osx-x64
New-Item -ItemType Directory -Force src\MQNet.Runtime\osx-arm64
New-Item -ItemType Directory -Force tests\MQNet.Tests
New-Item -ItemType Directory -Force build
New-Item -ItemType Directory -Force .github\workflows
```

- [ ] **Step 2: Create `.gitignore`**

```gitignore
## Build output
bin/
obj/

## Native library sources and built binaries
.mq/
src/MQNet/native/

## NuGet packages
*.nupkg
*.snupkg

## User-specific files
.vs/
*.user
```

- [ ] **Step 3: Create `Directory.Build.props`**

```xml
<Project>
  <PropertyGroup>
    <MqVersion>0.5.31</MqVersion>
    <Version>$(MqVersion)</Version>
    <Authors>MQNet Contributors</Authors>
    <PackageProjectUrl>https://github.com/OWNER/MQNet</PackageProjectUrl>
    <RepositoryUrl>https://github.com/OWNER/MQNet</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryType>git</RepositoryType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

- [ ] **Step 4: Create `MQNet.slnx`**

```xml
<Solution>
  <Project Path="src\MQNet\MQNet.csproj" />
  <Project Path="src\MQNet.Runtime\win-x64\MQNet.Runtime.win-x64.csproj" />
  <Project Path="src\MQNet.Runtime\win-arm64\MQNet.Runtime.win-arm64.csproj" />
  <Project Path="src\MQNet.Runtime\linux-x64\MQNet.Runtime.linux-x64.csproj" />
  <Project Path="src\MQNet.Runtime\linux-arm64\MQNet.Runtime.linux-arm64.csproj" />
  <Project Path="src\MQNet.Runtime\osx-x64\MQNet.Runtime.osx-x64.csproj" />
  <Project Path="src\MQNet.Runtime\osx-arm64\MQNet.Runtime.osx-arm64.csproj" />
  <Project Path="tests\MQNet.Tests\MQNet.Tests.csproj" />
</Solution>
```

- [ ] **Step 5: Create `src/MQNet/MQNet.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
    <PackageId>MQNet</PackageId>
    <Description>A .NET wrapper for mq — a jq-like tool for querying and transforming Markdown.</Description>
    <NuspecFile>MQNet.nuspec</NuspecFile>
    <NuspecProperties>version=$(Version);authors=$(Authors)</NuspecProperties>
  </PropertyGroup>

  <!-- Local development: copy native binary from build-native.ps1 output to test output dir -->
  <ItemGroup>
    <Content Include="native\mq_ffi.dll" Condition="Exists('native\mq_ffi.dll')">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <TargetPath>mq_ffi.dll</TargetPath>
    </Content>
    <Content Include="native\libmq_ffi.so" Condition="Exists('native\libmq_ffi.so')">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <TargetPath>libmq_ffi.so</TargetPath>
    </Content>
    <Content Include="native\libmq_ffi.dylib" Condition="Exists('native\libmq_ffi.dylib')">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <TargetPath>libmq_ffi.dylib</TargetPath>
    </Content>
  </ItemGroup>
</Project>
```

- [ ] **Step 6: Create `src/MQNet/MQNet.nuspec`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<package>
  <metadata>
    <id>MQNet</id>
    <version>$version$</version>
    <authors>$authors$</authors>
    <description>A .NET wrapper for mq — a jq-like tool for querying and transforming Markdown.</description>
    <projectUrl>https://github.com/OWNER/MQNet</projectUrl>
    <license type="expression">MIT</license>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <tags>markdown mq query transform jq</tags>
    <dependencies>
      <group targetFramework="net8.0">
        <dependency id="MQNet.Runtime.win-x64"    version="[$version$,)" />
        <dependency id="MQNet.Runtime.win-arm64"   version="[$version$,)" />
        <dependency id="MQNet.Runtime.linux-x64"   version="[$version$,)" />
        <dependency id="MQNet.Runtime.linux-arm64" version="[$version$,)" />
        <dependency id="MQNet.Runtime.osx-x64"     version="[$version$,)" />
        <dependency id="MQNet.Runtime.osx-arm64"   version="[$version$,)" />
      </group>
      <group targetFramework="net10.0">
        <dependency id="MQNet.Runtime.win-x64"    version="[$version$,)" />
        <dependency id="MQNet.Runtime.win-arm64"   version="[$version$,)" />
        <dependency id="MQNet.Runtime.linux-x64"   version="[$version$,)" />
        <dependency id="MQNet.Runtime.linux-arm64" version="[$version$,)" />
        <dependency id="MQNet.Runtime.osx-x64"     version="[$version$,)" />
        <dependency id="MQNet.Runtime.osx-arm64"   version="[$version$,)" />
      </group>
    </dependencies>
  </metadata>
  <files>
    <file src="bin\Release\net8.0\MQNet.dll"     target="lib\net8.0\MQNet.dll" />
    <file src="bin\Release\net10.0\MQNet.dll"    target="lib\net10.0\MQNet.dll" />
    <file src="bin\Release\net8.0\MQNet.xml"     target="lib\net8.0\MQNet.xml" />
    <file src="bin\Release\net10.0\MQNet.xml"    target="lib\net10.0\MQNet.xml" />
  </files>
</package>
```

- [ ] **Step 7: Create `tests/MQNet.Tests/MQNet.Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.3">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\MQNet\MQNet.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 8: Verify solution loads**

```powershell
dotnet sln MQNet.slnx list
```

Expected output: lists all 8 projects without errors.

- [ ] **Step 9: Commit**

```powershell
git init
git add .
git commit -m "chore: initial solution scaffolding"
```

---

## Task 2: Build Script

**Files:**
- Create: `build/build-native.ps1`

- [ ] **Step 1: Create `build/build-native.ps1`**

```powershell
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
```

- [ ] **Step 2: Run the build script**

```powershell
.\build\build-native.ps1
```

Expected output:
```
Target RID : win-x64
Library    : mq_ffi.dll
Cloning harehare/mq at tag v0.5.31...
Building mq-ffi (release)...
Copied to: ...\src\MQNet\native\mq_ffi.dll
Copied to: ...\src\MQNet.Runtime\win-x64\runtimes\win-x64\native\mq_ffi.dll
Done. Run 'dotnet test' to verify.
```

- [ ] **Step 3: Verify the binary exists**

```powershell
Test-Path src\MQNet\native\mq_ffi.dll   # Windows
# or
Test-Path src\MQNet\native\libmq_ffi.so  # Linux
```

Expected: `True`

- [ ] **Step 4: Commit**

```powershell
git add build\build-native.ps1 .gitignore
git commit -m "chore: add build-native.ps1 to build mq-ffi from source"
```

---

## Task 3: Core Types

**Files:**
- Create: `src/MQNet/InputFormat.cs`
- Create: `src/MQNet/ConversionOptions.cs`
- Create: `src/MQNet/MqException.cs`

These are pure type definitions — no native dependency, no TDD cycle needed.

- [ ] **Step 1: Create `src/MQNet/InputFormat.cs`**

```csharp
namespace MQNet;

/// <summary>Input format for mq query evaluation.</summary>
public enum InputFormat
{
    /// <summary>CommonMark / GFM Markdown (default).</summary>
    Markdown,
    /// <summary>Markdown with JSX (MDX).</summary>
    Mdx,
    /// <summary>HTML — auto-converted to Markdown before querying.</summary>
    Html,
    /// <summary>Plain text, split by lines.</summary>
    Text,
    /// <summary>Raw string, no parsing.</summary>
    Raw
}

internal static class InputFormatExtensions
{
    internal static string ToNativeString(this InputFormat format) => format switch
    {
        InputFormat.Markdown => "markdown",
        InputFormat.Mdx      => "mdx",
        InputFormat.Html     => "html",
        InputFormat.Text     => "text",
        InputFormat.Raw      => "raw",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };
}
```

- [ ] **Step 2: Create `src/MQNet/ConversionOptions.cs`**

```csharp
namespace MQNet;

/// <summary>Options for HTML to Markdown conversion.</summary>
public sealed class ConversionOptions
{
    /// <summary>Extract &lt;script&gt; tags as fenced code blocks.</summary>
    public bool ExtractScriptsAsCodeBlocks { get; init; }

    /// <summary>Generate YAML front matter from HTML &lt;head&gt; metadata.</summary>
    public bool GenerateFrontMatter { get; init; }

    /// <summary>Use the HTML &lt;title&gt; element as the H1 heading.</summary>
    public bool UseTitleAsH1 { get; init; }
}
```

- [ ] **Step 3: Create `src/MQNet/MqException.cs`**

```csharp
namespace MQNet;

/// <summary>Thrown when the mq native engine reports an error.</summary>
public sealed class MqException : Exception
{
    /// <inheritdoc />
    public MqException(string message) : base(message) { }

    /// <inheritdoc />
    public MqException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

- [ ] **Step 4: Build to confirm no errors**

```powershell
dotnet build src\MQNet\MQNet.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```powershell
git add src\MQNet\InputFormat.cs src\MQNet\ConversionOptions.cs src\MQNet\MqException.cs
git commit -m "feat: add InputFormat, ConversionOptions, MqException types"
```

---

## Task 4: Native Interop Layer

**Files:**
- Create: `src/MQNet/Native/NativeMethods.cs`

The exact mq-ffi C API (confirmed from source):
```c
MqContext* mq_create();
void       mq_destroy(MqContext* engine_ptr);
MqResult   mq_eval(MqContext* engine_ptr, const char* code, const char* input, const char* input_format);
void       mq_free_result(MqResult result);
void       mq_free_string(char* s);
char*      mq_html_to_markdown(const char* html, MqConversionOptions options, char** error_msg);
```

- [ ] **Step 1: Create `src/MQNet/Native/NativeMethods.cs`**

```csharp
using System.Runtime.InteropServices;

namespace MQNet.Native;

/// <summary>
/// Native struct matching mq-ffi's MqResult (repr(C)):
///   values:     *mut *mut c_char  (pointer to array of C string pointers)
///   values_len: usize             (number of strings)
///   error_msg:  *mut c_char       (null on success)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MqResultNative
{
    public IntPtr  Values;     // char**
    public UIntPtr ValuesLen;  // size_t
    public IntPtr  ErrorMsg;   // char*
}

/// <summary>
/// Native struct matching mq-ffi's MqConversionOptions (repr(C)).
/// Each bool is a single byte (Rust bool = 1 byte in C ABI).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MqConversionOptionsNative
{
    [MarshalAs(UnmanagedType.I1)] public bool ExtractScriptsAsCodeBlocks;
    [MarshalAs(UnmanagedType.I1)] public bool GenerateFrontMatter;
    [MarshalAs(UnmanagedType.I1)] public bool UseTitleAsH1;
}

internal static partial class NativeMethods
{
    // Library name — .NET resolves to mq_ffi.dll / libmq_ffi.so / libmq_ffi.dylib
    private const string LibName = "mq_ffi";

    /// <summary>Creates a new mq engine. Must be freed with mq_destroy.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_create")]
    internal static partial IntPtr MqCreate();

    /// <summary>Destroys an mq engine.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_destroy")]
    internal static partial void MqDestroy(IntPtr enginePtr);

    /// <summary>
    /// Evaluates a mq query. The returned MqResultNative MUST be freed with MqFreeResult.
    /// Strings are UTF-8 encoded.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "mq_eval",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial MqResultNative MqEval(
        IntPtr enginePtr,
        string code,
        string input,
        string inputFormat);

    /// <summary>Frees an MqResultNative returned by MqEval.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_free_result")]
    internal static partial void MqFreeResult(MqResultNative result);

    /// <summary>Frees a C string allocated by Rust.</summary>
    [LibraryImport(LibName, EntryPoint = "mq_free_string")]
    internal static partial void MqFreeString(IntPtr str);

    /// <summary>
    /// Converts HTML to Markdown. Returns null on error and sets errorMsg.
    /// The returned string and errorMsg MUST be freed with MqFreeString.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "mq_html_to_markdown",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr MqHtmlToMarkdown(
        string html,
        MqConversionOptionsNative options,
        out IntPtr errorMsg);
}
```

- [ ] **Step 2: Build to confirm no errors**

```powershell
dotnet build src\MQNet\MQNet.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src\MQNet\Native\NativeMethods.cs
git commit -m "feat: add P/Invoke declarations for mq-ffi"
```

---

## Task 5: MqResult

**Files:**
- Create: `tests/MQNet.Tests/MqResultTests.cs`
- Create: `src/MQNet/MqResult.cs`

`MqResult` is a pure managed type — no native dependency. Write and run tests before implementing.

- [ ] **Step 1: Write the failing tests in `tests/MQNet.Tests/MqResultTests.cs`**

```csharp
using MQNet;

namespace MQNet.Tests;

public class MqResultTests
{
    [Fact]
    public void Values_ReturnsAllValues()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal(["# H1", "## H2"], result.Values);
    }

    [Fact]
    public void Text_JoinsValuesWithNewline()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal("# H1\n## H2", result.Text);
    }

    [Fact]
    public void Indexer_ReturnsValueAtIndex()
    {
        var result = new MqResult(["# H1", "## H2"]);
        Assert.Equal("# H1", result[0]);
        Assert.Equal("## H2", result[1]);
    }

    [Fact]
    public void Count_ReturnsNumberOfValues()
    {
        var result = new MqResult(["a", "b", "c"]);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void EmptyResult_HasZeroCount()
    {
        var result = new MqResult([]);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void EmptyResult_TextIsEmpty()
    {
        var result = new MqResult([]);
        Assert.Equal("", result.Text);
    }

    [Fact]
    public void Enumeration_IteratesAllValues()
    {
        var result = new MqResult(["x", "y"]);
        Assert.Equal(["x", "y"], result.ToList());
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var result = new MqResult(["a"]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = result[5]);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj --filter "FullyQualifiedName~MqResultTests" -v minimal
```

Expected: Build error — `MqResult` type not found.

- [ ] **Step 3: Implement `src/MQNet/MqResult.cs`**

```csharp
using System.Collections;

namespace MQNet;

/// <summary>The result of an mq query evaluation.</summary>
public sealed class MqResult : IReadOnlyList<string>
{
    private readonly IReadOnlyList<string> _values;

    internal MqResult(IReadOnlyList<string> values)
    {
        _values = values;
    }

    /// <summary>All matched values.</summary>
    public IReadOnlyList<string> Values => _values;

    /// <summary>All values joined by a newline character.</summary>
    public string Text => _values.Count == 0 ? "" : string.Join('\n', _values);

    /// <inheritdoc />
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= _values.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _values[index];
        }
    }

    /// <inheritdoc />
    public int Count => _values.Count;

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator() => _values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

- [ ] **Step 4: Run tests to verify they pass**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj --filter "FullyQualifiedName~MqResultTests" -v minimal
```

Expected: All 8 tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src\MQNet\MqResult.cs tests\MQNet.Tests\MqResultTests.cs
git commit -m "feat: add MqResult with TDD"
```

---

## Task 6: MqEngine

**Files:**
- Create: `tests/MQNet.Tests/MqEngineTests.cs`
- Create: `src/MQNet/MqEngine.cs`

Requires native binary in `src/MQNet/native/`. Run `build/build-native.ps1` if not already done (Task 2).

- [ ] **Step 1: Write failing tests in `tests/MQNet.Tests/MqEngineTests.cs`**

```csharp
using MQNet;

namespace MQNet.Tests;

public class MqEngineTests
{
    // --- Lifecycle ---

    [Fact]
    public void Constructor_CreatesEngineWithoutError()
    {
        using var engine = new MqEngine();
        Assert.NotNull(engine);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var engine = new MqEngine();
        engine.Dispose();
        engine.Dispose(); // must not throw
    }

    [Fact]
    public void Eval_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new MqEngine();
        engine.Dispose();
        Assert.Throws<ObjectDisposedException>(() => engine.Eval(".h1", "# Hello"));
    }

    // --- Query evaluation ---

    [Fact]
    public void Eval_ExtractsH1Headings()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "# Hello\n\n## World\n\n# Another");
        Assert.Equal(["# Hello", "# Another"], result.Values);
    }

    [Fact]
    public void Eval_ExtractsH2Headings()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(2)", "# Title\n\n## Section A\n\n## Section B");
        Assert.Equal(["## Section A", "## Section B"], result.Values);
    }

    [Fact]
    public void Eval_ExtractsCodeBlocks()
    {
        using var engine = new MqEngine();
        var markdown = "# Title\n\n```csharp\nvar x = 1;\n```\n\n```rust\nlet x = 1;\n```";
        var result = engine.Eval(".code", markdown);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Eval_FiltersByCodeLanguage()
    {
        using var engine = new MqEngine();
        var markdown = "```csharp\nvar x = 1;\n```\n\n```rust\nlet x = 1;\n```";
        var result = engine.Eval(".code(\"rust\")", markdown);
        Assert.Equal(1, result.Count);
        Assert.Contains("rust", result[0]);
    }

    [Fact]
    public void Eval_PipeQuery_SelectContains()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h | select(contains(\"Foo\"))", "# Foo\n\n## Bar\n\n## FooBar");
        Assert.Equal(2, result.Count);
        Assert.All(result.Values, v => Assert.Contains("Foo", v));
    }

    [Fact]
    public void Eval_EmptyInput_ReturnsEmptyResult()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h1", "");
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void Eval_InvalidQuery_ThrowsMqException()
    {
        using var engine = new MqEngine();
        var ex = Assert.Throws<MqException>(() => engine.Eval(".!!invalid!!", "# Hello"));
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Eval_DefaultFormat_IsMarkdown()
    {
        using var engine = new MqEngine();
        // HTML input with default (Markdown) format — headings should NOT be extracted
        // because the <h1> tag is treated as raw text
        var result = engine.Eval(".h(1)", "<h1>Hello</h1>");
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void Eval_HtmlFormat_ConvertsBeforeQuerying()
    {
        using var engine = new MqEngine();
        var result = engine.Eval(".h(1)", "<h1>Hello</h1>", InputFormat.Html);
        Assert.Equal(1, result.Count);
        Assert.Contains("Hello", result[0]);
    }

    [Fact]
    public void Eval_TextFormat_SplitsByLines()
    {
        using var engine = new MqEngine();
        var result = engine.Eval("select(contains(\"B\"))", "Line A\nLine B\nLine C",
            InputFormat.Text);
        Assert.Equal(1, result.Count);
        Assert.Equal("Line B", result[0]);
    }

    // --- HTML conversion ---

    [Fact]
    public void HtmlToMarkdown_BasicConversion()
    {
        var markdown = MqEngine.HtmlToMarkdown("<h1>Hello</h1><p>World</p>");
        Assert.Contains("# Hello", markdown);
        Assert.Contains("World", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_WithNullOptions_UsesDefaults()
    {
        var markdown = MqEngine.HtmlToMarkdown("<p>Simple</p>");
        Assert.Contains("Simple", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_UseTitleAsH1_IncludesTitle()
    {
        var html = "<html><head><title>My Title</title></head><body><p>Body</p></body></html>";
        var markdown = MqEngine.HtmlToMarkdown(html, new ConversionOptions { UseTitleAsH1 = true });
        Assert.Contains("# My Title", markdown);
    }

    [Fact]
    public void HtmlToMarkdown_EmptyHtml_ReturnsEmptyOrWhitespace()
    {
        var markdown = MqEngine.HtmlToMarkdown("");
        Assert.NotNull(markdown);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj --filter "FullyQualifiedName~MqEngineTests" -v minimal
```

Expected: Build error — `MqEngine` type not found.

- [ ] **Step 3: Implement `src/MQNet/MqEngine.cs`**

```csharp
using System.Runtime.InteropServices;
using MQNet.Native;

namespace MQNet;

/// <summary>
/// Wraps the mq native engine. Create once, call Eval multiple times, then Dispose.
/// For single-query convenience, use <see cref="Mq.Query"/> instead.
/// </summary>
public sealed class MqEngine : IDisposable
{
    private IntPtr _enginePtr;
    private bool _disposed;

    /// <summary>Creates a new mq engine instance.</summary>
    /// <exception cref="InvalidOperationException">If the native engine could not be created.</exception>
    public MqEngine()
    {
        _enginePtr = NativeMethods.MqCreate();
        if (_enginePtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create mq native engine.");
    }

    /// <summary>
    /// Evaluates a mq query against Markdown input.
    /// </summary>
    /// <param name="query">The mq query string (e.g. ".h1", ".code(\"rust\")").</param>
    /// <param name="input">The input content to query.</param>
    /// <param name="format">Input format (default: Markdown).</param>
    /// <returns>The query result.</returns>
    /// <exception cref="ObjectDisposedException">If this engine has been disposed.</exception>
    /// <exception cref="MqException">If the query fails.</exception>
    public MqResult Eval(string query, string input, InputFormat format = InputFormat.Markdown)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(input);

        var nativeResult = NativeMethods.MqEval(_enginePtr, query, input, format.ToNativeString());
        try
        {
            if (nativeResult.ErrorMsg != IntPtr.Zero)
            {
                var errorMsg = Marshal.PtrToStringUTF8(nativeResult.ErrorMsg) ?? "Unknown error";
                throw new MqException(errorMsg);
            }

            return MarshalResult(nativeResult);
        }
        finally
        {
            NativeMethods.MqFreeResult(nativeResult);
        }
    }

    /// <summary>
    /// Converts HTML to Markdown.
    /// </summary>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>The Markdown representation of the HTML.</returns>
    /// <exception cref="MqException">If conversion fails.</exception>
    public static string HtmlToMarkdown(string html, ConversionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(html);

        var nativeOptions = options is null
            ? default
            : new MqConversionOptionsNative
            {
                ExtractScriptsAsCodeBlocks = options.ExtractScriptsAsCodeBlocks,
                GenerateFrontMatter        = options.GenerateFrontMatter,
                UseTitleAsH1               = options.UseTitleAsH1
            };

        var resultPtr = NativeMethods.MqHtmlToMarkdown(html, nativeOptions, out var errorMsgPtr);

        if (resultPtr == IntPtr.Zero)
        {
            var errorMsg = errorMsgPtr != IntPtr.Zero
                ? Marshal.PtrToStringUTF8(errorMsgPtr) ?? "HTML conversion failed"
                : "HTML conversion failed";
            if (errorMsgPtr != IntPtr.Zero)
                NativeMethods.MqFreeString(errorMsgPtr);
            throw new MqException(errorMsg);
        }

        var markdown = Marshal.PtrToStringUTF8(resultPtr) ?? "";
        NativeMethods.MqFreeString(resultPtr);
        return markdown;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_enginePtr != IntPtr.Zero)
        {
            NativeMethods.MqDestroy(_enginePtr);
            _enginePtr = IntPtr.Zero;
        }
    }

    private static MqResult MarshalResult(MqResultNative native)
    {
        var count = (int)native.ValuesLen;
        if (count == 0 || native.Values == IntPtr.Zero)
            return new MqResult([]);

        var values = new string[count];
        for (int i = 0; i < count; i++)
        {
            var ptrToStr = Marshal.ReadIntPtr(native.Values, i * IntPtr.Size);
            values[i] = Marshal.PtrToStringUTF8(ptrToStr) ?? "";
        }
        return new MqResult(values);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj --filter "FullyQualifiedName~MqEngineTests" -v minimal
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src\MQNet\MqEngine.cs tests\MQNet.Tests\MqEngineTests.cs
git commit -m "feat: add MqEngine P/Invoke wrapper with TDD"
```

---

## Task 7: Fluent API

**Files:**
- Create: `tests/MQNet.Tests/MqQueryBuilderTests.cs`
- Create: `src/MQNet/Mq.cs`

- [ ] **Step 1: Write failing tests in `tests/MQNet.Tests/MqQueryBuilderTests.cs`**

```csharp
using MQNet;

namespace MQNet.Tests;

public class MqQueryBuilderTests
{
    [Fact]
    public void Query_On_Run_ExtractsH1()
    {
        var result = Mq.Query(".h(1)").On("# Hello\n\n## World").Run();
        Assert.Equal(1, result.Count);
        Assert.Equal("# Hello", result[0]);
    }

    [Fact]
    public void Query_WithDefaultFormat_IsMarkdown()
    {
        // Without calling WithFormat, default should be Markdown
        var result = Mq.Query(".h(1)").On("# Title\n\n## Sub").Run();
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Query_WithHtmlFormat_ConvertsFirst()
    {
        var result = Mq.Query(".h(1)")
            .On("<h1>Title</h1>")
            .WithFormat(InputFormat.Html)
            .Run();
        Assert.Equal(1, result.Count);
        Assert.Contains("Title", result[0]);
    }

    [Fact]
    public void Query_WithTextFormat_QueriesLines()
    {
        var result = Mq.Query("select(contains(\"match\"))")
            .On("no\nmatch this\nno")
            .WithFormat(InputFormat.Text)
            .Run();
        Assert.Equal(1, result.Count);
        Assert.Equal("match this", result[0]);
    }

    [Fact]
    public void Query_ChainedCalls_ReturnSameBuilder()
    {
        var builder = Mq.Query(".h(1)");
        var withContent = builder.On("# Hello");
        var withFormat = withContent.WithFormat(InputFormat.Markdown);
        // All return the same builder instance (fluent)
        Assert.Same(builder, withContent);
        Assert.Same(builder, withFormat);
    }

    [Fact]
    public void Query_Run_DisposesEngineAfterCall()
    {
        // Multiple sequential runs should work (each creates/disposes its own engine)
        var r1 = Mq.Query(".h(1)").On("# A").Run();
        var r2 = Mq.Query(".h(1)").On("# B").Run();
        Assert.Equal("# A", r1[0]);
        Assert.Equal("# B", r2[0]);
    }

    [Fact]
    public void Query_Run_WithoutOn_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Mq.Query(".h(1)").Run());
    }

    [Fact]
    public void Query_InvalidQuery_ThrowsMqException()
    {
        Assert.Throws<MqException>(() => Mq.Query(".!!bad!!").On("# Hello").Run());
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj --filter "FullyQualifiedName~MqQueryBuilderTests" -v minimal
```

Expected: Build error — `Mq` type not found.

- [ ] **Step 3: Implement `src/MQNet/Mq.cs`**

```csharp
namespace MQNet;

/// <summary>Entry point for the fluent mq query API.</summary>
public static class Mq
{
    /// <summary>Starts a fluent query chain.</summary>
    /// <param name="query">The mq query string (e.g. ".h1", ".code(\"rust\")").</param>
    public static MqQueryBuilder Query(string query) => new(query);
}

/// <summary>
/// Fluent builder for mq queries. Obtain via <see cref="Mq.Query"/>.
/// For bulk queries over the same content, use <see cref="MqEngine"/> directly.
/// </summary>
public sealed class MqQueryBuilder
{
    private readonly string _query;
    private string? _input;
    private InputFormat _format = InputFormat.Markdown;

    internal MqQueryBuilder(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _query = query;
    }

    /// <summary>Sets the input content to query.</summary>
    public MqQueryBuilder On(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _input = input;
        return this;
    }

    /// <summary>Sets the input format (default: <see cref="InputFormat.Markdown"/>).</summary>
    public MqQueryBuilder WithFormat(InputFormat format)
    {
        _format = format;
        return this;
    }

    /// <summary>Executes the query and returns the result.</summary>
    /// <exception cref="InvalidOperationException">If <see cref="On"/> was not called.</exception>
    /// <exception cref="MqException">If the query fails.</exception>
    public MqResult Run()
    {
        if (_input is null)
            throw new InvalidOperationException("Call On(input) before Run().");

        using var engine = new MqEngine();
        return engine.Eval(_query, _input, _format);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj --filter "FullyQualifiedName~MqQueryBuilderTests" -v minimal
```

Expected: All 8 tests pass.

- [ ] **Step 5: Run the full test suite**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj -v minimal
```

Expected: All tests pass across both `net8.0` and `net10.0` target frameworks.

- [ ] **Step 6: Commit**

```powershell
git add src\MQNet\Mq.cs tests\MQNet.Tests\MqQueryBuilderTests.cs
git commit -m "feat: add fluent API (Mq.Query / MqQueryBuilder) with TDD"
```

---

## Task 8: Runtime Packages

**Files:**
- Create: `src/MQNet.Runtime/win-x64/MQNet.Runtime.win-x64.csproj` (and 5 similar)

Each runtime project follows the same pattern — only the RID and library filename differ.

- [ ] **Step 1: Create `src/MQNet.Runtime/win-x64/MQNet.Runtime.win-x64.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MQNet.Runtime.win-x64</PackageId>
    <Description>Native runtime assets for MQNet on Windows x64.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="runtimes\win-x64\native\mq_ffi.dll"
          Pack="true"
          PackagePath="runtimes\win-x64\native\"
          Condition="Exists('runtimes\win-x64\native\mq_ffi.dll')" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create `src/MQNet.Runtime/win-arm64/MQNet.Runtime.win-arm64.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MQNet.Runtime.win-arm64</PackageId>
    <Description>Native runtime assets for MQNet on Windows arm64.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="runtimes\win-arm64\native\mq_ffi.dll"
          Pack="true"
          PackagePath="runtimes\win-arm64\native\"
          Condition="Exists('runtimes\win-arm64\native\mq_ffi.dll')" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Create `src/MQNet.Runtime/linux-x64/MQNet.Runtime.linux-x64.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MQNet.Runtime.linux-x64</PackageId>
    <Description>Native runtime assets for MQNet on Linux x64.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="runtimes/linux-x64/native/libmq_ffi.so"
          Pack="true"
          PackagePath="runtimes/linux-x64/native/"
          Condition="Exists('runtimes/linux-x64/native/libmq_ffi.so')" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Create `src/MQNet.Runtime/linux-arm64/MQNet.Runtime.linux-arm64.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MQNet.Runtime.linux-arm64</PackageId>
    <Description>Native runtime assets for MQNet on Linux arm64.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="runtimes/linux-arm64/native/libmq_ffi.so"
          Pack="true"
          PackagePath="runtimes/linux-arm64/native/"
          Condition="Exists('runtimes/linux-arm64/native/libmq_ffi.so')" />
  </ItemGroup>
</Project>
```

- [ ] **Step 5: Create `src/MQNet.Runtime/osx-x64/MQNet.Runtime.osx-x64.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MQNet.Runtime.osx-x64</PackageId>
    <Description>Native runtime assets for MQNet on macOS x64.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="runtimes/osx-x64/native/libmq_ffi.dylib"
          Pack="true"
          PackagePath="runtimes/osx-x64/native/"
          Condition="Exists('runtimes/osx-x64/native/libmq_ffi.dylib')" />
  </ItemGroup>
</Project>
```

- [ ] **Step 6: Create `src/MQNet.Runtime/osx-arm64/MQNet.Runtime.osx-arm64.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MQNet.Runtime.osx-arm64</PackageId>
    <Description>Native runtime assets for MQNet on macOS arm64.</Description>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <None Include="runtimes/osx-arm64/native/libmq_ffi.dylib"
          Pack="true"
          PackagePath="runtimes/osx-arm64/native/"
          Condition="Exists('runtimes/osx-arm64/native/libmq_ffi.dylib')" />
  </ItemGroup>
</Project>
```

- [ ] **Step 7: Build all runtime projects**

```powershell
dotnet build src\MQNet.Runtime\win-x64\MQNet.Runtime.win-x64.csproj
dotnet build src\MQNet.Runtime\win-arm64\MQNet.Runtime.win-arm64.csproj
dotnet build src\MQNet.Runtime\linux-x64\MQNet.Runtime.linux-x64.csproj
dotnet build src\MQNet.Runtime\linux-arm64\MQNet.Runtime.linux-arm64.csproj
dotnet build src\MQNet.Runtime\osx-x64\MQNet.Runtime.osx-x64.csproj
dotnet build src\MQNet.Runtime\osx-arm64\MQNet.Runtime.osx-arm64.csproj
```

Expected: All 6 build with 0 errors (warnings about missing binaries are acceptable).

- [ ] **Step 8: Pack the host RID runtime package to verify structure**

On Windows x64:
```powershell
dotnet pack src\MQNet.Runtime\win-x64\MQNet.Runtime.win-x64.csproj -c Release -o ./artifacts
```

Inspect the package:
```powershell
# Rename to .zip and inspect, or use nuget viewer
Rename-Item artifacts\MQNet.Runtime.win-x64.*.nupkg temp.zip
Expand-Archive temp.zip -DestinationPath artifacts\temp_unpack -Force
Get-ChildItem artifacts\temp_unpack -Recurse
```

Expected: `runtimes/win-x64/native/mq_ffi.dll` present in the package.

- [ ] **Step 9: Commit**

```powershell
git add src\MQNet.Runtime\
git commit -m "feat: add runtime package projects for all 6 RIDs"
```

---

## Task 9: GitHub Actions CI

**Files:**
- Create: `.github/workflows/ci.yml`

- [ ] **Step 1: Create `.github/workflows/ci.yml`**

```yaml
name: CI

on:
  push:
    branches: [main]
    tags: ['v*.*.*']
  pull_request:
    branches: [main]

env:
  MQ_TAG: v0.5.31
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_NOLOGO: true

jobs:
  # ── Build native mq-ffi for each platform ────────────────────────────────
  native:
    name: Native (${{ matrix.rid }})
    runs-on: ${{ matrix.os }}
    strategy:
      fail-fast: false
      matrix:
        include:
          - os: windows-latest
            rid: win-x64
            cargo-target: x86_64-pc-windows-msvc
            lib: mq_ffi.dll
            cross: false
          - os: windows-latest
            rid: win-arm64
            cargo-target: aarch64-pc-windows-msvc
            lib: mq_ffi.dll
            cross: true
          - os: ubuntu-latest
            rid: linux-x64
            cargo-target: x86_64-unknown-linux-gnu
            lib: libmq_ffi.so
            cross: false
          - os: ubuntu-latest
            rid: linux-arm64
            cargo-target: aarch64-unknown-linux-gnu
            lib: libmq_ffi.so
            cross: true
          - os: macos-latest
            rid: osx-x64
            cargo-target: x86_64-apple-darwin
            lib: libmq_ffi.dylib
            cross: false
          - os: macos-latest
            rid: osx-arm64
            cargo-target: aarch64-apple-darwin
            lib: libmq_ffi.dylib
            cross: false

    steps:
      - uses: actions/checkout@v4

      - name: Install Rust toolchain
        uses: dtolnay/rust-toolchain@stable
        with:
          targets: ${{ matrix.cargo-target }}

      - name: Install cross (Linux arm64 only)
        if: matrix.cross == true && matrix.os == 'ubuntu-latest'
        run: cargo install cross --git https://github.com/cross-rs/cross

      - name: Clone mq at pinned tag
        run: git clone --depth=1 --branch ${{ env.MQ_TAG }} https://github.com/harehare/mq.git .mq

      - name: Build mq-ffi (native)
        if: matrix.cross == false
        run: cargo build --release -p mq-ffi --target ${{ matrix.cargo-target }}
        working-directory: .mq

      - name: Build mq-ffi (cross-compile)
        if: matrix.cross == true
        run: cross build --release -p mq-ffi --target ${{ matrix.cargo-target }}
        working-directory: .mq

      - name: Upload native binary
        uses: actions/upload-artifact@v4
        with:
          name: native-${{ matrix.rid }}
          path: .mq/target/${{ matrix.cargo-target }}/release/${{ matrix.lib }}

  # ── Build managed code, run tests, pack ───────────────────────────────────
  build:
    name: Build & Test (${{ matrix.os }})
    needs: native
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [windows-latest, ubuntu-latest]

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Download all native binaries
        uses: actions/download-artifact@v4
        with:
          path: native-artifacts

      - name: Place native binaries for local dev testing
        shell: pwsh
        run: |
          $rids = @('win-x64','win-arm64','linux-x64','linux-arm64','osx-x64','osx-arm64')
          $libNames = @{
            'win-x64'='mq_ffi.dll'; 'win-arm64'='mq_ffi.dll';
            'linux-x64'='libmq_ffi.so'; 'linux-arm64'='libmq_ffi.so';
            'osx-x64'='libmq_ffi.dylib'; 'osx-arm64'='libmq_ffi.dylib'
          }
          foreach ($rid in $rids) {
            $lib = $libNames[$rid]
            $src = "native-artifacts/native-$rid/$lib"
            # Place in MQNet/native/ for test runs
            $nativeDest = "src/MQNet/native"
            New-Item -ItemType Directory -Force -Path $nativeDest | Out-Null
            if (Test-Path $src) { Copy-Item $src "$nativeDest/$lib" -Force }
            # Place in runtime package runtimes/ for packing
            $rtDest = "src/MQNet.Runtime/$rid/runtimes/$rid/native"
            New-Item -ItemType Directory -Force -Path $rtDest | Out-Null
            if (Test-Path $src) { Copy-Item $src "$rtDest/$lib" -Force }
          }

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Test
        run: dotnet test tests/MQNet.Tests/MQNet.Tests.csproj --no-build -c Release -v normal

  # ── Pack and publish ───────────────────────────────────────────────────────
  pack:
    name: Pack & Publish
    needs: build
    runs-on: windows-latest
    if: startsWith(github.ref, 'refs/tags/v')

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Download all native binaries
        uses: actions/download-artifact@v4
        with:
          path: native-artifacts

      - name: Place native binaries in runtime packages
        shell: pwsh
        run: |
          $rids = @('win-x64','win-arm64','linux-x64','linux-arm64','osx-x64','osx-arm64')
          $libNames = @{
            'win-x64'='mq_ffi.dll'; 'win-arm64'='mq_ffi.dll';
            'linux-x64'='libmq_ffi.so'; 'linux-arm64'='libmq_ffi.so';
            'osx-x64'='libmq_ffi.dylib'; 'osx-arm64'='libmq_ffi.dylib'
          }
          foreach ($rid in $rids) {
            $lib = $libNames[$rid]
            $src = "native-artifacts/native-$rid/$lib"
            $dest = "src/MQNet.Runtime/$rid/runtimes/$rid/native"
            New-Item -ItemType Directory -Force -Path $dest | Out-Null
            Copy-Item $src "$dest/$lib" -Force
          }

      - name: Pack runtime packages
        run: |
          dotnet pack src/MQNet.Runtime/win-x64/MQNet.Runtime.win-x64.csproj     -c Release -o artifacts
          dotnet pack src/MQNet.Runtime/win-arm64/MQNet.Runtime.win-arm64.csproj  -c Release -o artifacts
          dotnet pack src/MQNet.Runtime/linux-x64/MQNet.Runtime.linux-x64.csproj  -c Release -o artifacts
          dotnet pack src/MQNet.Runtime/linux-arm64/MQNet.Runtime.linux-arm64.csproj -c Release -o artifacts
          dotnet pack src/MQNet.Runtime/osx-x64/MQNet.Runtime.osx-x64.csproj     -c Release -o artifacts
          dotnet pack src/MQNet.Runtime/osx-arm64/MQNet.Runtime.osx-arm64.csproj  -c Release -o artifacts

      - name: Build and pack main package
        run: |
          dotnet build src/MQNet/MQNet.csproj -c Release
          dotnet pack src/MQNet/MQNet.csproj -c Release -o artifacts

      - name: List packages
        run: Get-ChildItem artifacts\*.nupkg

      - name: Publish to NuGet.org
        run: |
          dotnet nuget push artifacts\*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

- [ ] **Step 2: Commit**

```powershell
git add .github\workflows\ci.yml
git commit -m "ci: add GitHub Actions workflow for native build, test, pack, publish"
```

---

## Task 10: Final Verification

- [ ] **Step 1: Run full test suite locally**

```powershell
dotnet test tests\MQNet.Tests\MQNet.Tests.csproj -v normal
```

Expected: All tests pass on both `net8.0` and `net10.0`.

- [ ] **Step 2: Try a local pack of the main library**

```powershell
dotnet build src\MQNet\MQNet.csproj -c Release
dotnet pack src\MQNet\MQNet.csproj -c Release -o artifacts
```

Expected: `artifacts/MQNet.0.5.31.nupkg` created.

- [ ] **Step 3: Try a local pack of the host RID runtime package**

```powershell
dotnet pack src\MQNet.Runtime\win-x64\MQNet.Runtime.win-x64.csproj -c Release -o artifacts
```

Expected: `artifacts/MQNet.Runtime.win-x64.0.5.31.nupkg` created with `runtimes/win-x64/native/mq_ffi.dll` inside.

- [ ] **Step 4: Final commit**

```powershell
git add .
git commit -m "chore: final verification - all tests passing, packages packing correctly"
```

---

## Summary

| Task | What it builds | TDD? |
|---|---|---|
| 1 | Solution scaffolding | No |
| 2 | build-native.ps1 | No |
| 3 | Core types (InputFormat, ConversionOptions, MqException) | No |
| 4 | Native interop (NativeMethods.cs) | No |
| 5 | MqResult | Yes |
| 6 | MqEngine | Yes |
| 7 | Mq / MqQueryBuilder | Yes |
| 8 | Runtime packages (6 × .csproj) | No |
| 9 | GitHub Actions CI | No |
| 10 | Final verification | — |
