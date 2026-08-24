# Peer Review Report — EIC

## Manuscript Information
- **Title**: Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
- **Review Date**: 2026-08-22
- **Review Round**: Fresh full panel review (post-revision)

---

## Reviewer Information

### Reviewer Role
EIC (Editor-in-Chief)

### Reviewer Identity
Senior Associate Editor of *Software: Practice and Experience*, specializing in polyglot systems and enterprise application architecture.

### Review Focus
Journal fit, originality, significance, relevance to software practitioners.

---

## Overall Assessment

### Recommendation
- [ ] Accept
- [x] **Minor Revision** — Minor revisions needed, no re-review after revision
- [ ] Major Revision
- [ ] Reject

### Confidence Score
4

### Summary Assessment

This thesis presents a fashion e-commerce platform integrating Vue 3, .NET 10, and a Python ML sidecar for Content-Based Image Retrieval (CBIR). It benchmarks six pre-trained deep learning models on 5,000 fashion images using 3-fold cross-validation, reporting Fashion-CLIP as the top performer (mAP 0.9336). The work is positioned as an engineering demonstration rather than a scientific contribution.

The engineering integration — polyglot .NET↔Python sidecar, pgvector ACID-compliant vector storage, pluggable model architecture — is a solid, practical contribution that addresses a genuine gap between ML research and production e-commerce systems. The thesis is well-structured, clearly written, and demonstrates competence across the full stack.

However, the thesis still carries a minor scope drift: the title promises "Recommendation" but the system delivers visual-search-driven similarity. The revised manuscript (post-2026-08-21 review) has clarified this in the text, but the title itself remains unchanged. Additionally, the benchmark is confirmatory rather than novel — it validates known model rankings on a smaller dataset without new methodological insight.

---

## Strengths

### S1: Engineering Integration Quality
The polyglot sidecar architecture connecting .NET and Python ML is well-designed and cleanly implemented. The strategy-pattern Model Manager controlled via environment variable (`EMBEDDING_MODEL`) is a practical, reusable pattern. The pgvector ACID-compliant vector storage eliminates stale-index bugs.
**Evidence Anchor**: text: Part 2 Chapter 2, §2.3.2–2.3.4 — "Python FastAPI service with three-layer architecture, lazy-loading" and "pgvector with cosine similarity; IVFFlat queries under 10 ms"

### S2: Honest Limitations Discussion
The thesis is refreshingly honest about its limitations: 5,000-image dataset, CPU-only hardware, no user study, binary category-label ground truth, unreliable RAM measurement. The ground-truth sensitivity analysis (category-only vs. colour+colour+pattern) is a genuine methodological contribution that reveals model ranking instability.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.7.3 — "RAM measurement via psutil proved unreliable... actual consumption ranges from approximately 100 MB to over 600 MB"

### S3: Practical Deployment Guidance
The deployment recommendations are specific and actionable: Fashion-CLIP for quality, EfficientNet-B0 for speed, DINOv2 for lightweight coarse retrieval. The pluggable architecture enables A/B testing.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.7.2 — "For retrieval quality, Fashion-CLIP is recommended (mAP 0.9336)... For CPU-only or latency-sensitive deployments, EfficientNet-B0 is recommended"

---

## Weaknesses

### W1: Title-Scope Mismatch
**Problem**: The title promises "Recommendation and Image-Based Product Search" but no recommender system exists — only CBIR similarity search is implemented. The revised text clarifies this (Scope, Conclusion), but the title remains unchanged.
**Evidence Anchor**: text: Title page — "BUILDING A FASHION E-COMMERCE APPLICATION WITH RECOMMENDATION AND IMAGE-BASED PRODUCT SEARCH"
**Why it matters**: Readers expecting collaborative filtering or personalized recommendation will be disappointed. The title sets an unmet expectation.
**Suggestion**: Either (a) implement a lightweight "Recommended for you" panel reusing the CBIR vector query, or (b) change the title to "Visual Search and Similarity-Based Product Discovery" or similar.
**Severity**: Major
**Confidence**: 4 — adjacent field: applying general title-scope alignment standards

