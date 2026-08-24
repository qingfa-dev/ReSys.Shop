# Master Reduction Prompt — Fashion E-Commerce Thesis (Single-Agent, 20–30 pp)

This is a single, copy-pasteable prompt for **one** editing agent that performs the entire
20–30 page reduction while enforcing every technical safeguard. It has been corrected
against the **current** source of truth (`thesis/`, Typst) and the **committed benchmark
outputs**, so it supersedes any stale numbers in prior review material.

```text
# Role
Act as the senior academic-thesis editor, software-engineering reviewer, and
experimental-methodology reviewer. You are supervising ONE agent (yourself) performing a
targeted 20–30 page reduction of an existing Bachelor's graduation thesis. This is NOT a
generic shortening task: the objective is a shorter thesis that is MORE focused and no
weaker.

Current length: approximately 183 pages (main.pdf).
Target final length: approximately 155–163 pages.
Do NOT aggressively minimize toward the smallest possible document.

# Source of Truth
Primary source: the thesis source tree under `thesis/` (Typst files, compiled to
`main.pdf`). Treat the actual thesis content as authoritative.
Authoritative benchmark numbers: the committed outputs under
`benchmarks/outputs/thesis_catonly` (category-only), `thesis_6models` (category+colour),
and `thesis_catpat` (category+colour+pattern). These MATCH the thesis; do not "correct"
thesis numbers to match the stale `outputs_5k`/`outputs_5k_split` runs, which are NOT
authoritative.
Do not invent missing facts. Do not silently "correct" technical facts from general
knowledge. Recheck any prior review's page counts, use-case counts, table counts, or model
dimensions against the thesis before trusting them.

# Verified Facts (use these, do not re-derive from memory)
Title: "Building a Fashion E-commerce Application with Recommendation and Image-Based
Product Search". Vue 3 frontend, .NET backend, modular monolith, vertical-slice/CQRS,
PostgreSQL + pgvector, Redis, Python FastAPI ML sidecar, pre-trained vision/multimodal
models, image-based CBIR, content-based visual recommendation, six-model embedding
benchmark on the Fashion Product Images dataset (~5,000 images, 3-fold CV).

Use-case inventory (VERIFIED by recounting from source): 25 total = 14 Administrator +
9 Customer + 2 System.
- Administrator (14): UC-ADM-PROD, UC-ADM-VAR, UC-ADM-IMG, UC-ADM-TAX, UC-ADM-OPT,
  UC-ADM-ORD, UC-ADM-ORD-ITEMS, UC-ADM-PAY, UC-ADM-LOC, UC-ADM-STK, UC-ADM-USR,
  UC-ADM-ROL, UC-ADM-SHP, UC-ADM-REF.
- Customer (9): UC-STR-AUT, UC-STR-BRW, UC-STR-CRT, UC-STR-CHK, UC-STR-OHI, UC-STR-PAY,
  UC-STR-PRF, UC-STR-SES, UC-STR-SRC.
- System (2): UC-SYS-EMB, UC-SYS-MNT.
(NOTE: A prior review claimed "26 use cases" and "12 of 15 admin" — both are WRONG. The
correct totals are 25 and 14. Do not propagate the old counts.)

Six models, category-only mAP (mean ± SD), verified against committed outputs:
Fashion-CLIP 0.9336 ± 0.0060; DINOv2 ViT-S/14 0.9299 ± 0.0058; CLIP ViT-B/16
0.9202 ± 0.0043; CLIP ViT-B/32 0.9184 ± 0.0060; ResNet-50 0.9132 ± 0.0057;
EfficientNet-B0 0.9077 ± 0.0076. Ground-truth sensitivity (Fashion-CLIP mAP):
category-only 0.9336, category+colour 0.2439, category+colour+pattern 0.2071.

CRITICAL statistical interpretation: the Fashion-CLIP vs DINOv2 difference is 0.40% and,
with only 3 folds, within measurement uncertainty. Do NOT describe it as statistically
established superiority. The Fashion-CLIP ±2SD lower bound (0.9216) OVERLAPS the upper
bounds of DINOv2 (0.9415), CLIP B/16 (0.9288), CLIP B/32 (0.9304), ResNet (0.9246), AND
EfficientNet (0.9229). It does NOT "exceed" any of them. Correct any phrasing that says it
does.

The system does NOT implement: collaborative filtering, personalized behavioural
recommendation, session-based recommendation learning, or custom training/fine-tuning of
the evaluated models. Describe recommendation precisely as "content-based product
recommendation through visual similarity."

Embedding/vector design: the active schema uses vector(512) with model-aware isolation;
different candidate models have different dimensionalities (DINOv2 384, CLIP 512,
EfficientNet 1280, ResNet 2048). Preserve this. Do not imply arbitrary models can share a
fixed vector(512) column without dimensional compatibility. Do not imply dimension
conversion happens automatically.

# Main Editorial Principle
"Explain each idea once at the correct abstraction level, then reference it instead of
reproducing it."
- §2.2 answers WHAT the system must do.
- §2.3 answers HOW the system is designed.
- §2.4 answers HOW the design was implemented.
- Chapter 3 answers HOW WELL the implementation worked.
Do not let one section repeatedly perform another section's job. Replace duplication with
cross-references such as: "The functional behaviour is specified in §2.2.x; this section
focuses on implementation."

# Page Reduction Budget (do not exceed ~30 pp unless a separate review justifies it)
1. §2.2 Use Cases (pp. 39–72): reduce 8–10 pages.
2. §2.4.5 Frontend Applications (pp. 98–122): reduce 8–10 pages.
3. Appendix D Database Schema (pp. 153+): reduce 5–10 pages.
Total target: 21–30 pages; final ~155–163 pages.

## §2.2 Use Cases
Keep FOUR detailed: UC-ADM-PROD (representative admin CRUD), UC-STR-SRC (central CBIR
contribution: upload -> embedding -> vector search -> threshold -> results), UC-STR-CHK
(complex multi-step transactional checkout), UC-SYS-EMB (automated background/ML
processing). Verify these are genuinely representative before finalizing.
For the remaining 21 use cases, do NOT delete — convert to a compact summary representation
containing at least: Actor | UC ID | Name | Goal | Trigger | Related FRs. Where a business
rule is genuinely unique, preserve a short note. Do not force identical sentence lengths.
Do not remove: use-case IDs, related functional requirements, the visual-search scenario,
payment/checkout logic, embedding behaviour, or system-boundary/actor diagrams.
Keep the use-case layer technology-independent (do not copy Vue component details in).

## §2.4.5 Frontend Applications
Preserve in detail: frontend architecture (structure, API-client, state management,
component boundaries), the complete Visual Search implementation, one reasonably detailed
Checkout implementation, and one representative Admin module (Product Management).
For all other admin/storefront screens, replace pages of screenshots+prose with: one
concise paragraph + at most one representative code/API fragment + at most one screenshot
that proves a NON-obvious implementation fact. Screenshots whose only purpose is "the UI
exists" should be compressed. Create a compact matrix:
Module | Vue Component(s) | Main API | Distinctive Behaviour.
Do not repeat business flows already in §2.2. Do not invent component names or API
endpoints; use only those present in the thesis. Do not convert descriptive UI content into
architectural claims.

## Appendix D Database Schema
Keep full/near-full treatment of: Product, Image Embedding / Product Image Embedding,
Order, Payment Capture, Stock Item, Stock Reservation (verify exact table names in the
thesis). Preserve evidence of: aggregate boundaries, vector dimensionality, model
isolation, indexing, concurrency, idempotency, state transitions, inventory reservation,
transaction integrity.
For ordinary CRUD/reference tables, replace exhaustive column lists with:
Table | Purpose | Key Relationships | Important Columns/Constraints.
Do NOT remove: the vector(512) explanation, model_name discrimination, per-model
dimensionality, the need for isolation between incompatible embeddings, or the HNSW/IVFFlat
description. Do not invent constraints. Do not claim all embeddings share vector(512).
Where appropriate, note that migration-level definitions live in the implementation
repository (only if true) and this appendix covers schema needed to understand the thesis.

# Known Technical Issues to FIX while editing (independent of page count)
1. Statistical overclaim (CRITICAL): fix any statement that Fashion-CLIP's ±2SD lower
   bound "exceeds" the CNN models' upper bounds. It overlaps ALL of them. Reword to
   "overlaps the upper bounds of all other models, indicating a statistically
   indistinguishable top tier on category-only retrieval."
2. Efficiency ratio error (MAJOR): §3.6 says EfficientNet-B0 (42.6 ms) is "2.2× faster
   than DINOv2 (126.3 ms)". 126.3/42.6 = 2.96, so this should be ~3.0×, not 2.2×.
   Also check the claim that throughput 21.4 is "2.1× higher than the two CLIP variants'
   lower range" — 21.4/4.0 = 5.4× and 21.4/11.9 = 1.8×; the 2.1× figure corresponds to
   21.4/10.2 (DINOv2/ResNet), so the "CLIP variants" reference is mislabeled. Correct the
   referent or the number.
3. RQ3 (MAJOR): distinguish "demonstrated experimentally" from "supported by architectural
   design" from "expected property". If the thesis says "independent scaling and fault
   isolation were achieved", reword unless there are actual experiments (service failure,
   scaling instances, resource isolation, recovery, throughput under concurrent load).
4. Terminology (MAJOR): "production-ready" -> "production-oriented"/"production-style"/
   "deployment-feasible"/"feasible for small-scale deployment" unless directly justified.
5. RAM (MODERATE): Chapter 3 lists RAM ranges (~100/~150/~600 MB) while Appendix A lists
   "N/A". Both already disclose the figures are not instrumented (psutil unreliable).
   Standardize the disclosure so the two do not appear contradictory.
6. The "eleven candidates it supports" claim is DEFENSIBLE (the registry supports 11; 6
   selected). Keep it; do not "correct" it to six.

# Editing Rules (mandatory)
1. Do not rewrite the entire thesis stylistically.
2. Do not change technical claims merely to make them sound stronger.
3. Do not invent experimental evidence.
4. Do not introduce citations without source verification.
5. Do not remove references that support surviving claims.
6. Do not remove figures that carry unique information.
7. Do not remove benchmark tables merely because they are long, if they provide evidence
   required by a research question.
8. Replace repetition with cross-reference where possible.
9. Preserve terminology and numbering.
10. Preserve requirements traceability.
11. Preserve use-case IDs.
12. Preserve model names exactly (Fashion-CLIP, DINOv2 ViT-S/14, CLIP ViT-B/16, CLIP
    ViT-B/32, ResNet-50, EfficientNet-B0).
13. Preserve numerical results exactly unless an explicit verified correction is applied.
14. Flag contradictions instead of silently resolving them.

# Required Workflow
Phase 1 — Audit: produce a page-by-page reduction map, redundant-content map,
technical-risk map, numerical-consistency map, terminology-consistency map, and lists of
pages to preserve/compress. Do not edit yet.
Phase 2 — Revision Design: for every proposed deletion state: current subsection,
approximate pages affected, information removed, reason redundant, where the surviving
information remains, whether a cross-reference is required, estimated page savings, risk.
Phase 3 — Edit: compress, rewrite, insert cross-references; preserve technical evidence and
academic qualification of claims.
Phase 4 — Integrity Review: verify every research question, objective, and contribution
still has evidence; use-case IDs exist; requirements map correctly; figure/table references
exist; numbering and terminology consistent; no numerical result changed accidentally; no
unsupported claim became stronger; no important limitation disappeared.
Phase 5 — Page Count Review: report old page count, new page count, pages removed, pages
retained, pages moved to appendix, and whether the 20–30 page target was met.

# Output Format
Return five sections:
A. Executive Diagnosis (major structural problems)
B. Page Reduction Plan (exact section-level targets)
C. Technical Integrity Problems (claims/methods needing correction independent of cuts)
D. Editing Constraints (rules for subsequent editors)
E. Agent Handoff (compact context block to paste into the next specialist agent)

Do not begin editing until the audit is complete.
```
