# MQNet

[![Tests](https://github.com/panuoksala/mqnet/actions/workflows/tests.yml/badge.svg)](https://github.com/panuoksala/mqnet/actions/workflows/tests.yml)
[![CI](https://github.com/panuoksala/mqnet/actions/workflows/ci.yml/badge.svg)](https://github.com/panuoksala/mqnet/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/MQNet.svg)](https://www.nuget.org/packages/MQNet)

MQNet is a .NET wrapper for [mq](https://github.com/harehare/mq), a jq-like query tool for Markdown. Use it to find headings, code blocks, links, and other content with simple queries.

## Install

```shell
dotnet add package MQNet
```

The package automatically selects the native runtime for your platform.

## Quick start

```csharp
using MQNet;

var result = Mq.Query(".h(1)")
    .On("# Hello\n\n## World\n\n# Another")
    .Run();

// result.Values: ["# Hello", "# Another"]
```

Queries use mq's query language:

```csharp
var headings = Mq.Query(".h | select(contains(\"API\"))")
    .On(markdown)
    .Run();

var rustBlocks = Mq.Query(".code(\"rust\")")
    .On(markdown)
    .Run();
```

## Features

- **Typed selectors:** Use `MarkdownTag.H1`, `MarkdownTag.Code`, and other selectors instead of raw query strings.
- **Reusable engine:** Run multiple queries with `MqEngine`.
- **More than Markdown:** Query HTML, MDX, plain text, or raw input with `WithFormat`.
- **Plain text results:** Use `.WithPlainText()` or mq's `to_text` function.
- **Built-in modules:** Load modules for JSON, YAML, CSV, TOML, XML, semver, tables, and more with `MqEngine.LoadModule`.

For example, query HTML or run multiple queries:

```csharp
var htmlResult = Mq.Query(".h(1)")
    .On("<h1>Title</h1><p>Body</p>")
    .WithFormat(InputFormat.Html)
    .Run();

using var engine = new MqEngine();
var links = engine.Eval(".link", markdown);
```

## Requirements

- .NET 8 or .NET 10
- Windows, Linux, and macOS on x64 or ARM64

See the [mq documentation](https://github.com/harehare/mq) for the full query language and module reference.

## License

MIT. See [LICENSE](LICENSE). MQNet wraps [mq](https://github.com/harehare/mq), also MIT licensed.
