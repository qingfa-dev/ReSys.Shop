=== CLIP and Fashion-CLIP

CLIP (Contrastive Language-Image Pre-training) bridges the gap between images and natural language, enabling search using both visual and textual queries. This section explains how CLIP works and introduces Fashion-CLIP, the domain-specialized variant used for visual search.

==== Contrastive Language-Image Pre-Training

Traditional image models classify images into fixed categories ("cat," "dog," "dress"). CLIP takes a different approach: it learns to match images with text descriptions @radford2021learning.

During training, CLIP processed 400 million image-text pairs from the public web. For example, an image of a floral dress paired with the caption "colourful floral summer dress," or sneakers paired with "white running shoes on grass." From these pairs, the model learned to:

#list(
  [Convert images into vectors (image encoder).],
  [Convert text into vectors (text encoder).],
  [Make matching image-text pairs produce similar vectors.],
)

==== Dual-Tower Architecture

CLIP has two separate towers:

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_clip-vit-b16.png", width: 70%),
  caption: [CLIP ViT-B/16 dual-tower architecture: images and text are converted to vectors in the same embedding space, allowing direct comparison],
) <fig-clip-arch>

- *Image Tower.* Processes the image using a Vision Transformer (ViT-B/16, ViT-B/32, or ViT-L/14).
- *Text Tower.* Processes text using a transformer.

Both towers output vectors of the same size (512 dimensions for ViT-B variants, 768 for ViT-L), so images and text can be directly compared using cosine similarity.

==== Multimodal Embedding Space

CLIP's ability to understand natural language makes it powerful for fashion:

- It understands concepts like "bohemian style" or "minimalist design."
- It can match images to abstract descriptions ("something for a casual Friday").
- It captures semantic meaning beyond just visual appearance.

However, the general CLIP model was trained on diverse internet images, not specifically fashion. It may not distinguish "A-line dress" from "sheath dress" or "Bohemian style" from "vintage aesthetic." This is where Fashion-CLIP comes in.

==== Multimodal Query Capabilities

The dual-tower design enables query modalities unavailable in vision-only models such as DINOv2 or EfficientNet:

- *Text-to-image search.* A user types "red floral summer dress"; the text encoder maps the description into the same embedding space as catalog images.
- *Hybrid queries.* An uploaded photo can be combined with a textual refinement ("like this, but in blue") by encoding both modalities and merging the results.

This flexibility makes CLIP-based models the primary choice for the visual search feature.

==== Fashion-CLIP and Domain-Specific Fine-Tuning

Fashion-CLIP is a version of CLIP further trained on fashion-specific data @chia2022fashionclip. The researchers used over 700,000 fashion product images paired with detailed descriptions covering garment categories, fabric textures, style descriptors, and occasion labels.

This specialization helps Fashion-CLIP understand:

- Fashion-specific vocabulary ("A-line," "empire waist," "distressed denim").
- Style categories ("streetwear," "preppy," "athleisure").
- Occasion suitability ("office wear," "cocktail party," "beach vacation").

Fashion-CLIP inherits the ViT-B/16 architecture, producing 512-dimensional embeddings. The original paper reports a 15-to-20% improvement on fashion retrieval over general CLIP, a result confirmed in the benchmark evaluation presented in Chapter 3.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_fashion-clip.png", width: 80%),
  caption: [Fashion-CLIP dual-tower architecture: image and text towers independently encode their inputs into 512-dimensional embeddings converging in a shared latent space],
) <fig-fashion-clip-arch>

==== Evaluated CLIP Variants

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
  caption: [CLIP variants evaluated],
) <tbl-clip-models>

Having introduced four model families (CNN, ViT, general CLIP, and Fashion-CLIP), the next section presents a comparative analysis to determine which model best balances retrieval quality, inference speed, and feature capabilities for the fashion e-commerce use case.
