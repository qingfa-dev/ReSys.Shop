# Chapter 5 — Database Design

## 5.1 Database Technology Selection

**Decision**: PostgreSQL 17 with the `pgvector` extension.

**Justification**:

| Criterion | PostgreSQL + pgvector | Alternative (separate vector DB like Pinecone/Milvus) |
|-----------|----------------------|--------------------------------------------------------|
| **ACID compliance** | Native — checkout + inventory in same transaction | Vector DB is eventually consistent; cross-DB transactions require Saga pattern |
| **Operational complexity** | One database to manage, backup, monitor | Two databases + synchronization logic |
| **Query flexibility** | SQL + vector similarity in one query (`ORDER BY embedding <=> $1`) | Requires application-level join or dual queries |
| **Cost** | Open source, runs locally via Aspire | SaaS pricing or additional self-hosted infrastructure |
| **Thesis demonstrability** | Single `docker run pgvector/pgvector:pg17-trixie` | Extra setup steps for examiners |

Trade-off: pgvector's HNSW/IVFFlat indexes are not as optimized as dedicated vector databases for billion-scale vectors. This is acceptable because a thesis catalog contains thousands, not billions, of products.

## 5.1a Normalization Design

The database schema is designed in **Third Normal Form (3NF)** with one intentional denormalization. Every table has a single-column surrogate primary key (`uuid id`), no repeating groups, and no transitive dependencies on non-key attributes. Foreign keys reference the surrogate key of their parent table, eliminating update anomalies.

The sole intentional denormalization is the `order.total` column, which stores the pre-computed sum of `item_total + adjustment_total + shipment_total`. This is a **read-optimized denormalization**: the total is recalculated on every write (checkout, refund, adjustment) by the `Order.Method.Checkout.cs` domain method, but stored redundantly to avoid recalculation during the high-frequency read path (order listing, checkout confirmation, admin dashboard). This trade-off is justified by the fact that orders are written far less frequently than they are read, and the recalculation logic is centralized in the domain layer per DDD principles — not scattered in SQL triggers or application queries.

All other computed fields (e.g., `order.item_count`, `variant_image.position`) are maintained by EF Core interceptors or domain methods at write time, preserving 3NF integrity while optimizing read performance.

**Evidence**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs:11-12`, `Shared/Operational/Persistence/Data/AppDbContext.cs:74-76` (`builder.HasPostgresExtension("vector")`), `Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`

## 5.2 Entity Relationship Diagram (ERD)

### 5.2.1 High-Level ERD (Core Business Entities)

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│    User     │       │   Product   │       │    Order    │
│  (Identity) │───────┤             │       │             │
│  id (PK)    │ 1   1 │  id (PK)    │       │  id (PK)    │
│  email      │       │  name       │       │  number     │
│  ...        │       │  slug (UQ)  │       │  status     │
└─────────────┘       │  master_var │───────┤  user_id(FK)│
                      │  ...        │       │  total      │
                      └──────┬──────┘       │  ...        │
                             │ 1            └──────┬──────┘
                             │                     │
                      ┌──────┴──────┐       ┌──────┴──────┐
                      │   Variant   │       │  LineItem   │
                      │  id (PK)    │       │  id (PK)    │
                      │  product_id │       │  order_id   │
                      │  sku (UQ)   │       │  variant_id   │
                      │  price      │       │  quantity   │
                      │  ...        │       │  price      │
                      └──────┬──────┘       └─────────────┘
                             │
                      ┌──────┴──────┐
                      │ VariantImage  │
                      │  id (PK)      │
                      │  variant_id   │
                      │  embedding    │  ← pgvector type (vector(512))
                      │  ...          │
                      └───────────────┘
```

### 5.2.2 Identity ERD (ASP.NET Identity Tables)

```
┌─────────┐     ┌───────────┐     ┌───────────┐
│  Users  │─────┤ UserRoles │─────┤   Roles   │
│ (PK:id) │ 1:n │ (composite)│ n:1 │ (PK:id)   │
└─────────┘     └───────────┘     └───────────┘
     │
     │ 1:n        ┌───────────┐     ┌───────────┐
     ├────────────┤UserClaims │     │RoleClaims │
     │            └───────────┘     └───────────┘
     │
     │ 1:n        ┌───────────┐     ┌───────────┐
     ├────────────┤UserLogins │     │UserTokens │
     │            └───────────┘     └───────────┘
     │
     │ 1:n        ┌───────────┐
     └────────────┤UserPasskeys│
                  └───────────┘
```

