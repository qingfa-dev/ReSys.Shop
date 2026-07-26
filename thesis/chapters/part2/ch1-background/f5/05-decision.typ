=== Architectural Decision and Trade-offs

Several specialised vector databases exist (Pinecone, Milvus, Weaviate), each optimised for large-scale vector search. pgvector was selected for practical reasons suited to this project's scope.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left, center, center),
    [*Feature*], [*Specialised Vector DB*], [*pgvector*],
    [Setup complexity], [Moderate to high], [Low (PostgreSQL extension)],
    [Data consistency], [Separate from main database], [Same transaction as product data],
    [Query language], [Custom API or query language], [Standard SQL],
    [Cost], [Often paid service (SaaS)], [Free and open source],
    [Scale limit], [Billions of vectors], [Millions of vectors],
  ),
  caption: [Comparison of pgvector with specialised vector databases],
)

For a system with thousands to tens of thousands of products, pgvector's simplicity and transactional consistency outweigh the scaling advantages of specialised databases. If the system needed to scale to tens of millions of products, migration to a dedicated vector database would be considered.

==== Trade-offs Acknowledged

Using pgvector has limitations:

- *Scale ceiling.* pgvector performs well for millions of vectors but is not designed for billion-vector deployments.
- *Less mature.* Fewer features and optimisation options than dedicated vector databases.
- *Single-node.* pgvector does not natively distribute across multiple servers.

For this project's scope (5,000 products in the evaluation, with a target of tens of thousands in production), these limitations are acceptable. The primary contribution is the architectural integration of vector search within a conventional e-commerce stack, not massive-scale infrastructure.
