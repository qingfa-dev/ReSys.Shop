# Design: Thesis Revision — Evidence-First

**Date:** 2026-07-30
**Branch:** `feature/thesis-revision`
**Status:** Draft (awaiting user review)
**Inputs:** Four-pass peer review (`thesis/review/pass{1..4}/*`); benchmark source inspection (`benchmarks/src/benchmark/*`)

## 1. Goal

Bring the defended thesis at `thesis/` to publication/archival quality by resolving every finding from the four-pass review. The thesis is already accepted — no defense deadline — so the standard is publication-quality reproducibility, not minimum-viable-defense.

## 2. Approach: Evidence-First

Three phases, strictly sequenced. Phase 1 gates Phase 2 (text cannot be written until the numbers are final); Phase 2 gates Phase 3 (polish cannot begin until structure is final).

| Phase | Scope | Why it gates the next |
|---|---|---|
| **1 — Evidence** | Benchmark code changes + full re-run (13 models × 3 folds × 2 label schemes + pipeline end-to-end + baselines) | Data drives every rewrite; writing before data lands means writing twice |
| **2 — Structure** | Chapter rewrites, moves, deletions, reconciliations across all 4 chapters + back matter | Polish assumes stable structure |
| **3 — Polish** | Redundancy, tone, terminology, screenshots, grammar, bibliography | Final pass |

The key empirical insight that motivated this sequencing: most "statistical rigor" critiques flagged by reviewers are **text fixes, not code changes** — the benchmark code already computes bootstrap 95% CIs, mean±SD for all metrics (including P@K/R@K), uses seed=42, and runs 10 warmup + 100 timed iterations. The thesis text simply did not report them. The actual re-run scope is small and targeted.

## 3. Source-Code Facts That Shape the Plan

Inspection of `benchmarks/src/benchmark/` resolved five of the reviewers' critiques before any re-run:

| Reviewer critique | Code reality | Fix type |
|---|---|---|
| "CLIP-generic unidentified" | `clip_generic.py:20` → `openai/clip-vit-base-patch32` (ViT-B/32) | Text: state it explicitly |
| Architecture confound (Fashion-CLIP vs wrong baseline) | `clip_vit_b16.py:20` → `openai/clip-vit-base-patch16` exists; same backbone as Fashion-CLIP. `THESIS_MODEL_KEYS` currently excludes it | Code: add `clip-vit-b16` to thesis run |
| "No random seed" | `_constants.py:25` → `MAGIC.SEED = 42`, used in splits + bootstrap | Text: document it |
| "No 95% CI" | `evaluation/stats.py:67` → `bootstrap_ci(10_000 resamples, seed)` already computed and stored as `aggregate["map"]["ci_95"]` | Text: report `ci_95` instead of ±2σ |
| "No effect size" | `evaluation/stats.py:38` → `cohens_d` exists but is not called from thesis runner | Code: wire into post-run comparison |
| "No warm-up protocol" | `_constants.py:29` → `WARMUP_RUNS=10`, `BENCHMARK_RUNS=100` | Text: document it |
| "P@K/R@K no variance" | `evaluation/thesis.py:144` → `aggregate_mean_std` is called for ALL metric keys | Text: report the SDs |
| "Coarse relevance = classification" | `evaluation/thesis.py:103` → `_run_with_label_field` supports multi-label; `thesis_results_pattern.json` already produced but treated as secondary | Config: elevate multi-label to primary |
| "Only 4 of 11 models evaluated" | `_constants.py:131` → `THESIS_MODEL_KEYS = [fashion-clip, resnet-50, efficientnet-b0, clip-generic]` — registry has 11, default run uses 4 | Code: expand keys |
| Missing baselines | No color-histogram or random baseline adapter exists | Code: add both (~100 lines) |

## 4. Phase 1 — Evidence Re-Run

All code work is in `benchmarks/`. TDD required per `benchmarks/AGENTS.md` (RED → GREEN → ruff → commit).

### 4.1 New Model Adapters (~100 lines)

**`models/color_histogram.py`** — `ColorHistogramModel` subclass of `EmbeddingModel`. HSV histogram flattened to fixed-dim vector (8×8×8 = 512 bins to match the contract), L2-normalized. Distance metric: cosine similarity on histograms (standard for Bhattacharyya approximation). Tests: `test_models.py` extends with color-histogram case.

