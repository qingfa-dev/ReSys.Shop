=== Domain-Driven Design

The ReSys.Shop platform applies Domain-Driven Design (DDD) to structure its business logic around eight bounded contexts, each with well-defined aggregate roots, domain entities, and invariants. This section presents the context map, the aggregate design with invariants, the ubiquitous language glossary, and the state machines that govern the checkout and payment lifecycles.

==== Bounded Context Map

The eight bounded contexts partition the e-commerce domain along business capability boundaries. Each context owns its data, its domain logic, and its vocabulary, terms that are well-defined within a context may carry different meaning in another. For example, a Variant in the Catalog context is a sellable unit with a SKU and pricing; a LineItem in the Ordering context references that same variant but from the perspective of purchase fulfilment.

The integration between contexts follows the *Conformist* pattern: all contexts conform to a shared technical kernel defined in the Shared layer, which provides the `Result<T>` return type, the `ICommand` and `IQuery` marker interfaces, and the `Entity` base class with audit and versioning columns. Communication occurs exclusively through MediatR `ISender`, a context dispatches a query or publishes a notification, and other contexts react without ever importing one another's namespace. This in-process dispatch model eliminates the network latency of inter-service messaging while preserving the logical isolation of the bounded contexts.

@fig-bounded-context-map depicts the eight contexts and the *Published Language*, the shared identifiers and value types, that flow between them.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/P2S2.2.3_bounded-context-map.png", width: 100%),
  caption: [Bounded Context Map showing the eight business contexts and the Published Language identifiers exchanged between them. All integration uses in-process MediatR dispatch; no context directly references another context's namespace.],
) <fig-bounded-context-map>

@tbl-context-responsibilities details each context's business responsibility and the integration data it exposes to the system.

#figure(
  table(
    columns: (auto, 2fr, auto),
    stroke: 0.5pt,
    align: (left + horizon, left, left),

    table.header([*Context*], [*Business Responsibility*], [*Published Language*]),

    [Catalog],
    [Manages the product lifecycle: creating products with fashion-specific metadata (style code, season, material, department, gender target), defining sellable variants with SKUs and independent pricing, uploading images with automatic embedding generation, and organising products through hierarchical taxonomies.],
    [ProductId, VariantId, Sku, Price, Slug],

    [Ordering],
    [Orchestrates the customer purchase workflow from cart to completed order. Manages cart with seven-day auto-expiry, forward-only checkout state machine, line items with price snapshots, adjustments, and cancellation at any pre-confirmation stage.],
    [OrderId, OrderNumber, Total, Currency, CheckoutState],

    [Payment],
    [Manages payment intent lifecycle, creation, capture, refund, void, across two gateway implementations. Maintains parallel payment state independent of the gateway for offline operations and consistent behaviour across providers.],
    [PaymentIntentId, PaymentState, Amount],

    [Inventory],
    [Tracks physical stock quantities per warehouse, manages temporary reservations during active checkouts to prevent overselling, records auditable stock movements through an append-only ledger, and handles inter-warehouse transfers.],
    [StockItemId, QuantityOnHand, QuantityReserved],

    [Identity],
    [Provides JWT-based authentication with refresh token rotation and reuse detection, role-based and permission-based authorisation with `domain:category:action` claim format, and guest session management for anonymous browsing.],
    [UserId, Email, PermissionClaim],

    [Profile],
    [Manages user addresses for shipping and billing, wishlists for product bookmarking, and notification preferences controlling email and SMS communication channels.],
    [ProfileId, AddressId],

    [Shipping],
    [Configures delivery methods, standard, express, local pickup, and calculates shipping rates by geographic zone using weight- and distance-based calculators.],
    [ShippingMethodId, Rate],

    [Location],
    [Provides country and state reference data with ISO 3166 codes. This context is read-only reference data shared across Shipping (zone configuration), Profile (address validation), and Ordering (checkout address selection).],
    [CountryId, StateId, IsoCode],
  ),
    kind: table,
  caption: [Bounded context responsibilities and Published Language identifiers. The Published Language column lists the value types that other contexts may reference by identifier only, never by importing the source context's namespace.],
) <tbl-context-responsibilities>

==== Aggregates and Invariants

An aggregate is a cluster of domain objects treated as a single consistency boundary. Each aggregate has a root entity through which all modifications must pass. The root enforces invariants, business rules that must hold true at all times within the aggregate boundary. ReSys.Shop takes a pragmatic approach to DDD: it defines aggregate roots and their invariants explicitly but does not require formal value-object base classes or a dedicated domain-event infrastructure for every operation.

