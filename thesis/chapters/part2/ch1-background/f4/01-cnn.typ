=== Convolutional Neural Networks

CNNs have dominated computer vision since AlexNet (2012). Variants benchmarked: ResNet-50, ResNet-101, EfficientNet-B0, and EfficientNet-B4.

==== Hierarchical Feature Extraction

A CNN processes an image through a stack of learned filters. Each filter is a small window (typically 3 by 3 pixels) sliding across the image detecting local patterns @he2016deep, building increasingly complex representations:
#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (center, left),
    [*Layer*], [*What It Detects*],
    [Early layers], [Simple patterns: edges, colours, corners],
    [Middle layers], [Combinations: textures, shapes, parts (lapels, sleeves)],
    [Late layers], [High-level features: garment categories, styles],
  ),
  caption: [What different CNN layers learn to detect in fashion images],
)

Local patterns cascade into global understanding: edges become textures in middle layers, garments in late layers. CNNs have a strong *inductive bias* toward local patterns: they excel at texture and colour but may miss relationships between distant image regions.

==== ResNet and Skip Connections

Deeper networks capture richer features but suffer from *vanishing gradients*: training signals decay as they propagate backward through many layers. ResNet solves this with *skip connections*: identity paths that bypass convolutional blocks and add the block input directly to its output @he2016deep. This lets gradients flow unimpeded, enabling 50-, 101-, or 152-layer networks to train effectively.
#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/diagrams/P2S2.1.4_cnn-resnet.png", width: 100%),
  caption: [ResNet architecture with residual skip connections],
) <fig-resnet-arch>

ResNet-50 (25.6M parameters, 2,048-dim embeddings) remains a strong baseline for image retrieval. ResNet-101 (44.5M parameters) provides additional depth for comparison.

==== EfficientNet and Compound Scaling

Traditional scaling enlarges a network along one dimension: depth, width, or resolution. EfficientNet introduces *compound scaling*, balancing all three simultaneously using a learned coefficient @tan2019efficientnet. This produces models (B0 through B7) with competitive accuracy and far fewer parameters.
#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/diagrams/P2S2.1.4_efficientnet-b0.png", height: 55%),
  caption: [EfficientNet-B0 architecture: input image to feature vector],
) <fig-efficientnet-arch>

EfficientNet-B0 uses 5.3M parameters and 1,280-dim embeddings, well suited to CPU-only deployments. EfficientNet-B4 (19.3M parameters, 1,792-dim embeddings) provides higher capacity.

==== Benchmarked CNN Variants

#figure(
  table(
    columns: (auto, auto, auto, auto, 1fr),
    align: center + horizon,
    table.header([*Family*], [*Model*], [*Parameters*], [*Embedding\
 Dim*], [*Training Data*]),
    [CNN], [ResNet-50], [25.6M], [2048], [ImageNet (1.2M images)],
    [CNN], [ResNet-101], [44.5M], [2048], [ImageNet (1.2M images)],
    [CNN], [EfficientNet-B0], [5.3M], [1280], [ImageNet (1.2M images)],
    [CNN], [EfficientNet-B4], [19.3M], [1792], [ImageNet (1.2M images)],
  ),
    kind: table,
  caption: [CNN-based models benchmarked],
) <tbl-cnn-models>
