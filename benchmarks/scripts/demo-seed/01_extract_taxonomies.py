#!/usr/bin/env python
"""Extract taxonomy entities from styles.csv (single entity: Taxonomy)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import (  # noqa: E402
    SCRIPTS_DIR,
    TAXONOMY_ARTICLE_TYPES_ID,
    TAXONOMY_BRANDS_ID,
    TAXONOMY_CATEGORIES_ID,
)


def build_taxonomies_json() -> list[dict]:
    return [
        {"id": TAXONOMY_CATEGORIES_ID, "name": "Categories", "presentation": "Departments", "position": 0},
        {"id": TAXONOMY_BRANDS_ID, "name": "Brands", "presentation": "Brands", "position": 1},
        {"id": TAXONOMY_ARTICLE_TYPES_ID, "name": "Article Types", "presentation": "Article Types", "position": 2},
    ]


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract taxonomy seed data")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "001_demo_taxonomies.json"
    check_overwrite(out, args.force)
    ensure_output_dir(args.output)
    write_json(out, build_taxonomies_json())
    print(f"Written 3 taxonomies to {out}")


if __name__ == "__main__":
    main()
