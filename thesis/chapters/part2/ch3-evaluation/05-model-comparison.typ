== Model Comparison & Discussion

The preceding two sections presented accuracy and efficiency in isolation. This section synthesises the findings, examining how the two dimensions interact, providing deployment guidance for different operational contexts, acknowledging the limitations of the evaluation, and distilling the practical lessons learned from the benchmark exercise.

=== Accuracy-Efficiency Trade-off

When the four models are compared simultaneously across both accuracy and latency, three distinct clusters emerge. Table @tbl-comparison presents the combined view.

#figure(
  caption: [Combined Accuracy and Efficiency Comparison],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@10*], [*R\@10*], [*Latency (ms)*], [*Throughput*], [*Load (ms)*], [*Storage (MB)*],
    ),
    [Fashion-CLIP], [*0.8788*], [*0.9155*], [*0.0646*], [92.0], [18.0], [5,441.8], [3.3],
    [CLIP-generic], [0.8341], [0.8862], [0.0597], [92.9], [19.9], [6,514.0], [3.3],
    [EfficientNet-B0], [0.8158], [0.8703], [0.0571], [*23.9*], [*33.2*], [*126.3*], [8.1],
    [ResNet-50], [0.8120], [0.8671], [0.0551], [64.0], [12.9], [286.1], [13.0],
  ),
  kind: table,
) <tbl-comparison>

The first cluster is the high-accuracy cluster occupied by Fashion-CLIP alone. Its mAP of 0.8788 leads every other model by at least 5.4%, placing it in a distinct accuracy tier. Its inference speed of 92.0 ms is moderate, within the sub-second interactive response budget.

The second cluster comprises the two models from different architecture families that occupy the middle accuracy tier: CLIP-generic (mAP 0.8341) and EfficientNet-B0 (mAP 0.8158). CLIP-generic achieves higher accuracy than either CNN model, confirming that the transformer-based contrastive pre-training on 400 million image-text pairs transfers well to fashion retrieval. EfficientNet-B0, despite lower mAP, achieves the fastest inference (23.9 ms) and highest throughput (33.2 img/s) of all models.

The third cluster is ResNet-50 alone: the lowest mAP (0.8120), low throughput (12.9 img/s), and the largest storage footprint (13.0 MB). Its 64.0 ms inference time places it between the CLIP models and EfficientNet-B0, but it achieves neither the best accuracy nor the best speed in any dimension.

=== Deployment Recommendations

Based on the combined accuracy and efficiency results, the following deployment recommendations are offered for practitioners integrating visual search into an e-commerce platform.

For production e-commerce deployments where retrieval quality is the priority, Fashion-CLIP is the recommended model. Its mAP of 0.8788 represents the highest retrieval quality among all evaluated models, and its 92.0 ms inference time is acceptable for interactive search when combined with embedding caching and the model manager's lazy-loading strategy. The fashion-specific training data gives it a measurable 5.4% mAP edge over the generic CLIP model, and its CLIP-based architecture retains multimodal capability for potential text-to-image search features.

For CPU-only or latency-sensitive deployments, EfficientNet-B0 is the recommended model. Its 23.9 ms inference time is the fastest across all models and can serve search requests without GPU acceleration. Its mAP of 0.8158 achieves 92.8% of the Fashion-CLIP quality at 26.0% of the latency. The low model load time (126.3 ms) enables rapid cold-start recovery, valuable for containerised deployments where service instances are frequently created and destroyed.

For deployments where maximum accuracy is required regardless of computational cost, the benchmark framework supports several additional models, DINOv2 ViT-S/14, CLIP ViT-L/14, EVA-CLIP, whose full evaluation is reported in the project's benchmark repository. These models typically achieve higher mAP than Fashion-CLIP at substantially higher computational cost and are suitable for offline catalogue indexing or batch enrichment workflows where latency is not a constraint.

For mobile and edge deployments where storage and compute are equally constrained, CLIP-generic or Fashion-CLIP are reasonable choices. Their 512-dimensional embeddings produce compact storage (3.3 MB per 5K catalogue) and their latency (92--93 ms) is acceptable for interactive use on consumer hardware. ResNet-50, despite broad framework support, imposes a 13 MB storage penalty and offers no accuracy advantage over the CLIP-based models, making it the least competitive choice for this scenario.

The pluggable model configuration mechanism described in Section 2.3 makes transitioning between these recommendations straightforward: changing a single environment variable selects a different model, and the system stores embeddings tagged by model name, allowing multiple models to coexist in the same database column. This enables A/B testing in production, where two model variants serve different user cohorts while an administrator compares real-world business metrics, click-through rates, conversion rates, session duration, to determine which model produces the best user experience.

=== Discussion of Limitations

Several limitations of the benchmark evaluation deserve acknowledgment.

*Dataset representativeness.* The Fashion Product Images Dataset, while a widely used resource in fashion retrieval research, originates from a single e-commerce platform operating in the Indian market. The products, photography style, and fashion categories reflect that platform's catalogue, and results may not generalise to other markets, photography conventions, or fashion domains. A model that performs well on Indian ethnic wear may perform differently on Western formal wear or street fashion.

