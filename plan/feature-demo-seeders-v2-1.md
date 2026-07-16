---
goal: "Consolidate Python ETL output, scale to 2000+ variants with embeddings, drop hardcoded seeder fallbacks"
version: 2.0
date_created: 2026-07-15
last_updated: 2026-07-15
owner: Platform
status: "Planned"
tags: ["feature", "data", "migration"]
---

# Introduction

![Status: planned](https://img.shields.io/badge/status-planned-blue)

V2 of the demo seeders pipeline. Consolidates Python ETL output into a single directory
(`benchmarks/scripts/demo-seed/output/`), scales from ~200 to 2000+ variants with multi-model
embeddings, drops all hardcoded seeder fallbacks (JSON-only), and wires up a deploy command
to push images to `infra/Storage/demo/` for runtime serving.

## 1. Requirements & Constraints

- **REQ-001**: Python ETL scripts export ALL artifacts (images + JSON) to a single directory: `benchmarks/scripts/demo-seed/output/`
- **REQ-002**: `infra/Storage/demo/` is for .NET runtime storage ONLY — Python scripts never write directly to it
- **REQ-003**: A `deploy` subcommand/step copies `output/images/` → `infra/Storage/demo/images/` for the Carter image endpoint
- **REQ-004**: All EF Core seeders are JSON-only — no hardcoded fallback logic remains
- **REQ-005**: Scale to 2000+ variants with 2 images each (Default + Search), embeddings for all available sidecar models
- **REQ-006**: C# seeders load JSON from a path configured in `appsettings.Development.json`, NOT from `Module/*/Persistence/Seeders/Data/`
- **REQ-007**: Remove `service/Api/src/Module/Catalog/Persistence/Seeders/Data/` and `service/Api/src/Module/Inventory/Persistence/Seeders/Data/` directories
- **REQ-008**: Remove `<Content>` item group from `Module.csproj` that copied JSON files to output
- **REQ-009**: `generate_embeddings.py` queries `/models` endpoint and generates embeddings for ALL available models, not just `fashion_clip`
- **REQ-010**: `run_all.py --count N` orchestrator must support `--deploy` flag to copy images to storage
- **CON-001**: Warnings-as-errors — any warning fails the .NET build
- **CON-002**: Result objects for all domain operations; no exceptions for control flow
- **CON-003**: Modules never cross-reference (except seeders for data seeding, existing pattern)
- **CON-004**: JSON uses snake_case property names; C# uses `SnakeCaseLower` naming policy
- **CON-005**: Idempotent seeding — `HasDataAsync<T>()` guard unchanged

## 2. Implementation Steps

### Implementation Phase 1: Consolidate Python Output

GOAL-001: Move all Python ETL output to `benchmarks/scripts/demo-seed/output/` with clear image/JSON sub-structure.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `extract_taxonomies.py` — change `--output` default to `SCRIPTS_DIR/output`; write JSON files to `output/` root | | |
| TASK-002 | Update `extract_products.py` — same `--output` default change; write JSON to `output/` root; set image `storage_path` to `images/medium/<id>.jpg` and `images/search/224/<id>.jpg` (relative to `infra/Storage/demo/`) | | |
| TASK-003 | Update `process_images.py` — accept `--output` for the unified output dir; write images to `output/images/medium/` and `output/images/search/224/` (NOT to `--storage`); drop `--storage` param | | |
| TASK-004 | Update `generate_embeddings.py` — accept `--output` for unified output dir; read search images from `output/images/search/224/`; write embeddings to `output/demo_embeddings.json` | | |
| TASK-005 | Update `extract_stock.py` — accept `--output`; write JSON to `output/` root | | |
| TASK-006 | Update `run_all.py` — forward `--output` to all scripts; add `--deploy` flag; add `deploy()` function that copies `output/images/` → `infra/Storage/demo/images/`; fix `REPO_ROOT` to `SCRIPTS_DIR.parent.parent.parent` | | |

### Implementation Phase 2: Drop Hardcoded Fallback (JSON-Only Seeders)

GOAL-002: Remove all hardcoded seeding logic from every seeder. If JSON is absent, the seeder returns `Result.Ok()` immediately (no-op, not an error).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Rewrite `CatalogOptionSeeder` — remove `SeedHardcodedAsync()`; load JSON or return Ok; keep `HasDataAsync` guard; keep `DemoOptionTypeJson`/`DemoOptionValueJson` record types | | |
| TASK-008 | Rewrite `CatalogTaxonomySeeder` — remove hardcoded Categories+Brands creation; JSON-only; keep record type | | |
| TASK-009 | Rewrite `CatalogTaxonSeeder` — remove `SeedHardcodedAsync()` and `CreateTaxon()` helper; JSON-only | | |
| TASK-010 | Rewrite `CatalogDemoSeeder` — remove `SeedHardcodedAsync()`, `SeedProductWithVariants()`, `SeedProductWithoutSizes()`; JSON-only | | |
| TASK-011 | Rewrite `StockLocationSeeder` — remove hardcoded "Default Warehouse" creation; JSON-only | | |
| TASK-012 | Rewrite `InventoryStockItemSeeder` — remove SKU-based hardcoded switch statement; JSON-only | | |
| TASK-013 | Rewrite `InventoryStockMovementSeeder` — remove hardcoded "Initial stock seeding" loop; JSON-only | | |
| TASK-014 | `CatalogEmbeddingSeeder` — already JSON-only, no changes needed (verify) | | |

### Implementation Phase 3: Reconfigure C# JSON Loading

GOAL-003: Seeders load JSON from a configurable path, not from `Module/*/Persistence/Seeders/Data/`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Add `"Seeders": { "DemoDataPath": "../../benchmarks/scripts/demo-seed/output" }` to `service/Api/src/Api/appsettings.Development.json` | | |
| TASK-016 | Update `Catalog/Persistence/Seeders/Seeder.Json.cs` — read `DemoDataPath` from `IConfiguration`; change `LoadIfExists` to resolve against configured path instead of `AppContext.BaseDirectory/Seeders/Data`; make helper non-static (injectable) | | |
| TASK-017 | Update `Inventory/Persistence/Seeders/Seeder.Json.cs` — identical changes as Catalog | | |
| TASK-018 | Update all 8 seeders to receive `DemoJsonHelper` via constructor injection (add `DemoJsonHelper helper` param) | | |
| TASK-019 | Register `DemoJsonHelper` as scoped service in `Catalog.Extension.cs` and `Inventory.Extension.cs` | | |
| TASK-020 | Remove `<Content Include="...Data\*.json">` item group from `Module.csproj` | | |
| TASK-021 | Delete `service/Api/src/Module/Catalog/Persistence/Seeders/Data/` directory | | |
| TASK-022 | Delete `service/Api/src/Module/Inventory/Persistence/Seeders/Data/` directory | | |

### Implementation Phase 4: Scale to 2000+ Variants + Multi-Model Embeddings

GOAL-004: Increase product count, generate embeddings for all available sidecar models.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Update `extract_products.py` — increase default `--count` to 2000; remove unused `import random`; ensure deterministic GUIDs scale correctly | | |
| TASK-024 | Update `generate_embeddings.py` — call `GET /models` first to discover available models; loop over ALL models (or at minimum the 4 known: `fashion_clip`, `efficientnet_b0`, `clip_vit_b16`, `dinov2_vits14`); generate one embedding per model per search image; model name stored in embedding record | | |
| TASK-025 | Update `process_images.py` — generate search images at sizes matching each model's expected input (224px for most, 256px for siglip if available); use `MODEL_INPUT_SIZES` dict | | |
| TASK-026 | Update `extract_products.py` — generate variant image records with `storage_path` for each model-specific search size; add `VariantImage` records per model per variant | | |

### Implementation Phase 5: Verification

GOAL-005: End-to-end validation of the 2000+ variant pipeline.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Run `uv run python benchmarks/scripts/demo-seed/run_all.py --count 2000 --deploy` — verify no errors, all JSON valid, images present in both output/ and infra/Storage/demo/ | | |
| TASK-028 | Run `dotnet build` — 0 warnings, 0 errors | | |
| TASK-029 | Run `dotnet run` — verify seeders execute without errors; verify `HasDataAsync` guard works (re-run: no duplicates); spot-check DB for seeded taxons, products, variants, images, embeddings, stock | | |

## 3. Alternatives

- **ALT-001**: Keep hardcoded fallback — rejected per REQ-004; adds maintenance burden and the fallback data is stale/irrelevant once JSON pipeline is the primary path
- **ALT-002**: Put JSON files in `infra/Storage/demo/` alongside images — rejected per REQ-002; `infra/Storage/demo/` is the runtime file-storage provider path, not a seed-data repository
- **ALT-003**: Embed JSON as assembly resources — rejected; requires recompilation to change seed data; config-path approach enables hot-swapping demo datasets
- **ALT-004**: Auto-detect model list from sidecar at C# startup — rejected; seeders run synchronously at startup, blocking model discovery behind network call is brittle; Python pre-generates all embeddings

## 4. Dependencies

- **DEP-001**: Python Embedding sidecar (`service/Embedding/`) must be running for `generate_embeddings.py` to produce embeddings; script degrades gracefully (warns, skips embeddings) if unavailable
- **DEP-002**: Kaggle Fashion Product Images dataset at `benchmarks/data/raw/fashion-product-images/` with valid `styles.csv` and `images/` directory
- **DEP-003**: `benchmarks/pyproject.toml` must have `httpx` (already added in v1)
- **DEP-004**: `service/Api/src/Api/appsettings.json` must have `Storage:Providers:Local` section (exists)

## 5. Files

- **FILE-001**: `benchmarks/scripts/demo-seed/run_all.py` — add `--deploy`, fix REPO_ROOT, update default paths
- **FILE-002**: `benchmarks/scripts/demo-seed/extract_taxonomies.py` — change default `--output`
- **FILE-003**: `benchmarks/scripts/demo-seed/extract_products.py` — change default `--output`, scale count, add per-model image paths, remove unused import
- **FILE-004**: `benchmarks/scripts/demo-seed/process_images.py` — accept `--output`, remove `--storage`, handle multi-model sizes
- **FILE-005**: `benchmarks/scripts/demo-seed/generate_embeddings.py` — accept `--output`, multi-model loop via `/models` endpoint
- **FILE-006**: `benchmarks/scripts/demo-seed/extract_stock.py` — change default `--json-dir` to `--output`
- **FILE-007**: `service/Api/src/Module/Catalog/Persistence/Seeders/Seeder.Json.cs` — make non-static, use IConfiguration for path
- **FILE-008**: `service/Api/src/Module/Inventory/Persistence/Seeders/Seeder.Json.cs` — same as FILE-007
- **FILE-009**: `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs` — drop hardcoded, inject helper
- **FILE-010**: `service/Api/src/Module/Catalog/Persistence/Seeders/Taxonomy.Seeder.cs` — drop hardcoded, inject helper
- **FILE-011**: `service/Api/src/Module/Catalog/Persistence/Seeders/Taxon.Seeder.cs` — drop hardcoded, inject helper
- **FILE-012**: `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs` — drop hardcoded, inject helper
- **FILE-013**: `service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs` — inject helper
- **FILE-014**: `service/Api/src/Module/Catalog/Catalog.Extension.cs` — register DemoJsonHelper as scoped
- **FILE-015**: `service/Api/src/Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs` — drop hardcoded, inject helper
- **FILE-016**: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs` — drop hardcoded, inject helper
- **FILE-017**: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs` — drop hardcoded, inject helper
- **FILE-018**: `service/Api/src/Module/Inventory/Inventory.Extension.cs` — register DemoJsonHelper as scoped
- **FILE-019**: `service/Api/src/Module/Module.csproj` — remove Content item group for Data/*.json
- **FILE-020**: `service/Api/src/Api/appsettings.Development.json` — add `Seeders:DemoDataPath`
- **FILE-021**: `service/Api/src/Module/Catalog/Persistence/Seeders/Data/` — DELETE directory
- **FILE-022**: `service/Api/src/Module/Inventory/Persistence/Seeders/Data/` — DELETE directory

## 6. Testing

- **TEST-001**: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 2000 --skip-embeddings --deploy` — produces output/images/ (512+ files for 2000 products), output/*.json (all valid), infra/Storage/demo/images/ populated
- **TEST-002**: `python -c "import json; d=json.load(open('benchmarks/scripts/demo-seed/output/demo_taxons.json')); assert len(d)>0; assert all('rgt' in x for x in d); print(f'PASS: {len(d)} taxons')"` — no sentinel entries, all have rgt
- **TEST-003**: `dotnet build` — 0 warnings, 0 errors (warnings-as-errors enforced)
- **TEST-004**: Run app, check DB for seeded entities: `SELECT count(*) FROM catalog.products` > 0, `SELECT count(*) FROM catalog.variants` >= 2000, `SELECT count(*) FROM catalog.product_images` >= 4000
- **TEST-005**: Re-run app — all seeders skip via HasDataAsync, no duplicate inserts
- **TEST-006**: Remove JSON files → run app → seeders return Ok without seeding (graceful no-op)

## 7. Risks & Assumptions

- **RISK-001**: Embedding sidecar memory — generating embeddings for 4000+ search images × 4 models = 16,000 API calls may cause OOM or timeout on the sidecar; mitigate by batching or reducing model count
- **RISK-002**: Startup time — seeding 2000 products with variants, images, embeddings, stock, movements may take 30-60 seconds at app startup; acceptable for dev but monitor
- **RISK-003**: Image disk space — 2000 products × ~2 sizes × ~50KB = ~200MB output images; ensure sufficient disk space
- **ASSUMPTION-001**: The `DemoJsonHelper` injection pattern (scoped service) is compatible with the existing `AbstractDataSeeder` base class and `IDataSeeder` registration — seeders already use DI for `IApplicationDbContext`, adding another parameter is straightforward
- **ASSUMPTION-002**: `IConfiguration` is available in the seeders' DI scope — seeders run during app startup, after configuration is built
- **ASSUMPTION-003**: Removing hardcoded fallback is safe — the app is only used by developers who have the dataset and run the ETL pipeline first

## 8. Related Specifications / Further Reading

- [Demo Seeders Design Spec](../docs/superpowers/specs/2026-07-15-demo-seeders-design.md)
- [Demo Seeders V1 Implementation Plan](../docs/superpowers/plans/2026-07-15-demo-seeders.md)
- [ReSys.Shop AGENTS.md](../AGENTS.md)
