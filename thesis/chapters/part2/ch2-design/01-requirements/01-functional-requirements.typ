=== Functional Requirements

The system's capabilities are specified as traceable requirements organised by *business module*. Each table enumerates specific requirements with identifiers referenced throughout design, implementation, and evaluation chapters. Features are implemented via *vertical slices*, described in Section 2.4.1.

==== Catalog Module

The Catalog module manages the product lifecycle: creation, classification, image handling, and the *CBIR* infrastructure that powers visual search.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [CAT-FR-01], [Product creation], [Create products with name, description, slug, and SEO metadata], [High],
    [CAT-FR-02], [Fashion metadata], [Assign fashion-specific fields: style code, season, material composition, care instructions, fit notes, department, gender target], [Medium],
    [CAT-FR-03], [Product variants], [Define sellable variants combining option values with SKU, barcode, dimensions, weight, and independent pricing], [High],
    [CAT-FR-21], [Variant option values], [Assign, synchronise, retrieve, and revoke option values to define the attribute combination each variant represents], [High],
    [CAT-FR-22], [Variant pricing], [Set, list, synchronise, and remove time-bound prices per variant with currency specification], [Medium],
    [CAT-FR-04], [Variant images], [Upload variant images with automatic thumbnail generation; multiple images per variant with display ordering], [High],
    [CAT-FR-14], [Variant image CRUD], [Delete, download, list, and update image metadata (alt text, display order) for variant images], [Medium],
    [CAT-FR-15], [Embedding regeneration], [Regenerate *vector embeddings* for images when the configured model changes or embedding is corrupted], [Medium],
    [CAT-FR-05], [Embedding generation], [Generate vector embeddings for each uploaded variant image via configured ML model; store in *pgvector* @pgvector2023 with model metadata], [High],
    [CAT-FR-06], [Image-based search], [Enable *CBIR*: upload query image, generate embedding, query pgvector with cosine distance via *HNSW* index @malkov2018efficient, return similarity-ranked results], [High],
    [CAT-FR-16], [Product availability], [Expose real-time per-variant stock availability through the storefront product detail endpoint], [High],
    [CAT-FR-17], [Similar products], [Retrieve visually similar products via embedding similarity for a given product], [Medium],
    [CAT-FR-07], [Search configuration], [Configure minimum similarity threshold and maximum result count for CBIR queries], [Medium],
    [CAT-FR-08], [Pluggable models], [Switch embedding model via environment variable without code changes; filter search results by active model], [High],
    [CAT-FR-09], [Taxonomy], [Organise products via hierarchical taxonomies with nested taxon trees], [Medium],
    [CAT-FR-18], [Taxon rules], [Define business rules on taxon nodes with update, sync, and retrieval operations], [Low],
    [CAT-FR-19], [Auto-classification], [Automatically classify products into taxons based on taxon rules when product attributes are updated], [Low],
    [CAT-FR-10], [Option types], [Define option types (e.g., Size, Colour, Material) with ordered option values reusable across products], [Medium],
    [CAT-FR-20], [Product option type association], [Assign, synchronise, retrieve, and revoke option types per product], [Medium],
    [CAT-FR-11], [Slug uniqueness], [Enforce slug uniqueness across the entire catalog at the database level], [High],
    [CAT-FR-12], [Status lifecycle], [Support product status: Draft, Active, Archived with configurable availability dates], [Medium],
    [CAT-FR-13], [Master variant], [Designate one variant per product as the master used for catalog listing display], [High],
  ),
    kind: table,
  caption: [Catalog module functional requirements.],
)

==== Identity Module

