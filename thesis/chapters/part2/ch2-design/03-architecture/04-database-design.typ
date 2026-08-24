=== Database Design

The ReSys.Shop database is a single PostgreSQL 17 instance partitioned into per-context schemas, each owned by one bounded context and managed through Entity Framework Core migrations. Every table uses a UUID primary key and a soft-deletion flag with a global query filter, while contention-sensitive aggregates carry an optimistic concurrency version. This section presents the schema organisation, the core entity-relationship model, and a per-module view of how each schema realises its domain aggregates, followed by the pgvector integration and the cross-cutting design rules.

==== Schema Organisation

Each bounded context owns a dedicated schema that mirrors its aggregate boundaries rather than a generic normalized model. The Catalog schema holds the product aggregate and its variant, media, option, and taxonomy members. The Identity schema extends ASP.NET Identity with users, roles, claims, refresh tokens, and passkeys. The Ordering schema contains the order aggregate with line items and adjustments. The Payment schema tracks payment captures, configured gateways, and inbound webhook events. The Inventory schema records stock locations, items, an append-only movement ledger, reservations, and inter-location transfers. The Shipping schema defines methods, rate tiers, geographic zones, and shipments. The Location schema provides country and state reference data. The Profile schema stores customer profiles, addresses, and wishlists.

Cross-context relationships use identifier references — loose UUID attributes rather than database-level foreign keys — so an order references user, variant, and address identifiers without coupling the schemas. This keeps modules logically independent and avoids coordinating transactions across contexts.

==== Core Entity-Relationship Model

@fig-erd-core shows the entity-relationship model across all eight bounded contexts. Dotted lines denote cross-context identifier references, while solid lines denote relationships contained within a single schema.

#set figure.caption(position: top)
#rotate(-90deg, reflow: true)[
  #figure(
    image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-core.png", width: 100%),
    caption: [Core entity-relationship model across eight bounded contexts.],
  ) <fig-erd-core>
]

The Catalog context centers on Product with one-to-many Variant relationships, each owning image records whose embeddings power visual search. Taxonomy and Taxon trees manage hierarchical classification through self-referencing foreign keys. The Ordering context centers on Order with one-to-many LineItem, which capture price snapshots so historical orders are insulated from later catalog changes. The remaining modules are detailed per schema below.

==== Module Schemas and Aggregates

Each subsection below reviews one schema: its aggregate roots, the members each root owns, and the invariants that preserve consistency, together with the schema-level entity-relationship diagram.

===== Catalog Schema

The Catalog schema is product-centric. Product is the root aggregate: it owns variants, variant images and their embeddings, option configuration, and classification assignments, and it enforces a unique slug, a designated master variant, and model-tagged embedding vectors. Taxon is a second root that models the category tree using a nested-set representation; taxonomies group its top-level nodes and taxon rules drive automatic classification.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-catalog.png", height: 50%),
  caption: [Catalog schema: product aggregate and taxonomy tree.],
) <fig-erd-catalog>

===== Identity Schema

The Identity schema is built on ASP.NET Identity Core. User is the aggregate root; it owns role and claim assignments, external logins, tokens, and rotating refresh tokens, and it will persist WebAuthn passkeys. Credentials are stored as Argon2 hashes, and single-use refresh tokens rotate through a token family so that a reused token revokes the whole session.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-identity.png", height: 40%),
  caption: [Identity schema: ASP.NET Identity user aggregate.],
) <fig-erd-identity>

===== Ordering Schema

The Ordering schema is anchored by the Order aggregate, which drives the forward-only checkout state machine. Line items snapshot the ordered variants at purchase time, and adjustments record polymorphic tax, shipping, and discount lines. The order enforces total consistency (total equals item plus adjustment plus shipment totals) and keeps the checkout, payment, and shipment sub-states advancing forward only.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-ordering.png", height: 30%),
  caption: [Ordering schema: order aggregate with line items and adjustments.],
) <fig-erd-ordering>

===== Payment Schema

The Payment schema centers on the PaymentCapture aggregate, which tracks a single authorisation through the Stripe lifecycle. Payment methods define the configured gateways, and webhook events log inbound Stripe notifications. The capture enforces that captured and refunded amounts never exceed the authorised amount and remains idempotent across replayed webhooks.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-payment.png", height: 30%),
  caption: [Payment schema: payment capture aggregate and gateway configuration.],
) <fig-erd-payment>

===== Inventory Schema

The Inventory schema models stock through three roots. StockItem is the authoritative on-hand quantity per variant and location, protected by a uniqueness constraint and an append-only StockMovement ledger. StockReservation holds temporary quantities during checkout with an expiry-based auto-release. StockTransfer moves stock between two locations through a created-to-cancelled lifecycle, enumerated by transfer items.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-inventory.png", height: 40%),
  caption: [Inventory schema: stock item, reservation, and transfer aggregates.],
) <fig-erd-inventory>

===== Shipping Schema

The Shipping schema centres on the Shipment aggregate, which ties an order to a carrier and a destination address. Shipping methods define the carriers, shipping rates hold tiered cost bands, and shipping method zones restrict a method to specific countries and states.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-shipping.png", height: 35%),
  caption: [Shipping schema: shipment aggregate with methods, rates, and zones.],
) <fig-erd-shipping>

===== Location Schema

The Location schema provides ISO reference data. Country is the root aggregate and owns its State subdivisions, providing the validation rules and shipping-zone inputs used across the platform.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-location.png", width: 60%),
  caption: [Location schema: country and state reference data.],
) <fig-erd-location>

===== Profile Schema

The Profile schema is customer-centric. UserProfile is the 1:1 root complementing the identity user and owns billing and shipping addresses. Wishlist is a second root that groups WishedItem entries, each referencing a variant, and enforces that a variant appears at most once per wishlist.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_erd-profile.png", height: 35%),
  caption: [Profile schema: user profile and wishlist aggregates.],
) <fig-erd-profile>

==== pgvector Integration

PostgreSQL's pgvector extension runs vector similarity searches inside the relational database. The variant_image_embeddings table stores feature vectors in a vector(512) column, matching the Fashion-CLIP output dimensionality. The platform defaults to HNSW indexing with cosine distance to meet the sub-second CBIR latency target (NFR-01a), with IVFFlat as a fallback for local environments (see Section 1.4.2 for index detail).

- *Cosine Distance:* The query operator (<=>) measures angular distance. Results rank by similarity score $1 - "cosine_distance"$, filtered against a configurable threshold ($0.70$).
- *Model Isolation:* Every record carries a model_name (e.g., Fashion-CLIP). Search queries filter by the active model to enforce embedding alignment.

==== Key Design Decisions

The data layer follows five global design rules:

- *UUID Primary Keys:* Safe client-side key generation and no sequential enumeration leakage.
- *Soft Deletion:* The soft-deletion flag is filtered globally by EF Core, preserving referential integrity.
- *Audit Columns:* Creation and modification timestamps are populated by EF Core save interceptors.
- *Composite Indexes:* Targeted indexes serve high-frequency access paths, such as (user_id, status) and (session_id, status) on orders.
- *Fixed Vector Dimensions:* The vector column uses vector(512), matching Fashion-CLIP. Other candidate models produce different dimensionalities, which would require per-model columns or a separate embedding table; the current single-column schema favours operational simplicity.
