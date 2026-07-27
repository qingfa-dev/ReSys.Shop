== CLIP AND FASHION-CLIP

CLIP (Contrastive Language-Image Pre-training) represents a breakthrough in connecting images with natural language, enabling search using both visual and textual queries. This section explains how CLIP works and introduces Fashion-CLIP, the domain-specialized version used for visual search in this project.

=== The Idea Behind CLIP

Traditional image models are trained to classify images into fixed categories ("cat," "dog," "dress"). CLIP takes a different approach: it learns to match images with text descriptions @radford2021learning.

During training, CLIP saw 400 million image-text pairs from the internet. For example:
- An image of a floral dress paired with the caption "colorful floral summer dress"
- An image of sneakers paired with "white running shoes on grass"

The model learned to:
1. Convert images into vectors (image encoder)
2. Convert text into vectors (text encoder)
3. Make matching image-text pairs have similar vectors

=== The Dual-Tower Architecture

CLIP has two separate "towers" that work together:

#figure(
  image("/images/diagrams/01-ml-models/ml-03-fashion-clip.png", width: 80%),
  caption: [CLIP's dual-tower architecture: images and text are converted to vectors in the same space, allowing direct comparison],
) <fig-fclip-arch>

*Image Tower:* Processes the image using a Vision Transformer (ViT-B/16)
*Text Tower:* Processes text using a Transformer (similar to GPT)

Both towers output vectors of the same size (512 dimensions), so images and text can be directly compared using cosine similarity.

=== Multimodal Embedding Space: Bridging Visual and Textual Queries

CLIP's ability to understand natural language descriptions makes it powerful for fashion:

- It understands concepts like "bohemian style" or "minimalist design"
- It can match images to abstract descriptions ("something for a casual Friday")
- It captures semantic meaning beyond just visual appearance

However, the general CLIP model was trained on diverse internet images, not specifically fashion. This is where Fashion-CLIP comes in.

=== Fashion-CLIP: Domain Specialization

Fashion-CLIP is a version of CLIP that was further trained on fashion-specific data @chia2022fashionclip. The researchers used:

- 700,000+ fashion product images
- Detailed fashion descriptions with domain terminology
- Fine-grained fashion concepts (silhouettes, styles, occasions)

This specialization helps Fashion-CLIP understand:

- Fashion-specific vocabulary ("A-line," "empire waist," "distressed denim")
- Style categories ("streetwear," "preppy," "athleisure")
- Occasion suitability ("office wear," "cocktail party," "beach vacation")

=== Comparison of Models

#figure(
  table(
    columns: (auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left, center, center, center),
    [*Aspect*], [*EfficientNet*], [*DINOv2*], [*Fashion-CLIP*],
    [Architecture], [CNN], [ViT], [ViT],
    [Training], [Supervised], [Self-supervised], [Contrastive (image-text)],
    [Embedding size], [1,280], [384], [512],
    [Input size], [224×224], [518×518], [224×224],
    [Strength], [Colors, textures], [Shapes, structure], [Fashion semantics],
    [Speed], [Fast], [Medium], [Medium],
    [Domain-specific], [No], [No], [Yes (fashion)],
  ),
  caption: [Comparison of the three embedding models used in this project],
) <tab-model-comparison>

=== CLIP ViT-B/16 for Recommendations

While Fashion-CLIP is used for precise visual search (high precision matching), the general CLIP model (ViT-B/16 variant) is used for the recommendation feature.

#figure(
  image("/images/diagrams/01-ml-models/ml-04-clip-vit-b16.png", width: 70%),
  caption: [CLIP ViT-B/16 is used for discovery-oriented recommendations],
) <fig-clip-rec-arch>

The model selection decision is based on several factors:
- Fashion-CLIP is too specific for recommendations, as it might only suggest very similar items
- General CLIP provides broader associations, potentially suggesting items that "go together" stylistically

For example, if a user is viewing a formal blazer:
- Fashion-CLIP might suggest other formal blazers
- General CLIP might suggest dress shirts, ties, or formal trousers

This provides a more discovery-oriented experience for users exploring the catalog.

=== Domain Specialization: Fashion-CLIP's Training on 700K+ Fashion Images

Fashion-CLIP's domain specialization makes it particularly suitable for fashion e-commerce applications. The model was fine-tuned on 700,000+ fashion product images with detailed descriptions, enabling it to understand:

- Fashion-specific vocabulary ("A-line," "empire waist," "distressed denim")
- Style categories ("streetwear," "preppy," "athleisure")
- Occasion suitability ("office wear," "cocktail party," "beach vacation")

This specialization, combined with the multimodal capability to search using both images and text, makes Fashion-CLIP a strong candidate for the visual search feature.

The quantitative evaluation comparing Fashion-CLIP against other models (DINOv2, EfficientNet, standard CLIP) is presented in the Model Selection section (@sec:model-selection), where data-driven justification for the final model choice is provided.

Having introduced four candidate models (EfficientNet-B0 (CNN), DINOv2 (self-supervised ViT), standard CLIP (multimodal), and Fashion-CLIP (domain-specialized multimodal)), the next section presents a systematic evaluation to determine which model best balances retrieval quality, inference speed, and feature capabilities for the fashion e-commerce use case.
