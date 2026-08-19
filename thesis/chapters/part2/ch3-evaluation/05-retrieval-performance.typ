== Retrieval Performance and Accuracy

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_map.png", width: 100%),
  caption: [mAP comparison across four evaluated models. Fashion-CLIP leads at 0.8788, followed by CLIP-generic (0.8341), EfficientNet-B0 (0.8158), and ResNet-50 (0.8120).],
) <fig-benchmark-map>

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_precision.png", width: 100%),
  caption: [Precision at K (K = 5, 10, 20) across four evaluated models. Fashion-CLIP maintains the highest precision at every retrieval depth.],
) <fig-benchmark-precision>

#figure(
  caption: [Aggregate Retrieval Metrics, 3-Fold Cross-Validation],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP (mean ± SD)*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [Fashion-CLIP], [*0.8788 ± 0.0022*], [*0.9304*], [*0.9155*], [*0.8982*], [*0.0350*], [*0.0646*], [*0.1155*],
    [CLIP-generic], [0.8341 ± 0.0043], [0.9025], [0.8862], [0.8640], [0.0322], [0.0597], [0.1052],
    [EfficientNet-B0], [0.8158 ± 0.0007], [0.8901], [0.8703], [0.8477], [0.0316], [0.0571], [0.1001],
    [ResNet-50], [0.8120 ± 0.0052], [0.8841], [0.8671], [0.8458], [0.0306], [0.0551], [0.0978],
  ),
  kind: table,
) <tbl-aggregate>

Fashion-CLIP achieved the highest retrieval accuracy across every metric. Its mAP of 0.8788 is 5.4% above CLIP-generic (0.8341), 7.7% above EfficientNet-B0 (0.8158), and 8.2% above ResNet-50 (0.8120). The advantage holds at all K values: P\@5 (0.9304 vs 0.9025), P\@10 (0.9155 vs 0.8862), and P\@20 (0.8982 vs 0.8640). Fashion-CLIP's standard deviation (±0.0022) is the lowest among all models, less than half of CLIP-generic (±0.0043), confirming both highest average quality and greatest cross-fold consistency.

CLIP-generic achieved second-highest mAP (0.8341), outperforming both CNN models by 2.2% over EfficientNet-B0 and 2.7% over ResNet-50. Contrastive pre-training on 400 million image-text pairs produces embeddings that generalise to fashion category retrieval without domain fine-tuning, though with higher cross-fold variability (±0.0043).

EfficientNet-B0 (0.8158) and ResNet-50 (0.8120) occupy the lowest accuracy tier, with P\@K and R\@K values tracking within 0.7% across all K levels. ResNet-50's higher embedding dimensionality (2,048 vs 1,280) does not improve category-level retrieval, consistent with higher dimensionality benefiting finer-grained distinctions.

Fashion-CLIP's mAP lower bound (mean minus two standard deviations: 0.8744) exceeds the upper bound of EfficientNet-B0 (0.8172) and ResNet-50 (0.8224), confirming statistically meaningful separation.

*Answer to RQ1.* Fashion-CLIP outperforms all three general-purpose models across every accuracy metric. The 5.4% mAP advantage over the generic CLIP wrapper demonstrates that domain-specific fine-tuning provides measurable retrieval quality improvements not achieved by general-purpose contrastive pre-training alone. The gap is consistent at shallow (P\@5: 0.9304 vs 0.9025) and deeper (P\@20: 0.8982 vs 0.8640) retrieval depths with clean statistical separation: Fashion-CLIP's lower mAP bound (0.8744) exceeds the upper bound of every other model.
