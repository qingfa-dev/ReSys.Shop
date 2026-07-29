=== Domain-Driven Design

The backend architecture is structured around eight *bounded contexts* adhering to *Domain-Driven Design* (DDD) principles. Each context manages explicit aggregate boundaries, enforces domain invariants, and uses a shared ubiquitous language across its domain operations.

- *Bounded Context Map:* Eight domain contexts operating under a Conformist integration pattern with Published Language contracts.
- *Aggregates and Invariants:* Four architecturally critical aggregate roots: `Product`, `Order`, `PaymentIntent`, and `StockItem`.
- *Ubiquitous Language:* Formally defined terminology establishing clear conceptual boundaries across distinct contexts.
- *State Machines:* Explicit transition boundaries governing forward-only checkout progression and local-to-gateway payment lifecycles.

==== Bounded Context Map

The platform is partitioned into eight *bounded contexts* along business capability boundaries. Each context independently owns its state model, business logic, and vocabulary: a *Variant* in Catalog represents a sellable entity with pricing and SKU attributes, whereas a *LineItem* in Ordering references that same variant strictly from a purchasing context.

Integration follows a *Conformist* pattern. All contexts inherit common primitive abstractions from the Core Shared Kernel (`Result<T>`, `ICommand`, `IQuery`, and entity base types). Context-to-context communication relies exclusively on *MediatR* `ISender` in-process dispatch: a context emits a command, query, or event notification, and receiving contexts process the request without establishing direct compile-time project references. This in-process model preserves modular isolation without introducing distributed network latency.

@fig-bounded-context-map illustrates the eight contexts and their inter-module communication pathways. The *Published Language* defines shared identifiers and value objects exchanged between contexts. @tbl-context-responsibilities details each context's business capabilities and public data boundaries.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_bounded-context-map.png", width: 100%),
  caption: [Bounded Context Map showing the eight business contexts and the Published Language identifiers exchanged between them. All integration uses in-process MediatR dispatch; no context directly references another context's namespace.],
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
      - Product lifecycle: creation, update, archiving, and fashion metadata\
      - Variants with SKUs, barcodes, dimensions, and independent pricing\
      - Image management triggering automated vector embedding generation\
      - Hierarchical taxonomy classification structures
    ],
    [`ProductId`, `VariantId`, `Sku`, `Price`, `Slug`],

    [Ordering],
    [
      - Customer purchasing workflow from cart through checkout completion\
      - Cart management with automated 7-day inactivity purging\
      - Forward-only checkout state machine execution\
      - Line item capture with locked price snapshots and fee adjustments\
      - Pre-confirmation order cancellation handling
    ],
    [`OrderId`, `OrderNumber`, `Total`, `Currency`, `CheckoutState`],

    [Payment],
    [
      - Payment intent lifecycles: intent creation, authorization, capture, refund, void\
      - Dual gateway abstractions (Stripe production and Bogus test environments)\
      - Local payment state mirroring for offline operation and resilience
    ],
    [`PaymentIntentId`, `PaymentState`, `Amount`],

    [Inventory],
    [
      - Warehouse stock balance tracking across locations\
      - Temporary inventory reservations for active checkouts\
      - Append-only audit logging for stock adjustments\
      - Inter-warehouse transfer processing
    ],
    [`StockItemId`, `QuantityOnHand`, `QuantityReserved`],

    [Identity],
    [
      - JWT-based authentication with single-use refresh token rotation\
      - Automated refresh token reuse detection and session revocation\
      - Role-Based Access Control enforcing `domain:category:action` claims\
      - Anonymous guest session management
    ],
    [`UserId`, `Email`, `PermissionClaim`],

    [Profile],
    [
      - Customer address book management for billing and shipping\
      - Personalized wishlists for product bookmarking\
      - Communication preferences across notification channels
    ],
    [`ProfileId`, `AddressId`],

    [Shipping],
    [
      - Delivery method definition and management\
      - Geographic shipping rate evaluation\
      - Weight- and order-value-based cost calculation rules
    ],
    [`ShippingMethodId`, `Rate`],

    [Location],
    [
      - Reference data for countries and regional states via ISO 3166 standards\
      - Shared reference lookup for Shipping, Profile, and Ordering domains
    ],
    [`CountryId`, `StateId`, `IsoCode`],
  ),
  kind: table,
  caption: [Bounded context responsibilities and Published Language identifiers. The Published Language column lists the value types that other contexts may reference by identifier only, never by importing the source context's namespace.],
) <tbl-context-responsibilities>

==== Aggregates and Invariants

