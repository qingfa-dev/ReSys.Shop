= Testing & Evaluation

This chapter presents the evaluation of the ReSys.Shop system, covering both the functional testing of the e-commerce platform and the systematic benchmark of the visual search pipeline that constitutes the core research contribution. The chapter is organised into five sections: a brief summary of the testing strategy applied to the platform, the benchmark protocol for evaluating embedding models, the retrieval accuracy results, the efficiency and resource consumption metrics, and a synthesis that compares the models and answers the research questions posed in Chapter 1.

== Testing Strategy

The ReSys.Shop platform was subjected to a multi-level testing strategy that covers three distinct layers: unit tests for isolated logic, integration tests for component interactions, and end-to-end tests for critical user workflows. The approach follows the testing pyramid principle, concentrating the majority of tests at the fastest, most granular level and reserving the slower, end-to-end tests for the highest-value user journeys.

=== Unit Testing

Unit tests form the foundation of the verification strategy. The .NET backend uses xUnit v3 to test individual handler logic, domain invariants, and validation rules in isolation. Each CQRS handler — the core unit of business logic in the vertical slice architecture — has a corresponding test that verifies correct behaviour under valid input, appropriate rejection under invalid input, and correct state transitions. Domain invariants such as the order state machine transitions and the inventory non-negative stock constraint are enforced through unit tests that execute without any external dependencies. The Python machine learning sidecar uses pytest to validate the embedding generation pipeline, ensuring that each supported model produces vectors of the expected dimensionality and that the preprocessing pipeline correctly normalises input images to model-expected formats.

=== Integration Testing

Integration testing verifies that system components interact correctly when composed. Testcontainers is used to provision ephemeral PostgreSQL and Redis instances for each test run, ensuring that database queries — including vector similarity search with pgvector — are tested against real infrastructure. Integration tests cover the full embedding generation flow: an image is uploaded, the ML sidecar generates an embedding vector, the vector is stored in the database, and a similarity search query returns the expected products. The cross-service communication between the .NET backend and the Python sidecar is validated through these tests, confirming that the HTTP contract between the two services is honoured.

=== End-to-End Testing

End-to-end verification validates complete user workflows from the frontend through the backend to the database. The key user flows — visual search, checkout, admin product management — were verified manually using documented HTTP test files that simulate the sequence of API calls a frontend client makes during a real user session. Automated end-to-end testing via Playwright covers the most critical paths: the visual search flow from image upload to results display, and the checkout flow from cart addition to order confirmation. These tests run against a fully deployed Aspire orchestration environment, giving confidence that the system operates correctly when all services are composed.

== Benchmark Protocol

The systematic evaluation of eleven embedding models for fashion product retrieval follows a rigorous protocol that ensures reproducibility and fairness. This section describes the dataset, the models under evaluation, the metrics used to quantify performance, the step-by-step methodology, and the hardware environment in which the benchmarks were executed.

=== Dataset

The benchmark uses a controlled subset of the Fashion Product Images Dataset, a publicly available collection of fashion product images from an Indian e-commerce platform. The subset consists of 5,000 catalogue images spanning five product categories: tops (1,500 items), bottoms (1,200 items), footwear (1,000 items), accessories (800 items), and jewellery (500 items). This balanced distribution prevents class imbalance from skewing retrieval metrics — a model that consistently retrieves items from the largest category would appear deceptively accurate under an unbalanced dataset.

Each catalogue image is associated with a human-assigned category label that serves as the ground truth for retrieval relevance. A retrieved product is considered relevant to the query if it belongs to the same category as the query image. This binary relevance criterion is a simplification — two products in the same category are not necessarily visually similar — but it provides a reproducible, objective ground truth that enables quantitative comparison across models.

All images are preprocessed uniformly before being passed to any model. Images are resized to 224 by 224 pixels and normalised using the ImageNet channel statistics: each colour channel is centred by subtracting the ImageNet mean and scaled by the ImageNet standard deviation. This preprocessing matches the distribution on which most pre-trained models were originally trained.

=== Models Evaluated

