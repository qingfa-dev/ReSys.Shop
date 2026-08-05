= Database Schema <appendix-d>

The database uses PostgreSQL 17 with pgvector via EF Core 10. Five migrations, 33 `IEntityTypeConfiguration<T>` classes across eight bounded contexts. All tables: UUID PKs, `created_at_utc`/`modified_at_utc` audit stamps, soft-deletion with `is_deleted` and global query filters, row version columns for optimistic concurrency on contention-sensitive entities.

== Catalog Schema

==== products

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(255)], [Display name],
    [3], [`slug`], [varchar(255)], [Unique URL slug],
    [4], [`description`], [varchar(2000)], [Marketing description],
    [5], [`status`], [text], [Draft, Active, Archived],
    [6], [`style_code`], [text], [Style code],
    [7], [`season_name`], [text], [Season label],
    [8], [`material_composition`], [text], [Material notes],
    [9], [`department`], [text], [Department],
    [10], [`gender_target`], [text], [Target gender],
    [11], [`master_variant_id`], [uuid], [FK to master variant],
    [12], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Product. 1:N variants, product_option_types, classifications (cascade).],
)

==== variants

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`sku`], [varchar(255)], [Unique SKU],
    [3], [`is_master`], [bool], [Master variant flag],
    [4], [`position`], [int], [Display order],
    [5], [`barcode`], [text], [UPC/EAN barcode],
    [6], [`weight`], [numeric(18,2)], [Weight in kg],
    [7], [`price`], [numeric(18,2)], [Default price],
    [8], [`cost_price`], [numeric(18,2)], [Cost for margin],
    [9], [`product_id`], [uuid], [FK to product (cascade)],
    [10], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Variant. 1:N prices, product_images, option_value_variants (cascade).],
)

==== variant_images

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`url`], [varchar(2048)], [Public CDN URL],
    [3], [`storage_path`], [varchar(500)], [Object-storage key],
    [4], [`alt`], [varchar(500)], [Alt text],
    [5], [`content_type`], [varchar(100)], [MIME type],
    [6], [`file_name`], [varchar(255)], [Original filename],
    [7], [`file_size`], [int], [Size in bytes],
    [8], [`width`], [int], [Width in px],
    [9], [`height`], [int], [Height in px],
    [10], [`position`], [int], [Display order],
    [11], [`variant_id`], [uuid], [FK to variant (cascade)],
  ),
  kind: table,
  caption: [Variant image. 1:N image_embeddings (cascade).],
)

==== image_embeddings

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`model_name`], [varchar(100)], [Embedding model id],
    [3], [`model_version`], [varchar(50)], [Model version],
    [4], [`vector`], [vector(512)], [512-dim; IVFFlat cosine index (lists=100)],
    [5], [`dimensions`], [int], [Dimension count],
    [6], [`status`], [text], [Generation state],
    [7], [`hangfire_job_id`], [text], [Background job id],
    [8], [`error`], [text], [Error on failure],
    [9], [`variant_image_id`], [uuid], [FK to variant image (cascade)],
  ),
  kind: table,
  caption: [Image embedding. pgvector vector(512) with IVFFlat cosine distance index.],
)

==== option_types

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(100)], [Option key (e.g. size)],
    [3], [`presentation`], [varchar(100)], [Display text],
    [4], [`position`], [int], [Display order],
    [5], [`filterable`], [bool], [Usable as filter],
    [6], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Option type. 1:N option_values (cascade).],
)

==== option_values

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(100)], [Value key (e.g. s)],
    [3], [`presentation`], [varchar(100)], [Display text],
    [4], [`position`], [int], [Display order],
    [5], [`option_type_id`], [uuid], [FK to option type (cascade)],
  ),
  kind: table,
  caption: [Option value. FK to option_types.],
)

==== product_option_types

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`position`], [int], [Display order],
    [3], [`product_id`], [uuid], [FK to product],
    [4], [`option_type_id`], [uuid], [FK to option type],
  ),
  kind: table,
  caption: [Product-option type join. Unique on (product_id, option_type_id).],
)

