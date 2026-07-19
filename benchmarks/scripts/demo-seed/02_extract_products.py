#!/usr/bin/env python
"""Extract products, variants, and image metadata from styles.csv."""
from __future__ import annotations

import argparse
import csv
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import (  # noqa: E402
    OPTION_TYPE_COLOR_ID,
    OPTION_TYPE_SIZE_ID,
    SCRIPTS_DIR,
    check_overwrite,
    guid,
    write_json,
)

ARTICLE_PRICE_MAP: dict[str, float] = {
    "Tshirts": 24.99, "Shirts": 34.99, "Jeans": 59.99, "Trousers": 49.99,
    "Shorts": 29.99, "Track Pants": 39.99, "Sweatshirts": 44.99, "Sweaters": 49.99,
    "Jackets": 79.99, "Blazers": 89.99, "Suits": 149.99, "Kurtas": 39.99,
    "Kurtis": 29.99, "Tops": 24.99, "Dresses": 59.99, "Skirts": 34.99,
    "Casual Shoes": 69.99, "Sports Shoes": 89.99, "Formal Shoes": 79.99,
    "Heels": 59.99, "Flats": 34.99, "Sandals": 29.99, "Sneakers": 79.99,
    "Watches": 89.99, "Sunglasses": 39.99, "Belts": 24.99, "Wallets": 29.99,
    "Handbags": 59.99, "Backpacks": 49.99, "Ties": 19.99, "Scarves": 24.99,
    "Caps": 19.99, "Hats": 24.99, "Jewellery": 49.99, "Bracelet": 29.99,
    "Perfume and Body Mist": 34.99, "Lipstick": 14.99, "Nail Polish": 9.99,
    "Earrings": 24.99, "Necklace and Chains": 39.99, "Ring": 49.99,
    "Sarees": 69.99, "Lehenga": 99.99, "Dupatta": 19.99, "Salwar": 29.99,
    "Churidar": 29.99, "Leggings": 24.99, "Capris": 29.99,
}


def derive_sku(base: str, variant_index: int) -> str:
    safe = base.upper().replace(" ", "-").replace("'", "").replace("&", "AND")[:20]
    return f"{safe}-{variant_index:03d}"


def extract_sizes_from_json(dataset_path: Path, product_id: str) -> list[str]:
    json_path = dataset_path / "styles" / f"{product_id}.json"
    if not json_path.exists():
        return []
    try:
        data = json.loads(json_path.read_text())
        style_options = data.get("data", {}).get("styleOptions", [])
        sizes: list[str] = []
        for opt in style_options:
            size = (opt.get("sizeOption", {}) or {}).get("value", "")
            if size:
                sizes.append(str(size))
        return sorted(set(sizes))
    except Exception:
        return []


def extract_material_and_care(html: str | None) -> tuple[str | None, str | None]:
    if not html:
        return None, None
    cleaned = re.sub(r'<[^>]+>', ' ', html)
    cleaned = re.sub(r'\s+', ' ', cleaned).strip()
    material = None
    care = None
    if 'Wash Care' in cleaned:
        idx = cleaned.index('Wash Care')
        care = cleaned[idx:].strip()[:500]
    for keyword in ['Material', 'Fabric', 'Cotton', 'Polyester']:
        if keyword.lower() in cleaned.lower():
            material = cleaned[:200].strip()[:200]
            break
    return material or None, care


