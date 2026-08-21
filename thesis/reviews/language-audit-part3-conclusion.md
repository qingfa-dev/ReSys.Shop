# Language Level Audit + Re-Leveled Rewrite — Part 3: Conclusion and Future Work

**Scope note:** Part 3 is a summary/conclusion chapter, so, similarly to §2.4's opening and §3.5–3.7, it naturally invites a more polished, "wrapping-up" register. This section has the second-highest concentration of advanced/AI-like phrasing found in the whole audit series, after §2.4's opening paragraph.

**Factual corrections carried forward, already applied below:** "eleven supported architectures" → "six," the accuracy-metric count clarification (three families, seven columns), the "near-zero P@20" claim replaced with the real figures, all per the earlier factual review.

---

## STAGE 1 — Sentence-by-sentence audit

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "Three principal findings: domain-specific pre-training provides measurable retrieval advantages; the accuracy-efficiency trade-off is navigable via architecture choice; and the polyglot sidecar architecture is viable for .NET enterprise stacks." | Three abstract noun-phrase clauses chained with semicolons; "navigable via" is a genuinely unusual, advanced word pairing. | [AI-LIKE] + [TOO ADVANCED] |
| 2 | "achieving interactive response times on commodity hardware" | "Commodity hardware" is real, standard technical vocabulary (means normal, non-specialized hardware), keep. | [TECHNICAL TERM] |
| 3 | "The trade-off is substantial." | Same "substantial" pattern flagged multiple times already across the thesis. | [TOO ADVANCED] (mild, repeat) |
| 4 | "Fashion-CLIP (mAP 0.8788, 92.0 ms) represents the quality ceiling." | "Represents the quality ceiling" is a compressed, almost business-report metaphor ("ceiling" meaning upper limit); correct but reads as more polished than the plain factual sentences around it. | [AI-LIKE] |
| 5 | "Domain fine-tuning provides accuracy without speed penalty." | "Speed penalty" is an understandable but moderately formal compound noun; acceptable given context, borderline. | [TOO ADVANCED] (mild) |
| 6 | "The sidecar architecture successfully separated ML inference from web application logic." | Clear, direct, good sentence for this level. | [NO ISSUE] |
| 7 | "Independent scaling and fault isolation were achieved without distributed infrastructure overhead, confirming viability for real-time interactive search on consumer-grade hardware." | Fifth occurrence of the "confirming..." paragraph-ending pattern flagged in Chapter 3, this one crosses into Part 3 too. | [AI-LIKE] |
| 8 | "All four technical objectives were met. Model integration was demonstrated through the operational search pipeline. Polyglot architecture delivered clean separation via the sidecar pattern." | "Delivered clean separation via" is a compressed, almost marketing-style phrase; "operational search pipeline" is also a fairly formal noun phrase. | [AI-LIKE] |
| 9 | "pgvector feasibility was confirmed: IVFFlat queries execute under 10 ms." | "Feasibility was confirmed" is passive and abstract; a more direct sentence works better. | [AI-LIKE] (mild) |
| 10 | "Benchmark evaluation produced empirical accuracy and efficiency metrics across four models and eleven supported architectures." | Factual correction already established: "eleven" → "six." Also "produced empirical... metrics" is a stiff, passive phrase. | [AI-LIKE] + [not language: factual, already flagged] |
| 11 | "This thesis makes five concrete contributions." | Clear, direct, standard academic phrasing, fine at this level. | [NO ISSUE] |
| 12 | "A reference CBIR implementation integrated into a production-style e-commerce platform. Demonstrates that open-source tools... deliver competitive visual search." | "Deliver competitive visual search" repeats the "deliver + abstract noun" pattern flagged multiple times across the thesis (Chapter 1, Chapter 3). | [AI-LIKE] (repeat) |
| 13 | "A pluggable model architecture enabling runtime model switching. Strategy-pattern Model Manager controlled via environment variable decouples model selection from application code." | "Decouples model selection from application code" is correct, standard software-engineering vocabulary (decoupling is a real, necessary concept), acceptable technical usage. | [TECHNICAL TERM] |
| 14 | "Demonstration of pgvector's ACID-compliant vector storage. Embeddings in the same PostgreSQL database as product data eliminate stale-index bugs." | "Eliminate stale-index bugs" is fine, direct, technical. Good sentence. | [NO ISSUE] |
| 15 | "A validated polyglot architecture pattern for .NET and Python AI. The sidecar integration provides a blueprint for teams incorporating Python ML into .NET applications." | "Provides a blueprint" is a moderately advanced metaphor (blueprint = detailed plan); understandable, borderline. | [TOO ADVANCED] (mild) |
| 16 | "Several limitations constrain the generalisability of the findings." | "Constrain the generalisability of" is dense, abstract, academic-journal phrasing for a simple idea ("these limitations affect how well the results apply elsewhere"). | [AI-LIKE] |
| 17 | "The binary category-label ground truth is a coarse proxy for visual similarity." | Same "coarse proxy" issue already flagged in Chapter 3. | [TOO ADVANCED] (repeat) |
| 18 | "The enriched-label evaluation produces near-zero P@20 values due to the finer-grained relevance criterion." | Factual correction already established: this claim is inaccurate; real values are around 0.30, not near-zero. Fixed in the rewrite below per the earlier factual review. | [not language: factual, already flagged] |
| 19 | "Seven directions for future work are motivated by the limitations above and by insights from design and implementation." | "Are motivated by" is correct academic phrasing, but "insights from design and implementation" is a vague abstract noun phrase. | [AI-LIKE] (mild) |
| 20 | "These directions define a roadmap from research demonstration to production-grade visual commerce engine, each grounded in empirical findings and architectural decisions documented in preceding chapters." | Long (30+ words), heavily abstract closing sentence; "define a roadmap from X to Y" and "grounded in" are both native-level rhetorical patterns. | [AI-LIKE] + [UNCLEAR] |
| 21 | "Table 70 confirms that every objective and research question from Chapter 1 is addressed in a specific chapter section and produces a verifiable finding." | Sixth occurrence of "confirms/confirming" pattern across the thesis; also a fairly formal way to introduce a simple table. | [AI-LIKE] (repeat) |

