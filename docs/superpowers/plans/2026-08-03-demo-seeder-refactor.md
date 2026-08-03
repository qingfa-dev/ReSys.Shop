# Demo Seeder Refactor — Domain-Driven Single-Responsibility Pipeline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor the demo data pipeline (Python ETL in `benchmarks/scripts/demo-seed/` + C# seeders in `service/Api/src/Module/`) into a domain-driven, single-responsibility structure: one entity per script/seeder, numbered `{NNN}_demo_{entity}.json` datasets, a shared deterministic core, ≤10 variants per product with the master using the first color×size combination, and enriched metadata mapped onto existing columns.

**Architecture:** Source-driven shared-core. `shared.py` keeps constants + `guid()`; new sibling modules `json_io.py`, `source.py`, `ids.py`, `metadata.py`, `variants.py` form the domain core. Each entity script is a thin writer that re-parses the source dataset (or a prior artifact for stock/images/embeddings) and writes exactly one numbered JSON file; `run_all.py` orchestrates in dependency order. On the C# side, `Option.Seeder` and `Product.Seeder` split into 2 + 4 seeder classes, each reading one numbered file; `LoadIfExists` filenames updated everywhere; enriched fields passed through to existing domain methods.

**Tech Stack:** Python 3.12 (argparse, csv, json, uuid5), Ruff, pytest. C# (.NET 10): `AbstractDataSeeder`, `DemoJsonHelper.LoadIfExists<T>`, EF Core `IApplicationDbContext`.

**Spec:** `docs/superpowers/specs/2026-08-03-demo-seeder-refactor-design.md`

## Global Constraints

- Python scripts live in `benchmarks/scripts/demo-seed/`; tests go in `benchmarks/scripts/demo-seed/tests/` (repo `testpaths` is `["src/tests"]`, so run them explicitly: `uv run pytest scripts/demo-seed/tests/`).
- Ruff on the demo-seed dir: `uv run ruff check scripts/demo-seed/` (rules E, F, I, UP, B, SIM per benchmarks/AGENTS.md). `uv run pytest scripts/demo-seed/tests/` must pass.
- Python imports stay script-local: `sys.path.insert(0, str(Path(__file__).resolve().parent))` + `from <module> import ...` (existing pattern; no `benchmark.` prefix — these are scripts, not the benchmark package).
- Deterministic IDs ONLY via `guid()` (uuid5 with `SEED_NAMESPACE`). Never `random`/`uuid4` in scripts 01-08. Script 09 stock keeps its existing seeded `random.seed(42)`.
- The master-variant ID formula `guid("variant", f"{display_name}.0")` and master image IDs MUST stay byte-identical to today so existing `images/` + `demo_embeddings.json` remain valid.
- C#: `dotnet build` must pass with zero warnings (TreatWarningsAsErrors). Seeder `Order` values: OptionType 100, OptionValue 105, Taxonomy 110, Taxon 120, Product 130, Variant 132, VariantImage 134, ProductTaxon 136, Embedding 137, StockLocation 100, StockItem 140, StockMovement 150.
- Old unnumbered JSON files are deleted ONLY in the final task, after the C# seeders read the new names.
- Commit only the files listed in each task. Commit message format: `type(scope): description` (e.g. `refactor(seed): split taxonomy extractor into per-entity scripts`).

## File Structure Map

```
benchmarks/scripts/demo-seed/
├── shared.py                [MODIFY] constants + guid() only (json helpers move to json_io.py in Task 4)
├── json_io.py               [CREATE] write_json / check_overwrite / load_json (Task 1)
├── source.py                [CREATE] CSV + styles JSON parsing, lazy index (Task 1)
├── ids.py                   [CREATE] deterministic entity ID formulas (Task 1)
├── metadata.py              [CREATE] SEO/branding/dimensions/tags/media extractors (Task 1)
├── variants.py              [CREATE] color×size combo generator, 10-cap, master=first (Task 1)
├── tests/
│   ├── conftest.py          [CREATE] sys.path bootstrap (Task 1)
│   ├── test_variants.py     [CREATE] combo/cap/master/invariant tests (Task 1)
│   ├── test_ids.py          [CREATE] ID stability tests (Task 1)
│   └── test_metadata.py     [CREATE] enrichment extractor tests (Task 1)
├── 01_extract_taxonomies.py [REWRITE] 001_demo_taxonomies.json only (Task 2)
├── 02_extract_taxons.py     [CREATE] 002_demo_taxons.json + SEO enrichment (Task 2)
├── 03_extract_option_types.py [CREATE] 003_demo_option_types.json (Task 2)
├── 04_extract_option_values.py [CREATE] 004_demo_option_values.json (colors + sizes) (Task 2)
├── 05_extract_products.py   [REWRITE] 005_demo_products.json + SEO enrichment (Task 3)
├── 06_extract_variants.py   [CREATE] 006_demo_variants.json, embedded option_values (Task 3)
├── 07_extract_variant_images.py [CREATE] 007_demo_variant_images.json + alt enrichment (Task 3)
├── 08_extract_product_taxons.py [CREATE] 008_demo_product_taxons.json (Task 3)
├── 09_extract_stock.py      [RENAME from 05_extract_stock.py] 009/010/011_demo_stock_*.json (Task 4)
├── 10_process_images.py     [RENAME from 03_process_images.py] images/ + backfill 007 dims/size (Task 4)
├── 11_generate_embeddings.py [RENAME from 04_generate_embeddings.py] 012_demo_embeddings.json (Task 4)
├── 12_verify_output.py      [RENAME from 06_verify_output.py] invariants + FK + ID consistency (Task 4)
└── run_all.py               [MODIFY] new step order (Task 4)

service/Api/src/Module/Catalog/Persistence/Seeders/
├── Option.Seeder.cs         [DELETE] split into two classes (Task 5)
├── OptionType.Seeder.cs     [CREATE] CatalogOptionTypeSeeder, Order 100 (Task 5)
├── OptionValue.Seeder.cs    [CREATE] CatalogOptionValueSeeder, Order 105 (Task 5)
├── Product.Seeder.cs        [DELETE] split into four classes (Task 5)
├── Product.Seeder.cs→Product.Seeder.cs  [CREATE] CatalogProductSeeder, Order 130 (Task 5)
├── Variant.Seeder.cs        [CREATE] CatalogVariantSeeder, Order 132 (Task 5)
├── VariantImage.Seeder.cs   [CREATE] CatalogVariantImageSeeder, Order 134 (Task 5)
├── ProductTaxon.Seeder.cs   [CREATE] CatalogProductTaxonSeeder, Order 136 (Task 5)
├── Taxonomy.Seeder.cs       [MODIFY] filename → 001_demo_taxonomies.json (Task 5)
├── Taxon.Seeder.cs          [MODIFY] filename → 002_demo_taxons.json + enrichment passthrough (Task 5)
├── Embedding.Seeder.cs      [MODIFY] filename → 012_demo_embeddings.json, Order → 137 (Task 5)
└── Catalog.Extension.cs     [MODIFY] registration of new seeder classes (Task 5)

service/Api/src/Module/Inventory/Persistence/Seeders/
├── StockLocation.Seeder.cs  [MODIFY] filename → 009_demo_stock_locations.json (Task 6)
├── InventoryStockItem.Seeder.cs  [MODIFY] filename → 010_demo_stock_items.json (Task 6)
└── InventoryStockMovement.Seeder.cs [MODIFY] filename → 011_demo_stock_movements.json (Task 6)
```

---

### Task 1: Python domain core modules + unit tests

**Files:**
- Create: `benchmarks/scripts/demo-seed/json_io.py`
- Create: `benchmarks/scripts/demo-seed/source.py`
- Create: `benchmarks/scripts/demo-seed/ids.py`
- Create: `benchmarks/scripts/demo-seed/metadata.py`
- Create: `benchmarks/scripts/demo-seed/variants.py`
- Create: `benchmarks/scripts/demo-seed/tests/conftest.py`
- Create: `benchmarks/scripts/demo-seed/tests/test_variants.py`
- Create: `benchmarks/scripts/demo-seed/tests/test_ids.py`
- Create: `benchmarks/scripts/demo-seed/tests/test_metadata.py`

**Interfaces:**
- Consumes: `shared.py` constants (`SEED_NAMESPACE`, `OPTION_TYPE_COLOR_ID`, `OPTION_TYPE_SIZE_ID`, `SCRIPTS_DIR`, `guid`).
- Produces (used by Tasks 2-4 scripts):
  - `json_io.write_json(path, data)`, `json_io.check_overwrite(path, force)`, `json_io.load_json(path) -> list[dict]`
  - `source.load_styles_rows(dataset_path) -> list[dict]` (styles.csv rows)
  - `source.group_products(rows) -> list[tuple[str, list[dict]]]` (display_name → rows, CSV order)
  - `source.extract_sizes(style_json) -> list[str]` (sorted, deduped)
  - `source.load_style_json(dataset_path, benchmark_id) -> dict`
  - `source.extract_material_and_care(html) -> tuple[str | None, str | None]`
  - `source.extract_product_metadata(style_json) -> dict` (keys: brand_name, season, material_composition, care_instructions, article_attributes, style_images, article_number)
  - `ids.master_variant_id(product_name) -> str`  == `guid("variant", f"{product_name}.0")` (MUST be stable)
  - `ids.variant_id(product_name, color, size) -> str` == `guid("variant", f"{product_name}.{color}.{size}")`
  - `ids.product_id(name) -> str`, `ids.taxon_id(identifier) -> str` == `guid("taxon", identifier)` (identifier is the full dotted form: `"categories_root"`, `"cat.Jeans"`, `"cat.Jeans.Skinny"`, `"brand.Levis"`, `"article_type.Tshirts"`, `"brands_root"`, `"article_types_root"` — MUST reproduce old IDs), `ids.option_value_id(kind, name) -> str` == `guid("option_value", f"{kind}.{name}")`, `ids.variant_image_id(product_name, suffix) -> str`
  - `metadata.build_taxon_seo(name, taxonomy_name) -> dict` (description, meta_title, meta_description, meta_keywords, permalink, pretty_name)
  - `metadata.build_product_seo(name, article, brand, department, tags) -> dict` (meta_title, meta_description, meta_keywords)
  - `metadata.variant_dimensions(article_type) -> dict` (weight, weight_unit, height, width, depth, dimensions_unit)
  - `metadata.cost_price(price) -> float` (≈ 50% of price, 2dp)
  - `metadata.build_image_alt(product_name, image_type) -> str`
  - `metadata.extract_tags(article_attributes) -> list[str]` (top 3 attribute values)
  - `variants.generate_variants(product_name, colors, sizes_by_color, max_variants=10) -> list[dict]` — each dict: `{"color", "size" | None, "is_master", "position"}`; size-major order; master = first combo; no duplicate child; cap.

