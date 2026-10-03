== MODEL SELECTION AND JUSTIFICATION <sec:model-selection>

This section presents the rationale for selecting Fashion-CLIP as the primary embedding model for the visual search feature, based on preliminary evaluation results and architectural considerations.

=== Candidate Models Evaluated

Four pre-trained models were evaluated for fashion product retrieval:

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: (left + horizon, center + horizon, center + horizon, center + horizon, center + horizon),
    stroke: 0.5pt,

    table.header([*Model*], [*Architecture*], [*Embedding Dim*], [*Training Method*], [*Domain*]),

    [CLIP ViT-B/16], [Vision Transformer], [512], [Contrastive (image-text)], [General],
    [DINOv2 ViT-S/14], [Vision Transformer], [384], [Self-supervised], [General],
    [EfficientNet-B0], [CNN], [1280], [Supervised (ImageNet)], [General],
    [Fashion-CLIP ViT-B/16], [Vision Transformer], [512], [Contrastive (fashion)], [Fashion-specific],
  ),
  caption: [
    Candidate embedding models evaluated for fashion product retrieval.
    All models are pre-trained and publicly available, enabling reproducible research.
  ],
 <tbl:candidate-models>

=== Evaluation Methodology

To ensure fair comparison, all models were evaluated using identical conditions:

*Dataset:* 5,000 fashion products from the project catalog, split into:
- Training set: 4,000 products (for index building)
- Validation set: 1,000 products (for evaluation)
- Test queries: 8,500 image-to-image searches

*Hardware:* NVIDIA MX330 GPU (2GB VRAM) - representative of commodity laptop hardware

*Metrics:*
- *Mean Average Precision (mAP\@10):* Primary metric measuring retrieval quality
- *Precision at K (P\@1, P\@5, P\@10):* Accuracy of top-K results
- *Recall at 10 (R\@10):* Coverage of relevant items in top-10
- *Inference Time:* Average embedding generation latency

*Relevance Criterion:* A retrieved product is considered relevant if it belongs to the same category as the query image.

=== Preliminary Evaluation Results

The preliminary evaluation on the validation split yielded the following results:

#figure(
  table(
    columns: (auto, auto, auto, auto, auto, auto, auto),
    align: (
      left + horizon,
      center + horizon,
      center + horizon,
      center + horizon,
      center + horizon,
      center + horizon,
      center + horizon,
    ),
    stroke: 0.5pt,

    table.header([*Model*], [*mAP\@10*], [*P\@1*], [*P\@5*], [*P\@10*], [*R\@10*], [*Inference (ms)*]),

    [CLIP ViT-B/16], [0.724], [0.833], [0.826], [0.816], [0.179], [60.3],
    [DINOv2 ViT-S/14], [*0.782*], [*0.897*], [0.852], [0.824], [0.184], [79.7],
    [EfficientNet-B0], [0.773], [0.882], [*0.860*], [*0.824*], [0.178], [*21.2*],
    [*Fashion-CLIP ViT-B/16*], [*0.799*], [0.892], [0.858], [*0.838*], [*0.186*], [67.7],
  ),
  caption: [
    Preliminary evaluation results on validation split (1,000 products, 8,500 queries).
    *Hardware Context:* NVIDIA MX330 GPU (2GB VRAM).
    Bold values indicate best performance per metric. Fashion-CLIP achieves the highest
    overall retrieval quality (mAP\@10) and best coverage (R\@10), with acceptable
    inference latency for real-time search.
  ],
  placement: auto,
  kind: table,
) <tbl:preliminary-results>

=== Analysis of Results

==== Retrieval Quality (mAP\@10)

Fashion-CLIP achieved the highest mAP\@10 of *0.799*, indicating superior overall retrieval quality:
- *+10.4%* improvement over general CLIP (0.724)
- *+3.4%* improvement over EfficientNet-B0 (0.773)
- *+2.2%* improvement over DINOv2 (0.782)

While DINOv2 shows the highest precision at top-1 (P\@1: 0.897), Fashion-CLIP demonstrates more balanced performance across all precision levels, making it more suitable for displaying multiple search results.

==== Coverage and Diversity (Recall\@10)

Fashion-CLIP achieves the highest recall (R\@10: 0.186), meaning it retrieves more relevant products in the top-10 results compared to other models. This is critical for e-commerce applications where users expect diverse options.

==== Inference Performance

While EfficientNet-B0 offers the fastest inference (21.2ms), Fashion-CLIP's 67.7ms latency is well within acceptable bounds for real-time search:
- Total search latency budget: ~300ms (including network, database query)
- Embedding generation: 67.7ms (~23% of budget)
- Remaining budget: 232ms for database query and response formatting

