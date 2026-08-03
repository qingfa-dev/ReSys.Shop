"""Shared constants and helpers for demo-seed ETL scripts."""
from __future__ import annotations

import json
import sys
from pathlib import Path
from uuid import NAMESPACE_DNS, uuid5

SEED_NAMESPACE = uuid5(NAMESPACE_DNS, "resys.shop.demo-seed")

TAXONOMY_CATEGORIES_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.categories"))
TAXONOMY_BRANDS_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.brands"))
TAXONOMY_ARTICLE_TYPES_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.article_types"))

OPTION_TYPE_SIZE_ID = str(uuid5(SEED_NAMESPACE, "option_type.size"))
OPTION_TYPE_COLOR_ID = str(uuid5(SEED_NAMESPACE, "option_type.color"))

MODEL_INPUT_SIZES: dict[str, int] = {
    "efficientnet_b0": 224, "clip_vit_b16": 224, "fashion_clip": 224,
    "dinov2_vits14": 224,
}

SCRIPTS_DIR = Path(__file__).resolve().parent

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


def check_overwrite(path: Path, force: bool) -> None:
    """Exit if output file exists and --force not set."""
    if path.exists() and not force:
        print(f"Output already exists: {path}")
        print("Use --force to overwrite.")
        sys.exit(1)


def write_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, indent=2))