- [ ] **Step 1: Write the failing unit tests**

`benchmarks/scripts/demo-seed/tests/conftest.py`:

```python
from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
```

`benchmarks/scripts/demo-seed/tests/test_variants.py`:

```python
from variants import generate_variants


def test_master_is_first_combo_and_child_combo_not_repeated():
    combos = generate_variants("Shirt", ["Red", "Blue"], {"Red": ["S", "M"], "Blue": ["L"]})
    masters = [c for c in combos if c["is_master"]]
    assert len(masters) == 1
    assert masters[0] == {"color": "Red", "size": "S", "is_master": True, "position": 0}
    child_combos = {(c["color"], c["size"]) for c in combos if not c["is_master"]}
    assert ("Red", "S") not in child_combos
    assert ("Red", "M") in child_combos
    assert ("Blue", "L") in child_combos


def test_size_major_ordering():
    combos = generate_variants("Shirt", ["Red", "Blue"], {"Red": ["S", "M"], "Blue": ["S"]})
    assert [(c["color"], c["size"]) for c in combos] == [
        ("Red", "S"), ("Red", "M"), ("Blue", "S"),
    ]


def test_cap_at_ten_variants():
    colors = [f"C{i}" for i in range(4)]
    sizes_by_color = {c: [f"S{j}" for j in range(5)] for c in colors}
    combos = generate_variants("Product", colors, sizes_by_color)
    assert len(combos) == 10
    assert combos[0]["is_master"]
    assert combos[0]["position"] == 0
    assert [c["position"] for c in combos] == list(range(10))


def test_no_sizes_color_only_master_and_children():
    combos = generate_variants("Perfume", ["Gold", "Silver"], {"Gold": [], "Silver": []})
    assert [(c["color"], c["size"], c["is_master"]) for c in combos] == [
        ("Gold", None, True), ("Silver", None, False),
    ]


def test_no_color_no_sizes_master_only():
    combos = generate_variants("Mystery", [], {})
    assert combos == [{"color": None, "size": None, "is_master": True, "position": 0}]


def test_every_variant_has_at_most_one_value_per_type():
    combos = generate_variants("Shirt", ["Red", "Blue"], {"Red": ["S", "M", "L"], "Blue": ["S"]})
    for c in combos:
        assert c["color"] is None or c["size"] is None or c["color"] != c["size"]
        assert sum(v is not None for v in (c["color"], c["size"])) <= 2
```

`benchmarks/scripts/demo-seed/tests/test_ids.py`:

```python
from ids import master_variant_id, option_value_id, product_id, taxon_id, variant_id, variant_image_id


def test_master_variant_id_is_stable():
    assert master_variant_id("Striped Shirt") == "11b775bd-cf45-5d6d-8361-4975a6e406ea"


def test_variant_id_is_deterministic():
    a = variant_id("Striped Shirt", "Red", "40")
    b = variant_id("Striped Shirt", "Red", "40")
    assert a == b
    assert a != variant_id("Striped Shirt", "Blue", "40")


def test_entity_ids_are_distinct_across_kinds():
    names = {product_id("X"), taxon_id("cat.X"), option_value_id("color", "X"), variant_image_id("X", "0.default")}
    assert len(names) == 4
```

`benchmarks/scripts/demo-seed/tests/test_metadata.py`:

```python
from metadata import build_image_alt, build_product_seo, build_taxon_seo, cost_price, extract_tags, variant_dimensions


def test_taxon_seo_fields():
    seo = build_taxon_seo("Jeans", "Categories")
    assert seo["pretty_name"] == "Jeans"
    assert seo["permalink"].endswith("jeans")
    assert "Jeans" in seo["meta_title"]
    assert seo["description"]


def test_product_seo_contains_brand_and_article():
    seo = build_product_seo("Slim Jeans", "Jeans", "Levis", "Apparel", ["Denim", "Slim"])
    assert "Levis" in seo["meta_keywords"]
    assert "Denim" in seo["meta_keywords"]
    assert seo["meta_description"]
    assert seo["meta_description"] != "Slim Jeans"


def test_variant_dimensions_defaults_by_article():
    assert variant_dimensions("Jeans")["weight"] == 0.3
    assert variant_dimensions("Casual Shoes")["weight"] == 1.0
    assert variant_dimensions("Unknown")["weight_unit"] == "Kg"


def test_cost_price_is_half_of_price():
    assert cost_price(24.98) == 12.49
    assert cost_price(0) == 0


def test_image_alt_mentions_product():
    alt = build_image_alt("Slim Jeans", "Default")
    assert "Slim Jeans" in alt
    assert "Default" in alt


def test_extract_tags_takes_top_three_values():
    attrs = {"fit": "Slim", "fabric": "Denim", "pattern": "Solid", "occasion": "Casual", "extra": "x"}
    assert sorted(extract_tags(attrs)) == ["Denim", "Slim", "Solid"]
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd benchmarks && uv run pytest scripts/demo-seed/tests/ -q`
Expected: FAIL with `ModuleNotFoundError: No module named 'variants'` etc.

- [ ] **Step 3: Write the domain core modules**

`benchmarks/scripts/demo-seed/json_io.py`:

```python
"""JSON read/write helpers for demo seed datasets."""
from __future__ import annotations

import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import SCRIPTS_DIR  # noqa: E402


def check_overwrite(path: Path, force: bool) -> None:
    """Exit if output file exists and --force not set."""
    if path.exists() and not force:
        print(f"Output already exists: {path}")
        print("Use --force to overwrite.")
        sys.exit(1)


def write_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, indent=2))


def load_json(path: Path) -> list[dict]:
    return json.loads(path.read_text())


def ensure_output_dir(path: Path) -> None:
    path.mkdir(parents=True, exist_ok=True)


if __name__ == "__main__":  # pragma: no cover
    pass
```

Wait — `json_io` needs `SCRIPTS_DIR` only as a convenience; drop it to avoid the unused import (Ruff F401). Final version:

```python
"""JSON read/write helpers for demo seed datasets."""
from __future__ import annotations

import json
import sys
from pathlib import Path


def check_overwrite(path: Path, force: bool) -> None:
    """Exit if output file exists and --force not set."""
    if path.exists() and not force:
        print(f"Output already exists: {path}")
        print("Use --force to overwrite.")
        sys.exit(1)


def write_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, indent=2))


def load_json(path: Path) -> list[dict]:
    return json.loads(path.read_text())


def ensure_output_dir(path: Path) -> None:
    path.mkdir(parents=True, exist_ok=True)
```

`benchmarks/scripts/demo-seed/ids.py`:

```python
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
    # images/ and demo_embeddings.json stay valid across regenerations.
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
```

`benchmarks/scripts/demo-seed/source.py`:

```python
"""Source dataset parsing: styles.csv + per-style JSON metadata."""
from __future__ import annotations

import csv
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import OPTION_TYPE_COLOR_ID, OPTION_TYPE_SIZE_ID  # noqa: E402

# (kept for the option_values extractor)


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
```

`benchmarks/scripts/demo-seed/metadata.py`:

```python
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
```

`benchmarks/scripts/demo-seed/variants.py`:

```python
"""Variant combination generator: one value per option type, capped."""
from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

MAX_VARIANTS_PER_PRODUCT = 10


def generate_variants(
    product_name: str,
    colors: list[str],
    sizes_by_color: dict[str, list[str]],
    max_variants: int = MAX_VARIANTS_PER_PRODUCT,
) -> list[dict]:
    """Generate color×size combinations in size-major order.

    The master variant is the first combination; no duplicate child is
    created for it. Returns at most ``max_variants`` entries.
    ``product_name`` is reserved for future deterministic ID coupling.
    """
    combos: list[tuple[str | None, str | None]] = []
    for color in colors:
        sizes = sorted(sizes_by_color.get(color, []) or [])
        if sizes:
            for size in sizes:
                combos.append((color, size))
        else:
            combos.append((color, None))
    if not combos:
        combos.append((None, None))

    selected = combos[:max_variants]
    return [
        {
            "color": color,
            "size": size,
            "is_master": i == 0,
            "position": i,
        }
        for i, (color, size) in enumerate(selected)
    ]
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd benchmarks && uv run pytest scripts/demo-seed/tests/ -q`
Expected: PASS (all tests green)

- [ ] **Step 5: Lint and commit**

Run: `cd benchmarks && uv run ruff check scripts/demo-seed/`
Expected: no errors

```bash
git add benchmarks/scripts/demo-seed/json_io.py benchmarks/scripts/demo-seed/source.py benchmarks/scripts/demo-seed/ids.py benchmarks/scripts/demo-seed/metadata.py benchmarks/scripts/demo-seed/variants.py benchmarks/scripts/demo-seed/tests/
git commit -m "feat(seed): add domain core modules for demo seed ETL"
```

---
### Task 2: Split taxonomy extractor into per-entity scripts (Taxonomy, Taxon, OptionType, OptionValue)

