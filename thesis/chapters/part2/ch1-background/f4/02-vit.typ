=== Vision Transformers

Vision Transformers (ViTs) apply the transformer architecture from NLP to images, replacing convolutional filters with self-attention over image patches.

==== Patch Embedding and Tokenization

Transformers were originally developed for NLP tasks such as translation and text generation. Their key innovation is *self-attention*, allowing the model to consider relationships between all parts of the input simultaneously. In 2020, researchers showed this approach also works for images @dosovitskiy2020vit:
#list(
  [Split the image into a grid of fixed-size patches (e.g., 14 by 14 or 16 by 16 pixels).],
  [Flatten each patch into a vector and treat it as a token, analogous to a word in a sentence.],
  [Add position encodings to preserve spatial information.],
  [Pass the sequence through transformer layers with multi-head self-attention.],
)

==== Global Context via Self-Attention

Unlike CNNs, which focus on local patterns, self-attention captures relationships across the entire image from the first layer. A ViT can directly compare any two patches, even if far apart. For fashion, this means understanding that collar and cuffs match across opposite sides of an image: valuable for garment retrieval where silhouette and drape matter as much as local texture.

==== DINOv2 and Self-Supervised Pre-Training

Where supervised learning requires expensive human labels, *self-supervised learning* learns from the images themselves. DINOv2 uses a student-teacher self-distillation framework @oquab2023dinov2:
#list(
  [Take an image and create two different views (different crops, slightly different colours).],
  [Pass them through student and teacher networks with identical architecture.],
  [Train the student to match the teacher's output.],
  [Update the teacher as an exponential moving average of the student weights.],
  [Repeat across 142 million uncurated images.],
)
#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/diagrams/P2S2.1.4_dinov2.png", width: 80%),
  caption: [DINOv2 architecture: image patches pass through transformer layers with self-attention to produce a feature vector],
) <fig-dinov2-arch>

DINOv2 produces features with strong object-level structure: silhouettes, part geometry, and garment boundaries without category labels, making it adaptable to fashion domains where curated labels are scarce.

==== Structural Fidelity for Fashion Retrieval

DINOv2 excels at *structural fidelity*: shapes, silhouettes, and proportions. It matches garments by cut (A-line, fitted, oversized), by proportion (cropped vs. full-length), and separates shape from colour; useful for finding a dress in a different colour with the same shape.

==== DINOv2 Model Specifications

#figure(
  table(
    columns: (auto, auto, auto),
    stroke: 0.5pt,
    align: (left + horizon, center + horizon, center + horizon),
    [*Property*], [*DINOv2 ViT-S/14*], [*DINOv2 ViT-B/14*],
    [Architecture], [Vision Transformer (Small)], [Vision Transformer (Base)],
    [Parameters], [21M], [86M],
    [Patch size], [14 by 14 pixels], [14 by 14 pixels],
    [Embedding dimension], [384], [768],
    [Training data], [142M images], [142M images],
    [Training method], [Self-supervised], [Self-supervised],
  ),
  caption: [DINOv2 model specifications evaluated in this project],
)

==== Trade-offs and Limitations

Vision Transformers trade off differently than CNNs:

*Advantages:*
- Better at understanding global structure and long-range relationships.
- Learned without human labels, so potentially more general.
- Strong structural fidelity: captures shapes and silhouettes.

*Disadvantages:*
- Slower than CNNs (more computation required).
- Require larger input images for best results.
- May be overkill for simple colour/pattern matching.
