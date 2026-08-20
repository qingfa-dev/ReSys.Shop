=== Functional Requirements

Functional requirements are organized by business module with unique identifiers traceable throughout design, implementation, and evaluation chapters.

==== Catalog Module

Manages product lifecycle, classification, image handling, and CBIR infrastructure.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [CAT-FR-01], [Product Creation], [Create products with name, description, URL slug, and SEO metadata.], [High],
    [CAT-FR-02], [Fashion Metadata], [Assign fashion attributes: style code, season, material composition, care instructions, fit notes, department, and gender target.], [Medium],
    [CAT-FR-03], [Product Variants], [Define sellable variants combining option values with SKU, barcode, physical dimensions, weight, and independent pricing.], [High],
    [CAT-FR-21], [Variant Option Values], [Assign, synchronize, retrieve, and revoke option values to define attribute combinations for variants.], [High],
    [CAT-FR-22], [Variant Pricing], [Set, list, synchronize, and remove time-bound prices per variant with currency specification.], [Medium],
    [CAT-FR-04], [Variant Images], [Upload variant images with automatic thumbnail generation, supporting multiple images per variant with display ordering.], [High],
    [CAT-FR-14], [Variant Image CRUD], [Delete, download, list, and update image metadata (alt text, display order) for variant images.], [Medium],
    [CAT-FR-15], [Embedding Regeneration], [Regenerate vector embeddings for images when the active model configuration changes or corrupted embeddings are detected.], [Medium],
    [CAT-FR-05], [Embedding Generation], [Generate vector embeddings for uploaded variant images via the ML sidecar; store in pgvector @pgvector2023 with model metadata.], [High],
    [CAT-FR-06], [Image-Based Search], [Enable CBIR: accept query images, extract embeddings, query pgvector via HNSW indexing @malkov2018efficient using cosine distance, and return ranked results.], [High],
    [CAT-FR-16], [Product Availability], [Expose real-time per-variant stock availability through storefront product detail endpoints.], [High],
    [CAT-FR-17], [Similar Products], [Retrieve visually similar products based on vector embedding similarity for a target product.], [Medium],
    [CAT-FR-07], [Search Configuration], [Configure minimum similarity thresholds and maximum result limits for CBIR queries.], [Medium],
    [CAT-FR-08], [Pluggable Models], [Switch embedding models via environment variables without code modifications; filter search results by active model.], [High],
    [CAT-FR-09], [Taxonomy], [Organize products via hierarchical taxonomies with nested taxon trees.], [Medium],
    [CAT-FR-18], [Taxon Rules], [Define business rules on taxon nodes with update, synchronization, and retrieval capabilities.], [Low],
    [CAT-FR-19], [Auto-Classification], [Automatically classify products into taxons based on taxon rules when product attributes change.], [Low],
    [CAT-FR-10], [Option Types], [Define option types (e.g., Size, Color, Material) with ordered values reusable across products.], [Medium],
    [CAT-FR-20], [Option Association], [Assign, synchronize, retrieve, and revoke option types on a per-product basis.], [Medium],
    [CAT-FR-11], [Slug Uniqueness], [Enforce URL slug uniqueness across the catalog at the database constraint level.], [High],
    [CAT-FR-12], [Status Lifecycle], [Support product lifecycles (Draft, Active, Archived) with configurable availability dates.], [Medium],
    [CAT-FR-13], [Master Variant], [Designate one variant per product as the master representation for catalog displays.], [High],
  ),
  kind: table,
  caption: [Catalog module functional requirements.],
) <cat-functional-requirements>

==== Identity Module

