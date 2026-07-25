== Retrieval Performance

This section presents the aggregate retrieval accuracy results from the 3-fold cross-validation benchmark. Table @tbl-aggregate displays the primary accuracy metrics for all four evaluated models, sorted by mAP in descending order.

#figure(
  caption: [Aggregate Retrieval Metrics, 3-Fold Cross-Validation],
  table(
    columns: 8,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP (mean ± SD)*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [Fashion-CLIP], [*0.7455 ± 0.0088*], [*0.7915*], [*0.7101*], [0.0000], [*0.2645*], [*0.3992*], [0.0000],
    [EfficientNet-B0], [0.7196 ± 0.0155], [0.7434], [0.6826], [0.0000], [0.2497], [0.3698], [0.0000],
    [ResNet-50], [0.7150 ± 0.0258], [0.7413], [0.6833], [0.0000], [0.2452], [0.3680], [0.0000],
    [CLIP-generic], [0.7026 ± 0.0222], [0.7503], [0.6792], [0.0000], [0.2486], [0.3812], [0.0000],
  ),
  kind: table,
) <tbl-aggregate>

Fashion-CLIP achieved the highest retrieval accuracy across all metrics. Its mAP of 0.7455 is 3.6% above EfficientNet-B0 (0.7196), 4.3% above ResNet-50 (0.7150), and 6.1% above the generic CLIP wrapper (0.7026). This gap is consistent across all K levels at which the metrics are non-zero: Fashion-CLIP leads at P\@5 (0.7915 vs the next-best 0.7503 from CLIP-generic), at P\@10 (0.7101 vs 0.6833 from ResNet-50), at R\@5 (0.2645 vs 0.2497), and at R\@10 (0.3992 vs 0.3812). The standard deviation of Fashion-CLIP's mAP (±0.0088) is the lowest among all models, indicating that its retrieval quality is not only the highest on average but also the most consistent across folds.

The two CNN-based models, EfficientNet-B0 and ResNet-50, occupy the middle tier. EfficientNet-B0 (mAP 0.7196) slightly outperforms ResNet-50 (mAP 0.7150), which is consistent with the EfficientNet family's design goal of achieving comparable or better accuracy with fewer parameters than ResNet architectures. However, ResNet-50 achieves marginally higher P\@10 (0.6833 vs 0.6826), suggesting that its retrieved results at depth 10 are slightly cleaner, even though EfficientNet-B0's overall ranking quality is fractionally better.

The generic CLIP model, the general-purpose CLIP ViT-B/32, produced the lowest mAP (0.7026) among the four models. Its P\@5 score (0.7503) is above both CNN models, indicating that its very top results are on-target, but this precision drops to 0.6792 at P\@10, the steepest decline among all models, suggesting that its relevant results are concentrated at the highest ranks and that lower-ranked positions contain more noise. Conversely, CLIP-generic achieves the highest R\@10 (0.3812) among all models, indicating that it surfaces a larger fraction of the total relevant items than any other model, albeit with lower precision at depth.

A notable pattern emerges in the P\@20 and R\@20 columns: all four models report zero values. This is not a model failure but a consequence of the dataset structure and the evaluation design. The dataset contains an average of 8.5 relevant items per query category in each fold. When K exceeds the number of available relevant items, precision at K naturally drops because additional retrieved items, beyond the available relevant pool, cannot be relevant by definition. Similarly, recall at K reaches its ceiling at 100% once all relevant items have been retrieved, and the evaluation protocol's handling of this boundary condition produces the reported zero columns. Section 3.5.3 discusses this phenomenon and its implications in more detail.

*Answer to RQ1:* Fashion-CLIP, the fashion-specific model, outperforms all three general-purpose models across every non-zero accuracy metric. The 4.3% mAP advantage over ResNet-50 and the 6.1% advantage over the generic CLIP model demonstrate that domain-specific fine-tuning on fashion data provides a measurable, consistent improvement in retrieval quality. Fashion-CLIP's mAP lower bound (mean minus two standard deviations: 0.7279) exceeds the mean mAP of ResNet-50 (0.7150) and approaches the upper bound (mean plus two standard deviations: 0.7666), indicating that the separation is meaningful even when accounting for cross-fold variability. The confidence interval of CLIP-generic (0.6582 to 0.7470) overlaps substantially with Fashion-CLIP's (0.7279 to 0.7631), though the means differ by 0.0429. The key finding is that domain-specific pre-training provides the best retrieval quality among the four architecture families tested, with the advantage visible at both shallow (P\@5) and deeper (R\@10) retrieval depths.
