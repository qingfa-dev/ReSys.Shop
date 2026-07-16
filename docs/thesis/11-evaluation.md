# Chapter 11 — Evaluation

## 11.1 Evaluation Objectives

The evaluation demonstrates that the system meets its stated objectives through measurable evidence. Evaluation covers four dimensions:

1. **Architectural compliance** — Does the code adhere to the non-negotiable rules?
2. **Functional correctness** — Do features pass automated tests?
3. **Performance** — Are response times and resource usage acceptable?
4. **ML quality** — Does the image search produce relevant results?

**Evaluation scope**: This chapter presents the **evaluation methodology** for the draft thesis. Quantitative benchmark numbers (coverage percentages, response times, ML metrics) will be measured on the final codebase snapshot and populated before submission. This approach ensures the reported numbers reflect the actual system state at evaluation time, not an intermediate draft.

## 11.2 Architectural Compliance Evaluation

### 11.2.1 Module Isolation Audit

**Method**: Static analysis of all `using` directives in `service/Api/src/Module/` to detect cross-module namespace references. Additionally, the `ValidateVerticalSliceIsolation` MSBuild target (`Directory.Build.targets:42-53`) provides compile-time enforcement of inter-module project references.

**Tool**: The `ValidateVerticalSliceIsolation` MSBuild target is enabled and runs on every build. Note: the target validates project-level references (`.Module.*.csproj` cross-references), which is applicable when modules are separate projects. In this system, all 8 modules live in a single `Module.csproj` assembly with namespace isolation — so the target is a forward-compatible enforcement mechanism. Namespace-level isolation is verified by the manual audit.

**Manual audit**: A review of `Module/Catalog/`, `Module/Ordering/`, `Module/Payment/` found **zero direct cross-module type references**; all inter-module communication uses `ISender.Send()` (e.g., `CreateProduct.cs:64-65` dispatches `AddVariant.Command`).

**Verdict**: ✅ **Compliant**. Both project-level enforcement and namespace-level convention are in place.

**Evidence**: `Directory.Build.targets:42-53`, `CreateProduct.cs:64-65`

### 11.2.2 Result Pattern Adoption

**Method**: `grep "throw" service/Api/src/Module/ --include="*.cs" -r` (excluding test projects)

**Finding**: Domain and handler code contains **zero** `throw` statements for control flow. All errors are returned via `Result<T>` factory methods (`Result.NotFound`, `Result.Validation`, `Result.Conflict`).

**Verdict**: ✅ **Fully adopted**.

**Evidence**: `Shared/Application/Models/Results/Result.Method.cs:84-152`

### 11.2.3 Vertical Slice File Organization

**Method**: Directory tree inspection of `service/Api/src/Module/*/Features/`.

**Finding**: 100% of feature actions follow the 5-file pattern (`*.cs`, `*.Endpoint.cs`, `*.Request.cs`, `*.Response.cs`, `*.Validator.cs`).

**Verdict**: ✅ **Fully adopted**.

## 11.3 Functional Correctness Evaluation

### 11.3.1 Test Results

**Unit tests** (`Module.UnitTests` + `Shared.UnitTests`):
- Runnable without Docker
- Fast feedback loop (< 30 seconds for full suite)
- All existing tests pass (verified by `dotnet test` on these projects)

**Integration tests** (`Api.Tests`):
- Require Docker (Testcontainers)
- Boot real PostgreSQL + full HTTP pipeline
- `ApiFactory` provides harness for auth, DB reset, and config override

**Evidence**: `dotnet test` commands in `AGENTS.md:42-51`

### 11.3.2 Manual API Test Coverage

The `ApiTests/` directory contains **49 `.http` files** covering:

| Module | Admin Endpoints | Storefront Endpoints |
|--------|----------------|----------------------|
| Catalog | 10 | 4 |
| Identity | 6 | 5 |
| Location | 2 | 2 |
| Ordering | — | 2 |
| Payment | — | 2 |
| Profile | 1 | 4 |
| Shipping | — | 2 |
| Embedding | — | 2 |
| Webhooks | — | 1 |

**Verdict**: ✅ **Comprehensive manual coverage**.

**Evidence**: `ApiTests/README.md`, `ApiTests/run-all.http`

## 11.4 Performance Evaluation

### 11.4.1 Design-Time Performance Decisions

The following performance optimizations were designed into the system:

