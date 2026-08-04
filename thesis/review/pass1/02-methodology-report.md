# Methodology Review Report — Pass 1 (Introduction + Background)

**Reviewer:** Prof. Dr. Markus Lindgren, Associate Professor of Information Systems, Chalmers University of Technology
**Role:** Peer Reviewer 1 — Methodology
**Review Date:** 30 July 2026

---

## Methodology Assessment

The thesis declares a Design Science Research (DSR) methodology with citations to Hevner et al. (2004) and Peffers et al. (2008) and sketches four phases: Research and Planning, Design, Implementation, and Test and Evaluation. **However, DSR is name-checked rather than instantiated.** The four-phase description maps coarsely to Peffers' six-step model but omits the essential DSR mechanisms that distinguish it from a routine software development project: iterative build-evaluate cycles, formative evaluation feeding back into redesign, explicit design principles articulated from the artifact, and a summative evaluation framework assessing utility, quality, and efficacy against the problem statement. The methodology reads as a linear waterfall — research, design, build, test — with no indication that any phase informed a revision of a prior phase. For a bachelor's thesis, the linear approach may have been pragmatically necessary, but the text should not claim DSR without demonstrating at least one iteration where evaluation results drove a design change.

The artifact itself is ambiguous in DSR terms. The thesis describes the primary output as "a working system accompanied by empirical data on performance trade-offs," which conflates the construct (the platform), the model (the architecture), and the method (the benchmark). Hevner requires clarity on what the artifact is and how it is evaluated. The benchmark evaluation is quantitative and systematic, but it evaluates models, not the design artifact itself. **The architecture — the polyglot pattern, the vertical slice structure, the pgvector integration — is presented in Chapter 1 as a fait accompli without any design evaluation criteria or validation method** beyond reporting end-to-end latency. DSR requires that the artifact's utility be demonstrated relative to the problem; here, the problem of "polyglot integration cost" (RQ3) is not tested against an alternative architecture or baseline. The evaluation described measures embedding model performance, not architectural fitness.

For defense readiness, the student must either (a) properly instantiate DSR by mapping each phase to Peffers' six steps, identifying at least one concrete design iteration, and defining artifact evaluation criteria, or (b) re-label the methodology as "systems development and empirical benchmarking" and drop the DSR framing, which currently over-claims methodological rigor.

## Technical Writing Clarity

**Clear passages (commendable):**

- *Cosine similarity explanation* (Section 1.2.4): The mathematical notation is precise and the interpretation ("scores above 0.70 generally correspond to strong visual similarity") gives a concrete, committee-accessible anchor. This is textbook-caliber technical exposition for a mixed audience.
- *CBIR pipeline* (Section 1.2.6): The four-stage summary (Image Input → Embedding Generation → Vector Comparison → Ranking & Retrieval) is concise and well-sequenced. A committee member unfamiliar with CBIR can follow the flow.
- *Architectural patterns comparison* (Section 1.5.1): The monolith → modular monolith → microservices progression with trade-offs is clearly structured. The decision paragraph succinctly justifies the chosen combination.

**Unclear or problematic passages:**

- *Methodology section (Section 1.1.8):* This is the weakest paragraph in the entire submission. At approximately 10 lines of prose, it is **dramatically undersized** relative to its importance and relative to sections like "HNSW" (15 lines on one indexing algorithm) or "Vue.js Frontend" (8 lines). The methodology that produced the entire thesis receives less space than the description of a single background job library (Hangfire). **Location: Section 1.1.8. Severity: CRITICAL.**
- *Pre-announcement of results in Chapter 1 (Section 1.3.5):* The "Model Selection and Justification" section resolves RQ1 within the background chapter by declaring that "Fashion-CLIP achieved the highest mAP@10 among the evaluated models," "a 15-to-20% improvement over general CLIP," and "confirmed through the systematic benchmark presented in Chapter 3." This is structurally incoherent: Chapter 1 cites Chapter 3's results before Chapter 3 exists for the reader. The background chapter can describe candidate models and selection *criteria*, but the *selection decision* based on empirical results belongs in the evaluation chapter. **Location: Section 2.3.5 (Selection Decision). Severity: MAJOR.** The entire "Selection Decision" subsection should move to Chapter 3 or be rewritten as criteria-only.
- *Redundancy on vertical slice architecture:* The same concept appears three times with slightly different phrasings. Consolidate. **Location: Sections 1.5.1, 1.5.2, 1.5.2.4. Severity: MINOR.**
- *Missing definitions for defense audience:* Terms like "CQRS," "ACID," and "TOTP" appear without expansion or definition. A defense committee may include non-computing faculty. **Location: scattered. Severity: MINOR.**
- *Diagram placeholders:* Placeholder comments for missing figures suggest the draft is incomplete. **Location: lines 187, 197, 690. Severity: MAJOR.**

## Research Questions Evaluation

**RQ1 — Model comparison:** "How do fashion-specific embedding models compare with general-purpose CNN and ViT architectures on fashion product retrieval?"

The question is well-defined but coarse. The benchmark framework supports comparison across 11 models with standard IR metrics (mAP, P@K, R@K). However, the relevance criterion — "same category as the query image" — reduces the retrieval problem to category classification: a model that retrieves any dress for a dress query scores equally to one that retrieves the most visually similar dress. For a fashion CBIR thesis, this is problematic. **The described methodology can answer RQ1 at the category level but may not discriminate between models on genuine visual similarity.** If the multi-label pipeline (category → category+colour → category+colour+pattern) is adopted as the primary evaluation, this concern is addressed.

