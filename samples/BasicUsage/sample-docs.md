# Project Chimera

A fictional multi-language platform for composing and transforming data pipelines.

## Overview

Project Chimera is an open-source toolkit that lets you build, test, and deploy composable
data pipelines. Pipelines are described as simple YAML or TOML configuration files and
executed by a lightweight runtime available for Linux, macOS, and Windows.

The project ships three main components:

- **chimera-core** — the pipeline execution engine
- **chimera-cli** — command-line interface for running and inspecting pipelines
- **chimera-sdk** — language SDKs for Go, Rust, Python, and C#

## Installation

### Prerequisites

Before installing Chimera, ensure your system meets the following requirements:

- Linux x64/arm64, macOS x64/arm64, or Windows x64/arm64
- 512 MB RAM minimum (2 GB recommended for large pipelines)
- Docker 24+ (optional, required for containerised steps)

### Install via script

The quickest way to install Chimera is the official install script:

```bash
curl -sSL https://chimera.example.com/install.sh | bash
```

### Install via package manager

```bash
# macOS / Linux (Homebrew)
brew install chimera

# Windows (winget)
winget install Chimera.Chimera
```

### Build from source

```bash
git clone https://github.com/example/chimera.git
cd chimera
cargo build --release
```

## Getting Started

### Your first pipeline

Create a file called `hello.yaml`:

```yaml
name: hello-world
steps:
  - name: greet
    run: echo "Hello from Chimera!"
  - name: timestamp
    run: date --iso-8601=seconds
```

Run it:

```bash
chimera run hello.yaml
```

### Passing parameters

```yaml
name: parameterised
parameters:
  - name: target
    default: world
steps:
  - name: greet
    run: echo "Hello, {{ target }}!"
```

```bash
chimera run parameterised.yaml --param target=Chimera
```

## API Reference

### Core API

The `chimera-core` package exposes the following public API surface:

#### Pipeline

```go
type Pipeline struct {
    Name       string
    Parameters []Parameter
    Steps      []Step
}

func Load(path string) (*Pipeline, error)
func (p *Pipeline) Run(ctx context.Context, params map[string]string) (*Result, error)
func (p *Pipeline) Validate() []ValidationError
```

#### Step

```go
type Step struct {
    Name    string
    Run     string
    Image   string   // optional Docker image
    Timeout Duration
    Env     map[string]string
}
```

#### Result

```go
type Result struct {
    ExitCode int
    Stdout   string
    Stderr   string
    Duration Duration
    Steps    []StepResult
}
```

### CLI Reference

```
chimera run <pipeline>  [--param key=value]...  Run a pipeline
chimera validate <file>                          Validate a pipeline file
chimera list                                     List available pipelines
chimera logs [--follow] <run-id>                 Stream or tail run logs
chimera version                                  Print version information
```

## Configuration

Chimera reads configuration from `~/.chimera/config.toml` or the path specified by
`CHIMERA_CONFIG`. The file is optional; all values have sensible defaults.

```toml
[runtime]
concurrency = 4
timeout     = "30m"
log_level   = "info"

[docker]
enabled    = true
pull_policy = "if-not-present"

[telemetry]
enabled  = false
endpoint = "https://telemetry.example.com"
```

## Architecture

### Pipeline execution model

Each pipeline runs in an isolated execution context. Steps execute sequentially by default;
parallel groups can be declared with the `parallel` key. A step failure stops the pipeline
unless `continue_on_error: true` is set on the step.

```
┌────────────────────────────────────┐
│           chimera-cli              │
│  parse args → load pipeline file  │
└───────────────┬────────────────────┘
                │
                ▼
┌────────────────────────────────────┐
│          chimera-core              │
│  validate → schedule → execute    │
│  ┌─────────┐  ┌─────────────────┐ │
│  │ Step A  │  │ Step B (docker) │ │
│  └─────────┘  └─────────────────┘ │
└────────────────────────────────────┘
```

### Plugin system

Chimera's step executor is pluggable. Built-in executor types:

| Type       | Key        | Description                          |
|------------|------------|--------------------------------------|
| Shell      | `shell`    | Run arbitrary shell commands         |
| Docker     | `docker`   | Run commands inside a container      |
| HTTP       | `http`     | Make HTTP requests                   |
| Script     | `script`   | Execute an embedded script file      |
| Subpipeline| `pipeline` | Compose pipelines recursively        |

## Contributing

### Development setup

```bash
git clone https://github.com/example/chimera.git
cd chimera
make dev-deps   # install development dependencies
make test       # run the full test suite
make lint       # run linters
```

### Running the test suite

```bash
# All tests
cargo test --workspace

# Integration tests only
cargo test --test integration

# With output
cargo test -- --nocapture
```

### Commit conventions

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add HTTP step executor
fix: handle empty pipeline gracefully
docs: update CLI reference
chore: bump serde to 1.0.197
```

### Code of conduct

All contributors are expected to abide by the [Contributor Covenant](https://www.contributor-covenant.org/) v2.1.

## Changelog

### v1.3.0 — 2026-06-01

- feat: add subpipeline step type for recursive composition
- feat: add `chimera validate` command
- fix: docker executor no longer leaks file descriptors on timeout
- fix: parameter interpolation now handles nested `{{ }}` correctly
- docs: rewrite Getting Started guide
- chore: bump Rust edition to 2024

### v1.2.0 — 2026-03-15

- feat: parallel step groups with `parallel` key
- feat: telemetry support (opt-in)
- fix: `chimera logs --follow` exits cleanly on pipeline completion
- chore: drop support for macOS x64 (Intel)

### v1.1.0 — 2025-12-01

- feat: plugin system for custom step executors
- feat: TOML pipeline format support alongside YAML
- fix: `continue_on_error` flag was ignored in parallel groups
- docs: add Architecture section

### v1.0.0 — 2025-09-01

- Initial stable release

## FAQ

**Q: Can I use Chimera without Docker?**
Yes. Docker is optional. Shell steps run natively; only `docker`-type steps require it.

**Q: Is Chimera production-ready?**
v1.0.0 is the first stable release. It is used in production at several organisations.
Breaking changes will follow SemVer from this point onwards.

**Q: Where are logs stored?**
Run logs are written to `~/.chimera/logs/<run-id>.log` by default. The path is configurable
via `log_dir` in `config.toml`.

**Q: Does Chimera support Windows?**
Yes — Windows x64 and ARM64 are first-class targets since v1.1.0.

## License

Apache-2.0. See [LICENSE](LICENSE) for the full text.

> Built with care by the Chimera contributors. Contributions welcome!
