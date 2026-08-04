# Editorial Decision Letter — Pass 3 (Evaluation)

**Thesis**: Building a Fashion E-Commerce Application with Image-Based Product Search and Model Benchmarking
**Student**: Nguyen Thanh Phat (B2005853), Can Tho University
**Review Panel**: EIC Dr. Elena Vasquez (SoftwareX), R1 Prof. Markus Lindgren (Chalmers), R3 Michael Torres (Shopify/pgvector), DA Prof. Arthur Kowalski (TU Dresden)
**Review Scope**: Pass 3 — Testing and Evaluation (Chapter 3, 269 lines)

---

## Decision

**MAJOR REVISION** — The experimental execution is competent, but the evaluation design conflates classification with retrieval, the central "Fashion-CLIP wins" claim is confounded by an unidentified baseline, and RQ3 is answered by assertion rather than evidence.

All four reviewers independently identify the same core problem: the category-level relevance criterion (5 broad categories) measures 5-way classification accuracy, not visual retrieval quality. Four models out of 11 evaluated, no baseline, statistical inference based on n=3 folds with no formal tests.

---

## Consensus Findings (3+ reviewers)

| # | Finding | Reviewers |
|---|---------|-----------|
| CF-1 | Category-level relevance conflates classification with retrieval — mAP scores are inflated by 2-3× | EIC, R1, R3, DA (all) |
| CF-2 | RQ3 ("architecture viability") is answered by assertion, not empirical data — no end-to-end latency measurement | EIC, R1, DA |
| CF-3 | "95% CI" from ±2SD with n=3 folds is statistically invalid (t-critical = 4.303, not 2.0) | EIC, R1, DA |
| CF-4 | "CLIP-generic" baseline is unidentified — ViT-B/32? ViT-B/16? ViT-L/14? Central claim unverifiable | R3, DA |
| CF-5 | No formal statistical tests: no paired t-test, Friedman, effect sizes, or correction for multiple comparisons | R1, DA |

---

## DA CRITICAL Adjudications

| ID | Finding | Adjudication |
|----|---------|-------------|
| C-1 | Relevance criterion conflates classification with retrieval — 5-way classification, not visual retrieval | **VALIDATED** — EIC, R1, R3 all independently confirm. The thesis acknowledges this in limitations but continues to frame mAP=0.8788 as "retrieval quality." |
| C-2 | Fashion-CLIP pre-selected to win — only domain-specific model on a fashion task, circular comparison | **VALIDATED** — R3 confirms: if CLIP-generic is ViT-B/32 while Fashion-CLIP is ViT-B/16, architecture confounds the domain-fine-tuning claim. No competitor was fine-tuned. |
| C-3 | Missing baselines make effect-size claims uninterpretable — no random, color histogram, or HOG baseline | **VALIDATED** — Without any baseline, the 0.8788 mAP is unanchored. A color histogram likely achieves mAP > 0.6 on 5 coarse categories. |
| C-4 | Deployment recommendations overfit to a single obsolete CPU — rankings invert on GPU | **VALIDATED** — R3 confirms transformer/CNN latency ratio inverts between CPU and GPU. The recommendation table is hardware-specific. |

---

## Revision Roadmap

### P0 — Must Fix Before Defense

| # | Item | Source |
|---|------|--------|
| P0.1 | Identify "CLIP-generic" baseline unambiguously — model ID, variant, checkpoint. If ViT-B/32 (different architecture from Fashion-CLIP ViT-B/16), qualify the domain-fine-tuning claim or add ViT-B/16 general CLIP | R3 CRITICAL, DA |
| P0.2 | Fix "five accuracy metrics" → "seven accuracy metrics" on line 12 of conclusion (see Pass 4 review) | EIC |
| P0.3 | Fix 5.4% vs 6.1% contradiction (line 147 vs line 258) — verify which number is correct | EIC |
| P0.4 | Add end-to-end latency measurement for RQ3, or explicitly state that RQ3 is answered by architecture (Ch.2) not benchmark data | EIC, R1, DA |
| P0.5 | Remove or fulfill screenshot placeholders | EIC |

### P1 — Strongly Recommended

| # | Item | Source |
|---|------|--------|
| P1.1 | Add at least one baseline (random, color histogram, or raw-pixel k-NN) to anchor mAP scores | DA, R1 |
| P1.2 | Replace "95% CI via ±2SD" with proper statistical treatment: report individual fold values, use t-distribution with df=2, or add Friedman+post-hoc | R1, DA |
| P1.3 | Report variance for P@K and R@K metrics (currently point estimates only) | R1, R3 |
| P1.4 | Document inference measurement methodology: warm-up iterations, batch size, thread config, framework versions | R3 |
| P1.5 | Add formal statistical test (paired t-test across folds, with Bonferroni correction) for model comparisons | R1 |
| P1.6 | Fix EfficientNet-B0 std dev (likely calculation error — 0.0007 is implausible with n=3) | R3, DA |
| P1.7 | Fix "~30 relevant items per query" claim — contradicted by P@K/R@K numbers (~140-170 implied) | R3 |
| P1.8 | Clarify throughput measurement methodology — single-threaded vs batched inconsistent across models | R3 |

### P2 — Nice to Have

| # | Item | Source |
|---|------|--------|
| P2.1 | Add random seed for fold splitting | R1 |
| P2.2 | Add software versions (PyTorch, torchvision, Python) | R1, R3 |
| P2.3 | Add nDCG or remove from chapter outline | R3 |
| P2.4 | Add model checkpoint hashes/URLs | R3 |
| P2.5 | Specify vector storage precision (halfvec vs vector) | R3 |
| P2.6 | Add RAM column footnote in efficiency table | R3 |

---

## Writing Polish Summary

**Strengths**: The 8-step protocol is crystal clear and reproducible (all reviewers). Metric definitions are precise. The limitations section is unusually honest and self-critical. The deployment recommendations table is practically useful.

**Issues**: The "CLIP-generic" ambiguity is a writing error with substantive consequences — it prevents verification of the central claim. Quantitative claims survive prose-level reading but fail cross-verification (5.4% vs 6.1%, ~30 vs ~140-170 relevant items, "95% CI" from n=3). Screenshot placeholders remain.

**Calibration**: For a B.Eng. thesis with CPU-only benchmarking, the execution is commendable. The evaluation design issues (coarse relevance, no baseline, unidentified competitor) are addressable within thesis scope by: (a) adding one baseline, (b) identifying CLIP-generic, (c) qualifying claims, and (d) running proper statistical tests.

---

*Synthesized from EIC (Dr. Elena Vasquez), R1-Methodology (Prof. Markus Lindgren), R3-Industry (Michael Torres), and DA (Prof. Arthur Kowalski). No new issues introduced beyond reviewer reports.*
