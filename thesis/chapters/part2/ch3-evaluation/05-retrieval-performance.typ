== Retrieval Performance and Accuracy

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_map.png", width: 75%),
  caption: [mAP comparison across four evaluated models. Fashion-CLIP leads at 0.9309, followed by CLIP-generic (0.9115), EfficientNet-B0 (0.8895), and ResNet-50 (0.8857).],
) <fig-benchmark-map>

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_precision.png", width: 75%),
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
    [Fashion-CLIP], [*0.9309 ± 0.0068*], [*0.9582*], [*0.9493*], [*0.9374*], [*0.0280*], [*0.0483*], [*0.0810*],
    [CLIP-generic], [0.9115 ± 0.0077], [0.9440], [0.9364], [0.9239], [0.0264], [0.0459], [0.0768],
    [EfficientNet-B0], [0.8895 ± 0.0056], [0.9340], [0.9229], [0.9077], [0.0249], [0.0426], [0.0720],
    [ResNet-50], [0.8857 ± 0.0114], [0.9327], [0.9203], [0.9035], [0.0274], [0.0470], [0.0799],
  ),
  kind: table,
) <tbl-aggregate>

Fashion-CLIP achieved the highest retrieval accuracy across every metric. Its mAP of 0.9309 is 2.13% above CLIP-generic (0.9115), 4.65% above EfficientNet-B0 (0.8895), and 5.10% above ResNet-50 (0.8857). The advantage holds at all K values: P\@5 (0.9582 vs 0.9440), P\@10 (0.9493 vs 0.9364), and P\@20 (0.9374 vs 0.9239). Fashion-CLIP's standard deviation (±0.0068) is comparable to CLIP-generic (±0.0077), confirming both highest average quality and competitive cross-fold consistency.

CLIP-generic achieved second-highest mAP (0.9115), outperforming both CNN models by 2.47% over EfficientNet-B0 and 2.91% over ResNet-50. Contrastive pre-training on 400 million image-text pairs produces embeddings that generalise to fashion category retrieval without domain fine-tuning, though with higher cross-fold variability (±0.0077).

EfficientNet-B0 (0.8895) and ResNet-50 (0.8857) occupy the lowest accuracy tier, with P\@K and R\@K values tracking within 0.4% across all K levels. ResNet-50's higher embedding dimensionality (2,048 vs 1,280) does not improve category-level retrieval, consistent with higher dimensionality benefiting finer-grained distinctions.

Fashion-CLIP's mAP lower bound (mean minus two standard deviations: 0.9173) exceeds the upper bound of EfficientNet-B0 (0.9007) and ResNet-50 (0.9085), indicating meaningful separation. With only 3 folds, formal significance testing has limited power; these non-overlapping bounds are presented as indicative rather than conclusive.

*Answer to RQ1.* Fashion-CLIP outperforms all three general-purpose models across every accuracy metric. The 2.13% mAP advantage over the generic CLIP wrapper demonstrates that domain-specific fine-tuning provides measurable retrieval quality improvements not achieved by general-purpose contrastive pre-training alone. The gap is consistent at shallow (P\@5: 0.9582 vs 0.9440) and deeper (P\@20: 0.9374 vs 0.9239) retrieval depths with clean statistical separation from the CNN models: Fashion-CLIP's lower mAP bound (0.9173) exceeds the upper bound of EfficientNet-B0 (0.9007) and ResNet-50 (0.9085); the small overlap with CLIP-generic's upper bound (0.9269) is consistent with the narrower mean gap (2.13%) between the two CLIP variants.
