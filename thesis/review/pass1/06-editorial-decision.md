# Editorial Decision Letter — Pass 1 (Introduction + Background)

**Thesis**: Building a Fashion E-Commerce Application with Image-Based Product Search and Model Benchmarking
**Student**: Nguyen Thanh Phat (B2005853), Can Tho University
**Review Panel**: EIC Dr. Elena Vasquez (SoftwareX), R1 Prof. Markus Lindgren (Chalmers), R2 Dr. Shreya Kapoor (Amazon), R3 Michael Torres (Shopify), DA Prof. Arthur Kowalski (TU Dresden)
**Synthesizer**: Managing Editor
**Date**: 30 July 2026
**Review Scope**: Pass 1 — Introduction (Chapter 0) + Background and Related Work (Chapter 1)

---

## Decision

**MAJOR REVISION** — This chapter cluster is **not defense-ready**.

Three structural problems block defense: (1) DSR methodology exists only as a name-check (~10 lines) with no Hevner/Peffers instantiation (R1), (2) Chapter 3 benchmark results are pre-announced in Chapter 1, making Pass 1 review circular (R1, DA C3), and (3) the category-level relevance criterion collapses retrieval into classification — any dress matches any dress (DA C1, R1, R3). These are not cosmetic; they undermine the thesis's methodological foundation and core evaluation claim. The 7 unresolved diagram placeholders (EIC) further make the document presentationally incomplete.

However, the domain framing, system architecture scoping, and literature coverage are structurally sound (R2), and the honest limitation scoping is commendable (EIC). A focused rewrite of the methodology section, removal of forward-referenced results, and adoption of a within-category visual similarity criterion can bring this to defense-ready.

---

## Consensus Findings

*Issues where 3+ reviewers independently agree.*

| # | Finding | Reviewers |
|---|---------|-----------|
| **CF1** | Category-level relevance criterion is invalid for retrieval evaluation — "same category" collapses retrieval into classification | DA C1, R1 Major, R3 Minor |
| **CF2** | Redundancy/verbosity throughout: semantic gap defined twice (EIC), 770B stats repeated (EIC), three redundant vertical-slice definitions (R1), circular Fashion-CLIP narration (R2), textbook-style verbosity (R3) | EIC, R1, R2, R3 |
| **CF3** | Model count inconsistency — thesis claims 11 benchmarks, candidate tables show 10 (ResNet-152 missing) | R2 Major, DA Minor |
| **CF4** | Training/preprocessing reproducibility insufficient — missing seeds, version pins, HF model IDs, preprocessing specs (R3 Major), training data size 1.2M vs 1.28M (EIC Minor) | EIC, R3 |

---

## Disputed Issues

### D1: pgvector rationale — strength or overstatement?
- **EIC** praises pgvector rationale as a strength.
- **DA M2** argues pgvector advantages are overstated: async Hangfire queue breaks ACID claim, comparison table strawmans dedicated vector DBs.

**Arbitration:** Both partially correct. The architectural choice of pgvector-over-separate-vector-DB is defensible for a monolith (EIC). However, DA M2's technical objection is accurate — the thesis claims ACID transactional consistency while admitting an async Hangfire embedding pipeline outside the transaction boundary. **Action:** Retain pgvector rationale but remove ACID claim. Revise vector DB comparison table to represent Qdrant/Milvus capabilities accurately.

### D2: Severity of methodology gap
- **EIC** rates thin methodology paragraph as MINOR.
- **R1** rates uninstantiated DSR as CRITICAL.

**Arbitration:** R1 is correct on substance. ~10 lines of methodology vs. 15 lines on HNSW algorithm signals misplaced depth. For a bachelor's thesis, a named methodology (DSR) that lacks Hevner guidelines, Peffers process steps, design iterations, or artifact evaluation criteria is a structural gap. EIC's MINOR rating may reflect journal-calibrated expectations where methodology is assumed; at bachelor's level, explicit demonstration of methodological competence is required. **R1's CRITICAL severity is upheld.**

### D3: Sidecar novelty claim
- **R2 Major:** "Engineering gap overstated — sidecar pattern is standard practice."
- **DA M1:** "Polyglot sidecar pattern lacks novelty analysis (ONNX Runtime not considered)."

**Arbitration:** No dispute — both agree. R2 identifies overclaiming; DA M1 specifies missing comparison. **Consolidate:** Reduce "engineering gap" framing to honest trade-off analysis. Add comparison: sidecar vs. ONNX Runtime vs. ML.NET.

---

## DA CRITICAL Adjudications

### C1: "Relevance criterion conflates classification with retrieval"
**VALIDATED**

