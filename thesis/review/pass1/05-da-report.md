# Devil's Advocate Report — Pass 1 (Introduction + Background)

**Reviewer:** Prof. (Emeritus) Dr. Arthur Kowalski, former Chair of Computer Science, TU Dresden
**Role:** Devil's Advocate
**Review Date:** 30 July 2026

---

## Strongest Counter-Argument

The thesis stakes its claim on a contribution that is "architectural, not algorithmic" — an engineering demonstration of embedding existing models into a conventional web stack. This framing contains a fatal tension. If the architecture is genuinely novel, the thesis must explain why the Python-sidecar-to-.NET pattern, which has been standard industry practice since at least 2018 (thousands of production systems route ML inference through REST sidecars to JVM/CLR backends), constitutes a contribution warranting academic credit. The "polyglot" argument is further undermined by the fact that FastAPI calling PyTorch behind an HTTP boundary is precisely the default pattern recommended in FastAPI's own documentation and PyTorch's serving guides. The student has followed a well-trodden path and is calling it a contribution.

If, alternatively, the architecture is accepted as routine and the real contribution is the *empirical benchmark data* from Chapter 3, then the entire architectural narrative in Chapters 1 and 2 is scaffolding around a model comparison study — and a modest one at that, run on a single CPU, on 5,000 images, using "same category" as a proxy for visual similarity. A bachelor's thesis should demonstrate competence; the question at defense is whether it demonstrates anything beyond the ability to wire together existing libraries. The student must articulate, in one sentence, what new knowledge a practitioner gains from this work that they could not obtain from reading FastAPI's tutorial, pgvector's README, and the Fashion-CLIP paper.

---

## Issue List

### CRITICAL Issues

**C1. Dimension: Methodology — Relevance criterion conflates classification with retrieval**
- **Location:** Section 1.4.2 (Evaluation Methodology): "A retrieved product was considered relevant if it belonged to the same category as the query image."
- **Description:** The thesis measures retrieval accuracy by whether retrieved products share a *category label* with the query. This is a classification proxy, not a visual similarity measure. Within the "dresses" category, a model could return a wedding gown for a sundress query, score 100% precision by this metric, and be completely useless to the user. The evaluation therefore measures category-level discrimination, which even the shallowest CNN achieves trivially. The reported mAP@10 scores may be artificially inflated and mask failures in within-category visual differentiation — precisely where visual search adds value over a category filter dropdown.
- **Why it matters for defense:** If the evaluation metric doesn't measure what the system claims to do (find *visually similar* products), the entire empirical contribution collapses. An examiner will ask: "If you're just classifying by category, why not use a category filter?"

**C2. Dimension: Logical coherence — Cold-start argument confuses retrieval with recommendation**
- **Location:** Section 1.1 (Problem Statement): "Recommendation models based on collaborative filtering depend on historical user-item interactions... Visual feature extraction bypasses this limitation."
- **Description:** The cold-start problem in recommender systems is about predicting *user preference* — which items a specific user will want to purchase — without interaction history. Visual similarity, computed by an embedding model, does not predict preference. A visually similar blouse may be in a different price tier, from a brand the user dislikes, or in a style the user finds unflattering. The thesis treats "can be found" (retrieval) as equivalent to "would be recommended" (personalisation), but these are distinct problems with distinct solutions.
- **Why it matters for defense:** This conflation appears in the problem statement, meaning the thesis's motivational foundation rests on a category error. If the core problem is misidentified, the entire design rationale is suspect.

**C3. Dimension: Evidence — Benchmark data referenced but not presented**
- **Location:** Multiple locations: "The full evaluation protocol, benchmark results, and cross-validation methodology are presented in Chapter 3."
- **Description:** The entire model selection justification (Section 1.4.2) asserts that Fashion-CLIP achieved the highest mAP@10 with 15-20% improvement, that EfficientNet-B0 was 3.4% behind, and that these results "were confirmed through the systematic benchmark." But Chapter 3 is not available for review. The Background chapter *asserts* conclusions whose evidence is deferred. This creates a forward-reference dependency that prevents independent verification.
- **Why it matters for defense:** The Background chapter claims to justify a model selection, but reads as a conclusion chapter that has jumped the gun. If Chapter 3's data contradicts or weakens these assertions, the entire architectural foundation (built on the Fashion-CLIP decision) is retrospectively unsound.

