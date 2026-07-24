== Functional Requirements

Functional requirements are organized by business module. Each requirement is traceable to a vertical-slice feature folder or domain invariant in the codebase.

=== Catalog Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [CAT-FR-01], [Administrators can create products with name, description, slug, SEO metadata, and fashion-specific fields (style code, season, material composition, care instructions, fit notes, department, gender target)], [High], [`Module/Catalog/Domain/Products/Product.cs:17-43`],
    [CAT-FR-02], [Products support variants (size/color combinations) with SKUs, barcodes, physical dimensions, and independent pricing], [High], [`Module/Catalog/Domain/Products/Variants/Variant.cs:14-69`],
    [CAT-FR-03], [Products can be classified via taxonomy (hierarchical categories: taxonomies → taxons)], [High], [`Module/Catalog/Features/Admin/Taxonomies/`, `Module/Catalog/Features/Admin/Taxons/`],
    [CAT-FR-04], [Products support option types (e.g., "Size", "Color") with option values (e.g., "Small", "Red")], [High], [`Module/Catalog/Domain/Products/Options/OptionType.cs`, `OptionValue.cs`],
    [CAT-FR-05], [Variant images can be uploaded, stored (Local/S3), and associated with variants], [High], [`Module/Catalog/Features/Admin/Products/Variants/Images/Upload/`],
    [CAT-FR-06], [*Image embeddings are generated via a pluggable ML sidecar* supporting Fashion-CLIP, ResNet-50, EfficientNet-B0, and CLIP-generic; embeddings stored in PostgreSQL pgvector with model metadata], [High], [`Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/`, `Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs`],
    [CAT-FR-07], [Storefront users can search products by image (upload an image, find visually similar products)], [High], [`ApiTests/Catalog/Storefront/search-by-image.http`],
    [CAT-FR-10], [Embedding model is *configurable per deployment* via `EMBEDDING_MODEL` env var (e.g., `fashion-clip`, `resnet50`, `efficientnet_b0`, `clip`)], [High], [`service/Embedding/src/models/clip_model.py` (refactored to strategy pattern)],
    [CAT-FR-08], [Product status lifecycle: Draft → Active → Archived, with availability and discontinuation dates], [Medium], [`Module/Catalog/Domain/Products/Product.cs:20,31-33`],
    [CAT-FR-09], [Slug uniqueness enforced across the catalog], [High], [`Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:41-43`],
  ),
  caption: [Catalog Module Functional Requirements],
)

=== Identity Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [ID-FR-01], [Users can register with email and password], [High], [`Module/Identity/Features/Store/Auth/Register/`],
    [ID-FR-02], [Users can log in with email/password or Google OAuth], [High], [`Module/Identity/Features/Store/Auth/Login/Password/`, `Shared/Security/Authentication/External/`],
    [ID-FR-03], [JWT access tokens (15-minute expiry) with refresh-token rotation and reuse detection], [High], [`appsettings.json:30-43`, `Shared/Security/Authentication/Tokens/Services/Refresh/`],
    [ID-FR-04], [Guest sessions via cookie for anonymous cart usage], [High], [`appsettings.json:88-94`, `Module/Ordering/Features/Storefront/Cart/AssociateCart/`],
    [ID-FR-05], [Role-based and permission-based authorization; admin vs storefront surface segregation], [High], [`Shared/Security/Authorization/Registry/PermissionContext.cs`, `Shared/Security/Authorization/Attributes/HasPermission.Attribute.Extension.cs`],
    [ID-FR-06], [Password reset via email token], [Medium], [`Module/Identity/Features/Store/Passwords/`],
    [ID-FR-07], [Administrators can manage users, roles, and permissions], [High], [`Module/Identity/Features/Admin/Users/`, `Module/Identity/Features/Admin/Roles/`, `Module/Identity/Features/Admin/Permissions/`],
  ),
  caption: [Identity Module Functional Requirements],
)

