== Domain Model Overview

ReSys.Shop implements a *Domain-Driven Design (DDD)* approach with the following characteristics:

- *Aggregates* are the consistency boundary; each aggregate root is an entity with a unique identity
- *Entities* have lifecycle continuity (e.g., `Order`, `Product`, `Variant`)
- *Value Objects* are immutable and equality-based (e.g., `Money` concepts represented as `decimal` with currency string)
- *Domain Services* are stateless operations that don't belong to an entity (e.g., `ShippingRateCalculator`)
- *Domain Events* are raised for significant state changes (e.g., `OrderPlacedEvent` --- though the current publisher infrastructure is in flux, see CONCERNS.md)

*Design decision*: The project pragmatically avoids over-engineering DDD patterns. There are no explicit `ValueObject` base classes; instead, value semantics are modeled via records or primitive types with validation. The aggregate boundaries are enforced through EF Core navigation properties and handler-level transaction control, not through event sourcing or complex repository patterns.

*Evidence*: `AGENTS.md:10-12`, `Module/*/Domain/*/*.cs`, `Shared/Application/Domain/Models/Entity.cs`

== Bounded Context Map

ReSys.Shop is decomposed into *8 bounded contexts*, each corresponding to one business module. A formal Context Map diagram is maintained at `docs/thesis/diagrams/bounded-context-map.mmd`.

*Integration pattern*: All contexts integrate via *in-process message dispatch* (MediatR `ISender`). There are no direct namespace references between contexts --- this is the *Conformist* pattern with a shared technical kernel (`Shared.Application` containing `Result<T>`, `ICommand`, `IQuery`).

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Context*], [*Responsibilities*], [*Published Language (shared terms)*],
    [*Catalog*], [Products, variants, images, taxonomies, option types, classifications], [`ProductId`, `VariantId`, `Sku`, `Price`, `Slug`],
    [*Ordering*], [Carts, orders, line items, adjustments, checkout state machine], [`OrderId`, `OrderNumber`, `Total`, `Currency`, `CheckoutState`],
    [*Payment*], [Payment intents, captures, refunds, voids, Stripe webhooks], [`PaymentIntentId`, `PaymentState`, `ClientSecret`],
    [*Inventory*], [Stock locations, items, reservations, transfers, movements], [`StockItemId`, `QuantityOnHand`, `StockLocationId`],
    [*Identity*], [Users, roles, permissions, JWT tokens, OAuth, guest sessions], [`UserId`, `Email`, `PermissionClaim`],
    [*Profile*], [Addresses, wishlists, notification preferences], [`ProfileId`, `AddressId`, `WishlistId`],
    [*Shipping*], [Shipping methods, rates, calculators, zones], [`ShippingMethodId`, `Rate`, `Zone`],
    [*Location*], [Countries, states, ISO codes], [`CountryId`, `StateId`, `IsoCode`],
  ),
  caption: [Bounded Context Map],
)

*Design rationale*: Using a modular monolith with 8 namespaces in one assembly means the bounded contexts share a common *technical kernel* (`Shared.Application`). This is a pragmatic trade-off: we get compile-time type safety and easy refactoring across contexts, while still maintaining logical boundaries through convention and the `ValidateVerticalSliceIsolation` target (currently disabled, but documented in `Directory.Build.targets:42-53`).

*Evidence*: `AGENTS.md:10-12`, `Module/*/Domain/`, `Shared/Application/`, `Directory.Build.targets:42-53`

== Aggregate Boundaries

An aggregate is a cluster of associated objects treated as a single unit for data changes. Each aggregate has one root entity.

=== Catalog Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children / Value Objects*], [*Invariants*],
    [*Product*], [`Product`], [`Variant` (1..n), `ProductOptionType` (0..n), `Classification` (0..n)], [Slug unique; `HasVariants` → ≥1 `OptionType`; `MasterVariantId` must exist],
    [*Variant*], [`Variant`], [`Price` (1..n), `OptionValueVariant` (0..n), `VariantImage` (0..n)], [SKU unique; `IsMaster` exclusive per Product; `TrackInventory` → StockItem exists],
    [*Taxonomy*], [`Taxonomy`], [`Taxon` (tree structure)], [Taxon tree integrity],
    [*OptionType*], [`OptionType`], [`OptionValue` (1..n)], [Values required],
  ),
  caption: [Catalog Aggregates],
)