**RQ2 — Accuracy-speed trade-off:** "What trade-offs exist between retrieval accuracy and inference latency?"

This is the best-supported question. The benchmark framework measures both accuracy (mAP, P@K, R@K) and efficiency (inference latency ms/image, throughput images/s) on identical hardware (Intel i7-1165G7, 16GB RAM). The cross-validation protocol with stratified category splits provides replication. The methodology produces the scatter-plot data needed to identify Pareto-frontier models. The CPU-only constraint is acknowledged. **Answerable with the described methodology.**

**RQ3 — Architecture viability:** "Can a service-oriented architecture with a dedicated AI sidecar separate image inference from the main application while maintaining interactive response times?"

This question is **under-specified and weakly supported by the methodology as described.** The pgvector pipeline mode measures end-to-end latency, but "architecture viability" is broader than a single latency number. The methodology does not describe: (a) how the sidecar pattern is evaluated against an in-process alternative or baseline, (b) failure-mode testing (sidecar crash, timeout behavior, cold-start cost), (c) throughput scaling under concurrent load, or (d) developer/maintainer ergonomics. As written, RQ3 can be answered only with "the system achieved X ms end-to-end latency," which addresses response time but not viability in a broader architectural sense. **Suggested fix:** Either narrow RQ3 to a latency constraint ("Can the sidecar architecture meet a sub-300ms response time target?") or add architectural evaluation criteria to the benchmark chapter (failure handling, concurrent load testing).

## Strengths

1. **The benchmark framework is well-specified** (Section 1.5.9): three evaluation modes, caching strategy, multi-format export, and a rich metrics palette (mAP, P@K, R@K, nDCG, latency, throughput, storage). This is professionally designed and avoids the common student error of a one-shot benchmark with no reproducibility mechanism.

2. **Technology stack justification is thorough.** Each component (pgvector, Redis two-tier cache, Hangfire, hybrid JWT-guest auth) has a clear rationale tied to system requirements. The pgvector transactional-consistency argument is a genuinely insightful architectural decision.

3. **The polyglot integration problem is well-motivated.** The framing of the Python/.NET boundary as a recurring applied-ML engineering challenge gives the thesis an identity beyond "we used some models on some images."

4. **Cosine similarity exposition** (Section 1.2.4) is pedagogical and precise. This is representative of the thesis at its clearest.

## Weaknesses / Issues

1. **CRITICAL — DSR methodology is name-checked, not instantiated** (Section 1.1.8). The methodology paragraph is ~10 lines. Hevner's seven DSR guidelines are not referenced, Peffers' six steps are not mapped, no design cycle iteration is described, and no artifact evaluation criteria are defined. **Fix:** Either expand to a full subsection (0.5–1 page) mapping each DSR guideline/step OR downgrade to "systems development and empirical benchmarking."

2. **CRITICAL — RQ3 has no corresponding evaluation methodology.** RQ1 and RQ2 have well-defined metrics and protocols; RQ3 has none. Without this, one of three research questions cannot be rigorously answered.

3. **MAJOR — Results pre-announced in Chapter 1** (Section 2.3.5). The "Selection Decision" subsection declares Fashion-CLIP the winner using Chapter 3 results before Chapter 3. The selection should be criteria-based anticipation, with empirical confirmation deferred to Chapter 3.

4. **MAJOR — Relevance criterion is too coarse.** "Same category" relevance means any dress matched to any dress is a hit. This evaluates category classification accuracy, not visual similarity retrieval.

5. **MAJOR — No statistical significance methodology.** The benchmark description lists metrics but no plan for confidence intervals, effect size calculation, or paired statistical tests. Without these, Chapter 3 cannot claim one model is "significantly better."

6. **MINOR — Missing figure placeholders.** Three diagram placeholders indicate incomplete artifacts.

7. **MINOR — Three redundant vertical-slice definitions** (Sections 1.5.1, 1.5.2, 1.5.2.4). Consolidate to one authoritative definition with forward references.

## Dimension Scores (methodology-relevant)

- **Methodological Rigor: 2/5.** DSR is cited but not executed. The four-phase linear description has no iteration, no formative evaluation feedback, no design principles articulation, and no artifact validation framework. The benchmark protocol is well-designed in isolation but serves a systems-comparison goal rather than a DSR evaluation goal.

- **Evidence Sufficiency (from what's described so far): 3/5.** The benchmark framework is detailed and the metrics set is appropriate for RQ1 and RQ2. However, the coarse relevance criterion, absence of statistical testing, and lack of any evaluation method for RQ3 prevent a higher score.

- **Writing Quality: 3/5.** Above average for an undergraduate thesis when it focuses on technical exposition (CBIR pipeline, cosine similarity, architecture comparison). Pulled down by the critically undersized methodology section, the structural error of pre-announcing results, definition gaps for domain terminology, and incomplete figure placeholders. A single editing pass focused on these four issues would raise this to a 4.

## Confidence Score: **4/5**

I am confident in this assessment based on the text provided in Pass 1. The one area where confidence is lower is RQ3: if the student has architectural evaluation data in Chapter 3 not described in the Pass 1 methodology sketch, the score might shift. A review of Chapter 3 is needed.
