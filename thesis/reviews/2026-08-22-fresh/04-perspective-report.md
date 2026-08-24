# Peer Review Report — Perspective Reviewer

## Manuscript Information
- **Title**: Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
- **Review Date**: 2026-08-22
- **Review Round**: Fresh full panel review (post-revision)

---

## Reviewer Information

### Reviewer Role
Peer Reviewer 3 (Perspective)

### Reviewer Identity
Researcher in AI Ethics and Responsible Innovation, with a background in Information Science.

### Review Focus
Cross-disciplinary connections, claim calibration, practical impact, title-scope alignment.

---

## Overall Assessment

### Recommendation
- [ ] Accept
- [x] **Minor Revision** — Minor revisions needed, no re-review after revision
- [ ] Major Revision
- [ ] Reject

### Confidence Score
3

### Summary Assessment

This thesis is an honest, well-structured engineering capstone that demonstrates integrating pre-trained vision models into a .NET/Python e-commerce stack. The work is positioned as a reference implementation, not a scientific discovery — and this framing is appropriate.

From a cross-disciplinary perspective, the main issue is the title-scope mismatch: "Recommendation" promises more than the system delivers. The revised text has clarified this (Scope, Conclusion), but the title remains unchanged. This is a framing issue, not a technical one.

The thesis is a solid capstone that provides practical value for software teams integrating Python ML into .NET applications. It is not a contribution to the fashion-IR research community, and it does not claim to be (after revision).

---

## Strengths

### S1: Practical Accessibility
The thesis demonstrates that production-quality visual search is achievable on commodity hardware (Intel i7, 16 GB RAM, CPU-only). This lowers the barrier for small-to-medium e-commerce platforms.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.7.4 — "Commodity CPU hardware is enough. Even CLIP models complete inference within 240 ms"

### S2: Honest Claim Calibration
The revised manuscript honestly scopes its contribution: "reference implementation for systematic comparison of embedding models in the fashion domain" (Abstract). The limitations section is thorough and specific.
**Evidence Anchor**: text: Abstract — "The thesis provides a reference implementation for systematic comparison of embedding models in the fashion domain"

### S3: Reusable Architecture Patterns
The strategy-pattern Model Manager and pgvector ACID-compliant storage are genuinely reusable patterns for other teams.
**Evidence Anchor**: text: Part 3, §4.2 — "A pluggable model architecture enabling runtime model switching"

---

## Weaknesses

### W1: Title Still Promises "Recommendation"
**Problem**: The title promises "Recommendation and Image-Based Product Search" but no recommender system is implemented. The revised text clarifies this, but the title itself remains unchanged.
**Evidence Anchor**: text: Title page — "BUILDING A FASHION E-COMMERCE APPLICATION WITH RECOMMENDATION AND IMAGE-BASED PRODUCT SEARCH"
**Why it matters**: A reader scanning titles would expect collaborative filtering or personalized recommendation. The system delivers visual similarity search, which is related but distinct.
**Suggestion**: Change the title to "Visual Search and Similarity-Based Product Discovery in Fashion E-Commerce" or similar.
**Severity**: Major
**Confidence**: 4 — adjacent field: title-scope alignment standards

### W2: No Discussion of Ethical Implications
**Problem**: The thesis does not discuss potential ethical implications of visual search in fashion e-commerce: algorithmic bias (e.g., skin tone, body type), privacy (uploading personal photos), or environmental impact (model training carbon footprint).
**Evidence Anchor**: absence: Part 3 — expected discussion of ethical considerations; checked §4.1–4.5
**Why it matters**: AI systems in fashion can perpetuate biases (e.g., recommending only certain body types or skin tones).
**Suggestion**: Add a brief "Ethical Considerations" subsection in Limitations or Future Work. Even one paragraph acknowledging these issues would be appropriate for a 2026 thesis.
**Severity**: Minor
**Confidence**: 3 — partially within expertise: AI ethics standards

### W3: Commercial Impact Not Quantified
**Problem**: The thesis claims practical value for e-commerce but provides no data on business impact (e.g., conversion rate lift, search abandonment reduction, customer satisfaction).
**Evidence Anchor**: absence: Part 3 — expected impact metrics; checked §4.1–4.5
**Why it matters**: Without business impact data, the "practical value" claim remains an assertion.
**Suggestion**: Acknowledge this in Future Work (it already mentions A/B testing as Future Work item 2, which is adequate).
**Severity**: Minor
**Confidence**: 3 — partially within expertise: e-commerce business metrics

---

## Detailed Comments

### Title & Abstract
- Title overpromises on "Recommendation."
- Abstract is accurate and well-written after revision.

### Introduction
- Problem statement is well-motivated.
- Research questions are specific and answerable.

### Literature Review
- Adequate for a capstone. Could benefit from 2–3 more recent references.

### Methodology
- DSR is appropriate.
- Benchmark protocol is sound.

### Results / Findings
- The ground-truth sensitivity analysis is the most interesting finding.
- Deployment recommendations are practical.

### Discussion
- Six summary findings are well-articulated.
- Limitations are honest.

### Conclusion
- Contributions are clearly listed.
- Future work is motivated by documented limitations.

---

## Questions for Authors

1. Has the thesis committee approved the "Recommendation" title given that no recommender is implemented?
2. Could the visual search system introduce algorithmic bias (e.g., recommending only certain styles, body types, or price ranges)?
3. What would it take to add a lightweight "Recommended for you" panel using the existing CBIR vectors?

---

## Dimension Scores

| Dimension | Score (0-100) | Descriptor | Notes |
|-----------|--------------|------------|-------|
| Originality (20%) | 50 | Weak | Engineering integration; practical but not novel |
| Methodological Rigor (25%) | 65 | Adequate | Sound protocol for capstone |
| Evidence Sufficiency (25%) | 68 | Adequate | Comprehensive; no business impact data |
| Argument Coherence (15%) | 75 | Strong | Clear framing after revision |
| Writing Quality (15%) | 72 | Strong | Well-written |
| **Weighted Average** | **64** | **Minor Revision** | Solid capstone; title and ethics improvements needed |
