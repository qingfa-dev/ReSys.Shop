#!/usr/bin/env python
"""Extract variant entities from styles.csv (single entity: Variant).

One variant per color×size combination (size-major order), capped at 10
per product; the master variant is the first combination. Each variant
embeds its option values (one per option type).
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import master_variant_id, product_id, variant_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import cost_price, variant_dimensions  # noqa: E402
from shared import (  # noqa: E402
    ARTICLE_PRICE_MAP,
    OPTION_TYPE_COLOR_ID,
    OPTION_TYPE_SIZE_ID,
    SCRIPTS_DIR,
)
from source import (
    extract_product_metadata,
    group_products,
    load_style_json,
    load_styles_rows,
    sizes_for_colors,
    unique_colors,
)  # noqa: E402
from variants import derive_sku, generate_variants  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract variant seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "006_demo_variants.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    variants: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        article = first.get("articleType", "").strip()
        price = ARTICLE_PRICE_MAP.get(article, 39.99)
        meta = extract_product_metadata(load_style_json(args.dataset, first.get("id", "").strip()))
        hs_code = meta.get("article_number")
        hs_code = hs_code[:20] if hs_code else None

        colors = unique_colors(group_rows)
        sizes_by_color = sizes_for_colors(args.dataset, group_rows)
        dims = variant_dimensions(article)
        cost = cost_price(price)

        for combo in generate_variants(display_name, colors, sizes_by_color):
            variant_id_str = (
                master_variant_id(display_name)
                if combo["is_master"]
                else variant_id(display_name, combo["color"] or "", combo["size"] or "")
            )
            option_values: list[dict] = []
            if combo["color"]:
                option_values.append({"option_type_id": OPTION_TYPE_COLOR_ID, "option_value_name": combo["color"]})
            if combo["size"]:
                option_values.append({"option_type_id": OPTION_TYPE_SIZE_ID, "option_value_name": combo["size"]})

            sku = derive_sku(display_name, combo["position"])
            variants.append({
                "id": variant_id_str,
                "product_id": product_id(display_name),
                "sku": sku,
                "is_master": combo["is_master"],
                "position": combo["position"],
                "price": price,
                "barcode": f"{sku}-BAR",
                "hs_code": hs_code,
                **dims,
                "cost_price": cost,
                "cost_currency": "USD",
                "option_values": option_values,
            })

    ensure_output_dir(args.output)
    write_json(out, variants)
    print(f"Written {len(variants)} variants to {out}")


if __name__ == "__main__":
    main()
