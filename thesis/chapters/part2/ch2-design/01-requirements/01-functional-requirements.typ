=== Functional Requirements

Functional requirements are organized by business module and grouped into related clusters. Each group carries a unique identifier (for example, CAT-GRP-01) that is used throughout the design, implementation, and testing chapters to trace each use case back to the capability it relies on.

==== Catalog Module

Manages product lifecycle, classification, image handling, and CBIR infrastructure.

#strong[Product Lifecycle] (CAT-GRP-01). Products are created with name, description, URL slug, and SEO metadata, enriched with fashion attributes such as style code, season, material composition, care instructions, fit notes, department, and gender target. A unique slug is enforced at the database constraint level, each product progresses through Draft, Active, and Archived states with configurable availability dates, and one variant is designated as the master representation for catalog displays.

#strong[Variants & Options] (CAT-GRP-02). Sellable variants combine option values into attribute configurations, each carrying its own SKU, barcode, physical dimensions, weight, and independent time-bound pricing with currency specification. Reusable option types (Size, Color, Material) define ordered values that are assigned, synchronized, retrieved, and revoked on a per-product basis.

#strong[Images & Embeddings] (CAT-GRP-03). Variant images support upload with automatic thumbnail generation, multiple images per variant with display ordering, and full CRUD for metadata such as alt text. Embeddings are generated for uploaded images by the ML sidecar and stored in pgvector with model metadata; regeneration runs when the active model configuration changes or corrupted embeddings are detected.

#strong[Visual Search] (CAT-GRP-04). Content-based image retrieval accepts query images, extracts embeddings, and queries pgvector via HNSW indexing with cosine distance to return ranked results. Minimum similarity thresholds and maximum result limits are configurable, embedding models are switchable via environment variables with results filtered by active model, per-variant availability is exposed through storefront endpoints, and visually similar products can be retrieved for a target product.

#strong[Taxonomy & Classification] (CAT-GRP-05). Products are organized through hierarchical taxonomies with nested taxon trees; business rules on taxon nodes support update, synchronization, and retrieval, and products are automatically classified into taxons when their attributes change.

==== Identity Module

Authentication, authorization, and user management with JWT access tokens and refresh token rotation.

#strong[Authentication & Sessions] (IDN-GRP-01). Customer accounts are registered with email, password, and basic profile information, and authenticated via email and password with a 15-minute JWT access token and refresh token. Refresh tokens rotate on each issuance and a previously consumed token revokes the whole token family, guest sessions operate through signed cookies, and users can inspect their sessions and terminate them via logout.

#strong[Credential & Email Lifecycle] (IDN-GRP-02). Passwords are reset through single-use, time-limited email verification tokens and changed after verifying the current credential; email addresses are confirmed via verification tokens with support for modification requests.

#strong[Roles & Permissions] (IDN-GRP-03). Role-based access control is enforced with permission claims formatted as domain.category.resource.action at middleware boundaries; roles are created, updated, deleted, and listed with per-role permission assignments, and roles are assigned and revoked per user with direct permission overrides.

#strong[User Governance] (IDN-GRP-04). Administrative endpoints manage user accounts, role bindings, and permission grants, and user accounts can be enabled or disabled without purging user record history.

==== Inventory Module

Stock tracking, checkout reservations, and audit ledgers.

#strong[Stock & Locations] (INV-GRP-01). Physical stock locations are defined with address metadata and active flags; on-hand and reserved quantities are tracked per variant per location with full CRUD, bulk adjustments, and CSV imports, and inventory falling below configured thresholds drives replenishment notifications.

#strong[Reservations & Overselling] (INV-GRP-02). Inventory is reserved during checkout and at cart level with status inspection and automatic release on cart expiry, cancellation, timeout, or abandonment; order confirmation is rejected when requested quantities exceed unreserved stock, and stale holds expire after a configurable timeout (default: 15 minutes).

