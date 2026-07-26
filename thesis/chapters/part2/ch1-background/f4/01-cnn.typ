=== Convolutional Neural Networks

A *convolutional neural network* (CNN) processes an image through a stack of learned filters. Early layers detect edges and colour transitions. Middle layers compose these into shapes: lapels, button rows, sleeve boundaries. Deep layers recognise complete structures: dress versus jacket, formal versus casual @he2016deep.

*ResNet* solves the vanishing gradient problem of deep networks with *skip connections*: identity paths that bypass convolutional blocks and let training signals flow unimpeded @he2016deep. ResNet-50 (25.6M parameters, 2,048-dimensional embeddings) remains a strong baseline for image retrieval.

*EfficientNet* uses *compound scaling* to balance network depth, width, and input resolution simultaneously, achieving competitive accuracy with far fewer parameters than conventional scaling @tan2019efficientnet. EfficientNet-B0 (5.3M parameters, 1,280-dimensional embeddings) is the smallest variant evaluated and is well suited to CPU-only deployments.

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