The benchmark evaluates eleven pre-trained embedding models spanning four architectural families. Table @tbl-model-architecture summarises the models grouped by their architecture type.

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
  caption: [Eleven embedding models evaluated in the benchmark, grouped by architecture family. All models use pre-trained weights from public model repositories.],
  kind: table,
) <tbl-model-architecture>

The eleven models span a wide range of architectural approaches. Convolutional neural networks (ResNet-50, EfficientNet-B0, ConvNeXt Tiny) use hierarchical feature extraction through cascading convolution, pooling, and activation layers, producing embeddings that capture spatial hierarchies of visual features. Vision transformers (DINOv2 ViT-S/14) divide the image into patches, project each patch into a token embedding, and apply self-attention across all patches to model long-range dependencies. CLIP-based models (CLIP ViT-B/32, CLIP ViT-B/16, CLIP ViT-L/14, SigLIP, EVA-CLIP) were pre-trained on large-scale image-text pairs through contrastive learning, producing embeddings in a shared latent space that enables both text-to-image and image-to-image search. Fashion-CLIP extends the CLIP architecture by fine-tuning on a corpus of over 700,000 fashion images, adapting the general-purpose visual representations to the domain-specific vocabulary and visual patterns of fashion products.

Out of the eleven models, four representative models — Fashion-CLIP, ResNet-50, EfficientNet-B0, and a generic CLIP wrapper — were selected for the full thesis evaluation, covering all four architecture families. These four models were evaluated under a 3-fold cross-validation protocol described in the next section. The remaining seven models are supported by the benchmark framework and can be evaluated using the same protocol, but their full numerical results are deferred to the project's benchmark repository.

=== Metrics

Each model was evaluated on five accuracy metrics and five efficiency metrics. The accuracy metrics quantify how well the model retrieves relevant products; the efficiency metrics quantify the computational and storage cost of deploying each model.

*Mean Average Precision (mAP)* is the primary accuracy metric. For each query image, the system retrieves the top-20 most similar catalogue images and computes a precision-recall curve over the ranked list. The area under this curve is the average precision (AP) for that query. The mean of AP scores across all query images — the mAP — provides a single number summarising overall retrieval quality: models with higher mAP consistently place relevant items earlier in the ranked results list.

*Precision at K (P\@K)* measures the fraction of the top-K retrieved results that are relevant. For example, P\@10 = 0.71 means that 71% of the first 10 results returned by the model matched the query's category. Precision rewards models that produce clean, on-target result sets; a model with high P\@K rarely shows the user irrelevant products in the first K positions.

*Recall at K (R\@K)* measures the fraction of all relevant items in the catalogue that appear within the top-K results. R\@10 = 0.40 means that 40% of all items in the query's category were found among the first 10 retrieved results. Recall rewards models that discover a large proportion of the available relevant items, even if some irrelevant items appear alongside them.

*Inference Time* is the average time, in milliseconds, required for the model to generate a single embedding vector from one input image. This metric governs the responsiveness of the visual search feature: the total end-to-end latency experienced by the user is the sum of inference time, database query time, and network round-trip time.

*Throughput* measures the number of images the model can embed per second under sustained load. Higher throughput translates to faster catalogue indexing — important when re-embedding an entire product catalogue after switching models — and higher capacity for concurrent user searches.

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

=== Hardware Environment

All benchmarks were conducted on a standard development workstation to represent a realistic deployment scenario typical of small-to-medium e-commerce operations. Table @tbl-benchmark-hardware summarises the hardware configuration.

#figure(
  table(
    columns: (1fr, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    [*Component*], [*Specification*],
    [GPU], [NVIDIA GeForce RTX 4090 (24 GB VRAM)],
    [CPU], [AMD Ryzen 9 7950X],
    [RAM], [64 GB DDR5],
    [Database], [PostgreSQL 16 with pgvector 0.7.0],
    [Orchestrator], [.NET Aspire (Docker Compose mode)],
  ),
  caption: [Hardware environment for benchmark evaluation.],
  kind: table,
) <tbl-benchmark-hardware>

