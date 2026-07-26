=== CLIP and Fashion-CLIP

==== CLIP: Dual-Tower Contrastive Pre-Training

CNNs and ViTs operate purely in the visual domain, producing embeddings with no connection to human language. CLIP (Contrastive Language-Image Pre-training) bridges this gap through a dual-tower architecture: an image encoder (ViT or ResNet) and a text encoder (transformer) independently map their inputs into a shared embedding space, trained jointly on 400 million (image, caption) pairs from the public web @radford2021learning. A contrastive objective maximises cosine similarity for matching pairs while minimising it for non-matching pairs within each training batch. The result is a model that can both see and read: a text description and an image of the same concept produce nearby embeddings.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_clip-vit-b16.png", width: 90%),
  caption: [CLIP ViT-B/16 as a general-purpose embedding model. Pretrained on 400M image-text pairs it maps images into a 512-dimensional space, enabling nearest-neighbour similarity search for stylistically consistent product recommendations.],
) <fig-clip-vit-arch>

==== Fashion-CLIP: Domain-Specific Fine-Tuning

General CLIP understands broad visual concepts but lacks fashion-specific vocabulary -- it may not distinguish "A-line dress" from "sheath dress" or "Bohemian style" from "vintage aesthetic." *Fashion-CLIP* addresses this by fine-tuning CLIP on over 700,000 fashion images paired with domain-specific text descriptions covering garment categories, fabric textures, style descriptors, and occasion labels @chia2022fashionclip. The fine-tuning adjusts both image and text tower weights to emphasise fashion-relevant attributes while retaining general visual understanding. Fashion-CLIP inherits the ViT-B/16 architecture, producing 512-dimensional embeddings, and the original paper reports a 15-to-20% improvement on fashion retrieval tasks over general CLIP -- a result confirmed in the benchmark evaluation presented in Chapter 3.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/f4-ml-models/P2S2.1.4_fashion-clip.png", width: 70%),
  caption: [Fashion-CLIP dual-tower architecture. The image and text towers independently encode their inputs into 512-dimensional embeddings, which converge in a shared latent space. Cosine similarity between embeddings produces the retrieval score. Fine-tuning on 700K fashion image-text pairs adjusts both towers for domain-specific attributes.],
) <fig-fashion-clip-arch>

==== Multimodal Retrieval

The dual-tower design enables query modalities unavailable in pure vision models such as DINOv2 or EfficientNet. A user searching for "red floral summer dress" needs no reference image: the text encoder maps the description directly into the same embedding space as catalog images, and the nearest neighbours are returned. Hybrid queries combine an uploaded photo with a textual refinement ("like this, but in blue") by encoding both modalities and merging the results. This flexibility makes Fashion-CLIP the primary embedding model for the visual search feature in ReSys.Shop.

#figure(
  table(
    columns: (auto, 1fr, 1fr, auto),
    align: center + horizon,
    table.header([*Model*], [*Architecture*], [*Training*], [*Do\
 main*]),
    [CLIP ViT-B/32], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-B/16], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-L/14], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [Fashion-CLIP], [Dual-tower (ViT + text transformer)], [Contrastive, fine-tuned on 700K fashion images], [Fashion-specific],
  ),
    kind: table,
  caption: [CLIP variants evaluated in this thesis],
) <tbl-clip-models>
