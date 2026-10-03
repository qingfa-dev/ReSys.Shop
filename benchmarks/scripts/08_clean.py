#!/usr/bin/env python
"""Remove generated outputs while preserving source code and configs.

Usage::

    uv run python scripts/08_clean.py           # remove outputs/
    uv run python scripts/08_clean.py --cache   # also remove data/cache/
    uv run python scripts/08_clean.py --all     # remove outputs/ and data/cache/
"""
from __future__ import annotations

import argparse
import shutil
from pathlib import Path


CLEAN_DIRS = [
    Path("outputs/embeddings"),
    Path("outputs/metrics"),
    Path("outputs/reports"),
    Path("outputs/tables"),
    Path("outputs/figures"),
    Path("outputs/logs"),
]

CACHE_DIR = Path("data/cache")


def clean(also_cache: bool = False) -> None:
    removed = []
    for d in CLEAN_DIRS:
        if d.exists():
            shutil.rmtree(d)
            d.mkdir(parents=True, exist_ok=True)
            (d / ".gitkeep").touch()
            removed.append(str(d))

    if also_cache and CACHE_DIR.exists():
        shutil.rmtree(CACHE_DIR)
        CACHE_DIR.mkdir(parents=True, exist_ok=True)
        removed.append(str(CACHE_DIR))

    if removed:
        print(f"Cleaned: {', '.join(removed)}")
    else:
        print("Nothing to clean.")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--cache", action="store_true", help="Also clear data/cache/")
    parser.add_argument("--all",   action="store_true", help="Clean outputs and cache")
    args = parser.parse_args()
    clean(also_cache=args.cache or args.all)


if __name__ == "__main__":
    main()