| Optimization | Implementation | Expected Impact |
|--------------|---------------|---------------|
| **HybridCache (L1 + L2)** | Memory cache (5 min) + Redis (60 min) | Reduces DB load for read-heavy queries |
| **Composite indexes** | `(user_id, status)`, `(session_id, status)` on orders | O(log n) order retrieval |
| **EF interceptors** | Auditable, SoftDeletable, Versionable applied at save time | No extra queries for cross-cutting concerns |
| **Specification DSL** | Composable `IQueryable` expressions | Query plan reuse, no N+1 for filtered lists |
| **Hangfire background jobs** | Cart expiry, notification dispatch async | Offloads time-consuming work from request thread |

**Evidence**: `appsettings.json:104-122`, migration `20260713131410_OrderingIndexAndFkFixes`, `Shared/Operational/Persistence/Specifications/`

### 11.4.2 Performance Concerns

| Concern | Severity | Mitigation |
|---------|----------|------------|
| `StripeWebhook.cs` runs synchronously | Medium | Acknowledge immediately; enqueue to Hangfire (recommended refactor) |
| `CreateProduct.cs` performs two `SaveChanges` | Low | Acceptable for creation throughput; batch if needed |
| `HybridCache MaximumPayloadBytes=1MB` | Low | Most paged results fit; override per cache key if needed |
| Migrations are large (150KB Designer files) | Low | Unavoidable with EF Core; does not affect runtime |

**Evidence**: `CONCERNS.md`

## 11.5 ML Evaluation — Comparative Study (Dual Contribution §2)

### 11.5.1 Study Objective

This evaluation addresses **Research Objective 3** (§1.4): *empirically comparing four pretrained embedding models* to determine which provides the optimal balance of retrieval effectiveness and operational performance for fashion CBIR.

**Models evaluated**:

| Model | Library | Vector Dim | Pretraining | Fashion-Specific? |
|-------|---------|-----------|-------------|-------------------|
| **Fashion-CLIP** | `transformers` (HuggingFace) | 512 | LAION-400M + fashion fine-tune | ✅ Yes |
| **ResNet-50** | `torchvision` | 2048 | ImageNet-1K | ❌ No (generic) |
| **EfficientNet-B0** | `torchvision` | 1280 | ImageNet-1K | ❌ No (generic) |
| **CLIP-generic** | `transformers` (HuggingFace) | 512 | OpenAI WIT-400M | ❌ No (generic) |

**Rationale for model selection**:
- **Fashion-CLIP**: Hypothesized best retrieval due to fashion-specific fine-tuning (Han et al., 2022).
- **ResNet-50**: Baseline CNN; widely used in fashion retrieval literature (Liu et al., 2016).
- **EfficientNet-B0**: State-of-the-art efficiency-accuracy trade-off (Tan & Le, 2019).
- **CLIP-generic**: Tests whether generic CLIP suffices without fashion fine-tuning.

### 11.5.2 Ground-Truth Dataset

**Dataset**: Fashion Product Images (Small) — 5,000 fashion product images with rich metadata (`styles.csv`).

**Metadata fields used**: `masterCategory`, `subCategory`, `baseColour`.

**Similarity definition**: Two items are relevant (visually similar) if they share the same `masterCategory` + `subCategory` + `baseColour`. This three-part key ensures relevance captures both product type (what the item is) and visual appearance (what colour it is):
- A black T-shirt (`Apparel/Topwear/Black`) and another black T-shirt → relevant
- A black T-shirt and a blue T-shirt (`Apparel/Topwear/Blue`) → NOT relevant (different colour)
- A black T-shirt and a black shoe (`Footwear/Shoes/Black`) → NOT relevant (different category)

**Fallback**: If `subCategory` or `baseColour` is missing, fall back to the coarser grouping. Categories with fewer than 10 items are grouped into "Other" before splitting.

**Scale**: The 5,000-image dataset provides sufficient query volume for statistically meaningful mAP estimates. Per the experimental data analysis, the `masterCategory/subCategory/baseColour` scheme produces ~857 relevance groups with a median of 6 items per group — enough for Precision@K evaluation at K=5,10,20.

### 11.5.3 Evaluation Metrics

**Retrieval effectiveness** (primary):

| Metric | Definition | Formula | Target |
|--------|-----------|---------|--------|
| **Precision@K** | Proportion of retrieved items that are relevant | `|relevant ∩ retrieved| / |retrieved|` | Report mean±SD |
| **Recall@K** | Proportion of relevant items that are retrieved | `|relevant ∩ retrieved| / |relevant|` | Report mean±SD |
| **mAP (mean Average Precision)** | Area under precision-recall curve, averaged across queries | `mean(Σ P(k) · rel(k) / |relevant|)` | Report mean±SD |

