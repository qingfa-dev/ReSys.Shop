=== Functional Requirements

The platform's capabilities are specified as traceable requirements organised by business module. Each module is introduced with a single sentence of context; the table that follows enumerates specific requirements with identifiers that can be referenced throughout the design and evaluation chapters.

==== Catalog Module

The Catalog module manages the product lifecycle, from creation and classification through image handling to CBIR infrastructure.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [CAT-FR-01], [Product creation], [Create products with name, description, slug, and SEO metadata (meta title, meta description)], [High],
    [CAT-FR-02], [Fashion metadata], [Assign fashion-specific fields: style code, season, material composition, care instructions, fit notes, department, and gender target], [Medium],
    [CAT-FR-03], [Product variants], [Define sellable variants combining option values (e.g., Size M + Colour Red) with SKU, barcode, physical dimensions, weight, and independent pricing], [High],
    [CAT-FR-21], [Variant option values], [Assign, synchronise, retrieve, and revoke option values on variants to define the specific attribute combination each variant represents], [High],
    [CAT-FR-22], [Variant pricing], [Set, list, synchronise, and remove time-bound prices per variant with currency specification; support multi-currency pricing for international catalogues], [Medium],
    [CAT-FR-04], [Variant images], [Upload variant images with automatic thumbnail generation; support multiple images per variant with display ordering], [High],
    [CAT-FR-14], [Variant image CRUD], [Delete, download image by ID, list images by variant, and update image metadata (alt text, display order) for variant images], [Medium],
    [CAT-FR-15], [Embedding regeneration], [Regenerate vector embeddings for existing images when the configured model changes or the original embedding is corrupted], [Medium],
    [CAT-FR-05], [Embedding generation], [Generate vector embeddings for each uploaded variant image via the configured ML model and store in pgvector with model metadata (model name, version, dimension)], [High],
    [CAT-FR-06], [Image-based search], [Enable CBIR: upload query image, generate embedding, query pgvector with cosine distance, return similarity-ranked results], [High],
    [CAT-FR-16], [Product availability], [Expose real-time per-variant stock availability through the storefront product detail endpoint], [High],
    [CAT-FR-17], [Similar products], [Retrieve visually similar products via embedding similarity for a given product, enabling recommendation on product detail pages], [Medium],
    [CAT-FR-07], [Search configuration], [Configure minimum similarity threshold and maximum result count for CBIR queries], [Medium],
    [CAT-FR-08], [Pluggable models], [Switch embedding model via environment variable without code changes; filter search results by active model], [High],
    [CAT-FR-09], [Taxonomy], [Organise products via hierarchical taxonomies with nested taxon trees (e.g., Clothing → Dresses → Evening Dresses)], [Medium],
    [CAT-FR-18], [Taxon rules], [Define business rules attached to taxon nodes (attribute constraints, automatic assignments) with update, sync, and retrieval operations], [Low],
    [CAT-FR-19], [Auto-classification], [Automatically classify products into appropriate taxons based on taxon rules, invoked when product attributes are updated], [Low],
    [CAT-FR-10], [Option types], [Define option types (e.g., Size, Colour, Material) with ordered option values reusable across multiple products], [Medium],
    [CAT-FR-20], [Product option type association], [Assign, synchronise, retrieve, and revoke option types per product, enabling products to expose relevant variant dimensions], [Medium],
    [CAT-FR-11], [Slug uniqueness], [Enforce slug uniqueness across the entire catalog at the database level], [High],
    [CAT-FR-12], [Status lifecycle], [Support product status lifecycle: Draft, Active, Archived with configurable availability and discontinuation dates], [Medium],
    [CAT-FR-13], [Master variant], [Designate one variant per product as the master (default) variant used for catalog listing display], [High],
  ),
  caption: [Catalog module functional requirements.],
)

==== Identity Module

The Identity module provides authentication, authorisation, and user management for both customer-facing and administrative functions.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [IDN-FR-01], [Registration], [Register new customer accounts with email, password, and basic profile information], [High],
    [IDN-FR-02], [Password login], [Authenticate users via email and password, returning JWT access token (15-minute lifetime) and refresh token], [High],
    [IDN-FR-03], [OAuth login], [Authenticate users via Google OAuth 2.0 as an alternative to password-based login], [Medium],
    [IDN-FR-04], [Token rotation], [Implement refresh token rotation: each refresh operation invalidates the previous token and issues a new access and refresh token pair], [High],
    [IDN-FR-05], [Reuse detection], [Detect refresh token reuse: presenting a previously consumed token revokes all active tokens for that user, containing credential theft], [High],
    [IDN-FR-06], [Guest sessions], [Support guest sessions via signed cookie-based identifiers, enabling anonymous cart usage and catalog browsing without registration], [High],
    [IDN-FR-07], [Role-based access], [Enforce role-based access control (Customer, Administrator) with permission granularity at domain:category:action level], [High],
    [IDN-FR-11], [Role management], [Create, update, delete, and list roles with paging; assign, synchronise, retrieve, and revoke permissions per role], [Medium],
    [IDN-FR-12], [User-role assignment], [Assign, synchronise, retrieve, and revoke roles for individual users; support direct permission grants bypassing role inheritance], [Medium],
    [IDN-FR-13], [User status management], [Enable and disable user accounts via the admin interface without deleting the account or its associated data], [Medium],
    [IDN-FR-08], [Password reset], [Reset passwords via time-limited, single-use email token with configurable expiry], [Medium],
    [IDN-FR-14], [Password change], [Allow authenticated users to change their password while logged in, requiring current password verification], [Medium],
    [IDN-FR-15], [Email lifecycle], [Confirm email addresses via verification token, change email with re-verification, and resend verification emails on demand], [Medium],
    [IDN-FR-16], [Session management], [Retrieve current session details, refresh tokens, and explicitly terminate sessions via logout], [High],
    [IDN-FR-09], [User management], [Manage user accounts, role assignments, and permission grants through the administration interface], [Medium],
    [IDN-FR-10], [Two-factor auth], [Support optional two-factor authentication via time-based one-time password (TOTP)], [Low],
  ),
  caption: [Identity module functional requirements.],
)