==== option_value_variants

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`variant_id`], [uuid], [FK to variant],
    [3], [`option_value_id`], [uuid], [FK to option value],
  ),
  kind: table,
  caption: [Variant-option value join. Unique on (variant_id, option_value_id).],
)

==== taxonomies

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(100)], [Taxonomy key],
    [3], [`presentation`], [varchar(100)], [Display text],
    [4], [`position`], [int], [Display order],
    [5], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Taxonomy. 1:N taxa (cascade).],
)

==== taxa

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [Taxon. Nested set hierarchy. FK to taxonomies; self-ref FK for tree.],
)

==== taxon_rules

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`taxon_id`], [uuid], [FK to target taxon (cascade)],
    [3], [`type`], [varchar(50)], [Rule type],
    [4], [`value`], [varchar(255)], [Match value],
    [5], [`match_policy`], [varchar(50)], [All/any policy],
  ),
  kind: table,
  caption: [Taxon rule. Auto-classification rules. FK to taxa.],
)

==== classifications

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`position`], [int], [Display order],
    [3], [`is_automatic`], [bool], [Auto-assigned flag],
    [4], [`product_id`], [uuid], [FK to product],
    [5], [`taxon_id`], [uuid], [FK to taxon],
  ),
  kind: table,
  caption: [Classification. Product-taxon join. Unique pair.],
)

==== prices

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`amount`], [numeric(18,2)], [Selling price],
    [3], [`compare_at_amount`], [numeric(18,2)], [Strikethrough price],
    [4], [`currency`], [varchar(3)], [ISO 4217 code],
    [5], [`country_iso`], [varchar(2)], [Country override],
    [6], [`is_default`], [bool], [Default for variant],
    [7], [`variant_id`], [uuid], [FK to variant (cascade)],
    [8], [`price_list_id`], [uuid], [FK to price list],
  ),
  kind: table,
  caption: [Price. Time-bound pricing per variant.],
)

== Identity Schema

==== users

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`user_name`], [text], [Login username],
    [3], [`normalized_user_name`], [text], [Upper-cased; unique index],
    [4], [`email`], [text], [Email address],
    [5], [`normalized_email`], [text], [Upper-cased; indexed],
    [6], [`password_hash`], [text], [Salted hash],
    [7], [`security_stamp`], [text], [Invalidates sessions on change],
    [8], [`first_name`], [text], [Given name],
    [9], [`last_name`], [text], [Family name],
    [10], [`is_active`], [bool], [Account enabled],
    [11], [`last_login_at_utc`], [timestamptz], [Last login],
    [12], [`sign_in_count`], [int], [Login count],
  ),
  kind: table,
  caption: [User. Extends IdentityUser<Guid>. 1:1 user_profiles; 1:N refresh_tokens, passkeys, wishlists.],
)

==== refresh_tokens

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`token_hash`], [varchar(500)], [Salted hash; raw token not stored],
    [3], [`user_id`], [uuid], [FK to user],
    [4], [`token_family_id`], [uuid], [Groups rotated tokens],
    [5], [`expires_at_utc`], [timestamptz], [Expiration],
    [6], [`revoked_at_utc`], [timestamptz], [Revocation timestamp],
    [7], [`replaced_by_token_id`], [uuid], [Self-ref FK to next in chain],
    [8], [`device_id`], [text], [Client device id],
    [9], [`user_agent`], [text], [Client user-agent],
    [10], [`ip_address`], [text], [Client IP],
  ),
  kind: table,
  caption: [Refresh token. Single-use rotation with reuse detection. Self-ref FK for chain.],
)

==== roles

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [text], [Role name],
    [3], [`normalized_name`], [text], [Upper-cased; unique],
    [4], [`description`], [text], [Purpose],
    [5], [`is_system`], [bool], [Immutable system role],
  ),
  kind: table,
  caption: [Role. Extends IdentityRole<Guid>. N:M users via user_roles.],
)

