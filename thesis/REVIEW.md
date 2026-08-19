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

# Thesis Review — Part 1: Introduction

**Document:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Scope of this file:** Part 1 only (printed pages 2–5: Context and Motivation, Problem Statement, Objectives, Scope and Limitations, Research Methodology, Thesis Outline)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. Thesis Outline describes a chapter structure that doesn't exist

**Location:** Section VI, "Thesis Outline" (p.5)

> "The thesis is organized into five chapters across three parts.
> Part I: Introduction. **Chapter 1** establishes research context...
> Part II: Thesis Content contains three chapters: **Chapter 1**: Background... Chapter 2: Design and Implementation... Chapter 3: Evaluation...
> Part III: Conclusion. **Chapter 4** synthesizes findings..."

**Problem:** This paragraph is internally contradictory and doesn't match your own Table of Contents.
- Part I is called "Chapter 1," and Part II *also* starts with "Chapter 1" — the same number used for two different chapters.
- Part III is called "Chapter 4," but if Part I's introduction counts as a chapter, Part II's three chapters would be 2, 3, 4 — making Part III's conclusion "Chapter 5," not "Chapter 4."
- Your actual Table of Contents doesn't use "Chapter" numbering for Part 1 or Part 3 at all — it uses Roman numerals (I–VI for Part 1, I–V for Part 3) and only numbers chapters inside Part 2 (Chapter 1 Background, Chapter 2 Design and Implementation, Chapter 3 Testing and Evaluation). There is no "Chapter 4" anywhere in the real document.

**Why this matters:** This reads like a leftover paragraph from an earlier drafting pass (or an AI-generated outline stub) that was never reconciled with the final structure. It's the first thing after your objectives and research questions, so it's a bad first impression and an easy, guaranteed catch for any reader who flips to the Table of Contents.

**Fix:** Rewrite to match your real structure, e.g.:
> "This thesis is organized into three parts. Part 1 (this part) introduces the research context, problem statement, objectives, scope, and methodology. Part 2 contains three chapters: Chapter 1 (Background and Related Work), Chapter 2 (Design and Implementation), and Chapter 3 (Testing and Evaluation). Part 3 presents the conclusion, contributions, limitations, future work, and requirements traceability."

---

## 🟠 REWRITE — 2. "11 models" overstates what was actually tested

**Location:** Section V, "Development Methodology" (p.4–5)

> "Testing and Evaluation (mAP accuracy with cross-validation, inference latency, throughput **across 11 models**)."

**Problem:** Everywhere else in the thesis (Chapter 3, Appendix A/B) you're careful and explicit that only **four** representative models were formally benchmarked with full accuracy/efficiency tables, and that these four were "selected from the eleven supported by the benchmark framework." This sentence in Part 1 drops that distinction and implies all 11 were tested end-to-end, which isn't what Chapter 3 shows.

**Fix:** Match the phrasing you already use correctly elsewhere:
> "Testing and Evaluation (mAP accuracy with cross-validation and inference latency/throughput for four representative models, selected from eleven supported by the benchmark framework)."

---

## 🟠 REWRITE — 3. Citation [2] doesn't obviously support the stat it's attached to

**Location:** Section I, "Context and Motivation" (p.2), also repeated in Chapter 1.1

> "Industry estimates place session abandonment after unsuccessful search at approximately 30 percent [2]."

**Problem:** Reference [2] in your bibliography is "Pinterest Engineering, 'Pinterest Visual Search: 600M+ Monthly Searches.'" That's Pinterest's own PR/newsroom post about search *volume*, not an industry-wide abandonment-rate study. A 30% search-abandonment figure needs its own source (there are UX/retail research reports that report numbers in this range — e.g. Baymard Institute–style search-abandonment studies) rather than being attached to a citation about a different statistic. As written, this looks like a plausible-sounding number attached to whichever reference was nearby, which is exactly the pattern a hallucination/citation check flags.

**Fix:** Either find the actual source for the 30% figure and cite that separately, or soften the claim to something you can support with [2] alone (e.g., cut the specific number and just cite Pinterest's search-volume growth as evidence that customers are shifting toward visual search).

---

## 🟢 KEEP — 4. "$770 billion in 2024" market-size figure

**Location:** Section I, "Context and Motivation" (p.2) and Chapter 1.1

> "Global fashion e-commerce revenue exceeded 770 billion USD in 2024, with projections surpassing one trillion by 2030 [1]."

I checked this against Statista's own published figure for fashion e-commerce (Statista's "Fashion eCommerce: market data & analysis" page states global revenues of US$770.9 billion in 2024), and it matches closely. The trillion-by-2030 trajectory is also broadly consistent with other market forecasts. **No change needed**, this one is solid.

---

## 🟢 KEEP — 5. Overall prose quality (AI-writing check)

I scanned Part 1 specifically for common LLM tells (leverage, delve, seamless, robust, testament, cutting-edge, "in today's landscape," triadic "not only X but also Y," stray em dashes in prose, etc.). It's clean. The one em dash present ("attributes that resist textual description" — no wait, checked: the em dash at "print density, and colour – attributes that resist..." uses an en dash, consistent with your Typst formatting elsewhere, not a stray AI em dash) is stylistic, not a red flag. The writing here reads as genuinely authored: specific, technical, no filler. **No action needed.**

---

## 🟡 CUT / TIGHTEN — 6. Minor redundancy between Objectives and Thesis Outline

**Location:** Section III "Objectives" (p.3) vs Section VI "Thesis Outline" (p.5)

The "Technical Objectives" bullets (model integration, polyglot architecture, vector storage validation, empirical benchmarking) and the Chapter 3 outline description say almost the same thing twice, once as forward-looking objectives and once as a chapter summary. Not wrong, just a little repetitive across two pages. Optional trim if you're tightening for length; not a correctness issue.

---

## Not checked in this pass

