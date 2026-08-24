# Peer Review Report — Domain Reviewer

## Manuscript Information
- **Title**: Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
- **Review Date**: 2026-08-22
- **Review Round**: Fresh full panel review (post-revision)

---

## Reviewer Information

### Reviewer Role
Peer Reviewer 2 (Domain)

### Reviewer Identity
Associate Professor of Information Systems, specializing in e-commerce platform architecture and recommendation systems.

### Review Focus
Literature coverage, theoretical framework, domain contribution, positioning against prior work.

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

This thesis addresses a genuine gap between ML research (Fashion-CLIP, DINOv2, etc.) and production e-commerce systems. The literature review covers the core CBIR and fashion-IR references (DeepFashion, FashionIQ, Fashion-CLIP) and positions the work as an engineering integration contribution.

The related work section is adequate for a capstone but could be strengthened by citing more recent (2023–2025) visual search systems. The DSR methodology is correctly applied. The contribution is honest: a reference implementation with empirical validation, not a novel algorithm.

The title still promises "Recommendation" but the system delivers CBIR similarity. The revised text clarifies this, but the title remains.

---

## Strengths

### S1: Honest Positioning Against Prior Work
The thesis clearly distinguishes itself from DeepFashion (dataset), FashionIQ (conversational retrieval), and Fashion-CLIP (domain-specific model). It positions as an engineering integration, not a research contribution.
**Evidence Anchor**: text: Part 2 Chapter 1, §1.6.1 — "This thesis follows that approach, using pre-trained models without custom training, and extends the evaluation to additional architectures"

### S2: Practical Technology Stack Documentation
The technology stack (.NET 10, Python 3.12, Vue 3, PostgreSQL pgvector) is documented with specific versions and configurations. This enables reproducibility.
**Evidence Anchor**: text: Part 1, §1.4.2 — "Backend: .NET 10 with Carter, MediatR, FluentValidation. AI Service: Python 3.12 with FastAPI, PyTorch, Hugging Face Transformers"

### S3: Reference Implementation Value
The pluggable model architecture and pgvector ACID-compliant vector storage are genuine contributions for teams integrating Python ML into .NET e-commerce stacks.
**Evidence Anchor**: text: Part 3, §4.2 — "A validated polyglot architecture pattern for .NET and Python AI"

---

## Weaknesses

### W1: Limited Literature on Recent Fashion Visual Search (2023–2025)
**Problem**: The related work cites DeepFashion (2016), FashionIQ (2021), and Fashion-CLIP (2022) but misses more recent work on visual search for e-commerce (e.g., Google's multi-modal search, Amazon's visual search, Pinterest Lens improvements).
**Evidence Anchor**: absence: Part 2 Chapter 1, §1.6 — expected coverage of 2023–2025 visual search systems; checked §1.6.1–1.6.2
**Why it matters**: The field moves fast. A 2026 thesis should acknowledge the latest commercial and academic systems.
**Suggestion**: Add 2–3 references to recent (2023–2025) visual search systems, even if only to note they are out of scope.
**Severity**: Minor
**Confidence**: 4 — adjacent field: literature review standards

### W2: "Recommendation" Framing
**Problem**: The title promises "Recommendation" but no collaborative filtering, user-based, or item-based recommendation is implemented. The system delivers visual similarity search, which is related but distinct.
**Evidence Anchor**: text: Title — "RECOMMENDATION AND IMAGE-BASED PRODUCT SEARCH"
**Why it matters**: Recommendation systems and visual search are different research areas with different evaluation criteria.
**Suggestion**: Either implement a lightweight recommender or change the title to focus on visual search.
**Severity**: Major
**Confidence**: 4 — adjacent field: recommendation system taxonomy

### W3: Dataset Limitation Not Fully Explored
**Problem**: The 5,000-image Fashion Product Images Dataset is acknowledged as a limitation, but the thesis does not discuss how the dataset's characteristics (single-platform origin, English-language metadata, specific photography style) might bias the results.
**Evidence Anchor**: text: Part 3, §4.3 — "The benchmark uses 5,000 product images from a single dataset; results may not generalise to other markets"
**Why it matters**: Dataset bias could affect the generalizability of the model rankings.
**Suggestion**: Add a brief discussion of dataset characteristics (e.g., "The dataset originates from a single Indian e-commerce platform with a specific photography convention; results on Western or East Asian fashion platforms may differ.").
**Severity**: Minor
**Confidence**: 3 — partially within expertise

---

## Detailed Comments

### Literature Review / Theoretical Framework
- Core CBIR concepts (§1.2) are well-covered.
- ML model families (§1.3) are explained clearly (CNN, ViT, CLIP).
- Vector databases (§1.4) coverage is adequate.
- Related work (§1.6) positions against key references but could be more comprehensive.

### Methodology
- DSR is the right methodology for a system-building thesis.
- The four-phase DSR structure (Research and Planning → Design → Implementation → Testing and Evaluation) is correctly applied.

### Results / Findings
- The model comparison is systematic and well-presented.
- The ground-truth sensitivity analysis is the most interesting finding.
- The deployment recommendations are practical.

### Conclusion
- Contributions are listed clearly (five contributions in §4.2).
- Limitations are honest and specific.
- Future work directions are motivated.

---

## Questions for Authors

1. How does the Fashion Product Images Dataset compare to DeepFashion or FashionIQ in terms of diversity, quality, and annotation richness?
2. Would the model rankings change on a Western fashion dataset (e.g., DeepFashion2)?
3. The thesis mentions "embedding-based recommendations" in Scope (§1.3) — is this intentional or a leftover from the pre-revision version?

---

## Dimension Scores

| Dimension | Score (0-100) | Descriptor | Notes |
|-----------|--------------|------------|-------|
| Originality (20%) | 48 | Weak | Engineering integration; confirmatory benchmark |
| Methodological Rigor (25%) | 65 | Adequate | Sound DSR; 3-fold CV appropriate |
| Evidence Sufficiency (25%) | 68 | Adequate | Comprehensive but dataset limited |
| Argument Coherence (15%) | 72 | Strong | Clear positioning and RQ structure |
| Writing Quality (15%) | 70 | Strong | Well-organized |
| **Weighted Average** | **62** | **Minor Revision** | Adequate capstone; minor literature and scope improvements |
