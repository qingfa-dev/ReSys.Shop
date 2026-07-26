=== CLIP and Fashion-CLIP

CNNs and ViTs produce embeddings with no connection to human language. *CLIP* (Contrastive Language-Image Pre-training) bridges this gap through a dual-tower architecture: an image encoder and a text encoder independently map their inputs into a shared embedding space, trained jointly on 400 million (image, caption) pairs @radford2021learning. A contrastive objective maximises cosine similarity for matching pairs while minimising it for non-matching pairs. The result is a model that can both see and read: a text description and an image of the same concept produce nearby embeddings.

*Fashion-CLIP* fine-tunes CLIP on over 700,000 fashion images paired with domain-specific descriptions covering garment categories, fabric textures, style descriptors, and occasion labels @chia2022fashionclip. Both the image and text towers are adjusted to emphasise fashion-relevant attributes while retaining general visual understanding. Fashion-CLIP inherits the ViT-B/16 architecture, producing 512-dimensional embeddings. The original paper reports a 15-to-20% improvement on fashion retrieval over general CLIP, confirmed in the benchmark evaluation presented in Chapter 3.

The dual-tower design enables query modalities unavailable in vision-only models. A text query ("red floral summer dress") needs no reference image: the text encoder maps it directly into the same embedding space as catalog images. Hybrid queries combine an uploaded photo with a textual refinement ("like this, but in blue") by encoding both modalities. This flexibility makes Fashion-CLIP the primary embedding model for visual search in ReSys.Shop.

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
  caption: [CLIP variants evaluated],
) <tbl-clip-models>