The four most architecturally significant aggregates are described below.

*Product (Catalog aggregate root).* The Product aggregate encapsulates a product family and all its variants, images, option configurations, and taxonomy classifications. A product may have one or more variants; exactly one is designated as the master variant displayed on listing pages. The aggregate enforces the following invariants: every product must have a unique slug for SEO-friendly URL generation; a product that declares options (such as size or colour) must have at least one option type defined; and the master variant must exist among the product's own variants. Variant images contain the `embedding` column, a 512-dimensional float vector generated by the ML sidecar, enabling cosine similarity search against the entire image corpus.

*Order (Ordering aggregate root).* The Order aggregate manages the checkout lifecycle from a nascent cart through to a completed purchase. It aggregates line items, each capturing a price snapshot of the variant at the time of purchase, and optional adjustments for discounts or promotions. The aggregate enforces the invariant that `Total = ItemTotal + AdjustmentTotal + ShipmentTotal`, maintaining financial consistency across all modifications. The checkout state progresses forward only, the address, delivery, payment, and confirmation stages must complete in sequence, and once an order is confirmed (finalised), it becomes immutable except for the cancel transition. This forward-only constraint is encoded in the domain entity itself and validated before every state transition.

*PaymentIntent (Payment aggregate root).* The PaymentIntent aggregate models the lifecycle of a customer's intent to pay. It is created with a specified amount and currency, and transitions through states, Pending, RequiresAction, Processing, Succeeded, Canceled, and Failed, based on gateway interactions. The aggregate tracks payment captures, where each capture debits a portion of the authorised amount. The system enforces the invariant that the sum of all captures must not exceed the original intent amount. A separate payment capture goes through its own state transitions: Succeeded → Captured → Refunded / Voided. The system maintains its own payment state in parallel with the gateway state, enabling consistent behaviour whether using the production Stripe gateway or the development Bogus gateway.

*StockItem (Inventory aggregate root).* The StockItem aggregate tracks the physical availability of a product variant at a specific warehouse location. It maintains two quantities: on-hand (physical count from warehouse operations) and reserved (units held for active checkouts). The aggregate enforces the invariant that `QuantityOnHand ≥ 0`, stock cannot go negative. Backorder support allows sales beyond on-hand quantity up to a configured backorder limit, but the system tracks the deficit separately. Quantity changes are not performed directly on StockItem; instead, they must be recorded as StockMovement entries in an append-only ledger, preserving a complete and auditable history of every stock change, including the quantity before and after, the reason, and the operating user.

==== Ubiquitous Language Glossary

A key practice in DDD is establishing a ubiquitous language, a shared vocabulary used by all team members and reflected directly in the codebase. @tbl-ubiquitous-language presents the core terms of the ReSys.Shop domain with their definitions.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),

    table.header([*Term*], [*Context*], [*Definition*]),

    [Product], [Catalog], [
      A product family representing a fashion item concept (e.g., "Cotton T-Shirt"). Holds shared metadata: description, slug, status, fashion-specific attributes, and taxonomy classifications. Does not have a price or SKU directly, those belong to Variants.
    ],

    [Variant], [Catalog], [
      A sellable, physical unit of a product (e.g., "Cotton T-Shirt, Red, Large"). Holds SKU, barcode, pricing, inventory tracking flag, and physical dimensions. Each variant belongs to exactly one product.
    ],

    [Master Variant], [Catalog], [
      The designated default variant shown on product listing pages. Every product with variants must have exactly one master variant.
    ],

    [Option Type], [Catalog], [
      Defines a configurable attribute class such as "Colour", "Size", or "Material". Option types are product-independent and reusable across the catalogue.
    ],

    [Taxonomy / Taxon], [Catalog], [
      A hierarchical categorisation tree for organising products. A Taxonomy (e.g., "Department") contains Taxons (e.g., "Clothing" → "Dresses" → "Evening Dresses") forming a nested set structure.
    ],

    [Cart], [Ordering], [
      An ephemeral collection of line items that represents a customer's intent to purchase. Carts automatically expire after seven days of inactivity. The cart is the initial state of the Order aggregate.
    ],

    [Line Item], [Ordering], [
      A single entry in an order or cart, referencing a specific product variant, quantity, and a price snapshot captured at the time of purchase to insulate historical orders from catalogue price changes.
    ],

    [Checkout State], [Ordering], [
      The sequential stage of the purchase process: Address → Delivery → Payment → Confirm → Complete. Progression is forward-only; cancellation is available from any pre-confirmation stage.
    ],

    [Payment Intent], [Payment], [
      A data structure representing a customer's intent to pay a specified amount. Follows a defined state machine through the gateway interaction lifecycle.
    ],

    [Payment Capture], [Payment], [
      A partial or full debit against a payment intent. Multiple captures can be performed against a single intent (e.g., capturing shipping cost after items ship).
    ],

    [Stock Item], [Inventory], [
      The current state of a product variant's availability at a specific warehouse. Maintains on-hand and reserved quantity counters with optimistic concurrency control to prevent overselling.
    ],

    [Stock Movement], [Inventory], [
      An immutable ledger entry recording every change to a stock item's quantity, the delta, the balance before and after, the reason, and the operating user. The single source of truth for inventory changes.
    ],

    [Refresh Token], [Identity], [
      A long-lived credential used to obtain new short-lived access tokens without re-authentication. Each token is single-use; presenting a previously consumed token triggers revocation of all tokens for that user.
    ],
  ),
    kind: table,
  caption: [Ubiquitous Language Glossary: core domain terms, their owning bounded context, and their precise definitions as used throughout the codebase and this thesis.],
) <tbl-ubiquitous-language>