### MAJOR Issues

**M1. Dimension: Architecture — Polyglot sidecar pattern lacks novelty analysis**
- **Location:** Section 1.5.1 (Architectural Decision): "Python sidecar as the only cross-process component."
- **Description:** The thesis presents the Python sidecar as a key architectural contribution, but never acknowledges that this is the default deployment pattern for ML-in-enterprise systems. A defense examiner would expect comparison with alternatives: ONNX Runtime within .NET (avoiding the sidecar entirely), gRPC-based serving with TorchServe, or embedding-as-a-service via managed APIs. The absence of this comparative analysis means the "architectural contribution" is asserted rather than argued.
- **Why it matters:** The thesis's stated differentiator is architectural. If the architecture is conventional, the thesis must fall back on benchmark data as its only contribution.

**M2. Dimension: Argument — pgvector advantages are overstated**
- **Location:** Section 1.4.4: "The critical advantage is transactional consistency. Vectors and product metadata share the same ACID boundary."
- **Description:** This argument has three weaknesses. First, embedding generation is *asynchronous* via Hangfire — the "atomic" update is already broken by the queue. Second, visual search is read-heavy; eventual consistency would produce functionally identical results. Third, the comparison table omits that Milvus, Weaviate, and Qdrant are open-source and free, creating a false dichotomy.
- **Why it matters:** pgvector's selection is reasonable, but the justification is oversold. Acknowledge the decision was primarily about simplicity and development velocity.

**M3. Dimension: Evaluation scope — Hardware limits undermine generalisation**
- **Location:** Section 1.1 (Known Limitations): "Experiments ran on consumer-grade hardware (Intel i7-1165G7, 16 GB RAM) with all inference executed on CPU."
- **Description:** The thesis makes deployment recommendations based on CPU-only benchmarks. But the relative performance ranking of models changes dramatically between CPU and GPU: transformers benefit disproportionately from GPU parallelism. The stated limitation that "results may not extrapolate" is actually stronger than acknowledged: the ranking almost certainly *would not hold* on GPU.
- **Why it matters:** If the benchmark cannot guide GPU deployments and cannot scale beyond 5,000 items, what deployment scenario does it actually inform? The intersection of "CPU-only, <10K products, requires visual search" is narrow and arguably contrived.

**M4. Dimension: Structure — Model selection appears in Background, not Design or Evaluation**
- **Location:** Section 1.4.2 (Model Selection and Justification).
- **Description:** The Background chapter performs the model selection decision — including weighing quantitative criteria — before the reader has seen the evaluation. This structurally conflates Background (what others have done) with Design (what this project chose) and Evaluation (whether it was right).
- **Why it matters:** This suggests the thesis is organised around justifying a foregone conclusion rather than presenting a genuine investigation.

### MINOR Issues

**M5. Dimension: Citation quality — Pinterest as e-commerce search abandonment source**
- **Location:** Section 1.1: "Industry estimates place the session abandonment rate after an unsuccessful search at approximately 30 percent @pinterest2023visual."
- **Description:** Pinterest is a social media platform, not an e-commerce site. Citing Pinterest for e-commerce search abandonment is a category mismatch. Use a retail-specific source (Baymard Institute, Google/BCG studies).

**M6. Dimension: Model count inconsistency**
- **Location:** Compare Section 1.4.2 ("Eleven pre-trained models") with candidate table (shows 10). ResNet-152 in benchmark section but absent from selection table.
- **Description:** The thesis repeatedly claims 11 models, but the table shows 10. Either the count is wrong or the table is incomplete.

**M7. Dimension: Terminology — "Real-time" is used inconsistently**
- **Location:** "real-time latency constraints" vs. "interactive latency bounds" vs. "sub-300 ms total response time."
- **Description:** The system describes itself as "real-time" but embedding generation for new products is asynchronous via Hangfire. Search *query* may be sub-300ms, but newly uploaded products are invisible until a background job completes. This is not a real-time system.

