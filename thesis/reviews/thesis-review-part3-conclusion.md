# Thesis Review — Part 3: Conclusion and Future Work

**Scope of this file:** printed pages 120–123 (I. Summary of Work, II. Contributions, III. Limitations, IV. Future Work, V. Requirements Traceability). Because Part 3 leans heavily on Chapter 3's results and Table 70 explicitly traces back to Appendix A, I cross-checked several claims here directly against Appendix A (Tables 71–75) as well.

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. Appendix A's efficiency table doesn't match Chapter 3's either — this is bigger than just the accuracy numbers

**Location:** Table 74, Appendix A.4 (Operational Efficiency) vs. Table 68, §3.6 (Chapter 3)

The Chapter 3 review already flagged that Table 67 (accuracy) doesn't match Appendix A.1–A.3. Checking Table 74 against Chapter 3's Table 68 while reviewing Part 3 shows the **same problem exists for the efficiency numbers**, for the identical four models under what's claimed to be the identical benchmark:

| Model | Chapter 3 (Table 68) latency | Appendix A.4 (Table 74) latency | Chapter 3 throughput | Appendix A.4 throughput |
|---|---|---|---|---|
| EfficientNet-B0 | 23.9 ± 2.5 ms | 37.8 ± 26.6 ms | 33.2 img/s | 30.2 img/s |
| ResNet-50 | 64.0 ± 3.1 ms | 61.9 ± 5.8 ms | 12.9 img/s | 13.5 img/s |
| CLIP-generic | 92.9 ± 2.9 ms | 86.6 ± 8.4 ms | 19.9 img/s | 21.4 img/s |
| Fashion-CLIP | 92.0 ± 5.8 ms | 96.8 ± 6.8 ms | 18.0 img/s | 18.5 img/s |

Load times diverge even more sharply (e.g., ResNet-50: 286.1 ms in Chapter 3 vs. 374.1 ms in the appendix, a 31% difference). Storage (8.1 / 13.0 / 3.3 / 3.3 MB) is the *only* column that matches exactly between the two tables.

**Why this matters:** this is independent confirmation, from a completely different metric set, of the same underlying problem flagged in the Chapter 3 review: the numbers presented in the main chapter and the numbers presented in the appendix that's supposed to document them don't come from the same run. It's not just Table 67 vs. Appendix A.1, it's the accuracy tables *and* the efficiency tables. This raises the likelihood that Chapter 3's body text and Appendix A were generated from two different benchmark executions (possibly different code versions or different points in development) that were never reconciled before submission.

**Also worth noting:** Appendix A.4 reports actual RAM numbers (e.g., "EfficientNet-B0: 2.6 ± 22.3 MB," "CLIP-generic: 0.0 ± 0.0 MB"), while Chapter 3 explicitly states RAM was "reported as dashes" because the measurement was unreliable. The appendix's RAM column looks like exactly the kind of broken measurement Chapter 3 says was excluded (zero values, implausibly large SDs relative to the mean), so if anything this confirms Chapter 3's decision to omit it was the right call, but it means the appendix should probably show dashes too, for consistency, rather than presenting numbers Chapter 3 itself says aren't trustworthy.

**Fix:** This is the same underlying issue as the Chapter 3 review's top finding, and the fix is the same: figure out which run (main body or appendix) is authoritative, regenerate whichever one is wrong, and make sure every table across Chapter 3 and Appendix A comes from a single, consistent benchmark execution before the thesis is finalized.

---

## 🔴 CORRECT — 2. "Near-zero P@20" in the Limitations section is contradicted by the thesis's own appendix data

**Location:** III. Limitations (p.121)

> "The enriched-label evaluation produces near-zero P@20 values due to the finer-grained relevance criterion."

**Problem:** I checked this directly against Appendix A.2 and A.3, the two "enriched-label" (category+colour, and category+colour+pattern) schemes. Fashion-CLIP's P@20 is **0.3510** under A.2 and **0.2997** under A.3. CLIP-generic, EfficientNet-B0, and ResNet-50 all sit in the same 0.28–0.33 range under both schemes. None of these is "near-zero," they're all around 30%, which is exactly what you'd expect (lower than the 0.90+ P@20 under the category-only scheme, but far from zero). This looks like a claim written to sound like an expected consequence of stricter labeling, without checking it against the actual numbers the thesis itself reports two tables later.

