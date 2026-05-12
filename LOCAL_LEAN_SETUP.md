# Local QuantConnect Lean Setup

This directory contains a local QuantConnect Lean source tree plus a Lean CLI
workspace for Python algorithm development.

## What Was Installed

- Lean source was copied from `https://github.com/QuantConnect/Lean` into this
  directory.
- Lean CLI `1.0.225` was installed into the workspace virtual environment:
  `.venv/`.
- yfinance and pandas were installed into the same virtual environment.
- Lean CLI state was redirected into `.lean-home/` by running CLI commands with:
  `HOME=/home/fabien/Documents/QCLean/.lean-home`.
- `lean.json` was generated from `Launcher/config.json`, with `data-folder`
  changed to `data`.
- `data/` was seeded from Lean's bundled sample `Data/` folder.
- `algorithms/SampleYFinanceAlgo` was created as a Python starter project.

## Decisions

- Use Lean CLI + Docker for Python algorithm work. This avoids requiring a local
  .NET SDK just to run Python backtests.
- Keep Python tooling isolated in `.venv` instead of installing packages into
  system Python.
- Use yfinance as a preload data source. The helper writes Lean-compatible local
  data files before the backtest starts, so algorithms can use normal
  `add_equity(...)` subscriptions.
- Use a local placeholder organization id in `lean.json`. The current Lean CLI
  requires an organization id to recognize a folder as initialized, but local
  backtests do not need QuantConnect cloud credentials.

## Commands

Activate the local tools:

```bash
source .venv/bin/activate
export HOME=/home/fabien/Documents/QCLean/.lean-home
```

Check the CLI:

```bash
lean --version
```

Create another Python project:

```bash
lean project-create algorithms/MyNewAlgo --language python
```

Download daily bars from yfinance into Lean local data:

```bash
python tools/yfinance_to_lean.py SPY --start 2024-01-02 --end 2024-01-13 --resolution daily --overwrite
```

Download minute bars, subject to yfinance intraday retention limits:

```bash
python tools/yfinance_to_lean.py SPY --start 2026-05-01 --end 2026-05-02 --resolution minute --overwrite
```

Run the sample backtest:

```bash
lean backtest algorithms/SampleYFinanceAlgo --no-update --lean-config lean.json
```

Debug a Python backtest with Lean CLI support:

```bash
lean backtest algorithms/SampleYFinanceAlgo --debug debugpy --no-update --lean-config lean.json
```

## yfinance Data Helper

The helper is `tools/yfinance_to_lean.py`.

Supported scope:

- US equities.
- Daily bars written to `data/equity/usa/daily/{ticker}.zip`.
- Minute bars written to
  `data/equity/usa/minute/{ticker}/{YYYYMMDD}_trade.zip`.
- OHLC prices scaled by `10000`, matching Lean equity trade-bar files.
- Simple map and factor files are created when missing.

Limitations:

- yfinance is suitable for research/backtesting convenience, not live trading.
- Intraday history availability is limited by Yahoo/yfinance retention.
- The helper writes raw yfinance OHLCV as Lean local data; it does not model
  corporate action history beyond simple placeholder factor files for new
  tickers.

## Verification Completed

- `lean --version` returned `lean 1.0.225`.
- SPY daily bars for 2024-01-02 through 2024-01-12 were downloaded and written to
  `data/equity/usa/daily/spy.zip`.
- The sample Python algorithm ran successfully in Docker:
  `algorithms/SampleYFinanceAlgo/backtests/2026-05-11_09-57-15`.
- The backtest placed one SPY order from local daily data and completed with:
  `End Equity 101317.70`.
- Syntax was checked for `tools/yfinance_to_lean.py` and
  `algorithms/SampleYFinanceAlgo/main.py` with Python AST parsing.

## Difficulties Overcome

- The original `.git` path in this environment is a read-only `tmpfs` mount, not
  a valid git repository. Because of that, a true root `git clone` could not be
  created. The Lean working tree was cloned into `/tmp` and copied into this
  directory while leaving the mounted `.git` path untouched.
- Network access was blocked in the default sandbox for GitHub, PyPI,
  QuantConnect CDN, and Yahoo Finance. Commands that needed external access were
  rerun with explicit approval.
- `lean init` requires QuantConnect credentials and tries to write `~/.lean`.
  For a local-only workflow, `lean.json`, `data/`, and `.lean-home/.lean/config`
  were created locally instead.
- Lean CLI currently depends on legacy `pkg_resources`; setuptools `82.0.1`
  removed it. The virtualenv was pinned to `setuptools<81`.
- Docker is installed, but the default sandbox user cannot access the Docker
  socket. The successful backtest used approved elevated Docker socket access.
