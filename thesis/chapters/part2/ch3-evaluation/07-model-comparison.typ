== Synthesis, Deployment Strategy, and Limitations

=== Accuracy-Efficiency Trade-off

@tbl-comparison presents the combined accuracy and efficiency view. Looking at accuracy and efficiency together, two distinct clusters appear: a transformer-based top tier and a CNN-based lower tier.

#figure(
  caption: [Combined Accuracy and Efficiency Comparison],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@10*], [*R\@10*], [*Latency (ms)*], [*Throughput*], [*Load (ms)*], [*Storage (MB)*],
    ),
    [FashionCLIP], [*0.9336*], [*0.9527*], [*0.0488*], [113.6], [14.2], [5 109.9], [3.3],
    [DINOv2 ViT-S/14], [0.9299], [0.9491], [0.0484], [126.3], [10.2], [1 223.4], [2.4],
    [CLIP ViT-B/16], [0.9202], [0.9423], [0.0474], [235.5], [4.0], [6 518.5], [3.3],
    [CLIP ViT-B/32], [0.9184], [0.9408], [0.0474], [140.5], [11.9], [1 868.8], [3.3],
    [ResNet-50], [0.9132], [0.9364], [0.0452], [96.6], [10.2], [385.6], [13.0],
    [EfficientNet-B0], [0.9077], [0.9342], [0.0443], [*42.6*], [*21.4*], [*118.3*], [8.1],
  ),
  kind: table,
) <tbl-comparison>

Fashion-CLIP stands alone in the high-accuracy cluster at the top: mAP 0.9336 leads every other model, while its 113.6 ms inference time remains within the sub-second interactive budget. The four transformer-based models (Fashion-CLIP, DINOv2 ViT-S/14, CLIP ViT-B/16, CLIP ViT-B/32) form a tightly packed top tier spanning 0.9184--0.9336, separated from the two CNN models (ResNet-50 0.9132, EfficientNet-B0 0.9077) by roughly half a percentage point. EfficientNet-B0 delivers the fastest inference (42.6 ms) and highest throughput (21.4 img/s) but the lowest mAP (0.9077).

=== Deployment Recommendations

For retrieval quality, Fashion-CLIP is recommended (mAP 0.9336). Its 113.6 ms inference is acceptable for interactive search; the pluggable model architecture enables lazy-loading and embedding caching. For CPU-only or latency-sensitive deployments, EfficientNet-B0 is recommended: 42.6 ms inference achieves 97.2% of Fashion-CLIP's mAP at 37.5% of the latency, with 118.3 ms load time enabling rapid cold-start recovery. DINOv2 ViT-S/14 is a strong, lighter-weight alternative for coarse category retrieval, matching Fashion-CLIP within 0.40% on category-only mAP at roughly half the load time. The pluggable configuration mechanism (Section 2.4) lets you switch between recommendations by changing a single environment variable; embeddings tagged by model name allow multiple models to coexist.

=== Limitations

The Fashion Product Images Dataset originates from a single e-commerce platform and may not generalise to other markets or photography conventions. Its category distribution is strongly imbalanced (Apparel 2,500, Accessories 1,250, Footwear 750, Personal Care 350, Sporting Goods 150), so retrieval scores are dominated by the heavily represented apparel class; rankings on a more balanced catalogue may differ. The binary category-label relevance criterion is an imperfect stand-in for true visual similarity: same-category products may be visually dissimilar, and different-category products may share strong visual features. As shown in @tbl-groundtruth, retrieval scores are highly sensitive to the ground-truth definition: absolute mAP falls from ≈0.93 under category-only labels to ≈0.20-0.25 under category + colour and ≈0.16-0.21 under category + colour + pattern, with the model ranking reordering as fine attributes dominate. All inference figures are tied to the specific CPU configuration without GPU acceleration. RAM figures are approximate, estimated from model parameter counts because direct psutil measurement proved unreliable on the benchmark's Linux host. With six models over three folds, the benchmark detects large effects but may miss smaller differences. Using the non-overlapping-bounds heuristic (mean ± two standard deviations), Fashion-CLIP's lower mAP bound (0.9216) overlaps the upper bounds of every other model, including both CNNs, so no model is statistically separable from the field on category-only retrieval; Fashion-CLIP should be described as having the highest observed mean mAP rather than a demonstrated superiority.

=== Summary

Six findings emerge from the six-model benchmark:

1. *Domain-specific fine-tuning matters.* Fashion-CLIP's 1.46% relative mAP improvement over generic CLIP ViT-B/16 shows that domain adaptation yields measurable gains, though the gap to the self-supervised DINOv2 ViT-S/14 is only 0.40%.
2. *Architecture choice dominates the trade-off.* CNN and transformer-based models sit in distinct accuracy-efficiency ranges; practitioners should choose family by operational constraints, then select the best model within that family.
3. *Ground-truth definition reshapes the ranking.* DINOv2 ViT-S/14 is competitive under coarse category labels but collapses under fine colour/pattern labels, whereas the CLIP family stays robust, a reminder that benchmark conclusions depend on the relevance criterion.
4. *The pluggable model architecture is genuinely useful in practice.* Switching models via one environment variable transforms model assessment into systematic comparison, enabling production A/B testing and graceful fallback.
5. *Commodity CPU hardware is enough.* Even CLIP models complete inference within 240 ms; combined with pgvector IVFFlat indexes (2.7-6.5 ms), total end-to-end latency stays under one second.
6. *Open-source tools are enough.* Pre-trained open-source models and pgvector provide production-oriented visual search without proprietary APIs or specialised hardware.

*Answer to RQ3.* The sidecar architecture successfully isolates ML inference from web application logic. The `EMBEDDING_MODEL` environment variable enables model switching without backend code changes. On CPU, end-to-end search latency stays under one second across all six benchmarked models. Independent scaling and fault isolation are enabled by the sidecar boundary without distributed infrastructure overhead; this study demonstrates the latency and model-separation behaviour, while treating the scaling and isolation guarantees as architectural design properties rather than experimentally stress-tested results. The design nonetheless shows that a polyglot architecture with a dedicated AI sidecar is viable for real-time interactive search on commodity hardware. This viability assumes a team with at least one engineer comfortable operating a Python service alongside the .NET deployment; for a very small team without that capability, the overhead of maintaining a second language runtime may outweigh the architectural benefit.
