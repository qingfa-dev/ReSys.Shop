# Thesis Rewrite — Part 3: Conclusion and Future Work

Five edits here. One of them (the "eleven models" fix) is safe to apply now. The others in this file are safe too, they're all self-contained corrections that don't depend on the unresolved Chapter 3 benchmark question. I'll flag the one exception clearly where it comes up.

All edits trace back to `thesis-review-part3-conclusion.md` and the master fix list.

---

## Edit 1 — "Achievement of Technical Objectives," eleven architectures (p.120)

**BEFORE:**
> Benchmark evaluation produced empirical accuracy and efficiency metrics across four models and **eleven supported architectures**.

**AFTER:**
> Benchmark evaluation produced empirical accuracy and efficiency metrics across four models, **selected from six supported by the framework**.

**Why:** same correction applied throughout the thesis, Table 55's actual registry has six models, not eleven. Safe to apply now, this doesn't depend on the Chapter 3 numbers question.

---

## Edit 2 — II. Contributions, eleven models and metric-count mismatch (p.121)

**BEFORE:**
> A four-model benchmark for fashion image retrieval. Systematic evaluation with **seven accuracy** and five efficiency metrics across four architecture families, **eleven models supported**, 3-fold cross-validation protocol.

**AFTER:**
> A four-model benchmark for fashion image retrieval. Systematic evaluation across three accuracy metric families (mAP, P@K, R@K, reported at three depths for seven total columns) and five efficiency metrics, spanning four architecture families and **six supported models**, 3-fold cross-validation protocol.

**Why:** two separate fixes bundled into one sentence. "Eleven" becomes "six" for the same reason as Edit 1. The accuracy-metric count also needed clarifying: the thesis states this as "five" in §3.4.2, as three distinct metric types in Table 65, and as "seven" here and in Table 70, three different numbers for the same thing. Seven is defensible if you're counting each reported column (mAP + P@5 + P@10 + P@20 + R@5 + R@10 + R@20), so I kept seven here but made explicit *why* it's seven, which resolves the apparent conflict with Table 65 rather than just picking one number and hoping the reader doesn't notice the others.

**Also needed:** once you've decided on this "three families, seven columns" framing, go back to Chapter 3 §3.4.2 and change "Five accuracy and five efficiency metrics were measured per model" to match, so all three locations (§3.4.2, this Contributions bullet, and Table 70) say the same thing.

---

## Edit 3 — III. Limitations, "near-zero P@20" is contradicted by the appendix's own data (p.121)

**BEFORE:**
> The enriched-label evaluation produces **near-zero P@20 values** due to the finer-grained relevance criterion.

**AFTER:**
> The enriched-label evaluation produces **substantially lower P@20 values, dropping from approximately 0.90 under category-only labels to approximately 0.30 under category-plus-colour-plus-pattern labels**, due to the finer-grained relevance criterion.

**Why:** Appendix A.2 and A.3 report Fashion-CLIP P@20 at 0.3510 and 0.2997 respectively, and all four models sit in the same 0.28–0.35 range under both enriched schemes, nowhere near zero. The rewrite states the real, verifiable effect (a large drop, not a collapse to zero), which is actually a more informative limitation than the original overstated version.

**Note on dependency:** this edit is safe to apply now regardless of how the bigger Chapter 3 vs. Appendix A discrepancy resolves, since the 0.90 → 0.30 drop pattern is consistent across every version of the appendix data I checked. If the appendix numbers do get regenerated, just confirm the approximate 0.90/0.30 figures still hold before finalizing this sentence.

---

## Edit 4 — Table 70, two confirmed mis-citations (p.122–123)

**BEFORE (row 1):**
> Validate pgvector feasibility for real-time similarity search | Chapter 2, **Section 2.2.4**, Section 2.3.3 | IVFFlat cosine similarity queries under 10 ms (2.7–6.5 ms); vectors share PostgreSQL database with relational data.

**AFTER (row 1):**
> Validate pgvector feasibility for real-time similarity search | Chapter 2, **Section 2.3.4**, Section 2.4.3 | IVFFlat cosine similarity queries under 10 ms (2.7–6.5 ms); vectors share PostgreSQL database with relational data.

**BEFORE (row 2):**
> Set up vector search | Chapter 2, **Section 2.2.4**, Section 2.3.3 | pgvector with cosine similarity; IVFFlat queries under 10 ms; vector storage coexists with relational data.

**AFTER (row 2):**
> Set up vector search | Chapter 2, **Section 2.3.4**, Section 2.4.4 | pgvector with cosine similarity; IVFFlat queries under 10 ms; vector storage coexists with relational data.

**BEFORE (row 3):**
> RQ3: Sidecar architecture viability for real-time search | Chapter 2, Sections 2.3.2–2.3.3; Chapter 3, **Section 3.5** | End-to-end latency under one second; independent scaling and fault isolation without distributed overhead.

**AFTER (row 3):**
> RQ3: Sidecar architecture viability for real-time search | Chapter 2, Sections 2.3.2–2.3.3; Chapter 3, **Section 3.7.4** | End-to-end latency under one second; independent scaling and fault isolation without distributed overhead.

**Why:** §2.2.4 is "Customer Use Cases," not where pgvector setup or vector search implementation is documented, that content is actually in §2.3.4 (Database Design) and §2.4.3–2.4.4 (Data Persistence, ML Sidecar). And RQ3's actual "Answer to RQ3" appears in §3.7.4, not §3.5 (which answers RQ1). These are the two I could confirm directly; I'd suggest checking the other 8 rows in Table 70 the same way, since I only spot-checked these two and didn't audit every citation, and a traceability table's whole purpose is undermined if its own citations don't hold up.

---

## What depends on the Chapter 3 decision (do these after, not now)

**I. Summary of Work and the RQ1–RQ3 answers** already use the correct 5.4%/7.7%/8.2%/92.8%/26.0% figures, matching Table 67/68. These don't need editing *unless* the Chapter 3 rewrite's decision framework results in different underlying numbers, in which case this section (and the corresponding rows in Table 70) will need updating together with Chapter 3's figures. I'd hold off touching these until that's resolved, so you're not editing the same numbers twice.

---

## What wasn't touched

IV. Future Work read as genuinely considered during review (each direction ties to a specific limitation) and needed no changes. The closing paragraph after Table 70 also held up fine.

---

Ready for the References list next whenever you want to continue, that one's mostly done already (two fabricated citations and one incomplete author list were found and given corrected entries in the earlier review), so it should be a quick pass to turn into the same rewrite format.
