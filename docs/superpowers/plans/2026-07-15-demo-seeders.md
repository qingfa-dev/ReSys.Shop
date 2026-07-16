# Demo Seeders Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace hardcoded demo seeders with JSON-driven seeding from the Kaggle Fashion Product Images dataset via Python ETL scripts.

**Architecture:** Python 3.12 ETL scripts (`benchmarks/scripts/demo-seed/`) read `styles.csv`, extract taxonomies/products/stock, scale images, generate embeddings via the Embedding sidecar, and output domain-aligned JSON files. EF Core seeders read these JSON files with `System.Text.Json`, falling back to existing hardcoded data when JSON is absent.

**Tech Stack:** Python 3.12 (uv, Pillow, pandas, httpx, tqdm), .NET 10 (System.Text.Json, EF Core), Pgvector

## Global Constraints

- Warnings-as-errors global; any warning fails the build
- Result objects, not exceptions; all domain operations return `Result<T>` or `Result`
- Modules never cross-reference; seeders reference Catalog entities from Inventory module (existing pattern, permitted for seeding only)
- Vertial slice feature files; follow static partial class pattern
- Forward-only dependency: Shared depends on nothing, Module depends on Shared
- JSON format uses snake_case property names; C# deserializers use `JsonSerializerOptions` with `PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower`
- All seeders are idempotent — `HasDataAsync<T>()` guard prevents double-seeding
- Python scripts follow existing conventions: `from __future__ import annotations`, `argparse`, `pathlib.Path`, typed `main()` with `if __name__ == "__main__"`
- Model names in embedding JSON use Python sidecar registry IDs (`fashion_clip`, `efficientnet_b0`, `clip_vit_b16`, `dinov2_vits14`)

---

## File Map

| File | Purpose | Action |
|---|---|---|
| `benchmarks/scripts/demo-seed/run_all.py` | Orchestrator | Create |
| `benchmarks/scripts/demo-seed/extract_taxonomies.py` | Taxonomy/option extraction | Create |
| `benchmarks/scripts/demo-seed/extract_products.py` | Product/variant extraction | Create |
| `benchmarks/scripts/demo-seed/process_images.py` | Image scaling/copy | Create |
| `benchmarks/scripts/demo-seed/generate_embeddings.py` | Embedding generation | Create |
| `benchmarks/scripts/demo-seed/extract_stock.py` | Stock data generation | Create |
| `benchmarks/pyproject.toml` | Add `httpx` dependency | Modify |
| `service/Api/src/Module/Module.csproj` | Add Content items for JSON data files | Modify |
| `service/Api/src/Module/Catalog/Persistence/Seeders/Seeder.Json.cs` | Shared JSON deserialization helper | Create |
| `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Module/Catalog/Persistence/Seeders/Taxonomy.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Module/Catalog/Persistence/Seeders/Taxon.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs` | New seeder for image embeddings | Create |
| `service/Api/src/Module/Catalog/Catalog.Extension.cs` | Register `CatalogEmbeddingSeeder` | Modify |
| `service/Api/src/Module/Inventory/Persistence/Seeders/Seeder.Json.cs` | Shared JSON helper (duplicated per module pattern) | Create |
| `service/Api/src/Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs` | Read JSON instead of hardcoded | Modify |
| `service/Api/src/Api/appsettings.Development.json` | Add storage LocalPath override | Modify |

---

## Phase 1: Python ETL Scripts

### Task 1.1: Add `httpx` dependency to benchmarks

**Files:**
- Modify: `benchmarks/pyproject.toml`

- [ ] **Step 1: Add `httpx` to dependencies**

Append `"httpx>=0.24.0",` to the `dependencies` list in `[project]` section. Add it alphabetically after `"faiss-cpu>=1.8",`:

```toml
"httpx>=0.24.0",
```

- [ ] **Step 2: Sync dependencies**

```bash
uv sync --extra dev
```

Expected: `httpx` installed into `.venv`, `uv.lock` updated.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/pyproject.toml benchmarks/uv.lock
git commit -m "build(benchmarks): add httpx dependency for embedding API calls"
```

---

### Task 1.2: Create `extract_taxonomies.py`

**Files:**
- Create: `benchmarks/scripts/demo-seed/extract_taxonomies.py`

**Interfaces:**
- Consumes: `styles.csv` (from Kaggle dataset)
- Produces: `demo_taxonomies.json`, `demo_taxons.json`, `demo_option_types.json`, `demo_option_values.json`

- [ ] **Step 1: Create directory**

```bash
mkdir -p benchmarks/scripts/demo-seed
```

- [ ] **Step 2: Write the script**

```python
#!/usr/bin/env python
"""Extract taxonomies, taxons, and option types/values from styles.csv."""
from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path
from uuid import uuid5, NAMESPACE_DNS

SEED_NAMESPACE = uuid5(NAMESPACE_DNS, "resys.shop.demo-seed")

TAXONOMY_CATEGORIES_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.categories"))
TAXONOMY_BRANDS_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.brands"))
TAXONOMY_ARTICLE_TYPES_ID = str(uuid5(SEED_NAMESPACE, "taxonomy.article_types"))

OPTION_TYPE_SIZE_ID = str(uuid5(SEED_NAMESPACE, "option_type.size"))
OPTION_TYPE_COLOR_ID = str(uuid5(SEED_NAMESPACE, "option_type.color"))


def guid(entity_type: str, name: str) -> str:
    return str(uuid5(SEED_NAMESPACE, f"{entity_type}.{name}"))


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
    lft = 1

    root_cat_id = guid("taxon", "categories_root")
    lft += 1  # skip root lft

    for master_cat in sorted(master_categories):
        mc_id = guid("taxon", f"cat.{master_cat}")
        mc_slug = master_cat.lower().replace(" ", "-").replace("&", "and")
        mc_lft = lft
        lft += 1
        for sub_cat in sorted(sub_categories.get(master_cat, set())):
            sc_id = guid("taxon", f"cat.{master_cat}.{sub_cat}")
            sc_slug = sub_cat.lower().replace(" ", "-").replace("&", "and")
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
    taxons.append({
        "id": root_brand_id, "taxonomy_id": TAXONOMY_BRANDS_ID,
        "parent_id": None, "name": "Brands", "presentation": "All Brands",
        "slug": "brands", "depth": 0, "lft": 1, "position": 0,
    })
    brand_lft = 2
    for brand in sorted(brands):
        b_id = guid("taxon", f"brand.{brand}")
        b_slug = brand.lower().replace(" ", "-").replace("&", "and").replace(",", "")
        taxons.append({
            "id": b_id, "taxonomy_id": TAXONOMY_BRANDS_ID,
            "parent_id": root_brand_id, "name": brand,
            "presentation": brand, "slug": b_slug,
            "depth": 1, "lft": brand_lft, "rgt": brand_lft + 1, "position": 0,
        })
        brand_lft += 2
    taxons.append({"_update_parent": root_brand_id, "rgt": brand_lft})

    root_at_id = guid("taxon", "article_types_root")
    taxons.append({
        "id": root_at_id, "taxonomy_id": TAXONOMY_ARTICLE_TYPES_ID,
        "parent_id": None, "name": "Article Types", "presentation": "All Article Types",
        "slug": "article-types", "depth": 0, "lft": 1, "position": 0,
    })
    at_lft = 2
    for atype in sorted(article_types):
        at_id = guid("taxon", f"article_type.{atype}")
        at_slug = atype.lower().replace(" ", "-").replace("&", "and")
        taxons.append({
            "id": at_id, "taxonomy_id": TAXONOMY_ARTICLE_TYPES_ID,
            "parent_id": root_at_id, "name": atype,
            "presentation": atype, "slug": at_slug,
            "depth": 1, "lft": at_lft, "rgt": at_lft + 1, "position": 0,
        })
        at_lft += 2
    taxons.append({"_update_parent": root_at_id, "rgt": at_lft})

    return taxons


