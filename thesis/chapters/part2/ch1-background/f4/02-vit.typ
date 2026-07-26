=== Vision Transformers

==== Patch Embedding and Self-Attention

Where CNNs rely on small local filters (3 by 3 pixel windows), Vision Transformers (ViTs) take a fundamentally different approach: an image is divided into a grid of fixed-size patches (14 by 14 or 16 by 16 pixels), each flattened into a vector and treated as a token analogous to a word in a sentence @dosovitskiy2020vit. Position encodings are added to preserve spatial information, and the sequence is processed through transformer layers where *multi-head self-attention* computes pairwise relationships between all patches simultaneously. This gives ViTs a global receptive field from the first layer, enabling them to relate a collar detail at one corner to a hemline pattern at the opposite edge without intermediate layers -- a capability valuable for garment retrieval, where silhouette and drape matter as much as local texture.

==== DINOv2: Self-Supervised Pre-Training

Conventional ViTs are trained on large labelled datasets such as ImageNet. *DINOv2*, developed by Meta AI, instead uses self-supervised learning on 142 million uncurated images, requiring no human annotations @oquab2023dinov2. Training follows a student-teacher self-distillation framework: two views of the same image (global and local crops) are passed through student and teacher networks with identical architecture; the student learns to match the teacher's output distribution via a cross-entropy objective, while the teacher is updated as an exponential moving average of the student weights. DINOv2 produces 384-dimensional embeddings (ViT-S/14, 21M parameters) or 768-dimensional embeddings (ViT-B/14, 86M parameters). Its self-supervised features exhibit strong object-level structure -- capturing silhouettes, part geometry, and garment boundaries without being trained on category labels -- making it adaptable to fashion domains where curated labels are scarce.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_dinov2.png", width: 90%),
  caption: [DINOv2 architecture. Image patches pass through a 12-layer transformer encoder with multi-head self-attention and feed-forward MLP in each layer. The CLS token output produces a 384- or 768-dimensional feature vector. The model is trained via self-supervised student-teacher self-distillation on 142 million images, requiring no human labels.],
) <fig-dinov2-arch>

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
  caption: [Vision Transformer models evaluated in this thesis. CLIP ViT variants are discussed further in Section 1.3.3.],
) <tbl-vit-models>