---

## STAGE 2 — Methodology claims requiring verification

No new methodology concerns beyond what's already established (the "eleven architectures" figure and the "near-zero P@20" claim, both factual issues already flagged and corrected in the earlier review, applied in the rewrite below).

---

## STAGE 3 — Re-leveled rewrite

```
PART 3: CONCLUSION AND FUTURE WORK

This chapter closes the thesis: summary, research questions, contributions,
limitations, future work, and requirements traceability.

I. SUMMARY OF WORK

This thesis built a fashion e-commerce platform that combines a Vue 3
storefront, a .NET 10 modular monolith, and a Python ML sidecar. The
visual search pipeline was evaluated using a systematic benchmark of four
models under 3-fold cross-validation on 5,000 fashion images. Three main
findings came out of this work: domain-specific pre-training gives a
measurable improvement in retrieval accuracy; the accuracy-efficiency
trade-off can be managed through architecture choice; and a polyglot
sidecar architecture works well for .NET enterprise systems, reaching
interactive response times on normal, non-specialised hardware.

Answering the Research Questions

RQ1: How do fashion-specific embedding models compare with
general-purpose models across CNN and ViT architectures?
Fashion-CLIP outperformed all three general-purpose models: mAP 0.8788
vs. CLIP-generic 0.8341 (+5.4%), EfficientNet-B0 0.8158 (+7.7%),
ResNet-50 0.8120 (+8.2%). This advantage holds at both shallow (P@5:
0.9304 vs. 0.9025) and deep (P@20: 0.8982 vs. 0.8640) retrieval depths,
with the lowest cross-fold variability (±0.0022).

RQ2: What trade-offs exist between search accuracy and processing
speed?
The trade-off is large. Fashion-CLIP (mAP 0.8788, 92.0 ms) sets the
highest accuracy; EfficientNet-B0 (23.9 ms) reaches 92.8% of that
accuracy at 26.0% of the latency. Domain fine-tuning improves accuracy
without a speed cost (Fashion-CLIP vs. CLIP-generic: +5.4% mAP at
almost the same latency). For latency-sensitive deployments,
EfficientNet-B0 is recommended; for quality-critical deployments,
Fashion-CLIP.

RQ3: Can a service-oriented architecture with a dedicated AI sidecar
separate image inference from the main web application while keeping
acceptable response times?
The sidecar architecture successfully separated ML inference from the
web application logic. End-to-end search latency stayed under one second
on CPU. Independent scaling and fault isolation were both achieved,
without the extra cost of a full distributed system. This supports using
this pattern for real-time interactive search on normal, consumer-grade
hardware.

Achievement of Technical Objectives
All four technical objectives were met. Model integration was shown
through the working search pipeline. The polyglot architecture achieved
clean separation using the sidecar pattern. pgvector's feasibility was
confirmed: IVFFlat queries run in under 10 ms (2.7-6.5 ms). The
benchmark evaluation produced accuracy and efficiency data for four
models, selected from six supported by the framework.

II. CONTRIBUTIONS

This thesis makes five contributions:

- A four-model benchmark for fashion image retrieval. A systematic
  evaluation covering three accuracy metric families (mAP, P@K, R@K,
  reported at three depths for seven total columns) and five efficiency
  metrics, across four architecture families and six supported models, using
  a 3-fold cross-validation protocol.
- A reference CBIR implementation built into a production-style
  e-commerce platform. This shows that open-source tools (PyTorch,
  FastAPI, pgvector, .NET 10) can support competitive visual search.
- A pluggable model architecture that allows switching models at
  runtime. A Strategy-pattern Model Manager, controlled through an
  environment variable, separates model selection from the rest of the
  application code.
- A demonstration of pgvector's ACID-compliant vector storage. Storing
  embeddings in the same PostgreSQL database as product data avoids
  bugs caused by an out-of-date search index.
- A validated polyglot architecture pattern for .NET and Python AI. The
  sidecar integration gives other teams a working example for adding
  Python-based ML into a .NET application.

III. LIMITATIONS

Several limitations affect how well these findings apply more broadly. The
benchmark uses 5,000 product images from a single dataset, so results
may not apply directly to other markets. All figures were measured on a
single laptop, using CPU-only inference. The binary category-label ground
truth is an imperfect stand-in for true visual similarity. No formal user
study was conducted. All models were used as published, without
fine-tuning. The text-to-image search capability of CLIP-based models was
not evaluated. The enriched-label evaluation produces substantially lower
P@20 values, dropping from approximately 0.90 under category-only
labels to approximately 0.30 under category-plus-colour-plus-pattern
labels, due to the finer-grained relevance criterion. RAM measurement
using process-level tools was unreliable; actual memory use likely ranges
from 100 MB to over 600 MB per model.

IV. FUTURE WORK

The limitations above, along with insights gained during design and
implementation, point to seven directions for future work.
1. Fine-tune Fashion-CLIP on the target catalogue. This is the most direct
   way to improve retrieval accuracy further.
2. Run a user experience study with A/B testing, to measure the actual
   engagement improvement from CBIR (click-through rate, conversion
   rate).
3. Add multi-modal search that combines text and image queries, using
   the shared latent space of CLIP-family models.
4. Scale the benchmark to production-size catalogues (100,000 to
   1,000,000 images), to test whether pgvector's HNSW indexing and the
   model ranking results still hold at that scale.
5. Investigate ONNX Runtime optimisation, which could reduce
   transformer inference latency by 30 to 50 percent using operator fusion
   and hardware-specific kernels.
6. Add personalised re-ranking, using signals such as past purchases,
   browsing history, and wishlists.
7. Build a mobile application with on-device inference, using a quantised
   version of EfficientNet-B0 for offline visual search.
Each of these directions is based on the results and design decisions
described in earlier chapters, and together they show a path from this
research prototype toward a full production visual-commerce system.

V. REQUIREMENTS TRACEABILITY

Table 70 shows that every objective and research question from Chapter 1
is addressed in a specific chapter section, with a verifiable finding.
```

