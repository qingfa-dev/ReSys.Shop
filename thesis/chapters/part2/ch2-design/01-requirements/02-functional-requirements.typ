=== Functional Requirements

The platform's capabilities are specified as traceable requirements organised by business module. Each module is introduced with a single sentence of context; the table that follows enumerates specific requirements with identifiers that can be referenced throughout the design and evaluation chapters.

==== Catalog Module

The Catalog module manages the product lifecycle: creation, classification, image handling, and CBIR infrastructure.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [CAT-FR-01], [Create products with name, description, slug, SEO metadata, and fashion-specific fields (style code, season, material composition, department, gender target)], [High],
    [CAT-FR-02], [Define sellable variants (size and colour combinations) with SKU, barcode, dimensions, and independent pricing], [High],
    [CAT-FR-03], [Upload variant images with automatic thumbnail generation], [High],
    [CAT-FR-04], [Generate vector embeddings for all uploaded images using the configured ML model and store in pgvector], [High],
    [CAT-FR-05], [Search products by image via CBIR with configurable minimum similarity threshold and result count], [High],
    [CAT-FR-06], [Support configurable embedding models (Fashion-CLIP, ResNet-50, and others) via environment variable without code changes], [High],
    [CAT-FR-07], [Organise products via hierarchical taxonomies with taxon trees], [Medium],
    [CAT-FR-08], [Manage option types (e.g., Size, Colour) with option values], [Medium],
    [CAT-FR-09], [Enforce slug uniqueness across the catalog], [High],
    [CAT-FR-10], [Support product status lifecycle: Draft, Active, Archived with availability dates], [Medium],
  ),
  caption: [Catalog module functional requirements.],
)

==== Identity Module

The Identity module provides authentication, authorisation, and user management for both customer-facing and administrative functions.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [IDN-FR-01], [Register new customer accounts with email, password, and profile information], [High],
    [IDN-FR-02], [Authenticate users with JWT access tokens (15-minute lifetime) and refresh token rotation with reuse detection], [High],
    [IDN-FR-03], [Support guest sessions via cookie-based identifiers for anonymous cart usage across page navigations], [High],
    [IDN-FR-04], [Enforce role-based and permission-based authorisation with domain:category:action claims format], [High],
    [IDN-FR-05], [Reset passwords via time-limited email token], [Medium],
    [IDN-FR-06], [Manage users and roles through the admin interface], [Medium],
    [IDN-FR-07], [Revoke all refresh tokens for a user when a previously consumed token is presented (compromise containment)], [High],
  ),
  caption: [Identity module functional requirements.],
)

==== Inventory Module

The Inventory module tracks physical stock quantities, manages checkout-time reservations, and records stock movements for audit trails.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [INV-FR-01], [Define stock locations (warehouses) with address and active status], [Medium],
    [INV-FR-02], [Track stock quantities per product variant per location with on-hand and reserved values], [High],
    [INV-FR-03], [Reserve inventory during checkout and release on cart expiry or cancellation to prevent overselling], [High],
    [INV-FR-04], [Record stock transfers between locations with full auditable movement history], [Medium],
    [INV-FR-05], [Maintain immutable audit log for all stock level changes], [Medium],
  ),
  caption: [Inventory module functional requirements.],
)

==== Ordering Module

The Ordering module handles the customer purchase workflow from cart through checkout to completed order.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [ORD-FR-01], [Support guest and authenticated shopping carts with product variant selection and quantity management], [High],
    [ORD-FR-02], [Auto-expire abandoned carts after configurable inactivity period (default seven days)], [Medium],
    [ORD-FR-03], [Enforce forward-only checkout state machine: Address, Delivery, Payment, Confirm, Complete], [High],
    [ORD-FR-04], [Calculate order totals including item subtotals, price adjustments, shipment costs, and taxes], [High],
    [ORD-FR-05], [Track payment and shipment state independently per order to enable partial fulfilment], [Medium],
    [ORD-FR-06], [Allow order cancellation at any pre-confirmation stage without penalty], [Medium],
    [ORD-FR-07], [Generate unique human-readable order numbers upon confirmation], [Medium],
  ),
  caption: [Ordering module functional requirements.],
)

==== Payment Module

The Payment module manages the lifecycle of payment intents across the Stripe production gateway and a Bogus test gateway.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [PAY-FR-01], [Create payment intents with amount, currency, and order reference], [High],
    [PAY-FR-02], [Capture, void, and refund payment intents with idempotency keys to prevent duplicate processing], [High],
    [PAY-FR-03], [Process Stripe webhooks with cryptographic signature verification for production], [High],
    [PAY-FR-04], [Provide Bogus gateway for development and testing that simulates the full payment lifecycle without external calls], [Medium],
    [PAY-FR-05], [Maintain independent payment state tracking in parallel with the gateway for offline consistency], [Medium],
  ),
  caption: [Payment module functional requirements.],
)

==== Shipping Module

The Shipping module configures delivery methods and calculates shipping rates by geographic zone.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [SHP-FR-01], [Configure delivery methods (standard, express, local pickup) with associated pricing rules], [Medium],
    [SHP-FR-02], [Calculate shipping rates by geographic zone and selected delivery method at checkout], [Medium],
  ),
  caption: [Shipping module functional requirements.],
)

==== Profile Module

The Profile module links customer identity to personalisation features and preference management.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [PRF-FR-01], [Manage user shipping and billing addresses, wishlists, and notification preferences], [Medium],
    [PRF-FR-02], [Configure per-channel notification preferences (email, SMS, push)], [Low],
  ),
  caption: [Profile module functional requirements.],
)

==== Location Module

The Location module provides country and state reference data for address validation and shipping zone configuration.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [LOC-FR-01], [Provide country and state reference data with ISO 3166 codes for address validation and shipping zone calculation], [Medium],
  ),
  caption: [Location module functional requirements.],
)