**`models/random_baseline.py`** — `RandomBaselineModel`. Seeded via `hash(image_bytes) % 2^32` so results are deterministic and reproducible. Returns random unit vector. Tests: verifies determinism across runs.

Both registered in `models/__init__.py` `_register()` and `get_registry()`.

### 4.2 Configuration Updates

- `THESIS_MODEL_KEYS` → all 11 + 2 baselines = **13 models**.
- `MAGIC.DEFAULT_THESIS_K_VALUES` unchanged (`[5, 10, 20]`).

### 4.3 Thesis Runner Extensions

- Add `"ndcg"` to `metric_keys` in `_evaluate_fold` (the metric exists; just not in the thesis run).
- Add pairwise Cohen's d post-run: for each model pair, `stats.cohens_d(fold_maps_a, fold_maps_b)` produces an effect-size matrix. Persisted as `thesis_cohens_d.json`.
- Verify EfficientNet-B0 std dev (±0.0007) against actual per-fold mAPs — if genuine, document the cause (likely trivially easy 5-class task); if bug, fix `aggregate_mean_std` usage.

### 4.4 Execution

1. `uv run benchmark thesis --dataset-root <PATH> --models <all 13>` (default category-only, produces `thesis_results.json`)
2. `uv run benchmark thesis --dataset-root <PATH> --secondary-label label_pattern` (multi-label = subCategory + baseColour + pattern, produces `thesis_results_pattern.json`)
3. `uv run benchmark pipeline --dataset-root <PATH>` with pgvector container running (`infra/init.sql`, Podman per `docs/08-replication-guide.md` §5) → `pipeline_results.json` with end-to-end latency
4. Generate Typst tables + charts: `uv run benchmark report --format typst`

### 4.5 Phase 1 Deliverables

| Artifact | Resolves |
|---|---|
| `thesis_results.json` (13 models, 3 folds, category-only) | Model breadth; per-fold values for SD computation |
| `thesis_results_pattern.json` (13 models, multi-label PRIMARY) | Core construct-validity critique |
| `pipeline_results.json` (RQ3 end-to-end) | RQ3 empirical evidence |
| `thesis_cohens_d.json` (pairwise effect sizes) | "No effect size" |
| Per-fold mAP values in aggregate output | "No variance" for P@K/R@K; EfficientNet-B0 std dev audit |
| nDCG column in tables | nDCG mentioned-but-unreported |

### 4.6 Expected Results Shift (Honest Forecast)

The plan must not assume conclusions. Based on code inspection:

- **Fashion-CLIP margin will likely shrink** vs the correct ViT-B/16 control — the headline claim of "5.4% advantage" will need reframing as "domain fine-tuning contributes ~X pp when architecture is held constant."
- **Multi-label primary will lower all mAPs** — finer ground truth is harder, but more honest.
- **RQ3 end-to-end may exceed 300 ms** on consumer CPU → the "sub-300 ms" claim revises down to "sub-second" (matching the original Ch.1 phrasing).

If any of these materialize, the plan accommodates them without rewriting earlier decisions.

## 5. Phase 2 — Structural Rewrite (Thesis Text)

### 5.1 Chapter 1 — Introduction

- **Expand DSR to ~1 page**: Hevner (2004) guideline mapping, Peffers (2007) step trace, one concrete design iteration documented.
- **Remove all pre-announced results**: Fashion-CLIP winner declaration, mAP scores, model-ranking claims → move to Ch.3. Keep only candidate-model list, evaluation protocol, hypothesis framing.
- **Add cold-start scope disclaimer**: retrieval (similarity-based discovery) ≠ recommendation (preference prediction). One paragraph.
- **Downgrade "engineering gap"**: to "architectural trade-off analysis." Add paragraph comparing polyglot sidecar vs ONNX Runtime in-process vs ML.NET (cite Microsoft docs).
- **State explicitly**: CLIP-generic = OpenAI CLIP ViT-B/32 (`openai/clip-vit-base-patch32`).
- **Fix preprocessing claim**: "ImageNet normalization applied uniformly" → "each model uses its own `CLIPProcessor`/`AutoImageProcessor` from its model card." The code is correct; the text was wrong.