**Files:**
- Rewrite: `benchmarks/scripts/demo-seed/01_extract_taxonomies.py` → writes only `001_demo_taxonomies.json`
- Create: `benchmarks/scripts/demo-seed/02_extract_taxons.py` → writes `002_demo_taxons.json` (+ SEO enrichment)
- Create: `benchmarks/scripts/demo-seed/03_extract_option_types.py` → writes `003_demo_option_types.json`
- Create: `benchmarks/scripts/demo-seed/04_extract_option_values.py` → writes `004_demo_option_values.json` (colors + sizes)

**Interfaces:**
- Consumes: `json_io` (`check_overwrite`, `write_json`, `ensure_output_dir`, `load_json`), `shared` constants, `source` (`load_styles_rows`, `group_products`, `load_style_json`, `extract_sizes`, `unique_colors`), `ids` (`taxon_id`, `option_value_id`), `metadata` (`build_taxon_seo`).
- Produces: `001_demo_taxonomies.json`, `002_demo_taxons.json`, `003_demo_option_types.json`, `004_demo_option_values.json` — consumed by C# seeders in Task 5 and the orchestrator in Task 4.

- [ ] **Step 1: Rewrite `01_extract_taxonomies.py` (single entity: Taxonomy)**

Replace the entire file:

```python
#!/usr/bin/env python
"""Extract taxonomy entities from styles.csv (single entity: Taxonomy)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import (  # noqa: E402
    SCRIPTS_DIR,
    TAXONOMY_ARTICLE_TYPES_ID,
    TAXONOMY_BRANDS_ID,
    TAXONOMY_CATEGORIES_ID,
)


def build_taxonomies_json() -> list[dict]:
    return [
        {"id": TAXONOMY_CATEGORIES_ID, "name": "Categories", "presentation": "Departments", "position": 0},
        {"id": TAXONOMY_BRANDS_ID, "name": "Brands", "presentation": "Brands", "position": 1},
        {"id": TAXONOMY_ARTICLE_TYPES_ID, "name": "Article Types", "presentation": "Article Types", "position": 2},
    ]


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract taxonomy seed data")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "001_demo_taxonomies.json"
    check_overwrite(out, args.force)
    ensure_output_dir(args.output)
    write_json(out, build_taxonomies_json())
    print(f"Written 3 taxonomies to {out}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Create `02_extract_taxons.py` (single entity: Taxon, SEO-enriched)**

```python
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

    def enrich(name: str, taxonomy_name: str) -> dict:
        seo = build_taxon_seo(name, taxonomy_name)
        seo["slug"] = make_slug(name)
        return seo

    lft = 1
    root_cat_id = taxon_id("categories_root")
    lft += 1

    for master_cat in sorted(master_categories):
        mc_id = taxon_id(f"cat.{master_cat}")
        mc_lft = lft
        lft += 1
        for sub_cat in sorted(sub_categories.get(master_cat, set())):
            sc_id = taxon_id(f"cat.{master_cat}.{sub_cat}")
            taxons.append({
                "id": sc_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
                "parent_id": mc_id, "name": sub_cat,
                "presentation": sub_cat, "slug": make_slug(sub_cat),
                "depth": 2, "lft": lft, "rgt": lft + 1, "position": 0,
                **build_taxon_seo(sub_cat, "Categories"),
            })
            lft += 2
        mc_rgt = lft
        lft += 1
        taxons.append({
            "id": mc_id, "taxonomy_id": TAXONOMY_CATEGORIES_ID,
            "parent_id": root_cat_id, "name": master_cat,
            "presentation": master_cat, "slug": make_slug(master_cat),
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
            "presentation": brand, "slug": make_slug(brand),
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
            "presentation": atype, "slug": make_slug(atype),
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
```

Notes:
- `taxon_id(identifier)` reproduces the old `guid("taxon", identifier)` calls exactly (`"categories_root"`, `"cat.{mc}"`, `"cat.{mc}.{sub}"`, `"brand.{b}"`, `"article_type.{at}"`), so IDs stay byte-identical.
- Taxon slug list order and tree structure are byte-identical to the old `01_extract_taxonomies.py`.

- [ ] **Step 3: Verify taxon IDs unchanged against old output**

Run:
```bash
cd benchmarks/scripts/demo-seed && uv run python 01_extract_taxonomies.py --output output --force && uv run python 02_extract_taxons.py --dataset ../../data/raw/fashion-product-images --output output --force
```

Then compare `id` sets between old `output/demo_taxons.json` and new `output/002_demo_taxons.json`:

```bash
python3 - <<'EOF'
import json
old = {t["id"] for t in json.load(open("output/demo_taxons.json"))}
new = {t["id"] for t in json.load(open("output/002_demo_taxons.json"))}
print("old:", len(old), "new:", len(new), "identical:", old == new)
EOF
```
Expected: `identical: True` (new may have MORE taxons only if the dataset grew).

- [ ] **Step 4: Create `03_extract_option_types.py` (single entity: OptionType)**

```python
#!/usr/bin/env python
"""Extract option type entities (single entity: OptionType)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import OPTION_TYPE_COLOR_ID, OPTION_TYPE_SIZE_ID, SCRIPTS_DIR  # noqa: E402


def build_option_types_json() -> list[dict]:
    return [
        {"id": OPTION_TYPE_SIZE_ID, "name": "Size", "presentation": "Size", "position": 0, "filterable": True},
        {"id": OPTION_TYPE_COLOR_ID, "name": "Color", "presentation": "Color", "position": 1, "filterable": True},
    ]


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract option type seed data")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "003_demo_option_types.json"
    check_overwrite(out, args.force)
    ensure_output_dir(args.output)
    write_json(out, build_option_types_json())
    print(f"Written 2 option types to {out}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 5: Create `04_extract_option_values.py` (single entity: OptionValue)**

```python
#!/usr/bin/env python
"""Extract option value entities (single entity: OptionValue).

Colors come from the whole styles.csv; sizes come from the style JSON of
the selected products (matching the old 01 + 02 combined behavior).
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import option_value_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import OPTION_TYPE_COLOR_ID, OPTION_TYPE_SIZE_ID, SCRIPTS_DIR  # noqa: E402
from source import extract_sizes, group_products, load_styles_rows  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract option value seed data")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "004_demo_option_values.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    colors: set[str] = set()
    sizes: set[str] = set()

    for row in rows:
        c = row.get("baseColour", "").strip()
        if c:
            colors.add(c)

    for _name, group_rows in group_products(rows)[: args.count]:
        for row in group_rows:
            style_json_path = args.dataset / "styles" / f"{row.get('id', '').strip()}.json"
            if not style_json_path.exists():
                continue
            try:
                sizes.update(extract_sizes(json.loads(style_json_path.read_text())))
            except Exception:
                continue

    values: list[dict] = []
    for pos, color in enumerate(sorted(colors)):
        values.append({
            "id": option_value_id("color", color),
            "option_type_id": OPTION_TYPE_COLOR_ID,
            "name": color, "presentation": color, "position": pos,
        })
    for pos, size in enumerate(sorted(sizes)):
        values.append({
            "id": option_value_id("size", size),
            "option_type_id": OPTION_TYPE_SIZE_ID,
            "name": size, "presentation": size, "position": len(colors) + pos,
        })

    ensure_output_dir(args.output)
    write_json(out, values)
    print(f"Written {len(colors)} colors + {len(sizes)} sizes to {out}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 6: Run the four scripts and verify counts against old output**

Run:
```bash
cd benchmarks/scripts/demo-seed && uv run python 03_extract_option_types.py --output output --force && uv run python 04_extract_option_values.py --dataset ../../data/raw/fashion-product-images --output output --force
```

Verify:
```bash
python3 - <<'EOF'
import json
old_types = json.load(open("output/demo_option_types.json"))
new_types = json.load(open("output/003_demo_option_types.json"))
print("option types identical:", old_types == new_types)
old_vals = json.load(open("output/demo_option_values.json"))
new_vals = json.load(open("output/004_demo_option_values.json"))
print("option value ids identical:", {v["id"] for v in old_vals} == {v["id"] for v in new_vals})
print("old:", len(old_vals), "new:", len(new_vals))
EOF
```
Expected: `option types identical: True` and `option value ids identical: True`.

- [ ] **Step 7: Lint, test, commit**

Run: `cd benchmarks && uv run ruff check scripts/demo-seed/ && uv run pytest scripts/demo-seed/tests/ -q`
Expected: no ruff errors, tests PASS.

```bash
git add benchmarks/scripts/demo-seed/01_extract_taxonomies.py benchmarks/scripts/demo-seed/02_extract_taxons.py benchmarks/scripts/demo-seed/03_extract_option_types.py benchmarks/scripts/demo-seed/04_extract_option_values.py
git commit -m "refactor(seed): split taxonomy extractor into per-entity scripts"
```

---
### Task 3: Product-domain scripts (Product, Variant, VariantImage, ProductTaxon)

**Files:**
- Rewrite: `benchmarks/scripts/demo-seed/05_extract_products.py` → `005_demo_products.json` (+ SEO enrichment)
- Create: `benchmarks/scripts/demo-seed/06_extract_variants.py` → `006_demo_variants.json` (embedded option_values)
- Create: `benchmarks/scripts/demo-seed/07_extract_variant_images.py` → `007_demo_variant_images.json`
- Create: `benchmarks/scripts/demo-seed/08_extract_product_taxons.py` → `008_demo_product_taxons.json`
- Modify: `benchmarks/scripts/demo-seed/shared.py` (add `ARTICLE_PRICE_MAP`)
- Modify: `benchmarks/scripts/demo-seed/variants.py` (add `derive_sku`)
- Modify: `benchmarks/scripts/demo-seed/source.py` (add `sizes_for_colors`)
- Modify: `benchmarks/scripts/demo-seed/tests/test_variants.py` (add sku test)

**Interfaces:**
- Consumes: `source` (`load_styles_rows`, `group_products`, `load_style_json`, `extract_sizes`, `extract_product_metadata`, `unique_colors`, `sizes_for_colors`), `ids` (`product_id`, `master_variant_id`, `variant_id`, `taxon_id`, `variant_image_id`), `metadata` (`build_product_seo`, `extract_tags`, `variant_dimensions`, `cost_price`, `build_image_alt`), `variants` (`generate_variants`, `derive_sku`), `shared.ARTICLE_PRICE_MAP`, `json_io`.
- Produces: `005_demo_products.json`, `006_demo_variants.json`, `007_demo_variant_images.json`, `008_demo_product_taxons.json`.

- [ ] **Step 1: Add `derive_sku` to `variants.py` + shared price map + `sizes_for_colors` to `source.py`**

Append to `benchmarks/scripts/demo-seed/variants.py`:

```python
def derive_sku(base: str, variant_index: int) -> str:
    safe = base.upper().replace(" ", "-").replace("'", "").replace("&", "AND")[:20]
    return f"{safe}-{variant_index:03d}"
```

Append to `benchmarks/scripts/demo-seed/shared.py`:

```python
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
```

Append to `benchmarks/scripts/demo-seed/source.py`:

```python
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
```

Append to `benchmarks/scripts/demo-seed/tests/test_variants.py`:

```python
from variants import derive_sku


def test_derive_sku_indexed():
    assert derive_sku("Navy Blue Dress", 3) == "NAVY-BLUE-DRESS-003"
    assert derive_sku("R&B Tee", 10) == "RANDB-TEE-010"
    assert derive_sku("Jacket 'Deluxe'", 0) == "JACKET-DELUXE-000"
```

Run: `cd benchmarks && uv run pytest scripts/demo-seed/tests/ -q` → PASS.

- [ ] **Step 2: Rewrite `05_extract_products.py` (single entity: Product, SEO-enriched)**

Replace the entire file:

```python
#!/usr/bin/env python
"""Extract product entities from styles.csv (single entity: Product)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import master_variant_id, product_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import build_product_seo, extract_tags  # noqa: E402
from shared import ARTICLE_PRICE_MAP, SCRIPTS_DIR  # noqa: E402
from source import extract_product_metadata, group_products, load_style_json, load_styles_rows  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract product seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "005_demo_products.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    products: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        article = first.get("articleType", "").strip()
        benchmark_id = first.get("id", "").strip()
        meta = extract_product_metadata(load_style_json(args.dataset, benchmark_id))
        brand = first.get("brandName", "").strip()
        brand_initials = "".join([w[0] for w in brand.split() if w]).upper() if brand else "XX"
        style_code = f"{brand_initials}-{article[:10]}".upper()
        department = first.get("masterCategory", "").strip()
        gender_target = first.get("gender", "").strip() or "Unisex"
        tags = extract_tags(meta.get("article_attributes"))

        slug = display_name.lower().replace(" ", "-").replace("'", "").replace("&", "and")[:200]
        slug = slug.rstrip("-")

        seo = build_product_seo(display_name, article, brand, department, tags)

        products.append({
            "id": product_id(display_name),
            "name": display_name[:255],
            "slug": slug[:255],
            "description": f"{display_name} — {article} by {brand or 'Unknown Brand'}"[:2000],
            "status": "Active",
            "gender_target": gender_target,
            "meta_title": seo["meta_title"],
            "meta_description": seo["meta_description"],
            "meta_keywords": seo["meta_keywords"],
            "style_code": style_code[:100],
            "season_name": meta.get("season"),
            "department": department[:100] if department else None,
            "material_composition": meta.get("material_composition"),
            "care_instructions": meta.get("care_instructions"),
            "master_variant_id": master_variant_id(display_name),
        })

    ensure_output_dir(args.output)
    write_json(out, products)
    print(f"Written {len(products)} products to {out}")


if __name__ == "__main__":
    main()
```

Note: scripts 05-08 are fully source-driven (no cross-script file reads). `ARTICLE_PRICE_MAP` lives in `shared.py` so `06_extract_variants.py` derives the same prices without reading `005_demo_products.json`.

- [ ] **Step 3: Create `06_extract_variants.py` (single entity: Variant)**

```python
#!/usr/bin/env python
"""Extract variant entities from styles.csv (single entity: Variant).

One variant per color×size combination (size-major order), capped at 10
per product; the master variant is the first combination. Each variant
embeds its option values (one per option type).
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import master_variant_id, product_id, variant_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import cost_price, variant_dimensions  # noqa: E402
from shared import ARTICLE_PRICE_MAP, OPTION_TYPE_COLOR_ID, OPTION_TYPE_SIZE_ID, SCRIPTS_DIR  # noqa: E402
from source import (
    extract_product_metadata,
    group_products,
    load_style_json,
    load_styles_rows,
    sizes_for_colors,
    unique_colors,
)  # noqa: E402
from variants import derive_sku, generate_variants  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract variant seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "006_demo_variants.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    variants: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        article = first.get("articleType", "").strip()
        price = ARTICLE_PRICE_MAP.get(article, 39.99)
        meta = extract_product_metadata(load_style_json(args.dataset, first.get("id", "").strip()))
        hs_code = meta.get("article_number")
        hs_code = hs_code[:20] if hs_code else None

        colors = unique_colors(group_rows)
        sizes_by_color = sizes_for_colors(args.dataset, group_rows)
        dims = variant_dimensions(article)
        cost = cost_price(price)

        for combo in generate_variants(display_name, colors, sizes_by_color):
            variant_id_str = (
                master_variant_id(display_name)
                if combo["is_master"]
                else variant_id(display_name, combo["color"] or "", combo["size"] or "")
            )
            option_values: list[dict] = []
            if combo["color"]:
                option_values.append({"option_type_id": OPTION_TYPE_COLOR_ID, "option_value_name": combo["color"]})
            if combo["size"]:
                option_values.append({"option_type_id": OPTION_TYPE_SIZE_ID, "option_value_name": combo["size"]})

            sku = derive_sku(display_name, combo["position"])
            variants.append({
                "id": variant_id_str,
                "product_id": product_id(display_name),
                "sku": sku,
                "is_master": combo["is_master"],
                "position": combo["position"],
                "price": price,
                "barcode": f"{sku}-BAR",
                "hs_code": hs_code,
                **dims,
                "cost_price": cost,
                "cost_currency": "USD",
                "option_values": option_values,
            })

    ensure_output_dir(args.output)
    write_json(out, variants)
    print(f"Written {len(variants)} variants to {out}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 4: Create `07_extract_variant_images.py` (single entity: VariantImage)**

```python
#!/usr/bin/env python
"""Extract variant image entities from styles.csv (single entity: VariantImage).

Master variant carries Default/Search/Gallery images (unchanged IDs); each
child variant gets one Default image. Width/height/file_size are backfilled
by 10_process_images.py.
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import master_variant_id, variant_id, variant_image_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import build_image_alt  # noqa: E402
from shared import SCRIPTS_DIR  # noqa: E402
from source import (
    extract_product_metadata,
    group_products,
    load_style_json,
    load_styles_rows,
    sizes_for_colors,
    unique_colors,
)  # noqa: E402
from variants import generate_variants  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract variant image seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--display-size", type=int, default=512)
    parser.add_argument("--search-size", type=int, default=224)
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "007_demo_variant_images.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    images: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        benchmark_id = first.get("id", "").strip()

        master_id = master_variant_id(display_name)
        images.append({
            "id": variant_image_id(display_name, "0.default"),
            "variant_id": master_id,
            "content_type": "image/jpeg",
            "file_name": f"{benchmark_id}.jpg",
            "storage_path": str(Path("images") / "medium" / f"{benchmark_id}.jpg"),
            "position": 0,
            "alt": build_image_alt(display_name, "Default"),
            "type": "Default",
        })
        images.append({
            "id": variant_image_id(display_name, "0.search"),
            "variant_id": master_id,
            "content_type": "image/jpeg",
            "file_name": f"{benchmark_id}.jpg",
            "storage_path": str(Path("images") / "search" / str(args.search_size) / f"{benchmark_id}.jpg"),
            "position": 1,
            "alt": build_image_alt(display_name, "Search"),
            "type": "Search",
        })
        meta = extract_product_metadata(load_style_json(args.dataset, benchmark_id))
        gallery_labels = ["back", "front"]
        for gi, gl in enumerate(gallery_labels):
            s_images = meta.get("style_images")
            if s_images and gl in s_images:
                images.append({
                    "id": variant_image_id(display_name, f"0.{gl}"),
                    "variant_id": master_id,
                    "content_type": "image/jpeg",
                    "file_name": f"{benchmark_id}.jpg",
                    "storage_path": f"images/medium/{benchmark_id}.jpg",
                    "position": 2 + gi,
                    "alt": build_image_alt(display_name, "Gallery"),
                    "type": "Gallery",
                })

        colors = unique_colors(group_rows)
        sizes_by_color = sizes_for_colors(args.dataset, group_rows)
        for combo in generate_variants(display_name, colors, sizes_by_color):
            if combo["is_master"]:
                continue
            child_id = variant_id(display_name, combo["color"] or "", combo["size"] or "")
            images.append({
                "id": variant_image_id(display_name, f"{combo['color'] or 'NA'}.{combo['size'] or 'NA'}.default"),
                "variant_id": child_id,
                "content_type": "image/jpeg",
                "file_name": f"{benchmark_id}.jpg",
                "storage_path": str(Path("images") / "medium" / f"{benchmark_id}.jpg"),
                "position": 0,
                "alt": build_image_alt(display_name, "Default"),
                "type": "Default",
            })

    ensure_output_dir(args.output)
    write_json(out, images)
    print(f"Written {len(images)} variant images to {out}")


if __name__ == "__main__":
    main()
```

Note: `variant_image_id(display_name, "0.default")`, `"0.search"`, `"0.back"`, `"0.front"` reproduce the OLD master image IDs exactly (`guid("variant_image", f"{display_name}.0.default")` etc.), keeping `demo_embeddings.json` valid.

- [ ] **Step 5: Create `08_extract_product_taxons.py` (single entity: ProductTaxon = Classification)**

```python
#!/usr/bin/env python
"""Extract product↔taxon classification entities (single entity: ProductTaxon)."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import product_id, taxon_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from shared import SCRIPTS_DIR  # noqa: E402
from source import group_products, load_styles_rows  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract product taxon seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "008_demo_product_taxons.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    refs: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        pid = product_id(display_name)
        mc = first.get("masterCategory", "").strip()
        b = first.get("brandName", "").strip()
        at = first.get("articleType", "").strip()
        pos = 0
        if mc:
            refs.append({"product_id": pid, "taxon_id": taxon_id(f"cat.{mc}"), "position": pos})
            pos += 1
        if b:
            refs.append({"product_id": pid, "taxon_id": taxon_id(f"brand.{b}"), "position": pos})
            pos += 1
        if at:
            refs.append({"product_id": pid, "taxon_id": taxon_id(f"article_type.{at}"), "position": pos})
            pos += 1

    ensure_output_dir(args.output)
    write_json(out, refs)
    print(f"Written {len(refs)} product taxon refs to {out}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 6: Run the four scripts and sanity-check invariants**

Run:
```bash
cd benchmarks/scripts/demo-seed && uv run python 05_extract_products.py --dataset ../../data/raw/fashion-product-images --output output --force && uv run python 06_extract_variants.py --dataset ../../data/raw/fashion-product-images --output output --force && uv run python 07_extract_variant_images.py --dataset ../../data/raw/fashion-product-images --output output --force && uv run python 08_extract_product_taxons.py --dataset ../../data/raw/fashion-product-images --output output --force
```

Sanity checks:
```bash
python3 - <<'EOF'
import json
from collections import Counter
products = json.load(open("output/005_demo_products.json"))
variants = json.load(open("output/006_demo_variants.json"))
images = json.load(open("output/007_demo_variant_images.json"))
refs = json.load(open("output/008_demo_product_taxons.json"))

per_product = Counter(v["product_id"] for v in variants)
print("products:", len(products))
print("variants:", len(variants), "max per product:", max(per_product.values()))
print("all products <=10 variants:", max(per_product.values()) <= 10)
print("masters all position 0:",
      all(v["position"] == 0 for v in variants if v["is_master"]))
violations = [v for v in variants
              if len(v["option_values"]) > len({o["option_type_id"] for o in v["option_values"]})]
print("variants with >1 value per option type:", len(violations))
pid_to_master = {p["id"]: p["master_variant_id"] for p in products}
print("master id matches product.master_variant_id:",
      all(v["id"] == pid_to_master[v["product_id"]] for v in variants if v["is_master"]))
print("images:", len(images), "refs:", len(refs))
EOF
```
Expected: `all products <=10 variants: True`, `variants with >1 value per option type: 0`, `master id matches product.master_variant_id: True`. Fix any script bugs until these hold.

- [ ] **Step 7: Lint, test, commit**

Run: `cd benchmarks && uv run ruff check scripts/demo-seed/ && uv run pytest scripts/demo-seed/tests/ -q`
Expected: no ruff errors, tests PASS.

```bash
git add benchmarks/scripts/demo-seed/05_extract_products.py benchmarks/scripts/demo-seed/06_extract_variants.py benchmarks/scripts/demo-seed/07_extract_variant_images.py benchmarks/scripts/demo-seed/08_extract_product_taxons.py benchmarks/scripts/demo-seed/variants.py benchmarks/scripts/demo-seed/source.py benchmarks/scripts/demo-seed/shared.py benchmarks/scripts/demo-seed/tests/test_variants.py
git commit -m "refactor(seed): split product extractor into per-entity scripts with variant cap"
```

---
### Task 4: Rename/adapt stock, images, embeddings, verify scripts + orchestrator

**Files:**
- Rename: `benchmarks/scripts/demo-seed/03_process_images.py` → `10_process_images.py` (read `007_demo_variant_images.json`, backfill dims/file_size)
- Rename: `benchmarks/scripts/demo-seed/04_generate_embeddings.py` → `11_generate_embeddings.py` (read `007`, write `012_demo_embeddings.json`)
- Rename: `benchmarks/scripts/demo-seed/05_extract_stock.py` → `09_extract_stock.py` (read `006_demo_variants.json`, write `009/010/011_demo_stock_*.json`)
- Rename: `benchmarks/scripts/demo-seed/06_verify_output.py` → `12_verify_output.py` (new filenames + invariant checks)
- Modify: `benchmarks/scripts/demo-seed/run_all.py` (new step order)
- Modify: `benchmarks/scripts/demo-seed/shared.py` (remove `write_json`/`check_overwrite` — now in `json_io.py`)

**Interfaces:**
- Consumes: `json_io`, `shared` constants, the numbered JSON datasets from Tasks 2-3.
- Produces: `009_demo_stock_locations.json`, `010_demo_stock_items.json`, `011_demo_stock_movements.json`, `012_demo_embeddings.json`, `images/` files.

- [ ] **Step 1: `git mv` the four scripts**

```bash
cd benchmarks/scripts/demo-seed
git mv 03_process_images.py 10_process_images.py
git mv 04_generate_embeddings.py 11_generate_embeddings.py
git mv 05_extract_stock.py 09_extract_stock.py
git mv 06_verify_output.py 12_verify_output.py
```

- [ ] **Step 2: Update `09_extract_stock.py` (renamed from 05)**

Only two changes:
1. Input: `variants_json = args.output / "demo_variants.json"` → `"006_demo_variants.json"`
2. Outputs: `demo_stock_locations.json` → `009_demo_stock_locations.json`, `demo_stock_items.json` → `010_demo_stock_items.json`, `demo_stock_movements.json` → `011_demo_stock_movements.json`
3. Imports: replace `from shared import SCRIPTS_DIR, check_overwrite, guid, write_json` with:

```python
from json_io import check_overwrite, write_json  # noqa: E402
from shared import SCRIPTS_DIR, guid  # noqa: E402
```

Run: `uv run python 09_extract_stock.py --output output --force` → writes 009/010/011.

- [ ] **Step 3: Update `10_process_images.py` (renamed from 03)**

Changes:
1. Input: `images_json = args.output / "demo_variant_images.json"` → `"007_demo_variant_images.json"`
2. Imports: `from shared import MODEL_INPUT_SIZES, SCRIPTS_DIR` stays; add `from json_io import write_json`
3. After each unique image is successfully resized, record its dimensions + file size, then backfill ALL records sharing that `storage_path` and rewrite `007_demo_variant_images.json`:

Replace the processing loops' bookkeeping with:

```python
    medium_imgs = [r for r in unique if "images/medium/" in r["storage_path"]]
    print(f"\n--- Processing {len(medium_imgs)} display images at {display_size}px ---")
    dims_by_storage: dict[str, tuple[int, int, int]] = {}
    for rec in tqdm(medium_imgs, desc="Display"):
        src = source_dir / rec["file_name"]
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue
        dst = args.output / rec["storage_path"]
        if resize_image(src, dst, display_size):
            ok += 1
            with Image.open(dst) as img:
                dims_by_storage[rec["storage_path"]] = (img.width, img.height, dst.stat().st_size)
        else:
            fail += 1

    search_imgs = [r for r in unique if "images/search/" in r["storage_path"]]
    print(f"\n--- Processing {len(search_imgs)} search images at {search_size}px ---")
    for rec in tqdm(search_imgs, desc="Search"):
        src = source_dir / rec["file_name"]
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue
        dst = args.output / rec["storage_path"]
        if resize_image(src, dst, search_size):
            ok += 1
            with Image.open(dst) as img:
                dims_by_storage[rec["storage_path"]] = (img.width, img.height, dst.stat().st_size)
        else:
            fail += 1

    if dims_by_storage:
        for rec in image_records:
            dims = dims_by_storage.get(rec["storage_path"])
            if dims is not None:
                rec["width"], rec["height"], rec["file_size"] = dims
        write_json(images_json, image_records)
        print(f"Backfilled width/height/file_size into {len(dims_by_storage)} unique images in {images_json}")
```

- [ ] **Step 4: Update `11_generate_embeddings.py` (renamed from 04)**

Changes:
1. Input: `images_json = args.output / "demo_variant_images.json"` → `"007_demo_variant_images.json"`
2. Output: `(args.output / "demo_embeddings.json").write_text(...)` → `(args.output / "012_demo_embeddings.json").write_text(...)`
3. Imports: `from shared import SCRIPTS_DIR` stays.

- [ ] **Step 5: Rewrite `12_verify_output.py` (renamed from 06)**

Keep the existing FK/dupe/uniqueness checks but update every filename to the numbered names, and add the new invariant section. The full file:

```python
#!/usr/bin/env python
"""Verify demo seed output JSON integrity — FKs, counts, cross-references, domain invariants."""
from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import load_json  # noqa: E402
from shared import SCRIPTS_DIR  # noqa: E402


def printfail(msg: str) -> None:
    print(f"  FAIL  {msg}")


def passmsg(msg: str) -> None:
    print(f"  PASS  {msg}")


def main() -> None:
    parser = argparse.ArgumentParser(description="Verify demo seed output JSON")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=0, help="Expected product count (0=skip check)")
    args = parser.parse_args()

    out = args.output
    print(f"Verifying output in {out}\n")

    errors = 0
    files = {
        "products": "005_demo_products.json",
        "variants": "006_demo_variants.json",
        "images": "007_demo_variant_images.json",
        "product_taxons": "008_demo_product_taxons.json",
        "taxonomies": "001_demo_taxonomies.json",
        "taxons": "002_demo_taxons.json",
        "option_types": "003_demo_option_types.json",
        "option_values": "004_demo_option_values.json",
        "stock_locations": "009_demo_stock_locations.json",
        "stock_items": "010_demo_stock_items.json",
        "embeddings": "012_demo_embeddings.json",
    }
    data: dict[str, list[dict]] = {}
    for key, name in files.items():
        fp = out / name
        if not fp.exists():
            printfail(f"MISSING: {name}")
            errors += 1
            data[key] = []
        else:
            data[key] = json.loads(fp.read_text())

    products = data["products"]
    variants = data["variants"]
    images = data["images"]
    refs = data["product_taxons"]
    taxons = data["taxons"]
    option_values = data["option_values"]
    stock_locations = data["stock_locations"]
    stock_items = data["stock_items"]
    embeddings = data["embeddings"]

    product_ids = {p["id"] for p in products}
    variant_ids = {v["id"] for v in variants}
    image_ids = {i["id"] for i in images}
    taxon_ids = {t["id"] for t in taxons}
    option_value_names = {v["name"] for v in option_values}
    location_codes = {loc["code"] for loc in stock_locations}
    master_ids = {v["id"] for v in variants if v.get("is_master")}
    search_image_ids = {i["id"] for i in images if i.get("type") == "Search"}

    # --- counts ---
    print("=== Counts ===")
    checks = [
        ("Products", len(products), args.count if args.count else ">=1"),
        ("Variants", len(variants), ">=1"),
        ("VariantImages", len(images), ">=2"),
        ("ProductTaxons", len(refs), ">=1"),
        ("Taxonomies", len(data["taxonomies"]), 3),
        ("Taxons", len(taxons), ">=3"),
        ("OptionTypes", len(data["option_types"]), 2),
        ("OptionValues", len(option_values), ">=2"),
        ("StockLocations", len(stock_locations), 3),
        ("StockItems", len(stock_items), ">=1"),
        ("Embeddings", len(embeddings), ">=1"),
    ]
    for label, actual, expected in checks:
        if isinstance(expected, int) and actual != expected:
            printfail(f"{label}: {actual} (expected {expected})")
            errors += 1
        elif isinstance(expected, str) and actual < 1:
            printfail(f"{label}: {actual} (expected at least 1)")
            errors += 1
        else:
            passmsg(f"{label}: {actual}")

    # --- FK integrity ---
    print("\n=== FK Integrity ===")
    checks_fk = [
        ("variant.product_id -> product.id", [v for v in variants if v["product_id"] not in product_ids]),
        ("image.variant_id -> variant.id", [i for i in images if i["variant_id"] not in variant_ids]),
        ("product_taxon.product_id -> product.id", [r for r in refs if r["product_id"] not in product_ids]),
        ("product_taxon.taxon_id -> taxon.id", [r for r in refs if r["taxon_id"] not in taxon_ids]),
        ("stock_item.variant_id -> variant.id", [s for s in stock_items if s["variant_id"] not in variant_ids]),
        ("stock_item.stock_location_code -> location.code", [s for s in stock_items if s["stock_location_code"] not in location_codes]),
        ("embedding.variant_image_id -> image.id", [e for e in embeddings if e["variant_image_id"] not in image_ids]),
        ("product.master_variant_id -> variant.id", [p for p in products if p.get("master_variant_id") and p["master_variant_id"] not in variant_ids]),
        ("assignment option_value_name -> option_value.name", [o for v in variants for o in v.get("option_values", []) if o["option_value_name"].lower() not in {n.lower() for n in option_value_names}]),
        ("embedding -> search image only", [e for e in embeddings if e["variant_image_id"] not in search_image_ids]),
    ]
    for label, bad in checks_fk:
        if bad:
            printfail(f"{len(bad)} {label}")
            errors += 1
        else:
            passmsg(label)

    # --- domain invariants ---
    print("\n=== Domain Invariants ===")

    per_product = Counter(v["product_id"] for v in variants)
    over_cap = {pid: n for pid, n in per_product.items() if n > 10}
    if over_cap:
        printfail(f"{len(over_cap)} products exceed 10 variants")
        errors += 1
    else:
        passmsg("every product has at most 10 variants")

    masters_bad_pos = [v for v in variants if v.get("is_master") and v.get("position") != 0]
    if masters_bad_pos:
        printfail(f"{len(masters_bad_pos)} master variants not at position 0")
        errors += 1
    else:
        passmsg("every master variant is at position 0")

    multi_type = [v for v in variants
                  if len(v.get("option_values", [])) > len({o["option_type_id"] for o in v.get("option_values", [])})]
    if multi_type:
        printfail(f"{len(multi_type)} variants with >1 value per option type")
        errors += 1
    else:
        passmsg("every variant has at most 1 value per option type")

    master_combo_dupes = 0
    for v in variants:
        if not v.get("is_master"):
            continue
        master_types = {o["option_type_id"]: o["option_value_name"] for o in v.get("option_values", [])}
        for other in variants:
            if other.get("is_master") or other["product_id"] != v["product_id"]:
                continue
            other_types = {o["option_type_id"]: o["option_value_name"] for o in other.get("option_values", [])}
            if master_types == other_types:
                master_combo_dupes += 1
    if master_combo_dupes:
        printfail(f"{master_combo_dupes} child variants duplicate the master combo")
        errors += 1
    else:
        passmsg("no child variant duplicates the master combo")

    pid_to_master = {p["id"]: p.get("master_variant_id") for p in products}
    bad_master_id = [v for v in variants if v.get("is_master") and pid_to_master.get(v["product_id"]) != v["id"]]
    if bad_master_id:
        printfail(f"{len(bad_master_id)} master variants mismatch product.master_variant_id")
        errors += 1
    else:
        passmsg("product.master_variant_id matches its master variant id")

    # --- uniqueness ---
    print("\n=== Uniqueness ===")
    for label, key, data_list in [
        ("Products", "id", products),
        ("Variants", "id", variants),
        ("VariantImages", "id", images),
        ("Taxons", "id", taxons),
        ("StockItems", "id", stock_items),
        ("ProductTaxons", ("product_id", "taxon_id"), refs),
    ]:
        if isinstance(key, tuple):
            ids = [tuple(d[k] for k in key) for d in data_list]
        else:
            ids = [d[key] for d in data_list]
        dupes = len(ids) - len(set(ids))
        if dupes:
            printfail(f"{label}: {dupes} duplicate {key}s")
            errors += 1
        else:
            passmsg(f"{label}: no duplicate {key}s")

    print(f"\n{'=' * 60}")
    if errors:
        printfail(f"VERIFICATION FAILED — {errors} error(s)")
        sys.exit(1)
    else:
        print("  VERIFICATION PASSED — all checks OK")
        print(f"{'=' * 60}")


if __name__ == "__main__":
    main()
```

Note: the old `06_verify_output.py` also checked `master_variant_id` FK and image-type rules; those are folded into `checks_fk` above. The old `--count` products check is preserved.

- [ ] **Step 6: Update `run_all.py`**

Replace the `steps` list with:

```python
    steps = [
        ("01_extract_taxonomies.py", ["--output", str(args.output)] + force_args),
        ("02_extract_taxons.py", ["--dataset", str(args.dataset), "--output", str(args.output)] + force_args),
        ("03_extract_option_types.py", ["--output", str(args.output)] + force_args),
        ("04_extract_option_values.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)] + force_args),
        ("05_extract_products.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)] + force_args),
        ("06_extract_variants.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)] + force_args),
        ("07_extract_variant_images.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count),
                                          "--display-size", args.display_size, "--search-size", args.search_size] + force_args),
        ("08_extract_product_taxons.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)] + force_args),
        ("09_extract_stock.py", ["--output", str(args.output)] + force_args),
        ("10_process_images.py", ["--dataset", str(args.dataset), "--output", str(args.output),
                                  "--display-size", args.display_size, "--search-size", args.search_size]),
        ("11_generate_embeddings.py", ["--output", str(args.output)]),
        ("12_verify_output.py", ["--output", str(args.output), "--count", str(args.count)]),
    ]
```

- [ ] **Step 7: Remove `write_json`/`check_overwrite` from `shared.py`**

All scripts now import them from `json_io.py`. In `shared.py`:

- Delete the `import json` line (if it becomes unused — check: `guid` needs only `uuid`; `json` was used by `write_json` only)
- Delete `check_overwrite` and `write_json` functions

Verify no remaining importer: `rg -n "from shared import.*(write_json|check_overwrite)" benchmarks/scripts/demo-seed/` → no matches.

- [ ] **Step 8: Lint, run full pipeline smoke test, commit**

Run: `cd benchmarks && uv run ruff check scripts/demo-seed/`
Expected: no errors.

Smoke test (fast, no image/embedding regeneration):
```bash
cd benchmarks/scripts/demo-seed
uv run python 01_extract_taxonomies.py --output output --force
uv run python 02_extract_taxons.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 03_extract_option_types.py --output output --force
uv run python 04_extract_option_values.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 05_extract_products.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 06_extract_variants.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 07_extract_variant_images.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 08_extract_product_taxons.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 09_extract_stock.py --output output --force
uv run python 12_verify_output.py --output output --count 1000
```
Expected: verify PASSES (images/embeddings steps are intentionally skipped here — they need the existing files, which remain valid).

```bash
git add benchmarks/scripts/demo-seed/
git commit -m "refactor(seed): renumber dataset outputs and add invariant verification"
```

---
### Task 5: Split C# Catalog seeders (OptionType, OptionValue, Product, Variant, VariantImage, ProductTaxon)

**Files:**
- Delete: `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs`
- Delete: `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs`
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/OptionType.Seeder.cs` (Order 100)
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/OptionValue.Seeder.cs` (Order 105)
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs` → new `CatalogProductSeeder` (Order 130)
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/Variant.Seeder.cs` (Order 132)
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/VariantImage.Seeder.cs` (Order 134)
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/ProductTaxon.Seeder.cs` (Order 136)
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Taxonomy.Seeder.cs` (filename → `001_demo_taxonomies.json`)
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Taxon.Seeder.cs` (filename → `002_demo_taxons.json` + enrichment passthrough)
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs` (filename → `012_demo_embeddings.json`, Order → 137)
- Modify: `service/Api/src/Module/Catalog/Catalog.Extension.cs` (registration)

**Interfaces:**
- Consumes: numbered JSON datasets from Tasks 2-3.
- Produces: DB seeders matching the numbered files; later tasks (Task 6) do the same for Inventory.

- [ ] **Step 1: Create `OptionType.Seeder.cs`**

```csharp
using Module.Catalog.Domain.OptionTypes;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionTypeSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<OptionType>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoOptionTypeJson>("003_demo_option_types.json");
        if (json is null)
            return Result.Ok();

        foreach (var t in json)
        {
            var result = OptionTypeMethod.Create(
                name: t.Name, presentation: t.Presentation,
                position: t.Position, filterable: t.Filterable,
                id: Guid.Parse(t.Id));
            Context.Set<OptionType>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoOptionTypeJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Presentation { get; init; } = default!;
        public int Position { get; init; }
        public bool Filterable { get; init; }
    }
}
```

- [ ] **Step 2: Create `OptionValue.Seeder.cs`**

```csharp
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionValueSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 105;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<OptionValue>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoOptionValueJson>("004_demo_option_values.json");
        if (json is null)
            return Result.Ok();

        foreach (var v in json)
        {
            var result = OptionValueMethod.Create(
                optionTypeId: Guid.Parse(v.OptionTypeId),
                name: v.Name, presentation: v.Presentation, position: v.Position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoOptionValueJson
    {
        public string Id { get; init; } = default!;
        public string OptionTypeId { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Presentation { get; init; } = default!;
        public int Position { get; init; }
    }
}
```

- [ ] **Step 3: Create the new `Product.Seeder.cs` (CatalogProductSeeder)**

```csharp
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Options;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogProductSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Product>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoProductJson>("005_demo_products.json");
        if (json is null)
            return Result.Ok();

        var optionTypes = await Context.Set<OptionType>().ToListAsync(cancellationToken);
        var colorTypeId = optionTypes.FirstOrDefault(o => o.Name == "Color")?.Id;
        var sizeTypeId = optionTypes.FirstOrDefault(o => o.Name == "Size")?.Id;

        foreach (var pj in json)
        {
            var productResult = ProductMethod.Create(
            #region Properties
                name: pj.Name,
                description: pj.Description,
                status: Enum.TryParse<ProductStatus>(pj.Status, out var parsedStatus) ? parsedStatus : ProductStatus.Active,
            #endregion Properties
            #region SEO
                slug: pj.Slug,
                metaTitle: pj.MetaTitle,
                metaDescription: pj.MetaDescription,
                metaKeywords: pj.MetaKeywords,
            #endregion SEO
            #region Timestamp
                availableOn: null,
                discontinueOn: null,
                makeActiveAt: null,
            #endregion Timestamp
            #region Fashion
                styleCode: pj.StyleCode,
                seasonName: pj.SeasonName,
                materialComposition: pj.MaterialComposition,
                careInstructions: pj.CareInstructions,
                fitNotes: pj.FitNotes,
                department: pj.Department,
                genderTarget: pj.GenderTarget,
            #endregion Fashion
                id: Guid.Parse(pj.Id));
            var product = productResult.Value;
            product.GenderTarget = pj.GenderTarget;
            product.MasterVariantId = Guid.Parse(pj.MasterVariantId);

            Context.Set<Product>().Add(product);

            if (colorTypeId is not null && sizeTypeId is not null)
            {
                Context.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, colorTypeId.Value, 0).Value);
                Context.Set<ProductOptionType>().Add(ProductOptionTypeMethod.Create(product.Id, sizeTypeId.Value, 1).Value);
            }
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoProductJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string GenderTarget { get; init; } = default!;
        public string MetaTitle { get; init; } = default!;
        public string? MetaDescription { get; init; }
        public string MetaKeywords { get; init; } = default!;
        public string MasterVariantId { get; init; } = default!;
        public string? StyleCode { get; init; }
        public string? SeasonName { get; init; }
        public string? MaterialComposition { get; init; }
        public string? CareInstructions { get; init; }
        public string? FitNotes { get; init; }
        public string? Department { get; init; }
    }
}
```

- [ ] **Step 4: Create `Variant.Seeder.cs` (CatalogVariantSeeder)**

```csharp
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogVariantSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 132;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Variant>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoVariantJson>("006_demo_variants.json");
        if (json is null)
            return Result.Ok();

        var optionValues = await Context.Set<OptionValue>().ToListAsync(cancellationToken);

        foreach (var vj in json)
        {
            var sku = vj.IsMaster ? $"MASTER-{vj.Sku}" : vj.Sku;
            var variantResult = VariantMethod.Create(
                productId: Guid.Parse(vj.ProductId), sku: sku,
                isMaster: vj.IsMaster, position: vj.Position,
                barcode: vj.Barcode, id: Guid.Parse(vj.Id));
            var variant = variantResult.Value;
            variant.Price = vj.Price;
            variant.HsCode = vj.HsCode;
            variant.Weight = vj.Weight;
            variant.WeightUnit = vj.WeightUnit is null ? null : Enum.Parse<WeightUnit>(vj.WeightUnit);
            variant.Height = vj.Height;
            variant.Width = vj.Width;
            variant.Depth = vj.Depth;
            variant.DimensionsUnit = vj.DimensionsUnit is null ? null : Enum.Parse<DimensionUnit>(vj.DimensionsUnit);
            variant.CostPrice = vj.CostPrice;
            variant.CostCurrency = vj.CostCurrency;

            Context.Set<Variant>().Add(variant);

            var priceResult = PriceMethod.Create(amount: vj.Price, currency: "USD", variantId: variant.Id);
            var price = priceResult.Value!;
            price.IsDefault = true;
            Context.Set<Price>().Add(price);

            foreach (var ov in vj.OptionValues)
            {
                var match = optionValues.FirstOrDefault(v =>
                    v.Name.Equals(ov.OptionValueName, StringComparison.OrdinalIgnoreCase) &&
                    v.OptionTypeId == Guid.Parse(ov.OptionTypeId));
                if (match is null)
                    continue;
                Context.Set<OptionValueVariant>().Add(OptionValueVariantMethod.Create(variant.Id, match.Id).Value);
            }
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoVariantJson
    {
        public string Id { get; init; } = default!;
        public string ProductId { get; init; } = default!;
        public string Sku { get; init; } = default!;
        public bool IsMaster { get; init; }
        public int Position { get; init; }
        public decimal Price { get; init; }
        public string? Barcode { get; init; }
        public string? HsCode { get; init; }
        public decimal? Weight { get; init; }
        public string? WeightUnit { get; init; }
        public decimal? Height { get; init; }
        public decimal? Width { get; init; }
        public decimal? Depth { get; init; }
        public string? DimensionsUnit { get; init; }
        public decimal? CostPrice { get; init; }
        public string? CostCurrency { get; init; }
        public List<DemoVariantOptionJson> OptionValues { get; init; } = [];
    }

    private record DemoVariantOptionJson
    {
        public string OptionTypeId { get; init; } = default!;
        public string OptionValueName { get; init; } = default!;
    }
}
```

- [ ] **Step 5: Create `VariantImage.Seeder.cs` (CatalogVariantImageSeeder)**

```csharp
using Module.Catalog.Domain.Products.Variants.Images;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogVariantImageSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 134;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<VariantImage>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoVariantImageJson>("007_demo_variant_images.json");
        if (json is null)
            return Result.Ok();

        foreach (var img in json)
        {
            var imageId = Guid.Parse(img.Id);
            var type = Enum.TryParse<VariantImageType>(img.Type, true, out var parsedType)
                ? parsedType
                : VariantImageType.Default;
            var imgResult = VariantImageMethod.Create(
                contentType: img.ContentType, fileName: img.FileName,
                fileSize: img.FileSize, url: $"/api/catalog/variant-images/{imageId}/download",
                storagePath: img.StoragePath, position: img.Position, alt: img.Alt,
                type: type, variantId: Guid.Parse(img.VariantId));
            var image = imgResult.Value;
            image.Id = imageId;
            image.Width = img.Width;
            image.Height = img.Height;
            Context.Set<VariantImage>().Add(image);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoVariantImageJson
    {
        public string Id { get; init; } = default!;
        public string VariantId { get; init; } = default!;
        public string ContentType { get; init; } = default!;
        public string FileName { get; init; } = default!;
        public int FileSize { get; init; }
        public int? Width { get; init; }
        public int? Height { get; init; }
        public string StoragePath { get; init; } = default!;
        public int Position { get; init; }
        public string Alt { get; init; } = default!;
        public string Type { get; init; } = default!;
    }
}
```

Note: this correctly maps `Gallery` images to `VariantImageType.Gallery` (the old code mapped everything except `Search` to `Default`).

- [ ] **Step 6: Create `ProductTaxon.Seeder.cs` (CatalogProductTaxonSeeder)**

```csharp
using Module.Catalog.Domain.Products.Classifications;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogProductTaxonSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 136;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Classification>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoClassificationJson>("008_demo_product_taxons.json");
        if (json is null)
            return Result.Ok();

        foreach (var c in json)
        {
            var result = ClassificationMethod.Create(
                Guid.Parse(c.ProductId), Guid.Parse(c.TaxonId),
                c.Position, isAutomatic: true);
            if (result.IsSuccess)
                Context.Set<Classification>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoClassificationJson
    {
        public string ProductId { get; init; } = default!;
        public string TaxonId { get; init; } = default!;
        public int Position { get; init; }
    }
}
```

- [ ] **Step 7: Update `Taxonomy.Seeder.cs`, `Taxon.Seeder.cs`, `Embedding.Seeder.cs`**

`Taxonomy.Seeder.cs` — one-line change:
```csharp
var json = jsonHelper.LoadIfExists<DemoTaxonomyJson>("001_demo_taxonomies.json");
```

`Taxon.Seeder.cs` — filename + enrichment passthrough:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 120;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Taxon>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoTaxonJson>("002_demo_taxons.json");
        if (json is null)
            return Result.Ok();

        var usedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingSlugs = await Context.Set<Taxon>()
            .Select(t => t.Slug)
            .ToListAsync(cancellationToken);
        foreach (var s in existingSlugs)
            usedSlugs.Add(s);

        foreach (var item in json)
        {
            var slug = item.Slug;
            var original = slug;
            int suffix = 2;
            while (!usedSlugs.Add(slug))
            {
                slug = $"{original}-{suffix}";
                suffix++;
            }

            Guid? parentId = string.IsNullOrEmpty(item.ParentId) ? null : Guid.Parse(item.ParentId);
            var result = TaxonMethod.Create(
                taxonomyId: Guid.Parse(item.TaxonomyId), parentId: parentId,
                name: item.Name, presentation: item.Presentation ?? item.Name,
                description: item.Description, position: item.Position,
                slug: slug, metaTitle: item.MetaTitle, metaDescription: item.MetaDescription,
                metaKeywords: item.MetaKeywords,
                automatic: false, rulesMatchPolicy: null, sortOrder: null, hideFromNav: false,
                imageUrl: item.ImageUrl, squareImageUrl: item.SquareImageUrl);

            var taxon = result.Value;
            taxon.Id = Guid.Parse(item.Id);
            taxon.Lft = item.Lft;
            taxon.Rgt = item.Rgt;
            taxon.Depth = item.Depth;
            taxon.Permalink = item.Permalink;
            taxon.PrettyName = item.PrettyName;
            taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
            taxon.CreatedBy = "System";

            Context.Set<Taxon>().Add(taxon);
        }

        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoTaxonJson
    {
        public string Id { get; init; } = default!;
        public string TaxonomyId { get; init; } = default!;
        public string? ParentId { get; init; }
        public string Name { get; init; } = default!;
        public string? Presentation { get; init; }
        public string? Description { get; init; }
        public string Slug { get; init; } = default!;
        public string? MetaTitle { get; init; }
        public string? MetaDescription { get; init; }
        public string? MetaKeywords { get; init; }
        public string? Permalink { get; init; }
        public string? PrettyName { get; init; }
        public string? ImageUrl { get; init; }
        public string? SquareImageUrl { get; init; }
        public int Depth { get; init; }
        public int Lft { get; init; }
        public int Rgt { get; init; }
        public int Position { get; init; }
    }
}
```

`Embedding.Seeder.cs` — two changes:
- `public override int Order => 135;` → `public override int Order => 137;`
- `LoadIfExists<DemoEmbeddingJson>("demo_embeddings.json")` → `LoadIfExists<DemoEmbeddingJson>("012_demo_embeddings.json")`

- [ ] **Step 8: Update `Catalog.Extension.cs` registration**

Replace the registration block:

```csharp
        builder.AddSeeder<CatalogOptionSeeder>();
        builder.AddSeeder<CatalogTaxonomySeeder>();
        builder.AddSeeder<CatalogTaxonSeeder>();
        builder.AddSeeder<CatalogDemoSeeder>();
        builder.AddSeeder<CatalogEmbeddingSeeder>();
```

with:

```csharp
        builder.AddSeeder<CatalogOptionTypeSeeder>();
        builder.AddSeeder<CatalogOptionValueSeeder>();
        builder.AddSeeder<CatalogTaxonomySeeder>();
        builder.AddSeeder<CatalogTaxonSeeder>();
        builder.AddSeeder<CatalogProductSeeder>();
        builder.AddSeeder<CatalogVariantSeeder>();
        builder.AddSeeder<CatalogVariantImageSeeder>();
        builder.AddSeeder<CatalogProductTaxonSeeder>();
        builder.AddSeeder<CatalogEmbeddingSeeder>();
```

- [ ] **Step 9: Delete old seeder files and build**

```bash
git rm service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs
```

Run: `cd service/Api && dotnet build`
Expected: build succeeds with zero warnings.

Run: `dotnet test tests/Module.UnitTests`
Expected: PASS (unit tests don't touch seeders; compile check only).

- [ ] **Step 10: Commit**

```bash
git add service/Api/src/Module/Catalog/
git commit -m "refactor(catalog): split demo seeders into single-entity classes"
```

---
### Task 6: Update Inventory seeder filenames

**Files:**
- Modify: `service/Api/src/Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs`
- Modify: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs`
- Modify: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs`

- [ ] **Step 1: Update the three `LoadIfExists` (local `n<T>`) filenames**

`StockLocation.Seeder.cs`:
```csharp
var json = jsonHelper.LoadIfExists<DemoStockLocationJson>("009_demo_stock_locations.json");
```

`InventoryStockItem.Seeder.cs`:
```csharp
var json = jsonHelper.LoadIfExists<DemoStockItemJson>("010_demo_stock_items.json");
```

`InventoryStockMovement.Seeder.cs`:
```csharp
var json = jsonHelper.LoadIfExists<DemoStockMovementJson>("011_demo_stock_movements.json");
```

Note: the Inventory `DemoJsonHelper` helper method is locally named `n<T>`; update the string argument only — do not rename the method.

- [ ] **Step 2: Build and commit**

Run: `cd service/Api && dotnet build`
Expected: zero warnings.

```bash
git add service/Api/src/Module/Inventory/
git commit -m "refactor(inventory): point seeders at numbered demo datasets"
```

---

### Task 7: Regenerate output datasets, remove legacy files, update docs

**Files:**
- Regenerate: `benchmarks/scripts/demo-seed/output/` numbered JSONs
- Delete: legacy `demo_*.json` files in `benchmarks/scripts/demo-seed/output/`
- Update: any `benchmarks/docs/` or repo docs referencing `demo_*.json` filenames

- [ ] **Step 1: Regenerate all datasets**

Images (`output/images/`) and embeddings (search-image IDs unchanged) are already valid — do NOT rerun `10_process_images.py` or `11_generate_embeddings.py`. Copy the embeddings file to its new name:

```bash
cd benchmarks/scripts/demo-seed
uv run python 01_extract_taxonomies.py --output output --force
uv run python 02_extract_taxons.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 03_extract_option_types.py --output output --force
uv run python 04_extract_option_values.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 05_extract_products.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 06_extract_variants.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 07_extract_variant_images.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 08_extract_product_taxons.py --dataset ../../data/raw/fashion-product-images --output output --force
uv run python 09_extract_stock.py --output output --force
cp output/demo_embeddings.json output/012_demo_embeddings.json
uv run python 12_verify_output.py --output output --count 1000
```

Expected: `VERIFICATION PASSED — all checks OK`.

- [ ] **Step 2: Delete legacy JSON files**

```bash
cd benchmarks/scripts/demo-seed/output
git rm demo_taxonomies.json demo_taxons.json demo_option_types.json demo_option_values.json \
       demo_products.json demo_variants.json demo_variant_images.json demo_option_assignments.json \
       demo_classifications.json demo_stock_locations.json demo_stock_items.json demo_stock_movements.json \
       demo_embeddings.json
```

If any are untracked (not in git), use `rm` for those.

- [ ] **Step 3: Update docs referencing old filenames**

Search the repo (excluding `.venv`, `node_modules`, `output/`, `.git`):

```bash
rg -n "demo_(taxonomies|taxons|option_types|option_values|products|variants|variant_images|option_assignments|classifications|stock_locations|stock_items|stock_movements|embeddings)\.json" benchmarks/docs docs ApiTests service --glob '!**/obj/**' --glob '!**/bin/**'
```

Update every match to the new numbered name (e.g., `demo_taxons.json` → `002_demo_taxons.json`, `demo_option_assignments.json` → remove references, `demo_stock_items.json` → `010_demo_stock_items.json`). If `docs/superpowers/specs/2026-08-03-demo-seeder-refactor-design.md` is the only match, no doc changes are needed.

- [ ] **Step 4: Full verification pass**

Run:
```bash
cd benchmarks && uv run ruff check scripts/demo-seed/ && uv run pytest scripts/demo-seed/tests/ -q
cd service/Api && dotnet build && dotnet test tests/Module.UnitTests
cd benchmarks/scripts/demo-seed && uv run python 12_verify_output.py --output output --count 1000
```
Expected: ruff clean, pytest PASS, dotnet build zero warnings, unit tests PASS, verify PASS.

- [ ] **Step 5: Commit**

```bash
git add -A benchmarks/scripts/demo-seed/output
git add benchmarks/docs 2>/dev/null || true
git commit -m "chore(seed): regenerate numbered demo datasets and remove legacy files"
```

---

## Self-Review Notes

- **Spec coverage:** Section 1 (script layout/orchestration) → Tasks 2-4; Section 2 (file naming + C# split) → Tasks 5-6; Section 3 (variant rules) → Task 1 (`variants.py`) + Task 3 (06 script) + Task 4 (verify invariants); Section 4 (enrichment) → Task 1 (`metadata.py`) + Tasks 2-3 (extractor wiring) + Task 5 (C# passthrough); Section 5 (verification/regeneration) → Task 4 (12_verify) + Task 7.
- **Type consistency:** `generate_variants(product_name, colors, sizes_by_color, max_variants=10) -> list[dict]` with keys `color/size/is_master/position` is used identically in Tasks 1, 3; `master_variant_id(product_name)` (stable) and `variant_id(product_name, color, size)` used in Tasks 3 + verify; `taxon_id(identifier)` full-dotted form used in Tasks 2-3.

