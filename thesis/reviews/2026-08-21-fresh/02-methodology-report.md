# Reviewer 2 — Methodology Report

**Persona:** Information-retrieval / ML evaluation researcher.
**Paper:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Lens:** Benchmark rigor, metric validity, statistical soundness, reproducibility.

## Overall assessment
The evaluation is competently executed *for a capstone* and the previously reported Table 67 ↔ Appendix A number contradiction is now fixed (mAP 0.9309 agrees everywhere). However, several methodology issues materially weaken the evidential weight of the central claims. None require new experiments that are impossible; most require honest restatement or a small added analysis.

## Issues

### [MAJOR] M1 — DINOv2 listed as evaluated but absent from all results
- **Where:** §3.4.1 Table 64 (lines 7512–7533) enumerates **five** model rows — ResNet-50, EfficientNet-B0, DINOv2 ViT-S/14 ("framework-supported"), CLIP ViT-B/16, Fashion-CLIP — yet the prose says "Four representative models" and Figure 88 / Table 67 / Appendix A present **only four** (no DINOv2 row, no 384-dim numbers anywhere).
- **Why it matters:** Either DINOv2 was benchmarked and its results were dropped (a reporting omission that hides a whole architecture family, including the "structural fidelity" selling point of §1.3.2.4), or it was never run and Table 64 is wrong. Both are unacceptable as written.
- **Fix:** Either add the DINOv2 results row to Table 67/68/Fig 88/Appendix A and correct the "four" wording, or remove DINOv2 from Table 64 and from the "evaluated" claim. Pick one and make every mention consistent.

### [MAJOR] M2 — Unsupported statistical-significance claim
- **Where:** §3.7.3 (line 7977): *"Fashion-CLIP's mean mAP exceeds the upper 95% confidence bound of every other model, confirming statistically robust top-tier separation."*
- **Why it matters:** No confidence interval, p-value, or significance test (paired t-test, bootstrap, Wilcoxon) is reported anywhere. Only per-fold SD (±0.0068 etc.) is given, and cross-validation uses **n = 3 folds**. A "95% confidence bound" cannot be asserted from SD alone with three samples; with n=3 the interval would be enormous and would *not* support the strong claim made.
- **Fix:** Either (a) add a proper test (e.g., paired bootstrap or t-test across query results / folds) and report the actual interval, or (b) delete the significance language and state only the observed ordering with SDs ("Fashion-CLIP had the highest mean mAP in all three folds; differences vs CLIP-generic were 2.13% relative"). Do not claim significance you did not compute.

### [MAJOR] M3 — mAP definition ambiguous; metric choice weak for the claim
- **Where:** Table 65 (line 7547): *"mAP — Mean average precision over top-20 results; primary accuracy metric."* §3.4.3 says accuracy used "exact cosine search over all gallery embeddings."
- **Why it matters:** Standard mAP is averaged over **queries**. "Over top-20 results" is ambiguous (averaged over the 20 ranks? over queries?). More importantly, with a 5,000-image gallery and **binary category-only relevance**, P@20 ≈ 0.94 but **R@20 ≈ 0.07–0.08** (Table 67/71). The headline mAP 0.93 is driven by high *precision in a tiny top-K window*, not by retrieving most relevant items. A 0.93 mAP under these labels is not evidence of "production-quality" retrieval quality as claimed in §3.7.4.
- **Fix:** (a) Define mAP precisely (query-averaged AP, rank cutoff). (b) Report query-count and how AP is computed. (c) Temper the interpretation: the coarse category label makes mAP a *relative* comparator between models, not an absolute quality certificate. Consider also reporting a finer label scheme (Appendix A.2/A.3 already exist — surface those results in the main discussion).

### [MAJOR] M4 — "Evaluates six models" contradicts "four benchmarked"
- **Where:** line 2158 ("This thesis evaluates six models on consumer-grade hardware") vs abstract/§3.4.1 ("four representative models").
- **Fix:** Use "supports N models in the framework; benchmarks four" consistently, or correct the count. (See also EIC/Domain on the 11/6/4 tangle.)

### [MINOR] M5 — Recall interpretation not discussed
- R@20 of 0.07–0.08 is never discussed as a threat to the mAP narrative. A short paragraph acknowledging that same-category ≠ visually-similar (and that the coarse label caps recall) would pre-empt the Devil's-Advocate critique and strengthen honesty.

### [MINOR] M6 — Reproducibility artifact absent
- §1.5.9 claims benchmark outputs JSON/CSV/Markdown/Typst; Appendix C.3 is titled "Precision and Reproducibility." But no code repository, dataset split, or seed is referenced. For a thesis this is acceptable, but a single footnote with the framework location / split definition would materially improve reproducibility credibility.

### [MINOR] M7 — "Production" index uses IVFFlat at 65–72% recall@10
- §3.4.3: pgvector production benchmark used IVFFlat (100 lists) at 65–72% recall@10, while HNSW is "designated for larger scales" but never measured. Calling IVFFlat the "production benchmark" at <72% recall is weak; either benchmark HNSW or scope the production claim to "rapid-evaluation configuration."

## Metrics-validity verdict
The *relative* ranking (Fashion-CLIP > CLIP-generic > EfficientNet-B0 ≈ ResNet-50) is almost certainly real and directionally useful. The *absolute* numbers and the *significance* language are not defensible as written. Fix M1–M4 before any submission-style claim of "robust" or "production-viable."

| Dimension | Score |
|---|---|
| Benchmark rigor | 60 |
| Metric validity | 55 |
| Statistical soundness | 50 |
| Reproducibility | 62 |
