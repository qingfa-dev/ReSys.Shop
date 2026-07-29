# Editorial Decision Letter — Pass 2 (Design and Implementation)

**Thesis**: Building a Fashion E-Commerce Application with Image-Based Product Search and Model Benchmarking
**Student**: Nguyen Thanh Phat (B2005853), Can Tho University
**Review Panel**: EIC Dr. Elena Vasquez (SoftwareX), R1 Prof. Markus Lindgren (Chalmers), R2 Dr. Shreya Kapoor (Amazon), R3 Michael Torres (Shopify/pgvector), DA Prof. Arthur Kowalski (TU Dresden)
**Review Scope**: Pass 2 — Design and Implementation (Chapter 2, 3660 lines)

---

## Decision

**MAJOR REVISION** — The chapter is architecturally sound but incomplete as a defense-ready implementation report.

The chapter demonstrates strong systems thinking and technically competent instantiation of DDD, CQRS, VSA, and modern .NET patterns (commended by EIC, R2, R3). However, five reviewers collectively identify nine critical-level issues spanning factual technical errors (R2, R3), absence of implementation evidence (EIC, R1, DA), and structural document-identity problems (DA, R1).

---

## Consensus Findings (§3 reviewers agree)

### CF-1. pgvector / Embedding Dimensionality and Storage Integrity
**R2, R3, DA agree.** Two concrete technical errors:
- `vector(512)` column declared but 384/768/1280/2048-dim embeddings claimed — mathematically impossible
- ImageNet normalization incorrectly claimed as universal for CLIP/DINOv2 — wrong mean/std, embeddings would be invalid
- Embedding table location inconsistent between sections (variant_images vs product_image_embeddings)

### CF-2. Implementation Evidence Gap
**EIC, R1, DA agree.** ~70 lines of code vs ~770 lines of prose, ~40 screenshot placeholders. The chapter is a specification document masquerading as an implementation report. One VSA feature shown for a claimed 257 endpoints. Use case boilerplate consumes ~46% of the chapter.

---

## Disputed Issues

### DI-1. Architecture Scope — Overengineering vs. Production Realism
**DA (CRITICAL)**: 9 modules, 8 bounded contexts, full DDD+CQRS+VSA+C4 for a bachelor's CBIR thesis is overengineering.
**R3 (Strength)**: VSA pattern is production-realistic; concrete buildable core.
**EIC (Strength)**: VSA code examples and C4 model execution are strengths.

**Arbitration: PARTIALLY VALIDATED — framing issue, not architectural error.** The architecture is technically sound (3 reviewers praise it). The problem is that CBIR contribution is buried under infrastructure prose. **Remedy:** Foreground CBIR implementation; prune redundant architecture description.

### DI-2. DSR Methodology Requirement
**R1 (CRITICAL)**: No DSR design-evaluate cycles anywhere in the chapter.
**Others**: No other reviewer raises DSR as a gate.

**Arbitration: PARTIALLY VALIDATED with bachelor's calibration.** Strict DSR cycles are atypical for B.Eng. theses. However, key design decisions lack rationale. Add lightweight narrative framing of 3-5 decisions with justification.

---

## DA CRITICAL Adjudications

| ID | Finding | Corroboration | Adjudication |
|----|---------|--------------|-------------|
| **C-1** | Requirements traceability collapse — 13.6% FRs→UCs, ~91% to implementation | R1: FRs vanish after Section 2.2 | **VALIDATED** |
| **C-2** | Implementation evidence gap — spec doc, not implementation report | EIC: 30+ placeholders. R1: 46% UC boilerplate | **VALIDATED** |
| **C-3** | Overengineering for bachelor's level | R3 & EIC partially rebut (architecture is correct) | **PARTIALLY VALIDATED** — architecture sound, CBIR foregrounding needed |

---

## Revision Roadmap

### P0 — Must Fix Before Defense