The Identity module provides authentication, authorisation, and user management. Authentication uses *JWT* access tokens @jones2015jwt with refresh token rotation.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [IDN-FR-01], [Registration], [Register new customer accounts with email, password, and basic profile information], [High],
    [IDN-FR-02], [Password login], [Authenticate via email and password, returning JWT access token (15-minute lifetime) and refresh token], [High],
    [IDN-FR-03], [OAuth login], [Authenticate via Google OAuth 2.0 as alternative to password-based login], [Medium],
    [IDN-FR-04], [Token rotation], [Each refresh invalidates previous token and issues new access and refresh pair], [High],
    [IDN-FR-05], [Reuse detection], [Presenting a consumed refresh token revokes all active tokens, containing credential theft], [High],
    [IDN-FR-06], [Guest sessions], [Support guest sessions via signed cookie, enabling anonymous cart and browsing], [High],
    [IDN-FR-07], [Role-based access], [Enforce role-based access control with permission granularity at domain:category:action level], [High],
    [IDN-FR-11], [Role management], [Create, update, delete, list roles; assign, synchronise, retrieve, revoke permissions per role], [Medium],
    [IDN-FR-12], [User-role assignment], [Assign, synchronise, retrieve, revoke roles per user; support direct permission grants], [Medium],
    [IDN-FR-13], [User status], [Enable and disable user accounts without deleting account or associated data], [Medium],
    [IDN-FR-08], [Password reset], [Reset passwords via time-limited, single-use email token with configurable expiry], [Medium],
    [IDN-FR-14], [Password change], [Allow authenticated users to change password, requiring current password verification], [Medium],
    [IDN-FR-15], [Email lifecycle], [Confirm email via verification token, change email with re-verification, resend verification on demand], [Medium],
    [IDN-FR-16], [Session management], [Retrieve current session, refresh tokens, terminate sessions via logout], [High],
    [IDN-FR-09], [User management], [Manage user accounts, role assignments, and permission grants through admin interface], [Medium],
    [IDN-FR-10], [Two-factor auth], [Optional TOTP-based two-factor authentication], [Low],
  ),
    kind: table,
  caption: [Identity module functional requirements.],
)

==== Inventory Module

The Inventory module tracks physical stock, manages checkout reservations to prevent overselling, and records movements for audit.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [INV-FR-01], [Stock locations], [Define stock locations with name, address, and active status flag], [Medium],
    [INV-FR-02], [Stock quantities], [Track quantities per variant per location with on-hand and reserved counters], [High],
    [INV-FR-08], [Stock item CRUD], [Create, update, delete, list stock items; bulk adjustment and CSV import], [Medium],
    [INV-FR-09], [Low stock alerting], [Identify items below configured threshold for proactive replenishment], [Low],
    [INV-FR-03], [Checkout reservation], [Reserve inventory during active checkout; release on cart expiry, cancellation, or timeout], [High],
    [INV-FR-11], [Cart-based reservation], [Reserve stock at cart level; retrieve status; release on abandonment or expiry], [High],
    [INV-FR-04], [Overselling prevention], [Reject checkout when requested quantity exceeds available at order confirmation], [High],
    [INV-FR-05], [Stock transfers], [Record transfers between locations with source, destination, quantity, timestamp], [Medium],
    [INV-FR-10], [Transfer lifecycle], [Create, in-transit, receive, cancel transfers; list with paging and detail], [Medium],
    [INV-FR-06], [Audit log], [Immutable audit log for all stock changes with reason and operator identity], [Medium],
    [INV-FR-12], [Movement audit trail], [Paged listing and detail view for stock movements as first-class data model], [Medium],
    [INV-FR-07], [Reservation expiry], [Expire stale reservations after configurable window (default 15 minutes inactivity)], [Medium],
  ),
    kind: table,
  caption: [Inventory module functional requirements.],
)

==== Ordering Module

The Ordering module handles purchase workflow from cart through checkout to completed order.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [ORD-FR-01], [Shopping cart], [Support guest and authenticated carts with variant selection and quantity management], [High],
    [ORD-FR-10], [Cart management], [Create, delete, empty carts; update quantities, remove items], [High],
    [ORD-FR-02], [Cart association], [Associate guest cart with user account on login/registration, merging without data loss], [Medium],
    [ORD-FR-03], [Cart expiry], [Auto-expire abandoned carts after 7 days via scheduled background job], [Medium],
    [ORD-FR-04], [Checkout states], [Enforce forward-only checkout: Address, Delivery, Payment, Confirm, Complete], [High],
    [ORD-FR-11], [Checkout validation], [Validate stock, address completeness, shipping method before payment], [High],
    [ORD-FR-12], [Shipping rate selection], [Select shipping rate from available options within cart before payment], [Medium],
    [ORD-FR-05], [Order totals], [Calculate ItemTotal + AdjustmentTotal + ShipmentTotal = Total independently], [High],
    [ORD-FR-06], [Dual state tracking], [Track payment state and shipment state independently per order], [Medium],
    [ORD-FR-07], [Cancellation], [Cancel order at any pre-confirmation stage; release inventory, void payment], [Medium],
    [ORD-FR-13], [Admin order lifecycle], [Approve, complete, resume orders; update status, shipping method, addresses], [Medium],
    [ORD-FR-14], [Customer order history], [List orders with paging; view detail with line items, payments, shipment tracking], [Medium],
    [ORD-FR-08], [Order numbering], [Generate unique, sequential order numbers within database transaction], [Medium],
    [ORD-FR-09], [Order immutability], [Mark completed orders as immutable; prevent modification after finalisation], [High],
  ),
    kind: table,
  caption: [Ordering module functional requirements.],
)

