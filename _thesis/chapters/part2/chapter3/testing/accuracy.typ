=== Search Accuracy Results

The main goal was to see how well each model finds relevant products. A result was counted as "relevant" if it was in the same category as the query image.

#figure(
  table(
    columns: (auto, auto, auto, auto),
    stroke: 0.5pt,
    align: center,
    [*Model*], [*mAP\@10*], [*Precision\@10*], [*Recall\@10*],
    [EfficientNet-B0], [0.773], [0.824], [0.178],
    [DINOv2], [0.782], [0.824], [0.184],
    [Fashion-CLIP], [0.799], [0.838], [0.186],
  ),
  caption: [Accuracy comparison between the three models],
  kind: table,
) <tab-accuracy>

==== Metric Interpretation and Statistical Significance

- *Fashion-CLIP performed best* with an mAP\@10 of 0.799 (approximately 80% average precision in top 10 results). This confirms that domain-specific training on fashion data provides measurable advantages.

- *DINOv2* achieved 0.782 mAP\@10, demonstrating that self-supervised pre-training effectively captures visual structure even without fashion-specific training.

- *EfficientNet-B0* reached 0.773 mAP\@10, showing that even general-purpose CNNs can achieve competitive results, though they are surpassed by transformer-based architectures.

The difference between Fashion-CLIP (0.799) and EfficientNet-B0 (0.773) represents a 3.4% improvement in retrieval quality. While DINOv2 and EfficientNet show similar mAP scores, Fashion-CLIP's advantage lies in its multimodal capability enabling text-to-image search.

=== Performance by Category

The accuracy varied quite a bit depending on what type of product was being searched:

#figure(
  table(
    columns: (auto, auto),
    stroke: 0.5pt,
    align: center,
    [*Category*], [*mAP\@10 (Fashion-CLIP)*],
    [Women's Apparel], [0.946],
    [Men's Apparel], [0.631],
    [Unisex Items], [0.648],
  ),
  caption: [Accuracy breakdown by category],
  kind: table,
) <tab-categorical>

The system performed notably better on women's apparel (94.6%) compared to men's apparel (63.1%). This might be because:

1. The Fashion-CLIP training data may have included more women's fashion
2. Women's fashion tends to have more distinct visual features (varied colors, patterns, silhouettes)
3. Men's fashion items often look similar to each other (e.g., polo shirts in different colors)

This is a limitation to be aware of. A production system might need additional work to improve performance on categories where the model currently struggles.