def build_option_types_json() -> list[dict]:
    return [
        {"id": OPTION_TYPE_SIZE_ID, "name": "Size", "presentation": "Size", "position": 0, "filterable": True},
        {"id": OPTION_TYPE_COLOR_ID, "name": "Color", "presentation": "Color", "position": 1, "filterable": True},
    ]


def build_option_values_json(colors: set[str]) -> list[dict]:
    values: list[dict] = []
    pos = 0
    for color in sorted(colors):
        values.append({
            "id": guid("option_value", f"color.{color}"),
            "option_type_id": OPTION_TYPE_COLOR_ID,
            "name": color, "presentation": color, "position": pos,
        })
        pos += 1
    return values
    # Size values are generated in extract_products.py from JSON articleAttributes


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract taxonomy seed data from styles.csv")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, required=True, help="Output directory for JSON files")
    args = parser.parse_args()

    styles_csv = args.dataset / "styles.csv"
    if not styles_csv.exists():
        print(f"ERROR: {styles_csv} not found"); return

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

    (args.output / "demo_taxonomies.json").write_text(
        json.dumps(build_taxonomies_json(), indent=2))
    (args.output / "demo_taxons.json").write_text(
        json.dumps(build_taxons_json(master_categories, sub_categories, brands, article_types), indent=2))
    (args.output / "demo_option_types.json").write_text(
        json.dumps(build_option_types_json(), indent=2))
    (args.output / "demo_option_values.json").write_text(
        json.dumps(build_option_values_json(colors), indent=2))

    print(f"Written taxonomies/taxons/options to {args.output}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 3: Verify it runs**

```bash
uv run python benchmarks/scripts/demo-seed/extract_taxonomies.py \
  --dataset benchmarks/data/raw/fashion-product-images \
  --output /tmp/demo-seed-test
```

Expected: 4 JSON files created in `/tmp/demo-seed-test/`. Check:
```bash
python -c "import json; d=json.load(open('/tmp/demo-seed-test/demo_taxons.json')); print(f'Taxons: {len(d)}')"
python -c "import json; d=json.load(open('/tmp/demo-seed-test/demo_option_values.json')); print(f'Colors: {len(d)}')"
```

- [ ] **Step 4: Commit**

```bash
git add benchmarks/scripts/demo-seed/extract_taxonomies.py
git commit -m "feat: add taxonomy extraction ETL script"
```

---

### Task 1.3: Create `extract_products.py`

**Files:**
- Create: `benchmarks/scripts/demo-seed/extract_products.py`

**Interfaces:**
- Consumes: `styles.csv`, JSON files in `styles/<id>.json` (from Kaggle dataset)
- Produces: `demo_products.json`, `demo_variants.json`, `demo_variant_images.json`, `demo_option_assignments.json`, appends size values to `demo_option_values.json`

- [ ] **Step 1: Write the script**

```python
#!/usr/bin/env python
"""Extract products, variants, and image metadata from styles.csv."""
from __future__ import annotations

import argparse
import csv
import json
import random
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
        print(f"ERROR: {styles_csv} not found"); return

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
```

- [ ] **Step 2: Verify with real data**

```bash
uv run python benchmarks/scripts/demo-seed/extract_taxonomies.py \
  --dataset benchmarks/data/raw/fashion-product-images \
  --output /tmp/demo-seed-test

uv run python benchmarks/scripts/demo-seed/extract_products.py \
  --dataset benchmarks/data/raw/fashion-product-images \
  --output /tmp/demo-seed-test \
  --count 20

python -c "import json; p=json.load(open('/tmp/demo-seed-test/demo_products.json')); v=json.load(open('/tmp/demo-seed-test/demo_variants.json')); print(f'{len(p)} products, {len(v)} variants')"
```

Expected: products > 0, variants >= products.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/scripts/demo-seed/extract_products.py
git commit -m "feat: add product/variant extraction ETL script"
```

---

### Task 1.4: Create `process_images.py`

**Files:**
- Create: `benchmarks/scripts/demo-seed/process_images.py`

**Interfaces:**
- Consumes: `images/<id>.jpg` from dataset, `demo_variant_images.json` for image list
- Produces: scaled JPEGs in `infra/Storage/demo/images/medium/` and `infra/Storage/demo/images/search/{size}/`

- [ ] **Step 1: Write the script**

```python
#!/usr/bin/env python
"""Scale product images to medium (512px) and search (224px) sizes."""
from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image
from tqdm import tqdm


def resize_image(src: Path, dst: Path, size: int) -> bool:
    try:
        img = Image.open(src).convert("RGB")
        img.thumbnail((size, size), Image.LANCZOS)
        dst.parent.mkdir(parents=True, exist_ok=True)
        img.save(dst, "JPEG", quality=85, optimize=True)
        return True
    except Exception as e:
        print(f"  WARN: Cannot process {src.name}: {e}")
        return False


def main() -> None:
    parser = argparse.ArgumentParser(description="Scale product images for demo")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--storage", type=Path, required=True, help="Path to infra/Storage/demo")
    parser.add_argument("--json-dir", type=Path, required=True, help="Directory with demo_variant_images.json")
    args = parser.parse_args()

    images_json = args.json_dir / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run extract_products.py first"); return

    image_records = json.loads(images_json.read_text())
    # Deduplicate by storage path
    seen: set[str] = set()
    unique: list[dict] = []
    for rec in image_records:
        sp = rec["storage_path"]
        if sp not in seen:
            seen.add(sp)
            unique.append(rec)

    source_dir = args.dataset / "images"
    medium_dir = args.storage / "images" / "medium"
    search_dir_224 = args.storage / "images" / "search" / "224"

    ok = fail = 0
    for rec in tqdm(unique, desc="Processing images"):
        fname = rec["file_name"]
        src = source_dir / fname
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue

        if "medium" in rec["storage_path"]:
            dst = args.storage / rec["storage_path"]
            if resize_image(src, dst, 512):
                ok += 1
            else:
                fail += 1
        elif "search" in rec["storage_path"]:
            dst = args.storage / rec["storage_path"]
            if resize_image(src, dst, 224):
                ok += 1
            else:
                fail += 1

    print(f"Done: {ok} images processed, {fail} failures")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Verify**

```bash
uv run python benchmarks/scripts/demo-seed/process_images.py \
  --dataset benchmarks/data/raw/fashion-product-images \
  --storage infra/Storage/demo \
  --json-dir /tmp/demo-seed-test

ls infra/Storage/demo/images/medium/ | head -5
```

Expected: scaled JPEGs in `infra/Storage/demo/images/medium/`.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/scripts/demo-seed/process_images.py
git commit -m "feat: add image processing ETL script"
```

---

### Task 1.5: Create `generate_embeddings.py`

**Files:**
- Create: `benchmarks/scripts/demo-seed/generate_embeddings.py`

**Interfaces:**
- Consumes: `demo_variant_images.json` (search-type images), Embedding sidecar at `http://localhost:8000`
- Produces: `demo_embeddings.json`

- [ ] **Step 1: Write the script**

```python
#!/usr/bin/env python
"""Generate image embeddings via the Embedding sidecar for search-type images."""
from __future__ import annotations

import argparse
import json
from pathlib import Path
from urllib.parse import urljoin

import httpx
from tqdm import tqdm

API_KEY = "dev-key-must-be-long-enough"


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate image embeddings")
    parser.add_argument("--json-dir", type=Path, required=True, help="Directory with demo_variant_images.json")
    parser.add_argument("--storage", type=Path, required=True, help="Path to infra/Storage/demo")
    parser.add_argument("--base-url", default="http://localhost:8000", help="Embedding service URL")
    args = parser.parse_args()

    images_json = args.json_dir / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found"); return

    records = json.loads(images_json.read_text())
    search_records = [r for r in records if r.get("type") == "Search"]

    headers = {"X-API-Key": API_KEY}

    embeddings: list[dict] = []
    for rec in tqdm(search_records, desc="Generating embeddings"):
        storage_path = rec["storage_path"]
        image_path = args.storage / storage_path
        if not image_path.exists():
            print(f"  WARN: {image_path} not found, skipping")
            continue

        model_name = "fashion_clip"
        try:
            with open(image_path, "rb") as f:
                files = {"image": (image_path.name, f, "image/jpeg")}
                data = {"model": model_name}
                resp = httpx.post(
                    urljoin(args.base_url, "/embeddings/bytes"),
                    headers=headers, files=files, data=data, timeout=30,
                )
            if resp.status_code != 200:
                print(f"  WARN: Embedding API returned {resp.status_code} for {storage_path}")
                continue
            result = resp.json()
            if not result.get("isSuccess"):
                print(f"  WARN: Embedding failed for {storage_path}: {result.get('errors')}")
                continue
            value = result["value"]
            embeddings.append({
                "variant_image_id": rec["id"],
                "model_name": model_name,
                "model_version": value["model_version"],
                "vector": value["vector"],
                "dimensions": value["dimension"],
            })
        except httpx.ConnectError:
            print("ERROR: Cannot connect to embedding service. Is it running?")
            print("  Start with: cd service/Embedding && uv run python src/main.py")
            return
        except Exception as e:
            print(f"  WARN: {storage_path}: {e}")
            continue

    (args.json_dir / "demo_embeddings.json").write_text(json.dumps(embeddings, indent=2))
    print(f"Written {len(embeddings)} embeddings")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Verify (skip if no sidecar)**

```bash
uv run python benchmarks/scripts/demo-seed/generate_embeddings.py \
  --json-dir /tmp/demo-seed-test \
  --storage infra/Storage/demo \
  --base-url http://localhost:8000
```

If Embedding service is not running, script should print "Cannot connect" and exit cleanly.
If running, should produce `demo_embeddings.json`.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/scripts/demo-seed/generate_embeddings.py
git commit -m "feat: add embedding generation ETL script"
```

---

### Task 1.6: Create `extract_stock.py`

**Files:**
- Create: `benchmarks/scripts/demo-seed/extract_stock.py`

**Interfaces:**
- Consumes: `demo_variants.json`
- Produces: `demo_stock_locations.json`, `demo_stock_items.json`, `demo_stock_movements.json`

- [ ] **Step 1: Write the script**

```python
#!/usr/bin/env python
"""Generate stock location, stock item, and stock movement seed data."""
from __future__ import annotations

import argparse
import json
import random
from pathlib import Path
from uuid import uuid5, NAMESPACE_DNS

SEED_NAMESPACE = uuid5(NAMESPACE_DNS, "resys.shop.demo-seed")


def guid(entity_type: str, name: str) -> str:
    return str(uuid5(SEED_NAMESPACE, f"{entity_type}.{name}"))


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract stock seed data")
    parser.add_argument("--json-dir", type=Path, required=True, help="Directory with demo_variants.json")
    parser.add_argument("--seed", type=int, default=42, help="Random seed")
    args = parser.parse_args()

    variants_json = args.json_dir / "demo_variants.json"
    if not variants_json.exists():
        print(f"ERROR: {variants_json} not found"); return

    random.seed(args.seed)
    variants = json.loads(variants_json.read_text())

    locations = [
        {
            "id": guid("stock_location", "main_warehouse"),
            "name": "Main Warehouse",
            "presentation": "Main Warehouse",
            "code": "MAIN",
            "is_default": True, "active": True,
            "address1": "123 Commerce Blvd", "city": "New York",
            "postal_code": "10001", "phone": "+12025550100",
            "backorderable_default": True, "propagate_all_variants": True,
            "position": 0, "country_iso": "US",
        },
        {
            "id": guid("stock_location", "east_distribution"),
            "name": "East Distribution",
            "presentation": "East Distribution",
            "code": "EAST",
            "is_default": False, "active": True,
            "address1": "456 Peachtree St", "city": "Atlanta",
            "postal_code": "30301", "phone": "+14045550100",
            "backorderable_default": True, "propagate_all_variants": False,
            "position": 1, "country_iso": "US",
        },
        {
            "id": guid("stock_location", "express_hub"),
            "name": "Express Hub",
            "presentation": "Express Hub",
            "code": "EXPRESS",
            "is_default": False, "active": True,
            "address1": "789 Sunset Blvd", "city": "Los Angeles",
            "postal_code": "90001", "phone": "+13105550100",
            "backorderable_default": False, "propagate_all_variants": False,
            "position": 2, "country_iso": "US",
        },
    ]

    stock_items: list[dict] = []
    stock_movements: list[dict] = []

    for variant in variants:
        is_master = variant.get("is_master", False)
        base_qty = random.randint(0, 5) if is_master else random.randint(10, 200)
        ratios = {"MAIN": 1.0, "EAST": 0.4, "EXPRESS": 0.25}

        for loc in locations:
            qty = int(base_qty * ratios[loc["code"]])
            if qty <= 0:
                continue
            si_id = guid("stock_item", f"{variant['sku']}.{loc['code']}")
            stock_items.append({
                "id": si_id,
                "variant_id": variant["id"],
                "stock_location_code": loc["code"],
                "count_on_hand": qty,
                "backorderable": qty > 0,
            })
            stock_movements.append({
                "variant_id": variant["id"],
                "stock_location_code": loc["code"],
                "quantity": qty,
                "previous_count_on_hand": 0,
                "originator_type": "Adjustment",
                "reason": "Initial stock seeding",
                "action": "restock",
            })

    (args.json_dir / "demo_stock_locations.json").write_text(json.dumps(locations, indent=2))
    (args.json_dir / "demo_stock_items.json").write_text(json.dumps(stock_items, indent=2))
    (args.json_dir / "demo_stock_movements.json").write_text(json.dumps(stock_movements, indent=2))

    print(f"Written {len(locations)} locations, {len(stock_items)} items, {len(stock_movements)} movements")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Verify**

```bash
uv run python benchmarks/scripts/demo-seed/extract_stock.py --json-dir /tmp/demo-seed-test
python -c "import json; l=json.load(open('/tmp/demo-seed-test/demo_stock_locations.json')); i=json.load(open('/tmp/demo-seed-test/demo_stock_items.json')); print(f'{len(l)} locations, {len(i)} stock items')"
```

- [ ] **Step 3: Commit**

```bash
git add benchmarks/scripts/demo-seed/extract_stock.py
git commit -m "feat: add stock data generation ETL script"
```

---

### Task 1.7: Create `run_all.py` orchestrator

**Files:**
- Create: `benchmarks/scripts/demo-seed/run_all.py`

**Interfaces:**
- Consumes: all scripts from Tasks 1.2-1.6
- Produces: all JSON output files + scaled images + embeddings

- [ ] **Step 1: Write the script**

```python
#!/usr/bin/env python
"""Orchestrate all demo seed ETL steps."""
from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

SCRIPTS_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPTS_DIR.parent.parent


def run_step(name: str, args: list[str]) -> int:
    print(f"\n{'='*60}\n  STEP: {name}\n{'='*60}")
    cmd = [sys.executable, str(SCRIPTS_DIR / name)] + args
    result = subprocess.run(cmd)
    return result.returncode


def main() -> None:
    parser = argparse.ArgumentParser(description="Run the full demo seed ETL pipeline")
    parser.add_argument("--count", type=int, default=200, help="Target number of product groups")
    parser.add_argument("--dataset", type=Path,
                        default=REPO_ROOT / "benchmarks" / "data" / "raw" / "fashion-product-images")
    parser.add_argument("--output", type=Path,
                        default=REPO_ROOT / "service" / "Api" / "src" / "Module" / "Catalog" / "Persistence" / "Seeders" / "Data")
    parser.add_argument("--storage", type=Path,
                        default=REPO_ROOT / "infra" / "Storage" / "demo")
    parser.add_argument("--base-url", default="http://localhost:8000")
    parser.add_argument("--skip-embeddings", action="store_true")
    args = parser.parse_args()

    steps = [
        ("extract_taxonomies.py", ["--dataset", str(args.dataset), "--output", str(args.output)]),
        ("extract_products.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)]),
        ("process_images.py", ["--dataset", str(args.dataset), "--storage", str(args.storage), "--json-dir", str(args.output)]),
    ]

    if not args.skip_embeddings:
        steps.append(("generate_embeddings.py", ["--json-dir", str(args.output), "--storage", str(args.storage), "--base-url", args.base_url]))

    steps.append(("extract_stock.py", ["--json-dir", str(args.output)]))

    for script_name, script_args in steps:
        rc = run_step(script_name, script_args)
        if rc != 0 and script_name != "generate_embeddings.py":
            print(f"\nERROR: {script_name} failed with code {rc}")
            sys.exit(rc)

    print(f"\nDone. JSON data written to {args.output}")
    print(f"Images written to {args.storage}")
    print("Next: run the .NET app to import seed data.")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Dry-run verify**

```bash
uv run python benchmarks/scripts/demo-seed/run_all.py --count 5 --skip-embeddings --output /tmp/demo-seed-runall
ls /tmp/demo-seed-runall/
```

Expected: all JSON files present.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/scripts/demo-seed/run_all.py
git commit -m "feat: add ETL orchestrator script"
```

---

## Phase 2: C# Infrastructure

### Task 2.1: Add JSON content files to Module.csproj

**Files:**
- Modify: `service/Api/src/Module/Module.csproj`

- [ ] **Step 1: Add `<Content>` item group**

Add after the `<InternalsVisibleTo>` block:

```xml
<ItemGroup>
  <Content Include="Catalog\Persistence\Seeders\Data\*.json" CopyToOutputDirectory="PreserveNewest" LinkBase="Seeders\Data\" Condition="Exists('Catalog\Persistence\Seeders\Data\*.json')" />
  <Content Include="Inventory\Persistence\Seeders\Data\*.json" CopyToOutputDirectory="PreserveNewest" LinkBase="Seeders\Data\" Condition="Exists('Inventory\Persistence\Seeders\Data\*.json')" />
</ItemGroup>
```

- [ ] **Step 2: Create empty Data directories**

```bash
mkdir -p service/Api/src/Module/Catalog/Persistence/Seeders/Data
mkdir -p service/Api/src/Module/Inventory/Persistence/Seeders/Data
```

- [ ] **Step 3: Verify build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

Expected: Build passes (warnings-as-errors).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Module.csproj
git commit -m "build: add JSON content files for demo seeders"
```

---

### Task 2.2: Create Catalog JSON seeder helper

**Files:**
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/Seeder.Json.cs`

**Interfaces:**
- Produces: `DemoJsonHelper.LoadIfExists<T>(string fileName)` — returns `T[]?` or null if file missing

- [ ] **Step 1: Write the helper**

```csharp
using System.Text.Json;

namespace Module.Catalog.Persistence.Seeders;

public static class DemoJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static T[]? LoadIfExists<T>(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var fullPath = Path.Combine(basePath, "Seeders", "Data", fileName);
        if (!File.Exists(fullPath))
            return null;

        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T[]>(json, JsonOptions);
    }

    public static string ResolveDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Seeders", "Data", fileName);
    }
}
```

- [ ] **Step 2: Verify build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Persistence/Seeders/Seeder.Json.cs
git commit -m "feat: add JSON deserialization helper for demo seeders"
```

