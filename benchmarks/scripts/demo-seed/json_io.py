"""JSON read/write helpers for demo seed datasets."""
from __future__ import annotations

import json
import sys
from pathlib import Path


def check_overwrite(path: Path, force: bool) -> None:
    """Exit if output file exists and --force not set."""
    if path.exists() and not force:
        print(f"Output already exists: {path}")
        print("Use --force to overwrite.")
        sys.exit(1)


def write_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, indent=2))


def load_json(path: Path) -> list[dict]:
    return json.loads(path.read_text())


def ensure_output_dir(path: Path) -> None:
    path.mkdir(parents=True, exist_ok=True)
