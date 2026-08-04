#!/usr/bin/env python
"""Extract option value entities (single entity: OptionValue).

Colors come from the whole styles.csv; sizes come from the style JSON of
the selected products (matching the old 01 + 02 combined behavior).
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import option_value_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import OPTION_TYPE_COLOR_ID, OPTION_TYPE_SIZE_ID, SCRIPTS_DIR  # noqa: E402
from source import extract_sizes, group_products, load_styles_rows  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract option value seed data")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "004_demo_option_values.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    colors: set[str] = set()
    sizes: set[str] = set()

    for row in rows:
        c = row.get("baseColour", "").strip()
        if c:
            colors.add(c)

    for _name, group_rows in group_products(rows)[: args.count]:
        for row in group_rows:
            style_json_path = args.dataset / "styles" / f"{row.get('id', '').strip()}.json"
            if not style_json_path.exists():
                continue
            try:
                sizes.update(extract_sizes(json.loads(style_json_path.read_text())))
            except Exception:
                continue

    values: list[dict] = []
    for pos, color in enumerate(sorted(colors)):
        values.append({
            "id": option_value_id("color", color),
            "option_type_id": OPTION_TYPE_COLOR_ID,
            "name": color, "presentation": color, "position": pos,
        })
    for pos, size in enumerate(sorted(sizes)):
        values.append({
            "id": option_value_id("size", size),
            "option_type_id": OPTION_TYPE_SIZE_ID,
            "name": size, "presentation": size, "position": len(colors) + pos,
        })

    ensure_output_dir(args.output)
    write_json(out, values)
    print(f"Written {len(colors)} colors + {len(sizes)} sizes to {out}")


if __name__ == "__main__":
    main()
