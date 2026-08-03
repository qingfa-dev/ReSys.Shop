#!/usr/bin/env python
"""Extract product↔taxon classification entities (single entity: ProductTaxon)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import product_id, taxon_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import SCRIPTS_DIR  # noqa: E402
from source import group_products, load_styles_rows  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract product taxon seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "008_demo_product_taxons.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    refs: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        pid = product_id(display_name)
        mc = first.get("masterCategory", "").strip()
        b = first.get("brandName", "").strip()
        at = first.get("articleType", "").strip()
        pos = 0
        if mc:
            refs.append({"product_id": pid, "taxon_id": taxon_id(f"cat.{mc}"), "position": pos})
            pos += 1
        if b:
            refs.append({"product_id": pid, "taxon_id": taxon_id(f"brand.{b}"), "position": pos})
            pos += 1
        if at:
            refs.append({"product_id": pid, "taxon_id": taxon_id(f"article_type.{at}"), "position": pos})
            pos += 1

    ensure_output_dir(args.output)
    write_json(out, refs)
    print(f"Written {len(refs)} product taxon refs to {out}")


if __name__ == "__main__":
    main()