### 5.2 Chapter 2 — Background

- **Reconcile model registry**: align the thesis's model table to the actual 11-model registry (FashionCLIP, CLIP-B/32, CLIP-ViT-B/16, CLIP-L/14, CLIP-generic, SigLIP, EVA-CLIP, DINOv2-S/14, ResNet-50, EfficientNet-B0, ConvNeXt-Tiny). Drop phantom ResNet-101/152 references or add adapters.
- **Add DeepFashion2 (Ge et al., CVPR 2019)** citation.
- **Fix DINOv2 "ignores colour"** → "deprioritizes low-level colour features."
- **Fix CNN "processes patches"** → "applies convolutional kernels across spatial dimensions."
- **Neutralize Fashion-CLIP narrative**: present all models with documented trade-offs; no arc toward inevitable selection.
- **Move model selection decision** to Ch.3 (the selection is evidence-driven, so it belongs where the evidence is).

### 5.3 Chapter 2 — Design (Pass 2 content)

- **Resolve vector dimensionality**: document the actual DB schema. If multiple models produce different dims, show how `vector(512)` is used per-model (or document the real column type).
- **pgvector version**: correct 0.3.2 → ≥0.5.0 (HNSW introduced in 0.5.0).
- **Add `SET hnsw.ef_search = 100`** to the schema DDL example.
- **Unify HNSW latency**: one measured number with documented test conditions (sample size, percentile, hardware).
- **Fix embedding-table naming**: reconcile `variant_images.embedding` vs `product_image_embeddings.embedding`.
- **Fix CBIR endpoint path**: unify C# (`/api/catalog/storefront/search-by-image`) and TypeScript (`/api/storefront/search-by-image`) — verify the actual deployed path.
- **Fix terminology**: settle "9 business modules" vs "8 bounded contexts" (document the relationship once, use consistently).
- **Restore FR traceability**: declare a core-FR subset (e.g., 15–20 FRs) traced end-to-end through UC → architecture → implementation; appendix the rest.
- **Condense use cases**: keep 3–5 representative UCs inline with brief description; move full 26 UC table to appendix.
- **Foreground CBIR** in a dedicated subsection: embedding pipeline per model, search index configuration, query flow, result quality with examples.
- **Add design-decision rationale** (3–5 brief paragraphs): pgvector vs Milvus/Qdrant, CQRS + MediatR for module isolation, Redis for hybrid caching, Carter over controllers, Hangfire for background jobs.
- **Fix pgvector ACID overclaim**: acknowledge Hangfire async queue breaks the atomic-update story for newly uploaded products; revise vector-DB comparison table to represent Qdrant/Milvus honestly (both open-source, both free).
- **Add out-of-scope statement**: tax computation, returns/RMA, discounts/promotions, product reviews.
- **Replace ~40 screenshot placeholders** with real captures of the running system.
- **Fix duplicated paragraph** at lines 2920 & 2956; fix typo "realization" → "realizes" (line 2820); "a unconstrained" → "an" (line 3071).

### 5.4 Chapter 3 — Evaluation (rewritten from Phase 1 data)

This chapter is largely **rewritten**, not edited. The structure is stable but the content depends on the re-run numbers.

- **New result set**: 13 models × 2 label schemes (multi-label primary, category-only supplementary).
- **Statistics**: report bootstrap `ci_95` instead of "±2σ ≈ 95% CI"; report per-fold values in an appendix table; report SD for P@K/R@K; Cohen's d matrix for key comparisons.
- **Baseline anchors**: color-histogram mAP and random mAP, with "deep learning adds X pp over color histogram" interpretation.
- **Fashion-CLIP vs CLIP-ViT-B/16 comparison**: isolates domain fine-tuning from architecture effect.
- **RQ3 end-to-end latency table**: inference + HTTP overhead + pgvector query + assembly + total.
- **Recompute and fix**: 5.4% vs 6.1% contradiction; "~30 relevant items" (recompute against actual ground-truth pool).
- **Document measurement protocol**: seed=42, warmup=10/bench=100, batch=64, framework versions, HF model IDs with commit hashes, per-model preprocessing pipeline, halfvec storage precision.
- **Add testing-strategy bridge paragraph**: explain that functional correctness testing (unit/integration/E2E) is complemented by the quantitative benchmark that follows.
- **Interpret low recall values**: add 2–3 sentences explaining that R@20 ≈ 11% is expected given the category-based ground-truth pool (~N relevant items per query), and discuss user-experience implications.

