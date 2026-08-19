# Thesis Review — Chapter 3: Testing and Evaluation

**Scope of this file:** printed pages 108–119 (3.1 Goal of Testing through 3.7 Synthesis, Deployment Strategy, and Limitations)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk
**Note:** this is the chapter every earlier review file pointed to as the "source of truth" for the accuracy numbers used elsewhere in the thesis. Several open questions from Chapters 1–2 get resolved here, and one major new problem specific to this chapter turns up.

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. Table 67 (the chapter's headline results table) doesn't match the appendix that's supposed to back it up

**Location:** §3.4 (p.112) and Table 67, §3.5 (p.114)

This is the single most important finding across the whole review so far, so it's worth laying out in full now that we're in the chapter where it originates.

§3.4 states the ground truth used for the primary evaluation explicitly: *"Each image carries a category label as ground truth for binary relevance."* That's a category-only scheme. Table 67 then reports, under that scheme: Fashion-CLIP mAP = **0.8788**, P@5 = **0.9304**.

Appendix A.1, titled "Category-Only Ground Truth", describing exactly this same scheme, reports Fashion-CLIP mAP = **0.9309**, P@5 = **0.9582**. I checked every metric and every model in Table 67 against all three appendix ground-truth tables (A.1 category-only ≈0.93 range, A.2 category+colour ≈0.245 range, A.3 category+colour+pattern ≈0.215 range). **Table 67's numbers don't match any of them.** Same story for CLIP-generic, EfficientNet-B0, and ResNet-50, across every mAP, P@K, and R@K column.

This matters enormously because Table 67 isn't a side table, it's the one everything else is built on: Figure 42, Figure 43, the RQ1 answer, the accuracy-efficiency synthesis in §3.7.1, the deployment recommendations in §3.7.2, and the abstract all quote numbers that trace back to this exact table. If a committee member cross-checks this table against the appendix that claims to document "the complete retrieval results... from the 3-fold cross-validation benchmark described in Chapter 3," they will find it doesn't reconcile.

**What I can confirm is solid:** the arithmetic *within* Table 67 is completely correct, every percentage and ratio derived from it (5.4%, 7.7%, 8.2%, 2.2%, 2.7%, confidence-interval bounds) computes precisely from the raw numbers in the table (see item 6 below). So this isn't sloppy math, it's that Table 67's raw numbers themselves don't have a documented origin anywhere else in the thesis.

**Fix:** This needs to be resolved before anything else in the thesis, since so much depends on it. Re-run the category-only benchmark and confirm which set of numbers (Table 67's or Appendix A.1's) is actually correct, then make Table 67, Figures 42–45, the abstract, §3.7's synthesis and recommendations, and Appendix A all agree.

---

## 🔴 CORRECT — 2. A fourth, different percentage for the same comparison, this time inside Chapter 3 itself

**Location:** §3.7.4, Summary, finding 1 (p.119)

> "Domain-specific fine-tuning matters. Fashion-CLIP's **6.1% relative mAP improvement** over generic CLIP confirms that domain adaptation yields measurable gains."

**Problem:** Every other place in this same chapter computes and states this comparison as **5.4%**: the analysis paragraph under Table 67 ("Its mAP of 0.8788 is 5.4% above CLIP-generic"), the RQ1 answer ("The 5.4% mAP advantage over the generic CLIP wrapper..."), Table 69's synthesis ("mAP 0.8788 leads every other model by at least 5.4%"), and the RQ2 answer ("Fashion-CLIP's mAP is 5.4% higher"). I verified 5.4% is the mathematically correct figure from Table 67's own numbers: (0.8788 − 0.8341) / 0.8341 = 5.36% ≈ 5.4%.

"6.1%" doesn't correspond to any pair of numbers in Table 67 or Table 68. This is the same failure pattern flagged repeatedly in Chapter 1 (where the figure was inflated to "15 to 20%"), just a smaller, subtler version of it occurring inside Chapter 3's own summary bullet list. Combined with the Chapter 1 findings, this comparison now has **three different values** stated across the thesis: 15–20% (Chapter 1, ×3), 5.4% (Chapter 3, ×4, and the one that's actually correct), and 6.1% (Chapter 3 §3.7.4, ×1).

**Fix:** Change "6.1%" to "5.4%" in §3.7.4, and once Table 67 itself is reconciled with the appendix (item 1 above), make sure this bullet, and the Chapter 1 mentions, are updated together.

---

## 🔴 CORRECT — 3. PostgreSQL version is inconsistent within this chapter, not just across chapters

**Location:** §3.2.1, Testing Environment (p.108) vs. Table 66, Hardware Environment (p.113)

> §3.2.1: *"Database. **PostgreSQL 17** with pgvector 0.7.0, provisioned via Testcontainers for integration tests."*
> Table 66: *"Database: **PostgreSQL 16**, pgvector 0.7.0"*

**Problem:** These two environment descriptions are five pages apart in the same chapter and disagree on the PostgreSQL major version, while agreeing on the pgvector version (0.7.0). This confirms and sharpens the version inconsistency flagged in the Chapter 2 review (where "PostgreSQL 17" appeared seven times across the architecture and implementation sections, and Table 66 was the lone outlier saying "16"), it's not just a cross-chapter drift, the two most relevant environment descriptions in Chapter 3 itself don't agree with each other.

**Fix:** Confirm which version the benchmark hardware actually ran (check your Testcontainers config or the actual container image tag, e.g. `pgvector/pgvector:pg17-trixie`, mentioned in Chapter 2's Table 51) and correct Table 66 to match. Given the container tag referenced elsewhere explicitly says `pg17`, "16" in Table 66 is very likely the one that's wrong.

---

## 🟢 KEEP — 4. Dataset composition sums exactly

**Location:** §3.4, opening paragraph (p.112)

> "5,000 catalogue images across five categories (Apparel 2,500, Accessories 1,250, Footwear 750, Personal Care 350, Sporting Goods 150)."

2,500 + 1,250 + 750 + 350 + 150 = **5,000**. Matches exactly. **No action needed.**

---

## 🟢 KEEP — 5. Functional test case counts are internally consistent

**Location:** §3.2 vs. §3.3.1–3.3.5 (p.108–112)

§3.2 states the scenario breakdown as "visual search (7 scenarios), ML embedding pipeline (6 scenarios), shopping cart and checkout (8 scenarios), and admin product management (7 scenarios)." I counted the actual numbered test cases in Tables 60–63: 7 (visual search, #1–7) + 6 (ML embedding, #8–13) + 8 (cart/checkout, #14–21) + 7 (admin, #22–28) = **28**, matching §3.3.5's "All 28 test cases passed" exactly. **No action needed**, this is fully self-consistent.

One soft observation, not a factual error: every single one of the 28 test cases is marked "Pass" with no failures, retries, or fixes reported anywhere in the table. That's not implausible for a well-built system, but a thesis committee sometimes finds a 100% first-pass rate across every functional test to read as suspiciously clean. If any of these tests did surface a bug during development that got fixed before the final write-up, it might strengthen the chapter's credibility to mention that briefly (e.g., "test case 17, guest cart merge, initially failed to invalidate the guest cookie and was corrected during development"). Optional, not a required fix.

---

## 🟢 KEEP — 6. Extensive arithmetic check across §3.5–3.7 — everything computes correctly

I independently recalculated every percentage and ratio claim in the retrieval-performance and efficiency sections directly from Table 67 and Table 68's raw numbers:

| Claim | Computed | Match |
|---|---|---|
| Fashion-CLIP 5.4% above CLIP-generic (mAP) | 5.36% | ✓ |
| Fashion-CLIP 7.7% above EfficientNet-B0 (mAP) | 7.72% | ✓ |
| Fashion-CLIP 8.2% above ResNet-50 (mAP) | 8.23% | ✓ |
| CLIP-generic 2.2% above EfficientNet-B0 (mAP) | 2.24% | ✓ |
| CLIP-generic 2.7% above ResNet-50 (mAP) | 2.72% | ✓ |
| EfficientNet-B0 = 92.8% of Fashion-CLIP's mAP | 92.83% | ✓ |
| EfficientNet-B0 = 26.0% of Fashion-CLIP's latency | 25.98% | ✓ |
| EfficientNet-B0 2.7× faster than ResNet-50 | 2.68× | ✓ |
| EfficientNet-B0 3.8× faster than CLIP models | 3.85×/3.89× | ✓ |
| EfficientNet-B0 throughput 1.7× CLIP-generic | 1.67× | ✓ |
| EfficientNet-B0 throughput 2.6× ResNet-50 | 2.57× | ✓ |
| ResNet-50 storage 1.6× EfficientNet-B0 | 1.61× | ✓ |
| ResNet-50 storage 3.9× CLIP models | 3.94× | ✓ |
| Fashion-CLIP mAP lower bound (−2SD) = 0.8744 | 0.8744 | ✓ |
| EfficientNet-B0 mAP upper bound (+2SD) = 0.8172 | 0.8172 | ✓ |
| ResNet-50 mAP upper bound (+2SD) = 0.8224 | 0.8224 | ✓ |

Every single one checks out precisely. This is genuinely careful, well-computed quantitative writing, worth knowing that the *analysis* in this chapter is trustworthy even though the *source table* (item 1) has an unresolved provenance problem. Figures 42–45's captions also match Table 67/68 exactly. **No action needed** on any of this.

---

## 🟢 KEEP — 7. AI-writing pattern check

Same clean result as every other chapter. No LLM-tell vocabulary found. All four em dashes in this chapter are Table 68's "—" placeholders for the unreliable RAM measurements, not prose em dashes. The candid methodological note about psutil's RAM measurement failing on this Linux kernel (§3.4.2, §3.6, §3.7.3, mentioned three times, consistently) is a good, honest research-integrity signal, it's the kind of caveat a real experimenter writes when something didn't work as planned, not something an LLM tends to fabricate. **No action needed.**

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

**Figures 42–45 (charts):** captions cross-checked against the underlying tables and match exactly; the rendered charts themselves weren't visually inspected.

**Appendix A, B, C, D:** referenced extensively in this file for cross-checking Table 67, but not reviewed in full themselves yet. Given how central Appendix A is to resolving item 1, it may be worth a dedicated pass next.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | Table 67 doesn't match Appendix A.1 despite claiming the same methodology | High | Correct, re-run and reconcile |
| 2 | "6.1%" contradicts "5.4%" used 4x elsewhere in this same chapter | High | Correct |
| 3 | PostgreSQL 17 (§3.2.1) vs. PostgreSQL 16 (Table 66), within the same chapter | High | Correct, verify actual version |
| 4 | Dataset composition sums to 5,000 | — | Keep, verified |
| 5 | 28 functional test cases, breakdown matches exactly | — | Keep, verified (optional note on 100% pass rate) |
| 6 | 16 independent arithmetic checks across §3.5–3.7 | — | Keep, all correct |
| 7 | Prose / AI-writing check | — | Keep, clean |

---

*Chapter 3 is the last content chapter. Given how much of this review has pointed back to Appendix A as the unresolved piece, that's the natural next target, send it (or Part 3: Conclusion and Future Work) whenever you're ready.*