R1 independently raises identical concern ("'same category' means any dress matches any dress"), and R3 flags category-level relevance as insufficient. Three reviewers agree this is a core evaluation flaw. The thesis must replace or supplement category-level matching with a within-category visual similarity metric (e.g., attribute-based matching, triplet-based embedding similarity, or subcategory granularity). Without this fix, all mAP scores are inflated and the core claim — "this system retrieves visually similar products" — is not actually measured.

### C2: "Cold-start argument confuses retrieval with recommendation"
**PARTIALLY VALIDATED (scope clarification required)**

DA argues the thesis conflates visual-similarity retrieval with user-preference recommendation. No other reviewer raises this specific confusion. **Adjudication:** The DA's conceptual distinction is academically correct — visual similarity ≠ user preference — but the thesis's scope is image-based *search*, not personalized *recommendation*. In cold-start e-commerce search (no behavioral data, limited text metadata), visual similarity retrieval IS the appropriate first-order mechanism. The thesis is not wrong; it is imprecise in framing. **Action:** Add one-paragraph scope disclaimer distinguishing retrieval from recommendation, stating visual similarity is a retrieval proxy (not preference model), and acknowledging that cold-start *recommendation* would require additional layers.

### C3: "Benchmark data referenced but not presented — model selection relies on unseen Chapter 3"
**VALIDATED**

R1 independently confirms: "Fashion-CLIP declared winner using Ch.3 data before Ch.3." This is structural circularity — Chapter 1 announces conclusions Chapter 3 is supposed to produce. **Action:** Chapter 1 should state *what will be benchmarked* and *how*, not *what won*. Move all pre-announced results to Chapter 3.

---

## Revision Roadmap

### P0 — Must Fix Before Defense

| # | Task | Reviewers | Severity |
|---|------|-----------|----------|
| **P0.1** | Instantiate DSR methodology: add Hevner (2004) guideline mapping, Peffers (2007) process-step trace, explicit design iterations, and artifact evaluation criteria. Replace HNSW deep-dive in methodology with substantive methodological treatment. | R1 CRITICAL, EIC MINOR | CRITICAL |
| **P0.2** | Replace category-level relevance criterion with within-category visual similarity metric. Define ground-truth standard: what makes two fashion items "visually similar"? At minimum, use subcategory granularity (e.g., "floral midi dress" not just "dress"). | DA C1, R1 Major, R3 Minor | CRITICAL |
| **P0.3** | Remove all pre-announced benchmark results from Chapter 1. Move Fashion-CLIP winner declaration, mAP scores, and model-ranking claims to Chapter 3. Chapter 1 should state *candidate models*, *evaluation protocol*, and *hypothesis* — not conclusions. | R1 Major, DA C3 | CRITICAL |
| **P0.4** | Resolve all 7 diagram/figure placeholders with actual diagrams. | EIC CRITICAL, R1 Minor | CRITICAL |
| **P0.5** | Add between-group statistical methodology: define confidence intervals, effect-size measures, and paired statistical tests for benchmark comparisons. Without these, claims that "Model A outperforms Model B" are unsubstantiated. | R1 Major | CRITICAL |

### P1 — Strongly Recommended

| # | Task | Reviewers | Severity |
|---|------|-----------|----------|
| **P1.1** | Add public code repository reference (GitHub/GitLab) with README. Cite URL in Chapter 1. Without this, "open-source alternative" narrative is unsupported. | R3 CRITICAL | Major |
| **P1.2** | Include DeepFashion2 (2019) in literature review as standard benchmark successor to DeepFashion. | R2 CRITICAL | Major |
| **P1.3** | Fix model count inconsistency: reconcile "11 models" claim with 10-entry candidate table (ResNet-152 missing). Count must be consistent across abstract, Chapter 1, and Chapter 2. | R2 Major, DA Minor | Major |
| **P1.4** | Tone down "engineering gap" novelty claim. Add comparison: sidecar vs. ONNX Runtime vs. ML.NET with rationale. Frame as architectural trade-off, not gap-filling. | R2 Major, DA M1 | Major |
| **P1.5** | Revise pgvector justification: remove ACID claim (Hangfire queue breaks transactional boundary). Revise vector DB comparison table to represent Qdrant/Milvus capabilities accurately. | DA M2 | Major |
| **P1.6** | Remove or relocate "model selection" from Background (Chapter 2) to Design/Evaluation (Chapter 3). Chapter 2 should survey models; Chapter 3 should select them. | DA M4 | Major |
| **P1.7** | Add deployment reproducibility: Aspire manifest, Dockerfiles, docker-compose, seed values, HuggingFace model IDs with commit hashes, preprocessing pipeline specification, and variance reporting across runs. | R3 Major | Major |
| **P1.8** | Add cold-start scope disclaimer distinguishing visual-similarity retrieval from personalized recommendation. | DA C2 | Major |

### P2 — Nice to Have

