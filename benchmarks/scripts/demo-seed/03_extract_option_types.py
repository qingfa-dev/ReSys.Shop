#!/usr/bin/env python
"""Extract option type entities (single entity: OptionType)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import OPTION_TYPE_COLOR_ID, OPTION_TYPE_SIZE_ID, SCRIPTS_DIR  # noqa: E402


def build_option_types_json() -> list[dict]:
    return [
        {"id": OPTION_TYPE_SIZE_ID, "name": "Size", "presentation": "Size", "position": 0, "filterable": True},
        {"id": OPTION_TYPE_COLOR_ID, "name": "Color", "presentation": "Color", "position": 1, "filterable": True},
    ]


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract option type seed data")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "003_demo_option_types.json"
    check_overwrite(out, args.force)
    ensure_output_dir(args.output)
    write_json(out, build_option_types_json())
    print(f"Written 2 option types to {out}")


if __name__ == "__main__":
    main()
