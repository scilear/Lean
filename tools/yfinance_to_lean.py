#!/usr/bin/env python3
"""Download yfinance OHLCV bars into Lean's local equity data layout."""

from __future__ import annotations

import argparse
import sys
import zipfile
from pathlib import Path
from zoneinfo import ZoneInfo

import pandas as pd
import yfinance as yf


PRICE_SCALE = 10_000
NEW_YORK = ZoneInfo("America/New_York")


def _parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Preload US equity daily or minute bars from yfinance into Lean data files."
    )
    parser.add_argument("tickers", nargs="+", help="Ticker symbols supported by yfinance, for example SPY AAPL MSFT")
    parser.add_argument("--start", required=True, help="Start date, YYYY-MM-DD")
    parser.add_argument("--end", required=True, help="End date, YYYY-MM-DD. yfinance treats this as exclusive.")
    parser.add_argument("--resolution", choices=["daily", "minute"], default="daily")
    parser.add_argument("--data-dir", default="data", help="Lean data directory, defaults to ./data")
    parser.add_argument(
        "--overwrite",
        action="store_true",
        help="Replace existing Lean zip files. Without this flag, existing files are left untouched.",
    )
    return parser.parse_args()


def _interval(resolution: str) -> str:
    return "1d" if resolution == "daily" else "1m"


def _normalize_download(frame: pd.DataFrame, ticker: str) -> pd.DataFrame:
    if frame.empty:
        return frame

    if isinstance(frame.columns, pd.MultiIndex):
        if ticker in frame.columns.get_level_values(-1):
            frame = frame.xs(ticker, axis=1, level=-1)
        elif ticker in frame.columns.get_level_values(0):
            frame = frame.xs(ticker, axis=1, level=0)

    rename = {column: str(column).title().replace(" ", "") for column in frame.columns}
    frame = frame.rename(columns=rename)
    required = ["Open", "High", "Low", "Close", "Volume"]
    missing = [column for column in required if column not in frame.columns]
    if missing:
        raise ValueError(f"{ticker}: yfinance response is missing columns: {', '.join(missing)}")

    frame = frame[required].dropna()
    return frame[frame["Volume"].fillna(0) > 0]


def _scaled(value: float) -> int:
    return int(round(float(value) * PRICE_SCALE))


def _write_zip(path: Path, entry_name: str, lines: list[str], overwrite: bool) -> bool:
    if path.exists() and not overwrite:
        print(f"skip existing {path}")
        return False

    path.parent.mkdir(parents=True, exist_ok=True)
    payload = "\n".join(lines) + "\n"
    with zipfile.ZipFile(path, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        archive.writestr(entry_name, payload)
    print(f"wrote {path} ({len(lines)} rows)")
    return True


def _write_auxiliary_files(data_dir: Path, ticker: str, first_date: pd.Timestamp, first_close: float) -> None:
    equity_dir = data_dir / "equity" / "usa"
    start = first_date.strftime("%Y%m%d")
    symbol = ticker.lower()

    map_file = equity_dir / "map_files" / f"{symbol}.csv"
    if not map_file.exists():
        map_file.parent.mkdir(parents=True, exist_ok=True)
        map_file.write_text(f"{start},{symbol},N\n20501231,{symbol},N\n", encoding="utf-8")
        print(f"wrote {map_file}")

    factor_file = equity_dir / "factor_files" / f"{symbol}.csv"
    if not factor_file.exists():
        factor_file.parent.mkdir(parents=True, exist_ok=True)
        factor_file.write_text(f"{start},1,1,{float(first_close):.6f}\n20501231,1,1,0\n", encoding="utf-8")
        print(f"wrote {factor_file}")


def _daily_lines(frame: pd.DataFrame) -> list[str]:
    lines: list[str] = []
    for index, row in frame.sort_index().iterrows():
        stamp = pd.Timestamp(index).strftime("%Y%m%d 00:00")
        lines.append(
            f"{stamp},{_scaled(row.Open)},{_scaled(row.High)},{_scaled(row.Low)},"
            f"{_scaled(row.Close)},{int(row.Volume)}"
        )
    return lines


def _minute_groups(frame: pd.DataFrame) -> dict[str, list[str]]:
    if frame.index.tz is None:
        frame = frame.tz_localize(NEW_YORK)
    else:
        frame = frame.tz_convert(NEW_YORK)

    grouped: dict[str, list[str]] = {}
    for index, row in frame.sort_index().iterrows():
        timestamp = pd.Timestamp(index)
        date_key = timestamp.strftime("%Y%m%d")
        millis = (
            timestamp.hour * 60 * 60 * 1000
            + timestamp.minute * 60 * 1000
            + timestamp.second * 1000
            + timestamp.microsecond // 1000
        )
        grouped.setdefault(date_key, []).append(
            f"{millis},{_scaled(row.Open)},{_scaled(row.High)},{_scaled(row.Low)},"
            f"{_scaled(row.Close)},{int(row.Volume)}"
        )
    return grouped


def _download(ticker: str, args: argparse.Namespace) -> pd.DataFrame:
    return yf.download(
        ticker,
        start=args.start,
        end=args.end,
        interval=_interval(args.resolution),
        auto_adjust=False,
        progress=False,
        threads=False,
    )


def main() -> int:
    args = _parse_args()
    data_dir = Path(args.data_dir)
    wrote_any = False

    for ticker in args.tickers:
        ticker = ticker.upper()
        symbol = ticker.lower()
        frame = _normalize_download(_download(ticker, args), ticker)
        if frame.empty:
            print(f"{ticker}: no rows returned by yfinance for {args.start} to {args.end}", file=sys.stderr)
            continue

        _write_auxiliary_files(data_dir, symbol, pd.Timestamp(frame.index[0]), float(frame.iloc[0].Close))

        if args.resolution == "daily":
            lines = _daily_lines(frame)
            wrote_any |= _write_zip(
                data_dir / "equity" / "usa" / "daily" / f"{symbol}.zip",
                f"{symbol}.csv",
                lines,
                args.overwrite,
            )
        else:
            for date_key, lines in _minute_groups(frame).items():
                wrote_any |= _write_zip(
                    data_dir / "equity" / "usa" / "minute" / symbol / f"{date_key}_trade.zip",
                    f"{date_key}_{symbol}_minute_trade.csv",
                    lines,
                    args.overwrite,
                )

    return 0 if wrote_any else 1


if __name__ == "__main__":
    raise SystemExit(main())
