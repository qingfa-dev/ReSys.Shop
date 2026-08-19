# Thesis Review — Master Consolidated Fix List

**Document:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Coverage:** Full thesis, Part 1 (Introduction), Chapter 1 (Background and Related Work), Chapter 2 (Design and Implementation, all 4 sections), Chapter 3 (Testing and Evaluation), Part 3 (Conclusion and Future Work), References (all 28 entries), Appendices B, C, D
**This file:** every 🔴 CORRECT and 🟠 REWRITE finding from the eight detailed review files, grouped by root cause so you can fix each recurring problem once instead of chasing it chapter by chapter. Ordered by how much of the thesis's credibility each item touches.

---

## How to use this document

Most of the serious problems in this thesis are not isolated typos, they're the **same underlying issue restated in multiple places**, because numbers and claims were copy-pasted across chapters as the thesis was assembled and never fully re-synced. So this list is organized by *root cause*, not by page order. Fix the root cause once, then use the location list under each item to update every place it echoes.

Tiers:
- **TIER 1 — Must fix before submission.** These are checkable, factual, or structural errors a committee member is likely to catch.
- **TIER 2 — Should fix.** Overclaims, imprecision, or inconsistencies that weaken credibility but are less likely to be caught immediately.
- **TIER 3 — Optional polish.** Minor, low-risk items.

---

## TIER 1 — Must fix before submission

### 1. The benchmark results in Chapter 3's main body don't match Appendix A — the single biggest issue in the thesis

**The problem:** Table 67 (accuracy) and Table 68 (efficiency) in Chapter 3, the tables that drive the abstract, both research-question answers, the deployment recommendation, and the conclusion, don't match the corresponding tables in Appendix A (Tables 71–75), even though:
- §3.4 explicitly describes the ground-truth methodology used in Table 67 as category-only binary relevance, which is *exactly* the methodology Appendix A.1 claims to document. The two tables' numbers disagree on every metric and every model (e.g., Fashion-CLIP mAP: 0.8788 in Table 67 vs. 0.9309 in Appendix A.1).
- Appendix A.4's efficiency table (Table 74) reports different latency, throughput, and load-time figures than Chapter 3's Table 68, for the same four models.
- Appendix C explicitly states *"All benchmark results reported in Chapter 3 and Appendix A were collected on a single workstation,"* which asserts they're the same run and makes the mismatch harder to explain, not easier.

**What's confirmed solid:** the arithmetic within each table is internally correct, every percentage and ratio in §3.5–3.7 (5.4%, 7.7%, 8.2%, 92.8%, 26.0%, all the ×-factors) was independently recalculated and checks out precisely against Table 67/68's own raw numbers. The problem isn't sloppy math, it's that the raw numbers in the main chapter and the appendix don't come from the same source.

**Action:** Before anything else, re-run the category-only benchmark and determine which set of numbers is authoritative. Then update, in this order: Table 67 → Table 68 → Figures 42–45 → the abstract → the RQ1/RQ2/RQ3 answers in §3.5–3.7 → Part 3's Summary of Work → Appendix A's tables (71–75) if they're the ones that need correcting instead.

**Found in:** Chapter 3 review (finding 1), Part 3 review (finding 1), Appendices B/C/D review (finding 2)

---

### 2. The "eleven models" claim doesn't match the actual model registry, which has six

**The problem:** Six separate places in the thesis claim the framework "supports eleven models," always in the context of "four representative models selected from eleven." But Table 55 (§2.4.4.1), the actual code-level model registry, lists exactly **six** models: `fashion_clip`, `clip_vit_b16`, `openclip-vit-b-32`, `efficientnet_b0`, `resnet50`, `dinov2_vits14`. The number eleven doesn't correspond to any real artifact anywhere in the thesis.

**All locations to fix:**
- Part 1, §V Research Methodology: "throughput across 11 models"
- Chapter 1, §1.3.4.1: "eleven models supported by the framework"
- Chapter 1, §1.6.3, Contribution 3: "This thesis evaluates 11 models"
- Chapter 2, §2.1.3, Table 19: "across 11 embedding models"
- Part 3, I. Summary of Work: "four models and eleven supported architectures"
- Part 3, II. Contributions: "eleven models supported"

**Action:** Change all six to reflect the real number (six), unless a genuinely separate, larger benchmark-only model list exists somewhere that simply hasn't been shown in the thesis, in which case add that list explicitly as evidence for the claim.

