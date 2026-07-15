#!/usr/bin/env python
"""Extract taxonomies, taxons, and option types/values from styles.csv."""
from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from uuid import uuid5, NAMESPACE_DNS

SEED_NAMESPACE = uuid5(NAMESPACE_DNS, "resys.shop.demo-seed")

TAXONOMY_CATEGORIES_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.categories"))
TAXONOMY_BRANDS_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.brands"))
TAXONOMY_ARTICLE_TYPES_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.article_types"))

OPTION_TYPE_SIZE_ID = str(uuid5(SEED_NAMESPACE, "option_type.size"))
OPTION_TYPE_COLOR_ID = str(uuid5(SEED_NAMESPACE, "option_type.color"))


def guid(entity_type: str, name: str) -> str:
    return str(uuid5(SEED_NAMESPACE, f"{entity_type}.{name}"))


def build_taxonomies_json() -> list[dict]:
    return [
        {"id": TAXONOMY_CATEGORIES_ID, "name": "Categories", "presentation": "Departments", "position": 0},
        {"id": TAXONOMY_BRANDS_ID, "name": "Brands", "presentation": "Brands", "position": 1},
        {"id": TAXONOMY_ARTICLE_TYPES_ID, "name": "Article Types", "presentation": "Article Types", "position": 2},
    ]


def build_taxons_json(
    master_categories: set[str],
    sub_categories: dict[str, set[str]],
    brands: set[str],
    article_types: set[str],
) -> list[dict]:
    taxons: list[dict] = []
    lft = 1

    root_cat_id = guid("taxon", "categories_root")
    lft += 1  # skip root lft

    for master_cat in sorted(master_categories):
        mc_id = guid("taxon", f"cat.{master_cat}")
        mc_slug = master_cat.lower().replace(" ", "-").replace("&", "and")
        mc_lft = lft
        lft += 1
        for sub_cat in sorted(sub_categories.get(master_cat, set())):
            sc_id = guid("taxon", f"cat.{master_cat}.{sub_cat}")
            sc_slug = sub_cat.lower().replace(" ", "-").replace("&", "and")
            taxons.append({
                "id": sc_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
                "parent_id": mc_id, "name": sub_cat,
                "presentation": sub_cat, "slug": sc_slug,
                "depth": 2, "lft": lft, "rgt": lft + 1, "position": 0,
            })
            lft += 2
        mc_rgt = lft
        lft += 1
        taxons.append({
            "id": mc_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
            "parent_id": root_cat_id, "name": master_cat,
            "presentation": master_cat, "slug": mc_slug,
            "depth": 1, "lft": mc_lft, "rgt": mc_rgt, "position": 0,
        })

    root_rgt = lft
    lft += 1
    taxons.append({
        "id": root_cat_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
        "parent_id": None, "name": "Categories", "presentation": "All Categories",
        "slug": "categories", "depth": 0, "lft": 1, "rgt": root_rgt, "position": 0,
    })

    root_brand_id = guid("taxon", "brands_root")
    brand_lft = 2
    for brand in sorted(brands):
        b_id = guid("taxon", f"brand.{brand}")
        b_slug = brand.lower().replace(" ", "-").replace("&", "and").replace(",", "")
        taxons.append({
            "id": b_id, "taxonomy_id": TAXONOMY_BRANDS_ID,
            "parent_id": root_brand_id, "name": brand,
            "presentation": brand, "slug": b_slug,
            "depth": 1, "lft": brand_lft, "rgt": brand_lft + 1, "position": 0,
        })
        brand_lft += 2
    taxons.append({
        "id": root_brand_id, "taxonomy_id": TAXONOMY_BRANDS_ID,
        "parent_id": None, "name": "Brands", "presentation": "All Brands",
        "slug": "brands", "depth": 0, "lft": 1, "rgt": brand_lft, "position": 0,
    })

    root_at_id = guid("taxon", "article_types_root")
    at_lft = 2
    for atype in sorted(article_types):
        at_id = guid("taxon", f"article_type.{atype}")
        at_slug = atype.lower().replace(" ", "-").replace("&", "and")
        taxons.append({
            "id": at_id, "taxonomy_id": TAXONOMY_ARTICLE_TYPES_ID,
            "parent_id": root_at_id, "name": atype,
            "presentation": atype, "slug": at_slug,
            "depth": 1, "lft": at_lft, "rgt": at_lft + 1, "position": 0,
        })
        at_lft += 2
    taxons.append({
        "id": root_at_id, "taxonomy_id": TAXONOMY_ARTICLE_TYPES_ID,
        "parent_id": None, "name": "Article Types", "presentation": "All Article Types",
        "slug": "article-types", "depth": 0, "lft": 1, "rgt": at_lft, "position": 0,
    })

    return taxons


def build_option_types_json() -> list[dict]:
    return [
        {"id": OPTION_TYPE_SIZE_ID, "name": "Size", "presentation": "Size", "position": 0, "filterable": True},
        {"id": OPTION_TYPE_COLOR_ID, "name": "Color", "presentation": "Color", "position": 1, "filterable": True},
    ]


def build_option_values_json(colors: set[str]) -> list[dict]:
    values: list[dict] = []
    pos = 0
    for color in sorted(colors):
        values.append({
            "id": guid("option_value", f"color.{color}"),
            "option_type_id": OPTION_TYPE_COLOR_ID,
            "name": color, "presentation": color, "position": pos,
        })
        pos += 1
    return values
    # Size values are generated in extract_products.py from JSON articleAttributes


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract taxonomy seed data from styles.csv")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, required=True, help="Output directory for JSON files")
    args = parser.parse_args()

    styles_csv = args.dataset / "styles.csv"
    if not styles_csv.exists():
        print(f"ERROR: {styles_csv} not found"); sys.exit(1)

    master_categories: set[str] = set()
    sub_categories: dict[str, set[str]] = {}
    brands: set[str] = set()
    article_types: set[str] = set()
    colors: set[str] = set()

    with open(styles_csv, encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            mc = row.get("masterCategory", "").strip()
            sc = row.get("subCategory", "").strip()
            b = row.get("brandName", "").strip()
            at = row.get("articleType", "").strip()
            c = row.get("baseColour", "").strip()

            if mc:
                master_categories.add(mc)
                if mc not in sub_categories:
                    sub_categories[mc] = set()
                if sc:
                    sub_categories[mc].add(sc)
            if b:
                brands.add(b)
            if at:
                article_types.add(at)
            if c:
                colors.add(c)

    args.output.mkdir(parents=True, exist_ok=True)

    (args.output / "demo_taxonomies.json").write_text(
        json.dumps(build_taxonomies_json(), indent=2))
    (args.output / "demo_taxons.json").write_text(
        json.dumps(build_taxons_json(master_categories, sub_categories, brands, article_types), indent=2))
    (args.output / "demo_option_types.json").write_text(
        json.dumps(build_option_types_json(), indent=2))
    (args.output / "demo_option_values.json").write_text(
        json.dumps(build_option_values_json(colors), indent=2))

    print(f"Written taxonomies/taxons/options to {args.output}")


if __name__ == "__main__":
    main()
