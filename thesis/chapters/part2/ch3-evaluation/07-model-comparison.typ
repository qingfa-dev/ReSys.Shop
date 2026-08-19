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
    [Fashion-CLIP], [*0.9309*], [*0.9493*], [*0.0483*], [96.8], [18.5], [5 255.4], [3.3],
    [CLIP-generic], [0.9115], [0.9364], [0.0459], [86.6], [21.4], [6 848.5], [3.3],
    [EfficientNet-B0], [0.8895], [0.9229], [0.0426], [*37.8*], [*30.2*], [*110.2*], [8.1],
    [ResNet-50], [0.8857], [0.9203], [0.0470], [61.9], [13.5], [374.1], [13.0],
  ),
  kind: table,
) <tbl-comparison>

Fashion-CLIP occupies the high-accuracy cluster alone: mAP 0.9309 leads every other model by at least 2.13%, while its 96.8 ms inference time remains within the sub-second interactive budget. CLIP-generic (0.9115) and EfficientNet-B0 (0.8895) form the middle tier from different architecture families: CLIP-generic achieves higher accuracy, EfficientNet-B0 delivers fastest inference (37.8 ms) and highest throughput (30.2 img/s). ResNet-50 occupies the lowest tier on all dimensions: lowest mAP (0.8857), lowest throughput (13.5 img/s), largest storage (13.0 MB).

=== Deployment Recommendations

For retrieval quality, Fashion-CLIP is recommended (mAP 0.9309). Its 96.8 ms inference is acceptable for interactive search; the pluggable model architecture enables lazy-loading and embedding caching. For CPU-only or latency-sensitive deployments, EfficientNet-B0 is recommended: 37.8 ms inference achieves 95.55% of Fashion-CLIP's mAP at 39.1% of the latency, with 110.2 ms load time enabling rapid cold-start recovery. The pluggable configuration mechanism (Section 2.3) enables transitioning between recommendations by changing a single environment variable; embeddings tagged by model name allow multiple models to coexist.

=== Limitations

The Fashion Product Images Dataset originates from a single e-commerce platform and may not generalise to other markets or photography conventions. The binary category-label relevance criterion is a coarse proxy: same-category products may be visually dissimilar, and different-category products may share strong visual features. All inference figures are tied to the specific CPU configuration without GPU acceleration. RAM measurement via psutil proved unreliable on the benchmark's Linux host. With four models over three folds, the evaluation detects large effects but may miss smaller differences. Fashion-CLIP's mean mAP exceeds the upper 95% confidence bound of every other model, confirming statistically robust top-tier separation.

=== Summary

Five findings emerge from the benchmark:

1. *Domain-specific fine-tuning matters.* Fashion-CLIP's 2.13% relative mAP improvement over generic CLIP confirms that domain adaptation yields measurable gains.
2. *Architecture choice dominates the trade-off.* CNN and transformer-based models occupy distinct accuracy-efficiency regions; practitioners should choose family by operational constraints, then select the best model within that family.
3. *The pluggable model architecture is a practical enabler.* Switching models via one environment variable transforms evaluation into systematic comparison, enabling production A/B testing and graceful fallback.
4. *Commodity CPU hardware suffices.* Even CLIP models complete inference within 200 ms; combined with pgvector IVFFlat indexes (2.7--6.5 ms), total end-to-end latency stays under one second.
5. *Open-source tools are sufficient.* Pre-trained open-source models and pgvector deliver production-viable visual search without proprietary APIs or specialised hardware.

*Answer to RQ3.* The sidecar architecture successfully isolates ML inference from web application logic. The `EMBEDDING_MODEL` environment variable enables model switching without backend code changes. On CPU, end-to-end search latency stays under one second. Independent scaling and fault isolation were achieved without distributed infrastructure overhead, confirming that a polyglot architecture with a dedicated AI sidecar is viable for real-time interactive search on commodity hardware.
