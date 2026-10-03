#!/usr/bin/env python
"""Extract product entities from styles.csv (single entity: Product)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import master_variant_id, product_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import build_product_seo, extract_tags  # noqa: E402
from shared import SCRIPTS_DIR  # noqa: E402
from source import (  # noqa: E402
    extract_product_metadata,
    group_products,
    load_style_json,
    load_styles_rows,
)


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract product seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "005_demo_products.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    products: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        article = first.get("articleType", "").strip()
        benchmark_id = first.get("id", "").strip()
        meta = extract_product_metadata(load_style_json(args.dataset, benchmark_id))
        brand = first.get("brandName", "").strip()
        brand_initials = "".join([w[0] for w in brand.split() if w]).upper() if brand else "XX"
        style_code = f"{brand_initials}-{article[:10]}".upper()
        department = first.get("masterCategory", "").strip()
        gender_target = first.get("gender", "").strip() or "Unisex"
        tags = extract_tags(meta.get("article_attributes"))

        slug = display_name.lower().replace(" ", "-").replace("'", "").replace("&", "and")[:200]
        slug = slug.rstrip("-")

        seo = build_product_seo(display_name, article, brand, department, tags)

        products.append({
            "id": product_id(display_name),
            "name": display_name[:255],
            "slug": slug[:255],
            "description": f"{display_name} — {article} by {brand or 'Unknown Brand'}"[:2000],
            "status": "Active",
            "gender_target": gender_target,
            "meta_title": seo["meta_title"],
            "meta_description": seo["meta_description"],
            "meta_keywords": seo["meta_keywords"],
            "style_code": style_code[:100],
            "season_name": meta.get("season"),
            "department": department[:100] if department else None,
            "material_composition": meta.get("material_composition"),
            "care_instructions": meta.get("care_instructions"),
            "master_variant_id": master_variant_id(display_name),
        })

    ensure_output_dir(args.output)
    write_json(out, products)
    print(f"Written {len(products)} products to {out}")


if __name__ == "__main__":
    main()
