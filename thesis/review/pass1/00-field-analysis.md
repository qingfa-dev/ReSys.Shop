# Field Analysis Report — Pass 1 (Introduction + Background)

**Thesis**: Building a Fashion E-Commerce Application with Image-Based Product Search and Model Benchmarking
**Student**: Nguyen Thanh Phat (B2005853), Can Tho University
**Review Scope**: Introduction (Chapter 0) + Background and Related Work (Chapter 1)

---

## 1. Field Analysis

### Primary Discipline

**Software Engineering** — applied system construction with empirical evaluation. The thesis explicitly states its contribution is *architectural, not algorithmic*: embedding existing ML capabilities into a conventional web stack and measuring the trade-offs practitioners face.

### Secondary Disciplines

| # | Discipline | Basis |
|---|-----------|-------|
| 1 | **Machine Learning / Computer Vision** | CBIR pipeline, 11-model benchmarking across CNN/ViT/CLIP families, embedding theory, latent space mathematics |
| 2 | **Information Retrieval** | Vector similarity search, ANN algorithms (HNSW, IVFFlat), precision/recall/mAP/nDCG metrics, pgvector integration |
| 3 | **Information Systems** | DSR methodology, polyglot architecture design, e-commerce domain modelling, .NET enterprise stack integration |

### Research Paradigm

**Design Science Research (DSR)** — explicitly self-identified. Produces and evaluates an IT artifact (the ReSys.Shop platform with integrated visual search) to address a defined problem domain. Follows four DSR phases: research/planning, design, implementation, test/evaluation.

### Methodology Type

**Quantitative empirical evaluation** — systematic benchmark of 11 pre-trained embedding models on 5,000 fashion images using stratified cross-validation, with standard IR metrics (mAP, P@K, R@K, nDCG) and operational metrics (latency, throughput, storage). Supplemented by qualitative visual inspection. No formal user study.

### Target Journal Tier: Q3

**Rationale:** The work is strong for a bachelor's thesis but as a standalone publication candidate, it is an *experience report / system demonstration* rather than a novel contribution. Key limiting factors:

- **No novel algorithm or model** — contribution is engineering integration, not theoretical advance.
- **Dataset modest** (5,000 images) — limits generalisability claims.
- **Single-hardware evaluation** (consumer CPU only) — constrains external validity.
- **The benchmark chapter (Ch.3) carries the empirical weight** — if that chapter shows methodologically rigorous, reproducible results, the work could reach Q2 in venues like *SoftwareX* or *Journal of Systems and Software*. Realistic baseline is Q3.

### Paper Maturity: Defense-Ready (with reservations)

| Strength | Concern |
|----------|---------|
| Clear problem statement and research gap | Occasional redundancy (semantic gap explained multiple times) |
| Well-justified technology choices | Some sections read like a textbook survey rather than a focused argument |
| Honest scope/limitation acknowledgment | Writing polish varies; some passages are crisp, others verbose |
| Strong architectural decision documentation | Terminology consistency needs verification |
| Systematic model selection framework | Typst markup needs rendering pass |

---

## 2. Recommended Target Journals (Top 3)

| # | Journal | Rationale |
|---|---------|-----------|
| 1 | **SoftwareX** (Elsevier, Q2) | Dedicated to software tools and applications with reproducible research. The ReSys.Shop platform, benchmark framework, and open-source architecture pattern are an ideal fit. |
| 2 | **Journal of Systems and Software (JSS)** (Elsevier, Q1–Q2) | Publishes software engineering practice papers, including architectural design, system integration, and empirical evaluation. The polyglot pattern and accuracy-efficiency trade-off data target this audience. |
| 3 | **IEEE Access** (IEEE, Q2–Q3) | Broad multidisciplinary scope accepting applied engineering papers with rigorous empirical work. The 11-model benchmark satisfies the empirical standard. |

---

## 3. Reviewer Configuration Cards

### EIC — Dr. Elena Vasquez

- **Role**: Editor-in-Chief
- **Identity**: Senior Associate Editor at *SoftwareX*, University of Amsterdam. 15 years in applied software engineering research. Former industry architect at Bol.com (Dutch e-commerce).
- **Review Focus**: (1) Originality assessment — does the polyglot sidecar pattern contribute beyond known patterns? (2) Significance — is the benchmark data genuinely useful to practitioners? (3) Writing quality and coherence — does the thesis read as a unified argument?
- **Will particularly care about**: Reproducibility of the benchmark. Are models, cross-validation, caching, and pgvector configuration documented sufficiently?
- **Possible blind spots**: May undervalue pedagogical contribution. May overlook significance of consumer-CPU constraint as deliberate choice.

### Reviewer 1 — Methodology (Prof. Dr. Markus Lindgren)

