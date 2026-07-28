=== Domain-Driven Design

The backend is structured around eight *bounded contexts* following *Domain-Driven Design* (DDD) principles, each with defined aggregate roots, invariants, and a shared ubiquitous language.

- *Bounded Context Map.* Eight contexts, Published Language identifiers, Conformist integration pattern.
- *Aggregates and Invariants.* Product, Order, PaymentIntent, StockItem: four architecturally significant roots.
- *Ubiquitous Language.* Core domain terms with precise definitions across contexts.
- *State Machines.* Order checkout (forward-only), Payment intent lifecycle (parallel system-gateway states).

==== Bounded Context Map

The platform is partitioned into eight *bounded contexts* along business capability boundaries. Each context owns its data, logic, and vocabulary: a *Variant* in Catalog is a sellable unit with SKU and pricing; a *LineItem* in Ordering references the same variant from the purchase perspective.

Integration follows the *Conformist* pattern: all contexts share a technical kernel from the Shared layer (`Result<T>`, `ICommand`, `IQuery`, audit base class). Communication uses exclusively *MediatR* `ISender`: a context dispatches a query or notification, and other contexts react without importing one another's namespace. This in-process model preserves logical isolation without inter-service network overhead.

@fig-bounded-context-map depicts the eight contexts and their integration paths. The *Published Language* consists of shared identifiers and value types that flow between them. @tbl-context-responsibilities lists each context's business responsibility and the identifiers it exposes.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_bounded-context-map.png", width: 100%),
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
    [
      - Product lifecycle: create, update, archive with fashion metadata\
      - Variants with SKUs, barcodes, and independent pricing\
      - Image uploads triggering automatic embedding generation\
      - Hierarchical taxonomies for product classification
    ],
    [ProductId, VariantId, Sku, Price, Slug],

    [Ordering],
    [
      - Customer purchase workflow: cart to completed order\
      - Cart with seven-day auto-expiry\
      - Forward-only checkout state machine\
      - Line items with price snapshots and adjustments\
      - Cancellation at any pre-confirmation stage
    ],
    [OrderId, OrderNumber, Total, Currency, CheckoutState],

    [Payment],
    [
      - Payment intent lifecycle: create, capture, refund, void\
      - Two gateway implementations\
      - Parallel payment state independent of gateway\
      - Consistent behaviour across providers
    ],
    [PaymentIntentId, PaymentState, Amount],

    [Inventory],
    [
      - Physical stock quantities per warehouse\
      - Temporary reservations during active checkouts\
      - Append-only ledger for auditable stock movements\
      - Inter-warehouse transfers
    ],
    [StockItemId, QuantityOnHand, QuantityReserved],

    [Identity],
    [
      - JWT-based authentication with refresh token rotation\
      - Reuse detection and revocation\
      - RBAC with `domain:category:action` claim format\
      - Guest session management
    ],
    [UserId, Email, PermissionClaim],

    [Profile],
    [
      - User addresses for shipping and billing\
      - Wishlists for product bookmarking\
      - Notification preferences (email, SMS)
    ],
    [ProfileId, AddressId],

    [Shipping],
    [
      - Delivery method configuration\
      - Shipping rate calculation by geographic zone\
      - Weight- and distance-based calculators
    ],
    [ShippingMethodId, Rate],

    [Location],
    [
      - Country and state reference data with ISO 3166 codes\
      - Read-only reference shared across Shipping, Profile, Ordering
    ],
    [CountryId, StateId, IsoCode],
  ),
    kind: table,
  caption: [Bounded context responsibilities and Published Language identifiers. The Published Language column lists the value types that other contexts may reference by identifier only, never by importing the source context's namespace.],
) <tbl-context-responsibilities>

==== Aggregates and Invariants

An *aggregate* is a cluster of domain objects treated as a single consistency boundary. All modifications pass through the aggregate *root* entity, which enforces *invariants*: business rules that must hold true across all operations. ReSys.Shop applies DDD pragmatically: aggregate roots and invariants are explicit, but formal value-object base classes and universal domain events are not mandated.