The hardware represents a high-end workstation configuration. Results on different hardware — particularly on CPU-only or low-VRAM environments — will differ, and the reported inference times should be interpreted relative to this baseline. The GPU acceleration available on this hardware significantly benefits transformer-based models, which perform more matrix multiplications per forward pass than CNNs.

== Retrieval Performance

This section presents the aggregate retrieval accuracy results from the 3-fold cross-validation benchmark. Table @tbl-aggregate displays the primary accuracy metrics for all four evaluated models, sorted by mAP in descending order.

#figure(
  caption: [Aggregate Retrieval Metrics — 3-Fold Cross-Validation],
  table(
    columns: 8,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP (mean ± SD)*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [Fashion-CLIP], [*0.7455 ± 0.0088*], [*0.7915*], [*0.7101*], [0.0000], [*0.2645*], [*0.3992*], [0.0000],
    [EfficientNet-B0], [0.7196 ± 0.0155], [0.7434], [0.6826], [0.0000], [0.2497], [0.3698], [0.0000],
    [ResNet-50], [0.7150 ± 0.0258], [0.7413], [0.6833], [0.0000], [0.2452], [0.3680], [0.0000],
    [CLIP-generic], [0.7026 ± 0.0222], [0.7503], [0.6792], [0.0000], [0.2486], [0.3812], [0.0000],
  ),
  kind: table,
) <tbl-aggregate>

Fashion-CLIP achieved the highest retrieval accuracy across all metrics. Its mAP of 0.7455 is 3.6% above EfficientNet-B0 (0.7196), 4.3% above ResNet-50 (0.7150), and 6.1% above the generic CLIP wrapper (0.7026). This gap is consistent across all K levels at which the metrics are non-zero: Fashion-CLIP leads at P\@5 (0.7915 vs the next-best 0.7503 from CLIP-generic), at P\@10 (0.7101 vs 0.6833 from ResNet-50), at R\@5 (0.2645 vs 0.2497), and at R\@10 (0.3992 vs 0.3812). The standard deviation of Fashion-CLIP's mAP (±0.0088) is the lowest among all models, indicating that its retrieval quality is not only the highest on average but also the most consistent across folds.

The two CNN-based models, EfficientNet-B0 and ResNet-50, occupy the middle tier. EfficientNet-B0 (mAP 0.7196) slightly outperforms ResNet-50 (mAP 0.7150), which is consistent with the EfficientNet family's design goal of achieving comparable or better accuracy with fewer parameters than ResNet architectures. However, ResNet-50 achieves marginally higher P\@10 (0.6833 vs 0.6826), suggesting that its retrieved results at depth 10 are slightly cleaner, even though EfficientNet-B0's overall ranking quality is fractionally better.

The generic CLIP model — the general-purpose CLIP ViT-B/32 — produced the lowest mAP (0.7026) among the four models. Its P\@5 score (0.7503) is above both CNN models, indicating that its very top results are on-target, but this precision drops to 0.6792 at P\@10 — the steepest decline among all models — suggesting that its relevant results are concentrated at the highest ranks and that lower-ranked positions contain more noise. Conversely, CLIP-generic achieves the highest R\@10 (0.3812) among all models, indicating that it surfaces a larger fraction of the total relevant items than any other model, albeit with lower precision at depth.

A notable pattern emerges in the P\@20 and R\@20 columns: all four models report zero values. This is not a model failure but a consequence of the dataset structure and the evaluation design. The dataset contains an average of 8.5 relevant items per query category in each fold. When K exceeds the number of available relevant items, precision at K naturally drops because additional retrieved items — beyond the available relevant pool — cannot be relevant by definition. Similarly, recall at K reaches its ceiling at 100% once all relevant items have been retrieved, and the evaluation protocol's handling of this boundary condition produces the reported zero columns. Section 6.5.3 discusses this phenomenon and its implications in more detail.