### 5.2.3 Profile ERD

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│  UserProfile  │       │   Address   │       │   Wishlist  │
│  id (PK)      │───────┤  id (PK)    │       │  id (PK)    │
│  user_id (UQ) │ 1:n   │ profile_id  │       │ profile_id  │
│  ...          │       │ ...         │       │ ...         │
└─────────────┘       └─────────────┘       └──────┬──────┘
                                                    │
                                             ┌──────┴──────┐
                                             │  WishedItem   │
                                             │  id (PK)      │
                                             │  wishlist_id  │
                                             │  variant_id   │
                                             └───────────────┘
```

### 5.2.4 Inventory ERD

```
┌───────────────┐     ┌───────────┐     ┌───────────────┐
│ StockLocation │─────┤ StockItem │─────┤ StockMovement │
│ id (PK)       │ 1:n │ id (PK)   │ 1:n │ id (PK)       │
│ name          │     │ location_id│     │ stock_item_id │
│ ...           │     │ variant_id│     │ quantity_delta│
└───────────────┘     │ quantity  │     │ ...           │
                      └───────────┘     └───────────────┘
```

## 5.3 Schema Organization

The database uses **per-module schemas** to provide logical separation within the shared PostgreSQL database:

| Schema | Tables | Evidence |
|--------|--------|----------|
| `catalog` | `products`, `variants`, `variant_images`, `option_types`, `option_values`, `taxonomies`, `taxons`, `classifications` | `Module/Catalog/Persistence/CatalogSchema.cs:1-31` |
| `ordering` | `orders`, `line_items`, `adjustments` | `Module/Ordering/Persistence/OrderingSchema.cs` |
| `payment` | `payment_intents`, `payment_captures`, `payment_methods` | `Module/Payment/Persistence/PaymentSchema.cs` |
| `inventory` | `stock_locations`, `stock_items`, `stock_movements`, `stock_reservations`, `stock_transfers` | `Module/Inventory/Persistence/InventorySchema.cs` |
| `shipping` | `shipping_methods`, `shipping_rates` | `Module/Shipping/Persistence/ShippingSchema.cs` |
| `profile` | `user_profiles`, `addresses`, `wishlists`, `wished_items`, `notification_preferences` | `Module/Profile/Persistence/ProfileSchema.cs` |
| `location` | `countries`, `states` | `Module/Location/Persistence/LocationSchema.cs` |
| `identity` | ASP.NET Identity tables (`users`, `roles`, `user_roles`, `user_claims`, `role_claims`, `user_logins`, `user_tokens`, `user_passkeys`) | `Shared/Security/Identity/Identity.Extension.cs` |

**Design rationale**: Schemas provide namespace isolation without the operational cost of separate databases. A DBA can back up or grant permissions per schema. EF Core supports schema mapping via `ToTable("name", "schema")` in entity configurations.

## 5.4 Key Tables and Columns

### 5.4.1 Product and Variant

| Table | Key Columns | Constraints | Indexes |
|-------|-------------|-------------|---------|
| `catalog.products` | `id`, `name`, `slug`, `status`, `master_variant_id`, `available_on`, `discontinue_on`, `style_code`, `season_name` | `slug` unique | `[slug]`, `[status]` |
| `catalog.variants` | `id`, `product_id`, `sku`, `is_master`, `price`, `cost_price`, `track_inventory`, `weight`, `height`, `width`, `depth` | `sku` unique, FK to `products` | `[product_id]`, `[sku]` |
| `catalog.variant_images` | `id`, `variant_id`, `file_path`, `alt_text`, `position`, `embedding` | FK to `variants` | `[variant_id]`, `embedding USING ivfflat` (pgvector) |

### 5.4.2 Order

| Table | Key Columns | Constraints | Indexes |
|-------|-------------|-------------|---------|
| `ordering.orders` | `id`, `number`, `session_id`, `user_id`, `store_id`, `status`, `checkout_state`, `currency`, `item_total`, `adjustment_total`, `shipment_total`, `total`, `payment_total`, `outstanding_balance` | `number` unique | **Composite**: `(user_id, status)`, `(session_id, status)` — added in migration `20260713131410_OrderingIndexAndFkFixes` |
| `ordering.line_items` | `id`, `order_id`, `variant_id`, `quantity`, `price`, `total` | FK to `orders` (NoAction to avoid cascade cycles) | `[order_id]` |

**Design decision**: The migration `20260713131410_OrderingIndexAndFkFixes` changed the `LineItem` → `Variant` foreign key to `NoAction` because cascading deletes from `orders` → `line_items` → `variants` would create a multi-hop cascade cycle (PostgreSQL restriction).

**Evidence**: `git log: commit bd042088`

### 5.4.3 Identity

The identity schema extends ASP.NET Identity with custom `User`, `Role`, and `UserPasskey` entities. The `ApplicationDbContext` inherits from `IdentityDbContext<User, Role, Guid, ...>` using `Guid` as the key type throughout.

**Evidence**: `ApplicationDbContext.cs:28`

## 5.5 pgvector Integration

### 5.5.1 Vector Column Configuration

```csharp
// From Vector.Configuration.cs
builder.Entity<VariantImage>()
    .Property(v => v.Embedding)
    .HasColumnType("vector(512)");  // Fashion-CLIP output dimension
