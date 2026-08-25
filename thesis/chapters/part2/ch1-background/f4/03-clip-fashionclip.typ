=== CLIP and Fashion-CLIP

CLIP (Contrastive Language-Image Pre-training) connects vision and language, enabling search using both visual and textual queries. Fashion-CLIP, its domain-specialized variant, is the primary model for visual search.

==== Contrastive Language-Image Pre-Training

Traditional image models classify images into fixed categories ("cat," "dog," "dress"). CLIP learns to match images with text descriptions instead @radford2021learning.

During training, CLIP processed 400 million image-text pairs from the public web (e.g., a floral dress with "colourful floral summer dress," sneakers with "white running shoes on grass"). From these pairs, the model learned to:
#list(
  [Convert images into vectors (image encoder).],
  [Convert text into vectors (text encoder).],
  [Make matching image-text pairs produce similar vectors.],
)

==== Dual-Tower Architecture

CLIP has two separate towers:
#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/diagrams/P2S2.1.4_clip-vit-b16.png", width: 75%),
  caption: [CLIP ViT-B/16 dual-tower architecture],
) <fig-clip-arch>

- *Image Tower.* Processes the image using a Vision Transformer (ViT-B/16, ViT-B/32, or ViT-L/14).
- *Text Tower.* Processes text using a transformer.

Both towers output vectors of the same size (512 dimensions for ViT-B variants, 768 for ViT-L), so images and text can be directly compared using cosine similarity.

==== Multimodal Embedding Space

CLIP's natural language understanding enables fashion concepts like "bohemian style" or "minimalist design," matching images to abstract descriptions ("something for a casual Friday"), and capturing semantic meaning beyond visual appearance. However, general CLIP was trained on diverse internet images, not specifically fashion; it may not distinguish "A-line dress" from "sheath dress" or "Bohemian" from "vintage."

==== Multimodal Query Capabilities

The dual-tower design enables query modalities unavailable in vision-only models such as DINOv2 or EfficientNet:

- *Text-to-image search.* A user types "red floral summer dress"; the text encoder maps the description into the same embedding space as catalog images.
- *Hybrid queries.* An uploaded photo combined with textual refinement ("like this, but in blue") by encoding both modalities and merging the results.

This flexibility makes CLIP-based models the primary choice for the visual search feature.

==== Fashion-CLIP and Domain-Specific Fine-Tuning

Fashion-CLIP further trains CLIP on over 700,000 fashion product images paired with detailed descriptions covering garment categories, fabric textures, style descriptors, and occasion labels @chia2022fashionclip. This specialization helps Fashion-CLIP understand:

- Fashion-specific vocabulary ("A-line," "empire waist," "distressed denim").
- Style categories ("streetwear," "preppy," "athleisure").
- Occasion suitability ("office wear," "cocktail party," "beach vacation").

Fashion-CLIP inherits the ViT-B/16 architecture, producing 512-dimensional embeddings. The original paper reports an improvement on fashion retrieval over general CLIP; the benchmark analysis in Chapter 3 (§3.6) measured a 1.46% relative mAP advantage under category-only relevance @chia2022fashionclip.
#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/diagrams/P2S2.1.4_fashion-clip.png", height: 50%),
  caption: [Fashion-CLIP dual-tower architecture with shared 512-dimensional embeddings],
) <fig-fashion-clip-arch>

==== Benchmarked CLIP Variants

#figure(
  table(
    columns: (auto, 1fr, 1fr, auto),
    align: center + horizon,
    table.header([*Model*], [*Architecture*], [*Training*], [*Domain*]),
    [CLIP ViT-B/32], [Dual-tower (ViT + text transformer)], [Contrastive (400M pairs)], [General],
    [CLIP ViT-B/16], [Dual-tower (ViT + text transformer)], [Contrastive (400M pairs)], [General],
    [CLIP ViT-L/14], [Dual-tower (ViT + text transformer)], [Contrastive (400M pairs)], [General],
    [Fashion-CLIP], [Dual-tower (ViT + text transformer)], [Contrastive, fine-tuned on 700K fashion images], [Fashion-specific],
  ),
    kind: table,
  caption: [CLIP variants benchmarked],
) <tbl-clip-models>
