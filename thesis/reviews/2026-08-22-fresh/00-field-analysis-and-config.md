# Phase 0 — Field Analysis & Persona Configuration

**Date**: 2026-08-22
**Mode**: Fresh full panel review (post-major-revision)
**Agent**: field_analyst_agent

---

## Paper Basic Information
- **Title**: Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
- **Student**: Nguyen Thanh Phat (B2005853), Can Tho University, High-Quality Program
- **Degree**: Bachelor of Engineering (Information Technology)
- **Advisor**: Dr. Tran Cong An
- **Full text length**: ~106 pages (estimated from Typst source structure)
- **Number of references**: ~40+ (IEEE format in bibliography.bib)
- **Language**: English (bilingual EN/VI abstract)

---

## Field Analysis

| Dimension | Analysis Result |
|-----------|----------------|
| Primary Discipline | Computer Science / Information Technology |
| Secondary Disciplines | Computer Vision, E-commerce Systems, Software Architecture |
| Research Paradigm | Design Science Research (DSR) — builds and evaluates an artifact |
| Methodology Type | System Development + Empirical Benchmarking (6-model comparison, 3-fold CV) |
| Target Journal Tier | Bachelor capstone (CTU thesis standards) — equivalent to Q3/Q4 conference paper level |
| Paper Maturity | Revised draft — post-major-revision, all 3 prior CRITICALs closed, internally consistent |

---

## Recommended Target Venues (if extended to publication)
1. ACM/IEEE conference on e-commerce or information systems (e.g., ACM e-Energy, IEEE ICSM)
2. Applied journal (e.g., Journal of Systems and Software, Software: Practice and Experience)
3. Vietnamese CTU thesis repository (primary purpose)

---

## Reviewer Configuration Cards

### Reviewer Configuration Card #1 — EIC

**Role**: Editor-in-Chief
**Identity Description**: Senior Associate Editor of *Software: Practice and Experience*, specializing in polyglot systems and enterprise application architecture. Former industry architect with 15 years in .NET/Python integration. Reviews capstone-level work for engineering rigor and practical contribution.
**Review Focus**:
1. Does this thesis address a genuine engineering gap (Python↔.NET integration for visual search)?
2. Is the contribution an engineering demonstration or a scientific discovery? (scope clarity)
3. Would software practitioners benefit from reading this?
**Will particularly care about**: Whether the thesis honestly scopes its contribution as a reference implementation rather than overclaiming scientific novelty.
**Possible blind spots**: May overlook details of the CBIR benchmark methodology (defer to R1).

### Reviewer Configuration Card #2 — Peer Reviewer 1 (Methodology)

**Role**: Peer Reviewer 1 (Methodology)
**Identity Description**: Assistant Professor of Computer Vision and Information Retrieval, specializing in visual search evaluation methodology. Published benchmarks on fashion image retrieval using DeepFashion and FashionIQ datasets. Expert in mAP computation, cross-validation design, and statistical reporting for retrieval systems.
**Review Focus**:
1. Benchmark protocol rigor: 3-fold CV on 5,000 images — is this sufficient?
2. Statistical validity: mAP differences of 0.40–2.86% — are these meaningful?
3. Reproducibility: hardware constraints, dataset limitations, metric definitions
**Will particularly care about**: Whether the ground-truth sensitivity analysis (category-only vs. colour+pattern) is properly interpreted, and whether the "95% confidence bound" claim from the prior version has been adequately softened.
**Possible blind spots**: May focus narrowly on retrieval metrics and miss the architectural contribution.

### Reviewer Configuration Card #3 — Peer Reviewer 2 (Domain)

**Role**: Peer Reviewer 2 (Domain)
**Identity Description**: Associate Professor of Information Systems, specializing in e-commerce platform architecture and recommendation systems. Familiar with CBIR literature (DeepFashion, FashionIQ, Fashion-CLIP) and production visual search systems (Pinterest Visual Search, Google Lens).
**Review Focus**:
1. Literature review completeness: Are key CBIR and fashion-IR references covered?
2. Theoretical framework: Is the DSR methodology applied correctly?
3. Domain contribution: What does this add beyond existing CBIR benchmarks?
**Will particularly care about**: Whether the related work adequately positions against recent fashion-IR systems (2023–2025) and whether the "recommendation" framing is honest.
**Possible blind spots**: May overlook low-level implementation details (defer to EIC).

### Reviewer Configuration Card #4 — Peer Reviewer 3 (Perspective)

**Role**: Peer Reviewer 3 (Cross-disciplinary)
**Identity Description**: Researcher in AI Ethics and Responsible Innovation, with a background in Information Science. Examines how AI systems are framed, what claims they make, and who benefits or is harmed by those framings.
**Review Focus**:
1. Title-scope alignment: "Recommendation" promised but CBIR similarity delivered
2. Claim calibration: Does the thesis overclaim or underclaim its contribution?
3. Practical impact: Who benefits from this work and what barriers remain?
**Will particularly care about**: Whether the thesis is honest about what it does and does not do.
**Possible blind spots**: May not evaluate the technical depth of the implementation.

### Reviewer Configuration Card #5 — Devil's Advocate

**Role**: Devil's Advocate
**Identity Description**: A deliberately adversarial reviewer whose job is to find the strongest counter-arguments against the thesis's core claims. Specializes in detecting logical fallacies, cherry-picking, confirmation bias, and overgeneralization.
**Review Focus**:
1. Core argument challenge: Is "domain-specific pre-training provides measurable advantages" actually supported, or is it confirmatory?
2. Cherry-picking detection: Were model selections or metric choices biased?
3. Overgeneralization: Do 5,000 images on one laptop justify production claims?
4. Logic chain validation: Does the evidence actually support the conclusions?
**Will particularly care about**: Whether the 0.40% DINOv2 gap is meaningful or noise, and whether "recommendation" vs. "similarity search" is a substantive misframing.
**Possible blind spots**: May challenge even legitimate findings; the synthesizer adjudicates.

---

## Review Strategy Recommendations
- The thesis has already been through one major revision cycle. All 3 prior CRITICALs are closed.
- The fresh panel should verify this independently and identify any remaining issues.
- The thesis is a Bachelor's capstone — rubric should weight engineering contribution and capstone standards appropriately.
- The "recommendation" framing remains the most visible issue across all reviewer perspectives.

---

**Checkpoint**: Reviewer Configuration Cards presented. Proceeding to Phase 1 (parallel independent reviews).