Authentication, authorization, and user management with JWT access tokens and refresh token rotation.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [IDN-FR-01], [Registration], [Register customer accounts with email, password, and basic profile information.], [High],
    [IDN-FR-02], [Password Login], [Authenticate via email and password, issuing a 15-minute JWT access token and refresh token.], [High],
    [IDN-FR-03], [OAuth Login], [Authenticate via Google OAuth 2.0 as an alternative to password credentials.], [Medium],
    [IDN-FR-04], [Token Rotation], [Invalidate previous refresh tokens upon issuance, returning a new access/refresh pair.], [High],
    [IDN-FR-05], [Reuse Detection], [Revoke all active refresh tokens for a user if a previously consumed token is presented.], [High],
    [IDN-FR-06], [Guest Sessions], [Support guest sessions via signed cookies, enabling anonymous cart management and browsing.], [High],
    [IDN-FR-07], [Role-Based Access], [Enforce RBAC with permission claims formatted as domain.category.resource.action at middleware boundaries.], [High],
    [IDN-FR-11], [Role Management], [Create, update, delete, and list roles; manage permission assignments per role.], [Medium],
    [IDN-FR-12], [User-Role Assignment], [Assign and revoke roles per user, supporting direct permission overrides.], [Medium],
    [IDN-FR-13], [User Status], [Enable or disable user accounts without purging user record history.], [Medium],
    [IDN-FR-08], [Password Reset], [Reset passwords via single-use, time-limited email verification tokens.], [Medium],
    [IDN-FR-14], [Password Change], [Allow authenticated users to update passwords upon verifying current credentials.], [Medium],
    [IDN-FR-15], [Email Lifecycle], [Confirm email addresses via verification tokens and support email modification requests.], [Medium],
    [IDN-FR-16], [Session Management], [Retrieve current sessions, inspect refresh tokens, and terminate sessions via logout.], [High],
    [IDN-FR-09], [User Governance], [Manage user accounts, role bindings, and permission grants via administrative endpoints.], [Medium],
    [IDN-FR-10], [Two-Factor Auth], [Provide optional TOTP-based two-factor authentication.], [Low],
  ),
  kind: table,
  caption: [Identity module functional requirements.],
) <idn-functional-requirements>

==== Inventory Module

Stock tracking, checkout reservations, and audit ledgers.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [INV-FR-01], [Stock Locations], [Define physical stock locations with address metadata and active flags.], [Medium],
    [INV-FR-02], [Stock Quantities], [Track on-hand and reserved quantities per variant per location.], [High],
    [INV-FR-08], [Stock Item CRUD], [Create, update, delete, and list stock items; support bulk adjustments and CSV imports.], [Medium],
    [INV-FR-09], [Low Stock Alerting], [Identify inventory falling below configured thresholds for replenishment notifications.], [Low],
    [INV-FR-03], [Checkout Reservation], [Reserve inventory during checkout; release upon cart expiry, cancellation, or timeout.], [High],
    [INV-FR-11], [Cart Reservations], [Reserve stock at cart level, inspect status, and auto-release upon abandonment.], [High],
    [INV-FR-04], [Overselling Protection], [Reject order confirmation when requested quantities exceed available unreserved stock.], [High],
    [INV-FR-05], [Stock Transfers], [Record stock movements between locations with origin, destination, quantity, and timestamp.], [Medium],
    [INV-FR-10], [Transfer Lifecycle], [Manage transfer states (Created, In-Transit, Received, Cancelled) with paged listings.], [Medium],
    [INV-FR-06], [Audit Logging], [Maintain an immutable audit log for stock changes recording operator identity and reason codes.], [Medium],
    [INV-FR-12], [Movement History], [Provide detailed, paged audit trails for stock movements as first-class domain entities.], [Medium],
    [INV-FR-07], [Reservation Expiry], [Expire stale inventory holds after a configurable timeout (default: 15 minutes).], [Medium],
  ),
  kind: table,
  caption: [Inventory module functional requirements.],
) <inv-functional-requirements>

==== Ordering Module

Shopping cart through checkout state transitions to completed orders.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [ORD-FR-01], [Shopping Cart], [Support guest and customer carts with variant item addition and quantity adjustments.], [High],
    [ORD-FR-10], [Cart Management], [Create, clear, and delete carts, as well as modify item quantities and line items.], [High],
    [ORD-FR-02], [Cart Association], [Merge guest cart contents into customer accounts upon authentication without data loss.], [Medium],
    [ORD-FR-03], [Cart Expiry], [Purge abandoned carts inactive for 7 days via scheduled background maintenance tasks.], [Medium],
    [ORD-FR-04], [Checkout Pipeline], [Enforce strict sequential checkout state transitions: Address → Delivery → Payment → Confirm → Complete.], [High],
    [ORD-FR-11], [Checkout Validation], [Validate inventory levels, address completeness, and shipping method selection prior to payment.], [High],
    [ORD-FR-12], [Shipping Selection], [Select applicable shipping rates within cart boundaries prior to finalizing payment.], [Medium],
    [ORD-FR-05], [Order Totals], [Calculate monetary balances independently: $text("Item Total") + text("Adjustments") + text("Shipping") = text("Grand Total").$], [High],
    [ORD-FR-06], [Dual State Tracking], [Maintain parallel, decoupled state machine counters for payment state and fulfillment state.], [Medium],
    [ORD-FR-07], [Order Cancellation], [Cancel orders prior to fulfillment confirmation, releasing inventory holds and voiding payments.], [Medium],
    [ORD-FR-13], [Admin Order Management], [Approve, complete, resume, and modify pending order details via administrative surfaces.], [Medium],
    [ORD-FR-14], [Customer History], [Expose paged customer order histories with line item detail, payment status, and tracking info.], [Medium],
    [ORD-FR-08], [Order Numbering], [Generate unique, sequential order reference numbers within database isolation transactions.], [Medium],
    [ORD-FR-09], [Order Immutability], [Lock finalized orders as immutable to prevent modification post-completion.], [High],
  ),
  kind: table,
  caption: [Ordering module functional requirements.],
) <ord-functional-requirements>

