=== Feature Classification

Platform features are categorized into Core Research contributions and Supporting Infrastructure to define thesis scope. Core contributions are detailed in Sections 2.3 and 2.4; supporting modules provide a realistic e-commerce environment for evaluation (Section 3.2).

#figure(
  table(
    columns: (1.2fr, 1.4fr, 4fr),
    stroke: 0.5pt,
    align: (left + horizon, center + horizon, left + horizon),
    inset: 7pt,

    table.header(
      [*Feature Area*], 
      [*Classification*], 
      [*Rationale*]
    ),

    [Visual Search (CBIR)], 
    [Core Research], 
    [
      *Primary Contribution:* Integrated multi-model CBIR system @manning2008introduction with a pluggable architecture for image-based product discovery across CNNs @he2016deep, ViTs @dosovitskiy2020vit, and CLIP-based architectures @radford2021learning @chia2022fashionclip.
    ],

    [ML Embedding Pipeline], 
    [Core Research], 
    [
      *Operational Backbone:* Automated pipeline processing product images, generating vector embeddings via a PyTorch sidecar @paszke2019pytorch, and managing pgvector HNSW indexing @malkov2018efficient @pgvector2023.
    ],

    [Model Benchmark System], 
    [Core Research], 
    [
      *Secondary Contribution:* Systematic benchmarking of retrieval accuracy and latency across 11 embedding models, providing model selection guidelines for deployment.
    ],

    [Product Catalog], 
    [Supporting], 
    [
      *Evaluation Domain:* Structured fashion product data serving as the vector search target.
    ],

    [Order System], 
    [Supporting], 
    [
      *Conversion Tracking:* Shopping workflows capturing cart additions and checkouts to validate visual search utility.
    ],

    [Inventory], 
    [Supporting], 
    [
      *Availability Constraints:* Filters search queries by real-time stock levels.
    ],

    [Authentication], 
    [Supporting], 
    [
      *Security Boundary:* Protects administrative surfaces and user session state.
    ],
  ),
  kind: table,
  caption: [Classification of system feature areas into Core Research contributions and Supporting Infrastructure components.],
) <tbl-feature-classification>