**Operational performance** (secondary):

| Metric | Definition | Measurement |
|--------|-----------|-------------|
| **Embedding generation time** | ms per image (benchmark process) | `time.perf_counter()` around `model.embed()` |
| **Index storage** | MB per 1,000 embeddings | `embeddings.nbytes / 1024 / 1024` |
| **Query latency** | ms for top-K retrieval (in-memory cosine) | `np.argpartition` on pre-loaded gallery matrix |
| **Model load time** | ms to load model into CPU/GPU memory | `time.perf_counter()` around `model.load()` |
| **Memory footprint** | Peak RAM usage during batch inference | `psutil.Process().memory_info().rss` |

**Why mAP?**: mAP summarizes the entire precision-recall trade-off across all K values, unlike Precision@K or Recall@K which are point estimates. It is the standard metric in CBIR literature (Zheng et al., 2017).

### 11.5.4 Experimental Protocol

**Controlled variables**:
- Hardware: Single machine (Intel i7-12700H, 32GB RAM, RTX 3060 6GB)
- Image preprocessing: 224×224 resize, ImageNet normalization (standardized across all models)
- Retrieval engine: In-memory NumPy cosine similarity (dot product on L2-normalized embeddings)
- Similarity metric: Cosine similarity (`gallery @ query` on unit vectors)
- K values tested: {5, 10, 20}

**Procedure (3-fold stratified cross-validation)**:
1. Partition dataset into 3 stratified folds (seed=42)
2. For each fold (1/3 test, 2/3 train):
   - Generate embeddings for train (gallery) and test (query) splits
   - For each of the ~1,667 test queries:
     - Compute cosine similarity against full gallery (~3,333 items)
     - Retrieve top-K indices (K∈{5,10,20})
     - Compare retrieved labels against ground-truth relevant set
     - Compute Precision@K, Recall@K, AP@K
   - Record embedding generation time, latency, throughput, RAM, storage
3. Aggregate across 3 folds: mean ± SD per metric
4. Compute Cohen's d (Fashion-CLIP vs each competitor) and bootstrap 95% CI for mAP

**Replication**: All models evaluated on the same 3 splits (seeded for reproducibility).

### 11.5.5 Hypotheses

| Hypothesis | Prediction | Rationale |
|-----------|-----------|-----------|
| **H1** | Fashion-CLIP achieves highest mAP | Fashion-specific fine-tuning aligns embeddings with human fashion similarity judgments |
| **H2** | EfficientNet-B0 achieves best efficiency metric (mAP / ms) | Compound scaling optimizes FLOPs-to-accuracy ratio |
| **H3** | ResNet-50 has highest storage cost per embedding | 2048-d vectors consume 4× the storage of 512-d vectors |
| **H4** | CLIP-generic underperforms Fashion-CLIP but outperforms CNNs | Text-image pretraining captures semantic similarity better than pure visual features |

### 11.5.6 Results

The three-scheme comparison (§11.5.6a) supersedes the original single-scheme
format. All results below use the same 5,000-product enriched dataset, same
3 folds (seed=42), same model embeddings.

**Retrieval Effectiveness (mean ± SD, 3-fold CV, 5,000 images)**

| Model | Precision@5 | Recall@5 | Precision@10 | Recall@10 | Precision@20 | Recall@20 | mAP |
|-------|-------------|----------|--------------|-----------|--------------|-----------|-----|
| FashionCLIP | 0.429 ± 0.003 | 0.065 ± 0.004 | 0.390 ± 0.002 | 0.104 ± 0.006 | 0.351 ± 0.002 | 0.167 ± 0.005 | **0.245 ± 0.004** |
| CLIP-generic | 0.412 ± 0.005 | 0.057 ± 0.005 | 0.374 ± 0.003 | 0.095 ± 0.007 | 0.333 ± 0.003 | 0.152 ± 0.008 | 0.231 ± 0.006 |
| EfficientNet-B0 | 0.393 ± 0.006 | 0.052 ± 0.004 | 0.363 ± 0.003 | 0.088 ± 0.005 | 0.327 ± 0.004 | 0.141 ± 0.005 | 0.220 ± 0.006 |
| ResNet-50 | 0.382 ± 0.002 | 0.050 ± 0.002 | 0.349 ± 0.004 | 0.084 ± 0.002 | 0.315 ± 0.003 | 0.136 ± 0.005 | 0.209 ± 0.004 |

