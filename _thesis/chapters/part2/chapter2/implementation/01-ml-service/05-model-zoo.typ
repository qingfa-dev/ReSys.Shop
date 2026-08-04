==== Model Zoo

The service implements a *Strategy Pattern* via a `ModelManager`, supporting a diverse ensemble of models to capture different visual aspects:
1. *EfficientNet-B0 (1280-dim):* Baseline CNN model for capturing structural silhouettes and low-level features.
2. *ConvNeXt-Tiny (768-dim):* Modern hierarchical CNN that bridges the gap between Transformers and CNNs.
3. *CLIP (ViT-B/16) (512-dim):* General-purpose semantic search model trained on 400M image-text pairs.
4. *Fashion-CLIP (512-dim):* Domain-adapted CLIP fine-tuned on fashion imagery for understanding specific attributes (e.g., "A-line skirt").
5. *DINOv2 (ViT-S/14) (384-dim):* Self-supervised Vision Transformer that excels at geometric matching and "Visual Similarity" without semantic bias.

#figure(
  placement: none,
  image("../../../../../images/diagrams/03-data-architecture/data-03-ml-service-structure.png", width: 85%),
  caption: [Model Ensemble Diagram. Visual representation of the Model Zoo strategy, showing how different architectures (CNN, ViT, Hybrid) contribute distinctive feature vectors.],
)