=== Inventory Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [INV-FR-01], [Stock locations (warehouses) can be created and managed], [High], [`Module/Inventory/Domain/StockLocations/StockLocation.cs`],
    [INV-FR-02], [Stock items track quantity on hand per variant per location], [High], [`Module/Inventory/Domain/StockLocations/StockItems/StockItem.cs`],
    [INV-FR-03], [Stock reservations hold inventory for active carts/orders], [High], [`Module/Inventory/Domain/StockReservations/StockReservation.cs`],
    [INV-FR-04], [Stock transfers move inventory between locations], [Medium], [`Module/Inventory/Domain/StockTransfers/StockTransfer.cs`],
    [INV-FR-05], [Stock movements (adjustments) are auditable], [Medium], [`Module/Inventory/Domain/StockLocations/StockItems/StockMovements/StockMovement.cs`],
  ),
  caption: [Inventory Module Functional Requirements],
)

=== Ordering Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [ORD-FR-01], [Guest and authenticated users can add items to a cart], [High], [`Module/Ordering/Features/Storefront/Cart/AddItem/`],
    [ORD-FR-02], [Carts auto-expire after 7 days of inactivity (Hangfire background job)], [Medium], [`appsettings.json:181-183`, `Module/Ordering/Backgrounds/CartExpiryJob.cs`],
    [ORD-FR-03], [Checkout proceeds through states: Address → Delivery → Payment → Confirm → Complete], [High], [`Module/Ordering/Domain/Orders/Order.cs:20`, `Order.Constant.cs:50-56`],
    [ORD-FR-04], [Orders calculate totals: ItemTotal + AdjustmentTotal + ShipmentTotal = Total], [High], [`Module/Ordering/Domain/Orders/Order.cs:22-25`, invariant comment line 12],
    [ORD-FR-05], [Orders track payment state and shipment state independently], [High], [`Module/Ordering/Domain/Orders/Order.cs:28-29`],
    [ORD-FR-06], [Orders can be canceled (with reason: customer or admin)], [High], [`Module/Ordering/Features/Storefront/Orders/Cancel/`, `Module/Ordering/Features/Admin/Orders/Cancel/`],
    [ORD-FR-07], [Guest carts can be associated with a user upon login/registration], [High], [`Module/Ordering/Features/Storefront/Cart/AssociateCart/`],
    [ORD-FR-08], [Order number generation inside database transaction with RepeatableRead isolation], [High], [`git log: commit 887a77c7`],
  ),
  caption: [Ordering Module Functional Requirements],
)

=== Payment Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [PAY-FR-01], [Payment intents can be created for an order], [High], [`Module/Payment/Features/Storefront/Payment/CreateIntent/`],
    [PAY-FR-02], [Payment intents can be confirmed (capture funds)], [High], [`Module/Payment/Features/Storefront/Payment/Confirm/`],
    [PAY-FR-03], [Administrators can capture, void, and refund payments], [High], [`Module/Payment/Features/Admin/Payments/Capture/`, `Void/`, `Refund/`],
    [PAY-FR-04], [Stripe webhook handling with signature validation], [High], [`Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:32-36`],
    [PAY-FR-05], [Bogus gateway for development/testing], [High], [`Module/Payment/Services/Provider/Bogus/BogusGateway.cs`],
  ),
  caption: [Payment Module Functional Requirements],
)

=== Shipping Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [SHIP-FR-01], [Shipping methods can be configured (name, carrier, zones)], [High], [`Module/Shipping/Domain/ShippingMethods/ShippingMethod.cs`],
    [SHIP-FR-02], [Shipping rates calculated based on address and method], [High], [`Module/Shipping/Domain/ShippingRates/ShippingRate.cs`, `Calculators/ShippingRateCalculator.cs`],
  ),
  caption: [Shipping Module Functional Requirements],
)

