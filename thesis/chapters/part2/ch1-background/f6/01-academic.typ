=== Academic Research

The *DeepFashion* dataset, introduced by Liu et al., established the foundational benchmark for fashion recognition and retrieval with over 800,000 images annotated with attributes, landmarks, and in-shop-to-consumer photo pairs @liu2016deepfashion. This dataset catalysed much of the subsequent work in fashion AI.

*FashionIQ* extended retrieval to the conversational setting, where users modify queries through natural language feedback ("like this dress but shorter") @wu2019fashioniq. While compelling, the interactive dialogue paradigm requires infrastructure beyond the scope of this project, which focuses on single-turn visual and text queries.

The *Fashion-CLIP* work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images improves retrieval over the general model @chia2022fashionclip. This thesis follows that approach, using pre-trained models without custom training, and extends the evaluation to additional architectures (ResNet, EfficientNet, DINOv2) for systematic comparison. The benchmark in Chapter 3 (§3.5) measured a 2.13% mAP advantage of Fashion-CLIP over generic CLIP under category-only evaluation.