- **Role**: Peer Reviewer 1
- **Identity**: Associate Professor of Information Systems at Chalmers University of Technology, Sweden. 40+ papers on DSR methodology evaluation. Senior member of AIS DSR community.
- **Review Focus**: (1) DSR rigor — proper Hevner/Peffers instantiation? (2) Benchmark protocol validity — stratified 3-fold cross-validation correctly designed? (3) Technical writing clarity — architectural descriptions precise enough?
- **Will particularly care about**: Whether stratified split is category-based (not product-ID based), whether similarity thresholds are justified, whether caching introduces evaluation bias.
- **Possible blind spots**: May expect DSR formalism exceeding bachelor's scope. May penalise 5K-image dataset without considering computational constraints.

### Reviewer 2 — Domain (Dr. Shreya Kapoor)

- **Role**: Peer Reviewer 2
- **Identity**: Research Scientist at Amazon Visual Search (Bangalore), previously NUS Centre for Fashion AI. 25+ publications on fashion image retrieval, multimodal embeddings, e-commerce recommendation.
- **Review Focus**: (1) Literature coverage — DeepFashion, Fashion IQ, Fashion-CLIP, HNSW covered? Missing critical works? (2) Research gap positioning — convincingly argued or strawman? (3) Domain contribution — does 11-model benchmark add value?
- **Will particularly care about**: Whether benchmark produces *surprising* results or merely confirms Fashion-CLIP paper. Whether exclusion of recent models (SigLIP, EVA-CLIP) is justified.
- **Possible blind spots**: May undervalue architectural contribution due to model-centric lens. May expect SOTA leaderboard comparison exceeding scope.

### Reviewer 3 — Industry / Perspective (Michael Torres)

- **Role**: Peer Reviewer 3
- **Identity**: Senior Engineer at Shopify (Ottawa), Open Source contributor to pgvector, maintainer of *pgai*. 12 years building search infrastructure for e-commerce. Previously led search relevance at Etsy.
- **Review Focus**: (1) Practical deployability — can a team replicate this? (2) Open-source value — reusable contributions beyond the thesis? (3) Cost/benefit realism — does pgvector at 5,000 items represent reality?
- **Will particularly care about**: Whether single-PostgreSQL approach survives. pgvector deployments at scale. Hangfire failure handling.
- **Possible blind spots**: May dismiss academic contributions not translating to his environment. May undervalue pedagogical value.

### Devil's Advocate — Prof. (Emeritus) Dr. Arthur Kowalski

- **Role**: Devil's Advocate
- **Identity**: Former Chair of Computer Science at TU Dresden. 40 years across databases, systems, and AI. Known for the "Kowalski Test": if your contribution can be summarised in one sentence and an enterprising undergraduate could replicate it in a semester, it's not PhD-worthy.
- **Review Focus**: (1) Core argument challenge — what is the minimum defensible contribution? (2) Logical fallacies — cherry-picking, circular reasoning, false dichotomies. (3) Strongest counter-arguments — ONNX Runtime alternative, async queue contradicting ACID claim, Fashion-CLIP pre-selection bias.
- **Will particularly care about**: Scope-to-claim gap. Are practitioner-facing claims valid given limitations? Would a practitioner trust CPU-only results from a laptop?
- **Possible blind spots**: Skepticism calibrated for PhD theses; may be excessively harsh for bachelor's level. May reject DSR contributions entirely. May overlook that engineering integration is legitimate software engineering contribution.

---

## 4. Review Strategy Recommendations

1. **Writing Polish for Defense (PRIMARY CONTEXT)**: All five reviewers evaluate with understanding that audience is a defense committee assessing undergraduate competence.
2. **DSR Argument Must Be Persuasive**: Merely naming DSR doesn't satisfy it. Each design decision should derive from problem analysis.
3. **Benchmark Chapter Is Load-Bearing**: Chapter 2 describes what was built; Chapter 3 provides evidence it works. If Chapter 3's methodology is thin, entire thesis weakens.
4. **Scope Honesty Is a Strength**: Thesis is unusually explicit about limitations. Claims must never exceed scope.
5. **Writing Quality Focus**: Redundancy elimination, narrative flow through Background, terminology consistency, grammar and academic style, diagram completeness.

---

## 5. Reviewer Assignment Matrix

| Role | Identity | Primary Lens | Defense-Relevant Question |
|------|----------|-------------|--------------------------|
| EIC | Dr. Elena Vasquez, SoftwareX / Amsterdam | Overall quality, originality, writing | "Is this a coherent, well-argued document demonstrating undergraduate competence?" |
| R1 | Prof. Markus Lindgren, Chalmers | DSR rigor, benchmark protocol, technical clarity | "Is DSR properly executed, and are empirical claims supported by valid methodology?" |
| R2 | Dr. Shreya Kapoor, Amazon Visual Search | Literature coverage, gap positioning, model comparison | "Does this thesis correctly position itself in fashion AI literature?" |
| R3 | Michael Torres, Shopify / pgvector | Deployability, reproducibility, open-source value | "Can a practitioner actually use this?" |
| DA | Prof. Arthur Kowalski, TU Dresden | Core argument challenge, logical fallacies | "What is the minimum defensible contribution, stripped of padding?" |

---

*Report complete. Ready for Phase 1 reviewer dispatch.*
