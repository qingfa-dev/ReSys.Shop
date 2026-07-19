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
