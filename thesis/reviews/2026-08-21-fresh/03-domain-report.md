# Reviewer 3 — Domain Report (Fashion-IR / E-Commerce)

**Persona:** Researcher in fashion image retrieval and e-commerce search systems.
**Paper:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Lens:** Literature coverage, theoretical framing, contribution to the field, related-work gap.

## Strengths
- The tutorial-style background (CNN/ViT/CLIP/Fashion-CLIP, ANN/HNSW/IVFFlat/pgvector) is accurate and well-cited with primary sources ([3] ResNet, [5] CLIP, [6] Fashion-CLIP, [10] ViT, [11] DINOv2, [12] HNSW, [13] pgvector). A committee will find the fundamentals sound.
- The decision to validate pgvector's transactional consistency for a real SME-scale catalogue is a sensible, defensible engineering choice and is well argued in §1.4.6.

## Issues

### [MAJOR] D1 — "Recommendation" in title and scope is not delivered
- **Where:** Title (every page header), Scope (line 995: "visual search … embedding-based recommendations"), Objectives (line 956).
- **What's missing:** There is **no recommendation system**. "Recommendation" appears only as (a) the title string, (b) §3.7.2 "Deployment Recommendations" (deployment *advice*, not a recommender), and (c) CBIR "similar products." The eight bounded contexts contain no Recommendation module; no collaborative filtering, no personalization, no "you may also like" beyond vector similarity.
- **Why it matters:** This is a scope misrepresentation. A committee member comparing the title to the content will immediately flag it.
- **Fix (choose one):** (a) Implement at least a lightweight recommendation surface (e.g., "similar products" already exists — generalize it into a recommendations module with session/vector-based suggestions) and describe it, **or** (b) reframe the title to *"…with Image-Based Product Search"* and narrow the scope text to remove the standalone "recommendation" promise, describing CBIR-similarity explicitly as the only personalization provided.

### [MAJOR] D2 — Model-count contradictions across the manuscript
- **Where:** "Eleven models … were evaluated" (§1.3.4.1, line 1489); "11 architectures" (§1.5.9, line 2004, Table 5 enumerates 11); "Six models span four architecture families" (§2.4.4.1 Table 55, line 6386); "evaluates six models" (line 2158); "Four representative models … selected from the six supported" (§3.4.1); Part 3 (line 8046) "four models and six supported architectures."
- **Why it matters:** The framework *candidate* set is 11 (Table 5), the *registry* is 6 (Table 55), and the *benchmarked* set is 4. The manuscript never states these as nested subsets, and several sentences say "were evaluated" when only 4 were. A domain reader cannot trust any quantitative claim once the counts disagree with each other.
- **Fix:** Define the three tiers explicitly once (candidate registry = 11; lazily-loadable in the running service = 6 per Table 55; empirically benchmarked in Ch3 = 4) and sweep every "eleven/six/evaluated" phrase to match.

### [MAJOR] D3 — DINOv2 benchmarked in name only
- **Where:** Table 64 lists DINOv2 ViT-S/14; §1.3.2.4 sells DINOv2's "structural fidelity." Yet no DINOv2 results appear in Ch3 or Appendix A.
- **Why it matters:** If structural fidelity is a differentiator, its absence from the benchmark is a real gap; if it was never run, listing it as an evaluated model is misleading (see Methodology M1).
- **Fix:** Add DINOv2 results or remove it from the evaluated set and soften the structural-fidelity sales pitch to "candidate, not benchmarked."

### [MODERATE] D4 — Contribution framing overstates novelty
- Contribution 1 (Part 3, line 8050) "A four-model benchmark for fashion image retrieval" is a *comparative study of published models*, not a contribution to knowledge. The field contribution is the **architecture pattern** (sidecar, pgvector ACID, pluggable switching). Frame the five contributions honestly: the benchmark is a *demonstration/illustration*, not a novel result.
- The "domain-specific pre-training provides measurable advantages" finding merely re-confirms [6]; state it as *validation/illustration*, not discovery.

### [MINOR] D5 — Related work under-covers e-commerce CBIR specifically
- §1.6 mentions DeepFashion [26] and FashionIQ [27] and commercial systems, but does not engage with applied CBIR-for-shopping literature (e.g., industrial visual-search papers, evaluation methodologies for fashion retrieval). A few targeted citations would sharpen the "engineering gap" the thesis claims to fill.

### [MINOR] D6 — Future work ignores the recommendation gap
- Future Work (Part 3 IV) does not mention implementing the recommendation capability the title promises, nor multi-modal/hybrid query evaluation (text+image, which §1.3.3.4 touts but never benchmarks).

## Domain-fit verdict
The topic is well-chosen and the fundamentals are correct. The damage is in **scope honesty** (D1) and **number reconciliation** (D2/D3) — both easy to fix and both currently credibility-threatening.

| Dimension | Score |
|---|---|
| Literature coverage | 65 |
| Contribution clarity | 50 |
| Domain fit | 72 |
| Related-work gap | 60 |
