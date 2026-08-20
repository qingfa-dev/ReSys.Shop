=== Data Persistence Architecture

The persistence layer maps eight bounded contexts to dedicated PostgreSQL schemas, co-locating vector embeddings with relational product data in a single PostgreSQL 17 instance.

==== Schema Organisation and Vector Storage

Each bounded context owns an isolated schema. @tbl-schema-mapping outlines the distribution:

#figure(
  table(
    columns: (1.2fr, 3.8fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,
    table.header([*Schema*], [*Principal Tables and Aggregate Entities*]),
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

Cross-schema references use unconstrained UUIDs, enforcing module independence. Product embeddings reside in `catalog.product_image_embeddings` alongside product metadata, participating in the same ACID transactions.

// [SCREENSHOT: implementation-pgadmin-schemas.png] pgAdmin tree view showing eight schema namespaces expanded to reveal their tables: catalog (13 tables), ordering (3), payment (2), inventory (6), identity (9), profile (4), shipping (3), location (2).

==== pgvector Indexing Strategy

Embeddings are stored in a `vector(512)` column with model-aware discriminators (e.g., `Fashion-CLIP`). The production HNSW index uses cosine distance for sub-second CBIR queries (see Section 2.3.4 for index detail and Section 1.4.2 for ANN algorithm comparison).

// [SCREENSHOT: implementation-pgadmin-vector-column.png] pgAdmin column inspector showing the embedding column typed as vector(512), the model_name filter column, the hangfire_job_id tracking column, and the HNSW index entry with vector_cosine_ops.

==== Model-Aware Vector Search

Each embedding carries a `model_name` discriminator. Cosine similarity search proceeds through four stages: candidate retrieval via `<=>` filtered by `model_name`, distance-to-similarity transformation, threshold filtering ($>= 0.70$), and deduplication by parent product.

==== Concurrency and Data Integrity

Three complementary strategies maintain integrity under concurrent throughput:

#figure(
  table(
    columns: (1.4fr, 1.8fr, 2.8fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,
    table.header([*Pattern*], [*Implementation*], [*Applied To*]),
    [Optimistic Locking], [PostgreSQL `xmin` column via EF Core `IsRowVersion()`], [General entity updates (HTTP 409 on stale write)],
    [Pessimistic Locking], [`SELECT FOR UPDATE` row locks], [Stock allocation during checkout (serializes low-quantity purchases)],
    [Repeatable Read], [IsolationLevel.RepeatableRead transaction scope], [Sequential order code generation (prevents phantom reads)],
  ),
  kind: table,
  caption: [Concurrency control mechanisms across system domains.],
) <tbl-concurrency>

Audit tracking uses EF Core interceptors: `AuditInterceptor` auto-populates `CreatedAtUtc`, `CreatedBy`, `UpdatedAtUtc`, and `UpdatedBy` during `SaveChangesAsync`. Stock modifications write append-only entries to `inventory.stock_movements`.
