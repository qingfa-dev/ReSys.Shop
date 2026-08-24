# Peer Review Report — Methodology Reviewer

## Manuscript Information
- **Title**: Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
- **Review Date**: 2026-08-22
- **Review Round**: Fresh full panel review (post-revision)

---

## Reviewer Information

### Reviewer Role
Peer Reviewer 1 (Methodology)

### Reviewer Identity
Assistant Professor of Computer Vision and Information Retrieval, specializing in visual search evaluation methodology.

### Review Focus
Benchmark protocol rigor, statistical validity, reproducibility, metric definitions.

---

## Overall Assessment

### Recommendation
- [ ] Accept
- [x] **Minor Revision** — Minor revisions needed, no re-review after revision
- [ ] Major Revision
- [ ] Reject

### Confidence Score
5

### Summary Assessment

This thesis benchmarks six pre-trained deep learning models for fashion CBIR on 5,000 images using 3-fold stratified cross-validation. The evaluation protocol is sound for a capstone: it uses standard metrics (mAP, P@K, R@K), reports means and standard deviations, and includes a novel ground-truth sensitivity analysis.

The statistical claims have been appropriately softened from the prior version — the "95% confidence bound" assertion has been replaced with an honest "non-overlapping-bounds heuristic" discussion. The methodology is internally consistent and reproducible given the documented hardware constraints.

The main methodological limitation is the 3-fold CV design: with only 3 folds, statistical power is limited and the standard deviations may be unstable. However, this is appropriate for a capstone thesis with a 5,000-image dataset.

---

## Strengths

### S1: Ground-Truth Sensitivity Analysis
The three-way ground-truth comparison (category-only, category+colour, category+colour+pattern) is a genuine methodological contribution. It reveals that DINOv2 collapses under fine-grained labels while CLIP-family models remain robust — a finding that would be invisible under category-only evaluation.
**Evidence Anchor**: table: Table 69 — DINOv2 drops from 0.9299 (category-only) to 0.1899 (category+colour), while Fashion-CLIP drops from 0.9336 to 0.2439

### S2: Honest Statistical Reporting
The revised manuscript correctly identifies the "non-overlapping-bounds heuristic" and acknowledges that "four of the six models therefore form a statistically indistinguishable cluster." This is honest and appropriate for 3-fold CV.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.5 — "With only 3 folds, formal significance testing has limited power"

### S3: Comprehensive Metric Suite
Three accuracy families (mAP, P@K, R@K) at three depths (K=5, 10, 20) plus five efficiency metrics provide a complete evaluation picture. The storage scaling analysis (embedding dimensionality × catalog size) is practical.
**Evidence Anchor**: table: Tables 66–68 — seven accuracy columns and five efficiency metrics per model

---

## Weaknesses

### W1: 3-Fold CV Limits Statistical Power
**Problem**: With only 3 folds, standard deviations are estimated with 2 degrees of freedom. The reported SDs (e.g., ±0.0060 for Fashion-CLIP) may be unstable. A 5-fold or 10-fold design would provide more reliable estimates.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.4 — "3-fold stratified cross-validation preserving category distribution"
**Why it matters**: The 0.40% mAP gap between Fashion-CLIP and DINOv2 may not be distinguishable given the SD instability.
**Suggestion**: Acknowledge this limitation more explicitly. The thesis already notes "limited power" but could quantify it: "With n=3 folds, the minimum detectable difference at α=0.05 with 80% power is approximately X%."
**Severity**: Major
**Confidence**: 5 — core expertise: statistical evaluation methodology

### W2: RAM Measurement Unreliable
**Problem**: The thesis reports RAM as "N/A" due to psutil measurement issues on the Linux kernel. This means one of the five efficiency metrics is missing for all models.
**Evidence Anchor**: text: Part 2 Chapter 3, §3.6, Table 67 — all RAM values are "N/A"
**Why it matters**: Memory consumption is a critical deployment constraint, especially for transformer models.
**Suggestion**: Use an alternative measurement method (e.g., `/proc/self/status` VmRSS, `memory_profiler` Python package, or manual `htop` snapshots). Even approximate numbers would be more useful than dashes.
**Severity**: Major
**Confidence**: 4 — adjacent field: system measurement methodology

### W3: mAP Definition Could Be More Precise
**Problem**: Table 65 defines mAP as "Mean average precision over top-20 results." The precise computation is: AP per query (precision at each relevant result position, averaged), then mean across queries. The current definition conflates "mean" (across queries) with "average" (precision computation).
**Evidence Anchor**: table: Table 65 — "Mean average precision over top-20 results; primary accuracy metric"
**Why it matters**: Precision in metric definitions ensures reproducibility.
**Suggestion**: Change to "Query-averaged Mean Average Precision, computed over the top-20 ranked results (AP per query, then mean across queries)."
**Severity**: Minor
**Confidence**: 5 — core expertise: information retrieval metric definitions

---

## Detailed Comments

### Methodology / Research Design
- The DSR methodology is appropriate and correctly applied across four phases.
- The benchmark protocol is well-documented: hardware, software versions, preprocessing (224×224, ImageNet normalization).
- The use of exact cosine search for accuracy metrics (isolating model quality from index effects) is methodologically correct.

### Results / Findings
- The "non-overlapping-bounds heuristic" is the right tool for 3-fold CV without formal testing.
- The ground-truth sensitivity analysis is the strongest methodological contribution.
- The storage scaling analysis (§3.6) is practical and underreported in similar work.

### Statistical Reporting
- Means and SDs are reported for all metrics — good.
- The absence of confidence intervals or significance tests is appropriate for 3-fold CV.
- The thesis correctly identifies the "statistically indistinguishable top tier" — honest and important.

---

## Questions for Authors

1. Could you report the minimum detectable effect size for your 3-fold CV design at α=0.05, power=0.80? This would quantify the "limited power" statement.
2. Did you consider using McNemar's test or paired bootstrap for pairwise model comparisons? Even with 3 folds, a permutation test could provide more insight.
3. What is the variance of the fold splits? Were the folds balanced in terms of category distribution?

---

## Dimension Scores

| Dimension | Score (0-100) | Descriptor | Notes |
|-----------|--------------|------------|-------|
| Originality (20%) | 40 | Weak | Confirmatory; ground-truth analysis is novel |
| Methodological Rigor (25%) | 68 | Adequate | Sound protocol; 3-fold CV limits power |
| Evidence Sufficiency (25%) | 72 | Adequate | Comprehensive metrics; RAM missing |
| Argument Coherence (15%) | 70 | Strong | Clear RQ-answer mapping |
| Writing Quality (15%) | 70 | Strong | Well-organized |
| **Weighted Average** | **63** | **Minor Revision** | Methodology sound for capstone; minor improvements possible |