*Evidence*: `Module/Catalog/Domain/Products/Product.cs`, `Module/Catalog/Domain/Products/Variants/Variant.cs`, `Module/Catalog/Persistence/CatalogSchema.cs:1-31`

==== Product Aggregate Class Diagram

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Class*], [*Properties*], [*Methods / Notes*],
    [`Product`], [`Id: Guid`, `Name: string`, `Description: string`, `Status: ProductStatus`, `Slug: string`, `MetaTitle: string?`, `MetaDescription: string?`, `AvailableOn: DateTimeOffset?`, `DiscontinueOn: DateTimeOffset?`, `StyleCode: string?`, `SeasonName: string?`, `MaterialComposition: string?`, `Department: string?`, `GenderTarget: string?`, `MasterVariantId: Guid`, `IsDeleted: bool`, `CreatedAtUtc: DateTimeOffset`, `ModifiedAtUtc: DateTimeOffset?`], [`Create() → Product`],
    [`Variant`], [`Id: Guid`, `ProductId: Guid`, `IsMaster: bool`, `Sku: string?`, `Barcode: string?`, `Price: decimal?`, `CostPrice: decimal?`, `TrackInventory: bool`, `Weight/Height/Width/Depth: decimal?`, `IsDeleted: bool`], [`Create() → Variant`],
    [`VariantImage`], [`Id: Guid`, `VariantId: Guid`, `FilePath: string`, `AltText: string?`, `Position: int`, `Embedding: float[]`], [Stores 512-d embedding for CBIR],
    [`Price`], [`Id: Guid`, `VariantId: Guid`, `Amount: decimal`, `Currency: string`, `ValidFrom: DateTimeOffset?`, `ValidUntil: DateTimeOffset?`], [Time-bound pricing],
    [`OptionType`], [`Id: Guid`, `Name: string`, `Presentation: string`, `Position: int`], [e.g., "Size", "Color"],
    [`OptionValue`], [`Id: Guid`, `OptionTypeId: Guid`, `Name: string`, `Presentation: string`, `Position: int`], [e.g., "Small", "Red"],
    [`OptionValueVariant`], [`OptionValueId: Guid`, `VariantId: Guid`], [Associative entity],
    [`Taxonomy`], [`Id: Guid`, `Name: string`, `Slug: string`, `Position: int`], [Classification tree root],
    [`Taxon`], [`Id: Guid`, `TaxonomyId: Guid`, `ParentId: Guid?`, `Name: string`, `Slug: string`, `Position: int`], [Self-referencing tree via `ParentId`],
    [`Classification`], [`ProductId: Guid`, `TaxonId: Guid`], [Associative entity — links products to taxons],
  ),
  caption: [Product aggregate class diagram (Catalog context)],
)

*Relationships*: Product → Variant (1:n, one master); Variant → VariantImage (1:n); Variant → Price (1:n); Variant → OptionValueVariant (1:n); OptionValueVariant → OptionValue (n:1); OptionValue → OptionType (n:1); Product → Classification (1:n); Classification → Taxon (n:1); Taxon → Taxonomy (n:1); Taxon → Taxon (self-referencing parent).

=== Ordering Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children / Value Objects*], [*Invariants*],
    [*Order*], [`Order`], [`LineItem` (1..n), `Adjustment` (0..n)], [`Total = ItemTotal + AdjustmentTotal + ShipmentTotal`; checkout state advances forward; finalized orders immutable except Cancel],
    [*Cart*], [(implicit --- `Order` with `Status = Draft` and `SessionId`)], [Same as Order], [Expires after 7 days],
  ),
  caption: [Ordering Aggregates],
)

*Evidence*: `Module/Ordering/Domain/Orders/Order.cs`, `Order.Constant.cs:1-98`

