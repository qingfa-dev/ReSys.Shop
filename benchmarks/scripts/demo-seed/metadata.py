"""Enriched metadata extractors mapped onto existing domain columns."""
from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))


def _slugify(name: str) -> str:
    return name.lower().replace(" ", "-").replace("&", "and").replace("'", "").replace(",", "").strip("-")


def build_taxon_seo(name: str, taxonomy_name: str) -> dict:
    slug = _slugify(name)
    return {
        "description": f"Shop {name} in our {taxonomy_name.lower()} collection.",
        "meta_title": f"{name} — {taxonomy_name}",
        "meta_description": f"Browse {name} across the {taxonomy_name.lower()} taxonomy.",
        "meta_keywords": f"{name}, {taxonomy_name}",
        "permalink": f"/{slug}",
        "pretty_name": name,
    }


def build_product_seo(name: str, article: str, brand: str, department: str, tags: list[str]) -> dict:
    keywords = ", ".join(dict.fromkeys([a for a in (article, brand, department) if a] + tags))
    return {
        "meta_title": name[:100],
        "meta_description": f"{name} — {article} by {brand or 'Unknown Brand'}"[:2000],
        "meta_keywords": keywords[:255],
    }


_WEIGHT_BY_ARTICLE: dict[str, float] = {
    "Casual Shoes": 1.0, "Sports Shoes": 1.0, "Formal Shoes": 1.0,
    "Heels": 0.8, "Flats": 0.6, "Sandals": 0.6, "Sneakers": 1.0,
}


def variant_dimensions(article_type: str) -> dict:
    weight = _WEIGHT_BY_ARTICLE.get(article_type, 0.3)
    return {
        "weight": weight,
        "weight_unit": "Kg",
        "height": 30.0 if weight >= 0.6 else 25.0,
        "width": 20.0,
        "depth": 5.0 if weight < 0.6 else 12.0,
        "dimensions_unit": "Cm",
    }


def cost_price(price: float) -> float:
    return round(price * 0.5, 2)


def build_image_alt(product_name: str, image_type: str) -> str:
    return f"{product_name} ({image_type} view)"[:500]


def extract_tags(article_attributes: dict | None) -> list[str]:
    if not article_attributes:
        return []
    seen: list[str] = []
    for key in ("fit", "fabric", "pattern", "sleeveLength", "occasion"):
        value = str(article_attributes.get(key, "")).strip()
        if value and value not in seen:
            seen.append(value)
    return seen[:3]
