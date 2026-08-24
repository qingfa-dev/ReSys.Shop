== Benchmark Protocol and Experimental Setup

The evaluation uses the Fashion Product Images Dataset: 5,000 catalogue images across five categories (Apparel 2,500, Accessories 1,250, Footwear 750, Personal Care 350, Sporting Goods 150). Each image carries a category label as ground truth for binary relevance. All images are preprocessed to $224 times 224$ pixels with ImageNet normalisation (mean 0.485/0.456/0.406, std 0.229/0.224/0.225).

=== Models Evaluated

Six models spanning four architectural families were evaluated by the benchmark framework (selected from the eleven candidates it supports):

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    table.header([*Architecture*], [*Evaluated Models (dimension)*]),
    [Convolutional Neural Network], [ResNet-50 (2,048-dim), EfficientNet-B0 (1,280-dim)],
    [Vision Transformer], [DINOv2 ViT-S/14 (384-dim): self-supervised ViT],
    [CLIP-based], [CLIP ViT-B/16 (512-dim): generic CLIP wrapper, CLIP ViT-B/32 (512-dim): OpenAI CLIP],
    [Fashion-specific], [Fashion-CLIP (512-dim), fine-tuned on 700,000+ fashion images],
  ),
  caption: [Six evaluated models representing CNN, ViT, CLIP, and domain-tuned architectures.],
  kind: table,
) <tbl-model-architecture>

=== Evaluation Metrics

Three accuracy metric families (mAP, P\@K, R\@K), evaluated at three depths (K\=5, 10, 20), and five efficiency metrics were measured per model.

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    table.header([*Metric*], [*Definition*]),
    [mAP], [Query-averaged Mean Average Precision over the top-20 ranked results (average precision per query, then mean across queries); primary accuracy metric.],
    [P\@K], [Fraction of top-K results sharing the query's category.],
    [R\@K], [Fraction of all relevant catalogue items appearing in top-K results.],
    [Inference Time], [Mean milliseconds per image embedding (preprocessing + forward pass).],
    [Throughput], [Images embedded per second under sustained load.],
    [Load Time], [One-time cost of loading model weights from disk into memory.],
    [Storage], [Disk space of the embedding index for the 5,000-image catalogue.],
    [RAM], [Peak main memory consumption. Reported as approximate ranges derived from each model's parameter count plus PyTorch runtime overhead, because direct psutil measurement proved unreliable on this Linux kernel. Cost scales from ~100 MB (EfficientNet-B0) to ~600 MB (CLIP-based).],
  ),
  kind: table,
  caption: [Evaluation metrics and definitions.],
) <tbl-metric-definitions>

=== Methodology

The protocol used 3-fold stratified cross-validation preserving category distribution. Accuracy metrics (mAP, P\@K, R\@K) used exact cosine search over all gallery embeddings to isolate model quality from index effects. The pgvector production benchmark used IVFFlat with 100 lists (sub-10 ms query latency at 65--72% recall\@10, near-instant build). HNSW is designated for larger catalogue scales.

=== Hardware Environment

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    [*Component*], [*Specification*],
    [CPU], [Intel Core i7-1165G7 (4 cores / 8 threads, 2.80 GHz)],
    [RAM], [16 GB DDR4],
    [Database], [PostgreSQL 17, pgvector 0.7.0],
  ),
  caption: [Hardware environment. All benchmarks executed on CPU without GPU acceleration.],
  kind: table,
) <tbl-benchmark-hardware>