**Fix:** Remove or correct this sentence. If you want to describe the real effect, it would be more accurate to say something like: "The enriched-label evaluation reduces P@20 substantially (from ~0.90 under category-only labels to ~0.30 under category+colour+pattern labels) due to the finer-grained relevance criterion," which is what your own Appendix A data actually shows.

---

## 🔴 CORRECT — 3. A third count for "accuracy metrics," on top of the two already in tension elsewhere

**Locations:**
- II. Contributions (p.121): *"Systematic evaluation with **seven accuracy** and five efficiency metrics..."*
- Table 70, last row (p.123): *"...seven accuracy and five efficiency metrics."*
- (for reference) Chapter 3, §3.4.2: *"**Five** accuracy and five efficiency metrics were measured per model."*
- (for reference) Chapter 3, Table 65: defines exactly **three** accuracy metric types (mAP, P@K, R@K).

**Problem:** the thesis now states three different counts for "accuracy metrics" depending on where you look: three (as distinct metric *types* in Table 65's definitions), five (§3.4.2's prose), and seven (both places in Part 3). Seven is actually defensible if you count each reported column separately (mAP + P@5 + P@10 + P@20 + R@5 + R@10 + R@20 = 7), and that's probably what Part 3 intends, but §3.4.2's "five" doesn't match either interpretation, and nowhere does the thesis explain that "seven" comes from counting individual K-values as separate metrics. A reader bounces between three different numbers for the same simple count.

**Fix:** Pick one consistent counting convention and use it everywhere, most likely "three accuracy metric families (mAP, P@K, R@K), evaluated at three depths (K=5,10,20) for seven total reported columns." Then fix §3.4.2's "five" to match.

---

## 🟠 REWRITE — 4. "Eleven supported architectures" appears twice more

**Locations:**
- I. Summary of Work, Achievement of Technical Objectives (p.120): *"Benchmark evaluation produced empirical accuracy and efficiency metrics across four models and **eleven supported architectures**."*
- II. Contributions (p.121): *"...across four architecture families, **eleven models supported**, 3-fold cross-validation protocol."*

**Problem:** as established in the Chapter 2 review, the actual model registry (Table 55, §2.4.4.1) lists exactly **six** models. "Eleven" doesn't correspond to any real artifact in the thesis. These two mentions in Part 3 are the conclusion's restatement of the same unresolved number that appears at least six times across the whole document now.

**Fix:** once the "eleven" vs. "six" question is resolved (see the Chapter 2 review), update these two mentions along with the others.

---

## 🟠 REWRITE — 5. Requirements Traceability table (Table 70) has section citations that don't line up

**Location:** Table 70, V. Requirements Traceability (p.122–123)

Two examples that don't check out when traced back:
- *"Validate pgvector feasibility... Addressed In: Chapter 2, **Section 2.2.4**, Section 2.3.3"* and *"Set up vector search... Addressed In: Chapter 2, **Section 2.2.4**, Section 2.3.3"* — §2.2.4 is "Customer Use Cases," not a place where pgvector setup or vector search implementation is actually documented. That material lives in §2.3.4 (Database Design, pgvector Integration) and §2.4.3–2.4.4 (Data Persistence, ML Sidecar). "2.2.4" looks like a stray digit, most likely meant to be "2.3.4" or "2.4.4."
- *"RQ3: Sidecar architecture viability... Addressed In: Chapter 2, Sections 2.3.2–2.3.3; Chapter 3, **Section 3.5**"* — the actual "Answer to RQ3" appears in §3.7.4, not §3.5 (§3.5 is Retrieval Performance, which answers RQ1). Same pattern for RQ1 and RQ2's citations to "Section 3.5," when their formal "Answer to RQ" text is also in §3.5 for RQ1 but §3.6 for RQ2, worth double-checking each row individually.