==== Order Aggregate Class Diagram

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Class*], [*Properties*], [*Methods / Notes*],
    [`Order`], [`Id: Guid`, `Number: string`, `SessionId: string?`, `UserId: Guid?`, `Status: OrderStatus`, `CheckoutState: CheckoutState`, `Currency: string`, `ItemTotal: decimal`, `AdjustmentTotal: decimal`, `ShipmentTotal: decimal`, `Total: decimal`, `PaymentTotal: decimal`, `OutstandingBalance: decimal`, `PaymentState: string?`, `ShipmentState: string?`, `IsDeleted: bool`, `CompletedAtUtc: DateTimeOffset?`, `CanceledAtUtc: DateTimeOffset?`], [`Checkout() → Result<Order>`, `Cancel(reason) → Result<Order>`, `Finalize() → Result<Order>`],
    [`LineItem`], [`Id: Guid`, `OrderId: Guid`, `VariantId: Guid`, `Quantity: int`, `Price: decimal`, `Total: decimal`], [References Variant, not Product],
    [`Adjustment`], [`Id: Guid`, `OrderId: Guid`, `Label: string`, `Amount: decimal`, `IsIncluded: bool`], [e.g., discounts, surcharges],
    [`OrderStatus` `(enum)`], [`Draft`, `Pending`, `Complete`, `Canceled`], [Cart = Draft + SessionId],
    [`CheckoutState` `(enum)`], [`Address`, `Delivery`, `Payment`, `Confirm`, `Complete`], [Forward-only state machine],
  ),
  caption: [Order aggregate class diagram (Ordering context)],
)

*Relationships*: Order → LineItem (1:n); Order → Adjustment (1:n); Order → OrderStatus (uses); Order → CheckoutState (uses).

=== Payment Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children*], [*Invariants*],
    [*PaymentIntent*], [`PaymentIntent`], [`PaymentCapture` (0..n)], [State machine: `Pending` → `RequiresAction` → `Processing` → `Succeeded` / `Canceled`],
  ),
  caption: [Payment Aggregates],
)

*Evidence*: `Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs`, `PaymentCapture.Method.State.cs`

==== Payment Aggregate Class Diagram

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Class*], [*Properties*], [*Methods / Notes*],
    [`PaymentIntent`], [`Id: Guid`, `OrderId: Guid`, `GatewayId: string`, `GatewayType: string`, `Status: PaymentStatus`, `Amount: decimal`, `Currency: string`, `ClientSecret: string?`, `CreatedAtUtc: DateTimeOffset`], [`Create() → PaymentIntent`, `Confirm() → Result<PaymentIntent>`, `Capture() → Result<PaymentCapture>`],
    [`PaymentCapture`], [`Id: Guid`, `PaymentIntentId: Guid`, `Amount: decimal`, `Currency: string`, `Status: CaptureStatus`, `GatewayCaptureId: string?`, `CreatedAtUtc: DateTimeOffset`], [`Create() → PaymentCapture`, `Refund(amount) → Result<PaymentCapture>`, `Void() → Result<PaymentCapture>`],
    [`PaymentMethod`], [`Id: Guid`, `Type: string`, `LastFourDigits: string?`, `Brand: string?`, `ExpiryMonth: int?`, `ExpiryYear: int?`, `IsDefault: bool`], [Payment card metadata],
    [`PaymentStatus` `(enum)`], [`Pending`, `RequiresAction`, `Processing`, `Succeeded`, `Canceled`, `Failed`], [Mirrors Stripe lifecycle],
    [`CaptureStatus` `(enum)`], [`Pending`, `Captured`, `Refunded`, `Voided`], [Tracks capture-level state],
  ),
  caption: [Payment aggregate class diagram (Payment context)],
)

*Relationships*: PaymentIntent → PaymentCapture (1:n); PaymentIntent → PaymentStatus (uses); PaymentCapture → CaptureStatus (uses); PaymentIntent → PaymentMethod (references).

=== Identity Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children*], [*Invariants*],
    [*User*], [`User` (extends `IdentityUser<Guid>`)], [`UserClaim`, `UserRole`, `UserLogin`, `UserToken`, `UserPasskey`], [Email uniqueness enforced by ASP.NET Identity],
    [*Role*], [`Role`], [`RoleClaim`], [Permission claims stored as claims],
  ),
  caption: [Identity Aggregates],
)

*Evidence*: `Shared/Security/Identity/Domain/Users/User.cs`, `Shared/Security/Identity/Domain/Roles/Role.cs`

