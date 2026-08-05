=== Architectural Decision and Trade-offs

Specialised vector databases (Pinecone, Milvus, Weaviate) exist for large-scale search. pgvector was selected for simplicity and transactional integration.

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
  caption: [pgvector vs specialised vector databases],
)

For thousands to tens of thousands of products, pgvector's simplicity outweighs scaling advantages. Limitations: not designed for billion-vector deployments, fewer features than dedicated vector databases, no native multi-server distribution. For this project's 5,000-product scope, these are acceptable.