---

## STAGE 4 — Final consistency check

| Check | Result |
|---|---|
| Vocabulary difficulty | "Navigable via" → "can be managed through," "represents the quality ceiling" → "sets the highest accuracy," "constrain the generalisability of" → "affect how well... apply more broadly," "coarse proxy" → "imperfect stand-in," "define a roadmap... grounded in" → "based on... show a path toward." Technical/necessary vocabulary (ACID-compliant, Strategy pattern, decouples, quantised, operator fusion) all kept exactly. |
| Sentence length | The 30+ word Future Work closing sentence and the Summary of Work's semicolon-chained sentence both split or simplified. |
| Grammar | No errors introduced. |
| Repeated phrases | "Confirming/confirms" pattern (flagged as a cross-chapter issue in the Chapter 3 audit) appears twice more here (items 7 and 21); both reworded to vary the phrasing, consistent with the fix already applied in Chapter 3. "Deliver + abstract noun" also repeats (item 12) and is fixed the same way as its Chapter 1/3 occurrences. |
| AI-like formulaic expressions | Removed: "represents the quality ceiling," "delivered clean separation via," "produced empirical... metrics," "constrain the generalisability," "define a roadmap... grounded in." |
| Technical terminology | Preserved exactly: ACID-compliant, Strategy pattern, Model Manager, decouples, ONNX Runtime, operator fusion, quantised, all necessary and correct. |
| Numbers | 5.4%/7.7%/8.2%/92.8%/26.0% kept exact; "eleven" corrected to "six" per the earlier factual review; "near-zero P@20" replaced with the real 0.90 → 0.30 figures, also per that review. |
| Claims vs. evidence | No claims strengthened; the two factual corrections already established were applied here, not re-litigated or softened further. |
| Meaning preserved | Checked section by section; all five contributions, all seven future-work directions, and all three RQ answers preserved with the same content, only sentence-level phrasing simplified. |