=== Profile Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [PROF-FR-01], [Users manage profile, addresses, wishlists], [High], [`Module/Profile/Domain/UserProfile.cs`, `Addresses/Address.cs`, `Wishlists/Wishlist.cs`],
    [PROF-FR-02], [Notification preferences per channel (email, SMS)], [Medium], [`Module/Profile/Domain/Notifications/NotificationPreferences.cs`],
  ),
  caption: [Profile Module Functional Requirements],
)

=== Location Module

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Priority*], [*Evidence*],
    [LOC-FR-01], [Countries and states/provinces with ISO codes], [High], [`Module/Location/Features/Admin/Countries/`, `Module/Location/Features/Admin/States/`],
  ),
  caption: [Location Module Functional Requirements],
)

== Non-Functional Requirements

#figure(
  table(
    columns: (auto, 1fr, auto, 1fr),
    align: (start, start, start, start),
    [*ID*], [*Requirement*], [*Target*], [*Evidence*],
    [NFR-01], [*Modularity* — Modules must have zero direct cross-references], [Compile-time isolation], [`AGENTS.md:10`, `Directory.Build.targets:42-53`],
    [NFR-02], [*Explicit error handling* — No exceptions for control flow; all failures return `Result<T>`], [100% of handlers], [`AGENTS.md:10`, `Result.cs:1-43`],
    [NFR-03], [*Build strictness* — Warnings treated as errors], [Zero warnings], [`Directory.Build.props:17`],
    [NFR-04], [*Testability* — Unit tests runnable without Docker; integration tests with Docker], [xUnit v3, Testcontainers], [`Directory.Packages.props:102-112`],
    [NFR-05], [*Observability* — OpenTelemetry traces, metrics, logs; correlation IDs], [OTLP export optional], [`infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:58-103`],
    [NFR-06], [*Rate limiting* — Named policies per endpoint category], [auth: 5/min, register: 3/hr, payment: 30/min], [`appsettings.json:79-86`],
    [NFR-07], [*Security headers* — CSP, HSTS, X-Frame-Options, etc.], [All responses], [`Shared/Security/Headers/SecurityHeadersMiddleware.cs`],
    [NFR-08], [*File upload security* — Magic-byte validation, extension allowlist, size limit, anti-forgery guard], [Max 10 MB; 15 blocked extensions], [`appsettings.json:129-155`],
    [NFR-09], [*Caching* — Multi-tier (memory L1 + Redis L2) with HybridCache], [5-minute default], [`appsettings.json:104-122`],
    [NFR-10], [*Background job reliability* — Hangfire with Redis or in-memory], [Jobs survive restart (Redis)], [`Shared/Operational/Backgrounds/Background.Extension.cs:54-80`],
  ),
  caption: [Non-Functional Requirements],
)

== Business Rules

Business rules are encoded as domain invariants (comments on entity classes), validation rules (FluentValidation), and domain methods (factory methods, state transitions).

#figure(
  table(
    columns: (1fr, auto, 1fr, 1fr),
    align: (start, start, start, start),
    [*Rule*], [*Type*], [*Enforcement*], [*Evidence*],
    [Product slug must be unique], [Invariant], [EF query in `CreateProduct` handler + db unique index], [`CreateProduct.cs:41-43`],
    [At least one `OptionType` if `Product.HasVariants`], [Invariant], [Domain check], [`Product.cs` invariant comment line 14],
    [Master variant must exist per product], [Invariant], [`CreateProduct` dispatches `AddVariant` before finalizing], [`CreateProduct.cs:64-65`],
    [Order total = ItemTotal + AdjustmentTotal + ShipmentTotal], [Invariant], [Domain property calculation], [`Order.cs:22-25`, comment line 12],
    [Finalized orders are immutable except Cancel], [Invariant], [`Order` state machine], [`Order.cs` comment line 12],
    [Cart expires after 7 days], [Policy], [Hangfire `CartExpiryJob`], [`CartExpiryJob.cs`, `appsettings.json:181-183`],
    [Refresh token reuse triggers blacklist], [Security], [`ReuseDetectionEnabled=true`], [`appsettings.json:40`],
    [Payment signature must be validated before processing], [Security], [Stripe webhook handler], [`StripeWebhook.cs:32-36`],
  ),
  caption: [Business Rules],
)