def extract_product_metadata(benchmark_id: str, dataset_path: Path) -> dict:
    json_path = dataset_path / "styles" / f"{benchmark_id}.json"
    result: dict = {
        "brand_name": None,
        "season": None,
        "material_composition": None,
        "care_instructions": None,
        "article_attributes": None,
        "style_images": None,
        "article_number": None,
    }
    if not json_path.exists():
        return result
    try:
        data = json.loads(json_path.read_text())
        d = data.get("data", {}) or {}
        result["brand_name"] = d.get("brandName")
        result["season"] = d.get("season")
        result["article_attributes"] = d.get("articleAttributes")
        result["style_images"] = d.get("styleImages")
        result["article_number"] = d.get("articleNumber")

        desc = d.get("productDescriptors", {}).get("description", {}).get("value", "")
        if desc:
            result["material_composition"], result["care_instructions"] = extract_material_and_care(desc)
    except Exception:
        pass
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract product seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--display-size", type=int, default=512)
    parser.add_argument("--search-size", type=int, default=224)
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    check_overwrite(args.output / "demo_products.json", args.force)

    styles_csv = args.dataset / "styles.csv"
    if not styles_csv.exists():
        print(f"ERROR: {styles_csv} not found")
        sys.exit(1)

    groups: dict[str, list[dict]] = defaultdict(list)
    with open(styles_csv, encoding="utf-8") as f:
        for row in csv.DictReader(f):
            name = row.get("productDisplayName", "").strip()
            if not name:
                continue
            groups[name].append(row)

    selected = dict(list(groups.items())[:args.count])

    products: list[dict] = []
    variants: list[dict] = []
    images: list[dict] = []
    assignments: list[dict] = []
    all_sizes: set[str] = set()
    product_taxon_refs: list[dict] = []  # for demo_classifications.json

    for _idx, (display_name, rows) in enumerate(selected.items()):
        product_id = guid("product", display_name)
        first = rows[0]
        article = first.get("articleType", "").strip()
        price = ARTICLE_PRICE_MAP.get(article, 39.99)
        benchmark_id = first.get("id", "").strip()
        meta = extract_product_metadata(benchmark_id, args.dataset)
        brand = first.get("brandName", "").strip()
        brand_initials = "".join([w[0] for w in brand.split() if w]).upper() if brand else "XX"
        style_code = f"{brand_initials}-{article[:10]}".upper()
        department = first.get("masterCategory", "").strip()

        slug = display_name.lower().replace(" ", "-").replace("'", "").replace("&", "and")[:200]
        slug = slug.rstrip("-")

        products.append({
            "id": product_id,
            "name": display_name[:255],
            "slug": slug[:255],
            "description": f"{display_name} — {article} by {brand or 'Unknown Brand'}"[:2000],
            "status": "Active",
            "gender_target": first.get("gender", "").strip() or "Unisex",
            "meta_title": display_name[:100],
            "meta_keywords": f"{article}, {brand}, {department}"[:255],
            "style_code": style_code[:100],
            "season_name": meta.get("season"),
            "department": department[:100] if department else None,
            "material_composition": meta.get("material_composition"),
            "care_instructions": meta.get("care_instructions"),
        })

        mc = first.get("masterCategory", "").strip()
        b = brand
        at = article
        pos = 0
        if mc:
            product_taxon_refs.append({
                "product_id": product_id,
                "taxon_id": guid("taxon", f"cat.{mc}"),
                "position": pos,
            })
            pos += 1
        if b:
            product_taxon_refs.append({
                "product_id": product_id,
                "taxon_id": guid("taxon", f"brand.{b}"),
                "position": pos,
            })
            pos += 1
        if at:
            product_taxon_refs.append({
                "product_id": product_id,
                "taxon_id": guid("taxon", f"article_type.{at}"),
                "position": pos,
            })
            pos += 1

        master_variant_id = None
        for vi, row in enumerate(rows):
            variant_id = guid("variant", f"{display_name}.{vi}")
            if vi == 0:
                master_variant_id = variant_id

            sku = derive_sku(display_name, vi)
            color = row.get("baseColour", "").strip()
            benchmark_id = row.get("id", "").strip()
            sizes = extract_sizes_from_json(args.dataset, benchmark_id)
            for s in sizes:
                all_sizes.add(s)

            hs_code_an = meta.get("article_number")
            hs_code = hs_code_an[:20] if hs_code_an else None
            variants.append({
                "id": variant_id,
                "product_id": product_id,
                "sku": sku,
                "is_master": vi == 0,
                "position": vi,
                "price": price,
                "hs_code": hs_code,
                "barcode": f"{sku}-BAR",
            })

            if color:
                assignments.append({
                    "variant_id": variant_id,
                    "option_value_name": color,
                    "option_type_id": OPTION_TYPE_COLOR_ID,
                })
            if sizes:
                for s in sizes:
                    assignments.append({
                        "variant_id": variant_id,
                        "option_value_name": s,
                        "option_type_id": OPTION_TYPE_SIZE_ID,
                    })

            default_img_id = guid("variant_image", f"{display_name}.{vi}.default")
            images.append({
                "id": default_img_id,
                "variant_id": variant_id,
                "content_type": "image/jpeg",
                "file_name": f"{benchmark_id}.jpg",
                "storage_path": f"images/medium/{benchmark_id}.jpg",
                "position": 0,
                "alt": display_name[:500],
                "type": "Default",
            })

            if vi == 0:
                search_img_id = guid("variant_image", f"{display_name}.{vi}.search")
                images.append({
                    "id": search_img_id,
                    "variant_id": variant_id,
                    "content_type": "image/jpeg",
                    "file_name": f"{benchmark_id}.jpg",
                    "storage_path": f"images/search/{args.search_size}/{benchmark_id}.jpg",
                    "position": 1,
                    "alt": display_name[:500],
                    "type": "Search",
                })

                gallery_labels = ["back", "front"]
                for gi, gl in enumerate(gallery_labels):
                    s_images = meta.get("style_images")
                    if s_images and gl in s_images:
                        gallery_img_id = guid("variant_image", f"{display_name}.{vi}.{gl}")
                        images.append({
                            "id": gallery_img_id,
                            "variant_id": variant_id,
                            "content_type": "image/jpeg",
                            "file_name": f"{benchmark_id}.jpg",
                            "storage_path": f"images/medium/{benchmark_id}.jpg",
                            "position": 2 + gi,
                            "alt": f"{display_name} ({gl} view)"[:500],
                            "type": "Gallery",
                        })

            products[-1]["master_variant_id"] = master_variant_id

    args.output.mkdir(parents=True, exist_ok=True)

    write_json(args.output / "demo_products.json", products)
    write_json(args.output / "demo_variants.json", variants)
    write_json(args.output / "demo_variant_images.json", images)
    write_json(args.output / "demo_option_assignments.json", assignments)
    write_json(args.output / "demo_classifications.json", product_taxon_refs)

    existing = json.loads((args.output / "demo_option_values.json").read_text()) if (args.output / "demo_option_values.json").exists() else []
    pos = len(existing)
    for size in sorted(all_sizes):
        if not any(v.get("name") == size and v.get("option_type_id") == OPTION_TYPE_SIZE_ID for v in existing):
            existing.append({
                "id": guid("option_value", f"size.{size}"),
                "option_type_id": OPTION_TYPE_SIZE_ID,
                "name": size, "presentation": size, "position": pos,
            })
            pos += 1
    write_json(args.output / "demo_option_values.json", existing)

    print(f"Written {len(products)} products, {len(variants)} variants, {len(images)} images, {len(assignments)} assignments, {len(product_taxon_refs)} classifications")


if __name__ == "__main__":
    main()
