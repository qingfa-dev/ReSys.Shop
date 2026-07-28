=== Feature Classification

Not all platform features carry equal research weight. The following classification distinguishes *core research* contributions from *supporting infrastructure*, clarifying the thesis scope. The three research contributions are examined in Sections 2.3 and 2.4; supporting modules are described only to the extent they enable realistic evaluation, as detailed in Section 3.2.

#figure(
  table(
    columns: (1fr, 2fr, 4fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left),
    inset: 8pt,

    table.header([*Feature Area*], [*Classification*], [*Rationale*]),

    [Visual Search (CBIR)], [Core Research], [
      Primary contribution: integrated multi-model *CBIR* @manning2008introduction with pluggable architecture enabling image-based product search across multiple embedding models (*CNN* @he2016deep, *ViT* @dosovitskiy2020vit, *CLIP*-based @radford2021learning @chia2022fashionclip) within a production-style e-commerce platform.
    ],

    [ML Embedding Pipeline], [Core Research], [
      Critical infrastructure: automated ingestion of product images, generation of *vector embeddings* via the Python sidecar (*PyTorch* @paszke2019pytorch), storage in *pgvector* @pgvector2023, and *HNSW* indexing @malkov2018efficient. The operational backbone of visual search.
    ],

    [Model Benchmark System], [Core Research], [
      Secondary contribution: systematic protocol for comparing retrieval accuracy and operational efficiency across 11 embedding models, providing practical guidance for model selection in resource-constrained deployments.
    ],

    [Product Catalog], [Supporting Infrastructure], [
      Required context: provides the structured dataset of fashion products with variants, images, taxonomies, and metadata serving as the search target for CBIR evaluation.
    ],

    [Order System], [Supporting Infrastructure], [
      Metric validation: provides conversion events (add-to-cart, checkout completion) as proxy indicators of search success within a realistic shopping workflow.
    ],

    [Inventory], [Supporting Infrastructure], [
      Realism constraint: ensures search results reflect actual product availability, preventing visually similar but out-of-stock items from appearing.
    ],

    [Authentication], [Supporting Infrastructure], [
      Security baseline: protects administrative functions and user data, enabling the platform to operate in a representative security posture.
    ],
  ),
    kind: table,
  caption: [
    Classification of feature areas into Core Research and Supporting Infrastructure. Core Research features represent the thesis's original contributions; Supporting Infrastructure features provide the realistic e-commerce context for evaluation.
  ],
) <tbl-feature-classification>