### 5.5 Chapter 4 — Conclusion + Back Matter

- Fix "five" → "seven" accuracy metrics.
- Refresh all quantitative claims from Phase 1 data.
- Replace rhetorical question ("Do some models degrade gracefully...?") with declarative statement.
- Verify `.bib` completeness and IEEE style; add DeepFashion2, ONNX Runtime, Microsoft Aspire entries.
- **Add repository URL + license** (e.g., MIT or Apache 2.0) — single sentence in the intro or conclusion.

## 6. Phase 3 — Polish

Single global pass.

- **Redundancy elimination**: semantic gap (define once), 770B/1T stats (state once), vertical-slice (define once), CBIR (define once), Fashion-CLIP selection narrative (neutral).
- **De-textbook compression**: ~40–50% reduction in fundamentals (cosine similarity math, CNN basics, HNSW internals) — replace with concise citations to canonical sources.
- **Terminology control**: settle "AI sidecar" / "ML sidecar" / "Python sidecar" / "embedding service" on one term; document in the ubiquitous-language glossary.
- **Transition smoothing**: 1–2 bridging sentences at each section boundary.
- **Grammar pass**: run-on sentences (≤25 words for technical prose), "subjected to" → "underwent," "now" draft markers removed.
- **Tone calibration**: confidence without overclaiming ("we selected X because Y" vs "X is clearly the best choice").
- **Final consistency audit**: model count (13 throughout), training-data figures (1.2M vs 1.28M), figure/table cross-references, acronym first-use definitions.

## 7. Sequencing, Verification, Risks

### 7.1 Gates

- Phase 1 → 2: all Phase 1 deliverables present and numbers finalized.
- Phase 2 → 3: all chapter rewrites complete; no pending P0 items.

### 7.2 Verification

- `make` (or the thesis Makefile) builds clean with no Typst errors.
- `uv run ruff check src/ && uv run pytest --ignore=src/tests/integration/` in `benchmarks/` green.
- Spot re-review of Ch.3 using the same reviewer panel, focused on "does the new data support the rewritten claims?"
- Final pass of the bibliography against rendered PDF (all cited, no orphaned entries, IEEE style correct).

### 7.3 Risks (expected, not failure)

| Risk | Mitigation |
|---|---|
| Fashion-CLIP margin shrinks vs correct ViT-B/16 control | Reframe as "domain fine-tuning contributes X pp when architecture is held constant" — honest and still publishable |
| Multi-label primary lowers all mAPs | Re-scope claims from "visual retrieval quality" to "fine-grained visual-similarity retrieval quality"; acknowledge the coarser category-only numbers as a secondary metric |
| RQ3 end-to-end exceeds 300 ms | Revise to "sub-second" (matches original Ch.1 phrasing); this is more honest and still defensible |
| EfficientNet-B0 std dev is a real calculation bug | Fix `aggregate_mean_std` usage in thesis runner; document in changelog |
| Screenshot captures require running system | Schedule after Phase 2 is stable; the running system exists in this repo |

## 8. Success Criteria

The revision is complete when:

1. All P0 items across all 4 passes are resolved.
2. All P1 items are resolved or explicitly deferred with rationale.
3. The benchmark re-run produces 13-model × 3-fold × 2-label results + end-to-end RQ3 latency.
4. The thesis text contains no pre-announced results, no internal contradictions, no placeholder figures.
5. The repository URL and license are stated.
6. The `.bib` is complete and IEEE-compliant.
7. `make` builds clean; `uv run ruff check src/` and `uv run pytest` green.

## 9. Out of Scope

- Rewriting the benchmark framework architecture (the framework is sound; only usage and reporting change).
- Adding new features to the e-commerce system (ReSys.Shop is frozen for this revision).
- Translating the thesis to another language (English-only archival).
- Formal user studies on the deployed search UI (already acknowledged as a limitation and future-work item).