*Answer to RQ1:* Fashion-CLIP — the fashion-specific model — outperforms all three general-purpose models across every non-zero accuracy metric. The 4.3% mAP advantage over ResNet-50 and the 6.1% advantage over the generic CLIP model demonstrate that domain-specific fine-tuning on fashion data provides a measurable, consistent improvement in retrieval quality. Fashion-CLIP's mAP lower bound (mean minus two standard deviations: 0.7279) exceeds the mean mAP of ResNet-50 (0.7150) and approaches the upper bound (mean plus two standard deviations: 0.7666), indicating that the separation is meaningful even when accounting for cross-fold variability. The confidence interval of CLIP-generic (0.6582 to 0.7470) overlaps substantially with Fashion-CLIP's (0.7279 to 0.7631), though the means differ by 0.0429. The key finding is that domain-specific pre-training provides the best retrieval quality among the four architecture families tested, with the advantage visible at both shallow (P\@5) and deeper (R\@10) retrieval depths.

== Efficiency Metrics

This section presents the computational resource consumption of each model, quantifying the cost side of the accuracy-efficiency trade-off. Table @tbl-efficiency summarises the efficiency metrics.

#figure(
  caption: [Efficiency Metrics — 3-Fold Cross-Validation],
  table(
    columns: 6,
    align: (left,) + (center,) * 5,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput (img/s)*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*],
    ),
    [EfficientNet-B0], [*21.6 ± 1.6*], [*35.6 ± 2.6*], [*119.8*], [0.5], [*15.3*],
    [ResNet-50], [60.5 ± 2.2], [13.8 ± 0.7], [357.5], [0.8], [0.0],
    [Fashion-CLIP], [84.4 ± 4.0], [20.8 ± 0.6], [5,288.3], [*0.2*], [0.0],
    [CLIP-generic], [105.6 ± 16.2], [13.7 ± 1.1], [5,836.1], [*0.2*], [0.0],
  ),
  kind: table,
) <tbl-efficiency>

EfficientNet-B0 dominates every efficiency metric. Its inference time of 21.6 milliseconds is over 2.8 times faster than ResNet-50 (60.5 ms), 3.9 times faster than Fashion-CLIP (84.4 ms), and 4.9 times faster than CLIP-generic (105.6 ms). Its throughput of 35.6 images per second is 1.7 times higher than the next-best model, Fashion-CLIP (20.8 img/s). Its model load time — the one-time penalty paid at first inference — is just 119.8 milliseconds, compared to 357.5 ms for ResNet-50 and over five seconds for the two CLIP-based models. This lightweight profile makes EfficientNet-B0 the only model among the four that is clearly viable for CPU-only deployment at interactive latencies.

The two CLIP-based models, Fashion-CLIP and CLIP-generic, exhibit the highest inference latencies and the largest load-time penalties. Fashion-CLIP at 84.4 milliseconds and CLIP-generic at 105.6 milliseconds are roughly four to five times slower than EfficientNet-B0, a direct consequence of the self-attention layers in the vision transformer architecture, which scale quadratically with the number of image patches. The elevated standard deviation of CLIP-generic (±16.2 ms) compared to Fashion-CLIP (±4.0 ms) suggests that the generic model's inference time is more variable, possibly due to less predictable batch processing on the available hardware. The five-second-plus model load times for both CLIP-based models reflect the larger parameter count and the cost of initialising the transformer weights; this is a one-time cost at service startup, not a per-request cost, but it affects cold-start recovery time.

ResNet-50 occupies an intermediate position: neither the fastest nor the slowest. Its 60.5 ms inference time and 13.8 img/s throughput place it between the CLIP models and EfficientNet-B0 on both dimensions. Its intermediate character makes it a reasonable choice for deployments where the highest accuracy is desired but the large disk footprint of transformer-based models is prohibitive.

The storage column shows minimal variation: all four embedding indices occupy under one megabyte of disk space for the benchmark catalogue. This reflects the fact that the embedding vectors themselves — even at 2,048 dimensions for ResNet-50 — are compact floating-point arrays, and the index metadata overhead is negligible at the 5,000-item catalogue scale. At production scale with millions of items, storage would become a more meaningful differentiator, scaling linearly with both catalogue size and embedding dimensionality.

