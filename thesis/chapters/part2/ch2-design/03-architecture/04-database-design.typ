=== Database Design

The ReSys.Shop database consists of a single PostgreSQL 17 instance partitioned into per-context schemas. Each schema is owned by a bounded context and managed via Entity Framework Core migrations.

==== Schema Organisation

Each of the eight bounded contexts manages its own dedicated database schema:
- *Catalog:* `products`, `variants`, `variant_images`, `option_types`, `option_values`, `taxonomies`, and `taxons`.
- *Ordering:* `orders`, `line_items`, `adjustments`, `shipments`, and `inventory_units`.
- *Payment:* `payment_intents`, `payment_captures`, and `payment_logs`.
- *Inventory:* `stock_locations`, `stock_items`, `stock_movements`, and `stock_reservations`.
- *Identity:* `users`, `roles`, `user_roles`, `refresh_tokens`, and `external_logins` (built on ASP.NET Identity Core).
- *Profile:* `user_addresses`, `wishlists`, and `notification_preferences`.
- *Shipping:* `shipping_methods`, `shipping_rates`, and `shipping_zones`.
- *Location:* `countries` and `states` reference data.

Cross-context relationships use identifier references (UUIDs) without database-level foreign key constraints. An order references `UserId` and `VariantId` as loose attributes, maintaining logical module isolation while avoiding distributed transaction overhead.

==== Core Entity-Relationship Model

@fig-erd-core illustrates the entity-relationship model across all eight bounded contexts. Dotted lines indicate cross-context identifier references.

#set figure.caption(position: top)
#rotate(-90deg, reflow: true)[
  #figure(
    image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-core.png", width: 100%),
    caption: [Core entity-relationship model across eight bounded contexts.],
  ) <fig-erd-core>
]

The *Catalog* domain centers on `Product`, which links one-to-many with `Variant`. Each variant stores SKU, pricing, and inventory tracking flags, with one designated as the master variant. `VariantImage` records hold image paths and an optional `vector(512)` embedding column. `Taxonomy` and `Taxon` trees manage hierarchical product classification via self-referencing foreign keys.

The *Ordering* domain centers on `Order`, linking one-to-many with `LineItem`. Line items reference specific variants and capture price snapshots at purchase time to decouple order history from catalog updates. `UserId` references support both registered user checkouts and nullable guest sessions.

==== pgvector Integration

PostgreSQL's *pgvector* extension @pgvector2023 executes vector similarity searches directly within the relational engine. The `variant_images` table stores feature vectors in an `embedding` column defined as `vector(512)`. Two approximate nearest neighbour (ANN) index types support distinct operational profiles:

- *HNSW (Hierarchical Navigable Small World)* @malkov2018efficient: Constructs a multi-layer graph where nodes connect locally and hierarchically.
  - *Query Speed:* Logarithmic search complexity ($< 2\ "ms"$ on 100,000-vector corpora).
  - *Build Cost:* Memory-intensive construction, ideal for read-heavy production workloads.
  - *Recall:* Delivers 95–99% recall at standard configurations (`ef_search = 100`).

- *IVFFlat (Inverted File with Flat Compression):* Partitions vector space into $k$-means clusters for targeted inverted list searches.
  - *Query Speed:* Slightly higher latency than HNSW due to cluster probe scans.
  - *Build Cost:* Rapid index creation with minimal memory overhead, optimal for frequent rebuilds or constrained hardware.
  - *Recall:* Achieves comparable recall when configured with optimal probes (`lists = sqrt(n)`, `probes = sqrt(lists)`).

The platform defaults to *HNSW* using cosine distance to meet the sub-second CBIR latency target (NFR-01a). *IVFFlat* serves as the fallback for local or GPU-constrained environments prioritizing rapid index rebuilds over raw query throughput.

- *Cosine Distance:* Query operator (`<=>`) measures angular distance. Results rank by similarity score $1 - "cosine_distance"$, filtering against a configurable default threshold ($0.70$).
- *Model Isolation:* Every record includes a `model_name` string (e.g., `"Fashion-CLIP"`). Search queries filter by active model name to enforce strict mathematical alignment across visual embeddings.

==== Key Design Decisions

The data layer adheres to five global design rules:
- *UUID Primary Keys:* Uses UUIDs across all tables to enable safe client-side key generation and prevent auto-increment sequential enumeration leakage.
- *Soft Deletion:* Implements an `IsDeleted` boolean column filtered globally by EF Core, preserving referential integrity for historical orders.
- *Audit Columns:* `CreatedAtUtc` and `ModifiedAtUtc` timestamps populate automatically via EF Core save interceptors.
- *Composite Indexes:* High-frequency access paths use targeted composite indexes, such as `(UserId, Status)` and `(SessionId, Status)` on `orders`.
- *Variable Vector Dimensions:* `pgvector` columns support variable dimensionalities per model architecture: $384$ (DINOv2-S), $512$ (Fashion-CLIP), $768$ (DINOv2-B), $1280$ (EfficientNet-B0), and $2048$ (ResNet-50). Embeddings from different models are indexed independently per dimension.

==== Per-Context Schema Description

- *Identity Schema:* Stores Argon2-hashed credentials, security stamps for session revocation, single-use refresh tokens, and linked OAuth logins.
- *Catalog Schema:* `Product` stores fashion-specific metadata (style codes, composition, department). `Variant` manages SKU, pricing, and physical dimensions. `OptionType` and `OptionValue` implement an EAV pattern for dynamic product attributes. `Taxon` records use a nested set model for efficient subtree retrieval.
- *Ordering Schema:* Financial attributes use 18-digit precision and 2-decimal scale (`decimal(18,2)`). Orders track item, adjustment, and shipping subtotals separately. `OrderHistory` provides an append-only state transition audit log.
- *Inventory Schema:* Tracks `QuantityOnHand` and `QuantityReserved` per variant-location pairing using PostgreSQL's `xmin` system column for optimistic concurrency control. `StockMovement` serves as an immutable transaction ledger recording balance deltas, unit costs, and operational user IDs.