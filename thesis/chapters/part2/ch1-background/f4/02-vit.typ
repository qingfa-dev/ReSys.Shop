=== Vision Transformers

Where CNNs use small local filters, *Vision Transformers* (ViTs) divide an image into a grid of fixed-size patches (e.g., 14 by 14 pixels). Each patch becomes a token, analogous to a word in a sentence, and the sequence is processed through transformer layers @dosovitskiy2020vit.

*Multi-head self-attention* computes relationships between all patch pairs simultaneously. This gives ViTs a global view from the first layer. A collar detail in one corner and a hemline pattern at the opposite edge are related directly, without intermediate layers. This capability is valuable for garment retrieval, where silhouette and drape matter as much as local texture.

*DINOv2*, developed by Meta AI, trains without human labels. A student-teacher self-distillation framework on 142 million uncurated images produces features that capture object silhouettes, part geometry, and garment boundaries without category labels @oquab2023dinov2. DINOv2 ViT-S/14 produces 384-dimensional embeddings (21M parameters); ViT-B/14 produces 768-dimensional embeddings (86M parameters).

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
  caption: [Vision Transformer models evaluated. CLIP variants are discussed in Section 1.3.3.],
) <tbl-vit-models>