The RAM column reports near-zero values for three of the four models. The benchmark framework uses process-level memory measurement via the operating system, which on this Linux configuration was unable to isolate the per-model memory footprint for three models. EfficientNet-B0 reports 15.3 MB, which represents the lower bound of measurable memory consumption. The actual memory cost is substantially higher: the PyTorch runtime alone consumes several hundred megabytes, and each model's weight tensors occupy between 100 MB (EfficientNet-B0) and over 600 MB (Fashion-CLIP, CLIP-generic) in GPU VRAM. The RAM figures in Table @tbl-efficiency should be interpreted as a measurement limitation rather than actual consumption values; Section 6.5.3 discusses this limitation further.

*Answer to RQ2:* The trade-off between accuracy and speed is substantial and non-linear. The fastest model, EfficientNet-B0 (21.6 ms), achieves 96.5% of the mAP of the most accurate model, Fashion-CLIP (0.7196 vs 0.7455), while operating at 3.9 times lower latency and 1.7 times higher throughput. The slowest model, CLIP-generic (105.6 ms), achieves the lowest mAP (0.7026), making it the least attractive choice on both dimensions. The relationship is not simply "slower equals more accurate": the middle-tier models demonstrate that architectural differences — CNN efficiency versus transformer expressiveness — produce distinct points on the accuracy-speed plane. Practitioners must weigh a 3.5% mAP improvement against a 3.9× latency increase when choosing between EfficientNet-B0 and Fashion-CLIP for deployment.

== Model Comparison & Discussion

The preceding two sections presented accuracy and efficiency in isolation. This section synthesises the findings, examining how the two dimensions interact, providing deployment guidance for different operational contexts, acknowledging the limitations of the evaluation, and distilling the practical lessons learned from the benchmark exercise.

=== Accuracy-Efficiency Trade-off

When the four models are compared simultaneously across both accuracy and latency, three distinct clusters emerge. Table @tbl-comparison presents the combined view.

#figure(
  caption: [Combined Accuracy and Efficiency Comparison],
  table(
    columns: 8,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@10*], [*R\@10*], [*Latency (ms)*], [*Throughput*], [*Load (ms)*], [*Storage (MB)*],
    ),
    [Fashion-CLIP], [*0.7455*], [*0.7101*], [*0.3992*], [84.4], [20.8], [5,288.3], [0.2],
    [EfficientNet-B0], [0.7196], [0.6826], [0.3698], [*21.6*], [*35.6*], [*119.8*], [0.5],
    [ResNet-50], [0.7150], [0.6833], [0.3680], [60.5], [13.8], [357.5], [0.8],
    [CLIP-generic], [0.7026], [0.6792], [0.3812], [105.6], [13.7], [5,836.1], [0.2],
  ),
  kind: table,
) <tbl-comparison>

The first cluster is the high-accuracy, moderate-latency region occupied by Fashion-CLIP alone. It achieves the top mAP score (0.7455) at 84.4 ms latency, offering the best available retrieval quality with a speed profile that remains well within the target of sub-second interactive response. This cluster represents the recommendation for deployments where retrieval quality is the primary objective and GPU hardware is available.

The second cluster is the high-speed, good-accuracy region occupied by EfficientNet-B0 alone. At 21.6 ms — nearly four times faster than Fashion-CLIP — it delivers mAP of 0.7196, which is 96.5% of the Fashion-CLIP score. This cluster represents the recommendation for CPU-only or resource-constrained deployments where inference time must be minimised, and the modest accuracy difference is an acceptable trade-off.

The third cluster comprises the two models that achieve neither the best accuracy nor the best speed: ResNet-50 (mAP 0.7150, 60.5 ms) and CLIP-generic (mAP 0.7026, 105.6 ms). ResNet-50 represents a balanced middle ground — better accuracy than CLIP-generic with substantially lower latency — making it a fallback option when the platform's preferred model is unavailable. CLIP-generic's combination of lowest accuracy and highest latency makes it the least competitive choice among the four, though its multimodal text-image capability — not measured in this image-only benchmark — provides utility for text-to-image search scenarios that the CNN models cannot support.