---

## Ignored Alternative Explanations/Paths

1. **ONNX Runtime within .NET.** Microsoft's ONNX Runtime has first-class .NET bindings. ResNet-50, EfficientNet, and even ViT models can be exported to ONNX and run in-process without Python. This eliminates the network boundary, the polyglot complexity, the async embedding queue, and the entire sidecar justification. The thesis never addresses this.

2. **Managed embedding APIs.** For sub-10K product catalogs, a managed service (OpenAI Embeddings, Vertex AI) would eliminate all infrastructure complexity at under $20/month. No cost-benefit comparison provided.

3. **Category-based retrieval as a baseline.** Since "same category" is the relevance criterion, a random-category baseline would already score well. The thesis should include trivial baselines to establish the *marginal* value of deep embeddings.

4. **Dataset size as an independent variable.** Subsampling at multiple sizes to characterise *how* performance degrades with scale would provide genuine insight into pgvector scaling behaviour.

---

## Missing Stakeholder Perspectives

1. **The DevOps/SRE engineer.** The embedding queue (Hangfire) already introduces inconsistency: product data is committed while embeddings are pending. The practical difference between "out-of-sync because two databases" and "out-of-sync because an async queue" is invisible to the end user.

2. **The small retailer.** A small retailer with 5,000 products and no ML engineer cannot maintain a FastAPI sidecar with PyTorch dependency management and GPU driver compatibility. Total cost of ownership (staff time, not cloud bills) makes this impractical.

3. **The user experience researcher.** The thesis acknowledges that "no formal user experience study was conducted." But without UX data, the thesis cannot claim the system is "effective" for its intended use.

4. **The fashion domain expert.** Fashion similarity is multi-faceted: silhouette, fabric, colour, pattern, occasion, brand aesthetic, trend alignment. Category-label evaluation cannot distinguish which attributes the model is actually matching.

---

## Logical Fallacies Detected

1. **False equivalence: visual similarity = recommendation quality.** The cold-start argument equates "can be discovered by visual search" with "the cold-start problem is solved." *Category error.*

2. **Cherry-picked comparison: pgvector vs. vector DBs.** The comparison table contrasts pgvector with "Specialised Vector DB" as a monolithic category, listing disadvantages without naming alternatives. Milvus, Weaviate, Qdrant are open-source and free. *Strawman framing.*

3. **Begging the question: model selection as evaluation preview.** Section 1.4.2 asserts Fashion-CLIP's superiority based on benchmark results from Chapter 3. *The conclusion is smuggled into the premises.*

4. **False dichotomy: "proprietary/commercial" vs. "open-source self-built."** The related work section presents commercial visual search as "proprietary, cannot be studied" and implies the only alternative is building from open-source components. Ignores ONNX-based in-process deployment and managed open-source services.

5. **Overgeneralisation from insufficient data.** Model selection based on a 5,000-image, single-dataset, CPU-only, category-label evaluation is then projected onto "the target deployment scenario" as if results generalise.

---

## Observations (Non-Defects)

1. **The contribution slide is well-signposted.** The student repeatedly and honestly states the contribution is "architectural, not algorithmic." Under questioning, this is defensible for a bachelor's thesis.

2. **The async embedding pipeline contradicts the "real-time" and "atomic consistency" claims.** The system is not real-time at ingestion, not atomic across the full pipeline, and the consistency benefit of pgvector is limited to the write path while the async queue governs availability. This is the most vulnerable architectural seam.

3. **The contribution differentiators would be stronger if reframed as design rationale rather than novelty claims.** The four contributions (polyglot architecture, vector-native consistency, commodity hardware benchmarking, applied model comparison) are defensible *as what was built* but vulnerable when claimed as *what was contributed*.

4. **The DSR methodology choice is questionable but salvageable.** DSR requires demonstrating that the artifact solves a *relevant* problem in a *novel* way, with *rigorous* evaluation. The artifact exists, the problem is real, but novelty is thin and rigour requires Chapter 3 data. Prepare to justify why DSR rather than a simpler experimental-comparison methodology.
