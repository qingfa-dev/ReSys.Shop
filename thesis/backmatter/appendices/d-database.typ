= Database Schema <appendix-d>

#show heading.where(level: 2): set heading(numbering: none)
#show heading.where(level: 3): set heading(numbering: none)
#show heading.where(level: 4): set heading(numbering: none)

#set par(leading: 0.45em, justify: true)

#show figure: set block(spacing: 2pt)
#set figure(gap: 2pt)
#show figure.caption: set text(size: 7.5pt)

#set table(
  stroke: 0.3pt + luma(150),
  inset: (x: 3pt, y: 2pt),
)
#show table: set text(size: 7.5pt)
#show table.cell.where(y: 0): set text(weight: "bold", size: 8pt)

#let db-table(cap, ..rows) = figure(
  table(
    columns: (auto, auto, auto, 1fr),
    align: (center, left, left, left),
    table.header([*\#*], [*Field*], [*Type*], [*Description*]),
    ..rows
  ),
  kind: table,
  caption: cap,
)

The database uses PostgreSQL 17 with pgvector via EF Core 10. Two migrations, 35 `IEntityTypeConfiguration<T>` classes across eight bounded contexts. All tables share UUID PKs, audit columns (`created_at_utc`, `modified_at_utc`, `created_by`, `modified_by`), and soft-deletion (`is_deleted`, `deleted_at_utc`, `deleted_by`) with global query filters; contention-sensitive entities add `row_version` for optimistic concurrency.

== Catalog Schema

==== Products

#db-table([Product. 1:N variants, product_option_types, classifications (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(255)], [Display name],
  [3], [`slug`], [varchar(255)], [Unique URL slug],
  [4], [`description`], [varchar(2000)], [Marketing description],
  [5], [`status`], [text], [Draft, Active, Archived],
  [6], [`style_code`], [text], [Style code],
  [7], [`season_name`], [text], [Season label],
  [8], [`material_composition`], [text], [Material notes],
  [9], [`care_instructions`], [text], [Care instructions],
  [10], [`fit_notes`], [text], [Fit notes],
  [11], [`department`], [text], [Department],
  [12], [`gender_target`], [text], [Target gender],
  [13], [`available_on`], [timestamptz], [Publish date],
  [14], [`discontinue_on`], [timestamptz], [End-of-life date],
  [15], [`make_active_at`], [timestamptz], [Scheduled activation],
  [16], [`meta_title`], [varchar(255)], [SEO title],
  [17], [`meta_description`], [varchar(500)], [SEO description],
  [18], [`meta_keywords`], [varchar(255)], [SEO keywords],
  [19], [`master_variant_id`], [uuid], [FK to master variant],
  [20], [`is_deleted`], [bool], [Soft-delete],
)

==== Variants

#db-table([Variant. 1:N prices, variant_images, option_value_variants (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`sku`], [varchar(255)], [Unique SKU],
  [3], [`is_master`], [bool], [Master variant flag],
  [4], [`position`], [int], [Display order],
  [5], [`track_inventory`], [bool], [Inventory tracking enabled],
  [6], [`barcode`], [text], [UPC/EAN barcode],
  [7], [`hs_code`], [text], [Harmonized tariff code],
  [8], [`weight`], [numeric(18,2)], [Weight],
  [9], [`weight_unit`], [text], [Weight unit (kg, lb, oz)],
  [10], [`height`], [numeric(18,2)], [Height],
  [11], [`width`], [numeric(18,2)], [Width],
  [12], [`depth`], [numeric(18,2)], [Depth],
  [13], [`dimensions_unit`], [text], [Dimension unit (cm, in)],
  [14], [`price`], [numeric(18,2)], [Default price],
  [15], [`cost_price`], [numeric(18,2)], [Cost for margin],
  [16], [`cost_currency`], [varchar(3)], [Cost currency code],
  [17], [`discontinued_on`], [timestamptz], [Discontinuation date],
  [18], [`product_id`], [uuid], [FK to product (cascade)],
  [19], [`is_deleted`], [bool], [Soft-delete],
)