==== Payment Module

Payment intent lifecycles across Stripe and internal test gateways.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [PAY-FR-01], [Intent Creation], [Generate payment intents specifying amount, currency, order reference, and gateway target.], [High],
    [PAY-FR-02], [Intent Confirmation], [Confirm payment intents to authorize fund capture during checkout finalization.], [High],
    [PAY-FR-08], [Setup Intent], [Create Stripe SetupIntents to securely save customer payment methods for future checkouts.], [Low],
    [PAY-FR-03], [Capture and Refund], [Execute capture, void, and refund operations using idempotency keys.], [High],
    [PAY-FR-04], [Webhook Processing], [Verify and process incoming gateway webhooks using HMAC cryptographic signatures.], [High],
    [PAY-FR-09], [Payment Voiding], [Void authorized uncaptured payments and release authorization holds on cancelled orders.], [Medium],
    [PAY-FR-10], [Method Management], [Create, update, toggle, and delete configured payment gateway methods via admin panels.], [Medium],
    [PAY-FR-05], [Refund Validation], [Enforce validation rules preventing refund amounts from exceeding total captured funds.], [Medium],
    [PAY-FR-06], [Bogus Gateway], [Provide a mock payment gateway simulating transaction lifecycles without external API calls.], [Medium],
    [PAY-FR-07], [State Tracking], [Maintain local payment state records parallel to external gateway records for offline query support.], [Medium],
  ),
  kind: table,
  caption: [Payment module functional requirements.],
) <pay-functional-requirements>

==== Shipping Module

Delivery options, geographic zones, and rate calculation.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [SHP-FR-01], [Delivery Methods], [Configure shipping methods with carrier details, rate rules, and geographic zones.], [Medium],
    [SHP-FR-04], [Method Management], [Create, update, activate, deactivate, and delete shipping methods via administrative endpoints.], [Medium],
    [SHP-FR-05], [Rate Lifecycle], [Manage rate matrices mapped to shipping methods, geographic zones, and weight/value tiers.], [Medium],
    [SHP-FR-02], [Rate Calculation], [Calculate shipping costs dynamically based on address zone, method, cart weight, and total value.], [Medium],
    [SHP-FR-06], [Storefront Evaluation], [Evaluate and display eligible shipping methods and rates during checkout.], [High],
    [SHP-FR-03], [Shipment Tracking], [Assign carrier identifiers and tracking tracking numbers to active shipments.], [Low],
  ),
  kind: table,
  caption: [Shipping module functional requirements.],
) <shp-functional-requirements>

==== Profile Module

Customer preferences, addresses, and saved items.

#figure(
  table(
     columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [PRF-FR-01], [Address Book], [Manage shipping and billing addresses with default selections and custom labels.], [Medium],
    [PRF-FR-02], [Wishlists], [Create, update, and remove named wishlists containing targeted product variants and notes.], [Low],
    [PRF-FR-03], [Notification Settings], [Manage channel-specific notification preferences (email, SMS) with opt-in flags.], [Low],
  ),
  kind: table,
  caption: [Profile module functional requirements.],
) <prf-functional-requirements>

==== Location Module

Reference data for countries and states used in address validation and shipping zones.

#figure(
  table(
    columns: (1.5fr, 2fr, 3.8fr, 1.3fr),
    stroke: 0.5pt,
    align: (center, left, left + horizon, center + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Requirement*], [*Description*], [*Priority*]),

    [LOC-FR-01], [Country Directory], [Maintain country reference entries with ISO 3166-1 alpha-2 codes, names, and active flags.], [Medium],
    [LOC-FR-02], [State Directory], [Maintain state/province entries with ISO 3166-2 codes linked to parent countries.], [Medium],
    [LOC-FR-03], [Country Management], [Create, update, delete, and retrieve country records by database ID or ISO code.], [Medium],
    [LOC-FR-04], [State Management], [Create, update, delete, and list state records filtered by parent country.], [Medium],
  ),
  kind: table,
  caption: [Location module functional requirements.],
) <loc-functional-requirements>