=== Benchmark Framework

The benchmark framework is a Python 3.12 pipeline for systematic evaluation of embedding models @paszke2019pytorch, operating separately from the application codebase. Experimental results are reported in Chapter 3.

- *Modes.* One-shot comparison, three-fold cross-validation with stratified category splits (default), and pgvector pipeline mode measuring end-to-end latency.
- *Models.* 11 architectures: CNN (ResNet-50, ResNet-101, ResNet-152, EfficientNet-B0, EfficientNet-B4), ViT (DINOv2 ViT-S/14, DINOv2 ViT-B/14), CLIP (ViT-B/32, ViT-B/16, ViT-L/14, Fashion-CLIP). Each has a thin adapter implementing `generate_embeddings`.
- *Caching.* Embeddings cached per model, fold, and split to avoid recomputation.
- *Metrics.* Retrieval accuracy: mAP, Precision at K, Recall at K, nDCG. Efficiency: inference latency, throughput, model load time, storage, RAM.
- *Outputs.* JSON, CSV, Markdown, and Typst table formats embed directly into the thesis without manual transcription.
- *Multi-label pipeline.* Enriched-dataset mode evaluates three label schemes (category; category+colour; category+colour+pattern).
