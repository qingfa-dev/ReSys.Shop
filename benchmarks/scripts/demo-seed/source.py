"""Source dataset parsing: styles.csv + per-style JSON metadata."""
from __future__ import annotations

import csv
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))


def load_styles_rows(dataset_path: Path) -> list[dict]:
    styles_csv = dataset_path / "styles.csv"
    with open(styles_csv, encoding="utf-8") as f:
        return [row for row in csv.DictReader(f)]


def group_products(rows: list[dict]) -> list[tuple[str, list[dict]]]:
    groups: dict[str, list[dict]] = defaultdict(list)
    for row in rows:
        name = row.get("productDisplayName", "").strip()
        if name:
            groups[name].append(row)
    return list(groups.items())


def load_style_json(dataset_path: Path, benchmark_id: str) -> dict:
    json_path = dataset_path / "styles" / f"{benchmark_id}.json"
    if not json_path.exists():
        return {}
    try:
        return json.loads(json_path.read_text())
    except Exception:
        return {}


def extract_sizes(style_json: dict) -> list[str]:
    style_options = style_json.get("data", {}).get("styleOptions", [])
    sizes: list[str] = []
    for opt in style_options:
        size = opt.get("value", "")
        if opt.get("name") == "Size" and size:
            sizes.append(str(size))
    return sorted(set(sizes))


def extract_material_and_care(html: str | None) -> tuple[str | None, str | None]:
    if not html:
        return None, None
    cleaned = re.sub(r"<[^>]+>", " ", html)
    cleaned = re.sub(r"\s+", " ", cleaned).strip()
    care = None
    if "Wash Care" in cleaned:
        idx = cleaned.index("Wash Care")
        care = cleaned[idx:].strip()[:500]
    material = None
    for keyword in ["Material", "Fabric", "Cotton", "Polyester"]:
        if keyword.lower() in cleaned.lower():
            material = cleaned[:200].strip()[:200]
            break
    return material or None, care


def extract_product_metadata(style_json: dict) -> dict:
    result: dict = {
        "brand_name": None,
        "season": None,
        "material_composition": None,
        "care_instructions": None,
        "article_attributes": None,
        "style_images": None,
        "article_number": None,
    }
    data = style_json.get("data", {}) or {}
    result["brand_name"] = data.get("brandName")
    result["season"] = data.get("season")
    result["article_attributes"] = data.get("articleAttributes")
    result["style_images"] = data.get("styleImages")
    result["article_number"] = data.get("articleNumber")
    desc = data.get("productDescriptors", {}).get("description", {}).get("value", "")
    if desc:
        result["material_composition"], result["care_instructions"] = extract_material_and_care(desc)
    return result


def unique_colors(rows: list[dict]) -> list[str]:
    colors: list[str] = []
    for row in rows:
        color = row.get("baseColour", "").strip()
        if color and color not in colors:
            colors.append(color)
    return colors


def sizes_for_colors(dataset_path: Path, group_rows: list[dict]) -> dict[str, list[str]]:
    """Map each unique color of a product group to its sorted size list."""
    by_color: dict[str, list[str]] = {}
    for row in group_rows:
        color = row.get("baseColour", "").strip()
        if not color or color in by_color:
            continue
        style_json = load_style_json(dataset_path, row.get("id", "").strip())
        by_color[color] = extract_sizes(style_json)
    return by_color