==== Identity Aggregate Class Diagram

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Class*], [*Properties*], [*Methods / Notes*],
    [`User`], [`Id: Guid`, `Email: string`, `PhoneNumber: string?`, `EmailConfirmed: bool`, `PhoneNumberConfirmed: bool`, `TwoFactorEnabled: bool`, `LockoutEnd: DateTimeOffset?`], [Extends `ASP.NET Identity User<Guid>`],
    [`Role`], [`Id: Guid`, `Name: string`, `Description: string?`], [Role-based authorization],
    [`UserRole`], [`UserId: Guid`, `RoleId: Guid`], [Join entity: User ↔ Role],
    [`UserClaim`], [`Id: Guid`, `UserId: Guid`, `ClaimType: string`, `ClaimValue: string`], [Claims-based permissions],
    [`RoleClaim`], [`Id: Guid`, `RoleId: Guid`, `ClaimType: string`, `ClaimValue: string`], [Role-level claims],
    [`PermissionContext` `(static)`], [`Registry: Dictionary<string, PermissionDescriptor>`], [`Register(domain, category, actions)`, `Resolve(policyName) → PermissionRequirement`],
    [`PermissionDescriptor`], [`Domain: string`, `Category: string`, `Action: string`, `FullName: string`], [e.g., `catalog:products:create`],
    [`ICurrentUser` `<<interface>>`], [`UserId: Guid?`, `UserName: string?`, `Email: string?`, `IsAuthenticated: bool`], [`HasPermission(permission) → bool`],
  ),
  caption: [Identity aggregate class diagram (Identity context)],
)

*Relationships*: User → UserRole (1:n); UserRole → Role (n:1); User → UserClaim (1:n); Role → RoleClaim (1:n); PermissionContext → PermissionDescriptor (registers).

=== Inventory Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children*], [*Invariants*],
    [*StockLocation*], [`StockLocation`], [`StockItem` (1..n)], [Location must be active to hold stock],
    [*StockItem*], [`StockItem`], [`StockMovement` (0..n)], [Quantity cannot go negative (enforced by domain method)],
    [*StockReservation*], [`StockReservation`], [---], [Linked to cart/order; auto-expires],
    [*StockTransfer*], [`StockTransfer`], [---], [Source and destination must be different],
  ),
  caption: [Inventory Aggregates],
)

*Evidence*: `Module/Inventory/Domain/StockLocations/StockLocation.cs`, `Module/Inventory/Domain/StockLocations/StockItems/StockItem.cs`

=== Profile Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children*], [*Invariants*],
    [*UserProfile*], [`UserProfile`], [`Address` (0..n), `Wishlist` (0..1)], [One profile per user],
    [*Wishlist*], [`Wishlist`], [`WishedItem` (0..n)], [Product + Variant uniqueness within list],
    [*NotificationPreferences*], [`NotificationPreferences`], [---], [Per-channel enablement flags],
  ),
  caption: [Profile Aggregates],
)

*Evidence*: `Module/Profile/Domain/UserProfile.cs`, `Module/Profile/Domain/Wishlists/Wishlist.cs`

=== Shipping Aggregates

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Aggregate*], [*Root*], [*Children*], [*Invariants*],
    [*ShippingMethod*], [`ShippingMethod`], [`ShippingRate` (1..n)], [Method must have at least one rate],
  ),
  caption: [Shipping Aggregates],
)

*Evidence*: `Module/Shipping/Domain/ShippingMethods/ShippingMethod.cs`, `Module/Shipping/Domain/ShippingRates/ShippingRate.cs`

== Entities and Value Objects

=== Base Entity Design

