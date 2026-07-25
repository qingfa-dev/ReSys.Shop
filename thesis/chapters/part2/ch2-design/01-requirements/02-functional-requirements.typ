=== Functional Requirements

The functional capabilities of ReSys.Shop are organised around eight business modules, each responsible for a coherent subset of domain logic. This section describes each module in narrative prose; a summary table at the end of the section consolidates responsibilities and research classification.

==== Catalog Module

The Catalog module manages the product lifecycle: creating products with fashion-specific metadata, including style code, season, material, department, and gender target, defining sellable variants with SKUs, barcodes, and independent pricing, uploading product images with automatic thumbnail generation, and organising products through hierarchical taxonomies that allow browsing by category (e.g., Clothing → Dresses → Evening Dresses). It also hosts the Content-Based Image Retrieval (CBIR) infrastructure: newly uploaded variant images are sent to the Python machine learning sidecar for vectorisation, and the resulting embeddings are stored in PostgreSQL using the pgvector extension for similarity search @pgvector2023. The catalog supports configurable embedding models, allowing the system to switch between Fashion-CLIP, ResNet-50, and other architectures without application changes, a capability that enables the systematic benchmark evaluation presented in Chapter 3.

==== Ordering Module

The Ordering module handles the customer purchase workflow from cart to completed order. Both guest and authenticated users can add products to a cart, which automatically expires after seven days of inactivity to prevent indefinite stock reservation. Checkout proceeds through a forward-only state machine: the customer selects a shipping address, chooses a delivery method, provides payment details, reviews the order summary, and confirms. Once confirmed, the system creates an order record, reserves inventory quantities for each line item, processes the payment intent, and clears the cart. Orders track item totals, price adjustments, shipment costs, and payment state independently, enabling partial fulfilment scenarios. Cancellation is available at any pre-confirmation stage without penalty.

==== Payment Module

The Payment module manages the lifecycle of payment intents, the data structure representing a customer's intent to pay, including creation, capture, refund, and void operations. It supports two gateway providers: the Stripe gateway for production, which validates incoming webhooks using signature verification to prevent spoofed payment confirmations, and a Bogus gateway for development and testing, which simulates the payment lifecycle by automatically transitioning through states without external calls. Payment intents follow their own state machine, Pending, RequiresAction, Processing, and Succeeded or Canceled, and the system maintains its own copy of the payment state in parallel with the gateway's state, enabling offline operations and consistent behaviour across both gateway implementations.

==== Inventory Module

The Inventory module tracks physical stock quantities across warehouse locations, manages temporary reservations during active checkouts to prevent overselling, records stock movements for audit trails, and handles inter-warehouse transfers. Each stock item is associated with a specific product variant and a warehouse location, with quantities maintained as both on-hand (physical count) and reserved (held for active checkouts) values. The reservation mechanism ensures that a variant added to a cart remains visible to other customers as having limited availability but cannot be sold twice.

==== Identity Module

The Identity module provides JWT-based authentication with short-lived access tokens (fifteen-minute lifetime) and refresh token rotation with reuse detection: each refresh token is single-use, and presenting a previously consumed token triggers revocation of all tokens for that user to contain potential compromise. Guest sessions enable anonymous cart usage through cookie-based identifiers that persist across page navigations without requiring account creation. Role-based and permission-based authorisation segregates admin functions from customer-facing endpoints, with permission claims following a `domain:category:action` format that allows fine-grained access control.

==== Supporting Modules

Three additional modules provide complementary infrastructure. The *Profile* module manages user addresses, wishlists, and notification preferences, linking customer identity to personalisation features. The *Shipping* module configures delivery methods, standard, express, and local pickup, and calculates shipping rates by geographic zone. The *Location* module provides country and state reference data with ISO codes, shared across Shipping (for zone configuration) and Profile (for address validation).

==== Summary

Table @tbl-module-summary consolidates the eight business modules with their key responsibilities and research classification relative to this thesis.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    align: (left + horizon, left, center + horizon),

    table.header([*Module*], [*Key Responsibilities*], [*Research Classification*]),

    [Catalog], [
      Product and variant lifecycle management; fashion-specific metadata (style code, season, material, department); hierarchical taxonomies; image upload and management; CBIR infrastructure, embedding generation pipeline and pgvector vector search.
    ], [Core Research],

    [Ordering], [
      Shopping cart with auto-expiry; forward-only checkout state machine (Address → Delivery → Payment → Confirm → Complete); order lifecycle tracking; cancellation and partial fulfilment support.
    ], [Supporting],

    [Payment], [
      Payment intent lifecycle (create, capture, refund, void); Stripe gateway with webhook signature validation; Bogus test gateway; parallel state tracking for offline operations.
    ], [Supporting],

    [Inventory], [
      Per-warehouse stock tracking; checkout-time quantity reservation to prevent overselling; auditable stock movements; inter-warehouse transfers.
    ], [Supporting],

    [Identity], [
      JWT authentication with 15-minute access tokens; refresh token rotation and reuse detection; guest session support; role-based and permission-based authorisation.
    ], [Supporting],

    [Profile], [
      User addresses (shipping and billing); wishlist management; notification preferences.
    ], [Supporting],

    [Shipping], [
      Delivery method configuration; zone-based rate calculation.
    ], [Supporting],

    [Location], [
      Country and state reference data with ISO codes.
    ], [Supporting],
  ),
  caption: [
    Summary of business modules, their key responsibilities, and classification as Core Research or Supporting Infrastructure.
    Only Catalog is classified as Core Research because it hosts the CBIR capability that is the primary subject of evaluation.
  ],
) <tbl-module-summary>

The functional scope of ReSys.Shop extends far beyond the visual search capability at its core. The supporting modules, Ordering, Payment, Inventory, and Identity, provide a realistic e-commerce context in which the research contribution can be meaningfully evaluated. Without a functioning checkout flow, for example, the value of visual search could not be measured through downstream conversion events. Without inventory awareness, search results could include out-of-stock items, undermining the realism of the evaluation.
