"""
CLI entry point.

Usage:
  uv run benchmark run --dataset-root data/raw/fashion-product-images-small --split-file data/raw/fashion-product-images-small/splits/test.json --models all
  uv run benchmark run ...                   (run benchmarks)
  uv run benchmark thesis ...                (run thesis benchmark)
  uv run benchmark report ...                (regenerate reports)
  uv run benchmark cache ...                 (manage cache)
"""
from __future__ import annotations

import typer

from benchmark.cli.benchmark import app as benchmark_app

app = typer.Typer(
    name="benchmark",
    help="Fashion image retrieval benchmark — one-shot model comparison.",
    no_args_is_help=True,
)

app.add_typer(benchmark_app, name="benchmark")

if __name__ == "__main__":
    app()