==== Variant Images

#db-table([Variant image. 1:N image_embeddings (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`url`], [varchar(2048)], [Public CDN URL],
  [3], [`storage_path`], [varchar(500)], [Object-storage key],
  [4], [`alt`], [varchar(500)], [Alt text],
  [5], [`content_type`], [varchar(100)], [MIME type],
  [6], [`file_name`], [varchar(255)], [Original filename],
  [7], [`file_size`], [int], [Size in bytes],
  [8], [`width`], [int], [Width in px],
  [9], [`height`], [int], [Height in px],
  [10], [`dimensions_unit`], [varchar(10)], [Dimension unit (px, in, cm)],
  [11], [`position`], [int], [Display order],
  [12], [`type`], [text], [Default, Thumbnail, Square, Gallery, Search],
  [13], [`variant_id`], [uuid], [FK to variant (cascade)],
)

==== Image Embeddings

#db-table([Image embedding. pgvector vector(512) with IVFFlat cosine distance index.],
  [1], [`id`], [uuid], [PK],
  [2], [`model_name`], [varchar(100)], [Embedding model id],
  [3], [`model_version`], [varchar(50)], [Model version],
  [4], [`vector`], [vector(512)], [512-dim; IVFFlat cosine index (lists=100)],
  [5], [`dimensions`], [int], [Dimension count],
  [6], [`status`], [text], [Generation state],
  [7], [`hangfire_job_id`], [text], [Background job id],
  [8], [`error`], [text], [Error on failure],
  [9], [`variant_image_id`], [uuid], [FK to variant image (cascade)],
)

==== Option Types

#db-table([Option type. 1:N option_values (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(100)], [Option key (e.g. size)],
  [3], [`presentation`], [varchar(100)], [Display text],
  [4], [`position`], [int], [Display order],
  [5], [`filterable`], [bool], [Usable as filter],
  [6], [`is_deleted`], [bool], [Soft-delete],
)

==== Option Values

#db-table([Option value. FK to option_types.],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(100)], [Value key (e.g. s)],
  [3], [`presentation`], [varchar(100)], [Display text],
  [4], [`position`], [int], [Display order],
  [5], [`option_type_id`], [uuid], [FK to option type (cascade)],
)

==== Product Option Types

#db-table([Product-option type join. Unique on (product_id, option_type_id).],
  [1], [`id`], [uuid], [PK],
  [2], [`position`], [int], [Display order],
  [3], [`product_id`], [uuid], [FK to product],
  [4], [`option_type_id`], [uuid], [FK to option type],
)

==== Option Value Variants

#db-table([Variant-option value join. Unique on (variant_id, option_value_id).],
  [1], [`id`], [uuid], [PK],
  [2], [`variant_id`], [uuid], [FK to variant],
  [3], [`option_value_id`], [uuid], [FK to option value],
)

==== Taxonomies

#db-table([Taxonomy. 1:N taxa (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(100)], [Taxonomy key],
  [3], [`presentation`], [varchar(100)], [Display text],
  [4], [`position`], [int], [Display order],
  [5], [`is_deleted`], [bool], [Soft-delete],
)

==== Taxa

#db-table([Taxon. Nested set hierarchy. FK to taxonomies; self-ref FK for tree.],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(255)], [Taxon name],
  [3], [`presentation`], [varchar(255)], [Display text],
  [4], [`description`], [varchar(2000)], [Category description],
  [5], [`position`], [int], [Sibling order],
  [6], [`lft`], [int], [Nested-set left],
  [7], [`rgt`], [int], [Nested-set right],
  [8], [`depth`], [int], [Tree depth],
  [9], [`slug`], [varchar(255)], [URL slug],
  [10], [`taxonomy_id`], [uuid], [FK to taxonomy (cascade)],
  [11], [`parent_id`], [uuid], [Self-ref FK to parent (restrict)],
  [12], [`is_deleted`], [bool], [Soft-delete],
)

==== Taxon Rules

