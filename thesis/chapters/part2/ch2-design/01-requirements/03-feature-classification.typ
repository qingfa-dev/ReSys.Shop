=== Feature Classification

Platform features are categorized into *Core Research* contributions and *Supporting Infrastructure* components to define the precise scope of this thesis. The core research contributions are detailed in Sections 2.3 and 2.4, while supporting infrastructure modules are included to provide a realistic e-commerce environment for evaluation, as described in Section 3.2.

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
      *Primary Contribution:* An integrated multi-model CBIR system @manning2008introduction featuring a pluggable architecture. Enables image-based product discovery across multiple embedding model families, including CNNs @he2016deep, ViTs @dosovitskiy2020vit, and CLIP-based architectures @radford2021learning @chia2022fashionclip.
    ],

    [ML Embedding Pipeline], 
    [Core Research], 
    [
      *Operational Backbone:* An automated ingestion pipeline that processes product images, generates vector embeddings via a PyTorch sidecar @paszke2019pytorch, and manages storage and HNSW indexing @malkov2018efficient using pgvector @pgvector2023.
    ],

    [Model Benchmark System], 
    [Core Research], 
    [
      *Secondary Contribution:* A systematic benchmarking methodology evaluating retrieval accuracy and execution latency across 11 embedding models, establishing model selection guidelines for production deployments.
    ],

    [Product Catalog], 
    [Supporting], 
    [
      *Evaluation Domain:* Maintains structured fashion product data (variants, image sets, taxonomies, and attributes) that serves as the vector search target.
    ],

    [Order System], 
    [Supporting], 
    [
      *Conversion Tracking:* Captures end-to-end shopping workflows (cart additions, checkouts) to validate visual search utility using real-world interaction metrics.
    ],

    [Inventory], 
    [Supporting], 
    [
      *Availability Constraints:* Filters search queries by real-time stock levels, ensuring retrieved items reflect purchasable inventory.
    ],

    [Authentication], 
    [Supporting], 
    [
      *Security Boundary:* Protects administrative surfaces and user session state, establishing a production-representative security baseline.
    ],
  ),
  kind: table,
  caption: [Classification of system feature areas into Core Research contributions and Supporting Infrastructure components.],
) <tbl-feature-classification>