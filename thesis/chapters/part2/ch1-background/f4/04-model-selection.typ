=== Model Selection and Justification

This section presents the comparative analysis of the embedding models and the rationale for selecting Fashion-CLIP as the primary model for visual search.

==== Candidate Models

Eleven pre-trained models spanning three architectural families were evaluated:

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: (left + horizon, center + horizon, center + horizon, center + horizon, center + horizon),
    stroke: 0.5pt,
    table.header([*Model*], [*Architecture*], [*Dim*], [*Parameters*], [*Training Method*]),
    [ResNet-50], [CNN], [2048], [25.6M], [Supervised (ImageNet)],
    [ResNet-101], [CNN], [2048], [44.5M], [Supervised (ImageNet)],
    [EfficientNet-B0], [CNN], [1280], [5.3M], [Supervised (ImageNet)],
    [EfficientNet-B4], [CNN], [1792], [19.3M], [Supervised (ImageNet)],
    [DINOv2 ViT-S/14], [ViT], [384], [21M], [Self-supervised (142M images)],
    [DINOv2 ViT-B/14], [ViT], [768], [86M], [Self-supervised (142M images)],
    [CLIP ViT-B/32], [ViT + Text], [512], [151M], [Contrastive (400M pairs)],
    [CLIP ViT-B/16], [ViT + Text], [512], [150M], [Contrastive (400M pairs)],
    [CLIP ViT-L/14], [ViT + Text], [768], [428M], [Contrastive (400M pairs)],
    [Fashion-CLIP], [ViT + Text], [512], [150M], [Fine-tuned (700K fashion)],
  ),
  caption: [Candidate embedding models evaluated for fashion product retrieval. All models are pre-trained and publicly available.],
) <tbl-candidate-models>

==== Evaluation Methodology

To ensure fair comparison, all models were evaluated under identical conditions. The evaluation used 5,000 fashion product images from the Fashion Product Images dataset, split into training and query sets. Hardware was consumer-grade: Intel i7-1165G7 CPU with 16 GB RAM, with all inference executed on CPU.

Metrics included:

- *Mean Average Precision (mAP\@10).* Primary metric measuring overall retrieval quality.
- *Precision at K (P\@1, P\@5, P\@10).* Accuracy of the top-K results.
- *Recall at 10 (R\@10).* Coverage of relevant items in the top-10 results.
- *Inference latency.* Average time to generate one embedding (milliseconds).

A retrieved product was considered relevant if it belonged to the same category as the query image. The full evaluation protocol, benchmark results, and cross-validation methodology are presented in Chapter 3.

==== Selection Criteria

The model selection was based on four criteria:

1. *Retrieval quality.* mAP\@10 and P\@K scores measuring how well the model retrieves visually similar products.
2. *Inference latency.* Must support real-time search with sub-300 ms total response time.
3. *Multimodal capability.* The ability to search by both image and text, enabling text-to-image queries.
4. *Hardware compatibility.* Must operate within the memory and compute constraints of commodity hardware.

==== Decision

*Fashion-CLIP* was selected as the primary embedding model for the visual search feature. Three factors drove this decision:

*Retrieval quality.* Fashion-CLIP achieved the highest mAP\@10 among the evaluated models, with a 15-to-20% improvement over general CLIP on fashion-specific queries. This result was confirmed through the systematic benchmark presented in Chapter 3.

*Multimodal capability.* Fashion-CLIP's dual-tower architecture enables search by image, by text description, and by hybrid image-plus-text queries. This capability is unavailable in vision-only models such as DINOv2 and EfficientNet.

*Domain specialization.* Fine-tuning on 700,000 fashion images gives Fashion-CLIP an understanding of fashion-specific vocabulary, styles, and garment attributes that general-purpose models lack.

While EfficientNet-B0 offers faster inference (suitable for ultra-low-latency mobile deployments) and DINOv2 excels at structural fidelity (useful for silhouette-based matching), Fashion-CLIP provides the best overall balance of retrieval quality, search flexibility, and inference performance for the target deployment scenario.

==== Alternative Deployment Scenarios

For different deployment contexts, alternative models may be preferred:

- *Ultra-low latency.* EfficientNet-B0 provides the fastest inference at 5.3M parameters. Trade-off: 3.4% lower mAP\@10 and no text-to-image search.
- *Maximum structural accuracy.* DINOv2 excels at shape and silhouette matching. Trade-off: no multimodal capability.
- *Non-fashion e-commerce.* General CLIP variants are suitable for multi-category marketplaces. Trade-off: lower accuracy on fashion-specific queries.
- *High-resource environments.* CLIP ViT-L/14 offers the largest model capacity. Trade-off: 428M parameters, requiring GPU with significant VRAM.

The complete numerical comparison and error analysis across all 11 models are presented in Chapter 3.
