=== Convolutional Neural Networks

Convolutional neural networks (CNNs) process images through a hierarchy of learned filters. Early layers detect simple patterns (edges, colour transitions, texture directions), middle layers compose these into shapes and parts (lapels, button rows, sleeve boundaries), and deep layers recognise complete structures (dress vs. jacket, formal vs. casual). This layered organisation mirrors aspects of biological vision and has proven remarkably effective for visual recognition @he2016deep.

*ResNet* (Residual Network) introduced skip connections that allow information to bypass layers, solving the degradation problem that plagued deeper networks. ResNet-50, the 50-layer variant used in this thesis, remains a strong baseline for image retrieval with 25.6 million parameters and 2,048-dimensional embeddings.

*EfficientNet* uses compound scaling to simultaneously balance network depth, width, and input resolution @tan2019efficientnet. EfficientNet-B0, the smallest variant evaluated here, achieves competitive accuracy with 5.3 million parameters and produces 1,280-dimensional embeddings. Its compact design makes it well-suited to CPU-only or memory-constrained deployments.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Family*], [*Model*], [*Parameters*], [*Embedding Dim*], [*Training Data*]),
    [CNN], [ResNet-50], [25.6M], [2048], [ImageNet (1.2M images)],
    [CNN], [ResNet-101], [44.5M], [2048], [ImageNet (1.2M images)],
    [CNN], [EfficientNet-B0], [5.3M], [1280], [ImageNet (1.2M images)],
    [CNN], [EfficientNet-B4], [19.3M], [1792], [ImageNet (1.2M images)],
  ),
  caption: [CNN-based models evaluated in this thesis],
) <tbl-cnn-models>