| # | Task | Reviewers | Severity |
|---|------|-----------|----------|
| **P2.1** | Eliminate all redundancy: single definition of semantic gap, single vertical-slice architecture description, single dataset size (1.2M or 1.28M), single CBIR definition. | EIC Minor, R1 Minor, R2 Minor | Minor |
| **P2.2** | Remove circular narration: Background chapter should not build narrative arc toward Fashion-CLIP as inevitable choice. Present all models neutrally with documented trade-offs. | R2 Minor | Minor |
| **P2.3** | Fix imprecise language: DINOv2 does not "ignore colour"; CNNs do not "process patches" (ViT terminology). Use architecture-correct descriptions. | R2 Minor | Minor |
| **P2.4** | Add CLIP-RN50 as baseline to isolate architecture effect (ResNet) from domain-specialization effect (Fashion-CLIP fine-tuning). | R2 Major (calibrated to Minor for bachelor's scope) | Minor |
| **P2.5** | Add sub-300ms latency claim evidence or qualify with hardware specs and measurement methodology. | R3 Major | Minor |
| **P2.6** | Fix chapter numbering consistency throughout. | EIC Major | Minor |
| **P2.7** | Add hardware-limitation discussion: note CPU rankings do not generalize to GPU; CPU-only constraint is deliberate, not a general claim. | DA M3 | Minor |
| **P2.8** | Replace "real-time" with precise latency terminology (e.g., "interactive," "sub-second") where hardware constraints preclude true real-time guarantees. | DA Minor | Minor |
| **P2.9** | Replace Pinterest references with fashion-specific e-commerce sources where possible. | DA Minor | Minor |
| **P2.10** | Add RQ3 evaluation methodology: if RQ3 asks "is the architecture viable," define measurable viability criteria in Chapter 1. | R1 CRITICAL (for RQ completeness) | Minor |

---

## Writing Polish Summary

*Consolidated from all five reviewers' writing-quality observations.*

### What Works
The thesis demonstrates competent technical English and domain-appropriate terminology (R2: Writing Quality 4/5). Scoping is honest and limitations are used as a forward-looking scaffold (EIC). Sentence-level grammar is largely correct.

### Structural Problems to Fix

1. **Chapter-numbering inconsistency:** Use a single, sequential numbering scheme throughout (EIC Major).

2. **Eliminate all redundancy.** Every concept defined more than once is dead weight:
   - Semantic gap: define once in Background, reference thereafter (EIC).
   - 770B dataset statistics: state once in the dataset section (EIC).
   - Vertical-slice architecture: one definition, not three (R1).
   - CBIR: define once (EIC).
   - Fashion-CLIP selection narrative: present neutrally, don't loop back (R2).

3. **De-textbook the prose.** The current tone reads like a survey paper, not a thesis (R3). A thesis should argue, not enumerate. Replace:
   - Enumerative transitions with argument-driven flow (EIC).
   - Textbook-style exposition of well-known concepts (CNN basics, HNSW internals) with concise citations to canonical sources (R3).

4. **Break run-on sentences.** Several reviewers noted long, multi-clause sentences that obscure the argument (EIC). Target: ≤25 words per sentence for technical prose.

5. **Precision in technical language (R2, DA):**
   - "DINOv2 ignores colour" → "DINOv2's self-supervised objective deprioritizes low-level colour features."
   - "CNN processes image patches" → "CNN applies convolutional kernels across spatial dimensions."
   - "Real-time" → "Interactive latency" or "Sub-second response" (unless hard real-time guarantees are measured).
   - "Processes" → Use specific verbs: convolves, embeds, indexes, retrieves, ranks.

6. **Consistency audit:** After fixing P0–P2 items, perform full-pass check:
   - Model count: 11 or 10? (R2, DA)
   - Training dataset: 1.2M or 1.28M? (EIC)
   - All cross-references: do figure/table numbers match targets?
   - All acronyms: defined exactly once at first use (CBIR, DSR, HNSW, mAP, ViT).

7. **Formulaic transitions:** Replace "In this chapter, we will discuss..." and "This section describes..." with declarative topic sentences that state the claim, not the intention (EIC).

### Tone Calibration for Defense
A bachelor's thesis defense audience expects:
- **Confidence without overclaiming:** "We selected X because Y" (supported), not "X is clearly the best choice" (pre-announced results).
- **Honest limitations as strength:** Extend existing good practice to hardware generalizability (DA M3) and latency claims (R3).
- **Methodological self-awareness:** Be able to explain *why* DSR was chosen and *how* Hevner/Peffers guided the process (R1).

---

*Synthesized from reports by EIC (Dr. Elena Vasquez), R1-Methodology (Prof. Markus Lindgren), R2-Domain (Dr. Shreya Kapoor), R3-Industry (Michael Torres), and DA (Prof. Arthur Kowalski). No new issues introduced beyond those raised in the five source reports.*
