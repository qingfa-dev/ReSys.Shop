=== Model Accuracy Assessment

The core hypothesis of this project was that domain-specific models would outperform general-purpose ones for fashion retrieval. The following table presents the Mean Average Precision (mAP\@10) scores collected from the evaluation subset (500 representative images).

#figure(
  table(
    columns: (auto, auto, auto, auto, auto, auto, auto),
    align: (left, center, center, center, center, center, center),
    stroke: 0.5pt,

    // Header
    table.header([*Model Architecture*], [*mAP\@10*], [*P\@1*], [*P\@5*], [*P\@10*], [*R\@10*], [*Inference (ms)*]),

    // Data rows (from final_results.csv - actual experimental data)
    [CLIP ViT-B/16], [0.642], [0.866], [0.824], [0.802], [0.097], [60.7],
    [DINOv2 ViT-S/14], [0.706], [*0.896*], [0.849], [0.816], [0.102], [94.8],
    [EfficientNet-B0], [0.648], [0.855], [0.811], [0.781], [0.097], [*31.9*],
    [*Fashion-CLIP ViT-B/16*], [*0.725*], [0.888], [*0.864*], [*0.839*], [*0.110*], [59.4],
  ),
  caption: [
    Performance comparison of embedding models on fashion product retrieval task.
    Dataset: 5,000 products with 8,500 test queries evaluated on validation split.
    Hardware: NVIDIA MX330 GPU (2GB VRAM).
    Metrics: mAP = Mean Average Precision, P = Precision at K, R = Recall at K.
    Bold values indicate best performance per metric.
    Fashion-CLIP selected for production deployment due to optimal balance of retrieval quality (mAP\@10: 0.725)
    and inference speed (59.4ms), combined with multimodal text-image search capability unavailable in DINOv2.
  ],
  placement: auto,
  kind: table,
) <tbl:model-comparison>

The quantitative evaluation demonstrates that *Fashion-CLIP* achieves the highest overall retrieval quality with mAP\@10 of *0.725*, surpassing both the general-purpose CLIP (0.642) and DINOv2 (0.706) on the full validation set (N=5,000). While DINOv2 achieves slightly superior precision at top-1 (P\@1: 0.896), Fashion-CLIP demonstrates more balanced performance across all precision metrics and achieves the highest recall (R\@10: 0.110), indicating better coverage of relevant products in the top-10 results.

#figure(
  image("/images/charts/results/accuracy-comparison.png", width: 100%),
  caption: [Visual comparison of mAP\@10 scores across different model architectures.],
)