#db-table([Taxon rule. Auto-classification rules. FK to taxa.],
  [1], [`id`], [uuid], [PK],
  [2], [`taxon_id`], [uuid], [FK to target taxon (cascade)],
  [3], [`type`], [varchar(50)], [Rule type],
  [4], [`value`], [varchar(255)], [Match value],
  [5], [`match_policy`], [varchar(50)], [All/any policy],
)

==== Classifications

#db-table([Classification. Product-taxon join. Unique pair.],
  [1], [`id`], [uuid], [PK],
  [2], [`position`], [int], [Display order],
  [3], [`is_automatic`], [bool], [Auto-assigned flag],
  [4], [`product_id`], [uuid], [FK to product],
  [5], [`taxon_id`], [uuid], [FK to taxon],
)

==== Prices

#db-table([Price. Time-bound pricing per variant.],
  [1], [`id`], [uuid], [PK],
  [2], [`amount`], [numeric(18,2)], [Selling price],
  [3], [`compare_at_amount`], [numeric(18,2)], [Strikethrough price],
  [4], [`currency`], [varchar(3)], [ISO 4217 code],
  [5], [`country_iso`], [varchar(2)], [Country override],
  [6], [`is_default`], [bool], [Default for variant],
  [7], [`variant_id`], [uuid], [FK to variant (cascade)],
)

== Identity Schema

The identity schema extends ASP.NET `IdentityDbContext<Guid>` with standard tables (`users`, `refresh_tokens`, `roles`, `user_roles`, `user_claims`, `user_logins`, `user_tokens`, `role_claims`). Custom fields on `users`: `first_name`, `last_name`, `is_active`, `last_login_at_utc`, `sign_in_count`. WebAuthn passwordless auth via `passkeys(credential_id bytea)`.

== Ordering Schema

==== Orders

#db-table([Order. Forward-only checkout state machine. Indexed (user_id, status) and (session_id, status). 1:N line_items, adjustments (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`number`], [varchar(50)], [Human-readable order ref],
  [3], [`session_id`], [varchar(100)], [Guest cart session; indexed with status],
  [4], [`status`], [text], [Lifecycle state],
  [5], [`checkout_state`], [text], [Address, Delivery, Payment, Confirm, Complete],
  [6], [`payment_state`], [text], [Checkout, Processing, Completed, Failed, Void],
  [7], [`shipment_state`], [text], [Pending, Ready, Shipped, Delivered],
  [8], [`currency`], [varchar(3)], [ISO 4217 code],
  [9], [`item_total`], [numeric(18,2)], [Sum of line items],
  [10], [`adjustment_total`], [numeric(18,2)], [Sum of adjustments],
  [11], [`shipment_total`], [numeric(18,2)], [Shipping total],
  [12], [`total`], [numeric(18,2)], [Grand total],
  [13], [`payment_total`], [numeric(18,2)], [Captured payments],
  [14], [`outstanding_balance`], [numeric(18,2)], [total - payment_total],
  [15], [`total_weight`], [numeric(18,2)], [Sum of line item weights],
  [16], [`item_count`], [int], [Number of line items],
  [17], [`is_free_shipping`], [bool], [Free shipping flag],
  [18], [`email`], [varchar(255)], [Contact email],
  [19], [`special_instructions`], [varchar(500)], [Customer notes],
  [20], [`completed_at_utc`], [timestamptz], [Completion time],
  [21], [`canceled_at_utc`], [timestamptz], [Cancellation time],
  [22], [`canceled_by_id`], [uuid], [Who canceled],
  [23], [`approved_at_utc`], [timestamptz], [Approval time],
  [24], [`approved_by_id`], [uuid], [Who approved],
  [25], [`payment_processing_at_utc`], [timestamptz], [Payment processing start],
  [26], [`payment_completed_at_utc`], [timestamptz], [Payment completed],
  [27], [`payment_failed_at_utc`], [timestamptz], [Payment failed],
  [28], [`shipment_shipped_at_utc`], [timestamptz], [Shipment dispatched],
  [29], [`shipment_delivered_at_utc`], [timestamptz], [Shipment delivered],
  [30], [`bill_address_id`], [uuid], [FK to billing address],
  [31], [`ship_address_id`], [uuid], [FK to shipping address],
  [32], [`user_id`], [uuid], [FK to user; indexed with status],
  [33], [`shipping_method_id`], [uuid], [FK to shipping method],
  [34], [`shipping_rate_id`], [uuid], [FK to shipping rate],
  [35], [`payment_method_id`], [uuid], [FK to payment method],
  [36], [`is_deleted`], [bool], [Soft-delete],
)

