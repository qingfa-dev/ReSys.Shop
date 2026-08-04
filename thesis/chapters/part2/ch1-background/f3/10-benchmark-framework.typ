=== Benchmark Framework

The benchmark framework is a Python 3.12 pipeline for systematic, reproducible evaluation of embedding models on fashion product retrieval @paszke2019pytorch. It is separate from the application codebase and produces the experimental results reported in Chapter 3.

- *Modes.* Three evaluation modes are supported: a one-shot comparison across all configured models, a three-fold cross-validation protocol with stratified category-based splits (the default for thesis results), and a pgvector pipeline mode that measures end-to-end latency including database query and network round-trip time.

- *Models.* 11 pre-trained architectures are implemented: CNN-based (ResNet-50, ResNet-101, ResNet-152, EfficientNet-B0, EfficientNet-B4), vision transformers (DINOv2 ViT-S/14, DINOv2 ViT-B/14), and CLIP variants (CLIP ViT-B/32, CLIP ViT-B/16, CLIP ViT-L/14, Fashion-CLIP). Each model has a thin adapter implementing a common `generate_embeddings` interface.

- *Caching.* Embeddings are cached to disk per model, per fold, and per split (query vs. catalog), avoiding recomputation when re-running evaluation on the same configuration.

- *Metrics.* Retrieval accuracy is measured through mAP, Precision at K, Recall at K, and nDCG. Operational efficiency is measured through inference latency (mean and SD per image), throughput (images per second), model load time, on-disk storage, and RAM usage.

- *Outputs.* Results are exported in JSON, CSV, Markdown, and Typst table formats. The Typst output files (`thesis_aggregate.typ`, `thesis_efficiency.typ`) embed directly into the thesis without manual transcription.

- *Multi-label pipeline.* A separate enriched-dataset mode supports evaluation across three label schemes of increasing granularity (category only, category and colour, and category, colour and pattern), enabling analysis of how embedding quality varies with annotation detail.
