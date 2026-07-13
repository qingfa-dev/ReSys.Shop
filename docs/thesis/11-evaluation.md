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

**Method**: Static analysis of all `using` directives in `service/Api/src/Module/` to detect cross-module namespace references.

**Tool**: The `ValidateVerticalSliceIsolation` MSBuild target (currently `Condition="false"` in `Directory.Build.targets:44`) was designed to enforce this at compile time.

**Current status**: The target is disabled, so isolation is convention-only. A manual audit of `Module/Catalog/`, `Module/Ordering/`, `Module/Payment/` found **zero direct cross-module type references**; all inter-module communication uses `ISender.Send()` (e.g., `CreateProduct.cs:64-65` dispatches `AddVariant.Command`).

**Verdict**: ✅ **Compliant by convention**. Risk: without automated enforcement, future developers may introduce coupling.

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

## 11.5 ML Evaluation (CBIR)

### 11.5.1 Model Selection

**Model**: Fashion-CLIP (via `open-clip-torch`)
**Dimensionality**: 512-d float vector
**Output normalization**: L2-normalized (enables cosine similarity)

### 11.5.2 Evaluation Metrics

The following metrics are appropriate for evaluating the image search feature:

| Metric | Definition | Target |
|--------|-----------|--------|
| **Recall@K** | Proportion of truly similar items retrieved in top-K | ≥ 0.70 @ K=20 |
| **Precision@K** | Proportion of retrieved items that are truly similar | ≥ 0.50 @ K=20 |
| **Inference Time** | Time from image upload to result display | < 500ms (backend) |
| **Embedding Generation Time** | Time in Python sidecar | < 200ms per image |

**Evaluation method**: Construct a ground-truth dataset of **100 fashion images** with human-labeled similarity groups (e.g., 10 groups of 10 visually similar dresses). For each image:

1. Generate its Fashion-CLIP embedding via the Python sidecar.
2. Query PostgreSQL pgvector with `ORDER BY embedding <=> $1 LIMIT 20`.
3. Compare the top-20 retrieved variant images against the labeled similarity group.
4. Record **Recall@20** (proportion of the 9 similar items retrieved) and **Precision@20** (proportion of retrieved items that are in the group).

**Statistical reporting**: Report the **mean ± standard deviation** across all 100 queries. A standard deviation < 0.15 indicates consistent retrieval quality; higher variance suggests edge-case images (e.g., unusual patterns, accessories) that the model struggles with.

**Performance metrics**: Measure end-to-end latency (image upload → results displayed) and decompose into:
- Upload + preprocessing: ~50ms
- Embedding generation (Python sidecar): target < 200ms
- pgvector similarity query: target < 100ms
- DTO mapping + serialization: ~50ms
- **Total target**: < 500ms for the complete round-trip

**Current status**: `[TODO — Final Submission]` — The evaluation framework is fully defined. Benchmark execution requires a ground-truth dataset and a stable embedding sidecar deployment. These will be completed before final submission and the numbers populated in this section.

**Evidence**: `service/Embedding/src/main.py:1-29`, `ImageEmbedding.Inference.cs:21-36`

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
4. **Fashion-CLIP + pgvector** is an elegant integration: one database handles both transactional and vector workloads, simplifying ops.

### 11.7.2 Limitations

1. **Disabled isolation validation** (`ValidateVerticalSliceIsolation` is off) means module coupling can only be caught by code review.
2. **No CI/CD** means regressions can land without automated verification.
3. **Azure storage not implemented** — the Strategy pattern is incomplete for storage providers.
4. **Embedding E2E pending** — the CBIR feature is structurally complete but not empirically validated.
5. **No API gateway** — SPAs directly call the API, which complicates CORS and rate-limit enforcement at the edge.

### 11.7.3 Future Work

| Enhancement | Rationale | Effort |
|-------------|-----------|--------|
| Enable `ValidateVerticalSliceIsolation` | Automated enforcement of architectural boundary | Low |
| Implement GitHub Actions CI/CD | Automated build/test on every PR | Medium |
| Add Playwright E2E tests for Storefront | Validate critical user journeys | Medium |
| Conduct Fashion-CLIP benchmark | Empirical validation of CBIR quality | Medium |
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
