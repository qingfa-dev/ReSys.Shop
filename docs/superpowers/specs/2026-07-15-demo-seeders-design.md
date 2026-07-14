# Demo Seeders — Python ETL + EF Core Import

**Date**: 2026-07-15
**Status**: Design approved, pending implementation

## Overview

Replace hardcoded demo data with script-driven seeding from the Kaggle Fashion Product Images
dataset (44,441 products in `benchmarks/data/raw/fashion-product-images/`). Python ETL scripts
extract/transform the CSV/JSON, output domain-aligned CSV files, and EF Core seeders import them
at app startup.

## Architecture

```
benchmarks/data/raw/fashion-product-images/
    styles.csv (44,446 rows) + images/ (44,441 JPEGs)

    ▼ Python ETL (benchmarks/scripts/demo-seed/)

benchmarks/scripts/demo-seed/
├── extract_taxonomies.py    → demo_taxonomies.csv, demo_taxons.csv,
│                               demo_option_types.csv, demo_option_values.csv
├── extract_products.py      → demo_products.csv, demo_variants.csv,
│                               demo_variant_images.csv, demo_option_assignments.csv
├── process_images.py        → scales images → infra/Storage/demo/images/
├── generate_embeddings.py   → calls Embedding sidecar → demo_embeddings.csv
├── extract_stock.py         → demo_stock_locations.csv, demo_stock_items.csv,
│                               demo_stock_movements.csv
└── run_all.py               → orchestrator, --count N parameter

    ▼ CSV files (service/Api/src/Module/{Catalog,Inventory}/Persistence/Seeders/Data/)

    ▼ EF Core seeders (read CSVs, fall back to hardcoded if absent)

Catalog:  OptionSeeder(100) → TaxonomySeeder(110) → TaxonSeeder(120) →
          DemoSeeder(130) → EmbeddingSeeder(135)
Inventory: StockLocationSeeder(100) → StockItemSeeder(140) → StockMovementSeeder(150)
```

## Data Mapping

### Taxonomies & Taxons (2-level nested set)

| Taxonomy | Depth-1 Taxons | Depth-2 Taxons |
|---|---|---|
| Categories | Apparel, Footwear, Accessories, Personal Care, etc. | Topwear, Shoes, Bags, etc. |
| Brands | Nike, Puma, Adidas, ... (97+) | — |
| Article Types | Tshirts, Shirts, Casual Shoes, ... (143) | — |

Nested-set Lft/Rgt/Depth computed by Python for the 2-level category tree. Brands and
Article Types are flat (depth-1 only).

### Option Types & Values

| OptionType | Values |
|---|---|
| Size (from JSON articleAttributes) | S, M, L, XL, 28, 30, 32, ... |
| Color (from CSV baseColour) | Black, White, Blue, Red, ... (48) |

### Product → Variant Grouping

- Products grouped by **exact `productDisplayName`** match
- First variant in each group = master (`IsMaster = true`)
- Variants differ by `baseColour` → Color option value
- JSON `articleAttributes.Size` → Size option value (when present)

### Variant Images (2 per variant)

| VariantImageType | Size | Purpose |
|---|---|---|
| Default | 512px | Primary display (medium) |
| Search | model-specific (224 or 256) | Embedding extraction |

Model-specific sizes: 224px for convnext-v2-tiny, openclip-vit-b-32, dinov2-vit-small,
fashion-clip-v1, efficientnet-b0, swin-base, dinov2-vit-base, ibot-vit-base; 256px for siglip-vit-b-16.

Both images sourced from the same JPEG, duplicated at different scales.

### Stock (3 physical-tier locations, 60/25/15 distribution)

| Location | Address | Stock % |
|---|---|---|
| Main Warehouse | New York, US | 60% |
| East Distribution | Atlanta, US | 25% |
| Express Hub | Los Angeles, US | 15% |

Base quantity: random 10–200 per variant for primary. Master variants: 0–5.

### Pricing

Single currency USD. Default amounts by article type (e.g., Tshirts $24.99, Watches $89.99).
`Price.IsDefault = true`.

## Storage & Image Serving