All domain entities inherit from `Entity` (defined in `Shared.Application.Domain.Models`):

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
}
```

*Design rationale*: Using `Guid` as the primary key type provides:

- Natural distributed ID generation (no central sequence required)
- Security through obscurity (sequential IDs leak business volume)
- Easier data merging across environments

Trade-off: GUIDs are larger than integers (16 bytes vs 4-8 bytes) and fragment clustered indexes. Mitigation: The project uses PostgreSQL which handles UUID indexes efficiently; composite indexes on `(UserId, Status)` and `(SessionId, Status)` are explicitly added for query performance (`git log: commit bd042088`).

*Evidence*: `Shared/Application/Domain/Models/Entity.cs`, `Order.cs:14` (inherits `Entity`)

=== Cross-Cutting Domain Concerns

The project uses interface markers for cross-cutting behavior rather than base classes (to avoid diamond inheritance):

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (start, start, start),
    [*Concern*], [*Interface*], [*Applied to*],
    [Auditable], [`IAuditable`], [All business entities],
    [Soft-deletable], [`ISoftDeletable`], [All business entities],
    [Versionable], [`IVersionable`], [Entities requiring optimistic concurrency],
  ),
  caption: [Cross-Cutting Domain Concerns],
)

*Evidence*: `Shared/Application/Domain/Concerns/Auditable/IAuditable.cs`, `SoftDeletable/ISoftDeletable.cs`, `Shared/Operational/Persistence/Interceptors/Auditable.Interceptor.cs`

== State Machine Diagrams

=== Order Lifecycle (CheckoutState)

#figure(
  table(
    columns: (auto, 1fr, auto),
    align: (start, start, start),
    [*From*], [*Trigger*], [*To*],
    [start], [Create cart / start checkout], [Address],
    [Address], [Set shipping address], [Delivery],
    [Address], [Cancel], [end],
    [Delivery], [Select shipping method], [Payment],
    [Delivery], [Cancel], [end],
    [Payment], [Select payment method], [Confirm],
    [Payment], [Cancel], [end],
    [Confirm], [Confirm order], [Complete],
    [Confirm], [Cancel], [end],
    [Complete], [Order finalized], [end],
  ),
  caption: [Order Checkout State Transitions],
)

The order checkout state machine progresses through five states: *Address* (initial) → *Delivery* → *Payment* → *Confirm* → *Complete*. State transitions are monotonic (no backward transitions except cancellation of the entire order).

*Business rule*: CheckoutState progresses forward; finalized orders are immutable except Cancel (`Order.cs:12`).

*Evidence*: `Order.cs:20` (`CheckoutState` property), `Order.Constant.cs:50-56`

=== Payment Intent Lifecycle

#figure(
  table(
    columns: (auto, 1fr, auto),
    align: (start, start, start),
    [*From*], [*Trigger*], [*To*],
    [start], [Create intent], [Pending],
    [Pending], [3D Secure / SCA required], [RequiresAction],
    [Pending], [Payment method attached], [Processing],
    [Pending], [Cancel intent], [Canceled],
    [RequiresAction], [Customer authenticates], [Processing],
    [RequiresAction], [Authentication fails], [Canceled],
    [Processing], [Charge succeeds], [Succeeded],
    [Processing], [Charge fails], [Failed],
    [Succeeded], [Capture funds], [Captured],
    [Succeeded], [Cancel (before capture)], [Canceled],
    [Captured], [Refund payment], [Refunded],
    [Captured], [Fulfillment complete], [end],
    [Failed], [Retry or abandon], [end],
    [Canceled], [Order canceled], [end],
    [Refunded], [Money returned], [end],
  ),
  caption: [Payment Intent State Transitions],
)

The payment intent state machine tracks the lifecycle of a payment through these states. Each state transition is driven by webhook events from the payment gateway (Stripe) or by explicit gateway API calls.

*Design decision*: The system maintains its own `PaymentIntent` entity state in parallel with Stripe's state to support the Bogus gateway and enable offline operations.

*Evidence*: `PaymentCapture.Method.State.cs`, `StripeWebhook.cs`

== Business Rules (Domain Invariants)

Business rules are enforced at three levels:

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Level*], [*Mechanism*], [*Example*],
    [*Domain entity*], [Property setters / factory methods], [`Order.Total` is computed, not set directly],
    [*Domain method*], [`Order.Method.Checkout.cs`, `Order.Method.Cancel.cs`], [Checkout validates prerequisites before state transition],
    [*Handler validation*], [FluentValidation `Validator.cs`], [`CreateProduct.Validator.cs` ensures slug format, name length],
    [*Database constraint*], [EF Core configurations (unique indexes, check constraints)], [Composite index on `(UserId, Status)` for order queries],
  ),
  caption: [Business Rule Enforcement Levels],
)

*Evidence*: `Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`, `CreateProduct.Validator.cs`, `Shared/Operational/Persistence/Configurations/`

== Ubiquitous Language Glossary

The following terms have precise, domain-specific meanings within the ReSys.Shop codebase. They are used consistently across code, documentation, and API contracts.

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Term*], [*Context*], [*Definition*], [*Code Reference*],
    [*Aggregate*], [All], [A cluster of domain objects treated as a single unit for data changes; one root entity controls consistency], [`Shared/Application/Domain/Models/Entity.cs`],
    [*Product*], [Catalog], [A sellable item with name, description, slug, and fashion metadata; may have variants], [`Module/Catalog/Domain/Products/Product.cs`],
    [*Variant*], [Catalog], [A specific configuration of a product (e.g., Size M + Color Red); has SKU, price, dimensions], [`Module/Catalog/Domain/Products/Variants/Variant.cs`],
    [*Master Variant*], [Catalog], [The default variant of a product; all products have exactly one master variant], [`Product.cs:47`, `Variant.cs:17`],
    [*Taxonomy*], [Catalog], [A hierarchical classification system (e.g., "Clothing → Dresses → Evening Dresses")], [`Module/Catalog/Domain/Taxonomies/Taxonomy.cs`],
    [*Taxon*], [Catalog], [A node in a taxonomy tree; products are classified by association with taxons], [`Module/Catalog/Domain/Taxons/Taxon.cs`],
    [*Option Type*], [Catalog], [A configurable attribute (e.g., "Size", "Color") with predefined values], [`Module/Catalog/Domain/Products/Options/OptionType.cs`],
    [*Option Value*], [Catalog], [A specific value of an option type (e.g., "Small", "Red")], [`Module/Catalog/Domain/Products/Options/OptionValue.cs`],
    [*Order*], [Ordering], [A customer's purchase request; contains line items, totals, checkout state], [`Module/Ordering/Domain/Orders/Order.cs`],
    [*Line Item*], [Ordering], [A single item within an order (references a variant, quantity, price at time of purchase)], [`Module/Ordering/Domain/LineItems/LineItem.cs`],
    [*Checkout State*], [Ordering], [The stage of checkout: Address → Delivery → Payment → Confirm → Complete], [`Order.cs:20`, `Order.Constant.cs:50-56`],
    [*Cart*], [Ordering], [An implicit aggregate: an Order with `Status = Draft` and a `SessionId` (guest) or `UserId`], [`Order.cs:18` (session-based)],
    [*Payment Intent*], [Payment], [A record of intent to charge a customer; tracks state through Stripe or Bogus gateway], [`Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs`],
    [*Stock Location*], [Inventory], [A physical place where inventory is held (warehouse, store)], [`Module/Inventory/Domain/StockLocations/StockLocation.cs`],
    [*Stock Item*], [Inventory], [The quantity of a specific variant at a specific stock location], [`Module/Inventory/Domain/StockLocations/StockItems/StockItem.cs`],
    [*Stock Reservation*], [Inventory], [A temporary hold on inventory for an active cart or order], [`Module/Inventory/Domain/StockReservations/StockReservation.cs`],
    [*User Profile*], [Profile], [Extended user data (addresses, wishlist, notification preferences) separate from Identity], [`Module/Profile/Domain/UserProfile.cs`],
    [*Permission*], [Identity], [`{Domain}:{Category}:{Action}` (e.g., `catalog:products:create`)], [`Shared/Security/Authorization/Registry/PermissionContext.cs`],
    [*Vertical Slice*], [Architecture], [A feature implementation co-located in one folder: handler, endpoint, request, response, validator], [`Module/Catalog/Features/Admin/Products/Create/`],
    [*Result*], [Architecture], [A type-safe wrapper for operation outcomes: either a value (success) or a list of errors (failure)], [`Shared/Application/Models/Results/Result.cs`],
    [*Embedding*], [ML], [A 512-dimensional float vector representing the visual features of a product image (Fashion-CLIP output)], [`Vector.Configuration.cs`, `VariantImage.cs`],
    [*CBIR*], [ML], [Content-Based Image Retrieval: finding visually similar products using embedding similarity search], [Chapter 3, Chapter 7],
  ),
  caption: [Ubiquitous Language Glossary],
)

*Design rationale*: A Ubiquitous Language glossary prevents "translation friction" between domain experts, developers, and examiners. Every term above is reflected in the codebase as a class name, property, or enum value --- ensuring the language is executable, not just documentation.

*Evidence*: All terms traceable to the domain entity files listed in the Code Reference column.

== Evidence

#list(
  [`service/Api/src/Module/*/Domain/*/*.cs` --- all domain entities],
  [`service/Api/src/Shared/Application/Domain/Models/Entity.cs` --- base entity],
  [`service/Api/src/Shared/Application/Domain/Concerns/` --- cross-cutting domain interfaces],
  [`service/Api/src/Module/Ordering/Domain/Orders/Order.cs` --- aggregate root example],
  [`service/Api/src/Module/Catalog/Domain/Products/Product.cs` --- aggregate root with children],
  [`service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs` --- state machine],
  [`service/Api/src/Module/Inventory/Domain/StockLocations/StockItems/StockItem.Method.cs` --- domain method enforcing invariant],
)