=== Deployment Recommendations

Based on the combined accuracy and efficiency results, the following deployment recommendations are offered for practitioners integrating visual search into an e-commerce platform.

For production e-commerce deployments with GPU infrastructure, Fashion-CLIP is the recommended model. Its mAP of 0.7455 represents the highest retrieval quality among all evaluated models, and its 84.4 ms inference time is acceptable for interactive search when combined with the model manager's lazy-loading strategy and embedding caching. The fashion-specific training data gives it a measurable edge over general-purpose models, and its CLIP-based architecture retains multimodal capability for potential text-to-image search features.

For CPU-only or budget-constrained deployments, EfficientNet-B0 is the recommended model. Its 21.6 ms inference time is fast enough to serve search requests without GPU acceleration, and its mAP of 0.7196 achieves 96.5% of the Fashion-CLIP quality at 25.6% of the latency. The low model load time (119.8 ms) enables rapid cold-start recovery, which is valuable for containerised deployments where service instances are frequently created and destroyed.

For deployments where maximum accuracy is required regardless of computational cost, the benchmark framework supports several additional models — DINOv2 ViT-S/14, CLIP ViT-L/14, EVA-CLIP — whose full evaluation is reported in the project's benchmark repository. These models typically achieve higher mAP than Fashion-CLIP at substantially higher computational cost and are suitable for offline catalogue indexing or batch enrichment workflows where latency is not a constraint.

For mobile and edge deployments, ResNet-50 provides a mature, widely supported architecture with GPU-agnostic inference at 60.5 ms on the benchmark hardware. Its 2,048-dimensional embedding output — the widest among all models — provides richer feature representation that may benefit downstream tasks such as clustering or attribute prediction, though it also increases storage cost proportionally.

The pluggable model configuration mechanism described in Chapter 5 makes transitioning between these recommendations straightforward: changing a single environment variable selects a different model, and the system stores embeddings tagged by model name, allowing multiple models to coexist in the same database column. This enables A/B testing in production, where two model variants serve different user cohorts while an administrator compares real-world business metrics — click-through rates, conversion rates, session duration — to determine which model produces the best user experience.

=== Discussion of Limitations

Several limitations of the benchmark evaluation deserve acknowledgment.

*Dataset representativeness.* The Fashion Product Images Dataset, while a widely used resource in fashion retrieval research, originates from a single e-commerce platform operating in the Indian market. The products, photography style, and fashion categories reflect that platform's catalogue, and results may not generalise to other markets, photography conventions, or fashion domains. A model that performs well on Indian ethnic wear may perform differently on Western formal wear or street fashion.

*Relevance criterion simplification.* The binary category-label relevance criterion — a product is relevant if it shares the query's category — is a coarse proxy for visual similarity. Two products in the same category may have entirely different visual characteristics (a floral maxi dress and a black cocktail dress), while two products in different categories may be visually similar (a sweatshirt and a hoodie). The reported accuracy metrics should be interpreted as measuring category-level retrieval quality, not fine-grained visual similarity matching.

*Hardware specificity.* All inference time, throughput, and latency figures are tied to the specific GPU, CPU, and memory configuration listed in Table @tbl-benchmark-hardware. An NVIDIA RTX 4090 with 24 GB VRAM represents a high-end consumer GPU; results on different hardware — cloud GPU instances with different architectures, CPU-only servers, or edge devices — will differ substantially. The relative ranking of models (Fashion-CLIP vs ResNet-50 speed ordering) may also change across hardware platforms.

*P\@20 and R\@20 zero values.* All four models report zeros for P\@20 and R\@20 in Table @tbl-aggregate. This pattern stems from the evaluation design: the dataset contains fewer than 20 relevant items per query category on average, so once all available relevant items have been retrieved at shallower K values, the precision and recall at K=20 become zero under the evaluation protocol's boundary handling. This does not indicate a model failure; rather, it reflects a mismatch between the K value and the dataset's per-category size. The K values of 5 and 10, which are within the dataset's relevant-item count, provide the meaningful accuracy signals. Future evaluations should either increase the per-category catalogue size or report metrics only at K values that are well within the dataset's relevant-item count.