**Why this matters:** a traceability table's entire purpose is to let a reader verify claims by following the citations. If the citations themselves are off, the table undermines the thing it's trying to demonstrate (thesis completeness).

**Fix:** Go through all eleven rows of Table 70 and verify each "Addressed In" citation actually points to the section containing that finding. Given the number of section-numbering issues already found elsewhere in the thesis (the phantom "Section 2.1.5," the "Section 2.2.2" mislabel), this table is worth a dedicated line-by-line audit rather than spot-fixing the two examples above.

---

## 🟢 KEEP — 6. RQ1/RQ2/RQ3 answers in Part 3 use the correct figures

**Location:** I. Summary of Work, Answering the Research Questions (p.120)

Unlike Chapter 3's own §3.7.4 summary (which had the erroneous "6.1%"), Part 3's restatement of the research question answers correctly uses **5.4%, 7.7%, 8.2%, 92.8%, 26.0%**, matching Table 67/68's actual computed values exactly. So the error introduced in §3.7.4 didn't propagate into the conclusion, this section is internally correct. **No action needed here** (once Table 67 itself is reconciled with the appendix per the Chapter 3 review, these numbers will need to update together, but as a restatement of Chapter 3's stated figures, it's accurate).

---

## 🟢 KEEP — 7. Per-fold breakdown (Appendix A.5) is internally consistent

**Location:** Table 75, A.5 Per-Fold Variability

I checked that the three per-fold mAP values average to the reported mean for Fashion-CLIP: (0.2473 + 0.2480 + 0.2410) / 3 = 0.2454, exactly matching Table 72's reported mean. This is a small thing, but it confirms the appendix's *internal* arithmetic is sound, the problem is specifically that Appendix A as a whole doesn't match Chapter 3's body tables, not that the appendix's own numbers are self-contradictory.

---

## 🟢 KEEP — 8. AI-writing pattern check

Clean, consistent with every other chapter. No LLM-tell vocabulary beyond three uses of "robust" (used in a technical, load-bearing sense, e.g. "statistically robust top-tier separation," not filler). No stray em dashes. The Future Work section in particular reads as genuinely considered (each of the 7 directions ties back to a specific limitation named two paragraphs earlier), not generic boilerplate. **No action needed.**

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in passages sampled; no institutional tool available.

**References list (in full):** two citation problems were already found and documented in the Part 2 / Chapter 1 review (references [6] and [27]). Given how many numeric inconsistencies keep surfacing between the main text and appendices, it's probably worth a dedicated pass through the full reference list next, spot-checking the remaining ~25 entries the same way.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | Appendix A.4 efficiency numbers don't match Chapter 3's Table 68 (same problem as Table 67, now confirmed on a second metric set) | High | Correct, same root cause as Chapter 3 finding #1 |
| 2 | "Near-zero P@20" claim contradicted by Appendix A.2/A.3 (actual values ~0.30) | High | Correct |
| 3 | Three different "accuracy metric" counts (3, 5, 7) across the thesis | High | Correct, standardize |
| 4 | "Eleven supported architectures," 2 more occurrences | Medium | Rewrite (tie to Chapter 2 fix) |
| 5 | Requirements Traceability table (Table 70) has at least 2 confirmed mis-citations | Medium | Audit all 11 rows |
| 6 | RQ1–RQ3 answers restate correct figures | — | Keep |
| 7 | Per-fold breakdown (A.5) internally consistent | — | Keep |
| 8 | Prose / AI-writing check | — | Keep, clean |

---

*This closes out the main content chapters and the conclusion. What's left unreviewed: the full References list (25 of ~30 entries not yet individually checked), Appendix B (Dataset Composition), Appendix C (Hardware Specifications), and Appendix D (Database Schema). Given how central Appendix A turned out to be, and how many issues trace back to number mismatches, I'd suggest the References list next, then a final consolidated action list pulling every 🔴 item across all six review files into one prioritized fix list you can work through in order. Let me know which you'd like.*
