# Documentation Review: ReSys.Shop Benchmark & Thesis Design Documentation

**Review Date**: 2026-07-15
**Reviewer Panel**: Academic Paper Review v1.10.0 (5-reviewer multi-perspective)
**Subject**: 25+ Markdown documentation files spanning the ReSys.Shop benchmark project,
production embedding service, and MSc thesis design documentation.

---

## Phase 0: Field Analysis & Reviewer Configuration

### Field Identification

| Dimension | Assessment |
|-----------|------------|
| **Primary discipline** | Software Engineering (MSc thesis) with an ML evaluation component |
| **Secondary discipline** | Machine Learning / Computer Vision (fashion CBIR) |
| **Research paradigm** | Design Science Research (artifact + evaluation) |
| **Methodology type** | Mixed: architectural design evaluation + controlled ML experiment |
| **Target journal tier** | MSc thesis (examination committee) |
| **Paper maturity** | Draft — architecture chapters are structurally complete; ML evaluation chapter defines methodology but results are `[TODO]` |

### Reviewer Configuration

| Role | Assigned Identity | Perspective |
|------|-------------------|-------------|
| **Editor-in-Chief** | Examiner in Software Engineering, CTU (CZ) | Thesis fitness, contribution scope, dual-contribution justification |
| **Reviewer 1 (Methodology)** | ML Researcher specializing in CBIR evaluation methodology | Experimental protocol, statistical validity, metric selection, reproducibility |
| **Reviewer 2 (Domain)** | Professor of Software Architecture | Architectural patterns (modular monolith, vertical slices, CQRS), design rationale, code organization |
| **Reviewer 3 (Perspective)** | Cross-disciplinary: Human-Computer Interaction + Information Retrieval | Production usability, practical deployment gaps, end-to-end flow completeness |
| **Devil's Advocate** | Adversarial: generalist reviewer who challenges core claims | Logical fallacies, overclaimed contributions, ignored alternatives, cherry-picked evidence |

---

## Phase 1: Multi-Perspective Review Reports

---

### Report 1: Editor-in-Chief (Thesis Fitness, Contribution Scope)

**Overall assessment**: The documentation set is unusually thorough for an MSc thesis draft.
The dual-contribution framing (software architecture + ML model comparison) is a genuine
strength — it distinguishes this work from a standard CRUD-app thesis. However, there is a
**structural tension** between chapters 1–10 (architectural design) and chapter 11 (ML
evaluation) that is not fully acknowledged: the two contributions have different evaluation
standards, and the thesis-level synthesis is missing.

#### Strengths

1. **Clear dual-contribution structure**: Chapter 1 explicitly defines two research
   objectives and justifies them with evidence. The `docs/thesis/README.md` resolution log
   shows systematic handling of 25+ stakeholder questions — this demonstrates process rigor
   that examiners value.

2. **Evidence traceability**: Every claim in every document maps to a source file path and
   line number. The Requirements Traceability Matrix (Chapter 12) and the `docs/codebase/`
   operational docs provide a rare level of verifiability.

3. **Honest limitations documentation**: Chapter 11 §11.7.2 lists weaknesses (disabled
   isolation validation, no CI/CD, pending model comparison) without obfuscation. This is
   better than claiming completeness. The thesis protocol (docs/06-thesis-protocol.md)
   documents deviations from the original plan.

4. **Documentation quality**: The benchmark docs (01 through 08) are exceptionally clear.
   They explain technical concepts (embeddings, cosine similarity, mAP) with concrete
   examples rather than mathematical notation — making them accessible to non-specialist
   examiners.

#### Weaknesses

1. **Missing thesis-level synthesis §11.7**: The two contributions (architecture + ML) are
   evaluated separately in Chapter 11, but there is no section that synthesises them into a
   unified thesis claim. What is the *single* contribution of this thesis? The reader must
   infer it. Suggest adding a §11.7.4 "Unified Contribution" that explains how the
   architectural patterns enable the ML comparison and vice versa.

2. **Evaluation chapter is methodology-only**: Chapter 11 §11.5 defines the experimental
   protocol but contains `[TODO — Final Submission]` for all quantitative results. For a
   draft this is acceptable, but the gap between "architecture evaluation" (structural
   audit with concrete findings) and "ML evaluation" (empty template) is striking.
   Consider running at least the benchmark thesis protocol and populating placeholder
   results before distributing for review.

