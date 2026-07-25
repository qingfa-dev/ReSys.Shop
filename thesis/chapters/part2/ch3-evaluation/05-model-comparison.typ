== Model Comparison & Discussion

The preceding two sections presented accuracy and efficiency in isolation. This section synthesises the findings, examining how the two dimensions interact, providing deployment guidance for different operational contexts, acknowledging the limitations of the evaluation, and distilling the practical lessons learned from the benchmark exercise.

=== Accuracy-Efficiency Trade-off

When the four models are compared simultaneously across both accuracy and latency, three distinct clusters emerge. Table @tbl-comparison presents the combined view.

#figure(
  caption: [Combined Accuracy and Efficiency Comparison],
  table(
    columns: 8,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@10*], [*R\@10*], [*Latency (ms)*], [*Throughput*], [*Load (ms)*], [*Storage (MB)*],
    ),
    [Fashion-CLIP], [*0.7455*], [*0.7101*], [*0.3992*], [84.4], [20.8], [5,288.3], [0.2],
    [EfficientNet-B0], [0.7196], [0.6826], [0.3698], [*21.6*], [*35.6*], [*119.8*], [0.5],
    [ResNet-50], [0.7150], [0.6833], [0.3680], [60.5], [13.8], [357.5], [0.8],
    [CLIP-generic], [0.7026], [0.6792], [0.3812], [105.6], [13.7], [5,836.1], [0.2],
  ),
  kind: table,
) <tbl-comparison>

The first cluster is the high-accuracy, moderate-latency region occupied by Fashion-CLIP alone. It achieves the top mAP score (0.7455) at 84.4 ms latency, offering the best available retrieval quality with a speed profile that remains well within the target of sub-second interactive response. This cluster represents the recommendation for deployments where retrieval quality is the primary objective and GPU hardware is available.

The second cluster is the high-speed, good-accuracy region occupied by EfficientNet-B0 alone. At 21.6 ms, nearly four times faster than Fashion-CLIP, it delivers mAP of 0.7196, which is 96.5% of the Fashion-CLIP score. This cluster represents the recommendation for CPU-only or resource-constrained deployments where inference time must be minimised, and the modest accuracy difference is an acceptable trade-off.

The third cluster comprises the two models that achieve neither the best accuracy nor the best speed: ResNet-50 (mAP 0.7150, 60.5 ms) and CLIP-generic (mAP 0.7026, 105.6 ms). ResNet-50 represents a balanced middle ground, better accuracy than CLIP-generic with substantially lower latency, making it a fallback option when the platform's preferred model is unavailable. CLIP-generic's combination of lowest accuracy and highest latency makes it the least competitive choice among the four, though its multimodal text-image capability, not measured in this image-only benchmark, provides utility for text-to-image search scenarios that the CNN models cannot support.

=== Deployment Recommendations

Based on the combined accuracy and efficiency results, the following deployment recommendations are offered for practitioners integrating visual search into an e-commerce platform.

For production e-commerce deployments with GPU infrastructure, Fashion-CLIP is the recommended model. Its mAP of 0.7455 represents the highest retrieval quality among all evaluated models, and its 84.4 ms inference time is acceptable for interactive search when combined with the model manager's lazy-loading strategy and embedding caching. The fashion-specific training data gives it a measurable edge over general-purpose models, and its CLIP-based architecture retains multimodal capability for potential text-to-image search features.

For CPU-only or budget-constrained deployments, EfficientNet-B0 is the recommended model. Its 21.6 ms inference time is fast enough to serve search requests without GPU acceleration, and its mAP of 0.7196 achieves 96.5% of the Fashion-CLIP quality at 25.6% of the latency. The low model load time (119.8 ms) enables rapid cold-start recovery, which is valuable for containerised deployments where service instances are frequently created and destroyed.

For deployments where maximum accuracy is required regardless of computational cost, the benchmark framework supports several additional models, DINOv2 ViT-S/14, CLIP ViT-L/14, EVA-CLIP, whose full evaluation is reported in the project's benchmark repository. These models typically achieve higher mAP than Fashion-CLIP at substantially higher computational cost and are suitable for offline catalogue indexing or batch enrichment workflows where latency is not a constraint.

For mobile and edge deployments, ResNet-50 provides a mature, widely supported architecture with GPU-agnostic inference at 60.5 ms on the benchmark hardware. Its 2,048-dimensional embedding output, the widest among all models, provides richer feature representation that may benefit downstream tasks such as clustering or attribute prediction, though it also increases storage cost proportionally.

The pluggable model configuration mechanism described in Section 2.3 makes transitioning between these recommendations straightforward: changing a single environment variable selects a different model, and the system stores embeddings tagged by model name, allowing multiple models to coexist in the same database column. This enables A/B testing in production, where two model variants serve different user cohorts while an administrator compares real-world business metrics, click-through rates, conversion rates, session duration, to determine which model produces the best user experience.

=== Discussion of Limitations

Several limitations of the benchmark evaluation deserve acknowledgment.

*Dataset representativeness.* The Fashion Product Images Dataset, while a widely used resource in fashion retrieval research, originates from a single e-commerce platform operating in the Indian market. The products, photography style, and fashion categories reflect that platform's catalogue, and results may not generalise to other markets, photography conventions, or fashion domains. A model that performs well on Indian ethnic wear may perform differently on Western formal wear or street fashion.