== Use Cases

=== Use Case: Search by Image (CBIR)

*Actor*: Storefront Customer \
*Precondition*: Product catalog contains images with embeddings (generated by currently configured model) \
*Flow*:
#enum(
  [Customer uploads an image via Storefront SPA],
  [Storefront POSTs to `/api/catalog/storefront/search-by-image`],
  [Backend forwards image bytes to Python embedding sidecar (`/embeddings`)],
  [Sidecar loads the configured embedding model (e.g., Fashion-CLIP, ResNet-50) and generates a vector],
  [Backend queries PostgreSQL pgvector with cosine similarity: `SELECT * FROM variant_images ORDER BY embedding <=> $1 LIMIT 20` (embeddings filtered by `model_name`)],
  [Results mapped to DTOs and returned],
)
*Postcondition*: Customer receives visually similar products

*Evidence*: `ApiTests/Catalog/Storefront/search-by-image.http`, `ImageEmbedding.Inference.cs:21-36`, `Vector.Configuration.cs`

=== Use Case: Model Comparison Evaluation (Research)

*Actor*: System / Researcher \
*Precondition*: Ground-truth dataset of 100 fashion images with human-labeled similarity groups exists \
*Flow*:
#enum(
  [Configure sidecar with Model A (e.g., Fashion-CLIP)],
  [Generate embeddings for all 100 query images and all catalog images],
  [Execute top-20 similarity search for each query image],
  [Record Precision@20, Recall@20, embedding generation time per image, total storage],
  [Repeat steps 1–4 for Model B (ResNet-50), Model C (EfficientNet-B0), Model D (CLIP-generic)],
  [Compute mean ± SD across all queries per model],
  [Generate comparison report: retrieval metrics vs. operational metrics trade-off],
)
*Postcondition*: Identification of optimal model for deployment based on empirical evidence

*Evidence*: `11-evaluation.md:§11.5`

=== Use Case: Checkout

*Actor*: Authenticated or Guest Customer \
*Precondition*: Cart contains items; stock is available \
*Flow*:
#enum(
  [Customer sets address → `CheckoutState = Address`],
  [Customer selects shipping method → `CheckoutState = Delivery`],
  [Customer selects payment method → `CheckoutState = Payment`],
  [Backend creates payment intent (Stripe or Bogus)],
  [Customer confirms → `CheckoutState = Confirm`],
  [Backend finalizes: creates order, deducts stock, clears cart → `CheckoutState = Complete`],
)
*Postcondition*: Order created with `Status = Draft` (or `Pending`); payment intent linked

*Evidence*: `Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`, `Order.Constant.cs:50-56`

== User Roles

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Role*], [*Permissions*], [*Surfaces*],
    [*Guest*], [Browse catalog, add to cart, search by image, checkout], [Storefront],
    [*Customer*], [All guest actions + profile, addresses, wishlists, order history], [Storefront],
    [*Admin*], [Full CRUD on all modules, user management, payment captures/refunds, shipping configuration], [Admin SPA],
    [*System*], [Background jobs, webhook handlers, cart expiry], [Backend only],
  ),
  caption: [User Roles and Permissions],
)

== Evidence

#list(
  [`service/Api/src/Api/appsettings.json:1-237` — runtime configuration documenting all functional areas],
  [`service/Api/src/Module/*/Domain/*/*.cs` — domain entities with invariant comments],
  [`ApiTests/` — 49 `.http` files documenting all endpoints (use case realization)],
  [`service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs:1-60` — permission enumeration per role],
  [`service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78` — concrete feature implementation],
  [`service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — checkout flow],
)