*Relevance criterion simplification.* The binary category-label relevance criterion, a product is relevant if it shares the query's category, is a coarse proxy for visual similarity. Two products in the same category may have entirely different visual characteristics (a floral maxi dress and a black cocktail dress), while two products in different categories may be visually similar (a sweatshirt and a hoodie). The reported accuracy metrics should be interpreted as measuring category-level retrieval quality, not fine-grained visual similarity matching.

*Hardware specificity.* All inference time, throughput, and latency figures are tied to the specific CPU and memory configuration listed in Table @tbl-benchmark-hardware. The benchmark was executed on CPU without GPU acceleration. Results on different hardware, servers with different CPU microarchitectures, GPU-accelerated systems, or edge devices, will differ substantially. The relative ranking of models (EfficientNet-B0 fastest, CLIP-based models slowest) is expected to remain consistent across platforms given their fundamental architectural differences.

*P\@20 and K-value selection.* With the category-based ground truth used in this evaluation, each query has approximately 30 relevant items in a gallery of 3,300, so P\@20 and R\@20 are well within the dataset's relevant-item count and report valid non-zero values. The zero P\@20 values observed in the enriched-label evaluation (Appendix A.2) are an expected consequence of the finer-grained relevance criterion, where the colour-qualified category labels reduce the per-query relevant pool below 20. Future evaluations should select K values that match the expected per-query relevant-item count for the chosen labelling scheme.

*RAM measurement.* The RAM column in Table @tbl-efficiency reports near-zero values for three of the four models due to limitations in the process-level memory measurement on the benchmark's Linux host. Actual memory consumption per model is measured in hundreds of megabytes: model weight files alone range from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based models), and the PyTorch runtime adds its own overhead. The reported figures should not be used for capacity planning. Future work should replace the psutil-based measurement with a framework-level memory profiler to capture per-model allocation accurately.

*Statistical significance.* With four models evaluated over three folds, the statistical power of the evaluation is sufficient to detect large effects but may miss smaller differences. The 0.0038 mAP difference between EfficientNet-B0 (0.8158) and ResNet-50 (0.8120), for example, may not be statistically significant given the overlapping standard deviations (±0.0007 and ±0.0052). Conversely, Fashion-CLIP's mean mAP of 0.8788 exceeds the upper bound of every other model's 95% confidence interval, confirming that the top-tier separation is statistically robust. Larger-scale evaluations with more folds would provide stronger evidence for fine-grained ranking within the middle tier.

=== Lessons Learned

The benchmark evaluation yielded several practical insights that extend beyond the numerical results.

*Domain-specific fine-tuning matters.* Fashion-CLIP's consistent mAP advantage over the generic CLIP model, a 6.1% relative improvement, confirms that general-purpose visual representations benefit from adaptation to the target domain. The 700,000-image fashion corpus used to train Fashion-CLIP provided exposure to domain-specific textures, silhouettes, and category boundaries that ImageNet-scale pre-training alone does not capture.

*Architecture choice dominates the accuracy-efficiency trade-off.* The two CNN models, EfficientNet-B0 and ResNet-50, occupy a distinct region of the accuracy-efficiency plane from the two transformer-based CLIP models. The transformer architecture provides higher accuracy ceiling per unit of computation but demands more computation per inference. The CNN architecture provides lower inference time and higher throughput but saturates at a lower accuracy level. Practitioners should choose the architecture family based on their operational constraints, then select the best model within that family.

*K-value selection must match the labelling scheme.* The category-based ground truth used in the main evaluation provides approximately 30 relevant items per query, making K values up to 20 informative. The enriched-label ground truth (category-plus-colour) in Appendix A.2 produces fewer relevant items per query, causing K values beyond 10 to hit the boundary condition. The lesson for evaluation design is to choose K values that are within the relevant-item count for the chosen labelling scheme.

*The pluggable model architecture is a practical enabler.* The ability to switch between any of the eleven supported models by changing one environment variable, a design decision validated by the benchmark, transforms what would otherwise be a single-model evaluation into a systematic comparison. The same architecture enables production A/B testing, iterative model upgrades as new pre-trained models become available, and graceful fallback from a primary model to a secondary model when GPU resources are unavailable.

*Commodity hardware suffices for production visual search.* Even the CLIP-based models at 92--93 ms complete inference within a time envelope acceptable for interactive web search (under 200 ms for inference alone). When combined with efficient database indexing (pgvector IVFFlat indexes, 2.7--6.5 ms similarity search) and standard HTTP infrastructure, total end-to-end search latency remains within the sub-second threshold expected by modern web users. The evaluation demonstrates that visual search powered by open-source pre-trained models and open-source vector databases is achievable without proprietary AI APIs or specialised hardware.

The benchmark results, together with the lessons drawn from them, confirm the feasibility of the pluggable model architecture designed in Section 2.2 and implemented in Section 2.3. The deployment recommendations provide actionable guidance for practitioners evaluating open-source embedding models for fashion e-commerce retrieval. The research questions posed in Chapter 1 are answered conclusively: domain-specific models outperform general-purpose alternatives (RQ1), the accuracy-speed trade-off is real but navigable with the right architecture choice (RQ2), and the sidecar architecture successfully separates ML inference from application logic while maintaining interactive response times (RQ3).