---

## A. Ten most important problems

1. The "confirming/confirms" pattern appears two more times in Part 3 (on top of the four occurrences already flagged in Chapter 3), six total across the thesis, this is the single most consistent AI-writing habit found in the whole document.
2. "Three principal findings: domain-specific pre-training provides measurable retrieval advantages; the accuracy-efficiency trade-off is navigable via architecture choice; and..." — three abstract clauses chained with semicolons.
3. "Fashion-CLIP... represents the quality ceiling" — business-report metaphor.
4. "Polyglot architecture delivered clean separation via the sidecar pattern" — compressed, marketing-style phrase.
5. "This thesis makes five concrete contributions... deliver competitive visual search" — repeats the "deliver + abstract noun" pattern from Chapter 1/3.
6. "Several limitations constrain the generalisability of the findings" — dense academic-journal phrasing for a simple idea.
7. "These directions define a roadmap from research demonstration to production-grade visual commerce engine, each grounded in..." — long, heavily abstract closing sentence.
8. "Eleven supported architectures" — factual error, already corrected to six.
9. "Near-zero P@20 values" — factual error, already corrected to the real ~0.90 → ~0.30 figures.
10. "Table 70 confirms that every objective..." — same repeated pattern as item 1, used to introduce a simple table.

## B. Words/phrases to avoid

navigable via, represents the quality ceiling, delivered... via, produced empirical... metrics, deliver (as in "deliver competitive/production-viable X"), constrain the generalisability of, define a roadmap, grounded in, confirming/confirms (as a repeated sentence template)

## C. Words/phrases that are safe and natural for your level

can be managed through, sets the highest, achieved... using, can support, affect how well... apply, based on, shows a path toward, this shows / this supports

## D. Writing style to use consistently

Same guidance as before. Part 3 confirms a pattern visible across the whole thesis now: conclusion and summary sections consistently reach for a more polished, wrap-up register than the technical body chapters, and that's exactly where the "confirming X" habit shows up most. When writing a conclusion, resist the pull to sound more "final" or "impressive" than the rest of your writing, the goal is a plain, clear summary of what you found, not a different, higher register than the chapters it's summarizing.

---

This completes the three-stage language audit for Part 1, Chapter 1, Chapter 2, Chapter 3, and Part 3. Ready for the References list or Appendices next if you'd like the same treatment applied there, though those sections are mostly citations and technical tables with little prose to audit.