- Python copies processed images to `infra/Storage/demo/images/medium/` and `infra/Storage/demo/images/search/{model}/`
- `appsettings.Development.json`: `"LocalPath": "../../../infra/Storage/demo"`
- `VariantImage.StoragePath` = `images/medium/<id>.jpg` (relative)
- `VariantImage.Url` = empty (local storage, same as existing behavior)
- Images served through existing `GET /api/storefront/images/{id}` Carter endpoint

## Python Scripts (`benchmarks/scripts/demo-seed/`)

### `run_all.py`
Entry point. `--count N` controls product count. `--dataset` and `--output` params.

### `extract_taxonomies.py`
Reads `styles.csv`, collects unique taxonomic values, builds nested-set, outputs taxonomy/taxon/option CSVs.

### `extract_products.py`
Reads `styles.csv`, groups by `productDisplayName`, maps variants/options, generates deterministic GUIDs, outputs product/variant/image/assignment CSVs.

### `process_images.py`
Reads each selected product's JPEG, resizes to 512px and model-specific sizes, copies to `infra/Storage/demo/images/`.

### `generate_embeddings.py`
Calls `service/Embedding/` FastAPI for each search-size image. Stores vectors in `demo_embeddings.csv`. Warns if sidecar unavailable (products import without embeddings).

### `extract_stock.py`
Produces 3 stock location records, generates per-variant stock quantities, outputs location/item/movement CSVs.

## EF Core Seeders (updated)

All retain fallback to existing hardcoded behavior when CSV absent. New seeder added.

| Seeder | Order | Change |
|---|---|---|
| `CatalogOptionSeeder` | 100 | Reads `demo_option_types.csv` + `demo_option_values.csv` |
| `CatalogTaxonomySeeder` | 110 | Reads `demo_taxonomies.csv` |
| `CatalogTaxonSeeder` | 120 | Reads `demo_taxons.csv`, computes nested-set from CSV |
| `CatalogDemoSeeder` | 130 | Reads `demo_products.csv` + `demo_variants.csv` + images, assigns classifications |
| `CatalogEmbeddingSeeder` | 135 | **New** — reads `demo_embeddings.csv`, creates `ImageEmbedding` records |
| `StockLocationSeeder` | 100 | Reads `demo_stock_locations.csv` |
| `InventoryStockItemSeeder` | 140 | Reads `demo_stock_items.csv` |
| `InventoryStockMovementSeeder` | 150 | Reads `demo_stock_movements.csv` |

### CSV Detection Logic

Each seeder checks `HasDataAsync<T>()` first (idempotent). Then checks for CSV file existence.
If CSV present → import from CSV. If no CSV → existing hardcoded behavior.

### CSV Format Example (`demo_variants.csv`)

```csv
id,product_id,sku,is_master,position,price,color_value_name,size_value_name
550e8400-e29b-...,6ba7b810-...,TEE-NKE-001-BLK,true,0,29.99,Black,M
6ba7b811-...,6ba7b810-...,TEE-NKE-001-WHT,false,1,29.99,White,M
```
(Referential columns use names — the C# seeder resolves them against already-seeded OptionValues.)

Deterministic GUIDs for product/variant IDs: Python generates UUID v5
(namespace: `a1b2c3d4-e5f6-7890-abcd-ef1234567890`, name: `benchmark-id` + entity-type suffix).
`product_id` in `demo_variants.csv` exactly matches `id` in `demo_products.csv`.

Option value references use **names** (e.g., `Black`, `M`) — the C# seeder resolves them against
the already-seeded OptionValues table during import. This avoids pre-computing GUIDs for 48 colors
and hundreds of size values.

## Error Handling

**Python**: missing `styles.csv` → exit with error; missing image → warn, skip product; embedding
service down → warn, embed as NULL; single-variant groups → still valid.

**C#**: CSV missing → fallback to hardcoded; CSV malformed → skip row, log warning; FK integrity
→ guaranteed by deterministic GUIDs; nested-set issues → `TaxonHierarchyService.Rebuild` at runtime.

## Idempotency

All seeders use `HasDataAsync<T>()` guard. Running the app multiple times never duplicates data.