==== Line Items

#db-table([Line item. Price snapshots protect historical orders from catalog updates.],
  [1], [`id`], [uuid], [PK],
  [2], [`quantity`], [int], [Purchased qty],
  [3], [`price`], [numeric(18,2)], [Unit price snapshot],
  [4], [`total`], [numeric(18,2)], [Line total],
  [5], [`adjustment_total`], [numeric(18,2)], [Line adjustments],
  [6], [`currency`], [varchar(3)], [ISO 4217 code],
  [7], [`order_id`], [uuid], [FK to order (cascade)],
  [8], [`variant_id`], [uuid], [FK to variant (no action; snapshot)],
)

==== Adjustments

#db-table([Adjustment. Polymorphic (tax, shipping, discount). FK to orders.],
  [1], [`id`], [uuid], [PK],
  [2], [`label`], [varchar(255)], [Label (e.g. Tax)],
  [3], [`amount`], [numeric(18,2)], [Signed amount],
  [4], [`eligible`], [bool], [Applies to order],
  [5], [`included`], [bool], [Included in price],
  [6], [`mandatory`], [bool], [Non-removable],
  [7], [`state`], [varchar(50)], [Lifecycle state],
  [8], [`adjustable_id`], [uuid], [Polymorphic target id],
  [9], [`adjustable_type`], [varchar(100)], [Polymorphic target type],
  [10], [`source_id`], [uuid], [Polymorphic source id],
  [11], [`source_type`], [varchar(100)], [Polymorphic source type],
  [12], [`order_id`], [uuid], [FK to order (cascade)],
)

== Payment Schema

==== Payment Methods

#db-table([Payment method. Preferences as jsonb. Settings encrypted. 1:N payment_captures (set null).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(255)], [Display name],
  [3], [`code`], [varchar(50)], [Unique code],
  [4], [`description`], [varchar(1000)], [Customer description],
  [5], [`provider_key`], [varchar(50)], [Gateway id (e.g. stripe)],
  [6], [`active`], [bool], [Currently available],
  [7], [`auto_capture`], [bool], [Auto vs manual capture],
  [8], [`display_on`], [text], [FrontEnd, BackEnd, Both],
  [9], [`position`], [int], [Sort order],
  [10], [`preferences`], [jsonb], [Gateway config],
  [11], [`settings`], [text], [Encrypted credentials],
  [12], [`webhook_enabled`], [bool], [Webhooks processed],
  [13], [`is_deleted`], [bool], [Soft-delete],
)

==== Payment Captures

