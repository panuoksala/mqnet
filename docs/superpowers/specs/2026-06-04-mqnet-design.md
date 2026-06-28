# MQNet Design Spec

**Date:** 2026-06-04  
**Project:** MQNet — .NET bindings for [mq](https://github.com/harehare/mq)  
**Status:** Approved

---

## Overview

MQNet is a .NET library that wraps the [mq](https://github.com/harehare/mq) Markdown query engine — a jq-like tool for processing Markdown. It uses P/Invoke to call the `mq-ffi` C-compatible shared library produced by the mq Rust project. The library multi-targets `net8.0` and `net10.0`, is distributed as a NuGet package, and ships with pre-built native binaries for all major platforms.

---

## Solution Structure

```
MQNet/
├── MQNet.slnx
├── src/
│   ├── MQNet/                            # Main managed library
│   │   ├── MQNet.csproj                  # <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
│   │   ├── Native/
│   │   │   └── NativeMethods.cs          # P/Invoke declarations
│   │   ├── MqEngine.cs                   # Core IDisposable FFI wrapper
│   │   ├── MqResult.cs                   # Result + MqValue types
│   │   ├── MqQueryBuilder.cs             # Fluent builder (Mq.Query(...))
│   │   ├── InputFormat.cs                # Enum: Markdown, Mdx, Html, Text, Raw
│   │   ├── ConversionOptions.cs          # HTML→Markdown conversion options
│   │   └── MqException.cs               # Exception type for FFI errors
│   └── MQNet.Runtime/                    # One subdirectory per platform RID
│       ├── win-x64/
│       │   ├── MQNet.Runtime.win-x64.csproj
│       │   └── runtimes/win-x64/native/    # mq_ffi.dll (built by CI)
│       ├── win-arm64/
│       │   ├── MQNet.Runtime.win-arm64.csproj
│       │   └── runtimes/win-arm64/native/
│       ├── linux-x64/
│       │   ├── MQNet.Runtime.linux-x64.csproj
│       │   └── runtimes/linux-x64/native/
│       ├── linux-arm64/
│       │   ├── MQNet.Runtime.linux-arm64.csproj
│       │   └── runtimes/linux-arm64/native/
│       ├── osx-x64/
│       │   ├── MQNet.Runtime.osx-x64.csproj
│       │   └── runtimes/osx-x64/native/
│       └── osx-arm64/
│           ├── MQNet.Runtime.osx-arm64.csproj
│           └── runtimes/osx-arm64/native/
├── tests/
│   └── MQNet.Tests/
│       └── MQNet.Tests.csproj            # <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
├── Directory.Build.props                 # Shared version, authors, package metadata (repo root)
├── build/
│   └── build-native.ps1                  # Clones mq, builds mq-ffi for host RID
└── .github/
    └── workflows/
        └── ci.yml                        # Build matrix + pack + publish
```

### NuGet Packages Produced

| Package | Contents |
|---|---|
| `MQNet` | Managed code; declares all `MQNet.Runtime.*` as dependencies |
| `MQNet.Runtime.win-x64` | `runtimes/win-x64/native/mq_ffi.dll` |
| `MQNet.Runtime.win-arm64` | `runtimes/win-arm64/native/mq_ffi.dll` |
| `MQNet.Runtime.linux-x64` | `runtimes/linux-x64/native/libmq_ffi.so` |
| `MQNet.Runtime.linux-arm64` | `runtimes/linux-arm64/native/libmq_ffi.so` |
| `MQNet.Runtime.osx-x64` | `runtimes/osx-x64/native/libmq_ffi.dylib` |
| `MQNet.Runtime.osx-arm64` | `runtimes/osx-arm64/native/libmq_ffi.dylib` |

Consumers add a single reference: `dotnet add package MQNet`. NuGet resolves all runtime packages; .NET's native library resolver loads the correct binary at runtime.

---

## Core API

### `MqEngine` (core P/Invoke wrapper)

```csharp
// Disposable — wraps mq_create / mq_destroy
using var engine = new MqEngine();

// Run a query on Markdown (default format)
MqResult result = engine.Eval(".h1", "# Hello\n\n## World");

// Run with explicit input format
MqResult result = engine.Eval(".h1", "<h1>Hello</h1>", InputFormat.Html);

// Static utility — no engine needed
string markdown = MqEngine.HtmlToMarkdown("<h1>Hi</h1>");
string markdown = MqEngine.HtmlToMarkdown("<h1>Hi</h1>", new ConversionOptions { UseTitleAsH1 = true });
```

- `Dispose()` calls `mq_destroy()`; safe to call multiple times
- Using after dispose throws `ObjectDisposedException`
- FFI errors throw `MqException` with the error message from the native layer

### `MqResult`

```csharp
IReadOnlyList<string> result.Values   // all matched values
string               result.Text      // Values joined by "\n"
string               result[0]        // index access
foreach (var v in result)             // enumerable
int                  result.Count
```

### `MqQueryBuilder` (fluent layer)

```csharp
string text = Mq.Query(".h1")
    .On("# Hello\n\n## World")
    .WithFormat(InputFormat.Markdown)   // optional, default = Markdown
    .Run()
    .Text;
```

Internally `MqQueryBuilder.Run()` creates a short-lived `MqEngine`, calls `Eval()`, disposes it, and returns the result. For bulk queries, use `MqEngine` directly to avoid the per-call engine creation overhead.

### `MarkdownTag`

Strongly-typed, discoverable mq selector values. Use instead of raw query strings.

```csharp
// Well-known tags
Mq.Query(MarkdownTag.H1).On(markdown).Run();
Mq.Query(MarkdownTag.Code).On(markdown).Run();

// Factory methods
Mq.Query(MarkdownTag.HeadingLevel(3)).On(markdown).Run();          // .h(3)
Mq.Query(MarkdownTag.HeadingRange(1, 3)).On(markdown).Run();       // .h(1..3) — inclusive
Mq.Query(MarkdownTag.Heading(1..3)).On(markdown).Run();            // .h(1..2) — C# Range, exclusive end
Mq.Query(MarkdownTag.Heading(1..)).On(markdown).Run();             // .h(1..6) — open end reaches H6
Mq.Query(MarkdownTag.Heading(^3..)).On(markdown).Run();            // .h(3..6) — from-end index, H3–H6
Mq.Query(MarkdownTag.CodeBlock("rust")).On(markdown).Run();        // .code("rust")

// Convenience entry points
Mq.Heading(1).On(markdown).Run();        // shorthand for HeadingLevel(1)
Mq.CodeBlock("rust").On(markdown).Run(); // shorthand for CodeBlock("rust")
```

`MarkdownTag` is a `readonly struct` with:
- **Well-known static properties:** `H1`–`H6`, `AllHeadings`, `Paragraph`/`Text`, `Code`, `InlineCode`, `Link`, `Image`, `List`, `HorizontalRule`, `LineBreak`, `Blockquote`, `Table`, `Footnote`, `MathInline`, `Html`
- **Factory methods:** `HeadingLevel(int)`, `HeadingRange(int, int)` (inclusive), `Heading(Range)` (C# exclusive-end), `CodeBlock(string)`
- Raw string queries via `Mq.Query(string)` remain fully supported.

### `InputFormat` enum

| Value | Description |
|---|---|
| `Markdown` | CommonMark / GFM (default) |
| `Mdx` | Markdown with JSX |
| `Html` | HTML, auto-converted to Markdown before querying |
| `Text` | Plain text split by lines |
| `Raw` | Raw string, no parsing |

### `ConversionOptions`

| Property | Type | Description |
|---|---|---|
| `ExtractScriptsAsCodeBlocks` | `bool` | Convert `<script>` tags to code blocks |
| `GenerateFrontMatter` | `bool` | Generate front matter from HTML `<head>` metadata |
| `UseTitleAsH1` | `bool` | Use `<title>` as H1 heading |

### `MqException`

```csharp
throw new MqException("Error message from native layer");
```

Thrown when `mq_eval` or `mq_html_to_markdown` returns a non-null `error_msg`.

---

## Native Interop

### P/Invoke Declarations (`NativeMethods.cs`)

```csharp
[DllImport("mq_ffi")] static extern IntPtr mq_create();
[DllImport("mq_ffi")] static extern void   mq_destroy(IntPtr engine);
[DllImport("mq_ffi")] static extern MqResultNative mq_eval(
    IntPtr engine, string code, string input, string inputFormat);
[DllImport("mq_ffi")] static extern void   mq_free_result(MqResultNative result);
[DllImport("mq_ffi")] static extern IntPtr mq_html_to_markdown(
    string html, ref MqConversionOptionsNative options);
[DllImport("mq_ffi")] static extern void   mq_free_string(IntPtr str);
```

The native library name `"mq_ffi"` resolves to:
- `mq_ffi.dll` on Windows
- `libmq_ffi.so` on Linux
- `libmq_ffi.dylib` on macOS

### Memory Management

- `MqEngine` uses a `SafeHandle`-style pattern: engine pointer held privately, freed on dispose
- `mq_eval()` result is marshalled to managed strings immediately; `mq_free_result()` called before returning `MqResult` — no unmanaged pointers escape into managed types
- `mq_html_to_markdown()` result is copied to `string`, then freed with `mq_free_string()`

---

## Build & CI

### Local Development (`build/build-native.ps1`)

1. Clone `harehare/mq` at pinned tag into `.mq/` (or pull if already cloned)
2. `cargo build --release -p mq-ffi` for the host platform
3. Copy binary to `src/MQNet.Runtime.<host-rid>/runtimes/<host-rid>/native/`

The `.mq/` directory is `.gitignore`-d.

### GitHub Actions (`ci.yml`)

**Build matrix** — 6 jobs:

| Runner | RID | Cross-compile? |
|---|---|---|
| `windows-latest` | `win-x64` | No |
| `windows-latest` | `win-arm64` | Yes (cargo target) |
| `ubuntu-latest` | `linux-x64` | No |
| `ubuntu-latest` | `linux-arm64` | Yes (cargo cross) |
| `macos-latest` | `osx-x64` | No |
| `macos-latest` | `osx-arm64` | No (M-series runner) |

Each job: installs Rust toolchain → clones mq at pinned tag → `cargo build --release -p mq-ffi` → uploads binary as GitHub Actions artifact.

**Pack job** (depends on all matrix jobs):
1. Downloads all 6 native binary artifacts
2. Places them in the correct `MQNet.Runtime.*/runtimes/*/native/` paths
3. `dotnet test` (Windows + Linux runners)
4. `dotnet pack` → 7 `.nupkg` files
5. On tagged release (`v*.*.*`): `dotnet nuget push` to NuGet.org

### Versioning

`Directory.Build.props` sets a single `<Version>` property used by all projects. Package version matches the pinned mq tag (e.g., `0.5.31`).

---

## Testing

**Project:** `MQNet.Tests` — xUnit, targets `net8.0;net10.0`. Tests run against the real `mq_ffi` binary (no mocking of the native layer).

### Test Categories

**Lifecycle**
- Engine creates and disposes without error
- `ObjectDisposedException` on use after dispose
- Double-dispose is safe

**Query evaluation**
- `.h1` extracts H1 headings
- `.h2` extracts H2 headings
- `.code` extracts code blocks
- `.code("rust")` filters code blocks by language
- Pipe: `.h | select(contains("foo"))`
- Empty input → empty result
- Invalid query → `MqException` with message

**Input formats**
- `InputFormat.Markdown` (default)
- `InputFormat.Html` — HTML is converted then queried
- `InputFormat.Text` — plain text split by lines

**Result API**
- `Values`, `Text`, indexer, enumeration
- Empty result: `Count == 0`, `Text == ""`

**HTML conversion**
- Basic `HtmlToMarkdown` round-trip
- `UseTitleAsH1`, `GenerateFrontMatter`, `ExtractScriptsAsCodeBlocks` flags

**Fluent builder**
- `Mq.Query(...).On(...).WithFormat(...).Run()` round-trip
- Default format is `Markdown`

---

## Constraints & Non-Goals

- **Synchronous only** — mq_eval is CPU-bound FFI; callers wrap in `Task.Run()` if needed
- **No REPL support** — CLI-only feature, not in scope
- **No streaming support** — v1 only; large-file streaming can be added later
- **No LSP/debugger support** — CLI tooling only, not in scope for a library
- **Pinned mq version** — deliberately versioned to a stable mq release tag, not latest

---

## Open Questions (resolved)

| Question | Decision |
|---|---|
| Native library distribution | Build from source in CI at pinned tag |
| Platform targets | Windows + Linux + macOS, x64 + arm64 |
| API style | Thin core + fluent builder |
| Async support | Synchronous only |
| NuGet structure | Split packages (MQNet + MQNet.Runtime.*) |
