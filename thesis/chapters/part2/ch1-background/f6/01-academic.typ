=== Academic Research

The *DeepFashion* dataset, introduced by Liu et al., established the foundational benchmark for fashion recognition and retrieval with over 800,000 images annotated with attributes, landmarks, and in-shop-to-consumer photo pairs @liu2016deepfashion. This dataset led to much of the later work in fashion AI.

*FashionIQ* extended retrieval to the conversational setting, where users modify queries through natural language feedback ("like this dress but shorter") @wu2019fashioniq. This is an interesting approach, but building an interactive dialogue system requires infrastructure beyond the scope of this project, which focuses on single-turn visual and text queries.

The *Fashion-CLIP* work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images improves retrieval over the general model @chia2022fashionclip. This thesis follows that approach, using pre-trained models without custom training, and extends the comparison to additional architectures (ResNet, EfficientNet, DINOv2) for systematic comparison. The benchmark in Chapter 3 (§3.6) measured a 1.46% relative mAP advantage of Fashion-CLIP over generic CLIP ViT-B/16 under category-only relevance.

More recently, unified multimodal embedding models such as ImageBind @girdhar2023imagebind and BLIP-2 @li2023blip2 have pushed beyond the image-text pairing of CLIP, binding several modalities into shared latent spaces. These advances underpin the cross-modal text-plus-image retrieval direction discussed in this thesis's future work (Part 3), which would reuse the CLIP-family shared latent space for hybrid queries; they were outside the scope of the single-query benchmark presented here.