An aggregate root defines a transactional consistency boundary, managing child entities and enforcing core business rules. ReSys.Shop uses a pragmatic DDD strategy without unnecessary base class overhead or forced global domain event dispatches.

- *Product (Catalog Root):* Controls product families, variants, media assets, options, and taxonomy mappings. Enforces catalog-wide URL slug uniqueness, mandates valid option types prior to variant instantiation, and ensures exactly one master variant exists per product family. Variant images store 512-dimensional vector embeddings targeting high-dimensional visual search.
- *Order (Ordering Root):* Coordinates purchasing pipelines from cart assembly to order completion. Locks historical line-item price snapshots at checkout initialization to protect completed orders from subsequent catalog price updates. Enforces the structural balance invariant $ text("Total") = text("ItemTotal") + text("AdjustmentTotal") + text("ShipmentTotal") $. Checkout state progression is strictly forward-only; completed orders remain immutable except through formal administrative cancellations.
- *PaymentIntent (Payment Root):* Models transactional authorization across `Pending`, `RequiresAction`, `Processing`, `Succeeded`, `Canceled`, and `Failed` operational states. Enforces debit limits ensuring cumulative captures cannot exceed initial authorized amounts. Maintains an isolated internal state machine mirroring external gateway responses for testability and offline querying.
- *StockItem (Inventory Root):* Tracks real-time on-hand and reserved stock per warehouse location. Enforces non-negative balance constraints $ text("QuantityOnHand") >= 0 $, writing backorders to an isolated ledger. All stock adjustments write to an append-only `StockMovement` ledger to ensure absolute auditability.

==== Ubiquitous Language Glossary

The ubiquitous language establishes a common domain vocabulary across software design and business rules. @tbl-ubiquitous-language defines key domain terms.

#figure(
  table(
    columns: (1.4fr, 1.2fr, 3.4fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left + horizon),
    inset: 5.5pt,

    table.header([*Term*], [*Context*], [*Definition*]),

    [Product], [Catalog], [Core fashion entity containing shared catalog metadata (description, slug, taxonomy). Prices and SKUs belong exclusively to Variants.],
    [Variant], [Catalog], [Purchasable unit containing SKU, barcode, price, physical dimensions, and stock management flags.],
    [Master Variant], [Catalog], [The primary variant displayed on product listing components. Exactly one master variant is required per product family.],
    [Option Type], [Catalog], [Reusable attribute categories (e.g., Color, Size) shared across catalog entities.],
    [Taxonomy / Taxon], [Catalog], [Hierarchical classification structures (Taxonomy) consisting of nested category nodes (Taxons).],
    [Cart], [Ordering], [Temporary container holding intended purchases. Purged after 7 consecutive days of inactivity.],
    [Line Item], [Ordering], [Cart or order entry linking a specific variant to a quantity and a locked price snapshot.],
    [Checkout State], [Ordering], [Sequential stages: Address → Delivery → Payment → Confirm → Complete. Enforces forward-only progression.],
    [Payment Intent], [Payment], [Tracks authorization states and transaction steps across payment gateway interactions.],
    [Payment Capture], [Payment], [Full or partial monetary debit executed against an authorized payment intent.],
    [Stock Item], [Inventory], [Real-time warehouse inventory record tracking available on-hand and reserved stock quantities.],
    [Stock Movement], [Inventory], [Immutable audit entry recording quantity deltas, updated balances, operator identity, and change reasons.],
    [Refresh Token], [Identity], [Single-use token used to generate short-lived JWT access tokens. Reuse attempts trigger session revocation.]
  ),
  kind: table,
  caption: [Ubiquitous Language Glossary across domain contexts.],
) <tbl-ubiquitous-language>

==== State Machines

Explicit state machines inside domain entities govern checkout and payment workflows, validating transitions prior to database persistence.

===== Order Checkout State Machine

Checkout advances strictly through five stages: Address, Delivery, Payment, Confirm, and Complete (@fig-order-state-machine). Users can cancel at any pre-completion stage without financial impact. Reaching `Complete` finalizes the order, reserves inventory, and locks order data against further edits.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_order-state-machine.png", width: 80%),
  caption: [Order checkout state machine showing forward-only stages and cancellation paths.],
) <fig-order-state-machine>

===== Payment Intent State Machine

Payment intents initialize as `Pending` and transition to `RequiresAction` (for 3D Secure) or `Processing` (@fig-payment-state-machine). Final outcomes resolve to `Succeeded` or `Failed`. Successful authorizations can be captured or refunded. Mirroring gateway states locally enables offline test execution and resilience during network drops.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_payment-state-machine.png", width: 80%),
  caption: [Payment intent lifecycle and terminal states.],
) <fig-payment-state-machine>