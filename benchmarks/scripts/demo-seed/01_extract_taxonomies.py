#!/usr/bin/env python
"""Extract taxonomies, taxons, and option types/values from styles.csv."""
from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import (  # noqa: E402
    OPTION_TYPE_COLOR_ID,
    OPTION_TYPE_SIZE_ID,
    SCRIPTS_DIR,
    TAXONOMY_ARTICLE_TYPES_ID,
    TAXONOMY_BRANDS_ID,
    TAXONOMY_CATEGORIES_ID,
    check_overwrite,
    guid,
    write_json,
)


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
    used_slugs: set[str] = {"categories", "brands", "article-types"}

    def make_slug(name: str) -> str:
        slug = name.lower().replace(" ", "-").replace("&", "and").replace(",", "")
        original = slug
        i = 2
        while slug in used_slugs:
            slug = f"{original}-{i}"
            i += 1
        used_slugs.add(slug)
        return slug

    lft = 1

    root_cat_id = guid("taxon", "categories_root")
    lft += 1  # skip root lft

    for master_cat in sorted(master_categories):
        mc_id = guid("taxon", f"cat.{master_cat}")
        mc_slug = make_slug(master_cat)
        mc_lft = lft
        lft += 1
        for sub_cat in sorted(sub_categories.get(master_cat, set())):
            sc_id = guid("taxon", f"cat.{master_cat}.{sub_cat}")
            sc_slug = make_slug(sub_cat)
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
        b_slug = make_slug(brand)
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
        at_slug = make_slug(atype)
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
    for pos, color in enumerate(sorted(colors)):
        values.append({
            "id": guid("option_value", f"color.{color}"),
            "option_type_id": OPTION_TYPE_COLOR_ID,
            "name": color, "presentation": color, "position": pos,
        })
    return values
    # Size values are generated in extract_products.py from JSON articleAttributes


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract taxonomy seed data from styles.csv")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output", help="Output directory for JSON files")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    check_overwrite(args.output / "demo_taxonomies.json", args.force)

    styles_csv = args.dataset / "styles.csv"
    if not styles_csv.exists():
        print(f"ERROR: {styles_csv} not found")
        sys.exit(1)

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

    write_json(args.output / "demo_taxonomies.json", build_taxonomies_json())
    write_json(args.output / "demo_taxons.json",
               build_taxons_json(master_categories, sub_categories, brands, article_types))
    write_json(args.output / "demo_option_types.json", build_option_types_json())
    write_json(args.output / "demo_option_values.json", build_option_values_json(colors))

    print(f"Written taxonomies/taxons/options to {args.output}")


if __name__ == "__main__":
    main()