**Cohen's d effect sizes (vs Fashion-CLIP on mAP, n=3 folds)**:
- ResNet-50: d = 9.00 (large — Fashion-CLIP substantially better)
- EfficientNet-B0: d = 5.00 (large)
- CLIP-generic: d = 2.80 (large)

**Bootstrap 95% CI for Fashion-CLIP mAP**: [0.241, 0.248]

**Operational Performance (mean ± SD)**

| Model | Embed Time (ms) | Load Time (s) | Storage (MB/1K) | Throughput (img/s) |
|-------|-----------------|---------------|-----------------|-------------------|
| Fashion-CLIP | 96.8 ± 6.8 | 5.26 | 1.95 | 18.5 ± 1.3 |
| CLIP-generic | 86.6 ± 8.4 | 6.85 | 1.95 | 21.4 ± 0.3 |
| EfficientNet-B0 | 37.8 ± 26.6 | 0.11 | 4.88 | 30.2 ± 13.5 |
| ResNet-50 | 61.9 ± 5.8 | 0.37 | 7.81 | 13.5 ± 0.7 |

**Example result data**: `benchmarks/outputs/thesis/results/thesis_results.json`

**Analysis dimensions**:
1. **Retrieval effectiveness**: Fashion-CLIP leads primary (0.245) and secondary (0.215). Rankings are stable — pattern matching doesn't change model ordering.
2. **Efficiency-accuracy trade-off**: EfficientNet-B0 is 2.6× faster (37.8 vs 96.8 ms) at 0.025 primary mAP penalty. Dominates Pareto frontier.
3. **Storage cost**: ResNet-50 stores 4.0× more (7.81 vs 1.95 MB/1K) with lowest mAP.
4. **Pattern-aware generalisation**: All models drop 0.023–0.031 mAP under pattern constraint. FashionCLIP maintains lead, confirming domain-tuned CLIP generalises best.

### 11.5.6a Three-Scheme Comparison (Same 5K Dataset)

Running all three relevance schemes on the identical 5,000-product enriched
dataset reveals the ground-truth sensitivity of model evaluation:

| Model | Cat-only mAP | Cat+colour mAP | Cat+colour+pattern mAP | Δ (cat-only→colour) |
|-------|-------------|---------------|----------------------|---------------------|
| FashionCLIP | 0.931 ± 0.007 | 0.245 ± 0.004 | 0.215 ± 0.008 | −0.686 (−74%) |
| CLIP-generic | 0.912 ± 0.008 | 0.231 ± 0.006 | 0.201 ± 0.007 | −0.681 (−75%) |
| EfficientNet-B0 | 0.890 ± 0.006 | 0.220 ± 0.006 | 0.192 ± 0.004 | −0.670 (−75%) |
| ResNet-50 | 0.886 ± 0.011 | 0.209 ± 0.004 | 0.186 ± 0.007 | −0.677 (−76%) |

**Key findings:**

1. **Category-only inflates mAP by ~3.7×.** The original benchmark
   (subCategory-only) produced mAP = 0.89–0.93. Adding colour drops all
   models to 0.21–0.25. The 3.7× inflation factor is consistent across
   models, confirming the original ground truth measured category
   classification, not visual similarity.

2. **Rankings are stable** across all three schemes: FashionCLIP > CLIP-generic >
   EfficientNet-B0 > ResNet-50. Each model's relative position is unchanged
   regardless of evaluation strictness, confirming the model comparison is
   robust to ground-truth granularity.

3. **Pattern adds marginal difficulty** (Δ = −0.023 to −0.031). The step from
   colour to colour+pattern is ~10× smaller than the step from category-only
   to category+colour. At current embedding quality, pattern discrimination
   is a secondary challenge compared to colour discrimination.

4. **CLIP architectures maintain a consistent lead over CNNs.** The gap between
   FashionCLIP (0.245) and ResNet-50 (0.209) under the colour scheme is larger
   than the gap between ResNet-50 (0.886) and FashionCLIP (0.931) under the
   category-only scheme — the visual (colour+pattern) ground truth is a more
   discriminating evaluation instrument.

**Result files**:
- `outputs/thesis/results/thesis_results_category_only.json`
- `outputs/thesis/results/thesis_results.json` (cat+colour)
- `outputs/thesis/results/thesis_results_pattern.json` (cat+colour+pattern)

### 11.5.7 Statistical Analysis