==== Inventory Module

The Inventory module tracks physical stock quantities, manages checkout-time reservations to prevent overselling, and records stock movements for audit trails.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [INV-FR-01], [Stock locations], [Define stock locations (warehouses or stores) with name, address, and active status flag], [Medium],
    [INV-FR-02], [Stock quantities], [Track stock quantities per product variant per location with separate on-hand and reserved counters], [High],
    [INV-FR-08], [Stock item CRUD], [Create, update, delete, retrieve by ID, and list all stock items with paging; support bulk adjustment across multiple items and CSV-based import for batch operations], [Medium],
    [INV-FR-09], [Low stock alerting], [Identify and list stock items where on-hand quantity falls below a configured threshold, enabling proactive replenishment], [Low],
    [INV-FR-03], [Checkout reservation], [Reserve inventory quantities during active checkout sessions; release on cart expiry, order cancellation, or checkout timeout], [High],
    [INV-FR-11], [Cart-based reservation], [Reserve stock at the cart level during active shopping sessions, retrieve reservation status, and release on cart abandonment or expiry], [High],
    [INV-FR-04], [Overselling prevention], [Reject checkout when requested quantity exceeds available stock (on-hand minus reserved) at time of order confirmation], [High],
    [INV-FR-05], [Stock transfers], [Record stock transfers between locations with source, destination, quantity, and timestamp metadata], [Medium],
    [INV-FR-10], [Transfer lifecycle], [Manage the full stock transfer lifecycle: create a transfer, record in-transit status, confirm receipt at the destination, and cancel pending transfers; list transfers with paging and detail view], [Medium],
    [INV-FR-06], [Audit log], [Maintain an immutable audit log for all stock level changes including manual adjustments with reason and operator identity], [Medium],
    [INV-FR-12], [Movement audit trail], [Provide a dedicated paged listing and detail view for all stock movements with source, destination, quantity, reason, and operator identity; support the stock movement entity as a first-class data model with query capabilities], [Medium],
    [INV-FR-07], [Reservation expiry], [Expire stale reservations after a configurable time window (default 15 minutes of checkout inactivity)], [Medium],
  ),
  caption: [Inventory module functional requirements.],
)

==== Ordering Module

The Ordering module handles the customer purchase workflow from cart through multi-step checkout to completed order, tracking payment and shipment state independently.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [ORD-FR-01], [Shopping cart], [Support guest and authenticated shopping carts with product variant selection, quantity management, and per-item pricing], [High],
    [ORD-FR-10], [Cart management], [Create, delete, and empty shopping carts; update item quantities and remove specific items from the cart], [High],
    [ORD-FR-02], [Cart association], [Associate a guest cart with a user account upon login or registration, merging contents without data loss], [Medium],
    [ORD-FR-03], [Cart expiry], [Auto-expire abandoned carts after seven days of inactivity via scheduled background job], [Medium],
    [ORD-FR-04], [Checkout states], [Enforce forward-only checkout state machine: Address, Delivery, Payment, Confirm, Complete], [High],
    [ORD-FR-11], [Checkout validation], [Validate checkout constraints (stock availability, address completeness, shipping method selection) before proceeding to payment], [High],
    [ORD-FR-12], [Shipping rate selection], [Select a shipping rate from available options within the cart before proceeding to payment], [Medium],
    [ORD-FR-05], [Order totals], [Calculate item total, adjustment total (discounts, surcharges), and shipment total independently; derive order total as their sum], [High],
    [ORD-FR-06], [Dual state tracking], [Track payment state (unpaid, authorised, captured, refunded) and shipment state (pending, shipped, delivered) independently per order], [Medium],
    [ORD-FR-07], [Cancellation], [Allow order cancellation at any pre-confirmation stage; cancelled orders release reserved inventory and void payment intents], [Medium],
    [ORD-FR-13], [Admin order lifecycle], [Approve, complete, and resume orders through the admin interface; update order status, shipping method, and shipping/billing addresses], [Medium],
    [ORD-FR-14], [Customer order history], [Allow authenticated customers to list their orders with paging and view individual order detail including line items, payments, and shipment tracking], [Medium],
    [ORD-FR-08], [Order numbering], [Generate unique, sequential, human-readable order numbers within a database transaction to prevent collisions under concurrent requests], [Medium],
    [ORD-FR-09], [Order immutability], [Mark completed orders as immutable; prevent modification of line items, adjustments, or totals after finalisation], [High],
  ),
  caption: [Ordering module functional requirements.],
)

