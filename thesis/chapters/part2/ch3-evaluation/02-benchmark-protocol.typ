== Benchmark Protocol

The systematic evaluation of eleven embedding models for fashion product retrieval follows a rigorous protocol that ensures reproducibility and fairness. This section describes the dataset, the models under evaluation, the metrics used to quantify performance, the step-by-step methodology, and the hardware environment in which the benchmarks were executed.

=== Dataset

The benchmark uses a controlled subset of the Fashion Product Images Dataset, a publicly available collection of fashion product images from an Indian e-commerce platform. The subset consists of 5,000 catalogue images spanning five master categories: Apparel (2,500 items), Accessories (1,250 items), Footwear (750 items), Personal Care (350 items), and Sporting Goods (150 items). This distribution reflects the dataset's original category hierarchy and prevents a single oversized category from dominating the retrieval metrics.

Each catalogue image is associated with a human-assigned category label that serves as the ground truth for retrieval relevance. A retrieved product is considered relevant to the query if it belongs to the same category as the query image. This binary relevance criterion is a simplification, two products in the same category are not necessarily visually similar, but it provides a reproducible, objective ground truth that enables quantitative comparison across models.

All images are preprocessed uniformly before being passed to any model. Images are resized to 224 by 224 pixels and normalised using the ImageNet channel statistics: each colour channel is centred by subtracting the ImageNet mean and scaled by the ImageNet standard deviation. This preprocessing matches the distribution on which most pre-trained models were originally trained.

=== Models Evaluated

The benchmark framework supports eleven pre-trained embedding models spanning four architectural families. Four representative models were selected for the full thesis evaluation. Table @tbl-model-architecture summarises the models grouped by their architecture type.

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    table.header([*Architecture*], [*Models*]),
    [Convolutional Neural Network],
    [ResNet-50 (2,048-dimensional output), EfficientNet-B0 (1,280-dimensional), ConvNeXt Tiny (768-dimensional)],
    [Vision Transformer],
    [DINOv2 ViT-S/14 (384-dimensional)],
    [CLIP-based],
    [CLIP ViT-B/32, CLIP ViT-B/16, CLIP ViT-L/14 (512-dimensional each), SigLIP (768-dimensional), EVA-CLIP (512-dimensional)],
    [Fashion-specific],
    [Fashion-CLIP (512-dimensional), fine-tuned on over 700,000 fashion images with domain-specific vocabulary],
  ),
  caption: [Embedding models supported by the benchmark framework, grouped by architecture family. Four representative models were evaluated in the thesis. All models use pre-trained weights from public model repositories.],
  kind: table,
) <tbl-model-architecture>

The eleven models span a wide range of architectural approaches. Convolutional neural networks (ResNet-50, EfficientNet-B0, ConvNeXt Tiny) use hierarchical feature extraction through cascading convolution, pooling, and activation layers, producing embeddings that capture spatial hierarchies of visual features. Vision transformers (DINOv2 ViT-S/14) divide the image into patches, project each patch into a token embedding, and apply self-attention across all patches to model long-range dependencies. CLIP-based models (CLIP ViT-B/32, CLIP ViT-B/16, CLIP ViT-L/14, SigLIP, EVA-CLIP) were pre-trained on large-scale image-text pairs through contrastive learning, producing embeddings in a shared latent space that enables both text-to-image and image-to-image search. Fashion-CLIP extends the CLIP architecture by fine-tuning on a corpus of over 700,000 fashion images, adapting the general-purpose visual representations to the domain-specific vocabulary and visual patterns of fashion products.

Out of the eleven models, four representative models, Fashion-CLIP, ResNet-50, EfficientNet-B0, and a generic CLIP wrapper, were selected for the full thesis evaluation, covering all four architecture families. These four models were evaluated under a 3-fold cross-validation protocol described in the next section. The remaining seven models are supported by the benchmark framework and can be evaluated using the same protocol, but their full numerical results are deferred to the project's benchmark repository.

=== Metrics

Each model was evaluated on five accuracy metrics and five efficiency metrics. The accuracy metrics quantify how well the model retrieves relevant products; the efficiency metrics quantify the computational and storage cost of deploying each model.

*Mean Average Precision (mAP)* is the primary accuracy metric. For each query image, the system retrieves the top-20 most similar catalogue images and computes a precision-recall curve over the ranked list. The area under this curve is the average precision (AP) for that query. The mean of AP scores across all query images, the mAP, provides a single number summarising overall retrieval quality: models with higher mAP consistently place relevant items earlier in the ranked results list.

*Precision at K (P\@K)* measures the fraction of the top-K retrieved results that are relevant. For example, P\@10 = 0.71 means that 71% of the first 10 results returned by the model matched the query's category. Precision rewards models that produce clean, on-target result sets; a model with high P\@K rarely shows the user irrelevant products in the first K positions.