==== user_roles

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`user_id`], [uuid], [FK to user; composite PK],
    [2], [`role_id`], [uuid], [FK to role; composite PK],
  ),
  kind: table,
  caption: [User-role join.],
)

==== user_claims

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [int], [Auto-increment PK],
    [2], [`user_id`], [uuid], [FK to user],
    [3], [`claim_type`], [text], [Claim type],
    [4], [`claim_value`], [text], [Claim value],
  ),
  kind: table,
  caption: [User claim. Direct permission grants per user.],
)

==== user_logins

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`login_provider`], [text], [Provider name; composite PK],
    [2], [`provider_key`], [text], [Provider user key; composite PK],
    [3], [`user_id`], [uuid], [FK to user],
    [4], [`provider_display_name`], [text], [Display name],
  ),
  kind: table,
  caption: [User login. External provider links (Google OAuth).],
)

==== user_tokens

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`user_id`], [uuid], [FK to user; composite PK],
    [2], [`login_provider`], [text], [Provider; composite PK],
    [3], [`name`], [text], [Purpose; composite PK],
    [4], [`value`], [text], [Token value],
  ),
  kind: table,
  caption: [User token. Password reset and email confirmation tokens.],
)

==== role_claims

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [int], [Auto-increment PK],
    [2], [`role_id`], [uuid], [FK to role],
    [3], [`claim_type`], [text], [Claim type],
    [4], [`claim_value`], [text], [Claim value],
  ),
  kind: table,
  caption: [Role claim. Permissions inherited by role members.],
)

==== passkeys

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`user_id`], [uuid], [FK to user],
    [3], [`credential_id`], [bytea], [WebAuthn credential id],
  ),
  kind: table,
  caption: [Passkey. FIDO2/WebAuthn passwordless auth.],
)

== Ordering Schema

==== orders

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`number`], [varchar(50)], [Human-readable order ref],
    [3], [`session_id`], [varchar(100)], [Guest cart session; indexed with status],
    [4], [`status`], [text], [Lifecycle state],
    [5], [`checkout_state`], [text], [Address, Delivery, Payment, Confirm, Complete],
    [6], [`currency`], [varchar(3)], [ISO 4217 code],
    [7], [`item_total`], [numeric(18,2)], [Sum of line items],
    [8], [`adjustment_total`], [numeric(18,2)], [Sum of adjustments],
    [9], [`shipment_total`], [numeric(18,2)], [Shipping total],
    [10], [`total`], [numeric(18,2)], [Grand total],
    [11], [`payment_total`], [numeric(18,2)], [Captured payments],
    [12], [`outstanding_balance`], [numeric(18,2)], [total - payment_total],
    [13], [`email`], [varchar(255)], [Contact email],
    [14], [`completed_at_utc`], [timestamptz], [Completion time],
    [15], [`canceled_at_utc`], [timestamptz], [Cancellation time],
    [16], [`user_id`], [uuid], [FK to user; indexed with status],
    [17], [`shipping_method_id`], [uuid], [FK to shipping method],
    [18], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Order. Forward-only checkout state machine. Indexed (user_id, status) and (session_id, status). 1:N line_items, adjustments (cascade).],
)

==== line_items

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`quantity`], [int], [Purchased qty],
    [3], [`price`], [numeric(18,2)], [Unit price snapshot],
    [4], [`total`], [numeric(18,2)], [Line total],
    [5], [`adjustment_total`], [numeric(18,2)], [Line adjustments],
    [6], [`currency`], [varchar(3)], [ISO 4217 code],
    [7], [`order_id`], [uuid], [FK to order (cascade)],
    [8], [`variant_id`], [uuid], [FK to variant (no action; snapshot)],
  ),
  kind: table,
  caption: [Line item. Price snapshots protect historical orders from catalog updates.],
)

==== adjustments

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [Adjustment. Polymorphic (tax, shipping, discount). FK to orders.],
)

== Payment Schema

==== payment_methods

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [Payment method. Preferences as jsonb. Settings encrypted. 1:N payment_captures (set null).],
)

