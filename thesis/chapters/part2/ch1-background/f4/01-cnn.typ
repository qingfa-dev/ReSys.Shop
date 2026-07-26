=== Convolutional Neural Networks

==== Hierarchical Feature Extraction

Convolutional neural networks (CNNs) process images through a hierarchy of learned filters: early layers detect edges and colour transitions, middle layers compose these into shapes and parts (lapels, button rows, sleeve boundaries), and deep layers recognise complete structures (dress vs. jacket, formal vs. casual) @he2016deep. This inductive bias toward locality and translation invariance makes CNNs parameter-efficient and data-efficient for visual recognition tasks.

==== ResNet: Residual Connections

Deeper networks capture richer features but suffer from vanishing gradients: training signals decay as they propagate backward through many layers. ResNet (Residual Network) solves this with *skip connections* that bypass convolutional blocks, adding the block input directly to its output @he2016deep. This identity path lets gradients flow unimpeded, enabling networks of 50, 101, or 152 layers to train effectively. ResNet-50, with 25.6 million parameters and 2,048-dimensional embeddings, remains a strong baseline for image retrieval.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_cnn-resnet.png", width: 90%),
  caption: [ResNet architecture with residual skip connections. Dashed arrows show the identity path bypassing each convolutional block, enabling gradient flow across very deep networks.],
) <fig-resnet-arch>

==== EfficientNet: Compound Scaling

Conventional scaling strategies enlarge a network along a single dimension: depth (more layers), width (more channels per layer), or resolution (larger input). EfficientNet introduces *compound scaling*, which balances all three simultaneously using a learned coefficient @tan2019efficientnet. This produces a family of models (B0 through B7) that achieve state-of-the-art accuracy with an order of magnitude fewer parameters than comparably accurate alternatives. EfficientNet-B0, the smallest variant evaluated in this thesis, uses 5.3 million parameters and produces 1,280-dimensional embeddings, making it suitable for CPU-only or memory-constrained deployments.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_efficientnet-b0.png", width: 70%),
  caption: [EfficientNet-B0 architecture. Seven stages of MBConv (Mobile Inverted Bottleneck) blocks extract hierarchical features. The Compound Scaling subgraph illustrates how depth, width, and resolution are jointly balanced across B0 through B7 model family.],
) <fig-efficientnet-arch>

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
  caption: [CNN-based models evaluated in this thesis],
) <tbl-cnn-models>
