---
goal: "Eliminate script duplication, generate Classification join records, drop job embedding mode, wire products-to-taxons in C# seeder"
version: 5.1
date_created: 2026-07-20
last_updated: 2026-07-20
owner: Platform
status: "Planned"
tags: ["feature", "data", "refactor"]
---

# Introduction

![Status: planned](https://img.shields.io/badge/status-planned-blue)

V5.1 eliminates cross-script duplication (shared constants, `guid()`, JSON helpers, `--force` guards),
generates `Classification` join records so 1000 products link to their brand/category/article-type
Taxons via `demo_classifications.json`, and drops the Hangfire `job` embedding mode. Embeddings are
now only two modes: `direct` (generate in Python, import via JSON) or `skip` (none). The C#
`CatalogDemoSeeder` drops Hangfire DI + enqueue logic and gains Classification persistence.

## 1. Requirements & Constraints

- **REQ-001**: Extract `shared.py` in `benchmarks/scripts/demo-seed/` containing: `SEED_NAMESPACE`, `guid()`, `SCRIPTS_DIR`, `OPTION_TYPE_SIZE_ID`, `OPTION_TYPE_COLOR_ID`, `TAXONOMY_CATEGORIES_ID`, `TAXONOMY_BRANDS_ID`, `TAXONOMY_ARTICLE_TYPES_ID`, `check_overwrite()`, `write_json()`
- **REQ-002**: All 5 scripts (`extract_*.py`, `process_images.py`, `generate_embeddings.py`) import from `shared.py` instead of defining local copies
- **REQ-003**: `extract_products.py` generates `demo_classifications.json` with entries `{product_id, taxon_id, position}` — one per brand taxon, per category taxon, per article-type taxon for each product
- **REQ-004**: `demo_classifications.json` entries use deterministic taxon UUIDs matching those in `demo_taxons.json` (same `guid()` function, same namespace)
- **REQ-005**: `CatalogDemoSeeder` (C#) reads `demo_classifications.json` and creates `Classification` entities via `ClassificationMethod.Create(productId, taxonId, position, isAutomatic)`
- **REQ-006**: Remove dead `taxonLookup` dict from `CatalogDemoSeeder` (loaded but never used) — replaced by direct `Classification` creation from JSON
- **REQ-007**: Pipeline still produces 1000 products with correct variants, prices, stock items, variant images (Default + Search master-only), embeddings, and option assignments
- **REQ-008**: Drop `job` from `--embedding-mode` — `run_all.py` accepts only `skip` or `direct`. `skip` means no embedding step runs. `direct` means `generate_embeddings.py` runs and writes `demo_embeddings.json`
- **REQ-009**: `CatalogEmbeddingSeeder` drops the `job` mode branch (Hangfire `IBackgroundJobClient` + `IEmbeddingOrchestrator` enqueue logic). Keep only two paths: `skip` → return Ok, `direct` → JSON import (existing)
- **REQ-010**: Remove `IBackgroundJobClient?` and `IEmbeddingOrchestrator?` constructor parameters from `CatalogEmbeddingSeeder` — no longer needed
- **CON-001**: All Python scripts share one `shared.py` — no circular imports, no new external deps
- **CON-002**: JSON retains `snake_case`; C# `DemoJsonHelper` unchanged
- **CON-003**: `dotnet build` passes with 0 warnings, 0 errors
- **CON-004**: `uv run ruff check benchmarks/scripts/demo-seed/` passes

## 2. Implementation Steps

### Implementation Phase 1: Extract `shared.py` and deduplicate all scripts

GOAL-001: Single source of truth for constants, `guid()`, `check_overwrite()`, `write_json()`.

| Task | Description | Status |
|------|-------------|--------|
| TASK-001 | Create `benchmarks/scripts/demo-seed/shared.py` with: `SEED_NAMESPACE`, `guid()`, `SCRIPTS_DIR`, `OPTION_TYPE_SIZE_ID`, `OPTION_TYPE_COLOR_ID`, `TAXONOMY_CATEGORIES_ID`, `TAXONOMY_BRANDS_ID`, `TAXONOMY_ARTICLE_TYPES_ID`, `MODEL_INPUT_SIZES` dict, `check_overwrite(path, force)` (returns `True` if should exit), `write_json(path, data)` | |
| TASK-002 | Rewrite `extract_taxonomies.py`: remove `SEED_NAMESPACE`, `guid()`, `SCRIPTS_DIR`, `OPTION_TYPE_*`, `TAXONOMY_*` constants — import from `shared`. Replace `write_text(json.dumps(...))` with `write_json()`. Replace `--force` check block with `check_overwrite()` | |
| TASK-003 | Rewrite `extract_products.py`: remove `SEED_NAMESPACE`, `guid()`, `SCRIPTS_DIR`, `OPTION_TYPE_*`, `MODEL_INPUT_SIZES` — import from `shared`. Replace `write_text(json.dumps(...))` with `write_json()`. Replace `--force` check block with `check_overwrite()` | |
| TASK-004 | Rewrite `extract_stock.py`: remove `SEED_NAMESPACE`, `guid()`, `SCRIPTS_DIR` — import from `shared`. Replace `write_text(json.dumps(...))` with `write_json()`. Replace `--force` check block with `check_overwrite()` | |
| TASK-005 | Rewrite `process_images.py`: remove `SCRIPTS_DIR`, `MODEL_INPUT_SIZES` — import from `shared` | |
| TASK-006 | Rewrite `generate_embeddings.py`: remove `SCRIPTS_DIR` — import from `shared` | |
| TASK-007 | Run `uv run ruff check benchmarks/scripts/demo-seed/` — 0 errors | |

### Implementation Phase 2: Drop `job` embedding mode — simplify to `direct` or `skip` only

GOAL-002: Embeddings are generated exclusively by Python script (`direct` mode) or not at all (`skip`). Hangfire `job` mode removed from Python orchestrator and C# seeder.

| Task | Description | Status |
|------|-------------|--------|
| TASK-008 | In `run_all.py`: change `--embedding-mode` choices from `["skip", "job", "direct"]` to `["skip", "direct"]`. Remove the `if args.embedding_mode == "job":` print line (lines 90-91). The conditional at line 49-50 (`if args.embedding_mode == "direct":`) is the only trigger for `generate_embeddings.py` now | |
| TASK-009 | In `CatalogEmbeddingSeeder.cs`: remove `IBackgroundJobClient?` and `IEmbeddingOrchestrator?` constructor parameters. Remove `using Hangfire;` import. Remove the `job` case in `SeedAsync` switch — the switch becomes `"skip" => return Ok`, `_ => await SeedFromJsonAsync(...)`. Remove `SeedViaJobsAsync` method entirely | |
| TASK-010 | In `CatalogEmbeddingSeeder.cs`: remove `using` import for `Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services` if only used for `IEmbeddingOrchestrator`. Remove `using Microsoft.EntityFrameworkCore` if only used in `SeedViaJobsAsync` (verify — `ToListAsync` was used there). Keep `DbContext`-related usings needed by `SeedFromJsonAsync` | |
| TASK-011 | Run `dotnet build` — 0 warnings, 0 errors | |
| TASK-012 | Run `uv run python benchmarks/scripts/demo-seed/run_all.py --help` — verify `--embedding-mode` shows only `skip` and `direct` | |

### Implementation Phase 3: Generate `demo_classifications.json` in Python

GOAL-003: Every product gets 3 Classification records (brand, category, article-type) referencing deterministic taxon UUIDs.

| Task | Description | Status |
|------|-------------|--------|
| TASK-013 | In `extract_products.py`, after processing all product groups, build a `classifications` list. For each product: look up its `brandName` → `guid("taxon", f"brand.{brand}")`, `masterCategory` → `guid("taxon", f"cat.{mc}")`, `articleType` → `guid("taxon", f"article_type.{at}")`. If any are empty/missing, skip that record. Output `{product_id, taxon_id, position}` per entry. Position starts at 0, increments per classification per product | |
| TASK-014 | Write `demo_classifications.json` via `write_json()` alongside other output files | |
| TASK-015 | Print classification count in summary: `"Written {n} products, {n} variants, {n} images, {n} assignments, {n} classifications"` | |

### Implementation Phase 4: Update C# `CatalogDemoSeeder` to apply Classifications

GOAL-004: `CatalogDemoSeeder` reads `demo_classifications.json` and creates `Classification` entities.

| Task | Description | Status |
|------|-------------|--------|
| TASK-016 | Remove dead `taxonLookup` dict from `CatalogDemoSeeder.SeedFromJsonAsync` (loaded but never used — lines loading `Taxon.Where(t => !t.IsDeleted)` and building the dictionary) | |
| TASK-017 | Add `DemoClassificationJson` private record: `{string ProductId, string TaxonId, int Position}` | |
| TASK-018 | In `SeedFromJsonAsync`, load `demo_classifications.json` via `jsonHelper.LoadIfExists<DemoClassificationJson>("demo_classifications.json")`. If not null, for each entry create `ClassificationMethod.Create(Guid.Parse(c.ProductId), Guid.Parse(c.TaxonId), c.Position, isAutomatic: true)` and add to context | |
| TASK-019 | Run `dotnet build` — 0 warnings, 0 errors | |

### Implementation Phase 5: Verification

GOAL-005: End-to-end validation with 1000 products and full taxon linkage.

| Task | Description | Status |
|------|-------------|--------|
| TASK-020 | Run full pipeline: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 1000 --embedding-mode skip --deploy --force` — all steps complete, all 13 JSON files produced | |
| TASK-021 | Verify `--embedding-mode` only shows `skip`/`direct`: `uv run python benchmarks/scripts/demo-seed/run_all.py --help` | |
| TASK-022 | Verify `demo_classifications.json`: `python -c "import json; c=json.load(open('benchmarks/scripts/demo-seed/output/demo_classifications.json')); p=json.load(open('benchmarks/scripts/demo-seed/output/demo_products.json')); print(f'Products: {len(p)}, Classifications: {len(c)}')"` | |
| TASK-023 | Verify taxon ID integrity: every `taxon_id` in `demo_classifications.json` exists in `demo_taxons.json`. `python -c "import json; c=json.load(open('benchmarks/scripts/demo-seed/output/demo_classifications.json')); t={x['id'] for x in json.load(open('benchmarks/scripts/demo-seed/output/demo_taxons.json'))}; missing={x['taxon_id'] for x in c} - t; print(f'Missing taxon refs: {len(missing)}'); assert len(missing)==0"` | |
| TASK-024 | Run `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Catalog"` — all pass | |
| TASK-025 | Run `uv run ruff check benchmarks/scripts/demo-seed/` — 0 errors | |
| TASK-026 | Run `dotnet build` — 0 warnings, 0 errors | |

## 3. Alternatives

- **ALT-001**: Generate Classifications in the C# seeder via string matching (look up taxon by slug matching brand/category/type) — rejected; requires loading all taxons into memory and string-matching, adds complexity vs pre-computed UUIDs in JSON that match by construction
- **ALT-002**: Add `option_value_id` to option assignments JSON instead of `option_value_name` — deferred; the C# seeder's name-based lookup works correctly (case-insensitive), changing both sides is scope creep for this plan
- **ALT-003**: Merge `shared.py` constants into `run_all.py` and import from there — rejected; `run_all.py` spawns subprocesses, not importable; `shared.py` as a standalone module is cleaner
- **ALT-004**: Keep Hangfire `job` mode for embedding generation — rejected; adds DI complexity to the seeder (`IBackgroundJobClient`, `IEmbeddingOrchestrator`), the Python `direct` mode already generates embeddings locally, and Hangfire jobs require a running Redis instance. Two clear modes (`skip` | `direct`) are simpler

## 4. Dependencies

- **DEP-001**: `benchmarks/scripts/demo-seed/shared.py` — new file, imported by all 5 scripts
- **DEP-002**: C# `ClassificationMethod.Create()` — already exists at `Module/Catalog/Domain/Products/Classifications/Classification.cs`
- **DEP-003**: `CatalogDemoSeeder` — already wired in `Catalog.Extension.cs` with DI registration
- **DEP-004**: `CatalogEmbeddingSeeder` — already wired; removing Hangfire DI params means `Catalog.Extension.cs` registration must drop `IBackgroundJobClient`/`IEmbeddingOrchestrator` if currently passed via `services.AddScoped<IDataSeeder, CatalogEmbeddingSeeder>()`

## 5. Files

- **FILE-001**: `benchmarks/scripts/demo-seed/shared.py` — **new** — constants: `SEED_NAMESPACE`, `guid()`, `SCRIPTS_DIR`, `OPTION_TYPE_SIZE_ID`, `OPTION_TYPE_COLOR_ID`, `TAXONOMY_CATEGORIES_ID`, `TAXONOMY_BRANDS_ID`, `TAXONOMY_ARTICLE_TYPES_ID`, `MODEL_INPUT_SIZES`, `check_overwrite()`, `write_json()`
- **FILE-002**: `benchmarks/scripts/demo-seed/extract_taxonomies.py` — import from `shared`, use `check_overwrite`/`write_json`
- **FILE-003**: `benchmarks/scripts/demo-seed/extract_products.py` — import from `shared`, generate `demo_classifications.json`, use `check_overwrite`/`write_json`
- **FILE-004**: `benchmarks/scripts/demo-seed/extract_stock.py` — import from `shared`, use `check_overwrite`/`write_json`
- **FILE-005**: `benchmarks/scripts/demo-seed/process_images.py` — import `MODEL_INPUT_SIZES` from `shared`
- **FILE-006**: `benchmarks/scripts/demo-seed/generate_embeddings.py` — import `SCRIPTS_DIR` from `shared`
- **FILE-007**: `benchmarks/scripts/demo-seed/run_all.py` — drop `job` from `--embedding-mode` choices, remove `job` summary line
- **FILE-008**: `benchmarks/scripts/demo-seed/output/demo_classifications.json` — **new** — automated output
- **FILE-009**: `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs` — remove dead `taxonLookup`, add `DemoClassificationJson` record, read + persist Classifications
- **FILE-010**: `service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs` — drop `IBackgroundJobClient`/`IEmbeddingOrchestrator` params, remove `job` case + `SeedViaJobsAsync`, remove `using Hangfire`
- **FILE-011**: `service/Api/src/Module/Catalog/Catalog.Extension.cs` — verify `CatalogEmbeddingSeeder` registration drops Hangfire DI params; may need update if currently passing them

## 6. Testing

- **TEST-001**: `uv run ruff check benchmarks/scripts/demo-seed/` — 0 errors after dedup
- **TEST-002**: `uv run python benchmarks/scripts/demo-seed/run_all.py --help` — `--embedding-mode` shows `{skip,direct}` only, no `job`
- **TEST-003**: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --embedding-mode skip --deploy --force` — produces 13 JSON files in output/, no embedding step runs
- **TEST-004**: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --embedding-mode direct --deploy --force` — produces `demo_embeddings.json` with embeddings for all models × search images
- **TEST-005**: `demo_classifications.json` exists and has ~30 entries (10 products × 3 taxon links = ~30, minus any products missing brand/category/type)
- **TEST-006**: Every `taxon_id` in `demo_classifications.json` matches an `id` in `demo_taxons.json`
- **TEST-007**: Every `product_id` in `demo_classifications.json` matches an `id` in `demo_products.json`
- **TEST-008**: `dotnet build` — 0 warnings, 0 errors
- **TEST-009**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Catalog"` — all pass

## 7. Risks & Assumptions

- **RISK-001**: Some products may lack `brandName`, `masterCategory`, or `articleType` in the Kaggle dataset — classification records for those fields are skipped; product still gets the remaining links. Some products will have <3 classifications
- **RISK-002**: `demo_classifications.json` may be large for 1000 products (~3000 entries) — JSON array is fine; ~3000 × 3 fields × ~50 bytes = ~450KB, well within limits
- **ASSUMPTION-001**: Taxons in `demo_taxons.json` are always generated before classifications by the ETL pipeline (order: taxonomies → taxons → products + classifications)
- **ASSUMPTION-002**: `ClassificationMethod.Create(productId, taxonId, position, isAutomatic)` accepts `true` for `isAutomatic` — seed data classifications are automated, not manual curation

## 8. Related Specifications / Further Reading

- [V4 Plan](feature-demo-seeders-v4-1.md) — embedding modes (direct/job/skip)
- [V3 Plan](feature-demo-seeders-v3-1.md) — size arguments, master-only search images
- [Architecture: Domain entities](../docs/codebase/ARCHITECTURE.md) — Classification, Taxon, Product relationships
