=== Architectural Decision and Trade-offs

Specialised vector databases (Pinecone, Milvus, Weaviate) exist for large-scale search. pgvector was selected for its simplicity and transactional integration.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left, center, center),
    [*Feature*], [*Specialised Vector DB*], [*pgvector*],
    [Setup], [Moderate to high], [Low (extension only)],
    [Consistency], [Separate database], [Same transaction as product data],
    [Query language], [Custom API], [Standard SQL],
    [Cost], [Often paid service], [Free, open source],
    [Scale limit], [Billions of vectors], [Millions of vectors],
  ),
  caption: [Comparison of pgvector with specialised vector databases],
)

For thousands to tens of thousands of products, pgvector's simplicity outweighs the scaling advantages of specialised databases.

*Limitations acknowledged:*

- *Scale.* Performs well for millions of vectors; not designed for billion-vector deployments.
- *Maturity.* Fewer features than dedicated vector databases.
- *Distribution.* Does not natively distribute across multiple servers.

For this project's scope (5,000 products in evaluation), these limitations are acceptable. The primary contribution is architectural integration within a conventional e-commerce stack, not massive-scale infrastructure.
