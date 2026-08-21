# Revision Roadmap (prioritized)

> Generated from the 2026-08-21 fresh review. Maps each finding to a concrete fix in the `.typ` source. After edits, run `typst compile main.typ` to rebuild `main.pdf`.

## TIER 1 — Must fix (closes the 3 CRITICALs; blocks acceptance)

### R1. Reconcile the model-count story (DA-C2, M4, D2)
Define three explicit, nested tiers **once** and sweep every mention:
- **Candidate registry = 11** (Table 5, §1.3.4.1 / §1.5.9) — keep as "considered/evaluable candidate models."
- **Runtime-loadable registry = 6** (Table 55, §2.4.4.1) — keep.
- **Empirically benchmarked in Ch3 = 4** (abstract, §3.4.1, Part 3).
Files:
- `chapters/part2/ch1-background/f4/04-model-selection.typ` → change "Eleven pre-trained models … were **evaluated**" to "… were **considered as candidates**; four were empirically benchmarked in Chapter 3."
- `chapters/part2/ch1-background/f3/10-benchmark-framework.typ` → §1.5.9: "11 architectures" → "11 candidate architectures (6 loadable in the running service; 4 benchmarked)."
- `chapters/part2/ch1-background/f6/03-contributions.typ` → line 2158 "evaluates six models" → "benchmarks four of six supported models."
- `chapters/part2/ch2-design/04-implementation/02-ml-pipeline.typ` → Table 55 caption consistent with "6 loadable."
- `chapters/part3/ch4-conclusion.typ` → "four models and six supported architectures" — already consistent; verify.

### R2. Fix DINOv2 in Table 64 / §3.4.1 (DA-C2, M1, D3)
Table 64 (§3.4.1) lists **5 rows** but says "Four representative models" and DINOv2 has **no results**.
Files: `chapters/part2/ch3-evaluation/04-benchmark-protocol.typ`
- **Option A (preferred):** remove DINOv2 from Table 64 and from the "evaluated" set; change "Four representative models spanning four architectural families" → "Four representative models spanning three architectural families (CNN, CLIP, domain-tuned)." (Keep DINOv2 only as a candidate in Table 5 / §1.3.2.)
- **Option B:** add DINOv2 results to Table 67/68, Fig 88/89/90/91, and Appendix A. (Heavier — needs raw numbers.) Choose A unless data exists.

### R3. Soften the statistical-significance claim (DA-C3, M2)
File: `chapters/part2/ch3-evaluation/07-model-comparison.typ` (§3.7.3)
- Delete: *"Fashion-CLIP's mean mAP exceeds the upper 95% confidence bound of every other model, confirming statistically robust top-tier separation."*
- Replace with: *"Fashion-CLIP achieved the highest mean mAP in all three folds (Δ vs CLIP-generic = 2.13% relative, SD ±0.0068 vs ±0.0077), indicating consistent top-tier ranking; formal significance testing is left to future work."*
- Do **not** assert confidence bounds you did not compute. (If raw per-query data exists, optionally add a paired bootstrap and report the interval — then the original claim may be restored with evidence.)

### R4. Reframe "Recommendation" scope (DA-C1, D1)
No recommender exists. Do **not** change the approved CTU title page; instead make the scope honest:
Files:
- `chapters/part1/ch1-introduction.typ` (Scope §IV, Objectives §III): replace standalone "embedding-based recommendations" promise with *"visual-search-driven similarity (used as the sole personalization/recommendation mechanism in this work — no separate collaborative-filtering recommender is implemented)."*
- `chapters/part2/ch1-background/f6/03-contributions.typ` (Contribution Differentiators): add one sentence clarifying recommendation = CBIR similarity.
- `chapters/part3/ch4-conclusion.typ` (Contributions/Limitations): note recommendation-as-similarity explicitly; add "implement a dedicated recommender" to Future Work.
- Optional (stronger): implement a lightweight "Similar / Recommended for you" panel reusing the CBIR vector query and describe it in §2.4.4 / Ch3. (Only if you want to keep the title promise literally.)

## TIER 2 — Should fix (credibility / honesty)

### R5. Clarify mAP definition (M3)
File: `chapters/part2/ch3-evaluation/04-benchmark-protocol.typ` (Table 65)
- Change "Mean average precision over top-20 results" → "Query-averaged Mean Average Precision, computed over the top-20 ranked results (AP per query, then mean across queries)."
- Add a one-line caveat: coarse category-only relevance makes mAP a *relative* comparator, not an absolute quality certificate; reference Appendix A.2/A.3.

### R6. Reword RQ3 "achieved" → "supported by design" (P1)
File: `chapters/part2/ch3-evaluation/07-model-comparison.typ` (RQ3 answer) + `chapters/part3/ch4-conclusion.typ`
- Replace "independent scaling and fault isolation were achieved" with "the design supports independent scaling and fault isolation; interactive single-query latency was confirmed, with load behaviour left to future work." (Or add a small k6/vegeta load test and keep the claim.)

### R7. Reconcile latency target (P2)
File: `chapters/part2/ch1-background/f4/04-model-selection.typ` (§1.3.4.3) + `chapters/part2/ch3-evaluation/07-model-comparison.typ` (§3.7.4)
- Pick one objective: state "<1 s interactive end-to-end" as the goal; remove the conflicting "sub-300 ms total response time" or relabel it as "inference-stage budget."

## TIER 3 — Optional polish
- R8 (M5/D5): add a short paragraph in §3.7.3 acknowledging R@20 ≈ 0.07–0.08 and that same-category ≠ visually-similar.
- R9 (M6): footnote the benchmark framework location / split definition for reproducibility.
- R10 (D6/P3): in Future Work, add hybrid text+image query evaluation (touted in §1.3.3.4 but never run) and the recommendation capability.
- R11 (D4): reframe Contribution 1 from "A four-model benchmark" (sounds like a knowledge contribution) to "An empirical illustration of off-the-shelf model trade-offs on commodity hardware."

## Verification before re-review (pass 2)
1. `grep -rniE "eleven|11 models|11 architectures|six models|evaluates six"` across `chapters/` → all consistent with the 11/6/4 tiering.
2. No "DINOv2" row in Table 64 unless results exist; "Four representative models" matches the 4 result tables.
3. No "95% confidence bound" / "statistically robust" without a reported test.
4. Scope/Objectives/Conclusion no longer promise a standalone recommender.
5. `typst compile main.typ` succeeds; `main.pdf` rebuilt.
