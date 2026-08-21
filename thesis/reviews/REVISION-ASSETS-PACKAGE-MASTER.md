# REVISION ASSETS PACKAGE — Master Consolidation

**Thesis:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**This document:** every item across all three review passes (factual/hallucination audit, language-level recalibration, peer review simulation), merged into one location-ordered checklist. 26 supporting files back this up with full evidence and exact before/after text; this is the single place to work from.

---

## How this package is organized

Three passes were run on this thesis, each answering a different question:

| Pass | Question it answers | Files |
|---|---|---|
| **1. Factual/Hallucination Audit** | Is every number, citation, and claim actually true and internally consistent? | `thesis-review-*.md` (9 files) + `thesis-review-MASTER-FIXLIST.md` |
| **2. Fact-Corrected Rewrite** | Given the fixes above, what's the exact replacement text? | `thesis-rewrite-*.md` (7 files) + `thesis-rewrite-MASTER-EDITLIST.md` |
| **3. Language-Level Recalibration** | Does the prose read like your actual pre-B2/B1-B2 English level, not native/AI-polished? | `language-audit-*.md` (7 files) |
| **4. Peer Review Simulation** | Would this pass a real committee, and what would they push back on? | `peer-review-simulation-thesis.md` |

This package merges all four into one revision sequence. Nothing below is new, every item traces to a specific source file listed next to it.

---

## PHASE 1 — Blocking issues (resolve before anything else)

These are the items that would stop a defense cold if not addressed. Do these first, in this order, since several later fixes depend on them.

### 1.1 — Reconcile Table 67/68 against Appendix A ⏸ REQUIRES YOUR ACTION
**Not a text edit, requires a decision or a fresh benchmark run.**
- Table 67 (Chapter 3 accuracy) and Table 74 (Appendix A.4 efficiency) don't match their appendix counterparts, despite §3.4 and Appendix C both stating they describe the same methodology and hardware.
- **What to do:** re-run the category-only benchmark, or check your script's git history/timestamps to determine which existing numbers are authoritative.
- **Once resolved:** propagate the correct numbers through Table 67, Table 68, Figures 42-45, the abstract, RQ1-RQ3 answers (Chapter 3 and Part 3), Table 70, and Appendix A itself.
- Full decision framework: `thesis-rewrite-chapter3-testing-evaluation.md`
- Why this is existential, not just factual: `peer-review-simulation-thesis.md`, Devil's Advocate section

### 1.2 — Correct two fabricated references
- **[6] Fashion-CLIP**: replace with `P. J. Chia, G. Attanasio, F. Bianchi, S. Terragni, A. R. Magalhães, D. Goncalves, C. Greco, and J. Tagliabue, "Contrastive language and vision learning of general fashion concepts," Scientific Reports, vol. 12, article 18958, 2022.` (cited 3× in Chapter 1)
- **[27] Fashion IQ**: replace with `H. Wu, Y. Gao, X. Guo, Z. Al-Halah, S. Rennie, K. Grauman, and R. Feris, "Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback," CVPR 2021, pp. 11307-11317.`
- Also: **[26] DeepFashion** needs one added co-author, "S. Qiu" — `Z. Liu, P. Luo, S. Qiu, X. Wang, and X. Tang, ...`
- Full text ready to paste: `thesis-rewrite-references.md`

### 1.3 — Fix the "15 to 20%" Fashion-CLIP claim (3 locations in Chapter 1)
All three currently claim a 15-20% improvement "confirmed in Chapter 3," which Chapter 3 doesn't contain (the real, correctly-computed figure is 5.4%).
- §1.3.3.5, §1.3.4.4, §1.6.1 — exact before/after text in `thesis-rewrite-part2-chapter1-background.md` (Edits 2, 3, 5)
- Also fix the same figure's stray "6.1%" appearance in §3.7.4 → `thesis-rewrite-chapter3-testing-evaluation.md` (Edit 1)

### 1.4 — Fix or remove the "30% search abandonment" statistic
Citation [2] (Pinterest search-volume press release) doesn't support this number. Appears in Part 1 §I and Chapter 1 §1.1.
- Fix: `thesis-rewrite-part1-introduction.md` (Edit 1)

---

## PHASE 2 — Should-fix factual/consistency items

Apply these next, all self-contained, none depend on Phase 1's benchmark decision.