#db-table([Payment capture. xmin for optimistic concurrency. Event ids as jsonb for idempotency.],
  [1], [`id`], [uuid], [PK],
  [2], [`number`], [varchar(50)], [Human-readable ref],
  [3], [`amount`], [numeric(18,2)], [Authorized amount],
  [4], [`captured_amount`], [numeric(18,2)], [Captured amount],
  [5], [`currency`], [varchar(3)], [ISO 4217 code],
  [6], [`state`], [text], [Checkout through Refunded],
  [7], [`response_code`], [varchar(255)], [Gateway code],
  [8], [`intent_client_secret`], [varchar(500)], [Stripe client secret],
  [9], [`stripe_session_id`], [varchar(255)], [Stripe Checkout Session id],
  [10], [`stripe_payment_intent_id`], [varchar(255)], [Stripe PaymentIntent id],
  [11], [`avs_response`], [varchar(10)], [Address verification result],
  [12], [`cvv_response_code`], [varchar(10)], [CVV check code],
  [13], [`cvv_response_message`], [varchar(255)], [CVV check message],
  [14], [`checkout_url`], [varchar(500)], [Stripe Checkout URL],
  [15], [`refunded_amount`], [numeric(18,2)], [Total refunded],
  [16], [`provider_key`], [varchar(50)], [Gateway id],
  [17], [`source_id`], [varchar(200)], [Stripe payment source id],
  [18], [`source_type`], [varchar(50)], [Payment source type],
  [19], [`last_stripe_event_id`], [varchar(255)], [Last processed webhook event],
  [20], [`processed_stripe_event_ids`], [jsonb], [Webhook idempotency log],
  [21], [`processed_at_utc`], [timestamptz], [System processing time],
  [22], [`completed_at_utc`], [timestamptz], [Payment completed],
  [23], [`failed_at_utc`], [timestamptz], [Payment failed],
  [24], [`voided_at_utc`], [timestamptz], [Payment voided],
  [25], [`refunded_at_utc`], [timestamptz], [Last refund recorded],
  [26], [`order_id`], [uuid], [FK to order (cascade)],
  [27], [`payment_method_id`], [uuid], [FK to method (set null)],
)

== Inventory Schema

==== Stock Locations

#db-table([Stock location. 1:N stock_items, stock_movements (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(255)], [Display name],
  [3], [`code`], [varchar(50)], [Unique code],
  [4], [`admin_name`], [varchar(255)], [Admin display name],
  [5], [`presentation`], [varchar(255)], [Customer display text],
  [6], [`address1`], [varchar(255)], [Street address line 1],
  [7], [`address2`], [varchar(255)], [Street address line 2],
  [8], [`city`], [varchar(100)], [City],
  [9], [`postal_code`], [varchar(10)], [Postal code],
  [10], [`phone`], [varchar(50)], [Phone],
  [11], [`country_id`], [uuid], [FK to country],
  [12], [`state_id`], [uuid], [FK to state],
  [13], [`active`], [bool], [Operational flag],
  [14], [`default`], [bool], [Default fulfillment],
  [15], [`backorderable_default`], [bool], [Allow backorders by default],
  [16], [`propagate_all_variants`], [bool], [Auto-create stock items],
  [17], [`position`], [int], [Display order],
  [18], [`low_stock_threshold`], [int], [Alert threshold],
  [19], [`notify_on_low_stock`], [bool], [Low-stock alerts],
  [20], [`is_deleted`], [bool], [Soft-delete],
)

==== Stock Items

#db-table([Stock item. Qty per variant per location. xmin concurrency. Unique (location_id, variant_id). 1:N stock_movements (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`count_on_hand`], [int], [On-hand qty],
  [3], [`backorderable`], [bool], [Zero-stock orders allowed],
  [4], [`stock_location_id`], [uuid], [FK to location],
  [5], [`variant_id`], [uuid], [FK to variant],
)

==== Stock Movements

#db-table([Stock movement. Immutable audit trail with balance snapshots.],
  [1], [`id`], [uuid], [PK],
  [2], [`quantity`], [int], [Signed delta],
  [3], [`previous_count_on_hand`], [int], [Balance before],
  [4], [`action`], [varchar(50)], [Movement type],
  [5], [`stock_item_id`], [uuid], [FK to stock item (cascade)],
  [6], [`stock_location_id`], [uuid], [FK to location (cascade)],
  [7], [`originator_id`], [uuid], [Polymorphic source id],
  [8], [`originator_type`], [varchar(200)], [Polymorphic source type],
  [9], [`reason`], [varchar(500)], [Adjustment reason],
)

==== Stock Reservations

