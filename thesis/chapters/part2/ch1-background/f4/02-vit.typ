=== Vision Transformers

While CNNs capture local patterns through their layered filter design, their reliance on small receptive fields (typically 3 by 3 pixel windows) limits their ability to model relationships between distant image regions. Vision Transformers (ViTs) address this by applying the *self-attention* mechanism that was originally developed for natural language processing to image data @dosovitskiy2020vit.

A ViT divides an image into a grid of fixed-size patches (typically 16 by 16 pixels), treats each patch as a "token" analogous to a word in a sentence, and passes the sequence through transformer layers. Self-attention computes pairwise relationships between all patches simultaneously. For fashion, this means a ViT can relate a collar detail in one corner to a hemline pattern at the opposite edge without needing many intermediate layers, a capability especially valuable for garment retrieval where global silhouette matters as much as local texture.

*DINOv2*, developed by Meta AI, takes a different approach: rather than training on human-labelled images, it uses self-supervised learning on a large, uncurated collection of images @oquab2023dinov2. The model learns visual features by solving a prediction task on its own representations, without ever seeing a category label. DINOv2 produces 384-dimensional embeddings (ViT-S variant) or 768-dimensional embeddings (ViT-B variant). Its self-supervised training makes it adaptable to domains where curated labels are scarce.

#figure(
  table(
    columns: (auto, auto, auto, auto, 1fr),
    align: center + horizon,
    table.header([*Family*], [*Model*], [*Parameters*], [*Embedding\
Dim*], [*Training Method*]),
    [ViT], [DINOv2 ViT-S/14], [21M], [384], [Self-supervised (142M images)],
    [ViT], [DINOv2 ViT-B/14], [86M], [768], [Self-supervised (142M images)],
    [ViT], [CLIP ViT-B/32], [151M], [512], [Contrastive (400M pairs)],
    [ViT], [CLIP ViT-B/16], [150M], [512], [Contrastive (400M pairs)],
    [ViT], [CLIP ViT-L/14], [428M], [768], [Contrastive (400M pairs)],
  ),
    kind: table,
  caption: [Vision Transformer models evaluated in this thesis],
) <tbl-vit-models>
