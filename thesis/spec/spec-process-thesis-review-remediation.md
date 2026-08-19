---
title: Thesis Review Remediation Process
version: 1.1
date_created: 2026-08-19
last_updated: 2026-08-19
owner: Thesis author
tags: [process, thesis, review, remediation, typst, academic-writing, undergraduate]
---

# Introduction

This specification defines a structured, verifiable process for remediating every
finding recorded in the peer review of the CTU **undergraduate** (bachelor) thesis
"Building a Fashion E-Commerce Application with Recommendation and Image-Based Product
Search." The review is recorded in two forms under `thesis/reviews/`: the master
consolidated fix list (`thesis-review-MASTER-FIXLIST.md`, also duplicated at
`thesis/REVIEW.md`) and nine per-chapter detail files (`thesis-review-part1-*.md` …
`thesis-review-appendices-bcd.md`). A companion set of nine **rewrite files**
(`thesis-rewrite-*.md`) provides ready-to-paste replacement text and per-change
rationale keyed to the master fix list. The review surfaced 22 findings across all
parts, chapters, references, and appendices, tiered by severity (TIER 1 must-fix,
TIER 2 should-fix, TIER 3 optional polish).

The remediation process ensures each finding is verified against an authoritative
source, corrected once at its root cause, and propagated consistently to every
location that echoes it, before the thesis is recompiled and submitted to the CTU
committee.

Two non-negotiable principles govern this process:

