# Demo Seeder Refactor — Domain-Driven Single-Responsibility Pipeline

**Date:** 2026-08-03
**Status:** Approved
**Context:** The demo data pipeline (`benchmarks/scripts/demo-seed/` Python ETL + `service/Api/src/Module/Catalog/Persistence/Seeders/` C# consumers) mixes multiple entities per script/seeder: `01_extract_taxonomies.py` writes taxonomies, taxons, option types, and option values; `02_extract_products.py` writes products, variants, images, assignments, and classifications; C# `Option.Seeder` and `Product.Seeder` each consume several files. The variant generator also violates the domain invariant (a variant must have exactly one value per option type) by attaching all of a product's sizes to each color variant.

## Goals

1. One seeder manages exactly one entity; each dataset lives in its own `{NNN}_demo_{entity}.json` file
2. Single orchestration script runs all seeders in dependency order; each seeder stays independently executable
3. Each product limited to a maximum of 10 variants; the master variant uses the first generated option combination (no duplicate child)
4. Enriched demo metadata (SEO, branding, dimensions, tags, shipping, media) mapped onto existing columns
5. Deterministic execution, improved maintainability/readability, easy per-domain regeneration — while preserving existing demo behavior and data model

## Approach

Source-driven shared-core (Approach 1): `shared.py` grows into a domain core (source parsing, deterministic ID formulas, metadata extractors, combination generator); each entity script becomes a thin writer that re-parses the source dataset and writes exactly one JSON file. `run_all.py` orchestrates in dependency order; any script runs standalone.

## Section 1: Python pipeline structure

New script layout in `benchmarks/scripts/demo-seed/`:

| Script | Writes | Depends on |
|---|---|---|
| `01_extract_taxonomies.py` | `001_demo_taxonomies.json` | source CSV |
| `02_extract_taxons.py` | `002_demo_taxons.json` | source CSV |
| `03_extract_option_types.py` | `003_demo_option_types.json` | source CSV |
| `04_extract_option_values.py` | `004_demo_option_values.json` | source CSV + styles JSON (sizes) |
| `05_extract_products.py` | `005_demo_products.json` | source CSV + styles JSON |
| `06_extract_variants.py` | `006_demo_variants.json` | source CSV + styles JSON |
| `07_extract_variant_images.py` | `007_demo_variant_images.json` | source CSV + styles JSON |
| `08_extract_product_taxons.py` | `008_demo_product_taxons.json` | source CSV |
| `09_extract_stock.py` | `009/010/011_demo_stock_*.json` | `006_demo_variants.json` |
| `10_process_images.py` | `images/` files | `007_demo_variant_images.json` |
| `11_generate_embeddings.py` | `012_demo_embeddings.json` | `007_demo_variant_images.json` |
| `12_verify_output.py` | — | all |
| `run_all.py` | — | all, in order |

**`shared.py` becomes the domain core:**

- `source.py` — CSV + styles-JSON parsing (products, colors, sizes, metadata) with a lazy in-memory index so repeated per-script parses are cheap
- `ids.py` — deterministic `guid()` formulas per entity (`master_variant_id(product)`, `variant_id(product,color,size)`, taxon/option IDs…) — single source of truth so IDs agree across scripts
- `metadata.py` — extractors: SEO, branding, dimensions, tags, shipping, media (from `articleAttributes`, `productDescriptors`, `styleImages`)
- `variants.py` — combination generator: color×size in size-major order, 10-cap, master = first combo, no duplicate child
- `json_io.py` — `write_json`, `check_overwrite`, priority helpers

**Orchestration:** `run_all.py` calls each script with `--force` support; each script stays executable standalone (scripts 01-08 derive only from the dataset; 09-11 read prior artifacts).

## Section 2: File naming, entity mapping, C# seeder split

Naming convention: `{NNN}_demo_{entity}.json` — the priority prefix is the DB seeding order, which is also the orchestration order.

| File | Entity | C# seeder (new split) | Order |
|---|---|---|---|
| `003_demo_option_types.json` | OptionType | `CatalogOptionTypeSeeder` (from `Option.Seeder`) | 100 |
| `004_demo_option_values.json` | OptionValue | `CatalogOptionValueSeeder` (from `Option.Seeder`) | 105 |
| `001_demo_taxonomies.json` | Taxonomy | `CatalogTaxonomySeeder` (unchanged) | 110 |
| `002_demo_taxons.json` | Taxon | `CatalogTaxonSeeder` (unchanged) | 120 |
| `005_demo_products.json` | Product | `CatalogProductSeeder` (from `Product.Seeder`) | 130 |
| `006_demo_variants.json` | Variant (+ embedded option assignments) | `CatalogVariantSeeder` | 132 |
| `007_demo_variant_images.json` | VariantImage | `CatalogVariantImageSeeder` | 134 |
| `008_demo_product_taxons.json` | Classification | `CatalogProductTaxonSeeder` | 136 |
| `012_demo_embeddings.json` | ImageEmbedding | `CatalogEmbeddingSeeder` (unchanged) | 137 |
| `009_demo_stock_locations.json` | StockLocation | `StockLocationSeeder` (renamed file only) | 100 |
| `010_demo_stock_items.json` | StockItem | `InventoryStockItemSeeder` (renamed file only) | 140 |
| `011_demo_stock_movements.json` | StockMovement | `InventoryStockMovementSeeder` (renamed file only) | 150 |

Key decisions:

- **Option assignments embedded in `006_demo_variants.json`** — each variant carries its own `option_values: [{option_type_id, option_value_name}]` list. The old `demo_option_assignments.json` file disappears (it was a cross-entity artifact, not an entity). The seeder resolves names → IDs from DB, so no cross-file ID coupling.
- **`product.master_variant_id`** stays in `005_demo_products.json`, computed via the shared `master_variant_id()` formula in `ids.py` — deterministic, consistent with `006` even though the scripts run independently.
- **Classifications** (`008_demo_product_taxons.json`) reference `taxon_id` via the shared deterministic formula — same as today.
- **VariantImage stays one entity** — "Product Images" (search/gallery on master) are just master-variant images with `type: Search/Gallery`; no separate product-image file.
- **C# `Option.Seeder` and `Product.Seeder` split** into 2 + 4 seeder classes respectively; all existing `LoadIfExists("demo_*.json")` calls updated to the prefixed names. The `Demo*Json` records move into their own seeder classes.
- **Inventory C# seeders unchanged** except the JSON filename strings.

## Section 3: Variant generation rules

The `variants.py` core generator replaces the buggy loop in the old `02_extract_products.py` (which attached all sizes to each color variant, violating the 1-value-per-option-type invariant):

**Combination generation (size-major):**

```
for each unique color (in CSV encounter order, deduped):
    sizes = extract_sizes(styles JSON for that color's row)   # sorted
    for each size:
        combo = (color, size)
```

- **Master variant** = first combo `(color[0], size[0])` — carries the master's `Default`/`Search`/`Gallery` images and gets `is_master: true`; **no duplicate child** is created for that combo
- **Children** = remaining combos, each `is_master: false`, `position` 1..N
- **Cap:** only the first 10 combos per product are emitted (master + 9 children); products with ≤10 combos keep all
- **No sizes** (e.g., perfume/watches): master = color-only (no size assignment); children = additional colors up to the cap
- **No color and no sizes:** master only, no option values

**Each variant emits:**

- `id` (deterministic), `product_id`, `sku` (unique per product), `position`, `price`, `barcode`, `hs_code`
- `is_master`, `weight/weight_unit/height/width/depth/dimensions_unit`, `cost_price`, `cost_currency` (enrichment)
- embedded `option_values: [{option_type_id, option_value_name}]` — exactly one entry per option type (invariant: never >1 per type)

**Behavior preserved:** prices from `ARTICLE_PRICE_MAP` per article type; `is_master` = position 0; storefront `GetAvailability` still reads `OptionValueVariants` per variant (now 2 values max: color + size); the `ValidateSingleValuePerOptionType` invariant holds by construction.

## Section 4: Enrichment mapping

All enrichment maps onto existing columns — no schema changes. Source: `styles/{id}.json` (`articleAttributes`, `productDescriptors`, `styleImages`) and CSV columns.

**Product** (`005`): real `meta_description` (from description descriptor, not a copy of the product description), `meta_keywords` (article + brand + department + tags from `articleAttributes`), `style_code` (brand initials + article, as today), `season_name`, `department`, `gender_target`, `material_composition` (from `productDescriptors.description` via the material extractor).

**Variant** (`006`): `weight`/`weight_unit`, `height`, `width`, `depth`, `dimensions_unit` (from article dimensions where available; sensible defaults otherwise — e.g., 0.3 kg clothing, 1.0 kg shoes), `cost_price`/`cost_currency` (≈ 45-55% of price), `barcode` (sku-derived as today), `hs_code` (article_number-derived).

**Taxon** (`002`): `description` (category blurb), `meta_title`/`meta_description`/`meta_keywords` (SEO strings derived from name + taxonomy), `permalink` (slug-prefixed), `pretty_name` (name), `image_url`/`square_image_url` (nullable — only set when a matching product image exists).

**Taxonomy** (`001`): unchanged (no columns available beyond name/presentation/position).

**VariantImage** (`007`): real `width`/`height` (backfilled by `10_process_images.py` during processing), `alt` (product name + type descriptor), `file_size` (from processed file).

**Deterministic defaults** for anything missing in source data — no randomness except stock quantities in `09` (seeded, as today).

## Section 5: Verification, regeneration, testing

**Verify script (`12_verify_output.py`, evolved from `06`):** adds the invariants this refactor guarantees:

- every variant has **≤1 value per option type**; master variants have no duplicate child combo
- each product ≤10 variants; master is `position 0` and its combo is not repeated among children
- FK integrity (as today): variant→product, image→variant, assignment names→option values, classification→product/taxon, stock→variant, embedding→search image
- deterministic-ID consistency: `product.master_variant_id` matches the first child combo's variant id formula

**Regeneration:**

- `run_all.py` regenerates everything in order (scripts 01-08 are source-driven and fully independent; 09-11 read prior artifacts)
- Single-domain regeneration: `uv run python 06_extract_variants.py --dataset … --force` rewrites only `006_demo_variants.json`
- Existing `output/` is regenerated as part of this task (dataset exists locally; images/embeddings stay valid because master-variant IDs and search-image IDs are unchanged by the ID formulas)

**C# verification:**

- `dotnet build` (warnings-as-errors) after the seeder split + renamed `LoadIfExists` strings
- `dotnet test service/Api/tests/Module.UnitTests` — seeder classes are exercised via integration tests; coverage stays via `Api.Tests` integration scenarios which seed from these files
- Ruff lint on the demo-seed scripts where in scope

**Docs:** update `benchmarks/docs/` references to the renamed files (search for `demo_*.json` mentions) and any `appsettings` DemoDataPath notes that reference file names.