#db-table([Stock reservation. Temp holds during checkout. xmin concurrency. Expires after configurable timeout.],
  [1], [`id`], [uuid], [PK],
  [2], [`variant_id`], [uuid], [FK to variant],
  [3], [`stock_location_id`], [uuid], [FK to location],
  [4], [`order_id`], [uuid], [FK to order; indexed with state],
  [5], [`quantity`], [int], [Reserved qty],
  [6], [`state`], [text], [Reserved, Fulfilled, Released, Expired; indexed],
  [7], [`expires_at_utc`], [timestamptz], [Auto-release timeout],
  [8], [`cart_token`], [text], [Guest cart id; indexed with state],
  [9], [`reason`], [varchar(500)], [Reservation reason],
)

==== Stock Transfers

#db-table([Stock transfer. Inter-location movement with state lifecycle. xmin concurrency.],
  [1], [`id`], [uuid], [PK],
  [2], [`number`], [varchar(50)], [Human-readable ref],
  [3], [`reference`], [varchar(255)], [External ref],
  [4], [`state`], [varchar(20)], [Created, In-Transit, Received, Cancelled; indexed],
  [5], [`source_location_id`], [uuid], [FK to source (restrict); indexed],
  [6], [`destination_location_id`], [uuid], [FK to destination (restrict); indexed],
)

==== Transfer Items

#db-table([Transfer item. Line items within a stock transfer.],
  [1], [`id`], [uuid], [PK],
  [2], [`stock_transfer_id`], [uuid], [FK to transfer (cascade)],
  [3], [`variant_id`], [uuid], [FK to variant],
  [4], [`quantity`], [int], [Requested qty],
  [5], [`received_quantity`], [int], [Received qty],
)

== Shipping Schema

==== Shipping Methods

#db-table([Shipping method. 1:N shipping_method_zones (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(255)], [Display name],
  [3], [`code`], [varchar(50)], [Unique code],
  [4], [`tracking_url`], [varchar(2048)], [Carrier tracking URL],
  [5], [`admin_name`], [varchar(255)], [Admin display name],
  [6], [`position`], [int], [Sort order],
  [7], [`available_to_users`], [bool], [Customer selectable],
  [8], [`calculator_type`], [varchar(100)], [Rate algorithm],
  [9], [`presentation`], [text], [Customer display text],
  [10], [`is_deleted`], [bool], [Soft-delete],
)

==== Shipping Rates

#db-table([Shipping rate. Weight/value tiered pricing per method.],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(255)], [Tier display name],
  [3], [`selected`], [bool], [Currently selected],
  [4], [`cost`], [numeric(18,2)], [Base cost],
  [5], [`final_price`], [numeric(18,2)], [Final price],
  [6], [`display_price`], [varchar(50)], [Formatted price],
  [7], [`delivery_range`], [varchar(100)], [Est. delivery],
  [8], [`min_weight`], [numeric(18,2)], [Min weight],
  [9], [`max_weight`], [numeric(18,2)], [Max weight],
  [10], [`free_shipping_threshold`], [numeric(18,2)], [Free shipping threshold],
  [11], [`shipping_method_id`], [uuid], [FK to method],
)

==== Shipping Method Zones

#db-table([Shipping method zone. Geographic restriction. Composite index on (method_id, country_code, state_code).],
  [1], [`id`], [uuid], [PK],
  [2], [`shipping_method_id`], [uuid], [FK to method (cascade)],
  [3], [`country_code`], [varchar(2)], [ISO 3166-1 alpha-2; composite index],
  [4], [`state_code`], [varchar(10)], [State code; composite index],
)

== Profile Schema

==== User Profiles

#db-table([User profile. 1:1 with users via shared PK. 1:N addresses (cascade).],
  [1], [`user_id`], [uuid], [PK; FK to user (1:1, cascade)],
  [2], [`first_name`], [varchar(100)], [Given name],
  [3], [`last_name`], [varchar(100)], [Family name],
  [4], [`email`], [varchar(255)], [Contact email],
  [5], [`phone_number`], [varchar(20)], [Phone],
  [6], [`date_of_birth`], [date], [Date of birth],
  [7], [`gender`], [varchar(20)], [Gender],
  [8], [`avatar_url`], [varchar(500)], [Avatar URL],
  [9], [`notifications_enable_email`], [bool], [Email notifications],
  [10], [`notifications_enable_sms`], [bool], [SMS notifications],
  [11], [`accepts_email_marketing`], [bool], [Marketing opt-in],
  [12], [`default_billing_address_id`], [uuid], [FK to billing address (set null)],
  [13], [`default_shipping_address_id`], [uuid], [FK to shipping address (set null)],
  [14], [`orders_count`], [int], [Total orders],
  [15], [`total_spent`], [numeric(18,2)], [Cumulative spend],
)