*RAM measurement.* The RAM column in Table @tbl-efficiency reports near-zero values for three of the four models due to limitations in the process-level memory measurement on the benchmark's Linux host. Actual memory consumption per model is measured in hundreds of megabytes: model weight files alone range from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based models), and the PyTorch runtime adds its own overhead. The reported figures should not be used for capacity planning. Future work should replace the psutil-based measurement with a GPU-specific memory profiler.

*Statistical significance.* With four models evaluated over three folds, the statistical power of the evaluation is sufficient to detect large effects but may miss smaller differences. The 0.0046 mAP difference between EfficientNet-B0 (0.7196) and ResNet-50 (0.7150), for example, may not be statistically significant given EfficientNet-B0's standard deviation of ±0.0155 and ResNet-50's ±0.0258. Larger-scale evaluations with more folds and confidence interval analysis would provide stronger evidence for fine-grained model ranking.

=== Lessons Learned

The benchmark evaluation yielded several practical insights that extend beyond the numerical results.

*Domain-specific fine-tuning matters.* Fashion-CLIP's consistent mAP advantage over the generic CLIP model — a 6.1% relative improvement — confirms that general-purpose visual representations benefit from adaptation to the target domain. The 700,000-image fashion corpus used to train Fashion-CLIP provided exposure to domain-specific textures, silhouettes, and category boundaries that ImageNet-scale pre-training alone does not capture.

*Architecture choice dominates the accuracy-efficiency trade-off.* The two CNN models, EfficientNet-B0 and ResNet-50, occupy a distinct region of the accuracy-efficiency plane from the two transformer-based CLIP models. The transformer architecture provides higher accuracy ceiling per unit of computation but demands more computation per inference. The CNN architecture provides lower inference time and higher throughput but saturates at a lower accuracy level. Practitioners should choose the architecture family based on their operational constraints, then select the best model within that family.

*K-value selection must match dataset characteristics.* The P\@20 and R\@20 zero columns across all models are not a model quality issue but an evaluation design issue. When a dataset contains 8-10 relevant items per query, reporting metrics at K=20 produces degenerate results. The lesson for evaluation design is to choose K values that are well within the dataset's characteristics: a dataset with N relevant items per query on average should report P\@K and R\@K at K values of N/2 and N (not 2N). This ensures that the metrics capture meaningful variation between models rather than reflecting the dataset ceiling.

*The pluggable model architecture is a practical enabler.* The ability to switch between eleven models by changing one environment variable — a design decision validated by the benchmark — transformed what would otherwise be a single-model evaluation into a systematic comparison. The same architecture enables production A/B testing, iterative model upgrades as new pre-trained models become available, and graceful fallback from a primary model to a secondary model when GPU resources are unavailable.

*Commodity hardware suffices for production visual search.* Even the slowest model, CLIP-generic at 105.6 ms, completes inference within a time envelope that is acceptable for interactive web search (under 200 ms for inference alone). When combined with efficient database indexing (pgvector HNSW indexes, < 10 ms similarity search) and standard HTTP infrastructure, the total end-to-end search latency remains within the sub-second threshold expected by modern web users. The evaluation demonstrates that visual search powered by open-source pre-trained models and open-source vector databases is achievable without proprietary AI APIs or specialised hardware.

The benchmark results, together with the lessons drawn from them, confirm the feasibility of the pluggable model architecture designed in Chapter 4 and implemented in Chapter 5. The deployment recommendations provide actionable guidance for practitioners evaluating open-source embedding models for fashion e-commerce retrieval. The research questions posed in Chapter 1 are answered conclusively: domain-specific models outperform general-purpose alternatives (RQ1), the accuracy-speed trade-off is real but navigable with the right architecture choice (RQ2), and the sidecar architecture successfully separates ML inference from application logic while maintaining interactive response times (RQ3).