1. **Verify-before-fix.** The review and the rewrite files are secondary artifacts.
   They were authored from the compiled PDF only, without access to the codebase or
   the benchmark output JSON. At least one of their recommendations (the "eleven
   models → six" finding, TIER 1 #2) is contradicted by the actual model registry,
   and the benchmark-reconciliation question (TIER 1 #1), which the review left
   open as "either table could be correct," is now resolved by the benchmark JSON:
   **Appendix A is authoritative and Table 67/68 are stale.** Applying review or
   rewrite recommendations without checking the authoritative source would introduce
   new errors. Every fix must cite the source it was validated against.

2. **Undergraduate-appropriate scope.** This is a bachelor thesis, not a journal
   paper. The remediation is a documentation-consistency exercise (fixing numbers,
   citations, counts, and cross-references that drifted during drafting). It is not
   a research redo, except for the one case where a stale table must be regenerated
   from an existing benchmark run. No new experiments, no new content beyond what is
   needed to correct a finding, and no methodology changes.

## 1. Purpose & Scope

**Purpose.** Establish a single, machine-followable workflow that takes the thesis
from its current reviewed state to a submission-ready state in which (a) no review
finding remains open, (b) every corrected number, citation, and count is consistent
across all locations that reference it, and (c) the remediation itself is auditable.

**Scope.**
- In scope: every 🔴 CORRECT and 🟠 REWRITE finding in the master fix list (`thesis/reviews/thesis-review-MASTER-FIXLIST.md`, identical to `thesis/REVIEW.md`) — TIER 1 (#1–#12) and TIER 2 (#13–#22), the TIER 3 polish items, the two confirmed fabricated bibliography entries, and the reconciliation of Chapter 3's headline tables against Appendix A (now resolved: Appendix A is authoritative).
- In scope (resources): the nine `thesis-rewrite-*.md` files may be used as ready-to-paste source text, subject to the verify-before-fix rule (see CON-003).
- Out of scope: writing new thesis content beyond what a finding requires, changing the research methodology, altering the benchmark code, and formal plagiarism screening (no institutional tool available; the author must run Turnitin separately before CTU submission).

**Intended audience.** The thesis author and any agent or collaborator applying the
remediation. The audience is assumed to be able to run `typst compile main.typ`, read
the benchmark JSON outputs, edit Typst (`.typ`) source, and edit BibTeX
(`backmatter/bibliography.bib`).

**Assumptions.**
- The benchmark code at `benchmarks/` is the source of truth for model counts and
  benchmark numbers, not the thesis text.
- The actual published papers are the source of truth for citation metadata, not the
  bibliography as currently written.
- The thesis is built with Typst 0.15.1 via `typst compile main.typ` from the
  `thesis/` directory (per `thesis/AGENTS.md`).

## 2. Definitions

| Term | Definition |
|------|------------|
| Finding | A single issue recorded in the master fix list, identified by a TIER number (TIER 1 #1–#12, TIER 2 #13–#22) or a per-file finding number. |
| Authoritative source | The artifact whose value is ground truth for a given claim: the benchmark code/JSON, the EF Core migrations, the Carter route definitions, or the real published paper. |
| Root cause | The single underlying error from which multiple location-level errors derive (e.g., one wrong benchmark run propagated to six chapters). |
| Propagation set | The complete list of file/section locations that must be updated when a root cause is fixed. |
| Verify-before-fix | The rule that no review or rewrite recommendation is applied until it has been checked against its authoritative source. |
| Review file | One of the nine `thesis-review-*.md` detail files plus the master fix list under `thesis/reviews/`. |
| Rewrite file | One of the nine `thesis-rewrite-*.md` files providing ready-to-paste replacement text keyed to the master fix list. Authored from the PDF only, without codebase access (see CON-003). |
| RQ | Research Question (RQ1–RQ3), answered in `chapters/part2/ch3-evaluation/`. |
| FR | Functional Requirement (e.g., `CAT-FR-01`), defined in `chapters/part2/ch2-design/`. |
| CBIR | Content-Based Image Retrieval. |
| mAP | mean Average Precision, the primary retrieval-accuracy metric. |
| P@K / R@K | Precision@K / Recall@K, secondary retrieval metrics. |
| BibTeX key | The Typst `@key` citation identifier in `backmatter/bibliography.bib` (e.g., `chia2022fashionclip`). The review's bracket numbers ([6], [26], [27]) map to these keys. |
| TIER 1 / 2 / 3 | Severity tiers from the master fix list: must-fix / should-fix / optional polish. |
| CTU | Can Tho University, the institution awarding the bachelor thesis. |

## 3. Requirements, Constraints & Guidelines

### Process requirements

- **REQ-001**: Every 🔴 and 🟠 finding in the master fix list shall be resolved before submission. TIER 3 items are recommended but not blocking.
- **REQ-002**: Before editing any thesis text for a finding, the remediator shall record the authoritative source that validates the correction (file path + line, JSON key, or published-paper metadata). No fix is applied on the review's say-so alone.
- **REQ-003**: Each root-cause fix shall be applied once, then propagated to every location in its propagation set in a single pass, to prevent partial fixes.
- **REQ-004**: After each fix, `typst compile main.typ` shall be run from `thesis/` and shall succeed (the compile fails on missing image refs and on unresolved `@key` citations, so a green compile is a structural sanity check).
- **REQ-005**: A remediation log shall be maintained (see §4) recording, per finding: status, authoritative source, files changed, and verification command output.
- **REQ-006**: The remediation shall be sequenced so that the benchmark-reconciliation fix (TIER 1 #1) is completed before any downstream number propagation, because the abstract, RQ answers, deployment recommendation, and Part 3 summary all derive from it.

### Verify-before-fix constraints (critical)

- **CON-001**: The "eleven models" finding (TIER 1 #2) shall NOT be applied as written in the review or the rewrite files ("change eleven to six"). Verified facts:
  1. The actual model registry at `benchmarks/src/benchmark/models/__init__.py:44-56` registers exactly **eleven** models (`efficientnet-b0`, `convnext-tiny`, `dinov2-vits14`, `fashion-clip`, `clip-b32`, `clip-generic`, `clip-l14`, `clip-vit-b16`, `siglip`, `resnet-50`, `eva-clip`).
  2. The thesis's own Table 55 (§2.4.4.1) lists only **six** models — Table 55 is stale, not the prose.
  3. The thesis benchmark run (`benchmarks/outputs/thesis/results/thesis_results_category_only.json`) evaluated exactly **four** models (FashionCLIP, ResNet-50, EfficientNet-B0, CLIP-generic), matching the "four representative models" claim.
  4. The rewrite files recommend "six" because the reviewer saw Table 55 but not the codebase; applying that recommendation would corrupt correct prose.
  Fix: keep the "eleven supported by the framework" wording everywhere; update Table 55 to list all eleven registered models (grouped by architecture family: CNN, ViT, CLIP-family). The "four representative, selected from eleven" framing is accurate.
- **CON-002**: No finding shall be resolved by changing a number to match another number stated only in the review or rewrite files. Both candidate numbers must be checked against the benchmark JSON output that authored them.
- **CON-003**: The `thesis-rewrite-*.md` files were authored from the compiled PDF without codebase or JSON access. They are a useful starting point for ready-to-paste prose, but every figure, count, and citation in them is subject to CON-001/CON-002. Specifically, the rewrite files' "six" model count (Part 1, Chapter 1, Chapter 2, Part 3 rewrites) is wrong per CON-001, and any rewrite that assumes Table 67 is authoritative (rather than Appendix A) is wrong per REQ-101.
- **CON-004**: Citation corrections shall use the corrected metadata in the master fix list TIER 1 #4 (which is verified against the real papers), not the review's bracket numbers. Edits target BibTeX keys in `backmatter/bibliography.bib`, not bracket numbers.

### Content requirements (per finding)

TIER 1 — Must fix:

- **REQ-101** (RESOLVED — Appendix A is authoritative): Chapter 3 Table 67 (accuracy) and Table 68 (efficiency) are **stale**; Appendix A Tables 71–75 are authoritative. This is confirmed by the benchmark JSON at `benchmarks/outputs/thesis/results/thesis_results_category_only.json` (FashionCLIP mAP mean = **0.9309**, matching Appendix A.1, NOT Table 67's 0.8788) and `benchmarks/outputs/thesis/results/thesis_results.json` (ResNet-50 load_time mean = **374.09 ms**, matching Appendix A.4, NOT Table 68's 286.1 ms). Action: regenerate Table 67, Table 68, and Figures 42–45 from the Appendix A numbers (or directly from the JSON); then recalculate every derived percentage in §3.5–3.7 from the new mAP values, then update the abstract (`frontmatter/abstract.typ`), the RQ1/RQ2/RQ3 answers, and Part 3's Summary of Work to match. Indicative recomputed values (recompute precisely from the JSON, do not copy these rounded figures blindly):
  - Fashion-CLIP vs CLIP-generic mAP: (0.9309 − 0.9115) / 0.9115 = **2.13%** (was stated as 5.4%)
  - Fashion-CLIP vs EfficientNet-B0 mAP: (0.9309 − 0.8895) / 0.8895 = **4.65%** (was stated as 7.7%)
  - Fashion-CLIP vs ResNet-50 mAP: (0.9309 − 0.8857) / 0.8857 = **5.10%** (was stated as 8.2%)
  - EfficientNet-B0 as % of Fashion-CLIP mAP: 0.8895 / 0.9309 = **95.56%** (was stated as 92.8%)
  The deployment recommendation in §3.7.2 must be re-checked: the accuracy-efficiency ranking may change (EfficientNet-B0 vs ResNet-50 gap narrows: 0.8895 vs 0.8857, ~0.43% — ResNet-50 is now nearly tied with EfficientNet-B0 on accuracy). Storage column (8.1 / 13.0 / 3.3 / 3.3 MB) is the only value unchanged. The §3.4 methodology text already matches the category-only scheme, so no methodology rewrite is needed.
- **REQ-102**: Update thesis Table 55 (`chapters/part2/ch2-design/`, §2.4.4.1) to list all eleven registered models, per CON-001. Do not change "eleven" to "six" anywhere.
- **REQ-103**: Standardize the Fashion-CLIP vs CLIP-generic improvement figure to a single value across the thesis. Per REQ-101, the authoritative value is recomputed from Appendix A.1: **2.13%** = (0.9309 − 0.9115) / 0.9115 (was previously stated as 5.4%, which was correct only against the stale Table 67). Fix the three "15 to 20%" occurrences in Chapter 1 (§1.3.3.5, §1.3.4.4, §1.6.1) and the one "6.1%" in §3.7.4, and update the four "5.4%" occurrences in §3.5–3.7 to the recomputed value. Remove all "confirmed in Chapter 3" language that attached to the 15–20% figure. The recomputation must be done from the JSON, not from this spec's rounded indicative value.
- **REQ-104**: Replace the two fabricated bibliography entries with the corrected metadata from the master fix list TIER 1 #4: `chia2022fashionclip` (real title "Contrastive language and vision learning of general fashion concepts", venue *Scientific Reports* vol. 12, article 18958, 2022, DOI 10.1038/s41598-022-23052-9, full eight-author list) and `wu2019fashioniq` (author Al-Halah not Al-Zahir, title "...Retrieving Images by Natural Language Feedback", venue CVPR 2021 pp. 11307–11317, full seven-author list).
- **REQ-105**: Change "nine bounded contexts" to "eight bounded contexts" in §2.3.1.
- **REQ-106**: Correct "88 functional requirements across nine business modules" to "87 functional requirements across eight business modules" in §2.1 opening, and drop "Dashboard" from that module list (Dashboard is real per Table 50 but has no FR table; optionally add its FR table instead).
- **REQ-107**: Replace the "near-zero P@20" claim in Part 3 §III Limitations with the accurate description: P@20 drops from ~0.90 (category-only) to ~0.30 (category+colour+pattern), per Appendix A.2/A.3.
- **REQ-108**: Reconcile the CBIR search endpoint URL across §2.4.4.3, §2.4.5.1, and the stated convention §2.3.5.2 against the actual Carter route definitions in `service/Api/src/Module/Features/`. Make prose and convention agree with the implemented routes.
- **REQ-109**: Resolve the "Variable Vector Dimensions" contradiction. Check the EF Core migration and `IEntityTypeConfiguration<ImageEmbedding>` in `service/Api/src/Module/`. Either remove/qualify the §2.3.4.4 bullet, or correct §2.3.4.3, §2.4.3.2, Appendix D Table 82, and Appendix D.9 to show however per-model dimensions are actually handled. The fixed `vector(512)` type cannot store other dimensionalities.
- **REQ-110**: Fix both "Section 2.1.5" references (§2.3.4.3 and §2.4.3.2) to point to the real section (likely §1.4.3–1.4.4 HNSW/IVFFlat comparison or §3.4.3 benchmark protocol).
- **REQ-111**: Resolve the visual-search UI state-count contradiction in §2.4.5.2.1: either add a fifth (Error) state to Table 58, or change "five" to "four" to match the table.
- **REQ-112**: Rewrite Part 1 §VI Thesis Outline to match the real structure: Part 1 (Introduction) → Part 2 Chapters 1–3 (Background; Design and Implementation; Testing and Evaluation) → Part 3 (Conclusion and Future Work). Remove the non-existent "Chapter 4" and the duplicate "Chapter 1" numbering.

TIER 2 — Should fix:

- **REQ-201**: Reconcile PostgreSQL version: Table 66 (§3.4.4) says 16; seven other locations say 17 and the container tag `pgvector/pgvector:pg17-trixie` implies 17. Correct Table 66 to 17, or document why the benchmark used 16.
- **REQ-202**: Reconcile pgvector version: Table 51 (§2.4.1) says 0.3.2; Chapter 3 says 0.7.0. Confirm against `Directory.Packages.props` or `pg_extension` output and correct Table 51.
- **REQ-203**: Standardize the permission-string format on one template matching the actual code (likely `domain.resource.action`, dot-separated, three parts). Fix §2.3.6.2 and any colon-separated `domain:category:action` mentions.
- **REQ-204**: Change "eight use cases" to "nine use cases" in §2.4.5.2 opening (nine storefront use cases are actually documented in §2.4.5.2.1–2.4.5.2.8).
- **REQ-205**: Standardize the "accuracy metrics" count. Recommended convention: "three accuracy metric families (mAP, P@K, R@K), evaluated at three depths (K=5,10,20), for seven reported columns." Fix §3.4.2's "five" and align Part 3's "seven" to this wording.
- **REQ-206**: Resolve the Citation [2] (Pinterest) mismatch with the "30% search abandonment" stat in Part 1 §I and Chapter 1 §1.1: either source the 30% figure separately, or soften the claim to what `pinterest2023visual` actually supports (search volume).
- **REQ-207**: Audit all eleven rows of Table 70 (Requirements Traceability) for section-citation accuracy. Confirmed mis-citations: "Validate pgvector feasibility" and "Set up vector search" cite §2.2.4 (should be §2.3.4/§2.4.4); RQ3 answer cited as §3.5 (actually §3.7.4).
- **REQ-208**: Correct the EfficientNet-B0 trade-off in §1.3.4.5. The stale figure was "3.4 percent lower mAP@10"; Chapter 3 (against stale Table 67) said 7.7%; the authoritative Appendix A.1 value is (0.9309 − 0.8895) / 0.8895 = **4.65% lower mAP**. Use the recomputed value from the JSON (per REQ-101), consistent everywhere.
- **REQ-209**: Add the missing co-author "Shi Qiu" to the `liu2016deepfashion` (DeepFashion) bibliography entry.
- **REQ-210**: Resolve the "sequential selection preserves the natural category distribution" logic gap in Appendix B.1. Either confirm and state that the source Kaggle dataset is pre-shuffled (making sequential selection valid), or describe the actual sampling method (e.g., stratified random sampling). Verify against the dataset preparation code in `benchmarks/` before rewriting.

TIER 3 — Optional polish (record in log if actioned or intentionally skipped):

- **REQ-301**: Fix the §2.4.5 cross-reference "Section 2.2.2" → "Section 2.2" (or "Sections 2.2.3–2.2.5").
- **REQ-302**: Add a clarifying sentence in §2.2.1 about the "Support" actor convention used in individual use-case tables.
- **REQ-303**: Optionally trim redundancy between Part 1 §III Objectives and §VI Thesis Outline.
- **REQ-304**: Optionally mention one real bug-and-fix encountered during development in §3.3 to offset the 100% first-pass pass-rate appearance.
- **REQ-305**: Fix the phantom "Chapter 6" references (Appendix A.2 Table 72 caption, Appendix B.3) to "Chapter 3".
- **REQ-306**: Optionally add a Dashboard functional-requirements table (alternative to dropping Dashboard in REQ-106).

### Guidelines

- **GUD-001**: Work root-cause-first. Six of the TIER 1 findings (the model count, the improvement percentage, the PostgreSQL/pgvector versions, the bounded-context count, the FR/module count) are each one root error echoed in many places; fix the root, then sweep the propagation set in one pass.
- **GUD-002**: When a number appears in both Chapter 3 and Appendix A and they disagree, the benchmark JSON is the tie-breaker — confirmed: Appendix A matches the JSON, Table 67/68 are stale and must be regenerated.
- **GUD-003**: Keep `frontmatter/abstract.typ` benchmark numbers in sync with `chapters/part2/ch3-evaluation/` (per `thesis/AGENTS.md` gotcha). The abstract hardcodes the stale mAP 0.8788 etc.; per REQ-101 these must change to the authoritative Appendix A values (0.9309 for Fashion-CLIP) as part of the benchmark reconciliation.
- **GUD-004**: Do not set heading numbering in chapter files; it is controlled per-part in `main.typ` (per `thesis/AGENTS.md`).
- **GUD-005**: Do not delete the `chapters/part2/ch2-design/04-implementation/` (singular) directory until `04-implementation` has been grepped across `chapters/` and `figures/` (per `thesis/AGENTS.md` gotcha); its diagram files are referenced by live content.
- **PAT-001**: Follow the existing chapter pattern: edit the `fN/NN-topic.typ` numbered files, never the aggregator `part{1,2,3}-*.typ` files, except for structural fixes like REQ-112.

## 4. Interfaces & Data Contracts

### Remediation log

A CSV/Markdown log shall be created at `thesis/spec/remediation-log.md` with one row per finding. Schema:

| Column | Type | Description |
|--------|------|-------------|
| finding_id | string | TIER + number, e.g., `T1-1`, `T1-2`, `T2-13` |
| title | string | Short finding title |
| status | enum | `unverified` \| `verified` \| `in_progress` \| `fixed` \| `propagated` \| `skipped` |
| authoritative_source | string | File path + line, JSON key, or paper DOI that validated the fix |
| files_changed | list | Typst/BibTeX paths edited |
| compile_ok | bool | `typst compile main.typ` succeeded after fix |
| notes | string | Deviations from the review recommendation (e.g., CON-001) |

Example row:

```
finding_id: T1-2
title: "Eleven models" claim
status: fixed
authoritative_source: benchmarks/src/benchmark/models/__init__.py:44-56 (11 entries); thesis_results_category_only.json (4 evaluated)
files_changed: chapters/part2/ch2-design/.../fN/NN-model-management.typ (Table 55)
compile_ok: true
notes: Review AND rewrite files recommended changing "eleven"→"six"; REJECTED per CON-001/CON-003. Registry has 11; Table 55 was stale (listed 6). Fixed Table 55 to list all 11; "eleven" wording retained everywhere. Rewrite files' "six" NOT pasted.
```

### Authoritative-source map

| Finding class | Authoritative source artifact | Location | Confirmed value(s) |
|---------------|-------------------------------|----------|--------------------|
| Model count (registry) | Model registry `_register()` | `benchmarks/src/benchmark/models/__init__.py:44-56` | 11 models |
| Model count (benchmark) | Thesis benchmark run | `benchmarks/outputs/thesis/results/thesis_results_category_only.json` | 4 models evaluated |
| Benchmark accuracy (category-only, Appendix A.1) | `thesis_results_category_only.json` | `benchmarks/outputs/thesis/results/` | FashionCLIP mAP 0.9309; ResNet-50 0.8857; EfficientNet-B0 0.8895; CLIP-generic 0.9115 |
| Benchmark accuracy (pattern, Appendix A.3) | `thesis_results_pattern.json` | `benchmarks/outputs/thesis/results/` | FashionCLIP 0.2146; ResNet-50 0.1859; EfficientNet-B0 0.1923; CLIP-generic 0.2007 |
| Benchmark efficiency (Appendix A.4) | `thesis_results.json` | `benchmarks/outputs/thesis/results/` | ResNet-50 load 374.09 ms; EfficientNet-B0 load 110.24 ms (matches Appendix A.4, NOT Table 68) |
| Stale tables (to be regenerated) | Table 67, Table 68 | `chapters/part2/ch3-evaluation/` | Do NOT match JSON; regenerate from Appendix A / JSON |
| PostgreSQL / pgvector version | Container tag + package versions | `Directory.Packages.props`; tag `pgvector/pgvector:pg17-trixie` | PostgreSQL 17, pgvector 0.7.0 (Table 66 and Table 51 are the outliers) |
| Bounded contexts / FR counts | Thesis tables themselves (internal consistency) | `chapters/part2/ch2-design/` | 8 contexts; 87 FRs across 8 modules |
| CBIR endpoint routes | Carter route definitions | `service/Api/src/Module/Features/` (Catalog storefront search) | Verify per REQ-108 |
| Embedding column type | EF Core migration + entity config | `service/Api/src/Module/` and `service/Api/src/Migrations/` | Fixed `vector(512)` per REQ-109 |
| Citation metadata | Real published papers | Cross-ref / publisher pages (DOIs in master fix list T1 #4) | `chia2022fashionclip`, `wu2019fashioniq`, `liu2016deepfashion` corrected |
| Ready-to-paste prose (use with CON-003) | Rewrite files | `thesis/reviews/thesis-rewrite-*.md` | "six" model count is wrong; "5.4%" assumes stale Table 67 |

## 5. Acceptance Criteria

- **AC-001**: Given the remediation log, when every row with severity 🔴 or 🟠 is inspected, then its `status` is `fixed` or `propagated` (not `unverified` or `in_progress`).
- **AC-002**: Given `benchmarks/outputs/thesis/results/thesis_results_category_only.json`, when Chapter 3 Table 67 and Appendix A Table 71 are compared, then every mAP, P@K, and R@K value matches the JSON exactly (FashionCLIP mAP = 0.9309, not the stale 0.8788).
- **AC-003**: Given Chapter 3 Table 68 and Appendix A Table 74, when compared against `thesis_results.json`, then every latency, throughput, load-time, and storage value matches (ResNet-50 load_time = 374.09 ms; storage 8.1/13.0/3.3/3.3 MB unchanged).
- **AC-004**: Given the thesis full text, when searched for "eleven models" / "11 models" / "eleven supported", then no occurrence has been changed to "six", and Table 55 lists all eleven registered models (the rewrite files' "six" recommendation was rejected per CON-001/CON-003).
- **AC-005**: Given the thesis full text, when searched for the Fashion-CLIP vs CLIP-generic improvement figure, then every occurrence states the same percentage — the REQ-101-recomputed value (~2.13% from Appendix A.1, not the stale 5.4%) — and no "15 to 20%", "6.1%", or "5.4%" occurrence remains.
- **AC-006**: Given `backmatter/bibliography.bib`, when entries `chia2022fashionclip` and `wu2019fashioniq` are inspected, then title, authors, venue, year, and pages match the corrected metadata in the master fix list TIER 1 #4, and `liu2016deepfashion` includes author "Shi Qiu".
- **AC-007**: Given `typst compile main.typ` run from `thesis/`, when executed after all fixes, then it exits 0 and produces `main.pdf` with no missing-image or unresolved-citation errors.
- **AC-008**: Given the thesis, when "eight bounded contexts", "87 functional requirements", "eight business modules", "nine use cases" (storefront), and the reconciled PostgreSQL/pgvector versions are searched, then every occurrence is consistent (no "nine bounded contexts", "88", "nine modules", "eight use cases", or version drift remains).
- **AC-009**: Given Table 70 (Requirements Traceability), when each "Addressed In" citation is followed, then it points to the section that actually contains the referenced content (no §2.1.5, no §2.2.4-for-pgvector, no §3.5-for-RQ3).
- **AC-010**: Given the remediation log, when the `notes` column is inspected for finding T1-2, then it records the CON-001 deviation (review/rewrite recommendation rejected; Table 55 expanded to 11 instead).
- **AC-011**: Given every derived percentage in §3.5–3.7 (the "X% above", "Y% of", "Z× faster" claims), when recomputed from the corrected Table 67 mAP values (0.9309 / 0.9115 / 0.8895 / 0.8857), then each stated percentage matches the recomputed value (the stale 5.4% / 7.7% / 8.2% / 92.8% / 26.0% set is replaced).
- **AC-012**: Given the deployment recommendation in §3.7.2, when re-read against the recomputed accuracy-efficiency trade-off, then its conclusion still follows from the new numbers (note: ResNet-50 0.8857 vs EfficientNet-B0 0.8895 are now nearly tied on accuracy, ~0.43% gap — confirm the recommendation's ranking still holds).

## 6. Test Automation Strategy

- **Test Levels**: Structural (Typst compile), content-consistency (grep/diff against authoritative sources), citation (bibliography metadata check), and arithmetic (recompute percentages from tables).
- **Frameworks / tooling**:
  - Typst compile: `typst compile main.typ` (the sole build verifier per `thesis/AGENTS.md`).
  - Consistency sweeps: ripgrep (`rg`) over `chapters/`, `frontmatter/`, `backmatter/` for each forbidden string and each required string.
  - Arithmetic recheck: a small Python script reading the benchmark JSON and recomputing 5.4% / 7.7% / 8.2% / 92.8% / 26.0% and the confidence intervals, asserting equality with the thesis-stated values (the review already verified 16 such checks; re-run after REQ-101).
  - Citation check: optional `bibtex`/Typst cite-resolution via the compile step (unresolved `@key` fails the build).
- **Test Data Management**: The benchmark JSON outputs in `benchmarks/outputs/thesis/results/` are the test data. Do not regenerate them during remediation unless REQ-101 concludes the thesis tables are wrong AND a re-run is commissioned; in that case regenerate, record the run config, and update both thesis tables and Appendix A from the single new run.
- **CI/CD Integration**: None in this folder (no CI for `thesis/` per `thesis/AGENTS.md`). The author should run `typst compile main.typ` locally before every commit.
- **Coverage Requirements**: 100% of 🔴 and 🟠 findings must reach `fixed`/`propagated` status. TIER 3 items may be `skipped` with a recorded reason.
- **Performance Testing**: Not applicable.

## 7. Rationale & Context

The thesis is a CTU bachelor (undergraduate) thesis due for committee review. The review
found that the underlying technical work is sound — implementation, code, analysis method,
and arithmetic all check out — but that numbers and citations were copy-pasted across
chapters during drafting and never re-synced. The remediation is therefore a
documentation-consistency exercise, not a research redo, with one exception: Table 67/68
in Chapter 3 are stale and must be regenerated from the authoritative benchmark run
(Appendix A / the JSON), which then cascades into recomputing every derived percentage
in §3.5–3.7 and updating the abstract, RQ answers, and Part 3 summary. The benchmark JSON
(`benchmarks/outputs/thesis/results/`) confirms Appendix A is authoritative: FashionCLIP
mAP 0.9309 in the JSON matches Appendix A.1, not Table 67's 0.8788.

The verify-before-fix rule (CON-001/CON-002/CON-003) exists because the review and the
rewrite files are themselves secondary artifacts, authored from the compiled PDF without
codebase or JSON access. During spec authoring this was shown to matter in two concrete
cases: (a) the "eleven models → six" recommendation is contradicted by the model registry,
which registers eleven adapters — Table 55 in the thesis is the stale artifact, not the
prose; and (b) the review left "which benchmark table is correct" as an open question,
which the JSON now answers (Appendix A). Applying either recommendation without checking
the authoritative source would have corrupted correct content or preserved a stale table.
This justifies treating every review/rewrite finding as a hypothesis to confirm against an
authoritative source, not an instruction to execute blindly.

The root-cause-first sequencing (REQ-006, GUD-001) exists because six findings are each
one error repeated in many places; fixing location-by-location without first fixing the
root guarantees partial fixes and rework. The benchmark reconciliation (REQ-101) is
sequenced first because the abstract, RQ answers, deployment recommendation, and Part 3
summary all derive from it and its derived percentages.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Typst 0.15.1 compiler — required to build `main.pdf` and to catch unresolved citations / missing image refs. No network access required.

### Third-Party Services
- **SVC-001**: Crossref / publisher pages — required to re-verify the corrected citation metadata for `chia2022fashionclip` (DOI 10.1038/s41598-022-23052-9) and `wu2019fashioniq` (CVPR 2021). One-time lookups.

### Infrastructure Dependencies
- **INF-001**: Local benchmark outputs at `benchmarks/outputs/thesis/results/*.json` — the authoritative benchmark numbers. Must exist and be readable before REQ-101.
- **INF-002**: Local codebase at `service/Api/src/Module/` and `service/Api/src/Migrations/` — the authoritative source for CBIR routes (REQ-108) and embedding column type (REQ-109).

### Data Dependencies
- **DAT-001**: `thesis/reviews/thesis-review-MASTER-FIXLIST.md` (and the nine per-chapter detail files) — the input findings list. Frozen for this remediation; do not edit the reviews to match the thesis.
- **DAT-002**: `thesis/reviews/thesis-rewrite-*.md` — ready-to-paste rewrite text; secondary resource subject to CON-003.
- **DAT-003**: `thesis/backmatter/bibliography.bib` — the citation source of record; edits target BibTeX keys.
- **DAT-004**: `benchmarks/outputs/thesis/results/*.json` — the authoritative benchmark results; the source of truth for REQ-101 (Appendix A matches this; Table 67/68 do not).

### Technology Platform Dependencies
- **PLT-001**: Typst 0.15.1 — version pinned per `thesis/AGENTS.md`; do not upgrade during remediation.
- **PLT-002**: ripgrep — for consistency sweeps; assumed available (repo uses `rg` in `scripts/`).

### Compliance Dependencies
- **COM-001**: CTU thesis format compliance (`thesis/compliance.json`: Times New Roman 13pt, margins L4/R2.5/T2.5/B2.5cm, line spacing 1.2, paragraph indent 1cm, abstract 200–350 words, 3–5 keywords) — remediation edits must not violate these; recompile to confirm.
- **COM-002**: Bibliography minimum of 15 references (IEEE) — the two citation replacements and the DeepFashion author addition must not reduce the reference count.

## 9. Examples & Edge Cases

### Example: applying CON-001 (rejecting a review AND rewrite recommendation)

```
Review/rewrite finding T1-2 says: change all "eleven models" to "six"
(Table 55 lists six). The rewrite files paste "selected from six" into
Part 1, Chapter 1, Chapter 2, and Part 3.

Step 1 — verify the registry: read benchmarks/src/benchmark/models/__init__.py:44-56.
  _register() returns 11 keys:
    efficientnet-b0, convnext-tiny, dinov2-vits14, fashion-clip,
    clip-b32, clip-generic, clip-l14, clip-vit-b16, siglip,
    resnet-50, eva-clip.

Step 2 — verify the benchmark run: read thesis_results_category_only.json.
  4 models evaluated (FashionCLIP, ResNet-50, EfficientNet-B0, CLIP-generic).
  So "four representative models" is correct; the question is only the
  "out of how many" denominator.

Step 3 — decide: registry (authoritative) has 11. Table 55 in the thesis
  lists 6 — Table 55 is stale, not the prose. The review/rewrite
  recommendation is REJECTED per CON-001/CON-003.

Step 4 — fix the root: update Table 55 (chapters/part2/ch2-design/.../fN/NN-model-management.typ)
  to list all eleven models, grouped by architecture family (CNN: ResNet-50,
  EfficientNet-B0, ConvNeXt-Tiny; ViT: DINOv2-ViTS14; CLIP-family: Fashion-CLIP,
  CLIP-ViT-B/16, CLIP-ViT-B/32, CLIP-ViT-L/14, CLIP-generic, SigLIP, EVA-CLIP).

Step 5 — propagate: confirm the six prose locations that say "eleven" are now
  consistent with the expanded Table 55. No text change needed there; the
  rewrite files' "six" must NOT be pasted.

Step 6 — log: record status=fixed, note the CON-001/CON-003 deviation.
```

### Example: applying REQ-101 (now resolved — Appendix A authoritative)

```
Step 1 — locate authoritative JSON:
  benchmarks/outputs/thesis/results/thesis_results_category_only.json
  (category-only ground truth, matching §3.4 methodology)

Step 2 — confirm which side matches the JSON. For Fashion-CLIP mAP:
  Thesis Table 67  = 0.8788   (stale)
  Appendix A.1     = 0.9309   (matches JSON)
  JSON             = 0.9309   AUTHORITATIVE

  Decision: Table 67 is stale. Regenerate Table 67 from Appendix A / JSON.
  Same for Table 68 vs Appendix A.4 (ResNet-50 load: Table 68 = 286.1 ms
  stale; Appendix A.4 = 374.1 ms matches JSON 374.09 ms).

Step 3 — regenerate Table 67 and Table 68 from the JSON (or copy Appendix A
  values if they already match exactly).

Step 4 — recompute every derived percentage in §3.5–3.7 from the new mAP
  values (0.9309 / 0.9115 / 0.8895 / 0.8857), replacing 5.4%, 7.7%, 8.2%,
  92.8%, 26.0% and all ×-factors. Recompute confidence-interval bounds.

Step 5 — propagate in order:
  Figures 42-45 -> frontmatter/abstract.typ -> RQ1/RQ2/RQ3 answers ->
  Part 3 Summary of Work -> §3.7.2 deployment recommendation (re-check
  that the ranking still holds; ResNet-50 vs EfficientNet-B0 gap narrows
  to ~0.43%).

Step 6 — recompile: typst compile main.typ.

Step 7 — log.
```

### Edge cases

- **Edge-1 (resolved — Table 67/68 are the stale ones):** The benchmark JSON confirms Appendix A.1 and A.4 are authoritative. No re-run is needed unless a later audit finds the JSON itself is not from the §3.4 protocol; in that case commission a re-run, record the config, and rebuild all tables from the new run.
- **Edge-2 (multiple JSON run directories):** `benchmarks/outputs/`, `outputs_5k/`, `outputs_5k_split/`, and `outputs/pipeline*/` all exist. The authoritative directory for the thesis is `benchmarks/outputs/thesis/results/` (its `splits/` contain the seeded 3-fold files matching §3.4.1). Record the chosen directory in the log.
- **Edge-3 (review/rewrite and codebase disagree — codebase/JSON wins):** Per CON-001/CON-002/CON-003, whenever the review or a rewrite file's stated "actual" value conflicts with the codebase or benchmark JSON, the codebase/JSON is authoritative. Record the deviation in the log `notes` column. Confirmed instances: the "six" model count (rewrite files) vs eleven (registry); the implicit "Table 67 may be correct" framing (review) vs Table 67 stale (JSON).
- **Edge-4 (Dashboard module):** REQ-106 drops Dashboard from the §2.1 module list because it has no FR table. If the author prefers to keep nine modules, add a Dashboard FR table instead (REQ-306) and keep "nine" — but then the FR total must still reconcile to whatever the tables sum to.
- **Edge-5 (deployment recommendation may flip):** Under the authoritative Appendix A numbers, ResNet-50 (0.8857) and EfficientNet-B0 (0.8895) are nearly tied on accuracy (~0.43% gap, vs the stale 8.2% gap). If §3.7.2's recommendation relied on the large accuracy gap to justify EfficientNet-B0, re-check the rationale; the efficiency advantage still favors EfficientNet-B0, so the recommendation likely holds, but the stated justification must be rewritten to match the new numbers.

## 10. Validation Criteria

The remediation is complete when ALL of the following hold:

1. `thesis/spec/remediation-log.md` exists and every 🔴 and 🟠 finding row is `fixed` or `propagated`; TIER 3 rows are `fixed` or `skipped` with a reason.
2. `typst compile main.typ` exits 0 from `thesis/` and `main.pdf` is produced.
3. AC-002 through AC-012 pass (verified by the consistency sweeps and the arithmetic recheck script).
4. No forbidden string remains: `rg -i "nine bounded contexts|88 functional|nine business modules|eight use cases|Section 2\.1\.5|Chapter 6|15 to 20|15-20 percent|6\.1%|near-zero P@20|Al-Zahir|Gieysztor"` returns no hits in `chapters/`, `frontmatter/`, or `backmatter/`. (The stale "5.4%" / "7.7%" / "8.2%" / "92.8%" / "26.0%" accuracy-claim strings are also replaced by the recomputed values, but those tokens may legitimately appear elsewhere — verify by location, not by global string ban.)
5. The remediation log's `notes` column records the CON-001/CON-003 deviation for finding T1-2 (review + rewrite "six" rejected; Table 55 expanded to 11).
6. `frontmatter/abstract.typ` benchmark numbers equal the reconciled Chapter 3 values (Appendix A / JSON) (GUD-003).
7. The bibliography still contains at least 15 entries (COM-002) and the abstract is 200–350 words with 3–5 keywords (COM-001).
8. The benchmark JSON at `benchmarks/outputs/thesis/results/thesis_results_category_only.json` has been read and its FashionCLIP mAP (0.9309) appears verbatim in the regenerated Table 67 and in `frontmatter/abstract.typ`.
9. Every rewrite file (`thesis/reviews/thesis-rewrite-*.md`) consulted during remediation has its pasted text checked against CON-001/CON-002/CON-003; any divergence is recorded in the log.

## 11. Related Specifications / Further Reading

- `thesis/reviews/thesis-review-MASTER-FIXLIST.md` — the master consolidated fix list (input findings; identical to `thesis/REVIEW.md`).
- `thesis/reviews/thesis-review-part1-introduction.md` … `thesis-review-appendices-bcd.md` — nine per-chapter detail files with full evidence and quoted text for each finding.
- `thesis/reviews/thesis-rewrite-part1-introduction.md` … `thesis-rewrite-part3-conclusion.md` — nine ready-to-paste rewrite files with change logs keyed to the master fix list. Use subject to CON-003; their "six" model count and any "Table 67 authoritative" assumption are wrong.
- `thesis/AGENTS.md` — thesis build/structure conventions (Typst 0.15.1, chapter pattern, figure/citation conventions, gotchas).
- `thesis/compliance.json` — CTU format-compliance constraints referenced by COM-001.
- `/home/ngtphat/Projects/ReSys.Shop/AGENTS.md` — platform-level rules and the verification commands.
- `benchmarks/AGENTS.md` — benchmark protocol, model registry, and run-output conventions (authoritative for REQ-101/REQ-102).
- `benchmarks/outputs/thesis/results/thesis_results_category_only.json` — authoritative category-only benchmark results (Appendix A.1 source).