==== State Machines

Two explicit state machines govern the most critical transactional workflows in the system: the order checkout process and the payment intent lifecycle. Both are encoded in domain entities, validated before every state transition, and drive the sequence of user-facing and system-level actions.

===== Order Checkout State Machine

The order checkout state machine enforces a forward-only progression through five sequential states: Address, Delivery, Payment, Confirm, and Complete. Each state transition is triggered by a specific user action and validated by the domain entity before being committed. @fig-order-state-machine depicts this lifecycle.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/P2S2.2.3_order-state-machine.png", width: 80%),
  caption: [Order checkout state machine: five sequential states with cancellation available from any pre-confirmation state. The forward-only constraint prevents regressing to earlier checkout stages.],
) <fig-order-state-machine>

The customer begins by providing a shipping address (Address state), selects a delivery method (Delivery state), chooses a payment method (Payment state), reviews the complete order summary (Confirm state), and finalises the purchase (Complete state). At any point before the Complete state, from Address through Confirm, the customer may cancel the checkout process, which terminates the order without financial consequence.

Once an order reaches the Complete state, it becomes finalised: the order record transitions to Pending status, inventory quantities are reserved for each line item, and the payment intent is processed. From this point forward, the order is immutable except for the cancel transition, which captures the cancellation timestamp and releases reserved inventory. This forward-only design ensures that at every stage of the checkout pipeline, the system can unambiguously determine the customer's position and the next required action.

===== Payment Intent State Machine

The payment intent state machine models the full lifecycle of a payment from creation through to terminal completion, reflecting the state transitions of the Stripe payment gateway while maintaining a parallel system-managed state for offline consistency. @fig-payment-state-machine shows all states and transitions.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/P2S2.2.3_payment-state-machine.png", width: 80%),
  caption: [Payment intent lifecycle: the state machine reflects Stripe gateway states while maintaining a parallel system copy for offline operations and Bogus gateway compatibility. Terminal states are Failed, Canceled, and Refunded.],
) <fig-payment-state-machine>

A payment intent is created in the Pending state. It may transition directly to RequiresAction when the payment requires 3D Secure or Strong Customer Authentication, or to Processing when the payment method has been attached without additional authentication challenges. From RequiresAction, successful customer authentication advances the intent to Processing; failure or timeout moves it to Canceled.

From Processing, a successful charge transitions the intent to Succeeded, while a declined or errored charge moves it to Failed. From Succeeded, the funds may be captured, transferring them from the customer's account, or the intent may be cancelled before capture. A captured intent may later be refunded, returning funds to the customer and reaching the terminal Refunded state.

The system maintains its own copy of the payment state in parallel with the gateway's representation. This design decision serves two purposes. First, it enables the Bogus test gateway, a development-only implementation that simulates payment lifecycles without external calls, to operate against the same domain entities as the production Stripe gateway. Second, it allows the system to reason about payment state offline without querying the gateway API, which improves resilience during network interruptions and reduces external dependency during business operations.