==== payment_captures

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`number`], [varchar(50)], [Human-readable ref],
    [3], [`amount`], [numeric(18,2)], [Captured amount],
    [4], [`currency`], [varchar(3)], [ISO 4217 code],
    [5], [`state`], [text], [Checkout through Refunded],
    [6], [`response_code`], [varchar(255)], [Gateway code; indexed],
    [7], [`intent_client_secret`], [varchar(500)], [Stripe client secret],
    [8], [`refunded_amount`], [numeric(18,2)], [Total refunded],
    [9], [`provider_key`], [varchar(50)], [Gateway id],
    [10], [`processed_stripe_event_ids`], [jsonb], [Webhook idempotency log],
    [11], [`order_id`], [uuid], [FK to order (cascade)],
    [12], [`payment_method_id`], [uuid], [FK to method (set null)],
    [13], [`source_id`], [varchar(200)], [Stripe payment source id],
  ),
  kind: table,
  caption: [Payment capture. xmin for optimistic concurrency. Event ids as jsonb for idempotency.],
)

== Inventory Schema

==== stock_locations

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(255)], [Display name],
    [3], [`code`], [varchar(50)], [Unique code],
    [4], [`address1`], [varchar(255)], [Street address],
    [5], [`city`], [varchar(100)], [City],
    [6], [`postal_code`], [varchar(10)], [Postal code],
    [7], [`phone`], [varchar(50)], [Phone],
    [8], [`country_id`], [uuid], [FK to country],
    [9], [`state_id`], [uuid], [FK to state],
    [10], [`active`], [bool], [Operational flag],
    [11], [`default`], [bool], [Default fulfillment],
    [12], [`low_stock_threshold`], [int], [Alert threshold],
    [13], [`notify_on_low_stock`], [bool], [Low-stock alerts],
    [14], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Stock location. 1:N stock_items, stock_movements (cascade).],
)

==== stock_items

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`count_on_hand`], [int], [On-hand qty],
    [3], [`backorderable`], [bool], [Zero-stock orders allowed],
    [4], [`stock_location_id`], [uuid], [FK to location],
    [5], [`variant_id`], [uuid], [FK to variant],
  ),
  kind: table,
  caption: [Stock item. Qty per variant per location. xmin concurrency. Unique (location_id, variant_id). 1:N stock_movements (cascade).],
)

==== stock_movements

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`quantity`], [int], [Signed delta],
    [3], [`previous_count_on_hand`], [int], [Balance before],
    [4], [`action`], [varchar(50)], [Movement type],
    [5], [`stock_item_id`], [uuid], [FK to stock item (cascade)],
    [6], [`stock_location_id`], [uuid], [FK to location (cascade)],
    [7], [`originator_id`], [uuid], [Polymorphic source id],
    [8], [`originator_type`], [varchar(200)], [Polymorphic source type],
    [9], [`reason`], [varchar(500)], [Adjustment reason],
  ),
  kind: table,
  caption: [Stock movement. Immutable audit trail with balance snapshots.],
)

==== stock_reservations

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`variant_id`], [uuid], [FK to variant],
    [3], [`stock_location_id`], [uuid], [FK to location],
    [4], [`order_id`], [uuid], [FK to order; indexed with state],
    [5], [`quantity`], [int], [Reserved qty],
    [6], [`state`], [text], [Lifecycle; indexed],
    [7], [`expires_at_utc`], [timestamptz], [Auto-release timeout],
    [8], [`cart_token`], [text], [Guest cart id; indexed with state],
  ),
  kind: table,
  caption: [Stock reservation. Temp holds during checkout. xmin concurrency. Expires after configurable timeout.],
)

==== stock_transfers

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`number`], [varchar(50)], [Human-readable ref],
    [3], [`reference`], [varchar(255)], [External ref],
    [4], [`state`], [varchar(20)], [Created, In-Transit, Received, Cancelled; indexed],
    [5], [`source_location_id`], [uuid], [FK to source (restrict); indexed],
    [6], [`destination_location_id`], [uuid], [FK to destination (restrict); indexed],
  ),
  kind: table,
  caption: [Stock transfer. Inter-location movement with state lifecycle. xmin concurrency.],
)