#strong[Transfers] (INV-GRP-03). Stock movements between locations are recorded with origin, destination, quantity, and timestamp, progressing through Created, In-Transit, Received, and Cancelled states with paged listings.

#strong[Audit & Movement] (INV-GRP-04). An immutable audit log records stock changes with operator identity and reason codes, and detailed, paged audit trails of stock movements are maintained as first-class domain entities.

==== Ordering Module

Shopping cart through checkout state transitions to completed orders.

#strong[Cart] (ORD-GRP-01). Guest and customer carts support variant item addition, quantity adjustment, creation, clearing, and deletion; guest cart contents merge into customer accounts upon authentication without data loss, and abandoned carts are purged after 7 days of inactivity by scheduled background maintenance.

#strong[Checkout Pipeline] (ORD-GRP-02). Checkout enforces strict sequential state transitions (Address, Delivery, Payment, Confirm, Complete) with validation of inventory levels, address completeness, and shipping method selection prior to payment; applicable shipping rates are selected within cart boundaries before payment is finalized.

#strong[Order Accounting & State] (ORD-GRP-03). Monetary balances are calculated independently (item total + adjustments + shipping = grand total), order numbering generates unique sequential references within database isolation transactions, finalized orders are locked as immutable, and parallel, decoupled state machine counters track payment and fulfillment state.

#strong[Order Management] (ORD-GRP-04). Orders can be cancelled before fulfillment confirmation, releasing inventory holds and voiding payments; administrators approve, complete, resume, and modify pending orders through administrative surfaces, and customers access paged order histories with line-item detail, payment status, and tracking information.

==== Payment Module

Payment intent lifecycles across Stripe and internal test gateways.

#strong[Payment Intents] (PAY-GRP-01). Payment intents specify amount, currency, order reference, and gateway target, and are confirmed to authorize fund capture during checkout finalization; Stripe SetupIntents securely save customer payment methods for future checkouts.

#strong[Capture, Refund & Void] (PAY-GRP-02). Capture, void, and refund operations execute with idempotency keys; validation prevents refund amounts from exceeding total captured funds, and authorized-but-uncaptured payments are voided on cancelled orders to release authorization holds.

#strong[Gateway Integration & State] (PAY-GRP-03). Incoming gateway webhooks are verified with HMAC cryptographic signatures and processed with idempotency; local payment state records run parallel to external gateway records for offline queries, and a mock gateway simulates transaction lifecycles without external API calls.

#strong[Method Configuration] (PAY-GRP-04). Administrators create, update, toggle, and delete configured payment gateway methods through admin panels.

==== Shipping Module

Delivery options, geographic zones, and rate calculation.

#strong[Delivery Methods & Rates] (SHP-GRP-01). Shipping methods are configured with carrier details, rate rules, and geographic zones; administrative endpoints create, update, activate, deactivate, and delete methods, and rate matrices map to methods, zones, and weight/value tiers.

#strong[Rate Calculation] (SHP-GRP-02). Shipping costs are calculated dynamically from address zone, method, cart weight, and total value, and eligible methods and rates are calculated and displayed during storefront checkout.

#strong[Shipment Tracking] (SHP-GRP-03). Active shipments progress through Pending, Ready, Shipped, and Delivered states via administrative status updates.

==== Profile Module

Customer preferences, addresses, and saved items.

#strong[Address Book & Saved Items] (PRF-GRP-01). Shipping and billing addresses are managed with default selections and custom labels, and named wishlists contain targeted product variants with notes.

#strong[Notification Preferences] (PRF-GRP-02). Channel-specific notification preferences (email, SMS) are managed with opt-in flags.

==== Location Module

Reference data for countries and states used in address validation and shipping zones.

#strong[Country & State Directories] (LOC-GRP-01). Country reference entries carry ISO 3166-1 alpha-2 codes, names, and active flags; state and province entries carry ISO 3166-2 codes linked to parent countries. Both support full CRUD by database ID or ISO code, with states listed and filtered by parent country.