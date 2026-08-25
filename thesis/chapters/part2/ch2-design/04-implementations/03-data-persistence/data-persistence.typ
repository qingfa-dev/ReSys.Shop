=== Data Persistence Architecture

The persistence layer maps eight bounded contexts to dedicated PostgreSQL schemas. Vector embeddings and relational product data are stored together, in a single PostgreSQL 17 instance.

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
  caption: [Schema-per-context mapping with UUID references, no cross-schema foreign keys.],
) <tbl-schema-mapping>

Cross-schema references use unconstrained UUIDs, enforcing module independence. Product embeddings reside in `catalog.product_image_embeddings` alongside product metadata, participating in the same ACID transactions.

// [SCREENSHOT: implementation-pgadmin-schemas.png] pgAdmin tree view showing eight schema namespaces expanded to reveal their tables: catalog (13 tables), ordering (3), payment (2), inventory (6), identity (9), profile (4), shipping (3), location (2).

==== pgvector Indexing Strategy

Embeddings are stored in an untyped `vector` column, discriminated per model through a `model_name` filter column (e.g., `fashion_clip`). Per-model HNSW partial indexes cast the column to each model's dimensionality and use cosine distance for sub-second CBIR queries (see Section 2.3.4 for index detail and Section 1.4.2 for ANN algorithm comparison).

// [SCREENSHOT: implementation-pgadmin-vector-column.png] pgAdmin column inspector showing the untyped embedding column, the model_name filter column, the hangfire_job_id tracking column, and the per-model HNSW index entry with vector_cosine_ops.

==== Model-Aware Vector Search

Each embedding carries a `model_name` discriminator and its `dimensions`. Cosine similarity search retrieves candidates via `<=>` filtered by `model_name` and dimension, transforms distance to similarity, and deduplicates to one result per product; the storefront additionally applies a configurable minimum-similarity threshold and score weighting to the returned scores.

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
