#!/usr/bin/env python
"""Extract products, variants, and image metadata from styles.csv."""
from __future__ import annotations

import argparse
import csv
import json
import sys
from collections import defaultdict
from pathlib import Path
from uuid import uuid5, NAMESPACE_DNS

SEED_NAMESPACE = uuid5(NAMESPACE_DNS, "resys.shop.demo-seed")
OPTION_TYPE_SIZE_ID = str(uuid5(SEED_NAMESPACE, "option_type.size"))
OPTION_TYPE_COLOR_ID = str(uuid5(SEED_NAMESPACE, "option_type.color"))

MODEL_INPUT_SIZES: dict[str, int] = {
    "efficientnet_b0": 224, "clip_vit_b16": 224, "fashion_clip": 224,
    "dinov2_vits14": 224,
}

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


def guid(entity_type: str, name: str) -> str:
    return str(uuid5(SEED_NAMESPACE, f"{entity_type}.{name}"))


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


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract product seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--count", type=int, default=200, help="Target number of product groups")
    args = parser.parse_args()

    styles_csv = args.dataset / "styles.csv"
    if not styles_csv.exists():
        print(f"ERROR: {styles_csv} not found"); sys.exit(1)

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

    for idx, (display_name, rows) in enumerate(selected.items()):
        product_id = guid("product", display_name)
        first = rows[0]
        article = first.get("articleType", "").strip()
        price = ARTICLE_PRICE_MAP.get(article, 39.99)

        slug = display_name.lower().replace(" ", "-").replace("'", "").replace("&", "and")[:200]
        slug = slug.rstrip("-")

        products.append({
            "id": product_id,
            "name": display_name[:255],
            "slug": slug[:255],
            "description": f"{display_name} — {article} by {first.get('brandName', '').strip() or 'Unknown Brand'}"[:2000],
            "status": "Active",
            "gender_target": first.get("gender", "").strip() or "Unisex",
            "meta_title": display_name[:100],
            "meta_keywords": f"{article}, {first.get('brandName', '').strip()}, {first.get('masterCategory', '').strip()}"[:255],
        })

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

            variants.append({
                "id": variant_id,
                "product_id": product_id,
                "sku": sku,
                "is_master": vi == 0,
                "position": vi,
                "price": price,
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

            search_img_id = guid("variant_image", f"{display_name}.{vi}.search")
            images.append({
                "id": search_img_id,
                "variant_id": variant_id,
                "content_type": "image/jpeg",
                "file_name": f"{benchmark_id}.jpg",
                "storage_path": f"images/search/224/{benchmark_id}.jpg",
                "position": 1,
                "alt": display_name[:500],
                "type": "Search",
            })

            products[-1]["master_variant_id"] = master_variant_id

    args.output.mkdir(parents=True, exist_ok=True)

    (args.output / "demo_products.json").write_text(json.dumps(products, indent=2))
    (args.output / "demo_variants.json").write_text(json.dumps(variants, indent=2))
    (args.output / "demo_variant_images.json").write_text(json.dumps(images, indent=2))
    (args.output / "demo_option_assignments.json").write_text(json.dumps(assignments, indent=2))

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
    (args.output / "demo_option_values.json").write_text(json.dumps(existing, indent=2))

    print(f"Written {len(products)} products, {len(variants)} variants, {len(images)} images, {len(assignments)} assignments")


if __name__ == "__main__":
    main()
