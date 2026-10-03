== VISION TRANSFORMERS

Vision Transformers (ViT) represent a paradigm shift in computer vision, applying the transformer architecture from natural language processing to images. This section introduces the ViT approach and explains DINOv2, a state-of-the-art self-supervised model evaluated in this project.

=== From Text to Images

Transformers were originally developed for natural language processing (NLP), encompassing tasks such as translation and text generation @vaswani2017attention. The key innovation was *self-attention*, which allows the model to consider relationships between all parts of the input simultaneously.

In 2020, researchers showed that this approach could also work for images @dosovitskiy2020image. The idea is simple:

1. Split the image into small patches (e.g., 16×16 pixels)
2. Treat each patch like a "word" in a sentence
3. Apply the Transformer architecture to learn relationships between patches

#figure(
  image("/images/diagrams/01-ml-models/ml-02-dinov2.png", width: 86%),
  caption: [Vision Transformer converts an image into patches and uses self-attention to understand relationships between them],
) <fig-dinov2-arch>

=== Self-Attention Mechanism and Global Context Modeling

Unlike CNNs, which focus on local patterns, self-attention can capture relationships across the entire image. For example:

- A CNN processes patches in order and mainly compares nearby regions
- A Vision Transformer can directly compare any two patches, even if they are far apart

For fashion, this means a ViT can better understand that the collar of a shirt and the cuffs should match, even though they are on opposite sides of the image.

=== Self-Supervised Learning with DINOv2

Most AI models are trained with supervised learning, where humans label images ("this is a dress," "this is a shoe"), and the model learns from those labels. This requires expensive manual labeling.

*Self-supervised learning* takes a different approach: the model learns from the images themselves, without human labels. DINOv2 uses a clever technique @oquab2023dinov2:

1. Take an image and create two different views (different crops, slightly different colors)
2. Train the model to recognize that both views represent the same thing
3. Repeat with millions of images

This teaches the model to focus on the *content* of images rather than superficial details like exact cropping or lighting conditions.

=== DINOv2 for Fashion

DINOv2 is particularly good at capturing what researchers call *structural fidelity*, which refers to the shapes, silhouettes, and proportions of objects. For fashion, this means:

- Matching garments with similar cuts (A-line, fitted, oversized)
- Finding items with similar proportions (cropped vs. full-length)
- Ignoring color differences when the shape is similar

This is valuable for users who might want "a dress shaped like this one, but in a different color."

=== Key Characteristics

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (center, left),
    [*Property*], [*DINOv2 (ViT-S/14)*],
    [Architecture], [Vision Transformer (Small)],
    [Patch size], [14 × 14 pixels],
    [Input size], [518 × 518 pixels (or 224 × 224)],
    [Output embedding], [384 dimensions],
    [Training], [Self-supervised on 142M images],
    [Strengths], [Shape matching, structural understanding],
  ),
  caption: [DINOv2 model specifications used in this project],
)

=== Trade-offs

Vision Transformers like DINOv2 have different trade-offs compared to CNNs:

*Advantages:*
- Better at understanding global structure
- Can capture long-range relationships in images
- Learned without human labels, so potentially more general

*Disadvantages:*
- Slower than CNNs (more computation required)
- Require larger input images for best results
- May be overkill for simple color/pattern matching

For this project, DINOv2 provides a good complement to Fashion-CLIP: while Fashion-CLIP understands fashion concepts, DINOv2 focuses on visual structure.


Both CNNs and Vision Transformers learn from images alone, mapping visual features to fixed category labels. However, fashion search often involves natural language queries like "red floral summer dress" or "casual weekend outfit." This requires models that can understand *both* images and text in a unified space. The next section introduces CLIP and Fashion-CLIP, which bridge this gap through multimodal learning.
