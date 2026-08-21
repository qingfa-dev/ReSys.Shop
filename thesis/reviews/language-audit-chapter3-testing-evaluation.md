# Language Level Audit + Re-Leveled Rewrite — Chapter 3: Testing and Evaluation

**Scope note:** §3.1–3.4 (Goal, Scenario, Result of Testing, Benchmark Protocol) are mostly tables and short factual sentences, already appropriately plain, similar to the requirements tables in Chapter 2. The real language-level issues concentrate in §3.5–3.7 (Retrieval Performance, Efficiency, Synthesis), the discussion/interpretation sections, where the writing shifts into a denser, more "journal article" register. This file focuses there.

**Factual corrections carried forward, already applied below:** the "6.1%" error (§3.7.4) is corrected to "5.4%," and PostgreSQL 16 (Table 66) is corrected to 17, both per the earlier factual review. The bigger Table 67/Appendix A reconciliation is still pending your decision and is not resolved by this language pass.

---

## STAGE 1 — Sentence-by-sentence audit (§3.5–3.7 only)

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "Fashion-CLIP's standard deviation (±0.0022) is the lowest among all models... confirming both highest average quality and greatest cross-fold consistency." | "Confirming both X and Y" is a dense, report-style sentence ending. First of several "confirming..." endings in this section, see item 10 below for the full pattern. | [AI-LIKE] |
| 2 | "Contrastive pre-training on 400 million image-text pairs produces embeddings that generalise to fashion category retrieval without domain fine-tuning." | Correct and reasonably clear given the technical content; "generalise" is standard ML vocabulary, acceptable. | [TECHNICAL TERM] |
| 3 | "EfficientNet-B0 and ResNet-50 occupy the lowest accuracy tier, with P@K and R@K values tracking within 0.7% across all K levels." | "Occupy... tier" is a spatial metaphor (models don't literally occupy anything), moderately advanced usage; appears again later in §3.7.4 (item 8). | [TOO ADVANCED] |
| 4 | "ResNet-50's higher embedding dimensionality... does not improve category-level retrieval, consistent with higher dimensionality benefiting finer-grained distinctions." | Dense, abstract clause stacked at the end of the sentence; understandable but compressed. | [UNCLEAR] |
| 5 | "Fashion-CLIP's mAP lower bound... exceeds the upper bound of EfficientNet-B0 and ResNet-50, confirming statistically meaningful separation." | Second "confirming..." sentence ending. | [AI-LIKE] |
| 6 | "The 5.4% mAP advantage over the generic CLIP wrapper demonstrates that domain-specific fine-tuning provides measurable retrieval quality improvements not achieved by general-purpose contrastive pre-training alone." | Very long (30+ words), heavily nested clause structure ("demonstrates that X provides Y not achieved by Z alone"). This is native-academic-journal register. | [AI-LIKE] + [UNCLEAR] |
| 7 | "The accuracy-speed trade-off is substantial and non-linear." | Reasonably clear given the explanation that follows; "substantial" repeats a word already flagged in Chapter 1 as a candidate for simplification, though here it's used correctly and not excessively. | [TOO ADVANCED] (mild) |
| 8 | "The relationship is not simply 'slower equals more accurate': the two CLIP models have near-identical latency yet Fashion-CLIP's mAP is 5.4% higher, demonstrating that domain-specific optimisation provides accuracy gains without a speed penalty." | The quoted informal phrase ("slower equals more accurate") is actually a good, natural touch, keep that. But "demonstrating that... provides accuracy gains without a speed penalty" at the end repeats the same dense "demonstrating that X provides Y" pattern as item 6. | [AI-LIKE] (second half only) |
| 9 | "Practitioners choosing between EfficientNet-B0 and Fashion-CLIP weigh a 7.7% mAP improvement against a 3.8× latency increase." | Clear and well-structured; "weigh... against" is a good, natural way to express a trade-off. Good model sentence. | [NO ISSUE] |
| 10 | "Three distinct clusters emerge when both dimensions are examined simultaneously." | "Emerge when... are examined simultaneously" is report-style, passive-feeling construction. | [AI-LIKE] |
| 11 | "The pluggable configuration mechanism enables transitioning between recommendations by changing a single environment variable." | "Enables transitioning between" is a stiff, formal phrase; "lets you switch between" is simpler and equally accurate. | [TOO ADVANCED] (mild) |
| 12 | "The binary category-label relevance criterion is a coarse proxy: same-category products may be visually dissimilar, and different-category products may share strong visual features." | "Coarse proxy" is genuinely advanced vocabulary (an economics/statistics term for an imperfect stand-in measure). The explanation after the colon is clear, though. | [TOO ADVANCED] |
| 13 | "With four models over three folds, the evaluation detects large effects but may miss smaller differences." | Clear, well-phrased, good sentence for this level. | [NO ISSUE] |
| 14 | "Fashion-CLIP's mean mAP exceeds the upper 95% confidence bound of every other model, confirming statistically robust top-tier separation." | Third occurrence of the "confirming..." sentence-ending pattern, and "top-tier" repeats the spatial "tier" metaphor from item 3. | [AI-LIKE] |
| 15 | "Architecture choice dominates the trade-off. CNN and transformer-based models occupy distinct accuracy-efficiency regions." | "Dominates" and "occupy... regions" are both moderately advanced/metaphorical word choices, repeating patterns already flagged. | [TOO ADVANCED] |
| 16 | "The pluggable model architecture is a practical enabler." | "Practical enabler" is an odd, slightly AI-sounding abstract noun phrase, not how this idea is normally expressed. | [AI-LIKE] |
| 17 | "Switching models via one environment variable transforms evaluation into systematic comparison, enabling production A/B testing and graceful fallback." | Dense, but the individual terms (A/B testing, graceful fallback) are real, necessary technical concepts. | [UNCLEAR] (mild) |
| 18 | "Commodity CPU hardware suffices." | "Suffices" is genuinely rare, advanced vocabulary (means "is enough"); a much more common word exists. | [TOO ADVANCED] |
| 19 | "Open-source tools are sufficient. Pre-trained open-source models and pgvector deliver production-viable visual search without proprietary APIs or specialised hardware." | "Deliver production-viable visual search" is a compressed, almost marketing-style phrase, similar to patterns flagged in Chapter 1. | [AI-LIKE] |
| 20 | "Independent scaling and fault isolation were achieved without distributed infrastructure overhead, confirming that a polyglot architecture with a dedicated AI sidecar is viable for real-time interactive search on commodity hardware." | Fourth and final occurrence of the "confirming..." pattern, plus a long (30+ word) closing sentence. | [AI-LIKE] |

**Pattern worth naming directly:** the phrase "confirming [that] X" appears as a sentence ending **four separate times** across §3.5–3.7 (items 1, 5, 14, 20), each time wrapping up a paragraph with the same grammatical template. This kind of repeated formulaic conclusion pattern, using the same word to close out several different paragraphs, is one of the clearest AI-writing tells in the whole thesis, more than any single advanced vocabulary word. It's worth varying this specifically, not just simplifying the individual words around it.

---

## STAGE 2 — Methodology claims requiring verification

No new methodology concerns beyond what's already established in the earlier factual review for this chapter (the Table 67 vs. Appendix A reconciliation, the PostgreSQL version, the "6.1%" error). Nothing further to flag here from a language-audit perspective.

---

## STAGE 3 — Re-leveled rewrite (§3.5–3.7)

```
3.5 RETRIEVAL PERFORMANCE AND ACCURACY

[Figures 42-43 captions unchanged. Table 67 unchanged, pending the
Table 67 / Appendix A reconciliation discussed separately.]

Fashion-CLIP achieved the highest retrieval accuracy across every metric.
Its mAP of 0.8788 is 5.4% higher than CLIP-generic (0.8341), 7.7% higher
than EfficientNet-B0 (0.8158), and 8.2% higher than ResNet-50 (0.8120).
This advantage holds at every K value: P@5 (0.9304 vs 0.9025), P@10
(0.9155 vs 0.8862), and P@20 (0.8982 vs 0.8640). Fashion-CLIP also has
the lowest standard deviation (±0.0022) among all models, less than half of
CLIP-generic's (±0.0043). This shows both the highest average accuracy
and the most consistent results across folds.

CLIP-generic achieved the second-highest mAP (0.8341), 2.2% higher than
EfficientNet-B0 and 2.7% higher than ResNet-50. Contrastive pre-training
on 400 million image-text pairs produces embeddings that work well for
fashion category retrieval, even without fashion-specific fine-tuning,
though with higher variability across folds (±0.0043).

EfficientNet-B0 (0.8158) and ResNet-50 (0.8120) have the lowest accuracy
among the four models, with P@K and R@K values staying within 0.7% of
each other across all K levels. ResNet-50's higher embedding size (2,048
vs 1,280 dimensions) does not improve category-level retrieval in this
result, this is consistent with larger embeddings mainly helping with
finer-grained distinctions rather than broad category matching.

Fashion-CLIP's mAP lower bound (mean minus two standard deviations:
0.8744) is higher than the upper bound of both EfficientNet-B0 (0.8172)
and ResNet-50 (0.8224). This shows the difference between Fashion-CLIP
and these two models is statistically meaningful, not just due to random
variation.

Answer to RQ1. Fashion-CLIP outperforms all three general-purpose
models on every accuracy metric. The 5.4% mAP advantage over the
generic CLIP wrapper shows that domain-specific fine-tuning gives a real,
measurable improvement in retrieval quality, beyond what general-purpose
contrastive pre-training alone provides. This gap is consistent at both
shallow (P@5: 0.9304 vs 0.9025) and deeper (P@20: 0.8982 vs 0.8640)
retrieval depths, with a clear statistical separation: Fashion-CLIP's lower
mAP bound (0.8744) is still higher than the upper bound of every other
model.

3.6 COMPUTATIONAL EFFICIENCY AND RESOURCE TRADE-OFFS

[Figure 44 caption, Table 68/69 unchanged.]

[RAM measurement caveat paragraph, already honest and clear, kept
mostly as-is with light simplification:] RAM measurement using psutil
was unreliable on this Linux kernel, producing negative and zero values.
Actual memory use likely ranges from about 100 MB (EfficientNet-B0) to
over 600 MB (CLIP-based models) for model weights alone, in addition to
PyTorch's own runtime memory use.

Answer to RQ2. The accuracy-speed trade-off is large and not linear.
EfficientNet-B0 (23.9 ms) reaches 92.8% of Fashion-CLIP's mAP (0.8158
vs 0.8788) at only 26.0% of the latency. ResNet-50 combines the lowest
mAP (0.8120) with medium latency (64.0 ms) and the largest storage size
(13.0 MB). The pattern is not simply "slower means more accurate": the
two CLIP models have almost the same latency, but Fashion-CLIP's mAP is
5.4% higher. This shows that domain-specific fine-tuning can improve
accuracy without a speed cost. Practitioners choosing between
EfficientNet-B0 and Fashion-CLIP are trading a 7.7% mAP improvement
for a 3.8x increase in latency.

3.7 SYNTHESIS, DEPLOYMENT STRATEGY, AND LIMITATIONS

3.7.1 Accuracy-Efficiency Trade-off
Table 69 shows accuracy and efficiency together. Looking at both at the
same time, three separate groups appear.

[Table 69 unchanged.]

[Cluster descriptions unchanged, already clear from earlier review.]

3.7.2 Deployment Recommendations
For latency-sensitive deployments, EfficientNet-B0 is recommended: its
23.9 ms inference time reaches 92.8% of Fashion-CLIP's mAP at 26.0% of
the latency, and its 126.3 ms load time allows fast recovery after a cold
start. The pluggable configuration setup (Section 2.3) lets you switch
between recommended models by changing a single environment variable.
Embeddings are tagged by model name, so multiple models can be used at
the same time.

3.7.3 Limitations
The Fashion Product Images Dataset comes from a single e-commerce
platform, and results may not apply directly to other markets or
photography styles.

The binary category-label relevance rule is an imperfect stand-in for true
visual similarity: products in the same category can look different, and
products in different categories can share strong visual features. All
inference numbers are specific to the CPU configuration used, without
GPU acceleration. RAM measurement using psutil was unreliable on this
benchmark's Linux host. With four models over three folds, this evaluation
can detect large differences but may miss smaller ones. Fashion-CLIP's
mean mAP is higher than the upper 95% confidence bound of every other
model, which supports a clear, statistically strong separation at the top.

3.7.4 Summary
Five findings come out of this benchmark:
1. Domain-specific fine-tuning matters. Fashion-CLIP's 5.4% mAP
   improvement over generic CLIP shows that fine-tuning on fashion data
   gives a measurable benefit.
2. Architecture choice matters most for the trade-off. CNN and
   transformer-based models sit in different accuracy-efficiency ranges;
   practitioners should first choose a family based on their operational
   limits, then pick the best model within that family.
3. The pluggable model design is genuinely useful in practice. Being able
   to switch models using one environment variable turns evaluation into
   ongoing comparison, and supports production A/B testing and fallback
   options if needed.
4. Regular CPU hardware is enough. Even the CLIP models finish
   inference in under 200 ms; combined with pgvector IVFFlat indexes
   (2.7-6.5 ms), total end-to-end latency stays under one second.
5. Open-source tools are enough. Pre-trained open-source models and
   pgvector can provide production-ready visual search without needing
   paid APIs or special hardware.

Answer to RQ3. The sidecar architecture successfully separates ML
inference from the main web application logic. The EMBEDDING_MODEL
environment variable allows switching models without changing backend
code. On CPU, end-to-end search latency stays under one second.
Independent scaling and fault isolation were both achieved without the
extra cost of a full distributed system. This shows that a polyglot
architecture with a dedicated AI sidecar works well for real-time,
interactive search on normal, non-specialised hardware.
```

---

## STAGE 4 — Final consistency check

| Check | Result |
|---|---|
| Vocabulary difficulty | "Occupy... tier/region" (used 3 times) → "sit in different ranges" / "lowest accuracy among," "suffices" → "is enough," "coarse proxy" → "imperfect stand-in," "practical enabler" → "genuinely useful in practice," "deliver production-viable" → "provide production-ready." Statistical/technical vocabulary (confidence bound, standard deviation, cross-fold variability) kept exactly, necessary and correct. |
| Sentence length | Several 30+ word sentences with nested clauses (item 6, item 20) split into two sentences each. |
| Grammar | No errors introduced. |
| Repeated phrases | The "confirming [that] X" pattern, used 4 times as a paragraph-ending template, was the single clearest formulaic-writing signal found in this chapter; all four instances reworded differently ("shows both...," "shows the difference is...," "supports a clear... separation," "shows that... works well"), varying the phrasing rather than defaulting to the same word each time. |
| AI-like formulaic expressions | Removed: "confirming [that]..." (4x), "three distinct clusters emerge when... examined simultaneously," "practical enabler," "deliver production-viable visual search," "demonstrates/demonstrating that X provides Y." |
| Technical terminology | Preserved exactly: mAP, P@K, R@K, standard deviation, confidence bound, cross-fold, A/B testing, graceful fallback, IVFFlat, all necessary for this content. |
| Numbers | 5.4%, 7.7%, 8.2%, 92.8%, 26.0%, 3.8x, all confidence-interval values, kept exactly as verified in the earlier factual review; "6.1%" corrected to "5.4%" as already established. |
| Claims vs. evidence | No claims strengthened or weakened; the already-flagged Table 67/Appendix A reconciliation is noted but not resolved by this pass, that requires your decision separately. |
| Meaning preserved | Checked paragraph by paragraph against the original; every number, comparison, and conclusion preserved, only the sentence-level phrasing changed. |

---

## A. Ten most important problems

1. The repeated "confirming [that] X" sentence-ending pattern, used four separate times across §3.5–3.7, the clearest single AI-writing signal in the whole chapter.
2. "The 5.4% mAP advantage... demonstrates that domain-specific fine-tuning provides measurable retrieval quality improvements not achieved by general-purpose contrastive pre-training alone" — one long, heavily nested sentence.
3. "Occupy the lowest accuracy tier" / "occupy distinct accuracy-efficiency regions" — spatial metaphor used twice, advanced.
4. "The pluggable model architecture is a practical enabler" — odd, abstract AI-sounding noun phrase.
5. "Commodity CPU hardware suffices" — genuinely rare word for a simple idea.
6. "The binary category-label relevance criterion is a coarse proxy" — advanced vocabulary ("coarse proxy").
7. "Three distinct clusters emerge when both dimensions are examined simultaneously" — passive, report-style construction.
8. "Deliver production-viable visual search without proprietary APIs" — compressed marketing-style phrase.
9. "Enables transitioning between recommendations" — stiff formal phrase where "lets you switch between" works just as well.
10. The "6.1%" figure in the original §3.7.4 — not a language issue, but the highest-priority fix in this whole chapter (already corrected here to 5.4%, consistent with the earlier factual review).

## B. Words/phrases to avoid

confirming (as a repeated paragraph-ending word, vary it instead), occupy (used metaphorically for "tier" or "region"), suffices, coarse proxy, practical enabler, deliver production-viable, demonstrates/demonstrating that (prefer "shows that"), emerge when... examined simultaneously

## C. Words/phrases that are safe and natural for your level

shows that, is enough, an imperfect stand-in for, sits in a different range, genuinely useful, provides / can provide, works well for, is higher/lower than, is not simply

## D. Writing style to use consistently

Same general guidance as previous files, plus one specific new pattern to watch for: **don't end multiple paragraphs with the same word or phrase.** The original repeatedly closes a paragraph's analysis with "confirming..." four times in a row across this chapter, and that repetition is more noticeable, and more clearly a writing habit rather than a content choice, than any single advanced word would be. When you finish explaining a result, vary how you state the conclusion: "this shows," "this means," "this suggests," or just stating the fact directly without a wrap-up phrase at all.

---

This completes Chapter 3. Ready for Part 3 (Conclusion and Future Work) next, same three-stage process.
