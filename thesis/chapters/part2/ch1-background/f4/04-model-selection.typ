=== Model Selection and Justification

The rationale for selecting Fashion-CLIP as the primary model for visual search is presented below.

==== Candidate Models

Eleven candidate pre-trained models spanning three architectural families were considered; six representative models were benchmarked:
#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: (left + horizon, center + horizon, center + horizon, center + horizon, center + horizon),
    stroke: 0.5pt,
    table.header([*Model*], [*Architecture*], [*Dim*], [*Parameters*], [*Training Method*]),
    [ResNet-50], [CNN], [2048], [25.6M], [Supervised (ImageNet)],
    [ResNet-101], [CNN], [2048], [44.5M], [Supervised (ImageNet)],
    [ResNet-152], [CNN], [2048], [60.2M], [Supervised (ImageNet)],
    [EfficientNet-B0], [CNN], [1280], [5.3M], [Supervised (ImageNet)],
    [EfficientNet-B4], [CNN], [1792], [19.3M], [Supervised (ImageNet)],
    [DINOv2 ViT-S/14], [ViT], [384], [21M], [Self-supervised (142M images)],
    [DINOv2 ViT-B/14], [ViT], [768], [86M], [Self-supervised (142M images)],
    [CLIP ViT-B/32], [ViT + Text], [512], [151M], [Contrastive (400M pairs)],
    [CLIP ViT-B/16], [ViT + Text], [512], [150M], [Contrastive (400M pairs)],
    [CLIP ViT-L/14], [ViT + Text], [768], [428M], [Contrastive (400M pairs)],
    [Fashion-CLIP], [ViT + Text], [512], [150M], [Fine-tuned (700K fashion)],
  ),
  caption: [Candidate pre-trained embedding models assessed for fashion product retrieval],
) <tbl-candidate-models>

==== Benchmark Methodology

All models were assessed under identical conditions: 5,000 fashion product images from the Fashion Product Images dataset, split into training and query sets. Hardware was consumer-grade: Intel i7-1165G7 CPU with 16 GB RAM, all inference on CPU.

Metrics included Mean Average Precision (mAP\@10) as the primary measure of overall retrieval quality, Precision at K (P\@K) for top-ranked accuracy, Recall at K (R\@K) for coverage of relevant items, and inference latency in milliseconds. A retrieved product was considered relevant if it belonged to the same category as the query image. The full benchmark protocol, results, and cross-validation methodology are presented in Chapter 3.

==== Weighted Selection Criteria

Model selection was based on four criteria: retrieval quality (mAP\@10 and P\@K scores), inference latency (sub-300 ms total response time), multimodal capability (search by image and text), and hardware compatibility within memory and compute constraints of commodity hardware.

==== Selection Decision

*Fashion-CLIP* was selected as the primary embedding model for the visual search feature. Three factors drove this decision.

First, retrieval quality: Fashion-CLIP achieved the highest mAP among the assessed models, outperforming generic CLIP ViT-B/16 by 1.46% under category-only relevance @chia2022fashionclip.

Second, multimodal capability: Fashion-CLIP's dual-tower architecture enables search by image, by text description, and by hybrid image-plus-text queries, unavailable in vision-only models such as DINOv2 and EfficientNet.

Third, domain specialization: fine-tuning on 700,000 fashion images gives Fashion-CLIP an understanding of fashion-specific vocabulary, styles, and garment attributes that general-purpose models lack.

Fashion-CLIP provides the strongest overall combination of retrieval quality, search flexibility, and inference performance for the target deployment scenario, though EfficientNet-B0 offers faster CPU inference and DINOv2 excels at structural fidelity for silhouette-based matching.

==== Alternative Deployment Scenarios

For different deployment contexts, alternative models may be preferred. EfficientNet-B0 provides the fastest inference at 5.3 million parameters, trading off 2.86% lower mAP with no text-to-image capability. DINOv2 excels at shape and silhouette matching but lacks multimodal capability. General CLIP variants suit multi-category marketplaces with lower fashion accuracy. CLIP ViT-L/14 offers the largest capacity at 428 million parameters but requires substantial GPU VRAM.

The complete numerical comparison and error analysis across all models are presented in Chapter 3.