### W2: Benchmark is Confirmatory, Not Novel
**Problem**: The six-model comparison validates known rankings (Fashion-CLIP > DINOv2 > CLIP > CNN) on a smaller dataset (5,000 images) without new methodological insight. The ground-truth sensitivity analysis is interesting but not sufficient to elevate the benchmark to a novel contribution.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.5 — "the six-model comparison reveals a tightly packed top tier"
**Why it matters**: As a capstone, this is acceptable. As a publication contribution, the benchmark adds limited knowledge.
**Suggestion**: Reframe the contribution as "reference implementation with empirical validation" rather than "systematic benchmark." The thesis already does this in places (§1.6.3) but the title and some phrasing suggest broader claims.
**Severity**: Minor
**Confidence**: 5 — core expertise: software engineering research methodology

### W3: No User Evaluation
**Problem**: The evaluation is exclusively quantitative (mAP, latency, throughput). No user study measures whether CBIR actually improves shopping experience.
**Evidence Anchor**: absence: Part 2 Chapter 3 — expected a user study section; checked §3.1–3.7, Part 3 Limitations
**Why it matters**: The thesis claims practical value for e-commerce, but without user data, this remains an engineering assertion.
**Suggestion**: Add a note in Future Work acknowledging this gap (it already exists in Limitations, which is adequate).
**Severity**: Minor
**Confidence**: 4 — adjacent field: HCI evaluation standards

---

## Detailed Comments

### Title & Abstract
- The title overpromises on "Recommendation." The abstract is accurate and well-written.
- The Vietnamese abstract (TÓM TẮT) is a faithful translation.

### Introduction
- Problem statement is clear and well-motivated (§1.1: $770B fashion e-commerce, keyword search limitations).
- Research questions are specific and answerable (RQ1–RQ3).
- Scope and limitations are honest.

### Literature Review / Theoretical Framework
- Coverage is adequate for a capstone thesis. The CBIR section (§1.2) covers core concepts.
- Related work (§1.6) positions against DeepFashion, FashionIQ, Fashion-CLIP — appropriate references.
- Could benefit from more recent (2023–2025) visual search systems, but this is a capstone, not a survey.

### Methodology / Research Design
- DSR methodology is appropriate and correctly applied.
- 3-fold CV on 5,000 images is a reasonable capstone-scale evaluation.
- Hardware constraints are documented (CPU-only, consumer-grade).

### Results / Findings
- Tables are well-formatted (Tables 64–68, Figures 88–91).
- The ground-truth sensitivity analysis (Table 69) is the most interesting finding.
- The accuracy-efficiency trade-off analysis is practical and actionable.

### Discussion
- The six summary findings (§3.7.4) are well-articulated.
- Deployment recommendations are specific and honest.
- Limitations section is thorough.

### Conclusion
- Research questions are answered directly with specific numbers.
- Contributions are listed clearly (five contributions in §4.2).
- Future work directions are motivated by documented limitations.

### References
- IEEE format, ~40+ references. Adequate for a capstone.

---

## Questions for Authors

1. The title still promises "Recommendation" — has the committee approved this framing given that no recommender is implemented?
2. The DINOv2 gap (0.40% mAP over Fashion-CLIP) is within the noise of 3-fold CV. Would you characterize this as a meaningful difference or a tie?
3. How would the system perform with a GPU? The CPU-only constraint is documented, but would GPU inference change the deployment recommendations?

---

## Minor Issues

### Language / Grammar
- Generally well-written. A few minor awkward phrases but nothing requiring correction.

### Citation Format
- IEEE format consistently applied.

### Figures and Tables
- Figures are clear and well-labeled. Tables use consistent formatting.

### Layout
- CTU thesis format compliance appears correct.

---

## Dimension Scores

| Dimension | Score (0-100) | Descriptor | Notes |
|-----------|--------------|------------|-------|
| Originality (20%) | 45 | Weak | Confirmatory benchmark; genuine engineering integration |
| Methodological Rigor (25%) | 65 | Adequate | 3-fold CV appropriate for capstone; ground-truth analysis is good |
| Evidence Sufficiency (25%) | 70 | Adequate | Six models, three metric families, three ground-truth definitions |
| Argument Coherence (15%) | 75 | Strong | Clear RQ structure, honest limitations |
| Writing Quality (15%) | 72 | Strong | Well-organized, readable, minor polish needed |
| **Weighted Average** | **63** | **Minor Revision** | Acceptable capstone; minor scope alignment needed |
