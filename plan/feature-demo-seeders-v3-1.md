---
goal: "Refine ETL pipeline with size arguments, master-only search images, duplicate checks, clear process"
version: 3.0
date_created: 2026-07-15
last_updated: 2026-07-15
owner: Platform
status: "Planned"
tags: ["feature", "data", "refactor"]
---

# Introduction

![Status: planned](https://img.shields.io/badge/status-planned-blue)

V3 refines the demo-seed ETL pipeline with explicit resize arguments (by pixel or by model name),
reduces search/embedding scope to master variants only (1 per product), adds duplicate detection,
and improves CLI ergonomics for easy replication.

## 1. Requirements & Constraints

- **REQ-001**: `process_images.py` accepts `--display-size` (default 512) and `--search-size` (default 224) — both accept integer pixels OR `model:<id>` syntax that resolves to the model's expected input size
- **REQ-002**: Only master variants (`is_master=true`) get a Search-type variant image + embedding; every variant keeps its Default display image
- **REQ-003**: `run_all.py` forwards `--display-size` and `--search-size` to sub-scripts
- **REQ-004**: `extract_products.py` generates the correct `storage_path` for search images matching the requested size
- **REQ-005**: Duplicate detection — `extract_products.py` and `extract_taxonomies.py` warn if output JSON files already exist, and `--force` flag overwrites them
- **REQ-006**: All Python scripts print clear step headers and progress for replicability
- **REQ-007**: `run_all.py` prints a summary table at the end showing products/variants/images/embeddings counts
- **CON-001**: `infra/Storage/demo/` remains runtime-only; Python writes images to `output/images/`, deploy copies to infra
- **CON-002**: JSON retains snake_case; C# `DemoJsonHelper` unchanged
- **CON-003**: Build must pass with 0 warnings, 0 errors

## 2. Implementation Steps

### Implementation Phase 1: Size Arguments + Master-Only Search

GOAL-001: Add `--display-size` and `--search-size` arguments. Reduce search images to master variants only.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `extract_products.py`: add `--display-size` (int, default 512) and `--search-size` (int, default 224) args. Move search image generation inside `if vi == 0:` block (master only). Use `args.search_size` in `storage_path` instead of hardcoded `224`. Add `--force` flag to skip duplicate check | | |
| TASK-002 | Update `process_images.py`: add `--display-size` (default 512) and `--search-size` (default 224) args accepting `int` or `model:<id>` syntax. When `model:<id>`, resolve size from `MODEL_INPUT_SIZES` dict. Remove hardcoded 512/224. Print `Processing display images at {size}px` and `Processing search images at {size}px` headers | | |
| TASK-003 | Update `generate_embeddings.py`: no changes needed (already reads `demo_variant_images.json` and only processes Search-type images — which are now only on master variants) | | |
| TASK-004 | Update `run_all.py`: add `--display-size` and `--search-size` args forwarded to sub-scripts. Print numbered steps with headers (Step 1/5: Extract Taxonomies, etc.). Print final summary table after all steps | | |

### Implementation Phase 2: Duplicate Detection + Clear Process

GOAL-002: Add `--force` override, step-by-step output, and final summary.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Update `extract_taxonomies.py`: if `demo_taxonomies.json` exists in output dir and `--force` not set, print "Output already exists, use --force to overwrite" and exit; add `--force` flag | | |
| TASK-006 | Update `extract_products.py`: same duplicate detection for `demo_products.json`; add `--force` flag | | |
| TASK-007 | Update `extract_stock.py`: same duplicate detection for `demo_stock_locations.json`; add `--force` flag | | |
| TASK-008 | Update `run_all.py`: add `--force` flag forwarded to all sub-scripts; add final summary table printing counts from each JSON file | | |

### Implementation Phase 3: Verification

GOAL-003: End-to-end validation with master-only search images.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Run `uv run python benchmarks/scripts/demo-seed/run_all.py --count 200 --skip-embeddings --deploy --force --display-size 512 --search-size 224` — verify output | | |
| TASK-010 | Verify: search images count == product count (master only). `python -c "import json; v=json.load(open('benchmarks/scripts/demo-seed/output/demo_variant_images.json')); search=[x for x in v if x['type']=='Search']; default=[x for x in v if x['type']=='Default']; products=json.load(open('benchmarks/scripts/demo-seed/output/demo_products.json')); print(f'Products: {len(products)}, Search images: {len(search)}, Default images: {len(default)}'); assert len(search) == len(products), 'FAIL: search != products'"` | | |
| TASK-011 | Run full build `dotnet build` — 0 warnings, 0 errors | | |

## 3. Alternatives

- **ALT-001**: Keep search images per-variant (current v2 behavior) — rejected per REQ-002; generates 3x more embeddings and images than needed since embeddings are product-level, not variant-level
- **ALT-002**: Use only `--size` without model resolution — rejected; model-based sizing (`model:fashion_clip`) is more intuitive for ML practitioners who think in model names, not pixel dimensions
- **ALT-003**: Hard error on duplicate output — rejected; `--force` flag is more user-friendly for re-running the pipeline

## 4. Dependencies

- **DEP-001**: `benchmarks/pyproject.toml` — Pillow, httpx, tqdm already present
- **DEP-002**: Kaggle dataset at `benchmarks/data/raw/fashion-product-images/`
- **DEP-003**: `infra/Storage/demo/` directory (exists)

## 5. Files

- **FILE-001**: `benchmarks/scripts/demo-seed/extract_products.py` — search image only for master; add size args; add --force
- **FILE-002**: `benchmarks/scripts/demo-seed/process_images.py` — add --display-size/--search-size with model resolution
- **FILE-003**: `benchmarks/scripts/demo-seed/run_all.py` — forward new args; numbered steps; summary table; --force
- **FILE-004**: `benchmarks/scripts/demo-seed/extract_taxonomies.py` — add --force duplicate check
- **FILE-005**: `benchmarks/scripts/demo-seed/extract_stock.py` — add --force duplicate check
- **FILE-006**: `benchmarks/scripts/demo-seed/generate_embeddings.py` — verify no changes needed

## 6. Testing

- **TEST-001**: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 50 --skip-embeddings --deploy --force` — all steps complete, output/ populated, infra/Storage/demo/ populated
- **TEST-002**: Verify search images == product count (master-only invariant)
- **TEST-003**: Re-run without `--force` — should warn "output already exists" and exit
- **TEST-004**: Run with `--search-size model:fashion_clip` — should resolve to 224
- **TEST-005**: `dotnet build` — 0 warnings, 0 errors
- **TEST-006**: Verify generated `demo_variant_images.json` — no search image has `variant_id` pointing to a non-master variant

## 7. Risks & Assumptions

- **RISK-001**: Changing `storage_path` format breaks C# image serving — the `storage_path` is relative to `LocalPath`, so as long as the directory structure under `infra/Storage/demo/` matches, no C# changes needed
- **ASSUMPTION-001**: `process_images.py` model-to-size resolution via `MODEL_INPUT_SIZES` dict covers all models users will specify; unknown models cause a clear error message
- **ASSUMPTION-002**: The `--force` flag only checks for existing output JSON files, not individual image files — image regeneration is idempotent (overwrites in place)

## 8. Related Specifications / Further Reading

- [V1 Design Spec](../docs/superpowers/specs/2026-07-15-demo-seeders-design.md)
- [V2 Implementation Plan](feature-demo-seeders-v2-1.md)
