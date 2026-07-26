=== Feature Classification

Not all features of ReSys.Shop carry equal research significance. Seven feature areas are classified as either *Core Research* (directly contributing to the thesis's academic objectives) or *Supporting Infrastructure* (providing the realistic e-commerce context in which the research is conducted and evaluated). This distinction clarifies the scope of the thesis's original contribution and explains why certain features exist in the platform but are not discussed in depth in subsequent chapters.

#figure(
  table(
    columns: (1fr, 2fr, 4fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left),
    inset: 8pt,

    table.header([*Feature Area*], [*Classification*], [*Rationale*]),

    [Visual Search (CBIR)], [Core Research], [
      Primary contribution: integrated multi-model CBIR with pluggable architecture enabling image-based product search across multiple embedding models (CNN, ViT, CLIP-based) within a production-style e-commerce platform.
    ],

    [ML Embedding Pipeline], [Core Research], [
      Critical infrastructure: automated ingestion of product images, generation of vector embeddings via the Python sidecar, storage in pgvector, and HNSW indexing. The operational backbone of the visual search capability.
    ],

    [Model Benchmark System], [Core Research], [
      Secondary contribution: systematic protocol for comparing retrieval accuracy and operational efficiency across 11 embedding models, providing a practical guide for model selection in resource-constrained deployments.
    ],

    [Product Catalog], [Supporting Infrastructure], [
      Required context: provides the structured dataset of fashion products, with variants, images, taxonomies, and metadata, that serves as the search target for CBIR evaluation.
    ],

    [Order System], [Supporting Infrastructure], [
      Metric validation: provides conversion events (add-to-cart, checkout completion) that serve as proxy indicators of search success, enabling evaluation of visual search within a realistic shopping workflow.
    ],

    [Inventory], [Supporting Infrastructure], [
      Realism constraint: ensures that search results reflect actual product availability, preventing the unrealistic scenario where visually similar but out-of-stock items appear in search results.
    ],

    [Authentication], [Supporting Infrastructure], [
      Security baseline: protects administrative functions and user-specific data, enabling the application to operate in a representative security posture without which the system would be a research prototype rather than a deployable platform.
    ],
  ),
    kind: table,
  caption: [
    Classification of feature areas into Core Research and Supporting Infrastructure. Core Research features represent the thesis's original contributions; Supporting Infrastructure features provide the realistic e-commerce context necessary for meaningful evaluation.
  ],
) <tbl-feature-classification>

The classification makes explicit what the thesis does and does not claim as contribution. The CBIR pipeline, encompassing embedding generation, vector storage, and similarity search, is the core research artefact. The e-commerce modules (Catalog, Ordering, Inventory, Payment, Identity) are supporting infrastructure, built to provide a realistic context that validates the visual search results in a production-like environment. This separation is maintained throughout the chapter: Sections 2.3 and 2.4 devote detailed treatment to the research features, while the supporting infrastructure is described only to the extent necessary to understand the system's design.