| # | Location | Fix | Source file |
|---|---|---|---|
| 1 | Part 1, §VI | Rewrite Thesis Outline to match the real Table of Contents (no "Chapter 4") | `thesis-rewrite-part1-introduction.md` |
| 2 | 6 locations across Part 1, Ch.1, Ch.2, Part 3 | "Eleven models" → "four representative, selected from six" | `thesis-rewrite-MASTER-EDITLIST.md` items 2, 9, 11, 25, 26 |
| 3 | §1.3.4.5 | EfficientNet-B0 "3.4% lower mAP" → "7.7% lower mAP" | `thesis-rewrite-part2-chapter1-background.md` Edit 4 |
| 4 | §2.1 opening | "88 requirements / nine modules" → "87 requirements / eight modules" | `thesis-rewrite-part2-chapter2-sections1-2.md` Edit 1 |
| 5 | §2.3.1 | "Nine bounded contexts" → "eight" | `thesis-rewrite-part2-chapter2-section3.md` Edit 1 |
| 6 | §2.3.4.3, §2.4.3.2 | Two phantom "Section 2.1.5" references → §1.4.3-1.4.4 | `thesis-rewrite-part2-chapter2-section3.md` Edits 2-3 |
| 7 | §2.3.6.2 + 5 other locations | Permission format "Domain.Category.Resource.Action" → "domain.resource.action"; global cleanup of `domain:category:action` colon-format mentions | `thesis-rewrite-part2-chapter2-section3.md` Edit 4 |
| 8 | §2.4.4.3, §2.4.5.1 | Two conflicting CBIR endpoint URLs → both `/api/catalog/storefront/search-by-image` (verify against real routes first) | `thesis-rewrite-part2-chapter2-section4.md` Edits 1-2 |
| 9 | §2.4.5.2.1 | "Five visual search states" → "four" (or add missing 5th state to Table 58) | `thesis-rewrite-part2-chapter2-section4.md` Edit 4 |
| 10 | §2.4.5.2 | "Eight use cases" → "nine" (storefront) | `thesis-rewrite-part2-chapter2-section4.md` Edit 5 |
| 11 | Table 51 | pgvector "0.3.2" → "0.7.0" | `thesis-rewrite-part2-chapter2-section4.md` Edit 6 |
| 12 | Table 66 | PostgreSQL "16" → "17" | `thesis-rewrite-chapter3-testing-evaluation.md` Edit 2 |
| 13 | Part 3, Limitations | "Near-zero P@20" → real figures (~0.90 → ~0.30) | `thesis-rewrite-part3-conclusion.md` Edit 3 |
| 14 | Table 70, 2 rows | Fix mis-citations (§2.2.4 → §2.3.4/§2.4.3-4; §3.5 → §3.7.4 for RQ3) | `thesis-rewrite-part3-conclusion.md` Edit 4 |
| 15 | Part 3, Contributions | Three-way "accuracy metrics" count (3/5/7) → standardize on "three families, seven columns" | `thesis-rewrite-part3-conclusion.md` Edit 2 |

---

## PHASE 3 — Language-level recalibration

