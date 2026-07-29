=== Vision Transformers

Vision Transformers (ViTs) apply the transformer architecture from natural language processing to images. This section explains how ViTs work and introduces DINOv2, a self-supervised model evaluated in this project.

==== Patch Embedding and Tokenization

Transformers were originally developed for NLP tasks such as translation and text generation. The key innovation was *self-attention*, which allows the model to consider relationships between all parts of the input simultaneously.

In 2020, researchers showed this approach could also work for images @dosovitskiy2020vit. The idea is straightforward:

#list(
  [Split the image into a grid of fixed-size patches (e.g., 14 by 14 or 16 by 16 pixels).],
  [Flatten each patch into a vector and treat it as a token, analogous to a word in a sentence.],
  [Add position encodings to preserve spatial information.],
  [Pass the sequence through transformer layers with multi-head self-attention.],
)

==== Global Context via Self-Attention

Unlike CNNs, which focus on local patterns, self-attention captures relationships across the entire image from the first layer. A CNN processes patches in order and mainly compares nearby regions. A ViT can directly compare any two patches, even if they are far apart.

For fashion, this means a ViT can better understand that the collar of a shirt and the cuffs should match, even though they are on opposite sides of the image. This capability is valuable for garment retrieval, where silhouette and drape matter as much as local texture.

==== DINOv2 and Self-Supervised Pre-Training

Most AI models are trained with supervised learning, where humans label images ("this is a dress," "this is a shoe"), and the model learns from those labels. This requires expensive manual annotation.

*Self-supervised learning* takes a different approach: the model learns from the images themselves, without human labels. DINOv2 uses a student-teacher self-distillation framework @oquab2023dinov2:

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

DINOv2 produces features that exhibit strong object-level structure: silhouettes, part geometry, and garment boundaries, without being trained on category labels. This makes it adaptable to fashion domains where curated labels are scarce.

==== Structural Fidelity for Fashion Retrieval

DINOv2 is particularly good at capturing *structural fidelity*: the shapes, silhouettes, and proportions of objects. For fashion, this means:

- Matching garments with similar cuts (A-line, fitted, oversized).
- Finding items with similar proportions (cropped vs. full-length).
- Ignoring colour differences when the shape is similar.

This is valuable for users who might want "a dress shaped like this one, but in a different colour."

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

Vision Transformers have different trade-offs compared to CNNs:

*Advantages:*
- Better at understanding global structure and long-range relationships.
- Learned without human labels, so potentially more general.
- Strong structural fidelity: captures shapes and silhouettes.

*Disadvantages:*
- Slower than CNNs (more computation required).
- Require larger input images for best results.
- May be overkill for simple colour/pattern matching.

Both CNNs and Vision Transformers learn from images alone, mapping visual features to embedding vectors. However, fashion search often involves natural language queries like "red floral summer dress" or "casual weekend outfit." This requires models that can understand both images and text. The next section introduces CLIP and Fashion-CLIP, which bridge this gap through multimodal learning.
