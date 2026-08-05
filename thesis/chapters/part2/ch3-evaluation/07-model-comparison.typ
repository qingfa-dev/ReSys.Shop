== Synthesis, Deployment Strategy, and Limitations

=== Accuracy-Efficiency Trade-off

@tbl-comparison presents the combined accuracy and efficiency view. Three distinct clusters emerge when both dimensions are examined simultaneously.

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

Fashion-CLIP occupies the high-accuracy cluster alone: mAP 0.8788 leads every other model by at least 5.4%, while its 92.0 ms inference time remains within the sub-second interactive budget. CLIP-generic (0.8341) and EfficientNet-B0 (0.8158) form the middle tier from different architecture families: CLIP-generic achieves higher accuracy, EfficientNet-B0 delivers fastest inference (23.9 ms) and highest throughput (33.2 img/s). ResNet-50 occupies the lowest tier on all dimensions: lowest mAP (0.8120), lowest throughput (12.9 img/s), largest storage (13.0 MB).

=== Deployment Recommendations

For retrieval quality, Fashion-CLIP is recommended (mAP 0.8788). Its 92.0 ms inference is acceptable for interactive search; the pluggable model architecture enables lazy-loading and embedding caching. For CPU-only or latency-sensitive deployments, EfficientNet-B0 is recommended: 23.9 ms inference achieves 92.8% of Fashion-CLIP's mAP at 26.0% of the latency, with 126.3 ms load time enabling rapid cold-start recovery. The pluggable configuration mechanism (Section 2.3) enables transitioning between recommendations by changing a single environment variable; embeddings tagged by model name allow multiple models to coexist.

=== Limitations

The Fashion Product Images Dataset originates from a single e-commerce platform and may not generalise to other markets or photography conventions. The binary category-label relevance criterion is a coarse proxy: same-category products may be visually dissimilar, and different-category products may share strong visual features. All inference figures are tied to the specific CPU configuration without GPU acceleration. RAM measurement via psutil proved unreliable on the benchmark's Linux host. With four models over three folds, the evaluation detects large effects but may miss smaller differences. Fashion-CLIP's mean mAP exceeds the upper 95% confidence bound of every other model, confirming statistically robust top-tier separation.

=== Summary

Five findings emerge from the benchmark:

1. *Domain-specific fine-tuning matters.* Fashion-CLIP's 6.1% relative mAP improvement over generic CLIP confirms that domain adaptation yields measurable gains.
2. *Architecture choice dominates the trade-off.* CNN and transformer-based models occupy distinct accuracy-efficiency regions; practitioners should choose family by operational constraints, then select the best model within that family.
3. *The pluggable model architecture is a practical enabler.* Switching models via one environment variable transforms evaluation into systematic comparison, enabling production A/B testing and graceful fallback.
4. *Commodity CPU hardware suffices.* Even CLIP models complete inference within 200 ms; combined with pgvector IVFFlat indexes (2.7--6.5 ms), total end-to-end latency stays under one second.
5. *Open-source tools are sufficient.* Pre-trained open-source models and pgvector deliver production-viable visual search without proprietary APIs or specialised hardware.

*Answer to RQ3.* The sidecar architecture successfully isolates ML inference from web application logic. The `EMBEDDING_MODEL` environment variable enables model switching without backend code changes. On CPU, end-to-end search latency stays under one second. Independent scaling and fault isolation were achieved without distributed infrastructure overhead, confirming that a polyglot architecture with a dedicated AI sidecar is viable for real-time interactive search on commodity hardware.
