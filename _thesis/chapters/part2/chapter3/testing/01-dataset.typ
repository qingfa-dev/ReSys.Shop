== DATASET AND METHODOLOGY <sec:dataset>

=== About the Dataset

To ensure reproducibility, the project utilizes a *Controlled Subset* of the *Fashion Product Images Dataset* @kaggle-fashion-dataset. Unlike traditional static datasets, the test data is managed dynamically via the system's seeding pipeline.

- *Source:* Kaggle Fashion Product Images (Myntra).
- *Management:* The `POST /database/seed` endpoint enforces a strict distribution, ensuring that every test run operates on an identical data profile.
- *Preprocessing:* All images are normalized to $1:1$ aspect ratios and resized to $224 times 224$ (CLIP/EfficientNet) or $518 times 518$ (DINOv2) during the ingestion pipeline.

==== Dataset Breakdown

The seeding script (`tests/research/integration/test_full_workflow.py`) generates a balanced portfolio of *5,000 items* to prevent class imbalance from skewing the accuracy metrics.

#figure(
  table(
    columns: (1fr, 1fr),
    align: center,
    stroke: 0.5pt,
    [*Category*], [*Count*],
    [Tops (Upper body)], [1,500],
    [Bottoms (Lower body)], [1,200],
    [Footwear], [1,000],
    [Accessories], [800],
    [Jewellery/Other], [500],
    [*Total*], [*5,000*],
  ),
  caption: [Distribution of the controlled test dataset enforced by the seeding engine.],
  kind: table,
) <tab-dataset>

=== Methodology: The 6-Phase Validation Suite

To verify the system across both Research (ML) and Engineering (App) dimensions, a comprehensive validation suite implementation is used.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    [*Phase*], [*Objective*], [*Implementation*],
    [1], [Search Accuracy (mAP\@10)], [`test_full_workflow.py`],
    [2], [Inference Latency], [`test_benchmarks.py`],
    [3], [Data Integrity], [`POST /database/verify`],
    [4], [Catalog Integration], [`ReSys.Api.IntegrationTests`],
    [5], [Checkout Flow], [`PlaceOrderTests.cs`],
    [6], [UI Components], [`vitest` (Admin Panel)],
  ),
  caption: [Mapping of Validation Phases to Codebase Artifacts.],
)


