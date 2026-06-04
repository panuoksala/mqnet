# MQNet

[![Tests](https://github.com/panuoksala/mqnet/actions/workflows/tests.yml/badge.svg)](https://github.com/panuoksala/mqnet/actions/workflows/tests.yml)
[![CI](https://github.com/panuoksala/mqnet/actions/workflows/ci.yml/badge.svg)](https://github.com/panuoksala/mqnet/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/MQNet.svg)](https://www.nuget.org/packages/MQNet)

A .NET wrapper for [mq](https://github.com/harehare/mq) — a jq-like query tool for Markdown. Query headings, code blocks, paragraphs, and more using a simple, composable query language.

## Installation

```shell
dotnet add package MQNet
```

MQNet uses a split NuGet package model. The main package contains the managed library; native binaries are in separate runtime packages that NuGet resolves automatically based on your platform:

| Platform | Runtime Package |
|----------|----------------|
| Windows x64 | `MQNet.Runtime.win-x64` |
| Windows ARM64 | `MQNet.Runtime.win-arm64` |
| Linux x64 | `MQNet.Runtime.linux-x64` |
| Linux ARM64 | `MQNet.Runtime.linux-arm64` |
| macOS x64 | `MQNet.Runtime.osx-x64` |
| macOS ARM64 | `MQNet.Runtime.osx-arm64` |

## Quick Start

```csharp
using MQNet;

// Fluent API — quick one-shot queries
var result = Mq.Query(".h(1)")
    .On("# Hello\n\n## World\n\n# Another")
    .Run();

// result[0]  → "# Hello"
// result[1]  → "# Another"
// result.Text → "# Hello\n# Another"
```

## Usage

### Fluent API

```csharp
// Extract all H2 headings
var headings = Mq.Query(".h(2)").On(markdown).Run();

// Filter headings containing a word
var filtered = Mq.Query(".h | select(contains(\"API\"))").On(markdown).Run();

// Extract code blocks by language
var rustBlocks = Mq.Query(".code(\"rust\")").On(markdown).Run();

// Query from HTML input
var result = Mq.Query(".h(1)")
    .On("<h1>Title</h1><p>Body</p>")
    .WithFormat(InputFormat.Html)
    .Run();

// Query plain text line by line
var matches = Mq.Query("select(contains(\"error\"))")
    .On(logOutput)
    .WithFormat(InputFormat.Text)
    .Run();
```

### MqEngine (reuse across multiple queries)

```csharp
using var engine = new MqEngine();

var h1 = engine.Eval(".h(1)", markdown);
var code = engine.Eval(".code", markdown);
var links = engine.Eval(".link", markdown);
```

### HTML to Markdown conversion

```csharp
// Basic conversion
string markdown = MqEngine.HtmlToMarkdown("<h1>Hello</h1><p>World</p>");

// With options
string markdown = MqEngine.HtmlToMarkdown(html, new ConversionOptions
{
    UseTitleAsH1 = true,
    GenerateFrontMatter = true,
    ExtractScriptsAsCodeBlocks = true
});
```

### Working with results

```csharp
MqResult result = Mq.Query(".h").On(markdown).Run();

result.Count       // number of matches
result[0]          // first match
result.Values      // IReadOnlyList<string>
result.Text        // all matches joined by "\n"

foreach (var item in result)
    Console.WriteLine(item);
```

## Input Formats

| Format | Description |
|--------|-------------|
| `InputFormat.Markdown` | CommonMark / GFM Markdown (default) |
| `InputFormat.Mdx` | Markdown with JSX (MDX) |
| `InputFormat.Html` | HTML — auto-converted to Markdown before querying |
| `InputFormat.Text` | Plain text, split by lines |
| `InputFormat.Raw` | Raw string, no parsing |

## Requirements

- .NET 8 or .NET 10
- Supported platforms: Windows x64/ARM64, Linux x64/ARM64, macOS x64/ARM64

## mq Query Language

MQNet wraps the [mq](https://github.com/harehare/mq) Rust library. For the full query language reference, see the [mq documentation](https://github.com/harehare/mq).

Some common queries:

```
.h          # all headings
.h(1)       # H1 headings only
.h(2)       # H2 headings only
.code       # all code blocks
.code("go") # code blocks with language "go"
.p          # paragraphs
.link       # links
.image      # images
.list       # list items

# Combinators
.h | select(contains("API"))         # headings containing "API"
.h | map(ascii_downcase)             # lowercase all headings
.code | select(startswith("fn "))    # code blocks starting with "fn "
```

## License

MIT — see [LICENSE](LICENSE).

This project wraps [mq](https://github.com/harehare/mq) by [harehare](https://github.com/harehare), which is also MIT licensed.