Plagiarism: I don't have an institutional plagiarism-detection tool (Turnitin/DoIT), so I can't give Part 1 a clean bill on text-matching against other theses or paywalled sources. Nothing in Part 1 read as copy-pasted prose during my read-through (the phrasing is specific to your project's architecture and numbers), but a formal check before submission is still worth doing.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | Thesis Outline chapter numbering contradicts the real TOC | High | Correct |
| 2 | "11 models" overstates benchmark scope vs. Chapter 3 | Medium | Rewrite |
| 3 | Citation [2] mismatched to the 30% abandonment stat | Medium | Rewrite / re-source |
| 4 | $770B market figure | — | Keep, verified |
| 5 | Prose / AI-writing check | — | Keep, clean |
| 6 | Objectives vs. Outline redundancy | Low | Optional trim |

---

*Next: send "Chapter 1: Background and Related Work" (Part 2) when you're ready, and I'll do the same pass on it.*
# Thesis Review — Part 2, Chapter 1: Background and Related Work

**Scope of this file:** printed pages 6–29 (1.1 Fashion E-Commerce through 1.6.3 Contribution Differentiators)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. The "15 to 20% improvement" claim contradicts your own Chapter 3 results — and appears three times

**Locations:**
- §1.3.3.5 (p.16): *"The original paper reports a 15-to-20% improvement on fashion retrieval over general CLIP, **confirmed in the benchmark evaluation presented in Chapter 3.**"*
- §1.3.4.4 (p.18): *"Fashion-CLIP achieved the highest mAP among the evaluated models, with a **15 to 20 percent improvement** over general CLIP on fashion-specific queries, **confirmed through the systematic benchmark in Chapter 3** [6]."*
- §1.6.1 (p.28): *"The Fashion-CLIP work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images **improves retrieval by 15 to 20%** over the general model [6]."*

**Problem:** This is a real, checkable contradiction, not a matter of interpretation. Your own Chapter 3, Table 67 (§3.5) reports Fashion-CLIP's mAP advantage over CLIP-generic as **5.4%** (0.8788 vs 0.8341), and that 5.4% figure is repeated consistently throughout Chapter 3, the abstract, and the requirements-traceability matrix. Two of the three passages above explicitly claim the 15–20% figure is "confirmed" by that same Chapter 3 benchmark. It isn't; Chapter 3 shows less than a third of the claimed improvement.

I also checked the actual Fashion-CLIP paper (Chia et al., "Contrastive language and vision learning of general fashion concepts," *Scientific Reports*, 2022) for a "15–20%" figure. Their multi-modal retrieval table reports HITS@5 of 0.61 for FashionCLIP vs. 0.22 for CLIP on their internal test set, a much larger gap than 15–20%, and on a different metric (HITS@5, not mAP). I could not find a "15–20%" figure anywhere in that paper, so this number doesn't seem to trace back to a verifiable source either.

**Why this matters:** This is the exact pattern a plagiarism/hallucination reviewer flags hardest: a specific, repeated, precise-sounding statistic that (a) doesn't match the source it's attributed to and (b) contradicts your own measured results one chapter later. A committee member who reads Chapter 1 and then Chapter 3 back to back will catch this immediately, and it undermines confidence in every other number in the thesis.

**Fix:** Remove the "15 to 20 percent" and "confirmed in Chapter 3" language in all three places. Replace with your own real number, consistent with Chapter 3:
> "Fashion-CLIP achieved the highest mAP among the evaluated models, outperforming general CLIP by 5.4% under the category-only evaluation, as confirmed in Chapter 3 (§3.5)."

This is also tied to a separate reference problem, see item 3 below.

---

## 🔴 CORRECT — 2. EfficientNet-B0's accuracy trade-off is misstated

**Location:** §1.3.4.5 (p.18)

> "EfficientNet-B0 provides the fastest inference at 5.3 million parameters, trading off **3.4 percent lower mAP@10** with no text-to-image capability."

**Problem:** Chapter 3 (§3.5, and repeated at line ~6306 in the source PDF) consistently states EfficientNet-B0's mAP is **7.7% below** Fashion-CLIP's (0.8158 vs. 0.8788), not 3.4%. This is a second, separate instance of a specific percentage in Chapter 1 that doesn't match the number reported for the same comparison in Chapter 3.

**Fix:** Change "3.4 percent lower mAP@10" to "7.7 percent lower mAP" (or whatever the correct figure is once you've resolved the Table 67 vs. Appendix A discrepancy flagged in the earlier full-thesis review). Either way, make sure every place in the thesis that quotes this comparison uses the same number.

---

## 🔴 CORRECT — 3. Reference [6] (Fashion-CLIP citation) is fabricated

**Location:** cited at §1.3.3.5, §1.3.4.4, and §1.6.1; defined in the References list as:

> [6] A. Chia, S. Gieysztor, and others, "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge," in *Proceedings of the 45th International ACM SIGIR Conference...* 2022.

**Problem:** (carried over from the earlier full-document pass, repeating it here since Chapter 1 is where it's actually used three times) The real paper is Chia, Attanasio, Bianchi, Terragni, Magalhães, Goncalves, Greco, Tagliabue, **"Contrastive language and vision learning of general fashion concepts,"** *Scientific Reports* (Nature), vol. 12, 2022. Wrong title, wrong venue (not SIGIR), and "S. Gieysztor" is not a real co-author on that paper. Since this reference underpins three separate claims in this chapter, it needs to be fixed at the source.

**Fix:** Replace the bibliography entry with the correct citation:
> Chia, P. J., Attanasio, G., Bianchi, F., Terragni, S., Magalhães, A. R., Goncalves, D., Greco, C., & Tagliabue, J. (2022). Contrastive language and vision learning of general fashion concepts. *Scientific Reports*, 12, 18958. https://doi.org/10.1038/s41598-022-23052-9

---

## 🟠 REWRITE — 4. "Evaluates 11 models" overclaims scope again

**Location:** §1.6.3, Contribution 3 (p.29)

> "Commercial visual search runs on cloud TPU clusters. This thesis **evaluates 11 models** on consumer-grade hardware, establishing that production-quality visual search is achievable without specialised infrastructure..."

**Problem:** Same issue flagged in Part 1: elsewhere you're careful to say four representative models were formally benchmarked, "selected from the eleven supported by the framework" (§1.3.4.1 gets this right). This sentence drops the distinction again and claims all 11 were evaluated, which overstates what Chapter 3 actually demonstrates as a contribution.

**Fix:**
> "This thesis benchmarks four representative models, spanning CNN, ViT, and CLIP-based architectures, on consumer-grade hardware, out of eleven supported by the framework, establishing that production-quality visual search is achievable without specialised infrastructure."

---

## 🟠 REWRITE — 5. Unsupported precision threshold

**Location:** §1.2.3.2 (p.7)

> "For normalized fashion embeddings, scores above 0.70 generally correspond to strong visual similarity perceptible to human shoppers."

**Problem:** This is a specific, confident empirical claim (a 0.70 cosine-similarity threshold tied to human perception) with no citation and no reference to your own data. It reads as plausible domain knowledge, but as written it's an assertion, not a sourced or measured fact. If this number came from your own qualitative observation while building the system, say so explicitly; if it's a general claim about embedding models, it needs a citation.

**Fix:** Either cite a source for the 0.70 threshold, or soften it: "informal inspection of retrieval results during development suggested that scores above roughly 0.70 tended to correspond to..." Vague, hedged claims are much safer here than a precise, uncited number.

---

## 🟢 KEEP — 6. Technical facts and figures verified

I checked several of the more checkable factual claims in this chapter against public sources:

- **DeepFashion dataset size** (§1.6.1): "over 800,000 images" matches DeepFashion's published scale. Correct.
- **Pinterest Lens "600M+ monthly searches"** (Table 9, §1.6.2): matches Pinterest's own published figures. Correct.
- **Model parameter counts** (Tables 2–5): ResNet-50 (25.6M), ResNet-101 (44.5M), EfficientNet-B0 (5.3M, 1280-dim), EfficientNet-B4 (19.3M, 1792-dim), CLIP ViT-B/16 (~150M, 512-dim) all match published architecture specs within normal reporting variance. No action needed.

---

## 🟢 KEEP — 7. AI-writing pattern check

Same result as Part 1: no chronic LLM tells (leverage, delve, seamless, testament, "in today's landscape," triadic filler phrases). The explanatory passages (semantic gap, latent space, cosine similarity, CBIR pipeline) read as genuinely authored technical writing, specific and grounded in the actual system, not generic filler. **No action needed** here; the writing style is not the issue in this chapter, the numeric/citation accuracy is.

---

## Not checked in this pass

**Plagiarism:** No verbatim matches found in the passages I spot-checked against public sources, but I don't have an institutional plagiarism tool, so this isn't a clearance, just an absence of red flags in what I sampled.

**Sections 1.5 (Platform Architecture and Technology Stack):** mostly descriptive of your own implementation choices (modular monolith, .NET, Vue, Redis, Hangfire) and didn't contain checkable external claims or numbers, so I didn't flag anything there. Let me know if you want a line-by-line pass on it anyway.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | "15–20% improvement" claim, repeated 3x, contradicts Chapter 3's 5.4% | High | Correct (all 3 locations) |
| 2 | EfficientNet-B0 "3.4% lower mAP" contradicts Chapter 3's 7.7% | High | Correct |
| 3 | Reference [6] (Fashion-CLIP citation) is fabricated | High | Correct in bibliography |
| 4 | "Evaluates 11 models" overclaim | Medium | Rewrite |
| 5 | Uncited 0.70 cosine-similarity threshold | Low | Rewrite / cite |
| 6 | Model specs, dataset sizes, Pinterest stat | — | Keep, verified |
| 7 | Prose / AI-writing check | — | Keep, clean |

---

*Next: send "Chapter 2: Design and Implementation" (Part 2) when you're ready, and I'll do the same pass on it.*

# Thesis Review — Part 2, Chapter 2, Sections 2.1–2.2

**Scope of this file:** printed pages 30–77 (2.1 Requirements Specification, 2.2 System Modeling)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. "88 functional requirements across nine business modules" doesn't match what's actually documented

**Location:** §2.1 opening paragraph (p.30)

> "The platform delivers **88 functional requirements across nine business modules**... Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, Location, and Dashboard."

**Problem:** I extracted every requirement ID from §2.1.1's tables (Tables 10–17) and counted them directly:

| Module | Prefix | Count |
|--------|--------|-------|
| Catalog | CAT-FR | 22 |
| Identity | IDN-FR | 16 |
| Inventory | INV-FR | 12 |
| Ordering | ORD-FR | 14 |
| Payment | PAY-FR | 10 |
| Shipping | SHP-FR | 6 |
| Profile | PRF-FR | 3 |
| Location | LOC-FR | 4 |
| **Total** | | **87** |

That's **87, not 88**, and it covers **eight** modules, not nine. **Dashboard**, the ninth module named in the opening sentence, has zero functional requirements anywhere in the thesis. I searched the full 171-page document: "Dashboard" appears exactly twice total, once in this sentence and once elsewhere as an unrelated folder-count entry (a Vue admin panel structure listing), never as a requirements table, never with a `DSH-FR-XX` ID.

I also checked each module's ID sequence (CAT-FR-01 through 22, IDN-FR-01 through 16, etc.) for gaps that might explain the off-by-one, there are none; every module's numbering is fully sequential with no skipped IDs. So this isn't a stray typo from a deleted requirement, it's a genuine mismatch between the summary claim and the actual content.

**Fix:** Either add the missing Dashboard requirements (if that module genuinely exists in the implementation and was just never written up here), or, more likely, drop "Dashboard" from the module list and correct "88" to "87":
> "The platform delivers 87 functional requirements across eight business modules: Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, and Location."

---

## 🟠 REWRITE — 2. The "11 embedding models" overclaim resurfaces a fourth time

**Location:** Table 19, Feature Classification (§2.1.3, p.40), "Model Benchmark System" row

> "Secondary Contribution: Systematic benchmarking of retrieval accuracy and latency across **11 embedding models**, providing model selection guidelines for deployment."

**Problem:** This is the same overclaim flagged twice already in Chapter 1 (§1.3.4.1 vs. §1.6.3) and once in Part 1 (§V Research Methodology): only **four** representative models were formally benchmarked with accuracy/latency tables; eleven is the total number *supported by the framework*, not the number actually evaluated. Because this table explicitly frames it as a thesis "contribution," the imprecision matters more here than elsewhere, a reader could reasonably expect eleven full result sets and only find four.

**Fix:**
> "Secondary Contribution: Systematic benchmarking of retrieval accuracy and latency across four representative embedding models (selected from eleven supported by the framework), providing model selection guidelines for deployment."

---

## 🟢 KEEP — 3. "26 use cases" claim — verified exact

**Location:** §2.2 opening (p.40): *"Three actors interact with the platform across 26 use cases..."*

I extracted every distinct `UC-ADM-*`, `UC-STR-*`, and `UC-SYS-*` identifier used across §2.2.3–2.2.5 and counted them directly: **15 Administrator use cases + 9 Customer/Storefront use cases + 2 System use cases = 26.** This matches exactly. **No action needed**, this is a case where a precise-sounding number actually checks out.

---

## 🟢 KEEP — 4. Requirements traceability is internally consistent

I cross-referenced every functional-requirement ID cited inside a use case's "Requirements" field (73 distinct IDs across §2.2.3–2.2.5) against the 87 IDs actually defined in §2.1.1's tables. **Every single reference resolves correctly**, there are no orphan citations to requirements that don't exist. This is a genuinely well-maintained piece of traceability across ~50 pages and a lot of tables; it's worth knowing this part of the chapter doesn't need rework.

I also checked a couple of specific numeric claims that appear in more than one place for consistency:
- JWT access-token lifetime: "15-minute JWT access token" (IDN-FR-02) matches "JWT access tokens expire after 15 minutes" (NFR-02a). Consistent.
- Inventory reservation timeout: "expire after 15-minute inactivity" (§2.2.1.3, System actor) matches "Unconfirmed checkout inventory holds expire after 15 minutes of inactivity" (NFR-05d). Consistent.

---

## 🟢 KEEP — 5. AI-writing pattern check

Same clean result as the earlier chapters. I scanned this ~2,500-line section for the standard LLM tells (leverage, delve, seamless, testament, "it is important to note," etc.) and found none. All 26 em dashes present are the structural "Use Case ID — Name" table-header convention used consistently throughout (e.g., "UC-ADM-PROD — Manage Products"), not stray prose em dashes. The use-case scenario writing (numbered main flows, alternatives, exceptions) is dense, specific, and consistent in structure across all 26 specifications, this reads as carefully hand-built content, not generated filler. **No action needed.**

---

## 🟡 Optional note — 6. "Support" actors aren't mentioned in the actor count

**Location:** §2.2.1 says three actors interact with the platform (Customer, Administrator, System), but several individual use case tables list a fourth field, "Support", naming external systems the use case depends on: ML Service (UC-STR-SRC, UC-ADM-IMG, UC-SYS-EMB), Payment Gateway (UC-ADM-PAY, UC-STR-CHK, UC-STR-PAY, UC-SYS-MNT), Email Service and Google OAuth (UC-STR-AUT).

This isn't wrong, "Support" actors are a standard UML convention for secondary/external systems a use case interacts with, distinct from the primary actors who initiate use cases. But since §2.2.1 doesn't mention this distinction explicitly, a reader could be briefly confused about why a fourth actor type appears inside individual use case tables after being told there are only three. A one-sentence clarification in §2.2.1 (e.g., "Individual use cases may additionally reference supporting external systems such as the ML sidecar, payment gateway, or OAuth provider") would close the gap. Low priority, not a factual error.

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available, so this isn't a clearance.

**Diagram content (Figures 6–32):** I read the figure captions and cross-checked their claims against the surrounding text, but I did not visually inspect the rendered use-case diagrams themselves. If you want those checked (e.g., for consistency between the diagram actors/relationships and the table text), let me know and I'll rasterize and review the actual figures.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | "88 FRs / nine modules" vs. actual 87 FRs / eight modules (Dashboard is a phantom module) | High | Correct |
| 2 | "11 embedding models" overclaim, 4th occurrence | Medium | Rewrite |
| 3 | "26 use cases" claim | — | Keep, verified exact |
| 4 | Requirements traceability (73 references, 87 definitions) | — | Keep, fully consistent |
| 5 | Prose / AI-writing check | — | Keep, clean |
| 6 | "Support" actors not mentioned in actor count | Low | Optional clarification |

---

*Next: send Section 2.3 (System Architecture & Design) and/or 2.4 (Implementation) when you're ready, and I'll do the same pass.*

# Thesis Review — Part 2, Chapter 2, Section 2.3

**Scope of this file:** printed pages 77–94 (2.3 System Architecture & Design: System Overview, Domain-Driven Design, C4 Architecture, Database Design, API Design, Security Design)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. "Nine bounded contexts" contradicts "eight" stated everywhere else, including two sentences earlier

**Location:** §2.3.1, System Overview (p.77)

> "ReSys.Shop comprises three services... and **eight** bounded contexts using Domain-Driven Design with MediatR dispatch between modules.
> ...
> Internally, the backend is partitioned into **nine** bounded contexts, each owning a dedicated database schema."

**Problem:** These two sentences are three lines apart in the same subsection and contradict each other directly. The correct number is **eight**:
- Table 47 (right after the "nine" sentence) lists exactly 8 rows: Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location.
- §2.3.2 repeats "eight bounded contexts" three separate times.
- Figure 33's own caption says "eight business contexts."
- Table 48 in §2.3.2.1 also lists exactly 8 contexts.

So "nine" is an isolated, one-off error surrounded on all sides by "eight." This one is a simple, high-confidence fix.

**Fix:** Change "partitioned into nine bounded contexts" to "partitioned into eight bounded contexts."

---

## 🔴 CORRECT — 2. Two references to a "Section 2.1.5" that doesn't exist

**Locations:**
- §2.3.4.3, pgvector Integration (p.89): *"...with IVFFlat as a fallback for local environments (see Section 2.1.5 for index detail)."*
- §2.4 area (p.95, just past this section's boundary but worth flagging since it's the same broken reference): *"...sub-second CBIR queries (see Section 2.3.4 for index detail and Section 2.1.5..."*

**Problem:** Section 2.1 in this thesis only goes up to §2.1.3 (Feature Classification). There is no §2.1.4 or §2.1.5 anywhere in the document. This is the same category of error as the phantom "Chapter 6" reference found in the appendix during the earlier full-document pass, a cross-reference to a section that either got renumbered or deleted during editing and was never updated at the citing end. It appears twice, so it's not a one-off typo.

**Fix:** Find whatever content this was meant to point to (likely the HNSW/IVFFlat comparison in §1.4.3–1.4.4, or the benchmark protocol in §3.4.3) and correct both references to the actual section number.

---

## 🟠 REWRITE / RECONCILE — 3. PostgreSQL version is "17" everywhere except Chapter 3's hardware table, which says "16"

**Location:** this section states "PostgreSQL 17" three times (§2.3.1 Table 46, §2.3.3.2, §2.3.4 opening), and it recurs elsewhere in the thesis (Chapter 1 §1.5.4, Chapter 2 §2.4, Appendix D), seven occurrences total.

**Problem:** Chapter 3's Table 66 ("Hardware Environment," the actual benchmark setup) states: *"Database: PostgreSQL 16, pgvector 0.7.0."* Every other mention of the database version in the thesis, including this architecture section, says PostgreSQL 17. This isn't necessarily wrong, it's plausible the production/architecture design targets 17 while the benchmark environment happened to run on 16 during development, but as written there's no explanation for the discrepancy, and a reader has no way to tell whether it's an intentional version difference or a typo in one of the two places.

**Fix:** Either confirm the benchmark genuinely ran on PostgreSQL 16 and add a one-line note explaining why (e.g., "the benchmark environment used PostgreSQL 16 during initial development; the production schema targets PostgreSQL 17"), or, if it's simply a typo, correct Table 66 to say 17 so all eight mentions agree.

---

## 🟠 REWRITE — 4. Permission-string format is described two different ways

**Location:** §2.3.6.2, Dynamic Authorization (p.93)

> "Permissions use the format **Domain.Category.Resource.Action**:
> ```
> catalog.products.create
> catalog.variants.delete
> identity.roles.manage
> ```"

**Problem:** Two things don't line up here:
1. Earlier in the thesis (NFR-02b, §2.3.6.2's own neighboring text conventions elsewhere), the permission format is described as **`domain:category:action`**, colon-separated, three parts.
2. Here it's described as **`Domain.Category.Resource.Action`**, dot-separated, **four** parts, but the actual example strings immediately below it (`catalog.products.create`, `identity.roles.manage`) are dot-separated and only have **three** segments each (domain.resource.action), there's no separate "Category" segment visible in any example.

So the stated template doesn't match its own examples, and the separator character (colon vs. dot) is inconsistent with how the same concept is described elsewhere in the thesis. This is a small thing individually, but permission-string format is exactly the kind of implementation detail a committee member (or a future maintainer of the actual codebase) will try to verify against the code, and right now the document doesn't give a single consistent answer.

**Fix:** Pick one format and use it everywhere. Given the code examples are the ground truth (they presumably come directly from your implementation), the fix is likely to change the prose descriptions elsewhere in the thesis to `domain.resource.action` (dot-separated, three parts) and drop "Category" from the template sentence here.

---

## 🟢 KEEP — 5. "Approximately 262 Carter endpoints" — verified exact

**Location:** §2.3.5, API Design (p.91) and Table 50

I summed the "N" column in Table 50 directly: Catalog 80 + Identity 37 + Ordering 35 + Inventory 32 + Profile 27 + Location 18 + Payment 17 + Shipping 15 + Dashboard 1 = **262**, matching the stated "approximately 262" exactly. **No action needed.**

---

## 🟢 KEEP — 6. "Eleven inter-module contract DTOs" — verified exact

**Location:** §2.3.5.2 (p.91)

I counted the DTOs listed: 4 for Inventory (ReserveCartStock, ReleaseCartStockReservations, ConsumeCartStockReservations, CheckVariantAvailability) + 3 for Ordering (GetCartForCheckout, GetCartForShipping, AdvanceCheckoutState) + 2 for Payment (GetPaymentForCheckout, MarkPaymentPaid) + 2 for Catalog (GetVariantDiscontinuedStatuses, GetVariantWeights) = **11**. Matches exactly. **No action needed.**

---

## 🟢 KEEP — 7. Dashboard module clarified, not a phantom feature

**Note tying back to the previous review file (2.1–2.2):** that earlier pass flagged "Dashboard" as a module named in the FR summary count but with zero functional requirements documented anywhere. This section's Table 50 shows Dashboard *does* exist as a real feature, with exactly 1 API endpoint ("Aggregated metrics: sales, inventory, catalog, activity"). So Dashboard isn't fabricated, it's a genuine, minimal part of the system. The correction still stands as written in the earlier file (§2.1's "88 requirements / nine modules" opening line needs fixing), but it's worth knowing Dashboard itself is real; it's specifically the *functional requirements table* for it that's missing, not the feature.

---

## 🟢 KEEP — 8. AI-writing pattern check

Same clean result as every prior chapter. No LLM-tell vocabulary, no stray em dashes in prose (zero found in this section at all). The architecture description is dense with specific implementation detail (actual C# code snippets, actual permission strings, actual table/column names), which is a strong positive signal, this reads like it was written by someone who actually built the system, not generated at a distance from it.

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

**Figures 33–39 (diagrams):** captions cross-checked against the surrounding text; the rendered diagrams themselves weren't visually inspected. Say the word if you want those rasterized and reviewed directly.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | "Nine bounded contexts" contradicts "eight" stated 5+ times nearby | High | Correct |
| 2 | Phantom "Section 2.1.5" cross-reference, appears twice | High | Correct |
| 3 | PostgreSQL 17 (7 mentions) vs. PostgreSQL 16 (Chapter 3 hardware table) | Medium | Reconcile |
| 4 | Permission-string format stated two different ways | Medium | Rewrite for consistency |
| 5 | "262 Carter endpoints" | — | Keep, verified exact |
| 6 | "Eleven inter-module DTOs" | — | Keep, verified exact |
| 7 | Dashboard module is real, just missing an FR table (see prior file) | — | Keep, context note |
| 8 | Prose / AI-writing check | — | Keep, clean |

---

*Next: send Section 2.4 (Implementation) when you're ready, and I'll do the same pass on it.*

# Thesis Review — Part 2, Chapter 2, Section 2.4

**Scope of this file:** printed pages 94–107 (2.4 Implementation: Technology Stack, Vertical Slice Architecture Core, Data Persistence, ML Sidecar and CBIR Search, Frontend Applications)
**Checked for:** AI-writing patterns, internal-consistency / hallucination, citation accuracy, plagiarism risk

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. The actual model registry has six models, not eleven — this resolves the "11 models" question raised in every earlier chapter

**Location:** Table 55, §2.4.4.1 Model Management (p.98)

> "**Six models span four architectures**, selected from a decorator-based registry on first inference:
> fashion_clip, clip_vit_b16, openclip-vit-b-32, efficientnet_b0, resnet50, dinov2_vits14."

**Why this matters:** Every prior chapter review in this series flagged the recurring claim that the framework "supports eleven models" (Part 1 §V, Chapter 1 §1.3.4.1/§1.6.3, Chapter 2 §2.1.3 Table 19), always framed as "four representative models selected from the eleven supported." I treated that as an overclaim before because Chapter 3's benchmark only reports four models in detail. **This table is the actual, concrete, code-level model registry, the ground truth for what the system supports, and it lists six, not eleven.**

So the "eleven" figure doesn't match any real artifact in the thesis: not the benchmark's four reported models, and now confirmably not the implementation's six registered models either. This changes my earlier recommendation: this isn't just an overclaim to soften, it's a number that should be corrected to match reality everywhere it appears.

**Fix:** Go through every occurrence of "eleven models" (there are at least four across Parts 1–2) and change it to reflect the real number, six, unless there's a separate, larger benchmark-only model list documented somewhere that I haven't seen yet (in which case, that list needs to be shown explicitly, e.g. as an appendix table, so the "eleven" claim has something to point to). As it stands, the number appears to have no basis anywhere in the actual thesis content.

---

## 🔴 CORRECT — 2. Visual search UI: "four-state model" vs. "five states," in the same subsection

**Location:** §2.4.5.2.1, Visual Search (p.102)

> "The visual search interface implements a **four-state UI model**:" [Table 58 lists exactly four rows: Empty, Upload, Loading, Results]
> ...
> "The **five** visual search states are illustrated below."

**Problem:** The subsection opens by declaring four states and Table 58 backs that up with exactly four rows. Two paragraphs later, the closing sentence says five. This is a same-page, same-subsection contradiction, likely a leftover from an earlier draft where a fifth state (perhaps an "Error" state, given the CBIR flow elsewhere mentions ML-service-unavailable and invalid-format error paths) was removed from the table but not from this sentence.

**Fix:** Either add the missing fifth state to Table 58 if it's meant to exist (an Error state seems like the obvious candidate given UC-STR-SRC's alternative flows A1/A4 in §2.2.4.4), or change "five" to "four" to match the table as it stands.

---

## 🔴 CORRECT — 3. The CBIR search endpoint is given three different, mutually inconsistent URLs

**Locations:**
- §2.4.4.3, step 1 (p.101): *"Dispatches multipart form to POST **/api/admin/catalog/storefront/search-by-image**."*
- §2.4.5.1 (p.102): *"...dispatching POST **/api/storefront/search-by-image**..."*
- Declared convention, §2.3.5.2 (p.91): *"Endpoints follow the convention **/api/{module}/{surface}/{resource}**, where surface is storefront or admin."*

**Problem:** These are three different paths for what is described as the same feature (uploading an image to search visually):
1. The first version has **four** path segments and includes both "admin" and "storefront" simultaneously, which shouldn't be possible under the stated convention (a route is either admin-surface or storefront-surface, not both).
2. The second version has only **two** segments, it's missing the `{module}` segment (`catalog`) entirely.
3. Neither matches the declared three-segment pattern (`/api/catalog/storefront/search-by-image` would be the version that actually fits the rule).

This also isn't isolated to just this one endpoint: the other concrete example routes shown in §2.4.5.2 (`GET /api/storefront/payment/methods`, `POST /api/storefront/payment/create-intent`) all put "storefront" as the **first** segment, which is the reverse ordering from the declared convention (`{module}/{surface}/{resource}` says module comes first, surface second). So the mismatch between the stated rule and the worked examples looks systemic, not a one-off typo.

**Fix:** Decide which ordering is actually implemented in the codebase (check your Carter route definitions directly), then make every stated example and every prose description of the convention consistent with it. Given the actual code is the source of truth here, this is a quick grep-and-fix in the thesis text once you confirm the real route pattern.

---

## 🟠 REWRITE — 4. pgvector version disagreement

**Location:** Table 51, §2.4.1 Technology Stack (p.94)

> "ORM and Database: EF Core 10.0.9, Npgsql 10.0.2, **pgvector 0.3.2**"

**Problem:** This is the thesis's dedicated "pinned version specifications" table, presumably the most authoritative version listing in the document. But Chapter 3's test environment and hardware descriptions both say **pgvector 0.7.0** (§3.2 test setup and Table 66 Hardware Environment). Table 51 is the only place in the thesis that says 0.3.2. Given it's the "pinned versions" table, this is the one most likely to get copied verbatim by anyone trying to reproduce your setup, so it's worth getting right. This sits alongside the PostgreSQL 16-vs-17 mismatch already flagged in the §2.3 review, both appear to stem from the same technology-stack table not being kept in sync with what Chapter 3 actually describes running.

**Fix:** Confirm the actual pgvector version used (check your `Directory.Packages.props` or actual `pg_extension` output) and make Table 51 and Chapter 3 agree.

---

## 🟠 REWRITE — 5. "Eight use cases" undercounts the storefront coverage actually documented

**Location:** §2.4.5.2, Storefront Interfaces (p.102)

> "The customer storefront implements **eight use cases** covering product discovery, purchasing, and account management."

**Problem:** The subsections that follow (§2.4.5.2.1 through §2.4.5.2.8) document exactly **nine** distinct storefront use case IDs: UC-STR-SRC, BRW, CRT, CHK, OHI, AUT, SES, PAY, and PRF (Profile Management, §2.4.5.2.8). All nine match the full storefront use case set defined back in §2.2.4. This is a simple off-by-one in the summary sentence, the content itself is complete and correctly covers all nine, it's just the introductory count that's wrong.

By contrast, I checked the equivalent claim for the admin side, "the administration dashboard implements fifteen administrative use cases" (§2.4.5.3), by counting IDs across all its subsections (5+2+2+2+2+1+1 = 15), and that one is exactly right.

**Fix:** Change "eight use cases" to "nine use cases" in §2.4.5.2's opening sentence.

---

## 🟡 CUT / CORRECT — 6. Minor cross-reference imprecision

**Location:** §2.4.5, opening (p.102): *"...organized by the 26 use cases defined in **Section 2.2.2**."*

**Problem:** §2.2.2 is "Functional Decomposition" (the work-breakdown structure), not where the 26 use cases are individually defined, those are spread across §2.2.3 (Administrator), §2.2.4 (Customer), and §2.2.5 (System). Minor, but worth fixing since it's a specific section pointer a reader might actually follow.

**Fix:** Change "Section 2.2.2" to "Section 2.2" (or "Sections 2.2.3–2.2.5" if you want to be precise).

---

## 🟢 KEEP — 7. "Six containerized resources" reconciles correctly

**Location:** §2.4.1.1 (p.95): *"The platform defines six containerized resources with startup dependencies..."*

This resolves the ambiguity I flagged as a minor note in the §2.3 review (where §2.3.3.2 said "six standalone deployable processes" but only listed five bullet points). Here it's explicit: PostgreSQL, Redis, the Python ML sidecar, and the .NET API are named individually (4), plus the two Vue SPAs (Store and Admin) make 6. Consistent with the earlier section once you count the two SPAs separately. **No further action needed**, this was already resolved by reading further into the document.

---

## 🟢 KEEP — 8. "Fifteen administrative use cases" — verified exact

I counted the use case IDs across all seven admin subsections (§2.4.5.3.1–2.4.5.3.7): Product Management (5: PROD, VAR, IMG, TAX, OPT) + Order Management (2: ORD, ORD-ITEMS) + Payment Management (2: PAY, PAY-METHOD) + Inventory Management (2: STK, LOC) + User/Role Administration (2: USR, ROL) + Shipping Configuration (1: SHP) + Reference Data (1: REF) = **15**, matching exactly. **No action needed.**

---

## 🟢 KEEP — 9. AI-writing pattern check

Same clean result as every chapter so far. No LLM-tell vocabulary, zero stray em dashes in this ~700-line section. The implementation walkthrough includes real code snippets, real response JSON, and specific UI-state tables, this reads as authored directly from the working system, which is exactly what you'd expect from an implementation chapter written by the person who built it.

---

## Not checked in this pass

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

**Figure 41 (CBIR search sequence diagram):** caption cross-checked against the six-stage flow described in prose; the rendered figure itself wasn't visually inspected.

---

## Summary table

| # | Item | Severity | Action |
|---|------|----------|--------|
| 1 | Model registry (Table 55) shows 6 models, not 11 — resolves the recurring "11 models" question | High | Correct everywhere "eleven models" appears |
| 2 | "Four-state" vs. "five states" contradiction, same subsection | High | Correct |
| 3 | CBIR endpoint given 3 different, inconsistent URLs | High | Correct, verify against actual routes |
| 4 | pgvector 0.3.2 (Table 51) vs. 0.7.0 (Chapter 3) | Medium | Reconcile |
| 5 | "Eight use cases" undercounts the 9 actually documented | Medium | Rewrite |
| 6 | "Section 2.2.2" cross-reference should be "Section 2.2" | Low | Correct |
| 7 | "Six containerized resources" | — | Keep, reconciles correctly |
| 8 | "Fifteen administrative use cases" | — | Keep, verified exact |
| 9 | Prose / AI-writing check | — | Keep, clean |

---

*This closes out Chapter 2 (2.1 through 2.4). Send Chapter 3 (Testing and Evaluation) when you're ready and I'll do the same pass, note that Chapter 3 is where several of the numbers flagged in earlier chapters (the 5.4%/7.7%/model-count figures) actually live, so it's worth treating as the source of truth once we get there and back-propagating any final corrections.*

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

# Thesis Review — References List

**Scope of this file:** printed pages 124–126, all 28 numbered references. I checked every reference that names a specific academic paper (the ones most likely to have been recalled from memory rather than copied from a source) against the real published record. Documentation links (Microsoft, Vue, Redis, Hangfire, pgvector GitHub, martinfowler.com, jimmybogard.com) and well-known standards (RFC 7519) weren't individually re-verified, they're low-risk, easily checkable by URL, and not the kind of reference that gets hallucinated.

Legend: 🔴 CORRECT (factual error, must fix) · 🟠 REWRITE (minor inaccuracy) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — [6] Fashion-CLIP citation is fabricated

**As written in the thesis:**
> A. Chia, S. Gieysztor, and others, "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge," in *Proceedings of the 45th International ACM SIGIR Conference on Research and Development in Information Retrieval (SIGIR)*, 2022.

**What's actually real:** the Fashion-CLIP paper is Chia, Attanasio, Bianchi, Terragni, Magalhães, Goncalves, Greco, and Tagliabue, **"Contrastive language and vision learning of general fashion concepts,"** published in ***Scientific Reports*** (Nature), vol. 12, 2022. Wrong title, wrong venue (it was never at SIGIR), and "S. Gieysztor" isn't a real co-author on the actual paper.

**Corrected entry:**
> P. J. Chia, G. Attanasio, F. Bianchi, S. Terragni, A. R. Magalhães, D. Goncalves, C. Greco, and J. Tagliabue, "Contrastive language and vision learning of general fashion concepts," *Scientific Reports*, vol. 12, article 18958, 2022. doi: 10.1038/s41598-022-23052-9

This reference is cited three times in the thesis (§1.3.3.5, §1.3.4.4, §1.6.1), so all three citing locations should be double-checked once the entry is fixed.

---

## 🔴 CORRECT — [27] Fashion IQ citation has a wrong author name, title, and venue

**As written in the thesis:**
> H. Wu, Y. Gao, X. Guo, Z. Al-Zahir, and others, "Fashion IQ: A New Dataset Towards Natural Language Guided Retrieval," in *Proceedings of the IEEE International Conference on Computer Vision (ICCV)*, 2019.

**What's actually real:** the fourth author is **Ziad Al-Halah**, not "Al-Zahir." The actual title is **"Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback."** It began as a 2019 arXiv preprint and was formally published at **CVPR 2021**, not ICCV 2019. Full author list: Hui Wu, Yupeng Gao, Xiaoxiao Guo, Ziad Al-Halah, Steven Rennie, Kristen Grauman, Rogerio Feris.

**Corrected entry:**
> H. Wu, Y. Gao, X. Guo, Z. Al-Halah, S. Rennie, K. Grauman, and R. Feris, "Fashion IQ: A New Dataset Towards Retrieving Images by Natural Language Feedback," in *Proceedings of the IEEE/CVF Conference on Computer Vision and Pattern Recognition (CVPR)*, 2021, pp. 11307–11317.

---

## 🟠 REWRITE — [26] DeepFashion citation drops a co-author

**As written in the thesis:**
> Z. Liu, P. Luo, X. Wang, and X. Tang, "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations," CVPR 2016, pp. 1096–1104.

**Problem:** the real author list is Ziwei Liu, Ping Luo, **Shi Qiu**, Xiaogang Wang, Xiaoou Tang, **five** authors, not four. "Shi Qiu" is missing entirely from the thesis's version. Everything else (title, venue, year, page range 1096–1104) is correct, this is a smaller error than [6] or [27], a dropped name rather than a fabricated title or venue, but still worth fixing since it's a factual inaccuracy in an otherwise-correct entry.

**Corrected entry:**
> Z. Liu, P. Luo, S. Qiu, X. Wang, and X. Tang, "DeepFashion: Powering Robust Clothes Recognition and Retrieval with Rich Annotations," in *Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition (CVPR)*, 2016, pp. 1096–1104.

---

## 🟢 KEEP — Verified correct

I checked the following against the published record and confirmed title, authors, venue, year, and page numbers all match exactly:

- **[3]** He, Zhang, Ren, Sun, "Deep Residual Learning for Image Recognition," CVPR 2016, pp. 770–778. ✓
- **[4]** Tan and Le, "EfficientNet: Rethinking Model Scaling for Convolutional Neural Networks," ICML 2019, pp. 6105–6114. ✓
- **[5]** Radford et al., "Learning Transferable Visual Models From Natural Language Supervision," ICML 2021, pp. 8748–8763. ✓ ("A. Radford et al." is standard shorthand for a 12-author paper, appropriate here.)
- **[10]** Dosovitskiy et al., "An Image is Worth 16x16 Words: Transformers for Image Recognition at Scale," ICLR 2021. ✓ (also a many-author paper correctly abbreviated as "et al.")
- **[12]** Malkov and Yashunin, "Efficient and Robust Approximate Nearest Neighbor Search Using Hierarchical Navigable Small World Graphs," IEEE TPAMI, vol. 42, no. 4, pp. 824–836, 2018. ✓ (checked earlier in this review series)

No action needed on any of these five.

---

## Not individually re-verified

The remaining 18 references fall into categories I judged lower-risk and didn't spend search budget re-confirming one by one:

- **Documentation/product references** ([13] pgvector GitHub, [16] martinfowler.com, [17] jimmybogard.com, [19] ASP.NET Core docs, [20] EF Core docs, [21] Vue.js docs, [22] Redis docs, [24] Hangfire docs): these are URLs to living documentation sites, easy for you to click and confirm directly, and not the type of source that gets hallucinated (there's no "real" paper to misattribute).
- **Books** ([14] Shaw & Garlan, [15] Newman, [28] Manning/Raghavan/Schütze): well-known, widely-cited textbooks, low risk of fabrication.
- **Standards** ([25] RFC 7519 JWT): a formal, numbered IETF standard, trivially verifiable and low risk.
- **Dataset/tooling sources** ([7] Aggarwal Kaggle dataset, [23] PyTorch NeurIPS 2019): plausible and specific enough that I'd expect them to check out, but I didn't independently confirm.
- **Academic methodology papers** ([8] Hevner et al., [9] Peffers et al., [11] Oquab et al. DINOv2, [18] Greg Young CQRS): foundational/well-known works in their respective fields, lower risk than the fashion-specific ML papers (which is exactly where the two confirmed errors were found).

Given that both confirmed fabrications ([6], [27]) were in the same category (fashion-domain ML papers cited to support specific numeric claims), and the DeepFashion error ([26]) was in the same category too, that category is the one I'd prioritize if you want to extend this check yourself: any reference supporting a specific number or being used as authority for a specific technique in Chapters 1–3.

---

## Summary table

| Ref | Item | Severity | Action |
|---|---|---|---|
| [6] | Fashion-CLIP: wrong title, wrong venue, fabricated co-author | High | Correct, cited 3x in the thesis |
| [27] | Fashion IQ: wrong author name, wrong title, wrong venue | High | Correct |
| [26] | DeepFashion: missing co-author (Shi Qiu) | Low | Correct |
| [3],[4],[5],[10],[12] | Verified correct | — | Keep |
| Remaining 18 | Not individually re-verified (docs, books, standards, lower-risk categories) | — | Spot-check if time allows, prioritize fashion/ML papers |

---

*This closes the References list. Remaining unreviewed material: Appendix B (Dataset Composition), Appendix C (Hardware Specifications), and Appendix D (Database Schema). Given the pattern found throughout this review, numbers not matching between sections, I'd suggest those three next, then a final consolidated priority list pulling every 🔴 finding across all seven files into one fix-it-in-order checklist. Let me know which you'd like.*

# Thesis Review — Appendices B, C, D

**Scope of this file:** printed pages 131–152 (Appendix B: Dataset Composition, Appendix C: Hardware Specifications, Appendix D: Database Schema, all 40 tables). This closes out the full-document pass.

Legend: 🔴 CORRECT (factual/structural error, must fix) · 🟠 REWRITE (imprecise, overclaiming, or unclear) · 🟡 CUT (redundant or unsupported, consider removing) · 🟢 KEEP (verified, no action)

---

## 🔴 CORRECT — 1. "Variable Vector Dimensions" is contradicted by the actual schema, shown four separate times

**Location:** the claim is in §2.3.4.4 (Chapter 2, "Key Design Decisions"), but the contradiction is fully visible once you look at Appendix D's concrete schema, so it belongs in this file.

> §2.3.4.4: *"Variable Vector Dimensions: pgvector columns support per-model dimensionalities: 384 (DINOv2-S), 512 (Fashion-CLIP), 768 (DINOv2-B), 1280 (EfficientNet-B0), 2048 (ResNet-50)."*

versus, stated identically in **four different places**:

> §2.3.4.3: *"stores feature vectors in an embedding column defined as **vector(512)**."*
> §2.4.3.2: *"Embeddings are stored in a **vector(512)** column with model-aware discriminators."*
> Appendix D, Table 82: *"vector(512); IVFFlat cosine index."*
> Appendix D.9: *"Vector embeddings are stored in catalog.image_embeddings with column type **vector(512)**, made nullable in migration 20260804013350."*

**Why this is a hard contradiction, not just an inconsistency:** pgvector's `vector(N)` column type is fixed-width by definition, a column declared `vector(512)` can only physically store 512-dimensional vectors. It cannot also hold a DINOv2-S embedding (384 dimensions), a DINOv2-B embedding (768), an EfficientNet-B0 embedding (1280), or a ResNet-50 embedding (2048) in the same column. There's no truncation, padding, or per-model column strategy mentioned anywhere that would reconcile this. So the "Variable Vector Dimensions" bullet describes a capability the concrete, four-times-repeated schema doesn't actually have.

**Two possible explanations, both worth checking:**
1. The system only ever actually stores Fashion-CLIP embeddings (512-dim) in production, and the "variable dimensions" bullet describes an aspirational/future capability rather than what's implemented, in which case the bullet should say so explicitly.
2. There's a real per-model dimension-handling mechanism in the codebase (e.g., separate tables or columns per model) that simply wasn't captured correctly when this schema was documented, in which case Appendix D needs to show it.

**Fix:** Check the actual EF Core migration files and `IEntityTypeConfiguration<ImageEmbedding>` class directly. Either the "Variable Vector Dimensions" bullet needs to be removed/qualified, or the schema documentation (§2.3.4.3, §2.4.3.2, Table 82, D.9) needs to be corrected to show however dimension variability is actually handled.

---

## 🟠 REWRITE — 2. Appendix C's claim that Chapter 3 and Appendix A share "a single workstation" sharpens the earlier unresolved discrepancy

**Location:** Appendix C opening (p.133)

> "All benchmark results reported in Chapter 3 and Appendix A were collected on a single workstation."

**Why this matters here:** this is a direct, explicit assertion that Chapter 3's Table 67/68 and Appendix A's Tables 71–75 come from the **same** hardware, software environment, and (by implication) the same run. Every earlier file in this review series found that these two sets of numbers don't actually match (Table 67's mAP of 0.8788 vs. Appendix A.1's 0.9309, Table 68's latency figures vs. Appendix A.4's Table 74). This sentence removes the most charitable explanation for that mismatch, "maybe they ran on different hardware or at different times", and replaces it with an explicit claim that they're the same experiment. That makes the discrepancy harder to explain away, not easier: if it really was one workstation and one benchmark run, the two tables should be identical, and they aren't.

**Fix:** once the Table 67/68 vs. Appendix A reconciliation is done (see the Chapter 3 and Part 3 review files), come back to this sentence and either confirm it's accurate (single run, tables now match) or correct it to reflect however many actual runs were involved.

---

## 🟢 KEEP — 3. Software stack versions checked out as accurate for the stated timeframe

**Location:** Table 78, §C.2 (p.133): PyTorch 2.13.0, TorchVision 0.28.0, HuggingFace Transformers 5.14.1, OpenCLIP 3.3.0, NumPy 2.5.1

I checked PyTorch and Transformers against their actual release histories. **Both check out precisely**: PyTorch reached the 2.11–2.13 range by mid-2026 (confirmed via official release notes), and Hugging Face Transformers 5.14.1 was released July 16, 2026, exactly matching the version stated here. This is a good sign, these aren't generic-sounding placeholder version numbers, they're specific and turned out to be genuinely accurate, which suggests real care was taken pinning the actual environment used. **No action needed.**

---

## 🟠 REWRITE — 4. "Sequential" image selection paired with "preserves the natural category distribution" doesn't quite follow

**Location:** Appendix B.1 (p.131)

> "For the thesis evaluation, a controlled subset of 5,000 images was selected. Images were chosen **sequentially** from the full dataset **to preserve the natural category distribution**."

**Problem:** this is a methodological claim that doesn't obviously hold together. Selecting images sequentially (i.e., taking the first N records in file/index order) preserves the natural distribution *only if* the underlying dataset is already randomly shuffled. Many scraped e-commerce catalogue dumps are naturally grouped or sorted (by category, by upload batch, by product ID range), in which case sequential selection would *distort* the distribution toward whatever appears first in the listing, not preserve it. As written, the sentence asserts a conclusion ("preserves the distribution") without justifying the premise (that sequential order is representative).

This doesn't necessarily mean the actual 5,000-image sample is wrong, if the Kaggle dataset happens to already be shuffled, sequential selection would genuinely work, but the thesis doesn't say that, and a methodology-focused reader (exactly the kind of reader Chapter 3 and this appendix are written for) is likely to ask the question.

**Fix:** either confirm and state explicitly that the source dataset is pre-shuffled (making sequential selection valid), or switch to describing the actual selection method accurately if it was, for example, stratified random sampling instead of pure sequential order.

---

## 🟢 KEEP — 5. Bounded context count in Appendix D matches the corrected "eight," and reinforces PostgreSQL 17

**Location:** Appendix D opening (p.135)

> "The database uses **PostgreSQL 17** with pgvector via EF Core 10. Five migrations, 33 IEntityTypeConfiguration<T> classes across **eight** bounded contexts."

This is good confirming evidence for two things already flagged: it reaffirms "eight" bounded contexts (matching the correction made to the "nine" typo found in the Chapter 2 review), and it's the eighth mention of "PostgreSQL 17" in the thesis, further reinforcing that Chapter 3's Table 66 ("PostgreSQL 16") is the lone outlier that needs correcting, not the other way around. Appendix D.1 through D.8 map cleanly onto the eight named contexts (Catalog, Identity, Ordering, Payment, Inventory, Shipping, Profile, Location). **No action needed**, this section is consistent with the corrections already recommended elsewhere.

---

## 🟢 KEEP — 6. Inventory schema matches the described stock-reservation architecture

**Location:** Appendix D.5 (p.145–148), `stock_items`, `stock_reservations`, `stock_movements`, `stock_transfers`

These tables consistently reference "xmin concurrency" (PostgreSQL's built-in system column used for optimistic concurrency control), matching the row-versioning approach described in Chapter 2's architecture section. The reservation table (`stock_reservations`) includes an `expires_at_utc` auto-release timeout field, consistent with the 15-minute reservation timeout claim verified earlier in the Chapter 2 review. No inconsistencies found in this section. **No action needed.**

---

## 🟢 KEEP — 7. Phantom "Chapter 6" reference reappears, already flagged, noted here for completeness

**Location:** Appendix B.3 (p.132): *"Category + Colour labels... This is the primary relevance criterion used in Chapter 6."*

This is the same broken cross-reference already flagged in the very first full-document pass and again in the Chapter 3/Appendix A review (Appendix A.2's Table 72 caption uses nearly identical wording). It appears at least twice now across Appendices A and B. No new finding, just confirming it needs to be part of the same fix, most likely this should say "Chapter 3" throughout.

---

## Not checked in this pass

**Appendix D.1–D.8's remaining ~35 table definitions:** I spot-checked the Catalog and Inventory schemas in detail and skimmed the rest (Identity, Ordering, Payment, Shipping, Profile, Location) for structural consistency (foreign keys, cascade rules, naming conventions). Nothing else stood out as contradicting earlier chapters, but a full field-by-field audit against the actual EF Core migrations would be the only way to be fully certain, which is outside what I can verify from the thesis text alone.

**Plagiarism:** no verbatim matches found in the passages sampled; no institutional tool available.

---

## Summary table

| # | Item | Severity | Action |
|---|---|---|---|
| 1 | "Variable Vector Dimensions" claim contradicted by fixed `vector(512)` schema, shown 4x | High | Correct, verify against actual migrations |
| 2 | Appendix C's "single workstation" claim sharpens the unresolved Table 67/68 vs. Appendix A discrepancy | Medium | Resolve alongside Chapter 3 finding |
| 3 | Software stack versions (PyTorch 2.13.0, Transformers 5.14.1) | — | Keep, verified accurate |
| 4 | "Sequential" selection claimed to "preserve natural distribution" | Medium | Rewrite / clarify |
| 5 | Bounded context count (8) and PostgreSQL 17 reaffirmed | — | Keep, consistent with prior corrections |
| 6 | Inventory schema matches described architecture | — | Keep |
| 7 | Phantom "Chapter 6" reference, 2nd occurrence | — | Keep (fix alongside prior instance) |

---

*This completes the full-document review: Part 1, Chapter 1, Chapter 2 (2.1–2.4), Chapter 3, Part 3, the References list, and Appendices B–D are all covered across eight files now. The natural next step is a consolidated priority list pulling every 🔴 finding across all eight files into one fix-it-in-order checklist, since several items (the "eleven models" figure, the PostgreSQL version, the Table 67/68 vs. Appendix A reconciliation, "Chapter 6") recur across multiple files and are easiest to fix once, together. Let me know if you'd like that.*