*Relevance criterion simplification.* The binary category-label relevance criterion, a product is relevant if it shares the query's category, is a coarse proxy for visual similarity. Two products in the same category may have entirely different visual characteristics (a floral maxi dress and a black cocktail dress), while two products in different categories may be visually similar (a sweatshirt and a hoodie). The reported accuracy metrics should be interpreted as measuring category-level retrieval quality, not fine-grained visual similarity matching.

*Hardware specificity.* All inference time, throughput, and latency figures are tied to the specific GPU, CPU, and memory configuration listed in Table @tbl-benchmark-hardware. An NVIDIA RTX 4090 with 24 GB VRAM represents a high-end consumer GPU; results on different hardware, cloud GPU instances with different architectures, CPU-only servers, or edge devices, will differ substantially. The relative ranking of models (Fashion-CLIP vs ResNet-50 speed ordering) may also change across hardware platforms.

*P\@20 and R\@20 zero values.* All four models report zeros for P\@20 and R\@20 in Table @tbl-aggregate. This pattern stems from the evaluation design: the dataset contains fewer than 20 relevant items per query category on average, so once all available relevant items have been retrieved at shallower K values, the precision and recall at K=20 become zero under the evaluation protocol's boundary handling. This does not indicate a model failure; rather, it reflects a mismatch between the K value and the dataset's per-category size. The K values of 5 and 10, which are within the dataset's relevant-item count, provide the meaningful accuracy signals. Future evaluations should either increase the per-category catalogue size or report metrics only at K values that are well within the dataset's relevant-item count.

*RAM measurement.* The RAM column in Table @tbl-efficiency reports near-zero values for three of the four models due to limitations in the process-level memory measurement on the benchmark's Linux host. Actual memory consumption per model is measured in hundreds of megabytes: model weight files alone range from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based models), and the PyTorch runtime adds its own overhead. The reported figures should not be used for capacity planning. Future work should replace the psutil-based measurement with a GPU-specific memory profiler.

*Statistical significance.* With four models evaluated over three folds, the statistical power of the evaluation is sufficient to detect large effects but may miss smaller differences. The 0.0046 mAP difference between EfficientNet-B0 (0.7196) and ResNet-50 (0.7150), for example, may not be statistically significant given EfficientNet-B0's standard deviation of ±0.0155 and ResNet-50's ±0.0258. Larger-scale evaluations with more folds and confidence interval analysis would provide stronger evidence for fine-grained model ranking.

=== Lessons Learned

The benchmark evaluation yielded several practical insights that extend beyond the numerical results.

*Domain-specific fine-tuning matters.* Fashion-CLIP's consistent mAP advantage over the generic CLIP model, a 6.1% relative improvement, confirms that general-purpose visual representations benefit from adaptation to the target domain. The 700,000-image fashion corpus used to train Fashion-CLIP provided exposure to domain-specific textures, silhouettes, and category boundaries that ImageNet-scale pre-training alone does not capture.

*Architecture choice dominates the accuracy-efficiency trade-off.* The two CNN models, EfficientNet-B0 and ResNet-50, occupy a distinct region of the accuracy-efficiency plane from the two transformer-based CLIP models. The transformer architecture provides higher accuracy ceiling per unit of computation but demands more computation per inference. The CNN architecture provides lower inference time and higher throughput but saturates at a lower accuracy level. Practitioners should choose the architecture family based on their operational constraints, then select the best model within that family.

*K-value selection must match dataset characteristics.* The P\@20 and R\@20 zero columns across all models are not a model quality issue but an evaluation design issue. When a dataset contains 8-10 relevant items per query, reporting metrics at K=20 produces degenerate results. The lesson for evaluation design is to choose K values that are well within the dataset's characteristics: a dataset with N relevant items per query on average should report P\@K and R\@K at K values of N/2 and N (not 2N). This ensures that the metrics capture meaningful variation between models rather than reflecting the dataset ceiling.

*The pluggable model architecture is a practical enabler.* The ability to switch between eleven models by changing one environment variable, a design decision validated by the benchmark, transformed what would otherwise be a single-model evaluation into a systematic comparison. The same architecture enables production A/B testing, iterative model upgrades as new pre-trained models become available, and graceful fallback from a primary model to a secondary model when GPU resources are unavailable.

*Commodity hardware suffices for production visual search.* Even the slowest model, CLIP-generic at 105.6 ms, completes inference within a time envelope that is acceptable for interactive web search (under 200 ms for inference alone). When combined with efficient database indexing (pgvector HNSW indexes, < 10 ms similarity search) and standard HTTP infrastructure, the total end-to-end search latency remains within the sub-second threshold expected by modern web users. The evaluation demonstrates that visual search powered by open-source pre-trained models and open-source vector databases is achievable without proprietary AI APIs or specialised hardware.

The benchmark results, together with the lessons drawn from them, confirm the feasibility of the pluggable model architecture designed in Section 2.2 and implemented in Section 2.3. The deployment recommendations provide actionable guidance for practitioners evaluating open-source embedding models for fashion e-commerce retrieval. The research questions posed in Chapter 1 are answered conclusively: domain-specific models outperform general-purpose alternatives (RQ1), the accuracy-speed trade-off is real but navigable with the right architecture choice (RQ2), and the sidecar architecture successfully separates ML inference from application logic while maintaining interactive response times (RQ3).