*Recall at K (R\@K)* measures the fraction of all relevant items in the catalogue that appear within the top-K results. R\@10 = 0.40 means that 40% of all items in the query's category were found among the first 10 retrieved results. Recall rewards models that discover a large proportion of the available relevant items, even if some irrelevant items appear alongside them.

*Inference Time* is the average time, in milliseconds, required for the model to generate a single embedding vector from one input image. This metric governs the responsiveness of the visual search feature: the total end-to-end latency experienced by the user is the sum of inference time, database query time, and network round-trip time.

*Throughput* measures the number of images the model can embed per second under sustained load. Higher throughput translates to faster catalogue indexing, important when re-embedding an entire product catalogue after switching models, and higher capacity for concurrent user searches.

*Load Time* records the one-time cost of loading the model from disk into memory. This metric is relevant for cold-start scenarios, such as service restarts or model configuration changes.

*Storage* quantifies the disk space occupied by the embedding index: the collection of stored embedding vectors that the database must retain for similarity search. Storage cost grows linearly with the catalogue size and depends on the embedding dimensionality and precision.

*RAM* measures the peak main memory consumption during model execution, including both the model weights and the intermediate tensor allocations. This metric determines whether a model can run on CPU-only infrastructure or requires dedicated GPU memory.

=== Methodology

Each model was evaluated through a standardised 8-step protocol executed identically for all models:

1. Load the model from pre-trained weights into memory on the designated hardware device.
2. Generate an embedding vector for every query image in the dataset.
3. Generate an embedding vector for every catalogue image in the dataset.
4. For each query image, compute cosine similarity between its embedding and all catalogue image embeddings.
5. Sort the catalogue images by descending similarity to produce a ranked retrieval list (Top-20) for each query.
6. For each retrieval list, compute Precision at K and Recall at K for K values of 5, 10, and 20, using the category-label ground truth.
7. Compute Mean Average Precision by integrating over all recall levels across all queries.
8. Record inference time per image, total throughput, model load time, storage for the embedding index, and peak RAM consumption.

Steps 1-8 were repeated for each of the four evaluated models. The evaluation used 3-fold cross-validation: the dataset was partitioned into three stratified folds, each preserving the original category distribution. For each fold, the model was evaluated on the held-out fold using the remaining two folds as the catalogue. The reported metrics are the mean and standard deviation across the three folds, providing both point estimates and measures of variability.

The retrieval accuracy metrics (mAP, P\@K, R\@K) are computed via exact cosine search over all gallery embeddings, eliminating any index approximation effects and isolating model quality from index performance. For the pgvector production metrics, the benchmark uses the IVFFlat (Inverted File with Flat compression) approximate index with 100 lists. IVFFlat was chosen over HNSW at this scale for three reasons. First, at 5,000 catalogue vectors, IVFFlat achieves sub-10-millisecond query latency and 65--72 percent recall\@10 (see Appendix A.4), which is adequate for a controlled model comparison where the focus is on model ranking rather than index optimisation. Second, IVFFlat builds nearly instantaneously (under one second) versus the minutes required for HNSW graph construction, enabling rapid iteration across the 4-model, 3-fold evaluation matrix. Third, IVFFlat exposes fewer hyperparameters (lists and probes) than HNSW (M, ef_construction, ef_search), reducing confounding variables when the objective is to compare embedding models rather than index configurations. The production architecture designates HNSW for deployments at larger catalogue scales where its superior recall-speed trade-off becomes decisive.

=== Hardware Environment

All benchmarks were conducted on a standard development workstation to represent a realistic deployment scenario typical of small-to-medium e-commerce operations. Table @tbl-benchmark-hardware summarises the hardware configuration.

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    [*Component*], [*Specification*],
    [CPU], [Intel (11th Gen Core i7-1165G7, 4 cores / 8 threads, 2.80 GHz)],
    [RAM], [16 GB DDR4],
    [Database], [PostgreSQL 16 with pgvector 0.7.0],
    [Orchestrator], [.NET Aspire (Docker Compose mode)],
  ),
  caption: [Hardware environment for benchmark evaluation.],
  kind: table,
) <tbl-benchmark-hardware>

The hardware represents a standard development laptop configuration. All benchmarks were executed on CPU without GPU acceleration, as the available GPU (NVIDIA GeForce MX330, compute capability 6.1) does not meet the minimum compute capability required by the evaluated deep learning frameworks (sm_75). Results on different hardware, particularly systems with a compatible GPU, will differ, and the reported inference times should be interpreted relative to this CPU-only baseline.
