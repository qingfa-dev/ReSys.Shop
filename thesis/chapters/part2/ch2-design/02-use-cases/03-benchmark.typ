=== Use Case 3: Model Benchmark Evaluation

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Researcher / System],
    [*Precondition*], [
      The benchmark dataset is available on disk, consisting of query images and catalog images organised into human-labelled similarity groups. The Python ML sidecar is running. All candidate embedding model weights are downloaded and accessible.
    ],
    [*Main Flow*], [
      1. Researcher selects a model from the candidate set (e.g., Fashion-CLIP, ResNet-50, DINOv2-S) and configures the ML sidecar via environment variable. \
      2. System generates embedding vectors for all query images and all catalog images using the selected model. \
      3. For each query image, the system executes a top-K (K = 20) similarity search against the catalog embeddings. \
      4. System computes retrieval metrics: Mean Average Precision (mAP), Precision at K, and Recall at K, using the human-labelled groups as ground truth. \
      5. System records operational metrics: average inference time per image, throughput (images per second), disk storage for the embedding index, and RAM consumption. \
      6. Steps 1 to 5 are repeated for each of the 11 candidate models. \
      7. System aggregates all results into comparison tables, ranking models by retrieval accuracy and operational efficiency.
    ],
    [*Postcondition*], [
      A complete benchmark report is produced containing accuracy metrics (mAP, P\@20, R\@20) and efficiency metrics (latency, throughput, storage, RAM) for every evaluated model. The report identifies the optimal model for each deployment scenario (GPU production, CPU-only, maximum accuracy, resource-constrained).
    ],
  ),
  caption: [UC-3: Model Benchmark Evaluation, the research methodology use case.],
) <tbl-uc-benchmark>
