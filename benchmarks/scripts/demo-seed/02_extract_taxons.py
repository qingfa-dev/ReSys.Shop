#!/usr/bin/env python
"""Extract taxon entities from styles.csv (single entity: Taxon)."""
from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import taxon_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import build_taxon_seo  # noqa: E402
from shared import (  # noqa: E402
    SCRIPTS_DIR,
    TAXONOMY_ARTICLE_TYPES_ID,
    TAXONOMY_BRANDS_ID,
    TAXONOMY_CATEGORIES_ID,
)


def build_taxons_json(
    master_categories: set[str],
    sub_categories: dict[str, set[str]],
    brands: set[str],
    article_types: set[str],
) -> list[dict]:
    taxons: list[dict] = []
    cat_slugs: set[str] = {"categories"}
    brand_slugs: set[str] = {"brands"}
    at_slugs: set[str] = {"article-types"}

    def make_slug(name: str, used_slugs: set[str]) -> str:
        slug = name.lower().replace(" ", "-").replace("&", "and").replace(",", "")
        original = slug
        i = 2
        while slug in used_slugs:
            slug = f"{original}-{i}"
            i += 1
        used_slugs.add(slug)
        return slug

    lft = 1
    root_cat_id = taxon_id("categories_root")
    lft += 1

    for master_cat in sorted(master_categories):
        mc_id = taxon_id(f"cat.{master_cat}")
        mc_lft = lft
        lft += 1
        mc_slug = make_slug(master_cat, cat_slugs)
        for sub_cat in sorted(sub_categories.get(master_cat, set())):
            sc_id = taxon_id(f"cat.{master_cat}.{sub_cat}")
            taxons.append({
                "id": sc_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
                "parent_id": mc_id, "name": sub_cat,
                "presentation": sub_cat, "slug": make_slug(sub_cat, cat_slugs),
                "depth": 2, "lft": lft, "rgt": lft + 1, "position": 0,
                **build_taxon_seo(sub_cat, "Categories"),
            })
            lft += 2
        mc_rgt = lft
        lft += 1
        taxons.append({
            "id": mc_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
            "parent_id": root_cat_id, "name": master_cat,
            "presentation": master_cat, "slug": mc_slug,
            "depth": 1, "lft": mc_lft, "rgt": mc_rgt, "position": 0,
            **build_taxon_seo(master_cat, "Categories"),
        })

    root_rgt = lft
    lft += 1
    taxons.append({
        "id": root_cat_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
        "parent_id": None, "name": "Categories", "presentation": "All Categories",
        "slug": "categories", "depth": 0, "lft": 1, "rgt": root_rgt, "position": 0,
        **build_taxon_seo("Categories", "Categories"),
    })

    root_brand_id = taxon_id("brands_root")
    brand_lft = 2
    for brand in sorted(brands):
        b_id = taxon_id(f"brand.{brand}")
        taxons.append({
            "id": b_id, "taxonomy_id": TAXONOMY_BRANDS_ID,
            "parent_id": root_brand_id, "name": brand,
            "presentation": brand, "slug": make_slug(brand, brand_slugs),
            "depth": 1, "lft": brand_lft, "rgt": brand_lft + 1, "position": 0,
            **build_taxon_seo(brand, "Brands"),
        })
        brand_lft += 2
    taxons.append({
        "id": root_brand_id, "taxonomy_id": TAXONOMY_BRANDS_ID,
        "parent_id": None, "name": "Brands", "presentation": "All Brands",
        "slug": "brands", "depth": 0, "lft": 1, "rgt": brand_lft, "position": 0,
        **build_taxon_seo("Brands", "Brands"),
    })

    root_at_id = taxon_id("article_types_root")
    at_lft = 2
    for atype in sorted(article_types):
        at_id = taxon_id(f"article_type.{atype}")
        taxons.append({
            "id": at_id, "taxonomy_id": TAXONOMY_ARTICLE_TYPES_ID,
            "parent_id": root_at_id, "name": atype,
            "presentation": atype, "slug": make_slug(atype, at_slugs),
            "depth": 1, "lft": at_lft, "rgt": at_lft + 1, "position": 0,
            **build_taxon_seo(atype, "Article Types"),
        })
        at_lft += 2
    taxons.append({
        "id": root_at_id, "taxonomy_id": TAXONOMY_ARTICLE_TYPES_ID,
        "parent_id": None, "name": "Article Types", "presentation": "All Article Types",
        "slug": "article-types", "depth": 0, "lft": 1, "rgt": at_lft, "position": 0,
        **build_taxon_seo("Article Types", "Article Types"),
    })

    return taxons


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract taxon seed data")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "002_demo_taxons.json"
    check_overwrite(out, args.force)

    styles_csv = args.dataset / "styles.csv"
    if not styles_csv.exists():
        print(f"ERROR: {styles_csv} not found")
        sys.exit(1)

    master_categories: set[str] = set()
    sub_categories: dict[str, set[str]] = {}
    brands: set[str] = set()
    article_types: set[str] = set()

    with open(styles_csv, encoding="utf-8") as f:
        for row in csv.DictReader(f):
            mc = row.get("masterCategory", "").strip()
            sc = row.get("subCategory", "").strip()
            b = row.get("brandName", "").strip()
            at = row.get("articleType", "").strip()
            if mc:
                master_categories.add(mc)
                sub_categories.setdefault(mc, set())
                if sc:
                    sub_categories[mc].add(sc)
            if b:
                brands.add(b)
            if at:
                article_types.add(at)

    ensure_output_dir(args.output)
    write_json(out, build_taxons_json(master_categories, sub_categories, brands, article_types))
    print(f"Written {len(master_categories)} master categories, {len(brands)} brands, {len(article_types)} article types to {out}")


if __name__ == "__main__":
    main()
