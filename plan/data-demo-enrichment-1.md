---
goal: "Enrich demo product/variant metadata with season, material, care instructions, multi-view images, and brand info from styles JSON"
version: 1.0
date_created: 2026-07-15
owner: Platform
status: "Planned"
tags: ["data", "feature"]
---

# Introduction

![Status: planned](https://img.shields.io/badge/status-planned-blue)

Enrich `extract_products.py` output with metadata from per-product JSON files (`styles/<id>.json`)
— season, material, care instructions, department, style code, multi-view images — to make
seeded products display richer data in the admin/storefront UI.

## 1. Requirements & Constraints

- **REQ-001**: `demo_products.json` adds fields: `style_code`, `season_name`, `material_composition`, `care_instructions`, `department`, `meta_title`
- **REQ-002**: `demo_variant_images.json` adds Gallery-type images from `styleImages.back` and `styleImages.front` (when available)
- **REQ-003**: `demo_variants.json` adds `hs_code` derived from the benchmark `articleNumber`
- **REQ-004**: Extraction from JSON is best-effort — missing files or fields produce null/defaults, not errors
- **REQ-005**: C# `Product.Seeder.cs` records must be updated to map the new JSON fields to domain properties
- **CON-001**: Warnings-as-errors — any warning fails the .NET build
- **CON-002**: `productDescriptors.description.value` is HTML — strip tags for Material/Care extraction
- **CON-003**: Python scripts follow existing conventions (`from __future__ import annotations`, `argparse`, `pathlib.Path`, `tqdm`)

## 2. Implementation Steps

### Implementation Phase 1: Python Metadata Extraction

GOAL-001: Enrich `extract_products.py` to read `styles/<id>.json` and extract metadata fields.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `extract_product_metadata(benchmark_id, dataset_path)` function that loads JSON and extracts: `brand_name`, `season`, `material_composition`, `care_instructions`, `article_attributes`, `style_images` dict, `price`, `discounted_price`, `article_number`. Return dict with null defaults for missing fields | | |
| TASK-002 | Enrich product records: map `season→season_name`, `masterCategory→department`, `productDisplayName→meta_title`, generate `style_code` from brand+articleType. Set `meta_description` from `productDisplayName + articleType + brandName` | | |
| TASK-003 | Add Gallery images: for each product, if `styleImages.back` or `styleImages.front` URLs exist in JSON, generate additional `VariantImage` records with `type: "Gallery"` and `storage_path` pointing to the same benchmark JPEG. Only for the master variant | | |
| TASK-004 | Extract Material/Care from `productDescriptors.description.value` HTML: strip tags with regex, search for "Wash Care" section → `care_instructions`. Extract fabric info → `material_composition` | | |
| TASK-005 | Enrich variant records: add `hs_code` from JSON `articleNumber` (truncated to 20 chars). If JSON has `styleOptions` with size data, capture size values | | |
| TASK-006 | Re-run ETL: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --embedding-mode skip --deploy --force` — verify new fields in JSON files | | |

### Implementation Phase 2: C# Seeder Updates

GOAL-002: Update `Product.Seeder.cs` record types and mapping to consume new JSON fields.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Update `DemoProductJson` record: add `string? StyleCode`, `string? SeasonName`, `string? MaterialComposition`, `string? CareInstructions`, `string? Department`, `string? MetaTitle` | | |
| TASK-008 | Update `SeedFromJsonAsync` mapping: set `product.StyleCode`, `product.SeasonName`, `product.MaterialComposition`, `product.CareInstructions`, `product.Department`, `product.MetaTitle` from JSON | | |
| TASK-009 | Update `DemoVariantJson` record: add `string? HsCode` | | |
| TASK-010 | Update variant mapping: set `variant.HsCode = vj.HsCode` | | |
| TASK-011 | Verify Gallery image handling: `VariantImageType.Gallery` is already an enum member — no code change needed for the image type itself, but ensure new Gallery images from TASK-003 are created only for the master variant | | |

### Implementation Phase 3: Verification

GOAL-003: Build and end-to-end test enriched data.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | `dotnet build` — 0 warnings, 0 errors | | |
| TASK-013 | ETL run + JSON validation: verify new fields appear in `demo_products.json` and have non-null values | | |

## 3. Alternatives

- **ALT-001**: Extract ALL fields from JSON into the product model — rejected per YAGNI; only fields that impact the UI/API display are worth adding
- **ALT-002**: Parse HTML descriptions with a full HTML parser — rejected; tag-stripping regex is sufficient for the well-structured Myntra description format
- **ALT-003**: Use `styleImages` URLs directly (original CDN) — rejected; images are already stored locally as JPEGs from the dataset

## 4. Dependencies

- **DEP-001**: Per-product JSON files at `benchmarks/data/raw/fashion-product-images/styles/<id>.json`
- **DEP-002**: `benchmarks` project packages: `re` (stdlib) for HTML tag stripping

## 5. Files

- **FILE-001**: `benchmarks/scripts/demo-seed/extract_products.py` — add metadata extraction functions + Gallery images
- **FILE-002**: `benchmarks/scripts/demo-seed/process_images.py` — needs NO changes (already handles all images from JSON)
- **FILE-003**: `service/Api/src/Module/Catalog/Persistence/Seeders/Product.Seeder.cs` — update record types + mapping

## 6. Testing

- **TEST-001**: `python3 -c "import json; p=json.load(open('benchmarks/scripts/demo-seed/output/demo_products.json')); print(p[0]); assert 'season_name' in p[0]"` — new fields present
- **TEST-002**: `python3 -c "import json; i=json.load(open('benchmarks/scripts/demo-seed/output/demo_variant_images.json')); gallery=[x for x in i if x['type']=='Gallery']; print(f'Gallery images: {len(gallery)}'); assert len(gallery) > 0"` — gallery images exist
- **TEST-003**: `dotnet build` — 0 warnings, 0 errors

## 7. Risks & Assumptions

- **RISK-001**: Some JSON files may not exist (5 of 44,446 are missing per benchmark docs) — `extract_product_metadata` returns null defaults gracefully
- **RISK-002**: HTML description format may vary across products — regex extraction is best-effort; missing material/care produces null
- **ASSUMPTION-001**: `VariantImageType.Gallery` is a valid enum member — confirmed from `VariantImage.Enumerate.cs`
- **ASSUMPTION-002**: The existing `Product.Seeder.cs` ignores extra JSON fields it doesn't map — true; only mapped fields are used

## 8. Related Specifications / Further Reading

- [V4 Plan](feature-demo-seeders-v4-1.md)
- [Demo Seeders Design Spec](../docs/superpowers/specs/2026-07-15-demo-seeders-design.md)