==== Payment Module

The Payment module manages payment intent lifecycle across Stripe and Bogus gateways.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [PAY-FR-01], [Intent creation], [Create payment intents with amount, currency, order identifier, gateway selection], [High],
    [PAY-FR-02], [Intent confirmation], [Confirm payment intents to authorise fund capture at checkout completion], [High],
    [PAY-FR-08], [Setup intent], [Create Stripe SetupIntents for saving payment methods for future use], [Low],
    [PAY-FR-03], [Capture and refund], [Capture, void, and refund payment intents with idempotency keys], [High],
    [PAY-FR-04], [Webhook processing], [Process Stripe webhooks with cryptographic signature verification], [High],
    [PAY-FR-09], [Payment void], [Void authorised un-captured payments; void all payments for cancelled orders], [Medium],
    [PAY-FR-10], [Payment method management], [Create, update, delete, activate, deactivate payment methods via admin], [Medium],
    [PAY-FR-05], [Refund validation], [Validate refund amount does not exceed captured amount], [Medium],
    [PAY-FR-06], [Bogus gateway], [Development gateway simulating full payment lifecycle without external API calls], [Medium],
    [PAY-FR-07], [State tracking], [Maintain independent payment state parallel to gateway for offline queries], [Medium],
  ),
    kind: table,
  caption: [Payment module functional requirements.],
)

==== Shipping Module

The Shipping module configures delivery methods and calculates rates by geographic zone.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [SHP-FR-01], [Delivery methods], [Configure methods with carrier name, pricing rules, geographic zones], [Medium],
    [SHP-FR-04], [Method lifecycle], [Create, update, delete, activate, deactivate methods via admin], [Medium],
    [SHP-FR-05], [Rate lifecycle], [Create, update, delete, list rates per method, zone, and weight/value tier], [Medium],
    [SHP-FR-02], [Rate calculation], [Calculate rates at checkout based on address zone, method, cart weight, cart value], [Medium],
    [SHP-FR-06], [Storefront calculation], [Calculate shipping cost for cart at checkout with available methods and rates], [High],
    [SHP-FR-03], [Shipment tracking], [Assign per-order tracking with carrier identifier and tracking number], [Low],
  ),
    kind: table,
  caption: [Shipping module functional requirements.],
)

==== Profile Module

The Profile module links customer identity to personalisation features.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [PRF-FR-01], [Addresses], [Manage shipping and billing addresses with type labels and default selection], [Medium],
    [PRF-FR-02], [Wishlists], [Create, rename, delete lists; add and remove product variants with notes], [Low],
    [PRF-FR-03], [Notifications], [Configure per-channel notification preferences (email, SMS) with opt-in/opt-out], [Low],
  ),
    kind: table,
  caption: [Profile module functional requirements.],
)

==== Location Module

The Location module provides country and state reference data for address validation, shipping zones, and tax.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [LOC-FR-01], [Countries], [Country reference data with ISO 3166-1 codes, display names, active status], [Medium],
    [LOC-FR-02], [States], [State reference data with ISO 3166-2 codes, linked to parent country], [Medium],
    [LOC-FR-03], [Country CRUD], [Create, update, delete, list countries; retrieve by ID and ISO code], [Medium],
    [LOC-FR-04], [State CRUD], [Create, update, delete, list states; retrieve by ID and ISO code; filter by country], [Medium],
  ),
    kind: table,
  caption: [Location module functional requirements.],
)

==== Dashboard Module

The Dashboard module aggregates KPIs from all business modules.

#figure(
  table(
    columns: (auto, 1fr, 2fr, auto),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),
    [DSH-FR-01], [Aggregate dashboard], [Cross-module dashboard aggregating product counts, order volumes, inventory, user statistics], [Medium],
  ),
    kind: table,
  caption: [Dashboard module functional requirements.],
)
