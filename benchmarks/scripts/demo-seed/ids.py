"""Deterministic entity ID formulas for demo seed datasets."""
from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import guid  # noqa: E402


def product_id(name: str) -> str:
    return guid("product", name)


def master_variant_id(product_name: str) -> str:
    # STABLE: must remain guid("variant", f"{product_name}.0") so existing
    # images/ and 012_demo_embeddings.json stay valid across regenerations.
    return guid("variant", f"{product_name}.0")


def variant_id(product_name: str, color: str, size: str) -> str:
    return guid("variant", f"{product_name}.{color}.{size}")


def taxon_id(identifier: str) -> str:
    # identifier is the FULL dotted form used by the old scripts, e.g.
    # "categories_root", "cat.Jeans", "cat.Jeans.Skinny", "brand.Levis",
    # "article_type.Tshirts", "brands_root", "article_types_root".
    # Must stay byte-identical to the old guid("taxon", identifier) calls.
    return guid("taxon", identifier)


def option_value_id(kind: str, name: str) -> str:
    return guid("option_value", f"{kind}.{name}")


def variant_image_id(product_name: str, suffix: str) -> str:
    return guid("variant_image", f"{product_name}.{suffix}")