Four aggregates anchor the system:

- *Product* (Catalog root). Encapsulates a product family with variants, images, option configurations, and taxonomy classifications. One variant is designated the *master variant* for listing display. Invariants: unique slug for SEO-friendly URLs; master variant must exist among the product's own variants; option types required before variant configuration. Each variant image stores a 512-dimension *embedding vector* generated by the ML sidecar, enabling cosine similarity search.
- *Order* (Ordering root). Manages the checkout lifecycle from cart to completed purchase, aggregating line items with price snapshots and adjustments. The checkout state machine enforces forward-only progression through Address, Delivery, Payment, Confirm, Complete. Invariants: `Total = ItemTotal + AdjustmentTotal + ShipmentTotal`; confirmed orders are immutable except for cancellation, which releases reserved inventory.
- *PaymentIntent* (Payment root). Models the lifecycle of a customer's payment authorisation, transitioning through `Pending`, `RequiresAction`, `Processing`, `Succeeded`, `Canceled`, and `Failed` states. Invariants: sum of all captures must not exceed the authorised amount. The system maintains its own payment state in parallel with the gateway, enabling consistent behaviour across the Stripe production gateway and the Bogus development gateway.
- *StockItem* (Inventory root). Tracks physical availability of a variant at a warehouse location with two counters: on-hand (physical count) and reserved (checkout holds). Invariants: `QuantityOnHand >= 0`; all quantity changes must be recorded as append-only *StockMovement* ledger entries with before/after balances, reason, and operator identity.

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
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_order-state-machine.png", width: 80%),
  caption: [Order checkout state machine: five sequential states with cancellation available from any pre-confirmation state. The forward-only constraint prevents regressing to earlier checkout stages.],
) <fig-order-state-machine>

The customer begins by providing a shipping address (Address state), selects a delivery method (Delivery state), chooses a payment method (Payment state), reviews the complete order summary (Confirm state), and finalises the purchase (Complete state). At any point before the Complete state, from Address through Confirm, the customer may cancel the checkout process, which terminates the order without financial consequence.

Once an order reaches the Complete state, it becomes finalised: the order record transitions to Pending status, inventory quantities are reserved for each line item, and the payment intent is processed. From this point forward, the order is immutable except for the cancel transition, which captures the cancellation timestamp and releases reserved inventory. This forward-only design ensures that at every stage of the checkout pipeline, the system can unambiguously determine the customer's position and the next required action.

===== Payment Intent State Machine

The payment intent state machine models the full lifecycle of a payment from creation through to terminal completion, reflecting the state transitions of the Stripe payment gateway while maintaining a parallel system-managed state for offline consistency. @fig-payment-state-machine shows all states and transitions.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_payment-state-machine.png", width: 80%),
  caption: [Payment intent lifecycle: the state machine reflects Stripe gateway states while maintaining a parallel system copy for offline operations and Bogus gateway compatibility. Terminal states are Failed, Canceled, and Refunded.],
) <fig-payment-state-machine>

A payment intent is created in the Pending state. It may transition directly to RequiresAction when the payment requires 3D Secure or Strong Customer Authentication, or to Processing when the payment method has been attached without additional authentication challenges. From RequiresAction, successful customer authentication advances the intent to Processing; failure or timeout moves it to Canceled.

From Processing, a successful charge transitions the intent to Succeeded, while a declined or errored charge moves it to Failed. From Succeeded, the funds may be captured, transferring them from the customer's account, or the intent may be cancelled before capture. A captured intent may later be refunded, returning funds to the customer and reaching the terminal Refunded state.

The system maintains its own copy of the payment state in parallel with the gateway's representation. This design decision serves two purposes. First, it enables the Bogus test gateway, a development-only implementation that simulates payment lifecycles without external calls, to operate against the same domain entities as the production Stripe gateway. Second, it allows the system to reason about payment state offline without querying the gateway API, which improves resilience during network interruptions and reduces external dependency during business operations.
