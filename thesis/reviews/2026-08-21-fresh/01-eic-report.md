# Reviewer 1 — Editor-in-Chief Report

**Persona:** Editor of a mid-tier software-engineering / e-commerce systems venue (calibrated to capstone/entry-conference expectations).
**Paper:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Decision tendency:** Major Revision (not reject — solid capstone with fixable credibility gaps)

## 1. Journal / venue fit
Strong topical fit. The manuscript sits squarely at the intersection of applied computer vision, information retrieval, and practical e-commerce systems engineering — exactly the kind of build-and-evaluate work appropriate for an undergraduate capstone or an industry-track conference. The writing is clear, well-structured (DSR four-phase framing, three parts, C4/DDD diagrams), and the bilingual EN/VI presentation satisfies CTU's local format. No fit concern.

## 2. Originality
**Weak.** This is the manuscript's central vulnerability.
- The empirical core — "Fashion-CLIP outperforms generic CLIP by 2.13% mAP on fashion category retrieval" — is *not a finding of this work*. It is precisely what the Fashion-CLIP paper [6] already established. The thesis uses off-the-shelf, published models on a published dataset with no new model, dataset, training, or algorithm.
- The genuinely original element is the **engineering integration**: a polyglot .NET 10 ↔ Python FastAPI sidecar, pgvector-backed ACID-consistent embeddings, and a pluggable model-switching strategy. That is a legitimate capstone contribution and should be foregrounded — but it is currently buried under a benchmark narrative that implies scientific novelty it does not have.

## 3. Significance
Moderate for a bachelor thesis; limited for the research field. A practitioner team can take away "use Fashion-CLIP, it's marginally better, and here is a reference .NET integration." That is useful but not field-advancing. The significance claim in §3.7.4 ("production-viable visual search") overreaches given the evidence (see Methodology and Devil's Advocate reports).

## 4. Completeness
Good structural coverage of system + evaluation, but several **completeness gaps** that a committee will notice:
- **Title/scope promises "Recommendation" but delivers none.** Every "recommendation" mention is either the title string, a "Deployment Recommendations" subsection (which is deployment *advice*, not a recommender), or CBIR "similar products." There is no recommendation engine (no collaborative filtering, no personalization module). The eight bounded contexts are Catalog, Identity, Inventory, Ordering, Payment, Profile, Shipping, Location — **no Recommendation context.** Either implement a recommendation view or honestly reframe the title and scope (MAJOR).
- **DINOv2 is listed as an evaluated model (Table 64, §3.4.1) but has no results anywhere** (Fig 88 / Table 67 / Appendix A contain only 4 models). Table 64 also says "Four representative models" while showing 5 rows. Contradiction (MAJOR, see Domain/Methodology).
- **Model-count inconsistency:** "eleven models" (§1.3.4.1, §1.5.9) vs "six models" (§2.4.4.1 Table 55, line 2158, Part 3) vs "four benchmarked" (abstract, Ch3). A reader cannot tell how many models the framework supports or were evaluated (MAJOR factual).

## 5. Presentation
Generally strong: figures, tables, TOC, abbreviations all present. The prior headline number contradiction (Ch3 Table 67 vs Appendix A) has been **resolved** — both now read mAP 0.9309 consistently. Minor: some table text wraps awkwardly in extraction; the 300 ms vs <1 s latency target is used inconsistently (§1.3.4.3 vs §3.7.4).

## Scores (0–100, evidence-calibrated)
| Dimension | Score | Note |
|---|---|---|
| Originality | 42 | Confirmatory benchmark, no novel artifact |
| Significance | 55 | Useful capstone, not field-advancing |
| Technical soundness | 70 | Engineering solid; stats overclaimed |
| Presentation | 68 | Clean; minor target inconsistencies |
| Completeness | 60 | Recommendation gap; DINOv2/11-6-4 contradictions |

**Overall: 62 — Major Revision.** The work is salvageable and genuinely competent as an engineering case study; it must stop implying a scientific contribution it does not make, and must reconcile its own numbers.