| # | Item | Source | Severity |
|---|------|--------|----------|
| P0.1 | Fix vector column dimension contradiction — `vector(512)` vs variable dims | R2, R3, DA | CRITICAL |
| P0.2 | Correct ImageNet normalization claim — CLIP/DINOv2 use different mean/std | R2 | CRITICAL |
| P0.3 | Fix pgvector version: HNSW requires ≥0.5.0, not 0.3.2 | R3 | CRITICAL |
| P0.4 | Add `SET hnsw.ef_search = 100` or remove ef_search=100 claim | R3 | CRITICAL |
| P0.5 | Unify HNSW latency numbers — three inconsistent values reported | R2 | CRITICAL |
| P0.6 | Replace all 30+ screenshot placeholders with actual screenshots | EIC, DA | CRITICAL |
| P0.7 | Remove duplicated paragraph at lines 2920 & 2956 | EIC | CRITICAL |
| P0.8 | Fix terminology drift: "9 modules" vs "8 bounded contexts" | DA | CRITICAL |

### P1 — Strongly Recommended

| # | Item | Source | Severity |
|---|------|--------|----------|
| P1.1 | Restructure for implementation evidence — 70 lines code, 770 lines prose | DA, R1, EIC | MAJOR |
| P1.2 | Restore FR traceability through architecture/implementation sections | R1, DA | MAJOR |
| P1.3 | Add design-decision rationale for key architectural choices | R1 | MAJOR |
| P1.4 | Trim use case section — ~46% of chapter is boilerplate | R1, EIC | MAJOR |
| P1.5 | Foreground CBIR contribution with dedicated subsection | DA, R1 | MAJOR |
| P1.6 | Resolve embedding table naming inconsistency | R2, R3 | MAJOR |
| P1.7 | Add `CONCURRENTLY` to index DDL and index on `model_name` | R3 | MAJOR |
| P1.8 | Fix CBIR endpoint path inconsistency between C# and TypeScript | R3 | MAJOR |
| P1.9 | Add missing business domain scoping (tax, returns, discounts) | R3 | MAJOR |

### P2 — Nice to Have

| # | Item | Source |
|---|------|--------|
| P2.1 | Clarify model count: 6 registered vs 11 benchmarked | R2 |
| P2.2 | Add cold-start latency discussion | R2 |
| P2.3 | Add testing strategy paragraph | DA |
| P2.4 | Fix all typos: "realization→realizes", "a→an", "tracking tracking" | EIC, R1 |
| P2.5 | Standardize code example references (resolve unresolved identifiers) | R1 |
| P2.6 | Add VND currency context for international readers | EIC |
| P2.7 | Smooth section transitions | EIC |
| P2.8 | Reduce "synchronize" verb overuse | R1 |

---

## Writing Polish Summary

### Copy Errors (Fix Immediately)
- Line 2820: "realization" → "realizes" (EIC)
- Line 3071: "a unconstrained" → "an unconstrained" (EIC)
- Line 205: "tracking tracking numbers" → "tracking numbers" (R1)
- Lines 2920 & 2956: Remove duplicate paragraph (EIC)

### Structural Issues
- **Prose-to-evidence ratio**: ~91% prose, ~9% code. Target 60/40 for implementation chapter (DA, R1, EIC)
- **Use case verbosity**: ~46% of chapter is UC boilerplate. Keep 3-5 representative UCs; appendix the rest (R1, EIC)
- **Terminology drift**: "9 modules" vs "8 bounded contexts" (DA); overuse of "synchronize" (R1)
- **Screenshot hygiene**: 30-40+ placeholders must become real images (EIC, DA)

### Calibration
Writing quality is **above average for B.Eng.** — ubiquitous language glossary and C4 execution praised across reviewers. Primary weakness is **document genre confusion**: reads as pre-build spec, not post-build report. Solving P1.1/P1.4/P1.5 resolves the majority of criticisms.

---

*Synthesized from reports by all 5 reviewers. No new issues introduced beyond reviewer reports.*
