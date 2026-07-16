# v2 Wave 3 — Scale to 2000+ variants & multi-model embeddings

**Date**: 2026-07-15
**Tasks**: TASK-023 through TASK-026

## Summary

Scaled the demo seed ETL pipeline from 200 to 2000+ product groups and added multi-model embedding support, enabling the demo seed to generate embeddings for all available image embedding models (not just fashion_clip).

## Changes

### TASK-023: Scale default count in extract_products.py
- Changed `--count` default from 200 to 2000 in `extract_products.py:71`
- Changed `--count` default from 200 to 2000 in `run_all.py:24`
- Verified no unused `import random` (not present)

### TASK-024: Multi-model embeddings in generate_embeddings.py
- Added model discovery via `GET /models` endpoint on the embedding sidecar
- Falls back to 4 known models if `/models` fails: fashion_clip, efficientnet_b0, clip_vit_b16, dinov2_vits14
- Each search image now gets embeddings for ALL available models
- Embedding records include `model_name` from the model's registry ID

### TASK-025: Multi-model image sizes in process_images.py
- Added `MODEL_INPUT_SIZES` dict (224px for all current models)
- Search images are now produced at each unique input size
- Directory layout: `output/images/search/{size}/` (currently only `224/`)

### TASK-026: Per-model image records — verified
- Confirmed extract_products.py generates 2 image records per variant (Default + Search) — correct
- Multi-model change is only in generate_embeddings.py (multiple embeddings per image)
- `--count` default is 2000, no unused imports

## Verification

- `dotnet build` — 0 warnings, 0 errors
- ETL pipeline tested with `--count 5 --skip-embeddings --deploy`: 5 products, 14 variants, 28 images, 14 search images at 224px
- No regressions in existing ETL pipeline behavior