==== transfer_items

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`stock_transfer_id`], [uuid], [FK to transfer (cascade)],
    [3], [`variant_id`], [uuid], [FK to variant],
    [4], [`quantity`], [int], [Requested qty],
    [5], [`received_quantity`], [int], [Received qty],
  ),
  kind: table,
  caption: [Transfer item. Line items within a stock transfer.],
)

== Shipping Schema

==== shipping_methods

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [Shipping method. 1:N shipping_method_zones (cascade).],
)

==== shipping_rates

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [Shipping rate. Weight/value tiered pricing per method.],
)

==== shipping_method_zones

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`shipping_method_id`], [uuid], [FK to method (cascade)],
    [3], [`country_code`], [varchar(2)], [ISO 3166-1 alpha-2; composite index],
    [4], [`state_code`], [varchar(10)], [State code; composite index],
  ),
  kind: table,
  caption: [Shipping method zone. Geographic restriction. Composite index on (method_id, country_code, state_code).],
)

== Profile Schema

==== user_profiles

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [User profile. 1:1 with users via shared PK. 1:N addresses (cascade).],
)

==== addresses

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
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
  ),
  kind: table,
  caption: [Address. Shipping and billing types with default designation.],
)

==== wishlists

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(200)], [Display name],
    [3], [`token`], [varchar(128)], [Shareable token],
    [4], [`is_default`], [bool], [Default wishlist],
    [5], [`is_private`], [bool], [Hidden from public],
    [6], [`user_id`], [uuid], [FK to user (cascade); indexed with is_default],
    [7], [`is_deleted`], [bool], [Soft-delete],
  ),
  kind: table,
  caption: [Wishlist. FK to users. 1:N wished_items (cascade). Indexed (user_id, is_default).],
)

==== wished_items

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`quantity`], [int], [Desired qty],
    [3], [`variant_id`], [uuid], [FK to variant],
    [4], [`wishlist_id`], [uuid], [FK to wishlist (cascade)],
  ),
  kind: table,
  caption: [Wished item. Unique variant per wishlist.],
)

== Location Schema

==== countries

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(100)], [Display name],
    [3], [`iso_code`], [varchar(3)], [ISO 3166-1 alpha-2],
    [4], [`iso3code`], [text], [ISO 3166-1 alpha-3],
    [5], [`calling_code`], [varchar(10)], [Intl calling code],
    [6], [`states_required`], [bool], [State required],
    [7], [`zipcode_required`], [bool], [Postal code required],
    [8], [`is_active`], [bool], [Selectable],
  ),
  kind: table,
  caption: [Country. ISO 3166-1 reference. 1:N states (cascade).],
)

==== states

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left, left),
    inset: 5pt,
    table.header([*No*], [*Field name*], [*Data type*], [*Description*]),
    [1], [`id`], [uuid], [PK],
    [2], [`name`], [varchar(100)], [Display name],
    [3], [`abbreviation`], [varchar(10)], [ISO 3166-2 code],
    [4], [`country_id`], [uuid], [FK to country (cascade)],
    [5], [`is_active`], [bool], [Selectable],
  ),
  kind: table,
  caption: [State. ISO 3166-2 reference. FK to countries.],
)

== pgvector Integration

Vector embeddings are stored in `catalog.image_embeddings` with column type `vector(512)`, made nullable in migration 20260804013350. An IVFFlat index with cosine distance (`vector_cosine_ops`) and `lists = 100` enables approximate nearest neighbour search. On PostgreSQL/Npgsql, the embedding maps to native `vector`; on other providers, it serializes as JSON via a value converter. Search queries use `<=>` cosine distance, ranking by `1 - cosine_distance`. Every embedding includes `model_name` and `model_version` for model-level filtering to ensure alignment between query and gallery embeddings.
