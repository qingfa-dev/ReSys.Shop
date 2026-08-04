=== Data Persistence Architecture

The persistence layer maps the eight bounded contexts to dedicated PostgreSQL schemas, co-locating high-dimensional vector embeddings with relational product data in a single PostgreSQL 17 database instance.

==== Schema Organisation and Vector Storage

Each bounded context owns an isolated database schema containing its aggregate roots and child entities. @tbl-schema-mapping outlines the schema-per-context distribution:

#figure(
  table(
    columns: (1.2fr, 3.8fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,

    table.header([*Schema*], [*Principal Tables & Aggregate Entities*]),

    [`catalog`], [`products`, `variants`, `variant_images`, `product_image_embeddings`, `taxonomies`, `taxons`],
    [`ordering`], [`orders`, `line_items`, `order_adjustments`, `shipments`],
    [`payment`], [`payment_intents`, `payment_captures`, `payment_methods`],
    [`inventory`], [`stock_locations`, `stock_items`, `stock_movements`, `stock_reservations`],
    [`identity`], [`users`, `roles`, `user_roles`, `refresh_tokens`],
    [`profile`], [`user_profiles`, `addresses`, `wishlists`],
    [`shipping`], [`shipping_methods`, `shipping_rates`, `shipping_zones`],
    [`location`], [`countries`, `states`],
  ),
  kind: table,
  caption: [Schema-per-context mapping. Cross-schema references use UUID attributes without database-level foreign key constraints.],
) <tbl-schema-mapping>

Schema boundaries mirror the C\# compile-time isolation rules. An order entity references a `variant_id` as a unconstrained UUID rather than a database foreign key constraint, enforcing logical module independence without distributed transaction overhead.

*Co-located vector storage.* Product embeddings reside within `catalog.product_image_embeddings` alongside product metadata. Co-locating relational data and vectors inside PostgreSQL eliminates dual-database synchronization issues: vector updates participate in the primary database transaction, guaranteeing ACID consistency without external sync workers.

// [SCREENSHOT: pgadmin-schema-tree.png] pgAdmin tree view showing the eight schemas with their tables expanded under each namespace node.

===== pgvector Indexing Strategy

Image embedding vectors are stored in a `vector(512)` column representing 512-dimensional latent spaces from vision-language models. The persistence layer supports two Approximate Nearest Neighbor (ANN) index types configured for different operational environments:

#figure(
  table(
    columns: (1.2fr, 2.4fr, 2.4fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,

    table.header([*Attribute*], [*HNSW (Hierarchical Navigable Small World)*], [*IVFFlat (Inverted File Flat)*]),

    [*Index Structure*], [Multi-layer navigable graph], [K-means cluster partitioning],
    [*Build Overhead*], [Memory-intensive (several minutes)], [Low memory footprint ($< 1" s"$)],[*Query Latency*], [Sub-millisecond ($< 5" ms"$, logarithmic)], [Fast ($< 10" ms"$, cluster-dependent)],
    [*Deployment*], [Production environment], [Benchmark iteration & constrained testing],
    [*Parameters*], [$m = 16, "ef"_("construction") = 64$], [$"lists" = 100$],
  ),
  kind: table,
  caption: [Comparison of pgvector ANN index algorithms using the cosine distance operator (`<=>`).],
) <tbl-ann-index>

The production HNSW index DDL is executed during migration initialization:

```sql
CREATE INDEX ix_product_image_embeddings_vector_hnsw
ON catalog.product_image_embeddings
USING hnsw (embedding vector_cosine_ops)
WITH (m = 16, ef_construction = 64);
```

===== Model-Aware Vector Search Pipeline

Vector embeddings from different neural architectures inhabit incompatible vector spaces. To prevent cross-model distance pollution, every record persists a `model_name` discriminator (e.g., `"Fashion-CLIP"`). Cosine similarity search follows a four-stage query execution process:

1. *Candidate Retrieval:* Executes a cosine distance query using the `<=>` operator to fetch the top $3 times K$ nearest neighbors filtered by `model_name`.
2. *Similarity Transformation:* Converts distance to similarity metrics via $text("similarity") = 1 - text("cosine_distance")$.
3. *Threshold Filtering:* Discards candidates failing the minimum confidence threshold ($text("similarity") >= 0.70$).
4. *Product Deduplication:* Group-by filtering reduces multiple matching variant images down to the highest-scoring master product entity.

// [SCREENSHOT: pgadmin-vector-column.png] pgAdmin inspector showing the embedding column typed as `vector(512)`, the `model_name` filter column, and the HNSW index entry with `vector_cosine_ops`.

==== Concurrency and Data Integrity

Data integrity under high concurrent throughput is maintained using three complementary concurrency strategies:

#figure(
  table(
    columns: (1.4fr, 1.8fr, 2.8fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,

    table.header([*Pattern*], [*Technical Implementation*], [*Target Operational Domain*]),

    [Optimistic Locking], [PostgreSQL `xmin` system column mapped via EF Core `IsRowVersion()`], [General entity updates. Stale writes trigger `DbUpdateConcurrencyException`, returning HTTP 409 Conflict.],
    [Pessimistic Locking], [`SELECT FOR UPDATE` row locks acquired explicitly], [Stock allocation during checkout. Serializes concurrent purchases of low-quantity SKUs.],
    [Repeatable Read], [`IsolationLevel.RepeatableRead` database transaction scope], [Sequential order code generation. Prevents phantom reads during atomic sequence assignment.],
  ),
  kind: table,
  caption: [Concurrency control mechanisms across system domains.],
) <tbl-concurrency>

===== Audit Ledger and EF Core Interceptors

Entity lifecycle tracking and stock auditing are handled automatically via EF Core `DbContext` interceptors, keeping timestamp boilerplate out of business handlers. Every stock modification writes an append-only entry to `inventory.stock_movements`.

An `AuditInterceptor` registered in the `SaveChanges` pipeline auto-populates audit fields without explicit handler code:

1. On `EntityState.Added`, sets `CreatedAt` to the current UTC timestamp and `CreatedBy` to the authenticated user ID.
2. On `EntityState.Added` or `EntityState.Modified`, sets `UpdatedAt` and `UpdatedBy` identically.

This interceptor pattern eliminates the class of bugs where timestamps are inconsistently applied across different features. Handlers never set audit fields manually; the persistence infrastructure applies them uniformly during `SaveChangesAsync`.