==== Addresses

#db-table([Address. Shipping and billing types with default designation.],
  [1], [`id`], [uuid], [PK],
  [2], [`address_type`], [text], [Shipping or Billing],
  [3], [`first_name`], [varchar(100)], [Recipient given name],
  [4], [`last_name`], [varchar(100)], [Recipient family name],
  [5], [`address1`], [varchar(200)], [Street line 1],
  [6], [`address2`], [varchar(200)], [Street line 2],
  [7], [`city`], [varchar(100)], [City],
  [8], [`zip_code`], [varchar(20)], [Postal code],
  [9], [`phone`], [varchar(20)], [Phone],
  [10], [`label`], [varchar(50)], [User label (e.g. Home)],
  [11], [`is_default`], [bool], [Default of its type],
  [12], [`country_name`], [varchar(100)], [Country name],
  [13], [`state_province`], [varchar(100)], [State name],
  [14], [`country_code`], [varchar(3)], [Country ISO; indexed],
  [15], [`state_code`], [varchar(10)], [State code],
  [16], [`user_profile_id`], [uuid], [FK to profile (cascade); indexed with address_type],
)

==== Wishlists

#db-table([Wishlist. FK to users. 1:N wished_items (cascade). Indexed (user_id, is_default).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(200)], [Display name],
  [3], [`token`], [varchar(128)], [Shareable token],
  [4], [`is_default`], [bool], [Default wishlist],
  [5], [`is_private`], [bool], [Hidden from public],
  [6], [`user_id`], [uuid], [FK to user (cascade); indexed with is_default],
  [7], [`is_deleted`], [bool], [Soft-delete],
)

==== Wished Items

#db-table([Wished item. Unique variant per wishlist.],
  [1], [`id`], [uuid], [PK],
  [2], [`quantity`], [int], [Desired qty],
  [3], [`variant_id`], [uuid], [FK to variant],
  [4], [`wishlist_id`], [uuid], [FK to wishlist (cascade)],
)

== Location Schema

==== Countries

#db-table([Country. ISO 3166-1 reference. 1:N states (cascade).],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(100)], [Display name],
  [3], [`iso_code`], [varchar(3)], [ISO 3166-1 alpha-2],
  [4], [`iso3code`], [text], [ISO 3166-1 alpha-3],
  [5], [`calling_code`], [varchar(10)], [Intl calling code],
  [6], [`states_required`], [bool], [State required],
  [7], [`zipcode_required`], [bool], [Postal code required],
  [8], [`is_active`], [bool], [Selectable],
)

==== States

#db-table([State. ISO 3166-2 reference. FK to countries.],
  [1], [`id`], [uuid], [PK],
  [2], [`name`], [varchar(100)], [Display name],
  [3], [`abbreviation`], [varchar(10)], [ISO 3166-2 code],
  [4], [`country_id`], [uuid], [FK to country (cascade)],
  [5], [`is_active`], [bool], [Selectable],
)

== pgvector Integration

Vector embeddings are stored in `catalog.image_embeddings` with column type `vector(512)`, made nullable in migration 20260804013350. An IVFFlat index with cosine distance (`vector_cosine_ops`) and `lists = 100` enables approximate nearest neighbour search. On PostgreSQL/Npgsql, the embedding maps to native `vector`; on other providers, it serializes as JSON via a value converter. Search queries use `<=>` cosine distance, ranking by `1 - cosine_distance`. Every embedding includes `model_name` and `model_version` for model-level filtering to ensure alignment between query and gallery embeddings.