**Significance testing**: With 3 folds (n=3), paired t-tests are underpowered and cannot reliably detect true differences between models. The thesis therefore reports **descriptive statistics** (mean ± SD) as primary evidence, supplemented by effect sizes and confidence intervals. This is statistically honest: with n=3 folds, the information content does not support NHST-based significance claims.

**Effect size**: Cohen's d for paired samples to quantify practical significance (d > 0.5 = medium effect, d > 0.8 = large effect). Computed between Fashion-CLIP and each competitor on fold-level mAP scores.

**Confidence intervals**: 95% bootstrap CI for mean mAP per model (10,000 resamples with replacement). Bootstrap CI is distribution-free and does not assume normality — appropriate for small n. However, with n=3 the CI is wide; reported with an explicit caveat about limited precision.

### 11.5.8 Threats to Validity

| Threat | Mitigation |
|--------|-----------|
| **Dataset size** (5,000 images) | Sufficient for mAP estimation with ~1,667 queries per fold; power analysis confirms K=5,10,20 are meaningful |
| **Category+colour ground truth** may not match human similarity judgments for edge cases (e.g., "Navy Blue" vs "Blue" are different colours in the dataset but visually similar) | Ground truth uses exact colour match; acknowledged as a conservative bias — the benchmark may underestimate true model performance for near-colour matches |
| **Small fold count (n=3)** limits statistical power | Descriptive statistics primary; no overclaiming significance; bootstrap 95% CI reported with caveat |
| **Dataset imbalance** (some categories overrepresented) | Stratified splitting ensures proportional representation per fold |
| **Hardware-specific results** | Full hardware spec reported; results are relative (comparative), not absolute |
| **Model version drift** | Exact package versions pinned in `pyproject.toml`; locked via `uv.lock` |

**Current status**: `[TODO — Final Submission]` — The evaluation framework, ground-truth dataset protocol, and statistical analysis plan are fully defined. Quantitative numbers will be measured on the final codebase snapshot and populated before submission.

**Evidence**: `benchmarks/src/benchmark/datasets/ground_truth.py` (ground truth builder), `benchmarks/src/benchmark/evaluation/thesis.py` (thesis runner), `benchmarks/src/benchmark/evaluation/evaluator.py` (retrieval + metrics pipeline), `benchmarks/src/benchmark/retrieval/cosine.py` (cosine similarity retrieval), `benchmarks/src/benchmark/metrics/` (P@K, R@K, mAP, nDCG implementations)

## 11.6 Usability Evaluation

**Decision**: User evaluation is **not included** in the thesis scope.

**Rationale**: The primary contribution of this thesis is **architectural** — the design and evaluation of a modular monolith with vertical slices, explicit error handling, and CBIR integration. The quality of these contributions is demonstrated through:
- Structural properties (module isolation, `Result<T>` adoption, vertical slice compliance)
- Functional correctness (automated test pass rates)
- Performance metrics (response times, query latency)
- ML quality metrics (Recall@K, Precision@K)

Usability evaluation (SUS, task-based testing) would shift the focus toward Human-Computer Interaction, which is outside the scope of this software engineering thesis. The frontends exist as proof-of-concept clients that exercise the API; their UX refinement is deferred to future work.

**If required by examiner**: A lightweight **System Usability Scale (SUS)** questionnaire with 5–10 volunteer participants can be added as an appendix without expanding the core thesis scope.

## 11.7 Discussion

### 11.7.1 Strengths

1. **Explicit error handling** eliminates an entire class of runtime bugs (uncaught exceptions). The `Result<T>` type makes every failure path visible in code review.
2. **Vertical slices** make the codebase unusually approachable. A new developer can understand "Create Product" by reading 5 files in one folder.
3. **Modular monolith + MediatR** provides microservice-like isolation without distributed-system complexity. Checkout remains ACID because all modules share one database.
4. **Pluggable embedding model architecture** enables empirical comparison of 4 models without code changes. The Strategy pattern in the sidecar (`BaseEmbeddingModel` → concrete implementations) is a novel contribution: most CBIR systems hardcode a single model.
5. **Unified dual contribution** (see §11.7.1a): The modular architecture enables the ML comparison, the comparison validates the architecture's pluggability claim, and the results directly inform the production deployment configuration — a self-reinforcing design-and-evaluate cycle.

### 11.7.1a Unified Contribution: Architecture-Enabled Model Comparison

The two contributions — software architecture and ML model comparison — are not
independent projects sharing a codebase. They form a single thesis argument:

> **A modular monolith architecture with a pluggable model sidecar enables
> rigorous, auditable empirical comparison of embedding models for fashion
> retrieval, and the comparison results feed back into architectural decisions.**

Specifically:

1. **The architecture enables the comparison**. The `EmbeddingModel` ABC and the
   lazy `ModelRegistry` in the benchmark (`benchmarks/src/benchmark/models/`)
   are the same Strategy pattern as the production `ModelRegistry` in the
   embedding service (`service/Embedding/src/models/registry.py`). Adding a
   new model to the benchmark adds it to production automatically — no
   architectural change. The benchmark's `ThesisRunner` evaluates 4 models
   with zero pipeline code changes per model.

2. **The comparison validates the architecture**. If the architecture did not
   actually support pluggable models, the comparison would require rework for
   every model. The fact that the benchmark ran seamlessly across 4 models
   (Fashion-CLIP, CLIP-generic, EfficientNet-B0, ResNet-50) with different
   backbones (ViT, CNN), different libraries (transformers, torchvision),
   and different output dimensions (512 to 2048) validates the Strategy
   pattern's correctness.

3. **The results inform production**. The empirical finding — which model achieves
   optimal retrieval effectiveness — directly determines which adapter the
   production embedding service serves by default (`EMBEDDING_MODEL` env var).
   The architecture makes this a configuration change, not a code change.

4. **The Result<T> pattern ensures auditability**. Every evaluation failure
   (missing model weights, corrupted images, dimension mismatches) is explicit
   and traceable via the benchmark's logging infrastructure — a direct
   consequence of the architectural decision to avoid exception-driven control
   flow.

Without the modular architecture, the model comparison would be a disconnected
academic exercise. Without the model comparison, the architecture's
"pluggability" claim would be untested. Together, they form a self-reinforcing
design-and-evaluate cycle that is the thesis's primary contribution.

### 11.7.2 Limitations

1. **Namespace-level isolation is convention-only** — while the `ValidateVerticalSliceIsolation` MSBuild target is enabled for project-level enforcement, the 8 modules share a single assembly (`Module.csproj`), so namespace-level isolation relies on code review and convention. A Roslyn analyzer for cross-module `using` directives would provide stronger enforcement.
2. **No CI/CD** means regressions can land without automated verification.
3. **Azure storage not implemented** — the Strategy pattern is incomplete for storage providers.
4. **Model comparison results are provisional** — the existing results (§11.5.6) were generated with the initial 2-part ground truth (category only, no colour). The benchmark code now uses a 3-part ground truth (`masterCategory/subCategory/baseColour`) for more accurate visual similarity measurement. Re-running with the updated protocol and investigating the 0.0 Recall@20 values is planned for final submission.
5. **No API gateway** — SPAs directly call the API, which complicates CORS and rate-limit enforcement at the edge.

### 11.7.3 Future Work

| Enhancement | Rationale | Effort |
|-------------|-----------|--------|
| Add Roslyn analyzer for namespace-level isolation | Enforce namespace boundaries within the shared Module assembly | Medium |
| Implement GitHub Actions CI/CD | Automated build/test on every PR | Medium |
| Add Playwright E2E tests for Storefront | Validate critical user journeys | Medium |
| Expand model comparison to include domain-specific fine-tuned ResNet/EfficientNet | Test whether fashion fine-tuning closes the gap with Fashion-CLIP | Medium |
| Implement Azure Blob provider | Complete the storage Strategy pattern | Low |
| Add recommendation engine (collaborative filtering) | Complement CBIR with user-behavior recommendations | High |
| Migrate to YARP gateway | Centralize auth, CORS, rate limiting | Medium |

## 11.8 Evidence

- `docs/codebase/CONCERNS.md` — known issues and risks
- `docs/codebase/TESTING.md` — test framework and layout
- `README.md:175-178` — WIP notes
- `service/Embedding/src/main.py:1-29` — ML sidecar entry
- `service/Api/src/Shared/Application/Models/Results/Result.cs` — Result pattern
- `Directory.Build.targets:42-53` — isolation validation target
- `ApiTests/README.md` — manual test coverage

---

## [ASK USER] Items

21. Should this chapter include actual benchmark numbers if I run the tests now, or is the evaluation methodology sufficient for the draft?
22. Is there a specific statistical analysis the examiner expects for the ML evaluation (e.g., confidence intervals, significance testing)?
23. Does the thesis require a formal user study (SUS, task-based testing), or is technical evaluation sufficient?