**Found in:** Part 1 review (finding 2), Chapter 1 review (finding 4), Chapter 2 §2.1–2.2 review (finding 2), Chapter 2 §2.4 review (finding 1), Part 3 review (finding 4)

---

### 3. The Fashion-CLIP vs. CLIP-generic improvement figure has three different values across the thesis

**The problem:** the same headline comparison is stated as three different numbers:
- **"15 to 20%"** — Chapter 1, §1.3.3.5, §1.3.4.4 (explicitly claims this is "confirmed in Chapter 3," which it isn't), and §1.6.1 (×3 total)
- **"5.4%"** — Chapter 3, used correctly and consistently ×4 in §3.5–3.7, and restated correctly in Part 3's RQ1 answer. This is the number that's mathematically correct given Table 67's own data: (0.8788 − 0.8341) / 0.8341 = 5.36% ≈ 5.4%.
- **"6.1%"** — Chapter 3, §3.7.4, Summary finding 1 (a one-off error inside the chapter that has the correct number everywhere else)

**Action:** Standardize on 5.4% (or whatever the corrected figure is once Tier 1 item #1 is resolved) everywhere. Fix all three occurrences in Chapter 1 and the one in §3.7.4.

**Found in:** Chapter 1 review (finding 1), Chapter 3 review (finding 2)

---

### 4. Two fabricated academic citations

**[6] Fashion-CLIP** (cited 3× in Chapter 1: §1.3.3.5, §1.3.4.4, §1.6.1): wrong title ("Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge" instead of the real "Contrastive language and vision learning of general fashion concepts"), wrong venue (claims SIGIR 2022; it was actually published in *Scientific Reports*), and a fabricated co-author ("S. Gieysztor" doesn't exist on the real paper).
> Corrected: Chia, P. J., Attanasio, G., Bianchi, F., Terragni, S., Magalhães, A. R., Goncalves, D., Greco, C., & Tagliabue, J. (2022). Contrastive language and vision learning of general fashion concepts. *Scientific Reports*, 12, 18958.

**[27] Fashion IQ**: wrong author name ("Z. Al-Zahir" instead of the real Ziad Al-Halah), a slightly wrong title, and wrong venue (claims ICCV 2019; it was published at CVPR 2021).
> Corrected: Wu, H., Gao, Y., Guo, X., Al-Halah, Z., Rennie, S., Grauman, K., & Feris, R. (2021). Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback. *CVPR 2021*, pp. 11307–11317.

**Action:** Replace both bibliography entries, then double-check the three citing sentences in Chapter 1 for [6] still make sense once the correct title/venue is substituted.

**Found in:** Chapter 1 review (finding 3), References review (findings 1–2)

---

### 5. "Eight bounded contexts" briefly becomes "nine," contradicting the very next sentence and its own table

**Location:** §2.3.1, System Overview: one sentence says "eight bounded contexts," three lines later a second sentence says "partitioned into **nine** bounded contexts," and Table 47 immediately below lists exactly 8 rows. Every other mention in the thesis (§2.3.2 ×3, Figure 33's caption, Appendix D's opening) says eight. This is an isolated one-word error.

**Action:** Change "nine bounded contexts" to "eight bounded contexts" in §2.3.1.

**Found in:** Chapter 2 §2.3 review (finding 1)

---

### 6. "88 functional requirements across nine business modules" is actually 87 across eight modules

**Location:** §2.1 opening paragraph. I counted every requirement ID directly from Tables 10–17: CAT 22, IDN 16, INV 12, ORD 14, PAY 10, SHP 6, PRF 3, LOC 4 = **87 total, across 8 modules**. "Dashboard," the ninth module named in this sentence, has zero functional requirements anywhere (though it is a real feature elsewhere, see Tier 2 item below).

**Action:** Change "88 functional requirements across nine business modules" to "87 functional requirements across eight business modules," and drop "Dashboard" from the module list (or add its missing requirements table if you'd rather keep it as a ninth module).

**Found in:** Chapter 2 §2.1–2.2 review (finding 1)

---

### 7. "Near-zero P@20" claim is contradicted by the thesis's own appendix data

**Location:** Part 3, III. Limitations: *"The enriched-label evaluation produces near-zero P@20 values."* Appendix A.2 and A.3 (the "enriched-label" schemes) actually report Fashion-CLIP P@20 = 0.3510 and 0.2997 respectively, and all four models sit in the same ~0.28–0.35 range. That's a real drop from the ~0.90 category-only baseline, but nowhere near zero.

**Action:** Replace with an accurate description, e.g., "P@20 drops substantially, from ~0.90 under category-only labels to ~0.30 under category+colour+pattern labels."

**Found in:** Part 3 review (finding 2)

---

### 8. CBIR search endpoint is given three different, mutually inconsistent URLs

**Locations:**
- §2.4.4.3: `POST /api/admin/catalog/storefront/search-by-image` (contains both "admin" and "storefront," 4 segments)
- §2.4.5.1: `POST /api/storefront/search-by-image` (missing the module segment entirely, 2 segments)
- Declared convention, §2.3.5.2: `/api/{module}/{surface}/{resource}` (3 segments, module first)
- None of the three agree, and the broader pattern (payment endpoints in §2.4.5.2 all put "storefront" first) suggests the stated convention itself may have the segment order backwards from what's actually implemented.

**Action:** Check the actual Carter route definitions in the codebase, then make every prose example and the stated convention agree with what's really there.

**Found in:** Chapter 2 §2.4 review (finding 3)

---

### 9. "Variable Vector Dimensions" contradicts the fixed `vector(512)` schema shown four times

**Location:** §2.3.4.4 claims pgvector columns support per-model dimensions (384/512/768/1280/2048), but §2.3.4.3, §2.4.3.2, Appendix D's Table 82, and Appendix D.9 all describe the actual embedding column as a fixed `vector(512)` type. Since pgvector's `vector(N)` is fixed-width by definition, a `vector(512)` column cannot also store a 2048-dimensional ResNet-50 embedding. This isn't just inconsistent phrasing, it's structurally contradictory given how the database type actually works.

**Action:** Check the real EF Core migrations/entity configuration. Either the "Variable Vector Dimensions" bullet needs to be removed or reframed as a future capability, or the schema documentation needs correcting to show whatever mechanism (if any) actually handles per-model dimensions.

**Found in:** Appendices B/C/D review (finding 1)

---

### 10. Two references to "Section 2.1.5," which doesn't exist

**Locations:** §2.3.4.3 and §2.4.3.2, both say "see Section 2.1.5 for index detail" / "ANN algorithm comparison." Section 2.1 only goes up to §2.1.3; there is no 2.1.4 or 2.1.5 anywhere in the thesis.

**Action:** Find whatever content this was meant to point to (likely §1.4.3–1.4.4's HNSW/IVFFlat comparison) and fix both references to the correct section number.

**Found in:** Chapter 2 §2.3 review (finding 2)

---

### 11. Visual search UI: "four-state model" vs. "five states," in the same subsection

**Location:** §2.4.5.2.1 opens by declaring a "four-state UI model," Table 58 lists exactly four rows (Empty, Upload, Loading, Results), then the closing sentence says "The five visual search states are illustrated below."

**Action:** Either add the missing fifth state (an Error state seems like the natural candidate given UC-STR-SRC's alternative flows) or change "five" to "four."

**Found in:** Chapter 2 §2.4 review (finding 2)

---

### 12. Thesis Outline (Part 1) describes a chapter structure that doesn't match the real Table of Contents

**Location:** Part 1, §VI Thesis Outline: uses "Chapter 1" for both the introduction (Part I) and the first chapter of Part II, and calls the conclusion "Chapter 4," which doesn't exist anywhere in the actual TOC (Part 3 uses Roman numerals, not chapter numbers).

**Action:** Rewrite to match the real structure: Part 1 (Introduction) → Part 2, Chapters 1–3 (Background, Design and Implementation, Testing and Evaluation) → Part 3 (Conclusion and Future Work).

**Found in:** Part 1 review (finding 1)

---

## TIER 2 — Should fix

### 13. PostgreSQL version: "17" used 8 times, "16" used once, in Chapter 3's own hardware table

Table 66 (§3.4.4) says PostgreSQL 16; every other mention in the thesis (§1.5.4, §2.3.1 Table 46, §2.3.3.2, §2.3.4, §2.4.1 Table 51, §3.2.1, Appendix D opening) says PostgreSQL 17, including §3.2.1, five pages before Table 66 in the same chapter. The container tag referenced elsewhere (`pgvector/pgvector:pg17-trixie`) suggests 17 is correct and Table 66 is the outlier.
**Action:** Confirm and correct Table 66 to say 17, or explain why the benchmark environment specifically used 16.
**Found in:** Chapter 2 §2.3 review (finding 3), Chapter 3 review (finding 3)

### 14. pgvector version: "0.3.2" in the pinned-versions table vs. "0.7.0" everywhere else

Table 51 (§2.4.1, "pinned version specifications") says pgvector 0.3.2; Chapter 3's test environment and hardware table both say 0.7.0.
**Action:** Confirm the real version and correct Table 51.
**Found in:** Chapter 2 §2.4 review (finding 4)

### 15. Permission-string format described two inconsistent ways

Described elsewhere as `domain:category:action` (colon-separated, 3 parts); §2.3.6.2 states `Domain.Category.Resource.Action` (dot-separated, 4 parts), but its own code examples (`catalog.products.create`) are dot-separated 3-part strings that match neither stated template exactly.
**Action:** Pick one format matching the actual code and use it consistently.
**Found in:** Chapter 2 §2.3 review (finding 4)

### 16. "Eight use cases" for storefront undercounts the nine actually documented

§2.4.5.2's opening sentence says eight; the eight subsections that follow (§2.4.5.2.1–2.4.5.2.8) document all nine storefront use cases including Profile Management, matching §2.2.4's full set.
**Action:** Change "eight" to "nine."
**Found in:** Chapter 2 §2.4 review (finding 5)

### 17. Three different counts for "accuracy metrics": 3, 5, and 7

§3.4.2 says "five accuracy... metrics measured per model"; Table 65 defines exactly three metric types (mAP, P@K, R@K); Part 3's Contributions and Table 70 both say "seven accuracy... metrics" (defensible only if counting each K-value as a separate column: mAP + 3×P@K + 3×R@K = 7).
**Action:** Standardize on one counting convention (recommend: "three metric families, seven reported columns") and fix §3.4.2's "five."
**Found in:** Part 3 review (finding 3)

### 18. Citation [2] (Pinterest) doesn't obviously support the "30% search abandonment" stat it's attached to

Reference [2] is Pinterest's press release about search *volume* (600M+ monthly searches), not an abandonment-rate study. Appears in Part 1 §I and Chapter 1 §1.1.
**Action:** Source the 30% figure separately, or soften the claim to only what [2] actually supports.
**Found in:** Part 1 review (finding 3)

### 19. Requirements Traceability table (Table 70) has at least two confirmed mis-citations

"Validate pgvector feasibility" and "Set up vector search" both cite "Section 2.2.4" (Customer Use Cases) instead of the database/ML sidecar sections where that content actually lives (§2.3.4 or §2.4.4). RQ3's answer is cited as "Section 3.5" when it's actually in §3.7.4.
**Action:** Audit all 11 rows of Table 70 for citation accuracy.
**Found in:** Part 3 review (finding 5)

### 20. EfficientNet-B0's stated accuracy trade-off doesn't match Chapter 3

§1.3.4.5 says "3.4 percent lower mAP@10" for EfficientNet-B0 vs. Fashion-CLIP; Chapter 3 consistently reports this gap as 7.7%.
**Action:** Correct to match Chapter 3's verified figure.
**Found in:** Chapter 1 review (finding 2)

### 21. DeepFashion citation [26] drops a co-author

Missing "Shi Qiu" from the real five-author list (Liu, Luo, Qiu, Wang, Tang); title, venue, year, and pages are otherwise correct.
**Action:** Add the missing author name.
**Found in:** References review (finding 3)

### 22. "Sequential" image selection paired with "preserves the natural category distribution" doesn't logically follow

Appendix B.1: sequential (non-random) selection only preserves distribution if the source data is already shuffled, which isn't stated.
**Action:** Either confirm and state the source data is pre-shuffled, or describe the actual (likely stratified) sampling method used.
**Found in:** Appendices B/C/D review (finding 4)

---

## TIER 3 — Optional polish

- **Cross-reference typo:** §2.4.5's "organized by the 26 use cases defined in Section 2.2.2" should say "Section 2.2" (2.2.2 is Functional Decomposition, not where use cases are defined). *Chapter 2 §2.4 review, finding 6.*
- **"Support" actors not mentioned in the actor count:** §2.2.1 says three actors interact with the platform, but individual use case tables reference a fourth "Support" field (ML Service, Payment Gateway, etc.), standard UML convention but could use one clarifying sentence. *Chapter 2 §2.1–2.2 review, finding 6.*
- **Objectives vs. Thesis Outline redundancy:** Part 1's Technical Objectives and the Chapter 3 outline summary repeat similar content across two pages. Optional trim only. *Part 1 review, finding 6.*
- **100% first-pass test rate:** all 28 functional test cases in §3.3 report "Pass" with zero failures or fixes. Not an error, but a committee sometimes reads a perfectly clean pass rate as suspicious; mentioning one real bug-and-fix during development (if one occurred) would strengthen credibility. *Chapter 3 review, finding 5.*
- **Phantom "Chapter 6" reference** appears twice (Appendix A.2's Table 72 caption, and Appendix B.3), almost certainly should say "Chapter 3." *Original full-document pass; Appendices B/C/D review, finding 7.*
- **Dashboard module clarification:** not a phantom feature, it's real (1 API endpoint, aggregated metrics per Table 50), it's specifically the missing functional-requirements table that needs fixing (see Tier 1 item #6). *Chapter 2 §2.3 review, finding 7.*

---

## What was verified as accurate (no action needed)

It's worth knowing what held up, since a lot did:

- **$770 billion global fashion e-commerce market figure (2024)** — matches Statista's published number almost exactly.
- **DeepFashion "800,000+ images," Pinterest "600M+ monthly searches"** — both verified against public sources.
- **Model parameter counts** (ResNet-50/101, EfficientNet-B0/B4, CLIP ViT-B/16) — match published architecture specs.
- **"26 use cases" total** — verified by direct count (15 admin + 9 storefront + 2 system).
- **"Fifteen administrative use cases"** — verified exact by count.
- **Requirements traceability** — all 73 FR-ID references inside use case specs correctly resolve to the 87 requirements actually defined; no orphan citations.
- **"Approximately 262 Carter endpoints"** and **"Eleven inter-module contract DTOs"** — both verified as exact sums from Table 50 and §2.3.5.2.
- **"Six containerized resources"** — reconciles correctly once you count the two Vue SPAs separately from the four backend services.
- **JWT 15-minute expiry, 15-minute inventory reservation timeout** — consistent everywhere they're mentioned.
- **16 independent arithmetic checks across Chapter 3 §3.5–3.7** (all the %, ×-factor, and confidence-interval claims) — every single one recalculated correctly from the raw table data.
- **Dataset composition** (2,500/1,250/750/350/150 = 5,000) and **28 functional test case breakdown** (7+6+8+7) — both sum exactly.
- **Appendix A.5 per-fold values** — average to the reported means exactly.
- **PyTorch 2.13.0 and Transformers 5.14.1** — both confirmed as genuinely accurate, current version numbers for the stated timeframe, not generic placeholders.
- **References [3] ResNet, [4] EfficientNet, [5] CLIP, [10] ViT, [12] HNSW** — all verified correct against the published record.
- **Overall prose** — every chapter scanned clean of LLM-tell vocabulary (leverage, delve, seamless, testament, "it is important to note," etc.) and free of stray prose em dashes. The writing reads as genuinely authored throughout.

---

## What's still unreviewed

- **~18 of the 28 references** not individually re-verified (documentation links, books, standards, lower-risk categories, see the References file for the full breakdown).
- **~35 of Appendix D's table definitions** (Identity, Ordering, Payment, Shipping, Profile, Location schemas) were skimmed for structural consistency but not checked field-by-field against the actual EF Core migrations.
- **Diagram/figure content** (Figures 1–45): captions were cross-checked against surrounding text throughout, but the rendered diagrams themselves weren't visually inspected in any chapter.
- **Formal plagiarism screening**: no institutional tool (Turnitin, etc.) was available for this review. Spot-checks against public web text found no verbatim matches, but this isn't a clearance, run it through your university's actual tool before submission.

---

## Source files

This master list draws from the following detailed review files (all in your outputs folder):

1. `thesis-review-part1-introduction.md`
2. `thesis-review-part2-chapter1-background.md`
3. `thesis-review-part2-chapter2-sections1-2.md`
4. `thesis-review-part2-chapter2-section3.md`
5. `thesis-review-part2-chapter2-section4.md`
6. `thesis-review-chapter3-testing-evaluation.md`
7. `thesis-review-part3-conclusion.md`
8. `thesis-review-references.md`
9. `thesis-review-appendices-bcd.md`

Each has the full evidence, quoted text, and exact wording for every finding summarized above.