```

### 5.5.2 Similarity Query

```sql
SELECT vi.*, v.sku, p.name
FROM catalog.variant_images vi
JOIN catalog.variants v ON vi.variant_id = v.id
JOIN catalog.products p ON v.product_id = p.id
ORDER BY vi.embedding <=> @query_embedding  -- cosine distance
LIMIT 20;
```

**Design decision**: Cosine similarity (`<=>` operator in pgvector) is used because Fashion-CLIP embeddings are normalized L2 vectors where cosine distance is equivalent to Euclidean distance and performs well for visual similarity.

**Evidence**: `Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs`, `ImageEmbedding.Inference.cs` (returns 512-d vector)

## 5.6 Indexing Strategy

| Index | Purpose | Evidence |
|-------|---------|----------|
| `slug` UNIQUE on `products` | Fast lookup by SEO-friendly URL | `CatalogSchema.cs` |
| `sku` UNIQUE on `variants` | Inventory lookup by SKU | `CatalogSchema.cs` |
| `number` UNIQUE on `orders` | Order lookup by human-readable number | `OrderingSchema.cs` |
| Composite `(user_id, status)` on `orders` | My-orders queries | Migration `20260713131410` |
| Composite `(session_id, status)` on `orders` | Guest cart retrieval | Migration `20260713131410` |
| `embedding` IVFFlat on `variant_images` | Approximate nearest neighbor search | `Vector.Configuration.cs` |

## 5.7 Migration Strategy

EF Core migrations are stored in a **separate assembly** (`Api.Migrations`) to keep the domain and host projects free of migration clutter.

**Current migrations**:

| Migration | Description | Size |
|-----------|-------------|------|
| `20260711090657_InitialCreate` | Baseline schema for all 8 modules + Identity | ~104 KB (C#), ~153 KB (Designer) |
| `20260712050728_FixPaymentMethodSettingsColumnType` | Changed `payment.payment_method.settings` from `jsonb` to `text` | Small fix |
| `20260713131410_OrderingIndexAndFkFixes` | Added composite indexes; changed `LineItem-Variant` FK to `NoAction` | ~152 KB (Designer) |

**Design rationale**: Large initial migrations are expected when bootstrapping a modular schema. The fix migration demonstrates the project's iterative approach: schema changes are versioned and reversible.

**Evidence**: `service/Api/src/Migrations/Migrations/`

## 5.8 Evidence

- `service/Api/src/Shared/Operational/Persistence/Data/AppDbContext.cs:1-113` — DbContext with pgvector extension, schema configuration discovery
- `service/Api/src/Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs` — pgvector column setup
- `service/Api/src/Module/Catalog/Persistence/CatalogSchema.cs` — schema constants
- `service/Api/src/Migrations/Migrations/20260711090657_InitialCreate.cs` — baseline migration
- `service/Api/src/Migrations/Migrations/20260713131410_OrderingIndexAndFkFixes.cs` — index optimization
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:11-12` — PostgreSQL 17 + pgvector image selection
- `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.cs` — entity with `Embedding` property

---

## [ASK USER] Items

9. Should the ERD be produced as a formal diagram (e.g., using dbdiagram.io / PlantUML), or are the ASCII/text representations in this document sufficient?
10. Does the examiner expect a discussion of normalization form (3NF, BCNF) and denormalization decisions (e.g., `Order.Total` stored as computed column)?
