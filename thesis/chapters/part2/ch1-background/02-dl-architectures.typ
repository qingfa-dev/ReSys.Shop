== Deep Learning Architectures for Embedding Generation

Generating useful embeddings requires models that extract features at multiple levels, from low-level textures to high-level garment structure. Three families of architectures have emerged over the past decade.

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

=== Vision Transformers

While CNNs capture local patterns through their layered filter design, their reliance on small receptive fields (typically 3×3 pixel windows) limits their ability to model relationships between distant image regions. Vision Transformers (ViTs) address this by applying the *self-attention* mechanism that was originally developed for natural language processing to image data @dosovitskiy2020vit.

A ViT divides an image into a grid of fixed-size patches (typically 16×16 pixels), treats each patch as a "token" analogous to a word in a sentence, and passes the sequence through transformer layers. Self-attention computes pairwise relationships between all patches simultaneously. For fashion, this means a ViT can relate a collar detail in one corner to a hemline pattern at the opposite edge without needing many intermediate layers, a capability especially valuable for garment retrieval where global silhouette matters as much as local texture.

*DINOv2*, developed by Meta AI, takes a different approach: rather than training on human-labelled images, it uses self-supervised learning on a large, uncurated collection of images @oquab2023dinov2. The model learns visual features by solving a prediction task on its own representations, without ever seeing a category label. DINOv2 produces 384-dimensional embeddings (ViT-S variant) or 768-dimensional embeddings (ViT-B variant). Its self-supervised training makes it adaptable to domains where curated labels are scarce.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Family*], [*Model*], [*Parameters*], [*Embedding Dim*], [*Training Method*]),
    [ViT], [DINOv2 ViT-S/14], [21M], [384], [Self-supervised (142M images)],
    [ViT], [DINOv2 ViT-B/14], [86M], [768], [Self-supervised (142M images)],
    [ViT], [CLIP ViT-B/32], [151M], [512], [Contrastive (400M pairs)],
    [ViT], [CLIP ViT-B/16], [150M], [512], [Contrastive (400M pairs)],
    [ViT], [CLIP ViT-L/14], [428M], [768], [Contrastive (400M pairs)],
  ),
  caption: [Vision Transformer models evaluated in this thesis],
) <tbl-vit-models>

=== CLIP and Fashion-CLIP: Bridging Vision and Language

CNNs and ViTs operate purely in the visual domain, mapping images to embedding spaces that have no connection to human language. *CLIP* (Contrastive Language-Image Pre-training) bridges this gap through a dual-tower architecture: one tower encodes images, a parallel text encoder processes natural language descriptions, and both are trained jointly on 400 million (image, caption) pairs from the public web @radford2021learning. A contrastive objective pulls matching image-text pairs together in a shared embedding space while pushing non-matching pairs apart. The result is a model that can both see and read.

*Fashion-CLIP* extends CLIP by fine-tuning it on over 700,000 fashion images paired with domain-specific text descriptions @chia2022fashionclip. The fine-tuning adjusts model weights to emphasise fashion-relevant attributes (garment categories, fabric textures, style descriptors, occasion labels) while retaining general visual understanding. Fashion-CLIP uses the ViT-B/16 architecture inherited from CLIP, producing 512-dimensional embeddings. The original paper reports a 15 to 20% improvement on fashion retrieval over general CLIP, a result confirmed in the benchmark evaluation presented in Chapter 3 of this thesis.

The dual-tower design also enables *multimodal queries* unavailable in pure vision models like DINOv2 or EfficientNet. A user searching for "red floral summer dress" does not need a reference image; the text encoder maps the description directly into the same embedding space as catalog images. A hybrid query combining an uploaded photo with a textual refinement, "like this, but in blue," becomes possible by encoding both modalities and merging the results.

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Model*], [*Architecture*], [*Training*], [*Domain*]),
    [CLIP ViT-B/32], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-B/16], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-L/14], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [Fashion-CLIP], [Dual-tower (ViT + text transformer)], [Contrastive, fine-tuned on 700K fashion images], [Fashion-specific],
  ),
  caption: [CLIP variants evaluated in this thesis],
) <tbl-clip-models>