3. **[ASK USER] Unresolved thesis-level questions**: Chapter 1 §1.6 ends with `[ASK USER]`
   items that affect the entire thesis framing:
   - Is the thesis target MSc, PhD, or BSc? This changes the depth of literature review,
     statistical rigour expected, and novelty standard. The README log resolves this as
     "MSc" but the question should be removed from the chapter itself.
   - Similar stale `[ASK USER]` items remain in Chapter 11 §11.8 (questions 21–23).

4. **Literature review gap**: The thesis documentation does not contain a dedicated
   literature review chapter. References in docs/07-references.md list relevant papers,
   but there is no synthesis of prior art — no positioning against existing modular
   monolith frameworks (e.g., Moduliths in Spring), no comparison with other CBIR
   benchmarks (e.g., DeepFashion's own evaluation protocol), no critical analysis of why
   the chosen architectural patterns are novel or necessary.

### Report 1 — Verdict Condition

| Dimension | Score (0–100) |
|-----------|---------------|
| Thesis contribution clarity | 60 |
| Evidence quality | 85 |
| Scope management | 75 |
| Literature positioning | 30 |
| Overall | 65 |

---

### Report 2: Methodology Reviewer (ML Evaluation Protocol)

**Overall assessment**: The ML evaluation protocol is well-defined but has three
**non-trivial validity threats** that must be addressed before the numbers can be
trusted. The benchmark pipeline itself is architecturally clean and the metrics are
correctly implemented, but the experimental design has gaps that a methods-focused
examiner will flag.

#### Strengths

1. **Full metric suite**: mAP, Precision@K, Recall@K, nDCG@K — all standard CBIR metrics
   implemented correctly. The documentation explains each metric with intuition and
   worked examples (docs/03-metrics.md). This is model clarity.

2. **Stratified 3-fold CV**: The thesis protocol (docs/06-thesis-protocol.md) uses
   stratified k-fold cross-validation, which is methodologically stronger than a single
   train/test split. The minimum frequency filter for rare categories (<10 samples grouped
   into "Other") is a good practical decision that prevents undefined behaviour in
   stratified splitting.

3. **Statistical honesty**: The protocol explicitly notes that paired t-tests on n=3
   folds are underpowered and omits them — reporting only descriptive statistics
   (mean ± SD), Cohen's d, and bootstrap CI. This is more defensible than running
   an underpowered test and overclaiming significance.

4. **Reproducibility measures**: Pinned dependencies (`uv lock` → `uv.lock`), deterministic
   seed (`--seed 42`), documented hardware spec requirement, and a reproducibility
   checklist. These are best practices.

#### Weaknesses

1. **🔴 CRITICAL: Ground truth validity is inadequate**: The benchmark defines
   "similar" as same `masterCategory` + `subCategory`. This conflates visual similarity
   with taxonomic categorisation:
   - A black T-shirt and a blue T-shirt are both `Apparel/Topwear` → labelled "similar"
   - A black T-shirt and a black dress are `Apparel/Topwear` vs `Apparel/Dresses` →
     labelled "not similar" even though a user searching by image might consider them
     similar (same colour, same style)
   - The ground truth is **attribute-agnostic**: colour, pattern, fabric, and style do
     not affect relevance at all.
   
   **Impact**: The benchmark measures "category prediction" not "visual similarity."
   A model that ignores visual features and simply memorises category membership could
   score well. This must be explicitly acknowledged as a threat and discussed in the
   limitations.

   **Suggestion**: Add a secondary evaluation using a finer-grained ground truth
   (e.g., same `articleType` + same `baseColour`), even on a subset, to test whether
   the model rankings change.

2. **🔴 CRITICAL: 100-image dataset for the thesis evaluation (Ch 11 §11.5.2) is
   insufficient**: The thesis protocol originally describes 5,000 images (docs/06-thesis-
   protocol.md) but Chapter 11 §11.5.2 specifies 100 images (10 categories × 10 items).
   This discrepancy is unexplained. With only 100 queries:
   - mAP estimates will have wide confidence intervals (with 10 items per category, AP
     per query is computed over at most 9 relevant items)
   - The paired t-test (planned in §11.5.7) is underpowered even at n=100 if effect
     sizes are small
   - 10 categories × 10 items is **not representative** of e-commerce catalogues

   **Resolution needed**: Either (a) reconcile the thesis protocol with the benchmark
   implementation (use 5,000 images with 3-fold CV as in docs/06), or (b) justify why
   100 images is sufficient with a formal power analysis that accounts for the small
   per-category sample size.

3. **🔴 MAJOR: Protocol discrepancies between documentation sources**: Multiple documents
   specify different evaluation protocols:
   - `docs/06-thesis-protocol.md`: 5,000 images, 3-fold CV, 4 models
   - `docs/thesis/11-evaluation.md`: 100 images, 3 repeated runs, 4 models, pgvector
   - `docs/08-visual-similarity-pipeline.md`: 11 models in general benchmark, 4 in thesis
   - `docs/codebase/ARCHITECTURE.md`: 11 models

   These need to be reconciled into a single, consistent evaluation plan.

4. **Model load time measurement conflates download + init**: `ThesisRunner` measures
   `load_time_ms` as the total wall-clock time of `model.load()`, which includes:
   - Checking cache (negligible)
   - Downloading weights from HuggingFace (network-bound, highly variable)
   - Loading into memory
   
   Only the last two matter for production. Recommend separating first-load (download)
   from subsequent loads.

#### Report 2 — Verdict Condition

| Dimension | Score (0–100) |
|-----------|---------------|
| Experimental design | 50 |
| Metric correctness | 90 |
| Reproducibility | 85 |
| Ground truth validity | 35 |
| Overall | 60 |

---

### Report 3: Domain Reviewer (Software Architecture)

**Overall assessment**: The architectural documentation is the strongest part of this
project. The modular monolith + vertical slices + CQRS + `Result<T>` combination is
coherent and well-justified. The non-negotiable rules in AGENTS.md are enforceable in
principle. However, there are gaps between documented intent and code reality that
weaken the architectural claims.

#### Strengths

1. **Architectural rationale depth**: The modular monolith decision (Ch 3 §3.1.1) is
   supported by a pros/cons table, three paragraphs of justification citing Brooks (1986)
   and Newman (2019), and clear thesis constraints. This is the gold standard for
   thesis-level design documentation.

2. **Vertical slice enforcement**: 100% of feature actions follow the 5-file pattern
   (Handler, Endpoint, Request, Response, Validator). The `ValidateVerticalSliceIsolation`
   MSBuild target exists (even if disabled). The design intent is clearly communicated.

3. **Result pattern adoption**: Zero `throw` statements for control flow in domain code.
   The `Result<T>` type hierarchy (Result.NotFound, Result.Validation, Result.Conflict)
   follows Railway-Oriented Programming principles. This is a genuine software
   engineering contribution.

4. **Module isolation by convention**: The manual audit found zero cross-module type
   references — all communication via `ISender.Send()`. For a thesis demonstration, this
   is sufficient.

#### Weaknesses

1. **🔴 MAJOR: `ValidateVerticalSliceIsolation` is disabled**: The build target exists
   (`Directory.Build.targets:44`) but has `Condition="false"`. This means the primary
   architectural enforcement mechanism is not running. The evaluation (Ch 11 §11.2.1)
   correctly notes this as a risk, but it should be higher priority — an examiner may
   ask "why define a rule if you don't enforce it?"

   **Suggestion**: Enable the target, fix any violations, and document the enforcement
   result in Chapter 11. If the target is too strict, document which false positives
   caused it to be disabled and fix those specific rules.

2. **8 modules in one assembly breaks assembly-level isolation**: All business modules
   are in a single `Module.csproj`. The justification (build overhead of 32 projects)
   is pragmatic, but it means there is **no compiler-level module boundary**. Namespace
   conventions are not enforceable. Consider at minimum: (a) add a custom Roslyn analyzer
   that flags cross-module `using` directives, or (b) enable the existing MSBuild target.

3. **Documentation debt in codebase docs**: The `docs/codebase/` files (ARCHITECTURE.md,
   CONCERNS.md, etc.) were produced by an automated process and contain `[ASK USER]`
   items that appear to be unresolved. Specifically:
   - `benchmarks/docs/codebase/CONCERNS.md` has 6 `[ASK USER]` questions about
     architectural decisions (model integrity, GPU memory, coverage thresholds)
   - These should be resolved before thesis submission, not left as open questions

4. **C4 diagrams are Mermaid-only**: The 19 Mermaid `.mmd` diagrams are well-structured
   but GitHub-flavoured Markdown does not render Mermaid on all viewers. The thesis
   PDF will need rendered SVG/PNG versions. This is a practical concern for submission.

#### Report 3 — Verdict Condition

| Dimension | Score (0–100) |
|-----------|---------------|
| Architectural coherence | 85 |
| Enforcement vs intent gap | 45 |
| Design rationale | 90 |
| Thesis-fit of architecture | 80 |
| Overall | 75 |

---

### Report 4: Perspective Reviewer (Practical Impact, Cross-Disciplinary Connections)

**Overall assessment**: The project connects software engineering with ML — a bridge
that most theses in either field do not attempt. The benchmark-to-production pipeline
(`docs/08-visual-similarity-pipeline.md`) is the best documentation of this connection.
However, the practical deployment story is incomplete and several integration details
are glossed over.

#### Strengths

1. **Benchmark-to-production pipeline documented**: Section 6 of `docs/08-visual-
   similarity-pipeline.md` shows the complete feedback loop from model evaluation to
   production serving. This is rare in academic projects and valuable for thesis
   examiners who want to see real-world applicability.

2. **Pluggable model strategy in production**: The `service/Embedding/` sidecar uses
   a `ModelRegistry` pattern (annotated with `@ModelRegistry.register`) that mirrors
   the benchmark's `_LazyRegistry`. The code symmetry between the two systems is
   well-documented in the pipeline doc.

3. **Background embedding pipeline**: The Hangfire integration (async embedding on
   product upload) shows attention to production concerns beyond the core research.

#### Weaknesses

1. **🔴 MAJOR: No API gateway / edge layer**: The thesis explicitly defers YARP API
   gateway (Ch 1 §1.4.3). This means the Storefront SPA calls the API directly.
   In practice, this creates:
   - CORS configuration complexity (currently handled in Program.cs)
   - No edge-level rate limiting (rate limiting is in-process)
   - No TLS termination at gateway level
   - No request aggregation for the storefront
   
   For a thesis this may be acceptable, but the architectural discussion should at
   least note that production deployment would require a gateway.

2. **No CI/CD documented**: The thesis acknowledges this (Ch 11 §11.7.2), but for a
   project that makes architectural claims about modularity and testability, the
   absence of automated verification is a meaningful gap. Consider adding a minimal
   GitHub Actions workflow (even if only `dotnet build + test`) before final submission.

3. **Missing SPA testing**: Both Vue 3 SPAs (Admin + Storefront) have test frameworks
   configured (`pnpm run test:unit`) but there is no evidence of test content in the
   documentation. No E2E tests (Playwright/Cypress) are documented. For a thesis
   with 8 business modules, the frontend testing gap is notable.

4. **[ASK USER] No Dockerfiles for production**: Aspire manages Docker containers for
   local dev, but there are no Dockerfiles for production deployment. The thesis
   explicitly defers this ("out of scope"), but a deployment diagram without containers
   is theoretical. Consider adding at minimum the embedding service Dockerfile, since
   that runs separately from the .NET API anyway.

#### Report 4 — Verdict Condition

| Dimension | Score (0–100) |
|-----------|---------------|
| Practical deployment readiness | 30 |
| Cross-domain integration | 75 |
| Benchmark-to-production bridge | 80 |
| Testing completeness | 35 |
| Overall | 55 |

---

### Report 5: Devil's Advocate (Core Argument Challenges)

**Overall assessment**: The thesis makes several claims that, while defensible, have
plausible counter-arguments that the current documentation does not acknowledge. This
report identifies the strongest challenges to the core thesis claims.

#### Challenge 1: "Dual contribution" — are there really two contributions?

**Claim in thesis**: "The thesis makes a dual contribution: (a) software architecture
design and (b) ML model comparison for CBIR."

**Counter-argument**: These are two separate claims that do not form a unified thesis.
The architectural contribution (modular monolith + vertical slices + Result<T>) is a
software engineering design, while the model comparison (which model is best for fashion
retrieval) is a ML empirical study. They share the same system but not the same research
question. An examiner could argue:

- "If you moved the embedding models to a different architecture, would the architectural
  contribution change? No. Conversely, if you wrapped the models in a microservice instead
  of a sidecar, would the ML findings change? No."
- **Dual-contribution theses require a synthesising claim**: e.g., "The modular monolith
  architecture enables pluggable model evaluation, and the evaluation results inform which
  model the architecture should serve." The current documentation implies this but never
  states it explicitly.

**Severity: MAJOR** — Need a §11.7.4 (or §1.3) that explicitly articulates how the two
contributions depend on each other.

#### Challenge 2: Is the architectural contribution novel enough for an MSc?

**Claim in thesis**: The architectural combination (modular monolith + vertical slices +
CQRS + Result<T>) is a contribution worthy of a thesis.

**Counter-argument**: Each of these patterns is well-established individually:
- Modular monoliths: described by Vernon (2013) in *Implementing Domain-Driven Design*
- Vertical slices: popularised by Jimmy Bogard (2015+)
- CQRS + MediatR: standard .NET pattern since ~2016
- `Result<T>`: common in F# (Railway-Oriented Programming, ~2014)

The thesis does not claim novelty in any individual pattern. The contribution is the
*combination and enforcement* — specifically, the non-negotiable rules (AGENTS.md) and
the `ValidateVerticalSliceIsolation` target. But since the target is **disabled**, the
enforcement contribution is aspirational rather than actual.

**Severity: MAJOR** — Either (a) enable the enforcement mechanism and document its
effectiveness, or (b) reframe the architectural contribution away from "enforcement
methodology" toward "demonstration of feasibility" (less novel but honest).

#### Challenge 3: Category-based relevance measures "category classification" not "visual similarity"

**Claim in thesis**: The benchmark measures visual similarity retrieval effectiveness.

**Counter-argument**: By defining relevance as "same `masterCategory` +
`subCategory`", the benchmark actually measures a model's ability to predict
taxonomic categories — a fundamentally different task from visual similarity.
Consider:

- A red dress and a blue dress (same category) → relevant (correct by definition)
- A red dress and a red shirt (different categories, same colour) → not relevant
- A green dress and a green hat (different categories, same colour) → not relevant

A model could achieve high mAP by learning only category boundaries and completely
ignoring visual features like colour, pattern, fabric, or style. The benchmark does
not distinguish between "found because it looks similar" and "found because it's in
the same category."

**Severity: CRITICAL** — This threatens the validity of the entire ML evaluation as
a measure of visual similarity. Must be addressed:
- Explicitly rename the evaluation to "category-based retrieval" or
- Add a secondary evaluation with visually-defined ground truth (even on a subset), or
- Provide bounding analysis: estimate how much of the mAP is attributable to category
  prediction vs. visual similarity

#### Challenge 4: 3-fold CV is standard but n=3 folds is low for statistics

**Claim in thesis**: 3-fold cross-validation provides sufficient statistical rigour.

**Counter-argument**: The thesis correctly notes that paired t-tests on n=3 are
underpowered. But the implication is that the study cannot detect true differences
between models — so how do we know Fashion-CLIP is "better"? The thesis relies on
Cohen's d effect sizes, but with n=3, the confidence interval around Cohen's d is
extremely wide. Bootstrap CI on n=3 is also questionable (only 10 possible bootstrap
samples from 3 values — though with replacement the number is higher, the information
content is fundamentally limited).

**Severity: MAJOR** — The thesis should either:
- Increase folds (5-fold CV on 5,000 images gives n=5, feasible with GPU)
- Or explicitly state that the ML evaluation is *descriptive* only (no inferential
  claims), which means the thesis cannot "conclude" that Fashion-CLIP is better —
  only that "in our sample, Fashion-CLIP scored higher"

#### Challenge 5: The disabled isolation target is a crack in the foundation

**Claim in thesis**: The system enforces module isolation with a compile-time check.

**Counter-argument**: `ValidateVerticalSliceIsolation` is disabled (`Condition="false"`).
The manual audit found zero violations, but this is a point-in-time observation —
there is no automated regression detection. The thesis itself acknowledges this as a
risk (Ch 11 §11.2.1), but then evaluates the architecture as "compliant" — which is
only true for the current snapshot, not as a property of the design.

In a thesis about architecture, the enforcement mechanism should be part of the
evaluated contribution, not a footnote about future work.

**Severity: MAJOR** — Must either enable it or remove the enforcement claim from the
contribution statement.

### Report 5 — Verdict Condition

| Dimension | Score (0–100) |
|-----------|---------------|
| Argument logic | 40 (weaknesses identified) |
| Alternative explanations provided | N/A |
| Strength of counter-arguments | 80 (strong challenges) |
| Overall assessment | The thesis claims are plausible but need significant reframing |

**Devil's Advocate Bottom Line**: The thesis needs to resolve four tension points
before submission: (1) unify the dual contributions with a synthesising claim,
(2) enable the enforcement mechanism or drop the enforcement claim, (3) fix the
ground truth validity threat, and (4) reconcile the multiple evaluation protocols.

---

## Phase 2: Editorial Synthesis & Decision

### Consensus Map

| Issue | EIC | Method | Domain | Perspective | DA | Consensus |
|-------|-----|--------|--------|-------------|-----|-----------|
| Documentation quality | ✅ strong | ✅ strong | ✅ strong | ✅ strong | — | Strong |
| Ground truth validity | ⚠️ noted | 🔴 critical | — | — | 🔴 critical | Critical flaw |
| Evaluation protocol inconsistency | ⚠️ noted | 🔴 critical | — | — | ⚠️ noted | Major issue |
| Architecture rationale | ✅ strong | — | ✅ strong | ⚠️ noted | — | Strong |
| Disabled isolation enforcement | — | — | 🔴 major | — | 🔴 major | Major gap |
| Dual-contribution synthesis | ⚠️ noted | — | — | — | 🔴 major | Major gap |
| Model comparison pending | ⚠️ noted | 🔴 critical | — | — | ⚠️ noted | Major concern |
| Missing Dockerfiles / CI/CD | — | — | — | 🔴 major | — | Medium concern |

### Editorial Decision

**Decision: MAJOR REVISION**

The documentation is structurally excellent — the best-architected thesis documentation
set I have reviewed at this stage. However, three issues cannot be resolved by
documentation edits alone: the ground truth validity threat, the protocol inconsistency,
and the pending evaluation results. These require code or experimental-design changes.

**Conditions for acceptance (in priority order)**:

1. **Resolve the ground truth validity** — either (a) rename the evaluation to
   "category-based retrieval" and adjust all claims, or (b) add a secondary visual-
   similarity ground truth on a subset. The current framing as "visual similarity" is
   misleading.

2. **Reconcile the evaluation protocols** — the thesis protocol (Chapter 11), the
   benchmark protocol (docs/06), and the pipeline doc (docs/08) must specify the same
   dataset, same model set, and same CV strategy. Currently they disagree on all three.

3. **Enable `ValidateVerticalSliceIsolation`** or drop the enforcement claim from the
   architectural contribution. The current state (disabled target, manual audit) does
   not support the "compile-time enforcement" claim.

4. **Articulate the synthesising claim** — add a section explaining how the
   architectural contribution and the ML contribution form a unified thesis, not
   two separate projects sharing a codebase.

5. **Run the thesis benchmark** — populate the `[TODO]` results in Chapter 11. Even
   provisional results on the small dataset are better than empty cells.

6. **Add at least a Dockerfile for the embedding service** — this is the component
   most likely to be reused and is already a standalone process.

### Revision Roadmap

| # | Task | Priority | Effort | Owning reviewer concern |
|---|------|----------|--------|------------------------|
| R1 | Rename evaluation or add visual-similarity ground truth | Critical | Medium-High | Method + DA (#2, #5) |
| R2 | Reconcile protocol: pick one dataset/CV/metric set across all docs | Critical | Low | Method (#2) |
| R3 | Enable `ValidateVerticalSliceIsolation`, fix violations, document | High | Medium | Domain + DA (#3, #5) |
| R4 | Add §11.7.4 "Unified Contribution" synthesising architecture + ML | High | Low | EIC + DA (#1, #5) |
| R5 | Run thesis benchmark, populate Chapter 11 results | High | Medium | All |
| R6 | Add Dockerfile for `service/Embedding/` | Medium | Low | Perspective (#4) |
| R7 | Add GitHub Actions CI (minimal: build + unit test) | Medium | Medium | Perspective (#4) |
| R8 | Resolve `[ASK USER]` items in `docs/codebase/CONCERNS.md` | Medium | Low | Domain (#3) |
| R9 | Render Mermaid diagrams to SVG for thesis PDF | Low | Low | Domain (#3) |
| R10 | Set up minimal frontend tests (one Vitest suite per SPA) | Low | Medium | Perspective (#4) |

---

## Summary

| Dimension | Grade | Key message |
|-----------|-------|-------------|
| Documentation organisation | A- | Excellent structure, evidence traceability, clarity |
| Architecture design rationale | A | Well-justified, cited, and contextualised |
| ML evaluation protocol | C+ | Correct metrics but flawed ground truth and inconsistent protocol |
| Code-practice alignment | B- | Architecture intent is clear; enforcement is aspirational |
| Production readiness | D | No deployment artifacts documented |
| Thesis contribution framing | B- | Dual contributions are identified but not synthesised |

**Bottom line**: This is a strong MSc thesis draft that needs 1–2 weeks of focused
revision on three fronts: (1) fixing the ground truth and protocol issues in the ML
evaluation, (2) enabling the architectural enforcement mechanism, and (3) writing the
synthesising chapter section that ties the two contributions together. The documentation
infrastructure (traceability, rationale, clarity) is already at submission quality.
