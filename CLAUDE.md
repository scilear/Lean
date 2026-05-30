# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

This is the [QuantConnect Lean](https://github.com/QuantConnect/Lean) algorithmic trading engine — an open-source, event-driven backtesting and live-trading platform. Algorithms can be written in C# or Python. The engine runs via a `.NET 9` solution (`QuantConnect.Lean.sln`).

## Build

```bash
# Register the local NuGet source first (one-time setup)
dotnet nuget add source /path/to/Lean/LocalPackages

# Build the full solution (Release)
dotnet build /p:Configuration=Release /v:quiet /p:WarningLevel=1 QuantConnect.Lean.sln

# Build debug (default)
dotnet build QuantConnect.Lean.sln
```

> CI builds run inside `quantconnect/lean:foundation` Docker image; locally you need dotnet 9 SDK and (for Python algorithms) Python 3.11 with `PYTHONNET_PYDLL` set.

## Run Tests

All tests live in `Tests/QuantConnect.Tests.dll` (after build). The test framework is NUnit. Filter by `TestCategory`:

```bash
# Run all non-excluded tests (standard CI gate)
dotnet test ./Tests/bin/Release/QuantConnect.Tests.dll \
  --filter "TestCategory!=TravisExclude&TestCategory!=ResearchRegressionTests" \
  -- TestRunParameters.Parameter\(name=\"log-handler\", value=\"ConsoleErrorLogHandler\"\)

# Run only regression tests
dotnet test ./Tests/bin/Release/QuantConnect.Tests.dll \
  --filter TestCategory=RegressionTests

# Run a single test by name
dotnet test ./Tests/bin/Release/QuantConnect.Tests.dll \
  --filter "FullyQualifiedName~MyTestClassName"
```

Known test categories: `RegressionTests`, `ResearchRegressionTests`, `TravisExclude`.

## Python Syntax Check

```bash
pip install quantconnect-stubs types-requests mypy
python run_syntax_check.py
```

## Run the Engine Locally

```bash
# Edit Launcher/config.json first to set algorithm-type-name, algorithm-language, etc.
cd Launcher/bin/Debug
dotnet QuantConnect.Lean.Launcher.dll
```

Alternatively use the [Lean CLI](https://www.lean.io/cli): `lean backtest`, `lean live`, `lean research`.

## Architecture Overview

### Core Execution Flow

```
Launcher/Program.cs
  └─> Engine.cs                   # primary loop: loads job, wires handlers
        ├─> LeanEngineSystemHandlers   (job queue, messaging, API)
        ├─> LeanEngineAlgorithmHandlers (data feed, results, transactions, real-time)
        └─> AlgorithmManager.cs        # event dispatch loop: feeds time slices to the algorithm
```

**`AlgorithmManager`** is the heartbeat. It receives `TimeSlice` objects from the data feed and calls the algorithm's event handlers (`OnData`, `OnEndOfDay`, scheduled events, etc.) in sequence.

### Key Projects

| Project | Role |
|---|---|
| `Common/` | Shared types: `Symbol`, `Security`, `BaseData`, `Order`, interfaces (`IAlgorithm`), `Securities/` hierarchy |
| `Algorithm/` | `QCAlgorithm` base class — what user algorithms extend; also holds indicators wrappers and universe selection helpers |
| `Algorithm.Framework/` | Modular framework: `Alphas/`, `Selection/`, `Portfolio/`, `Risk/`, `Execution/` — plug-in model for composable strategies |
| `Engine/` | Runtime: `DataFeeds/` (subscription management, enumerators), `TransactionHandlers/`, `Results/`, `RealTime/`, `Setup/` |
| `Brokerages/` | Brokerage abstractions and paper/backtesting implementations |
| `Indicators/` | 150+ technical indicators, all implementing `IIndicator` |
| `Algorithm.CSharp/` | C# sample/regression algorithms |
| `Algorithm.Python/` | Python sample/regression algorithms |
| `Tests/` | NUnit tests, mirroring the project structure above |

### Algorithm Framework (modular pipeline)

When using `QCAlgorithm.SetAlpha(...)` / `SetPortfolioConstruction(...)` / etc., Lean routes through a five-stage pipeline:

1. **Universe Selection** (`Algorithm.Framework/Selection/`) — which securities to trade
2. **Alpha Model** (`Algorithm.Framework/Alphas/`) — generates `Insight` objects (direction + magnitude + confidence)
3. **Portfolio Construction** (`Algorithm.Framework/Portfolio/`) — converts insights → `PortfolioTarget` (target weights/quantities)
4. **Risk Management** (`Algorithm.Framework/Risk/`) — adjusts targets for drawdown / position limits
5. **Execution** (`Algorithm.Framework/Execution/`) — places orders to hit targets

### Configuration

`Launcher/config.json` controls everything at runtime:

- `algorithm-type-name` / `algorithm-language` / `algorithm-location` — which algorithm to run
- `data-folder` — path to market data (default: `../../../Data/`)
- `environment` — `"backtesting"` or `"live-paper"` etc.
- All handlers (data provider, brokerage, results, messaging) are swappable via string class names

The `LocalPackages/` directory holds `.nupkg` files for pre-release dependencies. Must be registered as a NuGet source with `dotnet nuget add source`.

### Data Layout

`Data/` follows the pattern: `Data/{asset-class}/{market}/{resolution}/{ticker}/{date}.zip`. Equity data lives under `Data/equity/usa/`, options under `Data/option/usa/`, etc.

### Python Support

Python algorithms use **PythonNet** to bridge .NET and CPython. They extend `QCAlgorithm` from Python exactly like C# algorithms. The `AlgorithmFactory/Python/Wrappers/` layer wraps Python objects in .NET-compatible proxies. Set `PYTHONNET_PYDLL` to the path of `libpython3.11.so` (Linux) / `.dylib` (macOS) / `.dll` (Windows).

## Branch & PR Conventions

- Branch naming: `bug-<issue#>-<description>` or `feature-<issue#>-<description>`
- All PRs must include accompanying NUnit tests
- C# follows Microsoft C# guidelines; 4-space soft tabs
- Framework modules must be silent (no logging/charting inside alpha/selection/portfolio/risk/execution models)
- Rebase onto `upstream/master` before submitting; do not rebase after pushing to origin
