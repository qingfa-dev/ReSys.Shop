=== CLIP and Fashion-CLIP: Bridging Vision and Language

CNNs and ViTs operate purely in the visual domain, mapping images to embedding spaces that have no connection to human language. *CLIP* (Contrastive Language-Image Pre-training) bridges this gap through a dual-tower architecture: one tower encodes images, a parallel text encoder processes natural language descriptions, and both are trained jointly on 400 million (image, caption) pairs from the public web @radford2021learning. A contrastive objective pulls matching image-text pairs together in a shared embedding space while pushing non-matching pairs apart. The result is a model that can both see and read.

*Fashion-CLIP* extends CLIP by fine-tuning it on over 700,000 fashion images paired with domain-specific text descriptions @chia2022fashionclip. The fine-tuning adjusts model weights to emphasise fashion-relevant attributes (garment categories, fabric textures, style descriptors, occasion labels) while retaining general visual understanding. Fashion-CLIP uses the ViT-B/16 architecture inherited from CLIP, producing 512-dimensional embeddings. The original paper reports a 15 to 20% improvement on fashion retrieval over general CLIP, a result confirmed in the benchmark evaluation presented in Chapter 3 of this thesis.

The dual-tower design also enables *multimodal queries* unavailable in pure vision models like DINOv2 or EfficientNet. A user searching for "red floral summer dress" does not need a reference image; the text encoder maps the description directly into the same embedding space as catalog images. A hybrid query combining an uploaded photo with a textual refinement, "like this, but in blue," becomes possible by encoding both modalities and merging the results.

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Model*], [*Architecture*], [*Training*], [*Domain*]),
    [CLIP ViT-B/32], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-B/16], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-L/14], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [Fashion-CLIP], [Dual-tower (ViT + text transformer)], [Contrastive, fine-tuned on 700K fashion images], [Fashion-specific],
  ),
  caption: [CLIP variants evaluated in this thesis],
) <tbl-clip-models>