==== Payment Module

The Payment module manages the lifecycle of payment intents across the Stripe production gateway and a Bogus test gateway.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [PAY-FR-01], [Intent creation], [Create payment intents with amount, currency, order identifier, and gateway selection], [High],
    [PAY-FR-02], [Intent confirmation], [Confirm payment intents to authorise fund capture at checkout completion], [High],
    [PAY-FR-08], [Setup intent], [Create Stripe SetupIntents for saving customer payment methods for future use, separate from transactional PaymentIntents], [Low],
    [PAY-FR-03], [Capture and refund], [Capture, void, and refund payment intents with idempotency keys to prevent duplicate processing on retry], [High],
    [PAY-FR-04], [Webhook processing], [Process Stripe webhooks with cryptographic signature verification to confirm payment state transitions], [High],
    [PAY-FR-09], [Payment void], [Void authorised but un-captured payments as a distinct operation from refund; void all payments associated with a cancelled order via a cross-cutting command], [Medium],
    [PAY-FR-10], [Payment method management], [Create, update, delete, activate, and deactivate payment methods through the admin interface with paged listing; expose active methods to the storefront for customer selection], [Medium],
    [PAY-FR-05], [Refund validation], [Validate that refund amount does not exceed captured amount before processing the refund], [Medium],
    [PAY-FR-06], [Bogus gateway], [Provide a Bogus gateway for development and testing that simulates the full payment lifecycle (Pending, Processing, Succeeded, Canceled) without external API calls], [Medium],
    [PAY-FR-07], [State tracking], [Maintain independent payment state tracking in parallel with the gateway; enable offline state queries without gateway round-trips], [Medium],
  ),
  caption: [Payment module functional requirements.],
)

==== Shipping Module

The Shipping module configures delivery methods and calculates shipping rates by geographic zone.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [SHP-FR-01], [Delivery methods], [Configure delivery methods (standard, express, local pickup) with carrier name, pricing rules, and applicable geographic zones], [Medium],
    [SHP-FR-04], [Method lifecycle], [Create, update, delete, activate, and deactivate shipping methods with paged listing and detail retrieval through the admin interface], [Medium],
    [SHP-FR-05], [Rate lifecycle], [Create, update, delete, and list shipping rates per method, zone, and weight/value tier through the admin interface], [Medium],
    [SHP-FR-02], [Rate calculation], [Calculate shipping rates at checkout based on delivery address zone, selected method, cart weight, and cart value], [Medium],
    [SHP-FR-06], [Storefront calculation], [Calculate shipping cost for a given cart at checkout, returning available methods with rates applicable to the delivery address zone], [High],
    [SHP-FR-03], [Shipment tracking], [Assign per-order shipment tracking with carrier identifier and tracking number for customer visibility], [Low],
  ),
  caption: [Shipping module functional requirements.],
)

==== Profile Module

The Profile module links customer identity to personalisation features and preference management.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [PRF-FR-01], [Addresses], [Manage user shipping and billing addresses with address type labels and default selection per type], [Medium],
    [PRF-FR-02], [Wishlists], [Manage wishlists: create, rename, delete lists; add and remove product variants with notes], [Low],
    [PRF-FR-03], [Notifications], [Configure per-channel notification preferences (email, SMS) with opt-in and opt-out per notification category], [Low],
  ),
  caption: [Profile module functional requirements.],
)

==== Location Module

The Location module provides country and state reference data for address validation, shipping zone configuration, and tax calculation.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [LOC-FR-01], [Countries], [Provide country reference data with ISO 3166-1 codes, display names, and active status flags], [Medium],
    [LOC-FR-02], [States], [Provide state and province reference data with ISO 3166-2 codes, linked to parent country], [Medium],
    [LOC-FR-03], [Country CRUD], [Create, update, delete, and list countries with paging; retrieve by ID and ISO code through both admin and storefront interfaces], [Medium],
    [LOC-FR-04], [State CRUD], [Create, update, delete, and list states with paging; retrieve by ID and ISO code; filter by parent country through both admin and storefront interfaces], [Medium],
  ),
  caption: [Location module functional requirements.],
)

==== Dashboard Module

The Dashboard module aggregates key performance indicators from all business modules into a unified administrative overview.

#figure(
  table(
    columns: (auto, auto, 1fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [DSH-FR-01], [Aggregate dashboard], [Provide a cross-module administrative dashboard aggregating key metrics (product counts, order volumes, inventory levels, user statistics) from all business modules into a single view], [Medium],
  ),
  caption: [Dashboard module functional requirements.],
)