Apply after Phase 1-2 numbers are settled (so you're not editing the same sentences twice). This is a separate pass, same locations, different lens: reading level, not correctness.

**The single most important pattern to fix across the whole document:** the phrase **"confirming [that] X"** is used as a paragraph-ending template **six separate times** (4× in Chapter 3 §3.5-3.7, 2× in Part 3). This is the clearest AI/native-writer tell in the thesis, more than any individual advanced word. Vary this specifically wherever it appears.

**Second pattern:** the metaphor "bridges/bridging" (used for CBIR, CLIP, and the semantic gap) and "positions... within the landscape/environment of" both repeat 3× across Chapter 1 and §2.3. Replace with plain verbs ("connects," "closes the gap," "fits into").

**Highest-density single passage:** §2.4's opening paragraph, a 70-word sentence chaining five abstract clauses ("the concrete realization of... underpins... constitutes... deliver the user-facing experience"). This is the most native-sounding passage in the entire thesis. Full rewrite in `language-audit-part2-chapter2-section4.md`.

| Section | What needs recalibrating | Source file |
|---|---|---|
| Part 1 | "Fails where the domain succeeds," "bridges two ecosystems," "compounding inefficiencies," "extrapolate" | `language-audit-part1-introduction.md` |
| Ch.1 §1.1-1.3 | "Bridges" (×3), "serves as a universal descriptor," "unimpeded," "best overall balance" | `language-audit-part2-chapter1-sections1-3.md` |
| Ch.1 §1.4-1.6 | "Catalysed," "dialogue paradigm," "eliminating dual-database drift," 4× section-preview throat-clearing sentences; also flags a NEW factual issue: the "11 architectures" list names models (ResNet-152, CLIP ViT-B/32) absent from the real 6-model registry | `language-audit-part2-chapter1-sections4-6.md` |
| Ch.2 §2.1-2.2 | Two dense opening sentences; requirements tables/use-case specs correctly left untouched (standard format) | `language-audit-part2-chapter2-sections1-2.md` |
| Ch.2 §2.3 | "Positions... within" (3rd occurrence), "relies exclusively on," "executes... within the relational engine"; DDD/C4 terminology correctly kept as-is | `language-audit-part2-chapter2-section3.md` |
| Ch.2 §2.4 | The 70-word opening sentence (see above); "orchestrates," "co-locating" | `language-audit-part2-chapter2-section4.md` |
| Ch.3 | "Confirming X" (×4), "occupy... tier/region" (×2), "suffices," "coarse proxy" | `language-audit-chapter3-testing-evaluation.md` |
| Part 3 | "Confirming/confirms" (×2 more), "navigable via," "represents the quality ceiling," "define a roadmap... grounded in" | `language-audit-part3-conclusion.md` |

---

## PHASE 4 — Peer-review-driven strengthening

Not corrections, additions that would materially strengthen the thesis for a defense. From `peer-review-simulation-thesis.md`.

| # | What to add | Why |
|---|---|---|
| 1 | A brief cost comparison: open-source stack vs. a commercial visual-search API (rough infrastructure + engineering cost) | Substantiates the "lower-cost alternative" claim, currently asserted without evidence |
| 2 | One paragraph acknowledging the 5,000-image scale doesn't test the scaling behavior that's the actual hard problem in production visual search | Currently implicit in "future work" only; a committee will ask about this directly |
| 3 | Soften statistical-certainty language in §3.5/§3.7.3 to match what a 4-model, 3-fold design can support | Current phrasing ("exceeds the upper bound... confirming statistically meaningful separation") overstates what 12 data points can demonstrate |
| 4 | Justify or fix the sampling methodology (§B.1's "sequential selection preserves distribution" claim) | Logical gap: sequential selection only preserves distribution if the source is pre-shuffled, which isn't established |
| 5 | Distinguish "viable for a team with a dedicated engineer" from "viable for a two-person startup" when discussing the polyglot architecture | Current claim doesn't scope who it's actually viable for |
| 6 | Acknowledge (briefly) that Fashion-CLIP's advantage could partly reflect training-corpus differences, not purely domain fine-tuning | Alternative explanation currently unaddressed; strengthens the argument by showing awareness of it |

---

## Suggested working order

1. **Phase 1.1** first, alone, before touching anything else, since it determines what numbers Phase 2 items 13 and Phase 3's Chapter 3 fixes will ultimately use.
2. **Phase 1.2-1.4** next, these are independent and safe to batch together.
3. **Phase 2**, all 15 items, work top to bottom, most are single-sentence fixes.
4. **Phase 3**, do this as a dedicated pass once Phase 1-2 numbers are stable, so you're not re-editing sentences whose facts just changed.
5. **Phase 4**, additive strengthening, do this last or in parallel with your advisor's other feedback, since it's about defense-readiness rather than correctness.

---

## Full file index (26 files)

**Pass 1 — Factual audit:**
`thesis-review-part1-introduction.md` · `thesis-review-part2-chapter1-background.md` · `thesis-review-part2-chapter2-sections1-2.md` · `thesis-review-part2-chapter2-section3.md` · `thesis-review-part2-chapter2-section4.md` · `thesis-review-chapter3-testing-evaluation.md` · `thesis-review-part3-conclusion.md` · `thesis-review-references.md` · `thesis-review-appendices-bcd.md` · `thesis-review-MASTER-FIXLIST.md`

**Pass 2 — Fact-corrected rewrite text:**
`thesis-rewrite-part1-introduction.md` · `thesis-rewrite-part2-chapter1-background.md` · `thesis-rewrite-part2-chapter2-sections1-2.md` · `thesis-rewrite-part2-chapter2-section3.md` · `thesis-rewrite-part2-chapter2-section4.md` · `thesis-rewrite-chapter3-testing-evaluation.md` · `thesis-rewrite-part3-conclusion.md` · `thesis-rewrite-references.md` · `thesis-rewrite-MASTER-EDITLIST.md`

**Pass 3 — Language-level recalibration:**
`language-audit-part1-introduction.md` · `language-audit-part2-chapter1-sections1-3.md` · `language-audit-part2-chapter1-sections4-6.md` · `language-audit-part2-chapter2-sections1-2.md` · `language-audit-part2-chapter2-section3.md` · `language-audit-part2-chapter2-section4.md` · `language-audit-chapter3-testing-evaluation.md` · `language-audit-part3-conclusion.md`

**Pass 4 — Peer review:**
`peer-review-simulation-thesis.md`

**This file** is the index and merged checklist; each item above links to the source file with the full evidence, exact before/after text, and reasoning behind it.
