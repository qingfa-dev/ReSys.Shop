=== Functional Requirements

The platform's capabilities are specified as traceable requirements organised by business module. Each module is introduced with a single sentence of context; the table that follows enumerates specific requirements with identifiers that can be referenced throughout the design and evaluation chapters.

==== Catalog Module

The Catalog module manages the product lifecycle, from creation and classification through image handling to CBIR infrastructure.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [CAT-FR-01], [Create products with name, description, slug, and SEO metadata (meta title, meta description)], [High],
    [CAT-FR-02], [Assign fashion-specific fields: style code, season, material composition, care instructions, fit notes, department, and gender target], [Medium],
    [CAT-FR-03], [Define sellable variants combining option values (e.g., Size M + Colour Red) with SKU, barcode, physical dimensions, weight, and independent pricing], [High],
    [CAT-FR-04], [Support variant images with upload, storage, and automatic thumbnail generation], [High],
    [CAT-FR-05], [Generate vector embeddings for each uploaded variant image via the configured ML model and store in pgvector with model metadata (model name, version, dimension)], [High],
    [CAT-FR-06], [Enable image-based product search via CBIR: upload query image, generate embedding, query pgvector with cosine distance, return similarity-ranked results], [High],
    [CAT-FR-07], [Configure minimum similarity threshold and maximum result count for CBIR queries], [Medium],
    [CAT-FR-08], [Switch embedding model via environment variable without code changes; filter search results by active model], [High],
    [CAT-FR-09], [Organise products via hierarchical taxonomies with nested taxon trees (e.g., Clothing → Dresses → Evening Dresses)], [Medium],
    [CAT-FR-10], [Define option types (e.g., Size, Colour, Material) with ordered option values usable across multiple products], [Medium],
    [CAT-FR-11], [Enforce slug uniqueness across the entire catalog], [High],
    [CAT-FR-12], [Support product status lifecycle: Draft, Active, Archived with configurable availability and discontinuation dates], [Medium],
    [CAT-FR-13], [Assign one variant per product as the master (default) variant], [High],
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
    [IDN-FR-02], [Authenticate users via email and password, returning JWT access token (15-minute lifetime) and refresh token], [High],
    [IDN-FR-03], [Authenticate users via Google OAuth 2.0 as an alternative to password-based login], [Medium],
    [IDN-FR-04], [Implement refresh token rotation: each refresh operation invalidates the previous token and issues a new pair], [High],
    [IDN-FR-05], [Detect refresh token reuse: presenting a previously consumed token revokes all active tokens for that user], [High],
    [IDN-FR-06], [Support guest sessions via signed cookie-based identifiers, enabling anonymous cart usage and catalog browsing without registration], [High],
    [IDN-FR-07], [Enforce role-based access control (Customer, Administrator) with permission granularity at domain:category:action level], [High],
    [IDN-FR-08], [Reset passwords via time-limited, single-use email token], [Medium],
    [IDN-FR-09], [Manage users, roles, and permissions through the administration interface], [Medium],
    [IDN-FR-10], [Support optional two-factor authentication via time-based one-time password (TOTP)], [Low],
  ),
  caption: [Identity module functional requirements.],
)

==== Inventory Module

The Inventory module tracks physical stock quantities, manages checkout-time reservations to prevent overselling, and records stock movements for audit trails.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [INV-FR-01], [Define stock locations (warehouses or stores) with name, address, and active status flag], [Medium],
    [INV-FR-02], [Track stock quantities per product variant per location with separate on-hand and reserved counters], [High],
    [INV-FR-03], [Reserve inventory quantities during active checkout sessions and release on cart expiry, order cancellation, or checkout timeout], [High],
    [INV-FR-04], [Prevent overselling: reject checkout when requested quantity exceeds available (on-hand minus reserved) stock], [High],
    [INV-FR-05], [Record stock transfers between locations with source, destination, quantity, and timestamp metadata], [Medium],
    [INV-FR-06], [Maintain an immutable audit log for all stock level changes including manual adjustments], [Medium],
    [INV-FR-07], [Expire stale reservations after a configurable time window (default 15 minutes of checkout inactivity)], [Medium],
  ),
  caption: [Inventory module functional requirements.],
)

==== Ordering Module

The Ordering module handles the customer purchase workflow from cart through multi-step checkout to completed order, tracking payment and shipment state independently.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [ORD-FR-01], [Support guest and authenticated shopping carts with product variant selection, quantity management, and per-item pricing], [High],
    [ORD-FR-02], [Associate a guest cart with a user account upon login or registration, merging contents without data loss], [Medium],
    [ORD-FR-03], [Auto-expire abandoned carts after seven days of inactivity via scheduled background job], [Medium],
    [ORD-FR-04], [Enforce forward-only checkout state machine: Address, Delivery, Payment, Confirm, Complete], [High],
    [ORD-FR-05], [Calculate item total, adjustment total (discounts, surcharges), and shipment total independently; derive order total as their sum], [High],
    [ORD-FR-06], [Track payment state (unpaid, authorised, captured, refunded) and shipment state (pending, shipped, delivered) independently per order], [Medium],
    [ORD-FR-07], [Allow order cancellation at any pre-confirmation stage; cancelled orders release reserved inventory and void payment intents], [Medium],
    [ORD-FR-08], [Generate unique, sequential, human-readable order numbers within a database transaction to prevent collisions], [Medium],
    [ORD-FR-09], [Mark completed orders as immutable; prevent modification of line items, adjustments, or totals after finalisation], [High],
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
    [PAY-FR-01], [Create payment intents with amount, currency, order identifier, and gateway selection], [High],
    [PAY-FR-02], [Confirm payment intents to authorise fund capture at checkout completion], [High],
    [PAY-FR-03], [Capture, void, and refund payment intents with idempotency keys to prevent duplicate processing on retry], [High],
    [PAY-FR-04], [Process Stripe webhooks with cryptographic signature verification to confirm payment state transitions], [High],
    [PAY-FR-05], [Validate that refund amount does not exceed captured amount before processing], [Medium],
    [PAY-FR-06], [Provide a Bogus gateway for development and testing that simulates the full payment lifecycle (Pending, Processing, Succeeded, Canceled) without external API calls], [Medium],
    [PAY-FR-07], [Maintain independent payment state tracking in parallel with the gateway; enable offline state queries without gateway round-trips], [Medium],
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
    [SHP-FR-01], [Configure delivery methods (standard, express, local pickup) with carrier name, pricing rules, and applicable geographic zones], [Medium],
    [SHP-FR-02], [Calculate shipping rates at checkout based on delivery address zone, selected method, cart weight, and cart value], [Medium],
    [SHP-FR-03], [Assign per-order shipment tracking with carrier identifier and tracking number], [Low],
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
    [PRF-FR-01], [Manage user shipping and billing addresses with address type labels and default selection], [Medium],
    [PRF-FR-02], [Manage wishlists: create, rename, delete lists; add and remove product variants], [Low],
    [PRF-FR-03], [Configure per-channel notification preferences (email, SMS) with opt-in and opt-out per notification category], [Low],
  ),
  caption: [Profile module functional requirements.],
)

==== Location Module

The Location module provides country and state reference data for address validation, shipping zone configuration, and tax calculation.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Priority*]),
    [LOC-FR-01], [Provide country reference data with ISO 3166-1 codes, display names, and active status flags], [Medium],
    [LOC-FR-02], [Provide state and province reference data with ISO 3166-2 codes, linked to parent country], [Medium],
  ),
  caption: [Location module functional requirements.],
)