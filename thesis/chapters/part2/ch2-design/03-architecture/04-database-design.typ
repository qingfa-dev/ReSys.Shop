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

The *Catalog* domain centers on `Product` with one-to-many `Variant` relationships. `VariantImage` records hold image paths and an optional `vector(512)` embedding column. *Taxonomy* and *Taxon* trees manage hierarchical classification via self-referencing foreign keys.

The *Ordering* domain centers on `Order`, linking one-to-many with `LineItem`. Line items capture price snapshots at purchase time to decouple order history from catalog updates.

==== pgvector Integration

PostgreSQL's *pgvector* extension @pgvector2023 executes vector similarity searches within the relational engine. The `variant_images` table stores feature vectors in an `embedding` column defined as `vector(512)`. The platform defaults to *HNSW* indexing @malkov2018efficient using cosine distance to meet the sub-second CBIR latency target (NFR-01a), with *IVFFlat* as a fallback for local environments (see Section 2.1.5 for index detail).

- *Cosine Distance:* Query operator (`<=>`) measures angular distance. Results rank by similarity score $1 - "cosine_distance"$, filtered against a configurable threshold ($0.70$).
- *Model Isolation:* Every record includes a `model_name` string (e.g., `"Fashion-CLIP"`). Search queries filter by active model to enforce embedding alignment.

==== Key Design Decisions

The data layer adheres to five global design rules:
- *UUID Primary Keys:* Safe client-side key generation and no sequential enumeration leakage.
- *Soft Deletion:* `IsDeleted` flag filtered globally by EF Core, preserving referential integrity.
- *Audit Columns:* `CreatedAtUtc` and `ModifiedAtUtc` populated via EF Core save interceptors.
- *Composite Indexes:* Targeted indexes on high-frequency access paths (`(UserId, Status)`, `(SessionId, Status)` on `orders`).
- *Variable Vector Dimensions:* `pgvector` columns support per-model dimensionalities: $384$ (DINOv2-S), $512$ (Fashion-CLIP), $768$ (DINOv2-B), $1280$ (EfficientNet-B0), $2048$ (ResNet-50).

==== Per-Context Schema Description

- *Identity Schema:* Argon2-hashed credentials, security stamps, single-use refresh tokens, and linked OAuth logins.
- *Catalog Schema:* `Product` stores fashion metadata; `Variant` manages SKU, pricing, and dimensions; `OptionType` and `OptionValue` implement an EAV pattern; `Taxon` uses a nested set model.
- *Ordering Schema:* Financial attributes use `decimal(18,2)`; `OrderHistory` provides an append-only state transition log.
- *Inventory Schema:* `StockMovement` serves as an immutable transaction ledger recording balance deltas, unit costs, and operator IDs.