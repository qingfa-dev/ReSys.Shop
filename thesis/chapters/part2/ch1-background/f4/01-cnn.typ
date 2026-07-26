=== Convolutional Neural Networks

Convolutional neural networks (CNNs) have been the dominant architecture for computer vision. This section explains how CNNs extract features and introduces the specific variants evaluated: ResNet and EfficientNet.

==== Hierarchical Feature Extraction

A CNN processes an image through a stack of learned filters. Each filter is a small window (typically 3 by 3 pixels) that slides across the image detecting local patterns @he2016deep. This process builds increasingly complex representations:

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

Early layers might detect the edge of a sleeve, middle layers recognise a striped pattern, and late layers understand "a formal button-down shirt." This hierarchical organisation gives CNNs an *inductive bias* toward local patterns: they excel at detecting texture and colour but may miss relationships between distant image regions.

==== ResNet and Skip Connections

Deeper networks capture richer features but suffer from *vanishing gradients*: training signals decay as they propagate backward through many layers. ResNet (Residual Network) solves this with *skip connections*: identity paths that bypass convolutional blocks and add the block input directly to its output @he2016deep. This identity path lets gradients flow unimpeded, enabling networks of 50, 101, or 152 layers to train effectively.

ResNet-50, with 25.6 million parameters and 2,048-dimensional embeddings, remains a strong baseline for image retrieval. ResNet-101 (44.5M parameters) provides additional depth for comparison.

==== EfficientNet and Compound Scaling

Traditional scaling strategies enlarge a network along a single dimension: depth (more layers), width (more channels), or resolution (larger inputs). EfficientNet introduces *compound scaling*, which balances all three dimensions simultaneously using a learned coefficient @tan2019efficientnet. This produces a family of models (B0 through B7) that achieve competitive accuracy with far fewer parameters than conventional scaling.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/ml-01-efficientnet-b0.png", width: 80%),
  caption: [EfficientNet-B0 architecture showing the flow from input image to feature vector],
) <fig-efficientnet-arch>

EfficientNet-B0, the smallest variant, uses 5.3 million parameters and produces 1,280-dimensional embeddings. Its compact design makes it well suited to CPU-only deployments. EfficientNet-B4 (19.3M parameters, 1,792-dimensional embeddings) provides higher capacity at increased computational cost.

==== Evaluated CNN Variants

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
  caption: [CNN-based models evaluated],
) <tbl-cnn-models>

While CNNs excel at detecting local patterns through their convolutional layers, they have limitations in capturing long-range dependencies and global context. This motivated researchers to explore alternative architectures: vision transformers.
