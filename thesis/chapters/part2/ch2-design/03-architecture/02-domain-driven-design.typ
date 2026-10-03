=== Domain-Driven Design

The backend follows *Domain-Driven Design* (DDD) with eight bounded contexts, each managing explicit aggregate boundaries and domain invariants under a Conformist integration pattern via Published Language contracts.

==== Bounded Context Map

The platform is partitioned into eight *bounded contexts* along business capability boundaries. Each context independently owns its state model, business logic, and vocabulary. Integration follows a *Conformist* pattern with core abstractions from the Shared Kernel (#emph("Result<T>"), #emph[ICommand], #emph[IQuery]). Context-to-context communication uses only *MediatR* #emph[ISender] in-process dispatch without direct compile-time project references.

@fig-bounded-context-map illustrates the eight contexts and their inter-module communication pathways. @tbl-context-responsibilities details each context's business capabilities and Published Language boundaries.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_bounded-context-map.png", width: 100%),
  caption: [Bounded Context Map: eight contexts with Published Language identifiers.],
) <fig-bounded-context-map>

#figure(
  table(
    columns: (1.2fr, 2.8fr, 2.0fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left + horizon),
    inset: 5.5pt,

    table.header([*Context*], [*Business Responsibility*], [*Published Language*]),

    [Catalog],
    [
      - Product lifecycle, variants with SKU and independent pricing\
      - Image management with automated vector embedding generation\
      - Hierarchical taxonomy classification
    ],
    [#emph[ProductId], #emph[VariantId], #emph[Sku], #emph[Price], #emph[Slug]],

    [Ordering],
    [
      - Checkout workflow with forward-only state machine\
      - Line item capture with locked price snapshots
    ],
    [#emph[OrderId], #emph[OrderNumber], #emph[Total], #emph[Currency], #emph[CheckoutState]],

    [Payment],
    [
      - Payment intent lifecycle (authorize, capture, refund, void)\
      - Local state mirroring for offline operation
    ],
    [#emph[PaymentIntentId], #emph[PaymentState], #emph[Amount]],

    [Inventory],
    [
      - Stock tracking with reservations to prevent overselling\
      - Append-only audit logging for stock adjustments
    ],
    [#emph[StockItemId], #emph[QuantityOnHand], #emph[QuantityReserved]],

    [Identity],
    [
      - JWT authentication with refresh token rotation\
      - RBAC with domain.category.resource.action claims
    ],
    [#emph[UserId], #emph[Email], #emph[PermissionClaim]],

    [Profile],
    [
      - Customer address book and wishlists
    ],
    [#emph[ProfileId], #emph[AddressId]],

    [Shipping],
    [
      - Delivery method definitions, rate calculation, and zone configuration
    ],
    [#emph[ShippingMethodId], #emph[Rate]],

    [Location],
    [
      - ISO 3166 country and state reference data
    ],
    [#emph[CountryId], #emph[StateId], #emph[IsoCode]],
  ),
  kind: table,
  caption: [Bounded context responsibilities and Published Language identifiers.],
) <tbl-context-responsibilities>

==== Aggregates and Invariants

An aggregate root defines a transactional consistency boundary, managing child entities and enforcing core business rules using a pragmatic DDD strategy without base class overhead or forced domain event dispatches.

- *Product (Catalog Root):* Manages product families, variants, media, options, and taxonomy mappings. Enforces slug uniqueness and a designated master variant. Variant images store 512-dimensional vector embeddings for visual search.
- *Order (Ordering Root):* Governs the purchasing pipeline with locked price snapshots at checkout initialization. Enforces the structural balance invariant $ text("Total") = text("ItemTotal") + text("AdjustmentTotal") + text("ShipmentTotal") $. Completed orders are immutable except through formal administrative cancellations.
- *PaymentIntent (Payment Root):* Models transactional authorization states (#emph[Pending], #emph[RequiresAction], #emph[Processing], #emph[Succeeded], #emph[Canceled], #emph[Failed]). Enforces cumulative capture debit limits and mirrors gateway states locally.
- *StockItem (Inventory Root):* Tracks on-hand and reserved stock per warehouse location. Enforces non-negative balance constraints $ text("QuantityOnHand") >= 0 $ and writes all adjustments to an append-only ledger.

==== Ubiquitous Language Glossary

The ubiquitous language establishes a common domain vocabulary across software design and business rules. @tbl-ubiquitous-language defines key domain terms.

#figure(
  table(
    columns: (1.4fr, 1.2fr, 3.4fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left + horizon),
    inset: 5.5pt,

    table.header([*Term*], [*Context*], [*Definition*]),

    [Product], [Catalog], [Core entity with shared metadata (description, slug, taxonomy). Prices and SKUs belong exclusively to Variants.],
    [Variant], [Catalog], [Purchasable unit with SKU, barcode, price, dimensions, and stock flags.],
    [Master Variant], [Catalog], [Primary variant for product listings. One required per product.],
    [Option Type], [Catalog], [Reusable attribute categories (e.g., Color, Size) shared across products.],
    [Taxonomy / Taxon], [Catalog], [Hierarchical classification with nested category nodes.],

    [Cart], [Ordering], [Temporary purchase container. Purged after 7 days of inactivity.],
    [Line Item], [Ordering], [Entry linking a variant to a quantity and locked price snapshot.],
    [Checkout State], [Ordering], [Sequential stages (Address → Delivery → Payment → Confirm → Complete) with forward-only progression.],

    [Payment Intent], [Payment], [Tracks authorization states across payment gateway interactions.],
    [Payment Capture], [Payment], [Monetary debit executed against an authorized payment intent.],

    [Stock Item], [Inventory], [Warehouse record tracking on-hand and reserved stock per location.],
    [Stock Movement], [Inventory], [Immutable audit entry recording quantity deltas, operator, and reason.],

    [Refresh Token], [Identity], [Single-use token for JWT access token generation. Reuse triggers session revocation.]
  ),
  kind: table,
  caption: [Ubiquitous Language Glossary across domain contexts.],
) <tbl-ubiquitous-language>

==== State Machines

Explicit state machines inside domain entities govern checkout and payment workflows, validating transitions prior to database persistence.

===== Order Checkout State Machine

Checkout advances through five forward-only stages: Address, Delivery, Payment, Confirm, and Complete (@fig-order-state-machine). Users can cancel at any pre-completion stage.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_order-state-machine.png", height: 60%),
  caption: [Order checkout state machine showing forward-only stages and cancellation paths.],
) <fig-order-state-machine>

===== Payment Intent State Machine

Payment intents follow a lifecycle from #emph[Pending] through #emph[Processing] to #emph[Succeeded] or #emph[Failed] (@fig-payment-state-machine). Authorized intents support capture and refund operations.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_payment-state-machine.png", height: 60%),
  caption: [Payment intent lifecycle and terminal states.],
) <fig-payment-state-machine>