The 3× speed difference compared to EfficientNet is justified by the 3.4% improvement in retrieval quality.

=== Decision Criteria

The model selection was based on the following weighted criteria:

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (left + horizon, center + horizon, left + horizon),
    stroke: 0.5pt,

    [*Criterion*], [*Weight*], [*Rationale*],
    [Retrieval Quality (mAP\@10)], [40%], [Primary user experience metric],
    [Inference Latency], [25%], [Must support real-time search (\<100ms)],
    [Multimodal Capability], [20%], [Enables text-to-image search],
    [Hardware Compatibility], [10%], [Must run on commodity GPU],
    [Recall (Coverage)], [5%], [Diversity of search results],
  ),
  caption: [
    Model selection criteria with assigned weights reflecting business priorities.
  ],
  kind: table,
) <tbl:selection-criteria>

=== Selection Decision: Fashion-CLIP Based on Quantitative Evaluation

Based on the evaluation results and decision criteria, *Fashion-CLIP ViT-B/16* was selected as the production model for the following reasons:

1. *Highest Overall Retrieval Quality*
  - mAP\@10 of 0.799 surpasses all other candidates
  - Balanced precision across P\@1, P\@5, P\@10
  - Best recall (0.186) ensures diverse search results

2. *Domain Specialization Advantage*
  - Trained on 700,000+ fashion images with domain-specific vocabulary
  - Understands fashion concepts (silhouettes, styles, occasions)
  - Captures semantic similarity beyond visual appearance

3. *Multimodal Search Capability*
  - Dual-tower architecture (image encoder + text encoder)
  - Enables text-to-image search: "red floral summer dress"
  - Enables hybrid queries: image + text refinement
  - This capability is *unavailable* in DINOv2 and EfficientNet

4. *Acceptable Inference Performance*
  - 67.7ms embedding generation meets real-time requirements
  - 512-dimensional vectors balance expressiveness and efficiency
  - Compatible with commodity hardware (2GB VRAM)

5. *Production Readiness*
  - Pre-trained model available via Hugging Face
  - Well-documented API and inference pipeline
  - Active community support and updates

=== Trade-offs and Limitations

While Fashion-CLIP is the optimal choice for this project, it is important to acknowledge the trade-offs:

*Compared to DINOv2:*
- Lower P\@1 (0.892 vs 0.897) - slightly less accurate for single top result
- Slower inference (67.7ms vs 79.7ms) - acceptable difference
- *Advantage:* Multimodal capability and higher mAP\@10

*Compared to EfficientNet-B0:*
- 3× slower inference (67.7ms vs 21.2ms)
- Higher memory footprint (512-dim vs 1280-dim embeddings are actually smaller)
- *Advantage:* 3.4% better retrieval quality and semantic understanding

*Compared to General CLIP:*
- Similar inference speed (67.7ms vs 60.3ms)
- *Advantage:* 10.4% better retrieval quality due to fashion-specific training

=== Alternative Deployment Scenarios

For different deployment contexts, alternative models may be preferred:

- *Ultra-low latency requirement (\<30ms):* EfficientNet-B0
  - Use case: Mobile app with strict latency constraints
  - Trade-off: Accept 3.4% lower mAP\@10

- *Maximum accuracy (P\@1 priority):* DINOv2
  - Use case: "Find exact duplicate" feature
  - Trade-off: No text-to-image search capability

- *General e-commerce (non-fashion):* Standard CLIP
  - Use case: Multi-category marketplace
  - Trade-off: Lower accuracy on fashion-specific queries

=== Validation Plan

The preliminary results presented in @tbl:preliminary-results will be validated through:

1. *Full-scale evaluation* on the complete 5,000-product dataset
2. *Cross-validation* across different product categories
3. *User acceptance evaluation* with real search queries
4. *Performance benchmarking* under production load

The detailed validation results are presented in Chapter 3 (Evaluation and Validation).

=== Summary

Fashion-CLIP ViT-B/16 was selected as the production embedding model based on:
- *Quantitative evidence:* Highest mAP\@10 (0.799) and R\@10 (0.186)
- *Qualitative advantage:* Multimodal text-image search capability
- *Practical feasibility:* Acceptable inference latency (67.7ms) on commodity hardware

This data-driven selection process ensures that the chosen model balances retrieval quality, performance, and feature richness for the fashion e-commerce use case.

With the embedding model selected, the next challenge is storing and searching millions of product embeddings efficiently. The following sections cover the infrastructure components that enable fast vector similarity search and the full-stack implementation of the e-commerce platform.