---

## Phase 3: C# Catalog Seeders

### Task 3.1: Update `CatalogOptionSeeder` to support JSON

**Files:**
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs`

**New internal shape — JSON record types:**

```csharp
private record DemoOptionTypeJson(string Id, string Name, string Presentation, int Position, bool Filterable);
private record DemoOptionValueJson(string Id, string OptionTypeId, string Name, string Presentation, int Position);
```

- [ ] **Step 1: Read existing file**

```bash
cat service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs
```

- [ ] **Step 2: Write updated seeder**

Replace the file content with:

```csharp
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasOptionTypes = await HasDataAsync<OptionType>(cancellationToken);
        if (hasOptionTypes)
            return Result.Ok();

        var jsonTypes = DemoJsonHelper.LoadIfExists<DemoOptionTypeJson>("demo_option_types.json");
        var jsonValues = DemoJsonHelper.LoadIfExists<DemoOptionValueJson>("demo_option_values.json");

        if (jsonTypes is not null && jsonValues is not null)
        {
            await SeedFromJsonAsync(jsonTypes, jsonValues, cancellationToken);
            return Result.Ok();
        }

        await SeedHardcodedAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(
        DemoOptionTypeJson[] types, DemoOptionValueJson[] values, CancellationToken ct)
    {
        foreach (var t in types)
        {
            var result = OptionTypeMethod.Create(
                name: t.Name, presentation: t.Presentation,
                position: t.Position, filterable: t.Filterable,
                id: Guid.Parse(t.Id));
            Context.Set<OptionType>().Add(result.Value);
        }
        await Context.SaveChangesAsync(ct);

        foreach (var v in values)
        {
            var result = OptionValueExtensions.Create(
                optionTypeId: Guid.Parse(v.OptionTypeId),
                name: v.Name, presentation: v.Presentation, position: v.Position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await Context.SaveChangesAsync(ct);
    }

    private async Task SeedHardcodedAsync(CancellationToken ct)
    {
        var sizeResult = OptionTypeMethod.Create(name: "Size", presentation: "Size", position: 0, filterable: true, id: Guid.NewGuid());
        var colorResult = OptionTypeMethod.Create(name: "Color", presentation: "Color", position: 1, filterable: true, id: Guid.NewGuid());
        var size = sizeResult.Value;
        var color = colorResult.Value;
        Context.Set<OptionType>().AddRange(size, color);
        await Context.SaveChangesAsync(ct);

        var sizeValues = new (string Name, string Presentation, int Position)[]
            { ("S", "S", 0), ("M", "M", 1), ("L", "L", 2), ("XL", "XL", 3) };
        var colorValues = new (string Name, string Presentation, int Position)[]
            { ("Red", "Red", 0), ("Blue", "Blue", 1), ("Green", "Green", 2),
              ("Black", "Black", 3), ("White", "White", 4), ("Yellow", "Yellow", 5), ("Purple", "Purple", 6) };

        foreach (var (name, presentation, position) in sizeValues)
        {
            var result = OptionValueExtensions.Create(optionTypeId: size.Id, name: name, presentation: presentation, position: position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        foreach (var (name, presentation, position) in colorValues)
        {
            var result = OptionValueExtensions.Create(optionTypeId: color.Id, name: name, presentation: presentation, position: position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await Context.SaveChangesAsync(ct);
    }

    private record DemoOptionTypeJson(string Id, string Name, string Presentation, int Position, bool Filterable);
    private record DemoOptionValueJson(string Id, string OptionTypeId, string Name, string Presentation, int Position);
}
```

- [ ] **Step 3: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs
git commit -m "feat(Catalog): add JSON-driven option seeding with hardcoded fallback"
```

---

### Task 3.2: Update `CatalogTaxonomySeeder` to support JSON

**Files:**
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Taxonomy.Seeder.cs`

- [ ] **Step 1: Write updated seeder**

```csharp
using Module.Catalog.Domain.Taxonomies;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonomySeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 110;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasTaxonomies = await HasDataAsync<Taxonomy>(cancellationToken);
        if (hasTaxonomies)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoTaxonomyJson>("demo_taxonomies.json");
        if (json is not null)
        {
            foreach (var t in json)
            {
                var result = TaxonomyExtensions.Create(
                    name: t.Name, presentation: t.Presentation,
                    position: t.Position, id: Guid.Parse(t.Id));
                Context.Set<Taxonomy>().Add(result.Value);
            }
            await Context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var categoriesResult = TaxonomyExtensions.Create(name: "Categories", presentation: "Departments", position: 0, id: Guid.NewGuid());
        var brandsResult = TaxonomyExtensions.Create(name: "Brands", presentation: "Brands", position: 1, id: Guid.NewGuid());
        Context.Set<Taxonomy>().AddRange(categoriesResult.Value, brandsResult.Value);
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoTaxonomyJson(string Id, string Name, string Presentation, int Position);
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj && git add service/Api/src/Module/Catalog/Persistence/Seeders/Taxonomy.Seeder.cs && git commit -m "feat(Catalog): add JSON-driven taxonomy seeding"
```

---

### Task 3.3: Update `CatalogTaxonSeeder` to support JSON

**Files:**
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Taxon.Seeder.cs`

- [ ] **Step 1: Write updated seeder**

```csharp
using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 120;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasTaxons = await HasDataAsync<Taxon>(cancellationToken);
        if (hasTaxons)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoTaxonJson>("demo_taxons.json");
        if (json is not null)
        {
            await SeedFromJsonAsync(json, cancellationToken);
            return Result.Ok();
        }

        await SeedHardcodedAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(DemoTaxonJson[] items, CancellationToken ct)
    {
        var taxonMap = new Dictionary<string, Taxon>();
        var pendingRgt = new Dictionary<string, int>();

        foreach (var item in items)
        {
            Guid? parentId = string.IsNullOrEmpty(item.ParentId) ? null : Guid.Parse(item.ParentId);
            var result = TaxonMethod.Create(
                taxonomyId: Guid.Parse(item.TaxonomyId), parentId: parentId,
                name: item.Name, presentation: item.Presentation ?? item.Name,
                description: null, position: item.Position,
                slug: item.Slug, metaTitle: null, metaDescription: null, metaKeywords: null,
                automatic: false, rulesMatchPolicy: null, sortOrder: null, hideFromNav: false,
                imageUrl: null, squareImageUrl: null);

            var taxon = result.Value;
            taxon.Id = Guid.Parse(item.Id);
            taxon.Lft = item.Lft;
            taxon.Rgt = item.Rgt;
            taxon.Depth = item.Depth;
            taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
            taxon.CreatedBy = "System";

            taxonMap[item.Id] = taxon;
            Context.Set<Taxon>().Add(taxon);
        }

        await Context.SaveChangesAsync(ct);
    }

    private async Task SeedHardcodedAsync(CancellationToken ct)
    {
        var categoriesTaxonomy = await Context.Set<Taxonomy>().FirstOrDefaultAsync(t => t.Name == "Categories", ct);
        var brandsTaxonomy = await Context.Set<Taxonomy>().FirstOrDefaultAsync(t => t.Name == "Brands", ct);
        if (categoriesTaxonomy is null || brandsTaxonomy is null) return;

        var rootCategories = CreateTaxon(categoriesTaxonomy.Id, null, "Categories", "All Categories", "categories", 1, 8, 0);
        var men = CreateTaxon(categoriesTaxonomy.Id, rootCategories.Id, "Men", "Men", "men", 2, 3, 1);
        var women = CreateTaxon(categoriesTaxonomy.Id, rootCategories.Id, "Women", "Women", "women", 4, 5, 1);
        var accessories = CreateTaxon(categoriesTaxonomy.Id, rootCategories.Id, "Accessories", "Accessories", "accessories", 6, 7, 1);

        var rootBrands = CreateTaxon(brandsTaxonomy.Id, null, "Brands", "All Brands", "brands", 1, 12, 0);
        var nike = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Nike", "Nike", "nike", 2, 3, 1);
        var adidas = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Adidas", "Adidas", "adidas", 4, 5, 1);
        var zara = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Zara", "Zara", "zara", 6, 7, 1);
        var hm = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "H&M", "H&M", "h-m", 8, 9, 1);
        var uniqlo = CreateTaxon(brandsTaxonomy.Id, rootBrands.Id, "Uniqlo", "Uniqlo", "uniqlo", 10, 11, 1);

        Context.Set<Taxon>().AddRange(rootCategories, men, women, accessories, rootBrands, nike, adidas, zara, hm, uniqlo);
        await Context.SaveChangesAsync(ct);
    }

    private static Taxon CreateTaxon(Guid taxonomyId, Guid? parentId, string name, string presentation, string slug, int lft, int rgt, int depth)
    {
        var result = TaxonMethod.Create(taxonomyId, parentId, name, presentation, null, 0, slug, null, null, null, false, null, null, false, null, null);
        var taxon = result.Value;
        taxon.Lft = lft; taxon.Rgt = rgt; taxon.Depth = depth;
        taxon.CreatedAtUtc = DateTimeOffset.UtcNow; taxon.CreatedBy = "System";
        return taxon;
    }

    private record DemoTaxonJson(string Id, string TaxonomyId, string? ParentId, string Name, string? Presentation,
        string Slug, int Depth, int Lft, int Rgt, int Position);
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj && git add service/Api/src/Module/Catalog/Persistence/Seeders/Taxon.Seeder.cs && git commit -m "feat(Catalog): add JSON-driven taxon seeding"
```

---

### Task 3.4: Update `CatalogDemoSeeder` to support JSON

**Files:**
- Modify: `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs`

- [ ] **Step 1: Write updated seeder**

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Domain.Products.Variants.Prices;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogDemoSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 130;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasProducts = await HasDataAsync<Product>(cancellationToken);
        if (hasProducts)
            return Result.Ok();

        var jsonProducts = DemoJsonHelper.LoadIfExists<DemoProductJson>("demo_products.json");
        var jsonVariants = DemoJsonHelper.LoadIfExists<DemoVariantJson>("demo_variants.json");
        var jsonImages = DemoJsonHelper.LoadIfExists<DemoVariantImageJson>("demo_variant_images.json");
        var jsonAssignments = DemoJsonHelper.LoadIfExists<DemoOptionAssignmentJson>("demo_option_assignments.json");

        if (jsonProducts is not null && jsonVariants is not null)
        {
            await SeedFromJsonAsync(jsonProducts, jsonVariants, jsonImages, jsonAssignments, cancellationToken);
            return Result.Ok();
        }

        await SeedHardcodedAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(
        DemoProductJson[] products, DemoVariantJson[] variants,
        DemoVariantImageJson[]? images, DemoOptionAssignmentJson[]? assignments, CancellationToken ct)
    {
        var optionValues = await Context.Set<OptionValue>().ToListAsync(ct);
        var optionTypes = await Context.Set<OptionType>().ToListAsync(ct);

        var colorTypeId = optionTypes.FirstOrDefault(o => o.Name == "Color")?.Id;
        var sizeTypeId = optionTypes.FirstOrDefault(o => o.Name == "Size")?.Id;

        var taxonLookup = await Context.Set<Taxon>()
            .Where(t => !t.IsDeleted).ToDictionaryAsync(t => t.Slug, ct);

        foreach (var pj in products)
        {
            var productResult = ProductMethod.Create(
                name: pj.Name, slug: pj.Slug, description: pj.Description,
                status: ProductStatus.Active, availableOn: DateTimeOffset.UtcNow,
                metaTitle: pj.MetaTitle, metaDescription: pj.Description,
                metaKeywords: pj.MetaKeywords, id: Guid.Parse(pj.Id));
            var product = productResult.Value;
            product.GenderTarget = pj.GenderTarget;

            product.MasterVariantId = Guid.Parse(pj.MasterVariantId);

            Context.Set<Product>().Add(product);

            if (colorTypeId is not null && sizeTypeId is not null)
            {
                var potColor = ProductOptionTypeMethod.Create(product.Id, colorTypeId.Value, 0);
                Context.Set<ProductOptionType>().Add(potColor.Value);
                var potSize = ProductOptionTypeMethod.Create(product.Id, sizeTypeId.Value, 1);
                Context.Set<ProductOptionType>().Add(potSize.Value);
            }
        }
        await Context.SaveChangesAsync(ct);

        foreach (var vj in variants)
        {
            var variantResult = VariantMethod.Create(
                productId: Guid.Parse(vj.ProductId), sku: vj.Sku,
                isMaster: vj.IsMaster, position: vj.Position,
                barcode: vj.Barcode, id: Guid.Parse(vj.Id));
            var variant = variantResult.Value;
            variant.Price = vj.Price;

            var priceResult = PriceMethod.Create(amount: vj.Price, currency: "USD", variantId: variant.Id);
            var price = priceResult.Value!;
            price.IsDefault = true;

            Context.Set<Variant>().Add(variant);
            Context.Set<Price>().Add(price);
        }
        await Context.SaveChangesAsync(ct);

        if (images is not null)
        {
            foreach (var img in images)
            {
                var type = img.Type == "Search" ? VariantImageType.Search : VariantImageType.Default;
                var imgResult = VariantImageMethod.Create(
                    contentType: img.ContentType, fileName: img.FileName,
                    fileSize: 1, url: string.Empty, storagePath: img.StoragePath,
                    position: img.Position, alt: img.Alt, type: type,
                    variantId: Guid.Parse(img.VariantId));
                var image = imgResult.Value;
                image.Id = Guid.Parse(img.Id);
                Context.Set<VariantImage>().Add(image);
            }
            await Context.SaveChangesAsync(ct);
        }

        if (assignments is not null)
        {
            foreach (var a in assignments)
            {
                var ov = optionValues.FirstOrDefault(v =>
                    v.Name.Equals(a.OptionValueName, StringComparison.OrdinalIgnoreCase) &&
                    v.OptionTypeId == Guid.Parse(a.OptionTypeId));
                if (ov is null) continue;

                var assocResult = OptionValueVariantMethod.Create(
                    Guid.Parse(a.VariantId), ov.Id);
                if (assocResult.IsSuccess)
                    Context.Set<OptionValueVariant>().Add(assocResult.Value);
            }
            await Context.SaveChangesAsync(ct);
        }
    }

    private async Task SeedHardcodedAsync(CancellationToken ct)
    {
        // Existing hardcoded seeder logic preserved verbatim
        var menTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "men", ct);
        var womenTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "women", ct);
        var accessoriesTaxon = await Context.Set<Taxon>().FirstOrDefaultAsync(t => t.Slug == "accessories", ct);
        if (menTaxon is null && womenTaxon is null && accessoriesTaxon is null) return;

        await SeedProductWithVariants(new("Classic Cotton T-Shirt", "classic-cotton-t-shirt", "A comfortable classic cotton t-shirt.", "Classic Cotton T-Shirt", "t-shirt, cotton", menTaxon, "TEE-CTN-001-MSTR", "TEE-CTN-001-MSTR-BAR", 29.99m, null, [("S", "TEE-CTN-001-S"), ("M", "TEE-CTN-001-M"), ("L", "TEE-CTN-001-L"), ("XL", "TEE-CTN-001-XL")]), ct);
        await SeedProductWithVariants(new("Slim Fit Jeans", "slim-fit-jeans", "Modern slim-fit jeans.", "Slim Fit Jeans", "jeans, denim", menTaxon, "JNS-SLM-001-MSTR", "JNS-SLM-001-MSTR-BAR", 79.99m, null, [("30", "JNS-SLM-001-30"), ("32", "JNS-SLM-001-32"), ("34", "JNS-SLM-001-34")]), ct);
        await SeedProductWithVariants(new("Floral Summer Dress", "floral-summer-dress", "Light and breezy floral dress.", "Floral Summer Dress", "dress, floral", womenTaxon, "DRS-FLR-001-MSTR", "DRS-FLR-001-MSTR-BAR", 59.99m, 49.99m, [("S", "DRS-FLR-001-S"), ("M", "DRS-FLR-001-M"), ("L", "DRS-FLR-001-L")]), ct);
        await SeedProductWithoutSizes(new("Leather Tote Bag", "leather-tote-bag", "Handcrafted genuine leather tote bag.", "Leather Tote Bag", "bag, tote", accessoriesTaxon, "BAG-LEA-001", "BAG-LEA-001-BAR", 129.99m), ct);
        await SeedProductWithVariants(new("Running Sneakers", "running-sneakers", "Lightweight performance running shoes.", "Running Sneakers", "sneakers, running", menTaxon, "SNK-RUN-001-MSTR", "SNK-RUN-001-MSTR-BAR", 89.99m, 74.99m, [("8", "SNK-RUN-001-8"), ("9", "SNK-RUN-001-9"), ("10", "SNK-RUN-001-10")]), ct);
        await Context.SaveChangesAsync(ct);
    }

    private async Task SeedProductWithVariants((string Name, string Slug, string Description, string MetaTitle, string MetaKeywords, Taxon? Taxon, string MasterSku, string MasterBarcode, decimal Price, decimal? CompareAtPrice, (string Size, string Sku)[]? Sizes) seed, CancellationToken ct)
    {
        if (seed.Taxon is null) return;
        var productId = Guid.NewGuid(); var variantId = Guid.NewGuid();
        var productResult = ProductMethod.Create(seed.Name, seed.Slug, seed.Description, ProductStatus.Active, DateTimeOffset.UtcNow, seed.MetaTitle, seed.Description, seed.MetaKeywords, id: productId);
        var product = productResult.Value; product.GenderTarget = seed.Taxon.Name;
        var masterResult = VariantMethod.Create(productId, seed.MasterSku, true, 0, seed.MasterBarcode, id: variantId);
        var masterVariant = masterResult.Value; masterVariant.Price = seed.Price;
        var masterPriceResult = PriceMethod.Create(seed.Price, "USD", variantId, seed.CompareAtPrice, "US");
        masterPriceResult.Value!.IsDefault = true;
        var classificationResult = ClassificationMethod.Create(productId, seed.Taxon.Id, 0);
        product.Variants.Add(masterVariant); product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);
        Context.Set<Product>().Add(product); Context.Set<Variant>().Add(masterVariant); Context.Set<Price>().Add(masterPriceResult.Value);
        int pos = 1;
        foreach (var (size, sku) in seed.Sizes!)
        {
            var childVariantId = Guid.NewGuid();
            var childResult = VariantMethod.Create(productId, sku, false, pos, $"{sku}-BAR", id: childVariantId);
            var childVariant = childResult.Value; childVariant.Price = seed.Price;
            var childPriceResult = PriceMethod.Create(seed.Price, "USD", childVariantId, seed.CompareAtPrice, "US");
            product.Variants.Add(childVariant); Context.Set<Variant>().Add(childVariant); Context.Set<Price>().Add(childPriceResult.Value); pos++;
        }
    }

    private async Task SeedProductWithoutSizes((string Name, string Slug, string Description, string MetaTitle, string MetaKeywords, Taxon? Taxon, string MasterSku, string MasterBarcode, decimal Price) seed, CancellationToken ct)
    {
        if (seed.Taxon is null) return;
        var productId = Guid.NewGuid(); var variantId = Guid.NewGuid();
        var productResult = ProductMethod.Create(seed.Name, seed.Slug, seed.Description, ProductStatus.Active, DateTimeOffset.UtcNow, seed.MetaTitle, seed.Description, seed.MetaKeywords, id: productId);
        var product = productResult.Value; product.GenderTarget = "Unisex";
        var variantResult = VariantMethod.Create(productId, seed.MasterSku, true, 0, seed.MasterBarcode, id: variantId);
        var variant = variantResult.Value; variant.Price = seed.Price;
        var priceResult = PriceMethod.Create(seed.Price, "USD", variantId, compareAtAmount: null, "US");
        priceResult.Value!.IsDefault = true;
        var classificationResult = ClassificationMethod.Create(productId, seed.Taxon.Id, 0);
        product.Variants.Add(variant); product.MasterVariantId = variantId;
        product.Classifications.Add(classificationResult.Value);
        Context.Set<Product>().Add(product); Context.Set<Variant>().Add(variant); Context.Set<Price>().Add(priceResult.Value);
    }

    private record DemoProductJson(string Id, string Name, string Slug, string Description, string Status,
        string GenderTarget, string MetaTitle, string MetaKeywords, string MasterVariantId);
    private record DemoVariantJson(string Id, string ProductId, string Sku, bool IsMaster, int Position,
        decimal Price, string? Barcode);
    private record DemoVariantImageJson(string Id, string VariantId, string ContentType, string FileName,
        string StoragePath, int Position, string Alt, string Type);
    private record DemoOptionAssignmentJson(string VariantId, string OptionValueName, string OptionTypeId);
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj && git add service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs && git commit -m "feat(Catalog): add JSON-driven product/variant seeding"
```

---

### Task 3.5: Create `CatalogEmbeddingSeeder`

**Files:**
- Create: `service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs`

**Interfaces:**
- Consumes: `demo_embeddings.json`
- Produces: `ImageEmbedding` records in DB

- [ ] **Step 1: Write the seeder**

```csharp
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogEmbeddingSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 135;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ImageEmbedding>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoEmbeddingJson>("demo_embeddings.json");
        if (json is null)
            return Result.Ok();

        foreach (var e in json)
        {
            var embedding = ImageEmbeddingMethod.Create(
                variantImageId: Guid.Parse(e.VariantImageId),
                modelName: e.ModelName,
                modelVersion: e.ModelVersion,
                vectorData: e.Vector);
            Context.Set<ImageEmbedding>().Add(embedding);
        }
        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private record DemoEmbeddingJson(string VariantImageId, string ModelName, string ModelVersion,
        float[] Vector, int Dimensions);
}
```

- [ ] **Step 2: Register in Catalog.Extension.cs**

Add after `CatalogDemoSeeder` line:

```csharp
builder.AddSeeder<CatalogEmbeddingSeeder>();
```

- [ ] **Step 3: Verify build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs service/Api/src/Module/Catalog/Catalog.Extension.cs
git commit -m "feat(Catalog): add embedding seeder for JSON-driven demo data"
```

---

## Phase 4: C# Inventory Seeders

### Task 4.1: Create Inventory JSON helper

**Files:**
- Create: `service/Api/src/Module/Inventory/Persistence/Seeders/Seeder.Json.cs`

- [ ] **Step 1: Write the helper (identical to Catalog's but in Inventory namespace)**

```csharp
using System.Text.Json;

namespace Module.Inventory.Persistence.Seeders;

public static class DemoJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static T[]? LoadIfExists<T>(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var fullPath = Path.Combine(basePath, "Seeders", "Data", fileName);
        if (!File.Exists(fullPath))
            return null;

        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T[]>(json, JsonOptions);
    }
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj && git add service/Api/src/Module/Inventory/Persistence/Seeders/Seeder.Json.cs && git commit -m "feat(Inventory): add JSON deserialization helper"
```

---

### Task 4.2: Update `StockLocationSeeder` to support JSON

**Files:**
- Modify: `service/Api/src/Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs`

- [ ] **Step 1: Write updated seeder**

```csharp
using Module.Inventory.Domain.StockLocations;
using Module.Location.Domain.Countries;

namespace Module.Inventory.Persistence.Seeders;

public sealed class StockLocationSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStockLocations = await HasDataAsync<StockLocation>(cancellationToken);
        if (hasStockLocations)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoStockLocationJson>("demo_stock_locations.json");
        if (json is not null)
        {
            var countries = await Context.Set<Country>().ToListAsync(cancellationToken);
            foreach (var loc in json)
            {
                var country = countries.FirstOrDefault(c => c.IsoCode == loc.CountryIso);
                var result = StockLocationMethod.Create(
                    name: loc.Name, isDefault: loc.IsDefault, active: loc.Active,
                    countryId: country?.Id, presentation: loc.Presentation, code: loc.Code,
                    address1: loc.Address1, city: loc.City, postalCode: loc.PostalCode,
                    phone: loc.Phone, backorderableDefault: loc.BackorderableDefault,
                    propagateAllVariants: loc.PropagateAllVariants,
                    position: loc.Position, id: Guid.Parse(loc.Id));
                Context.Set<StockLocation>().Add(result.Value);
            }
            await Context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var us = await Context.Set<Country>().FirstOrDefaultAsync(c => c.IsoCode == "US", cancellationToken);
        var defaultResult = StockLocationMethod.Create(
            name: "Default Warehouse", presentation: "Default Warehouse", code: "DEFAULT",
            isDefault: true, active: true, propagateAllVariants: true, countryId: us?.Id,
            address1: "123 Commerce Blvd", city: "New York", postalCode: "10001", phone: "+12025550100");
        Context.Set<StockLocation>().Add(defaultResult.Value);
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockLocationJson(string Id, string Name, string? Presentation, string Code,
        bool IsDefault, bool Active, string? Address1, string? City, string? PostalCode, string? Phone,
        bool BackorderableDefault, bool PropagateAllVariants, int Position, string CountryIso);
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj && git add service/Api/src/Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs && git commit -m "feat(Inventory): add JSON-driven stock location seeding"
```

---

### Task 4.3: Update `InventoryStockItemSeeder` and `InventoryStockMovementSeeder` to support JSON

**Files:**
- Modify: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs`
- Modify: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs`

- [ ] **Step 1: Write updated `InventoryStockItemSeeder`**

```csharp
using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockItemSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 140;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStockItems = await HasDataAsync<StockItem>(cancellationToken);
        if (hasStockItems)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoStockItemJson>("demo_stock_items.json");
        if (json is not null)
        {
            var locations = await Context.Set<StockLocation>().ToListAsync(cancellationToken);
            var variants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
            var variantLookup = variants.ToDictionary(v => v.Id);

            foreach (var item in json)
            {
                var location = locations.FirstOrDefault(l => l.Code == item.StockLocationCode);
                if (location is null) continue;
                if (!variantLookup.TryGetValue(Guid.Parse(item.VariantId), out _)) continue;

                var result = StockItemMethod.Create(
                    stockLocationId: location.Id,
                    variantId: Guid.Parse(item.VariantId),
                    countOnHand: item.CountOnHand,
                    backorderable: item.Backorderable);
                if (result.IsSuccess)
                    Context.Set<StockItem>().Add(result.Value);
            }
            await Context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var stockLocation = await Context.Set<StockLocation>().FirstOrDefaultAsync(sl => sl.Default, cancellationToken);
        if (stockLocation is null) return Result.Ok();
        var allVariants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
        foreach (var variant in allVariants)
        {
            int countOnHand = variant.Sku switch
            {
                "TEE-CTN-001-S" => 50, "TEE-CTN-001-M" => 75, "TEE-CTN-001-L" => 40, "TEE-CTN-001-XL" => 25, "TEE-CTN-001-MSTR" => 10,
                "JNS-SLM-001-30" => 30, "JNS-SLM-001-32" => 45, "JNS-SLM-001-34" => 20, "JNS-SLM-001-MSTR" => 5,
                "DRS-FLR-001-S" => 15, "DRS-FLR-001-M" => 35, "DRS-FLR-001-L" => 20, "DRS-FLR-001-MSTR" => 3,
                "BAG-LEA-001" => 12,
                "SNK-RUN-001-8" => 30, "SNK-RUN-001-9" => 55, "SNK-RUN-001-10" => 40, "SNK-RUN-001-MSTR" => 8,
                _ => 0
            };
            var result = StockItemMethod.Create(stockLocation.Id, variant.Id, countOnHand > 0, countOnHand);
            Context.Set<StockItem>().Add(result.Value!);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockItemJson(string VariantId, string StockLocationCode, int CountOnHand, bool Backorderable);
}
```

- [ ] **Step 2: Write updated `InventoryStockMovementSeeder`**

```csharp
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockMovementSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 150;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasMovements = await HasDataAsync<StockMovement>(cancellationToken);
        if (hasMovements)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoStockMovementJson>("demo_stock_movements.json");
        if (json is not null)
        {
            var stockItems = await Context.Set<StockItem>().ToListAsync(cancellationToken);
            var locations = await Context.Set<StockLocation>().ToListAsync(cancellationToken);

            foreach (var m in json)
            {
                var location = locations.FirstOrDefault(l => l.Code == m.StockLocationCode);
                if (location is null) continue;
                var stockItem = stockItems.FirstOrDefault(si =>
                    si.VariantId == Guid.Parse(m.VariantId) && si.StockLocationId == location.Id);
                if (stockItem is null) continue;

                var result = StockMovementMethod.Create(
                    stockItemId: stockItem.Id, quantity: m.Quantity,
                    previousCountOnHand: m.PreviousCountOnHand,
                    originatorType: m.OriginatorType, reason: m.Reason,
                    action: m.Action, stockLocationId: location.Id);
                if (result.IsSuccess)
                    Context.Set<StockMovement>().Add(result.Value);
            }
            await Context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var items = await Context.Set<StockItem>().Where(si => si.CountOnHand > 0).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            var result = StockMovementMethod.Create(item.Id, item.CountOnHand, 0, "Adjustment", reason: "Initial stock seeding", action: "restock");
            Context.Set<StockMovement>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockMovementJson(string VariantId, string StockLocationCode, int Quantity,
        int PreviousCountOnHand, string OriginatorType, string Reason, string Action);
}
```

- [ ] **Step 3: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj && git add service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs && git commit -m "feat(Inventory): add JSON-driven stock item/movement seeding"
```

---

## Phase 5: Configuration

### Task 5.1: Update `appsettings.Development.json` for demo storage path

**Files:**
- Modify: `service/Api/src/Api/appsettings.Development.json`

- [ ] **Step 1: Add demo LocalPath override**

In the existing `"Storage"` > `"Providers"` > `"Local"` block, change or add `LocalPath`:

```json
"Storage": {
  "Providers": {
    "Local": {
      "LocalPath": "../../../infra/Storage/demo"
    }
  }
}
```

If this path causes issues in non-demo development, comment it out and document:

```json
// "LocalPath": "../../../infra/Storage/demo"  // Uncomment when using demo seeders
```

- [ ] **Step 2: Verify build**

```bash
dotnet build service/Api/src/Api/Api.csproj
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Api/appsettings.Development.json
git commit -m "config: add demo storage LocalPath for development"
```

---

### Task 5.2: Integration verification

- [ ] **Step 1: Run full ETL pipeline**

```bash
uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --skip-embeddings
```

Expected: JSON files in `service/Api/src/Module/Catalog/Persistence/Seeders/Data/` and `infra/Storage/demo/images/`.

- [ ] **Step 2: Verify all JSON files are valid**

```bash
python -c "
import json, pathlib
d = pathlib.Path('service/Api/src/Module/Catalog/Persistence/Seeders/Data')
for f in sorted(d.glob('*.json')):
    data = json.loads(f.read_text())
    print(f'{f.name}: {len(data)} records')
"
```

- [ ] **Step 3: Build .NET**

```bash
dotnet build
```

- [ ] **Step 4: Commit any final changes**

```bash
git add -A && git status
```

---

## Spec Coverage Self-Review

| Spec Requirement | Covered By |
|---|---|
| Python ETL reads `styles.csv`, outputs JSON | Tasks 1.2 – 1.6 |
| Configurable product count (`--count`) | Task 1.7 (`run_all.py`) |
| 2-level category nested-set taxons | Task 1.2 (`extract_taxonomies.py`) |
| Brands + Article Types taxonomies | Task 1.2 |
| Size + Color option types | Tasks 1.2 – 1.3 |
| Variant grouping by `productDisplayName` | Task 1.3 |
| 2 images per variant (Default 512px + Search 224px) | Tasks 1.3 – 1.4 |
| 3-tier stock locations (60/25/15) | Task 1.6 |
| USD pricing by article type | Task 1.3 |
| Deterministic GUIDs (UUID v5) | All Python scripts |
| Python generates embeddings via sidecar | Task 1.5 |
| Images stored in `infra/Storage/demo/` | Tasks 1.4, 5.1 |
| EF Core seeders read JSON, fall back to hardcoded | Tasks 3.1 – 4.3 |
| `CatalogEmbeddingSeeder` (new, order 135) | Task 3.5 |
| Idempotent via `HasDataAsync<T>()` | All seeder tasks |
| `appsettings.Development.json` LocalPath | Task 5.1 |
