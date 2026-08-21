# Thesis Rewrite — Master Edit List (Document Order)

This pulls every edit from all seven rewrite files into one document, in the order they appear in the thesis, so you can work through your actual `.typ` source top to bottom in a single pass instead of jumping between files. Full before/after text, rationale, and page numbers are in the individual rewrite files; this is the compact version for actually doing the editing.

**28 edits total.** All are safe to apply now, except where marked ⏸ (waiting on the Chapter 3 benchmark decision).

---

## PART 1: INTRODUCTION

| # | Location | Change |
|---|---|---|
| 1 | §I, Context and Motivation | Drop the "30 percent abandonment" figure (citation [2] doesn't support it); keep the qualitative point about session abandonment |
| 2 | §V, Development Methodology | "throughput across 11 models" → "throughput for four representative models, selected from six supported by the embedding framework" |
| 3 | §VI, Thesis Outline | Rewrite entirely to match the real Table of Contents (three parts; Part 2 = Chapters 1–3; Part 3 unnumbered) |

*Full text: `thesis-rewrite-part1-introduction.md`*

---

## PART 2, CHAPTER 1: BACKGROUND AND RELATED WORK

| # | Location | Change |
|---|---|---|
| 4 | §1.2.3.2 | Reframe the uncited "0.70 similarity threshold" as an observation from your own development work, not a general claim |
| 5 | §1.3.3.5 | "confirmed in the benchmark evaluation presented in Chapter 3" (15–20%) → "5.4% higher mAP under the category-only evaluation scheme (§3.5)" |
| 6 | §1.3.4.4 | "15 to 20 percent improvement... confirmed through the systematic benchmark in Chapter 3" → "outperforming general CLIP by 5.4% under the systematic benchmark presented in Chapter 3 (§3.5)" |
| 7 | §1.3.4.5 | "3.4 percent lower mAP@10" (EfficientNet-B0) → "7.7 percent lower mAP" |
| 8 | §1.6.1 | "improves retrieval by 15 to 20% over the general model" → softened to "improves fashion retrieval quality... a finding this thesis's own benchmark corroborates with a 5.4% mAP improvement (§3.5)" |
| 9 | §1.6.3, Contribution 3 | "evaluates 11 models on consumer-grade hardware" → "benchmarks four representative models, spanning CNN, ViT, and CLIP-based architectures, on consumer-grade hardware" |

*Full text: `thesis-rewrite-part2-chapter1-background.md`*

---

## PART 2, CHAPTER 2, §2.1–2.2: REQUIREMENTS AND SYSTEM MODELING

| # | Location | Change |
|---|---|---|
| 10 | §2.1 opening | "88 functional requirements across nine business modules... Dashboard" → "87 functional requirements across eight business modules" (drop Dashboard from this list, it has no FR table) |
| 11 | Table 19, §2.1.3 | "across 11 embedding models" → "across four representative embedding models, selected from six supported by the framework" |
| 12 | §2.2.1 (optional) | Add one sentence clarifying that "Support" actors (ML Service, Payment Gateway, etc.) are secondary/external, distinct from the three primary actors |

*Full text: `thesis-rewrite-part2-chapter2-sections1-2.md`*

---

## PART 2, CHAPTER 2, §2.3: SYSTEM ARCHITECTURE & DESIGN

| # | Location | Change |
|---|---|---|
| 13 | §2.3.1 | "partitioned into nine bounded contexts" → "partitioned into eight bounded contexts" |
| 14 | §2.3.4.3 | "(see Section 2.1.5 for index detail)" → "(see §1.4.3–1.4.4 for the HNSW/IVFFlat algorithm comparison)" |
| 15 | §2.4.3.2 | "(see Section 2.3.4 for index detail and Section 2.1.5 for ANN algorithm comparison)" → "(see §2.3.4 for index detail and §1.4.3–1.4.4 for the ANN algorithm comparison)" |
| 16 | §2.3.6.2 | "Permissions use the format Domain.Category.Resource.Action" → "Permissions use the format domain.resource.action" (matches the actual 3-part code examples) |
| 16b | *(cleanup, 5 locations)* | Global find-and-replace: `domain:category:action` → `domain.resource.action` across Chapter 1 §1.5.4, Chapter 2 NFR-02b (§2.1.2), §2.2.1, §2.3.2, and §2.4.5 |

*Full text: `thesis-rewrite-part2-chapter2-section3.md`*

---

## PART 2, CHAPTER 2, §2.4: IMPLEMENTATION

| # | Location | Change |
|---|---|---|
| 17 | §2.4.4.3 | `POST /api/admin/catalog/storefront/search-by-image` → `POST /api/catalog/storefront/search-by-image` *(verify against real Carter routes first, see note in source file)* |
| 18 | §2.4.5.1 | `POST /api/storefront/search-by-image` → `POST /api/catalog/storefront/search-by-image` *(same verification note)* |
| 19 | §2.4.5 opening | "organized by the 26 use cases defined in Section 2.2.2" → "...defined in Section 2.2" |
| 20 | §2.4.5.2.1, closing line | "The five visual search states are illustrated below" → "The four visual search states are illustrated below" *(or add the missing 5th state to Table 58 instead, see source file)* |
| 21 | §2.4.5.2 opening | "The customer storefront implements eight use cases" → "...implements nine use cases" |
| 22 | Table 51, §2.4.1 | "pgvector 0.3.2" → "pgvector 0.7.0" |

*Full text: `thesis-rewrite-part2-chapter2-section4.md`*

---

## CHAPTER 3: TESTING AND EVALUATION

| # | Location | Change |
|---|---|---|
| 23 | §3.7.4, finding 1 | "6.1% relative mAP improvement" → "5.4% mAP improvement" |
| 24 | Table 66 | "PostgreSQL 16" → "PostgreSQL 17" |
| ⏸ | Table 67, Table 68, Figures 42–45 | **Pending your decision.** Reconcile against Appendix A (Tables 71–75) once you've determined which run is authoritative. See the decision framework in `thesis-rewrite-chapter3-testing-evaluation.md`. |

*Full text: `thesis-rewrite-chapter3-testing-evaluation.md`*

---

## PART 3: CONCLUSION AND FUTURE WORK

| # | Location | Change |
|---|---|---|
| 25 | I. Achievement of Technical Objectives | "four models and eleven supported architectures" → "four models, selected from six supported by the framework" |
| 26 | II. Contributions, bullet 1 | "seven accuracy and five efficiency metrics across four architecture families, eleven models supported" → "three accuracy metric families (mAP, P@K, R@K, reported at three depths for seven total columns) and five efficiency metrics... six supported models" |
| 27 | III. Limitations | "near-zero P@20 values" → "substantially lower P@20 values, dropping from approximately 0.90 under category-only labels to approximately 0.30 under category-plus-colour-plus-pattern labels" |
| 28 | Table 70, row "Validate pgvector feasibility" | "Chapter 2, Section 2.2.4, Section 2.3.3" → "Chapter 2, Section 2.3.4, Section 2.4.3" |
| 28b | Table 70, row "Set up vector search" | "Chapter 2, Section 2.2.4, Section 2.3.3" → "Chapter 2, Section 2.3.4, Section 2.4.4" |
| 28c | Table 70, row "RQ3" | "Chapter 3, Section 3.5" → "Chapter 3, Section 3.7.4" |
| ⏸ | I. Answering the RQs | Already correct as written (matches Table 67). Revisit only if the Chapter 3 numbers change. |

*Full text: `thesis-rewrite-part3-conclusion.md`*

---

## REFERENCES

| # | Location | Change |
|---|---|---|
| — | [6] Fashion-CLIP | Replace with corrected entry (real title, *Scientific Reports* venue, real author list) |
| — | [27] Fashion IQ | Replace with corrected entry (Al-Halah not Al-Zahir, correct title, CVPR 2021) |
| — | [26] DeepFashion | Add missing co-author "S. Qiu" |

*Full text: `thesis-rewrite-references.md`*

---

## Suggested order of operations

1. **Apply edits 1–22 and 25–28c now.** None of these depend on anything else; they're all self-contained corrections.
2. **Fix the three References entries** ([6], [26], [27]), ideally right after finishing the Chapter 1 edits (5–9), since those are the passages citing [6].
3. **Do the global `domain:category:action` cleanup (16b)** in one pass across all five locations, easier to do as a single find-and-replace than piecemeal.
4. **Resolve the Chapter 3 benchmark question last.** Re-run the category-only benchmark or check your script's git history/timestamps to determine whether Table 67/68 or Appendix A is authoritative (see the decision framework in the Chapter 3 rewrite file). Once resolved, come back and update Table 67, Table 68, Figures 42–45, and the Part 3 items marked ⏸.
5. **Optional low-priority polish** (Part 1's Objectives/Outline redundancy, the §2.2.1 Support-actor clarification, an honest note about the 28/28 test pass rate): apply if you have time, skip if not, none of these affect correctness.

---

## Files in this rewrite set

1. `thesis-rewrite-part1-introduction.md`
2. `thesis-rewrite-part2-chapter1-background.md`
3. `thesis-rewrite-part2-chapter2-sections1-2.md`
4. `thesis-rewrite-part2-chapter2-section3.md`
5. `thesis-rewrite-part2-chapter2-section4.md`
6. `thesis-rewrite-chapter3-testing-evaluation.md`
7. `thesis-rewrite-part3-conclusion.md`
8. `thesis-rewrite-references.md`

Each has the full before/after text and the reasoning behind every change. This master file is the index, use it to track progress as you apply edits, and refer back to the individual files for the exact wording to paste in.
