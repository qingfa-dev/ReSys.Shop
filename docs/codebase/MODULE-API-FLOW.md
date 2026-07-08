---
goal: Full Module Flow, Endpoint Reference, and Cross-Module Integration Specification
version: 2.0
date_created: 2026-07-08
last_updated: 2026-07-08
owner: Platform Team
status: 'Completed'
tags: reference, architecture, api, flow, integration, endpoints
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Complete specification of ReSys.Shop's module architecture: endpoint inventory with source paths, request/response shapes, cross-module integration points, authentication patterns, and usage examples. Covers 9 modules (Catalog, Identity, Profile, Location, Payment, Shipping, Inventory, Ordering, Webhooks) and 248 API endpoints across 20 database schemas and 40+ feature folders.

## Quick Reference

| Module | Schema | Source Root | Storefront Endpoints | Admin Endpoints | Seeders |
|--------|--------|-------------|---------------------|-----------------|---------|
| Location | `location` | `Module/Location/Features/` | 6 | 12 | 2 |
| Identity | `identity` | `Module/Identity/Features/` + `Shared/Security/Identity/` | 13 | 25 | 2 |
| Profile | `profile` | `Module/Profile/Features/` | 18 | 0 | 2 |
| Catalog | `catalog` | `Module/Catalog/Features/` | 11 | 65 | 4 |
| Ordering | `ordering` | `Module/Ordering/Features/` | 16 | 17 | 2 |
| Payment | `payment` | `Module/Payment/Features/` | 5 | 12 | 2 |
| Shipping | `shipping` | `Module/Shipping/Features/` | 3 | 12 | 2 |
| Inventory | `inventory` | `Module/Inventory/Features/` | 3 | 24 | 3 |
| Webhooks | `operational` | `Shared/Operational/Webhooks/` | 0 | 6 | 0 |

---

## 1. Architecture Overview

### 1.1 Module Dependency Graph

```
                    ┌──────────────┐
                    │   Location   │  (standalone reference data)
                    └──────┬───────┘
                           │ Country, State FK
           ┌───────────────┼───────────────────┐
           ▼               ▼                   ▼
     ┌──────────┐  ┌──────────────┐  ┌────────────────┐
     │ Catalog  │  │   Profile    │  │   Inventory    │
     │(products,│  │(addresses,   │  │(stock tracking)│
     │ variants,│  │ wishlists)   │  └───────┬────────┘
     │ taxons)  │  └───────┬──────┘          │
     └────┬─────┘          │                  │
          │ Variant FK     │ Address FK       │ StockItem FK
          ▼                ▼                  ▼
     ┌───────────────────────────────────────────┐
     │                Ordering                    │
     │  (Cart, Orders, LineItems, Checkout)       │
     └────┬──────────────┬──────────────┬─────────┘
          │              │              │
          ▼              ▼              ▼
     ┌────────┐  ┌────────────┐  ┌──────────────┐
     │Payment │  │  Shipping  │  │  Webhooks    │
     │(txns,  │  │ (methods,  │  │ (outbound    │
     │ intents│  │  rates)    │  │  event bus)  │
     └────────┘  └────────────┘  └──────────────┘
```

### 1.2 Integration Patterns

| Pattern | Example | Mechanism |
|---------|---------|-----------|
| **Cross-module FK** | `LineItem.VariantId` references `Catalog.Variant.Id` | Direct GUID FK in shared DbContext (`IApplicationDbContext`), table-per-schema |
| **Seeded dependency** | `AddressSeeder` queries `Country` and `State` from Location | Seeders call `Context.Set<T>()` on shared DbContext — cross-module queries allowed |
| **Event bus** | `IOrderEventPublisher` → `WebhookOrderEventPublisher` → `IWebhookDispatcher` | DI swap in `Program.cs:49`: `NullOrderEventPublisher` replaced by `WebhookOrderEventPublisher` |
| **Background service** | `CartExpiryService` polls expired carts, `WebhookDeliveryBackgroundService` delivers webhooks | `BackgroundService` implementations with `IServiceScopeFactory` sweep every 1-5 min |
| **Shared permissions** | `[HasPermission]` on every admin endpoint | `PermissionMetadata` classes define `admin.{module}.{resource}.{action}` identifiers resolved via `PermissionStore` (backed by `RoleClaim` + `UserPermission` tables) |

### 1.3 Route Namespace Convention

| Scope | Pattern | Example |
|-------|---------|---------|
| Admin CRUD | `api/{module}/{resource}` | `api/catalog/products` |
| Admin nested | `api/{module}/{parent}/{parentId}/{child}` | `api/catalog/taxonomies/{id}/taxons/{id}/rules` |
| Storefront public | `api/storefront/{resource}` | `api/storefront/products` |
| Store profile (auth) | `api/store/profiles/{resource}` | `api/store/profiles/addresses` |
| Store identity | `api/store/identity/{feature}/{action}` | `api/store/identity/auth/login/password` |
| Webhooks admin | `api/webhooks/{resource}` | `api/webhooks/subscriptions` |

### 1.4 Common Response Envelope

Every endpoint returns the `Result<T>` envelope:
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "value": { ... },          // payload on success (or absent for void actions)
  "errors": [],              // error details on failure
  "failures": [],            // validation failures
  "firstFailure": null       // first error shortcut
}
```

Paged endpoints return `PagedResult<T>`:
```json
{
  "isSuccess": true,
  "items": [ ... ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### 1.5 Common Query Parameters (Pagination, Filtering, Sorting)

Modules that implement `FilterSortAndPagedQuery<T>` support these query params:

| Param | Type | Example | Description |
|-------|------|---------|-------------|
| `page` | int | `page=1` | Page number (1-indexed) |
| `pageSize` | int | `pageSize=20` | Items per page (default varies, typically 10-50) |
| `search` | string | `search=shirt` | Full-text search across allowed fields |
| `sortBy` | string | `sortBy=name` | Field to sort by |
| `sortDirection` | asc/desc | `sortDirection=asc` | Sort direction |
| `{field}` | varies | `active=true`, `isDeleted=false` | Direct field filters |

---

## 2. Module Deep Dives

### 2.1 Location Module

**Source**: `service/Api/src/Module/Location/Features/`  
**Schema**: `location` — Tables: `countries`, `states`  
**Purpose**: Reference data for countries and states/provinces.

**Entity Model**:
```
Country (1) ──→ State (N)    ← CountryId FK
  isoCode (unique), iso3, name, phoneCode
  └── State
       abbreviation (unique per country), name
```

**Seeded Data**: US (+1) + 50 states, Vietnam (+84) + 63 provinces.

#### Storefront Endpoints

Public — no auth required. Source: `Features/{Admin,Storefront}/Countries/` and `States/`.

| Method | Route | Source Folder | Codes | Description |
|--------|-------|---------------|-------|-------------|
| `GET` | `api/store/locations/countries` | `Storefront/Countries/List/` | 200 | List all countries (paged/searchable) |
| `GET` | `api/store/locations/countries/{id:guid}` | `Storefront/Countries/GetById/` | 200, 404 | Get country by UUID |
| `GET` | `api/store/locations/countries/by-iso/{isoCode}` | `Storefront/Countries/GetByIso/` | 200, 404 | Lookup by ISO code (`US`, `VN`) |
| `GET` | `api/store/locations/states` | `Storefront/States/List/` | 200 | List states (paged, filterable by `countryId`) |
| `GET` | `api/store/locations/states/{id:guid}` | `Storefront/States/GetById/` | 200, 404 | Get state by UUID |
| `GET` | `api/store/locations/states/by-iso/{isoCode}` | `Storefront/States/GetByIso/` | 200, 404 | Lookup by abbreviation (`NY`, `CA`) |

#### Admin Endpoints

All require permission. Source: `Features/Admin/Countries/` and `States/`.

**Countries**:

| Method | Route | Source Folder | Permission | Description |
|--------|-------|---------------|------------|-------------|
| `POST` | `api/locations/countries` | `Countries/Create/` | `admin.location.countries.create` | Body: `{ name, isoCode, iso3?, phoneCode? }` |
| `GET` | `api/locations/countries` | `Countries/List/` | `admin.location.countries.list` | Paged, searchable by name/isoCode |
| `GET` | `api/locations/countries/{id:guid}` | `Countries/GetById/` | `admin.location.countries.detail` | Full country detail |
| `GET` | `api/locations/countries/by-iso/{isoCode}` | `Countries/GetByIso/` | `admin.location.countries.detail` | Lookup by ISO |
| `PUT` | `api/locations/countries/{id:guid}` | `Countries/Update/` | `admin.location.countries.update` | Partial update |
| `DELETE` | `api/locations/countries/{id:guid}` | `Countries/Delete/` | `admin.location.countries.delete` | Soft delete |

**States**:

| Method | Route | Source Folder | Permission | Description |
|--------|-------|---------------|------------|-------------|
| `POST` | `api/locations/states` | `States/Create/` | `admin.location.states.create` | Body: `{ name, abbreviation, countryId }` |
| `GET` | `api/locations/states` | `States/List/` | `admin.location.states.list` | Paged, filter by `countryId` |
| `GET` | `api/locations/states/{id:guid}` | `States/GetById/` | `admin.location.states.detail` | Detail |
| `GET` | `api/locations/states/by-iso/{isoCode}` | `States/GetByIso/` | `admin.location.states.detail` | Lookup |
| `PUT` | `api/locations/states/{id:guid}` | `States/Update/` | `admin.location.states.update` | Update |
| `DELETE` | `api/locations/states/{id:guid}` | `States/Delete/` | `admin.location.states.delete` | Soft delete |

**Usage** — Fetch states for a checkout dropdown:
```
GET /api/store/locations/states?countryId={us_id}&pageSize=50
→ 200 { "items": [{ "id": "...", "name": "New York", "abbreviation": "NY" }, ...], "totalCount": 50 }
```

#### Integration
- **Consumed by**: Profile (`Address.countryCode`), Inventory (`StockLocation.countryId`)
- **Seeded**: `CountrySeeder` (10) → `StateSeeder` (20)

---

### 2.2 Identity Module

**Source**: `service/Api/src/Module/Identity/Features/` + `Shared/Security/Identity/Features/`  
**Schema**: `identity` — Tables: `users`, `roles`, `user_roles`, `role_claims`, `user_permissions`  
**Purpose**: Auth (login/register/JWT), user/role/permission management.

**Entity Model**:
```
User (ASP.NET Identity)
  ├── UserRole → Role
  │               └── RoleClaim (permission)    ← `admin.{module}.{resource}.{action}` string
  └── UserPermission (bypasses role)
```

**Auth Config** (dev): `RequireConfirmedEmail = false`, `RequireConfirmedAccount = false` (in `IdentitySetting.Constant.cs`)

**Seeded Data**: 3 roles (Admin, Manager, User) + 7 users (admin, 3 managers, 3 customers).

#### Authentication Flow

```
┌──────────┐     ┌─────────────────────────┐     ┌─────────────┐
│  Client  │────>│ POST /auth/login/password │────>│  JWT Token  │
│          │<────│ { credential, password }  │<────│  + Refresh  │
└──────────┘     └─────────────────────────┘     └──────┬──────┘
                                                         │
              ┌──────────────────────────────────────────┘
              ▼
     All subsequent requests include:
     Authorization: Bearer {accessToken}
```

**OAuth Scaffolds** (not fully configured): Facebook, Microsoft providers registered.

#### Storefront Endpoints

Source: `Features/{Storefront/Auth/,Passwords/,Emails/}`. Auth varies.

| Method | Route | Source | Auth | Codes | Description |
|--------|-------|--------|------|-------|-------------|
| `POST` | `api/store/identity/auth/login/password` | `Auth/Login/Password/` | No | 200, 401 | Body: `{ credential, password }`. Returns `{ accessToken, refreshToken, expiresAt }` |
| `POST` | `api/store/identity/auth/login/external` | `Auth/Login/External/` | No | 200, 401 | OAuth login callback |
| `GET` | `api/store/identity/auth/login/external/providers` | `Auth/Login/Providers/` | No | 200 | Returns `[{ name, url }]` |
| `POST` | `api/store/identity/auth/register` | `Auth/Register/` | No | 200, 400, 409 | Body: `{ email, userName, password, firstName, lastName, acceptTerm }` |
| `POST` | `api/store/identity/auth/logout` | `Auth/Logout/` | Yes | 200 | Invalidate session |
| `GET` | `api/store/identity/auth/sessions` | `Auth/Sessions/` | Yes | 200 | Current session info |
| `POST` | `api/store/identity/auth/sessions/refresh` | `Auth/Sessions/Refresh/` | No | 200, 401 | Body: `{ refreshToken }` |
| `POST` | `api/store/identity/passwords/change` | `Passwords/Change/` | Yes | 200, 400 | Body: `{ currentPassword, newPassword }` |
| `POST` | `api/store/identity/passwords/forgot` | `Passwords/Forgot/` | No | 200 | Body: `{ email }`. Triggers email |
| `POST` | `api/store/identity/passwords/reset` | `Passwords/Reset/` | No | 200, 400 | Body: `{ email, token, newPassword }` |
| `POST` | `api/store/identity/emails/change` | `Emails/Change/` | Yes | 200, 400 | Body: `{ newEmail }` |
| `POST` | `api/store/identity/emails/confirm` | `Emails/Confirm/` | No | 200, 400 | Body: `{ email, token }` |
| `POST` | `api/store/identity/emails/resend` | `Emails/Resend/` | No | 200 | Body: `{ email }` |

**Usage** — Register and login:
```
POST /api/store/identity/auth/register
{ "email": "test@example.com", "userName": "testuser", "password": "TestUser123!",
  "firstName": "Test", "lastName": "User", "acceptTerm": true }
→ 201 { "isSuccess": true, "value": { "userName": "testuser", "email": "test@example.com" } }

POST /api/store/identity/auth/login/password
{ "credential": "test@example.com", "password": "TestUser123!" }
→ 200 { "isSuccess": true, "value": { "accessToken": "eyJ...", "expiresAt": "2026-07-09T00:00:00Z",
      "refreshToken": "...", "tokenType": "Bearer" } }
```

#### Admin Endpoints — Users

Source: `Features/Admin/Users/`. All require `admin.identity.users.*`.

| Method | Route | Subfolder | Permission | Body / Notes |
|--------|-------|-----------|------------|--------------|
| `POST` | `api/identity/users` | `Create/` | `admin.identity.users.create` | `{ email, userName, password, firstName, lastName, roles?: [guid] }` |
| `GET` | `api/identity/users` | `List/` | `admin.identity.users.list` | Paged, searchable by email/username |
| `GET` | `api/identity/users/{id:guid}` | `GetById/` | `admin.identity.users.list` | Roles + permissions included |
| `PUT` | `api/identity/users/{id:guid}` | `Update/` | `admin.identity.users.update` | Body: `{ firstName?, lastName?, email?, phoneNumber? }` |
| `DELETE` | `api/identity/users/{id:guid}` | `Delete/` | `admin.identity.users.delete` | Soft delete |
| `PATCH` | `api/identity/users/{id:guid}/status` | `ToggleStatus/` | `admin.identity.users.update` | Body: `{ isActive: bool }` |

#### Admin Endpoints — User Roles & Permissions

Source: `Features/Admin/UserRoles/` and `UserPermissions/`.

| Method | Route | Permission |
|--------|-------|------------|
| `GET` | `api/identity/users/{id}/roles` | `admin.identity.users.list` |
| `POST` | `api/identity/users/{id}/roles/assign` | `admin.identity.users_roles.assign` |
| `DELETE` | `api/identity/users/{id}/roles/revoke` | `admin.identity.users_roles.revoke` |
| `PATCH` | `api/identity/users/{id}/roles/sync` | `admin.identity.users_roles.sync` |
| `GET` | `api/identity/users/{id}/permissions` | `admin.identity.users.list` |
| `POST` | `api/identity/users/{id}/permissions/assign` | `admin.identity.users_permissions.assign` |
| `DELETE` | `api/identity/users/{id}/permissions/revoke` | `admin.identity.users_permissions.revoke` |
| `PATCH` | `api/identity/users/{id}/permissions/sync` | `admin.identity.users_permissions.sync` |

#### Admin Endpoints — Roles

Source: `Features/Admin/Roles/`.

| Method | Route | Permission |
|--------|-------|------------|
| `POST` | `api/identity/roles` | `admin.identity.roles.create` |
| `GET` | `api/identity/roles` | `admin.identity.roles.list` |
| `GET` | `api/identity/roles/{id:guid}` | `admin.identity.roles.list` |
| `PUT` | `api/identity/roles/{id:guid}` | `admin.identity.roles.update` |
| `DELETE` | `api/identity/roles/{id:guid}` | `admin.identity.roles.delete` |
| `GET` | `api/identity/roles/{id}/permissions` | `admin.identity.roles.list` |
| `PATCH` | `api/identity/roles/{id}/permissions/sync` | `admin.identity.roles_permissions.sync` |
| `POST` | `api/identity/roles/{id}/permissions/assign` | `admin.identity.roles_permissions.assign` |
| `DELETE` | `api/identity/roles/{id}/permissions/revoke` | `admin.identity.roles_permissions.revoke` |
| `GET` | `api/identity/permissions` | `admin.identity.permissions.list` | Returns all system permissions |

#### Integration
- **Provides**: `User.Id` to Profile (`UserProfile.UserId`), Ordering (`Order.UserId`), Inventory (reservation owner)
- **Enforces**: `.HasPermission()` on all admin endpoints via `PermissionStore` service
- **Seeded**: `RoleSeeder` (30) → `UserSeeder` (40)

---

### 2.3 Profile Module

**Source**: `service/Api/src/Module/Profile/Features/`  
**Schema**: `profile` — Tables: `user_profiles`, `addresses`, `wishlists`, `wishlist_items`, `notification_preferences`  
**Purpose**: Customer profiles, addresses, notification prefs, wishlists.

**Entity Model**:
```
User (identity) @1:1→ UserProfile
  ├── Address (1:N)         ← UserProfileId FK, Country/State refs
  ├── NotificationPreferences (owned value object, JSON column)
  └── Wishlist (1:N)
       └── WishlistItem (1:N)  ← VariantId FK to Catalog
```

**Seeded Data**: 1 profile per user, 1 default address per profile.

#### Endpoints

All under `api/store/profiles/` — require JWT. Source: `Features/Storefront/Profiles/`, `Addresses/`, `Wishlists/`.

**Profiles** (`Features/Storefront/Profiles/`):

| Method | Route | Codes | Description |
|--------|-------|-------|-------------|
| `GET` | `api/store/profiles/profiles` | 200, 401 | Get current user's profile |
| `GET` | `api/store/profiles/profiles/all` | 200, 401 | All profiles (multi-tenant admin) |
| `PUT` | `api/store/profiles/profiles` | 200, 400, 401 | Body: `{ firstName?, lastName?, phoneNumber?, dateOfBirth?, gender?, bio? }` |
| `DELETE` | `api/store/profiles/profiles` | 200, 401 | Delete profile |

**Addresses** (`Features/Storefront/Addresses/`):

| Method | Route | Codes | Description |
|--------|-------|-------|-------------|
| `POST` | `api/store/profiles/addresses` | 201, 400, 401 | Body: `{ firstName, lastName, address1, address2?, city, countryCode, stateCode?, zipCode, phone?, isDefault, addressType }` |
| `GET` | `api/store/profiles/addresses` | 200, 401 | List user's addresses |
| `GET` | `api/store/profiles/addresses/{id:guid}` | 200, 404, 401 | Get address |
| `PUT` | `api/store/profiles/addresses/{id:guid}` | 200, 400, 404, 401 | Update |
| `DELETE` | `api/store/profiles/addresses/{id:guid}` | 200, 404, 401 | Delete |

**Usage** — Create a shipping address:
```
POST /api/store/profiles/addresses
Authorization: Bearer {jwt}
{ "firstName": "John", "lastName": "Doe", "address1": "123 Main St", "city": "New York",
  "zipCode": "10001", "countryCode": "US", "stateCode": "NY", "phone": "+12025550100",
  "isDefault": true, "addressType": "shipping" }
→ 201 { "isSuccess": true, "value": { "id": "guid", "firstName": "John", ... } }
```

**Notification Preferences** (`Features/Storefront/NotificationPreferences/`):

| Method | Route | Codes | Description |
|--------|-------|-------|-------------|
| `GET` | `api/store/profiles/notification-preferences` | 200, 401 | Get prefs — `{ emailNotifications, smsNotifications, pushNotifications, orderUpdates, promotions }` |
| `PUT` | `api/store/profiles/notification-preferences` | 200, 400, 401 | Update subset of prefs |

**Wishlists** (`Features/Storefront/Wishlists/`):

| Method | Route | Codes | Description |
|--------|-------|-------|-------------|
| `GET` | `api/store/profiles/wishlists` | 200, 401 | List wishlists |
| `GET` | `api/store/profiles/wishlists/{id:guid}` | 200, 404, 401 | Get wishlist with nested items |
| `POST` | `api/store/profiles/wishlists` | 201, 400, 401 | Body: `{ name, isDefault? }` |
| `PUT` | `api/store/profiles/wishlists/{id:guid}` | 200, 400, 404, 401 | Update |
| `DELETE` | `api/store/profiles/wishlists/{id:guid}` | 200, 404, 401 | Delete |
| `POST` | `api/store/profiles/wishlists/{id}/items` | 201, 400, 404, 401 | Body: `{ variantId, quantity? }` |
| `DELETE` | `api/store/profiles/wishlists/{id}/items/{itemId}` | 200, 404, 401 | Remove item |

**Integration**
- **Depends**: Identity (User.Id), Location (Country/State lookup for address validation)
- **Consumed by**: Ordering (`Order.BillAddressId`, `Order.ShipAddressId`)
- **Seeded**: `UserProfileSeeder` (50) → `AddressSeeder` (60)

---

### 2.4 Catalog Module

**Source**: `service/Api/src/Module/Catalog/Features/`  
**Schema**: `catalog` — 14 tables  
**Purpose**: Product catalog — taxonomies, taxons, products, variants, prices, images, image search.

**Entity Hierarchy**:
```
Taxonomy ("Categories", "Brands")
  └── Taxon ("Men" → "T-Shirts")             ← Nested set (Lft/Rgt/Depth)
       ├── Product ←→ Classification          ← Many-to-many via TaxonId
       │    ├── ProductOptionType             ← Available option types for this product
       │    └── Variant (master + child)
       │         ├── Price (per currency)     ← Compound: variantId + countryIso
       │         ├── VariantOptionValue       ← e.g., Size=M, Color=Red
       │         └── VariantImage → Embedding  ← pgvector for image search
       └── TaxonRule (auto-classification conditions)
OptionType ("Size", "Color")
  └── OptionValue ("S", "M", "L", ...)       ← Translation enum
```

**Seeded Data**: 2 taxonomies, 8 taxons, 5 products, 20+ variants, Size+Color option types.

#### Storefront Endpoints

Public — no auth. Source: `Features/Storefront/{Products,Variants,Images,Taxonomies,Taxons,OptionTypes}/`.

| Method | Route | Source Folder | Codes | Description |
|--------|-------|---------------|-------|-------------|
| `GET` | `api/storefront/products` | `Products/List/` | 200 | Search/paginate/filter. Query: `?search=shirt&taxonId=...&priceMin=10&priceMax=100` |
| `GET` | `api/storefront/products/{slug}` | `Products/GetBySlug/` | 200, 404 | Full product detail: variants, prices, images, classifications |
| `GET` | `api/storefront/products/{id}/availability` | `Products/Availability/` | 200, 404 | Per-variant stock status |
| `GET` | `api/storefront/products/{id}/related` | `Products/Related/` | 200 | By same taxon classification |
| `GET` | `api/storefront/products/{id}/similar` | `Products/Similar/` | 200 | By image embedding similarity |
| `POST` | `api/storefront/search-by-image` | `Products/SearchByImage/` | 200, 400 | Multipart image upload → vector → pgvector similarity |
| `GET` | `api/storefront/taxonomies/{id:guid}` | `Taxonomies/GetTree/` | 200, 404 | Full tree with nested taxons |
| `GET` | `api/storefront/taxons` | `Taxons/List/` | 200 | Flat list of all taxons |
| `GET` | `api/storefront/taxons/{id}/products` | `Taxons/Products/` | 200 | Products classified under a taxon |
| `GET` | `api/storefront/option-types` | `OptionTypes/List/` | 200 | With nested option values |
| `GET` | `api/storefront/images/{id:guid}` | `Images/GetImage/` | 200, 404 | Binary serve of variant image |

**Usage** — Browse products by taxon:
```
GET /api/storefront/products?taxonId={taxon_id}&page=1&pageSize=10
→ 200 { "isSuccess": true, "value": {
    "items": [{
      "id": "guid", "name": "Classic Cotton T-Shirt", "slug": "classic-cotton-t-shirt",
      "price": 29.99, "currency": "USD",
      "variants": [{ "id": "guid", "sku": "TEE-CTN-001-S", "price": 29.99, "optionValues": [...] }],
      "images": [{ "id": "guid", "url": "/api/storefront/images/{id}" }],
      "classifications": [{ "taxonId": "guid", "taxonName": "Men" }]
    }], "totalCount": 1 } }
```

#### Admin Endpoints — Option Types & Values

Source: `Features/Admin/OptionTypes/`. All require `admin.catalog.option_types.*`.

| Method | Route | Subfolder | Permission |
|--------|-------|-----------|------------|
| `POST` | `api/catalog/option-types` | `Create/` | `admin.catalog.option_types.create` |
| `GET` | `api/catalog/option-types` | `List/` | `admin.catalog.option_types.list` |
| `GET` | `api/catalog/option-types/{id:guid}` | `GetById/` | `admin.catalog.option_types.list` |
| `PUT` | `api/catalog/option-types/{id:guid}` | `Update/` | `admin.catalog.option_types.update` |
| `DELETE` | `api/catalog/option-types/{id:guid}` | `Delete/` | `admin.catalog.option_types.delete` |
| `POST` | `api/catalog/option-types/{id}/values` | `OptionValues/Create/` | `admin.catalog.option_type_option_values.create` |
| `GET` | `api/catalog/option-types/{id}/values` | `OptionValues/List/` | `admin.catalog.option_type_option_values.list` |
| `GET` | `api/catalog/option-types/{id}/values/{vid}` | `OptionValues/GetById/` | `admin.catalog.option_type_option_values.list` |
| `PUT` | `api/catalog/option-types/{id}/values/{vid}` | `OptionValues/Update/` | `admin.catalog.option_type_option_values.update` |
| `DELETE` | `api/catalog/option-types/{id}/values/{vid}` | `OptionValues/Delete/` | `admin.catalog.option_type_option_values.delete` |

#### Admin Endpoints — Taxonomies & Taxons

Source: `Features/Admin/Taxonomies/` and `Taxons/`.

**Taxonomies** (`admin.catalog.taxonomies.*`):

| Method | Route | Subfolder |
|--------|-------|-----------|
| `POST` | `api/catalog/taxonomies` | `Taxonomies/Create/` |
| `GET` | `api/catalog/taxonomies` | `Taxonomies/List/` |
| `GET` | `api/catalog/taxonomies/{id:guid}` | `Taxonomies/GetById/` |
| `PUT` | `api/catalog/taxonomies/{id:guid}` | `Taxonomies/Update/` |
| `DELETE` | `api/catalog/taxonomies/{id:guid}` | `Taxonomies/Delete/` |
| `PATCH` | `api/catalog/taxonomies/{id}/restore` | `Taxonomies/Restore/` |

**Taxons** (`admin.catalog.taxons.*`):

| Method | Route | Subfolder |
|--------|-------|-----------|
| `POST` | `api/catalog/taxonomies/{tid}/taxons` | `Taxons/Create/` |
| `GET` | `api/catalog/taxonomies/{tid}/taxons` | `Taxons/List/` |
| `GET` | `api/catalog/taxonomies/{tid}/taxons/{id}` | `Taxons/GetById/` |
| `GET` | `api/catalog/taxonomies/{tid}/taxons/tree` | `Taxons/Tree/` |
| `PUT` | `api/catalog/taxonomies/{tid}/taxons/{id}` | `Taxons/Update/` |
| `DELETE` | `api/catalog/taxonomies/{tid}/taxons/{id}` | `Taxons/Delete/` |
| `PATCH` | `api/catalog/taxonomies/{tid}/taxons/{id}/restore` | `Taxons/Restore/` |
| `POST` | `api/catalog/taxonomies/{tid}/taxons/{id}/reposition` | `Taxons/Reposition/` |

**Taxon Rules** (`admin.catalog.taxons.manage_rules`):

| Method | Route |
|--------|-------|
| `POST` | `api/catalog/taxonomies/{tid}/taxons/{id}/rules` |
| `GET` | `api/catalog/taxonomies/{tid}/taxons/{id}/rules` |
| `PUT` | `api/catalog/taxonomies/{tid}/taxons/{id}/rules/{rid}` |
| `DELETE` | `api/catalog/taxonomies/{tid}/taxons/{id}/rules/{rid}` |
| `PUT` | `api/catalog/taxonomies/{tid}/taxons/{id}/rules/sync` |

#### Admin Endpoints — Products

Source: `Features/Admin/Products/`. Permission prefix: `admin.catalog.products`.

| Method | Route | Subfolder | Permission Suffix |
|--------|-------|-----------|-------------------|
| `POST` | `api/catalog/products` | `Create/` | `.create` |
| `GET` | `api/catalog/products` | `List/` | `.list` |
| `GET` | `api/catalog/products/{id:guid}` | `GetById/` | `.list` |
| `PUT` | `api/catalog/products/{id:guid}` | `Update/` | `.update` |
| `DELETE` | `api/catalog/products/{id:guid}` | `Delete/` | `.delete` |
| `PATCH` | `api/catalog/products/{id}/activate` | `Activate/` | `.manage` |
| `PATCH` | `api/catalog/products/{id}/discontinue` | `Discontinue/` | `.manage` |

**Product Option Types** (`admin.catalog.product_option_types.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/catalog/products/{id}/option-types` | `.detail` |
| `POST` | `api/catalog/products/{id}/option-types/assign` | `.assign` |
| `DELETE` | `api/catalog/products/{id}/option-types/revoke` | `.revoke` |
| `PUT` | `api/catalog/products/{id}/option-types/sync` | `.sync` |

**Product Classifications** (`admin.catalog.product_classifications.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/catalog/products/{id}/classifications` | `.detail` |
| `POST` | `api/catalog/products/{id}/classifications/assign` | `.assign` |
| `DELETE` | `api/catalog/products/{id}/classifications/revoke` | `.revoke` |
| `PUT` | `api/catalog/products/{id}/classifications/sync` | `.sync` |

**Usage** — Create a product:
```
POST /api/catalog/products
Authorization: Bearer {admin_jwt}
{ "name": "New Product", "slug": "new-product", "description": "A great new product",
  "status": "Active", "availableOn": "2026-07-08T00:00:00Z",
  "metaTitle": "New Product | Store", "metaKeywords": "new, product",
  "shippingCategoryId": null, "taxCategoryId": null }
→ 201 { "isSuccess": true, "value": { "id": "guid", "name": "New Product", "slug": "new-product",
      "status": "Active", ... } }
```

#### Admin Endpoints — Variants

Source: `Features/Admin/Variants/`. Permission prefix: `admin.catalog.product_variants`.

| Method | Route | Subfolder | Permission Suffix |
|--------|-------|-----------|-------------------|
| `POST` | `api/catalog/products/{pid}/variants` | `Create/` | `.create` |
| `GET` | `api/catalog/products/{pid}/variants` | `ListByProduct/` | `.list` |
| `GET` | `api/catalog/variants/{id:guid}` | `GetById/` | `.list` |
| `PUT` | `api/catalog/variants/{id:guid}` | `Update/` | `.update` |
| `DELETE` | `api/catalog/variants/{id:guid}` | `Delete/` | `.delete` |

**Variant Prices** (`admin.catalog.product_variants.manage_price`):

| Method | Route |
|--------|-------|
| `POST` | `api/catalog/variants/{vid}/prices` |
| `GET` | `api/catalog/variants/{vid}/prices` |
| `DELETE` | `api/catalog/variants/{vid}/prices/{pid}` |
| `PUT` | `api/catalog/variants/{vid}/prices/sync` |

**Variant Option Values** (`admin.catalog.product_variant_option_values.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/catalog/variants/{vid}/option-values` | `.list` |
| `POST` | `api/catalog/variants/{vid}/option-values/assign` | `.manage` |
| `DELETE` | `api/catalog/variants/{vid}/option-values/revoke` | `.manage` |
| `PUT` | `api/catalog/variants/{vid}/option-values/sync` | `.manage` |

**Variant Images** (`admin.catalog.product_variant_images.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `POST` | `api/catalog/variants/{vid}/images` | `.create` |
| `GET` | `api/catalog/variants/{vid}/images` | `.list` |
| `GET` | `api/catalog/variants/images/{id}` | `.list` |
| `PUT` | `api/catalog/variants/images/{id}` | `.update` |
| `DELETE` | `api/catalog/variants/images/{id}` | `.delete` |
| `GET` | `api/catalog/variants/images/{id}/download` | `.list` |
| `POST` | `api/catalog/variants/images/{id}/embeddings` | `.manage_assets` |

#### Integration
- **Consumed by**: Ordering (`LineItem.VariantId`), Inventory (`StockItem.VariantId`), Profile (`WishlistItem.VariantId`)
- **Image search**: Product image → Python Embedding service → pgvector `<=>` cosine similarity → similar products
- **Seeded**: `CatalogOptionSeeder` (100) → `CatalogTaxonomySeeder` (110) → `CatalogTaxonSeeder` (120) → `CatalogDemoSeeder` (130)

---

### 2.5 Ordering Module

**Source**: `service/Api/src/Module/Ordering/Features/`  
**Schema**: `ordering` — Tables: `orders`, `line_items`, `adjustments`  
**Purpose**: Shopping cart, checkout, order management.

**Entity Model**:
```
Order
  ├── LineItems (1:N)       ← VariantId FK → Catalog
  ├── Adjustments (1:N)     ← Tax, shipping, promo adjustments
  └── PaymentRecords (owned) ← Value object list (not separate table)
OrderStatus: Draft (0) → Placed (1) → Canceled (2) | Expired (4)
CheckoutState: Address → Delivery → Payment → Confirm → Complete
```

**Seeded Data**: 3 placed orders with line items, PaymentRecords, and Payment entities.

#### Checkout Flow (Full Customer Journey)

```
 1. Browse             GET  /api/storefront/products
 2. Create Cart        POST /api/storefront/cart
 3. Add Item           POST /api/storefront/cart/items      { variantId, quantity }
 4. Login              POST /api/store/identity/auth/login/password
 5. Associate          POST /api/storefront/cart/associate   links cart → user
 6. Set Addresses      PUT  /api/storefront/cart             { billAddressId, shipAddressId }
 7. Select Shipping    POST /api/storefront/cart/shipping-rate  { shippingRateId }
 8. Validate           POST /api/storefront/cart/validate
 9. Reserve Stock      POST /api/storefront/cart/reserve     (automated during checkout)
10. Place Order        POST /api/storefront/cart/checkout    → Order.Status = Placed
11. Create Payment     POST /api/storefront/payment/create-intent  → clientSecret
12. Confirm Payment    POST /api/storefront/payment/confirm/{paymentId}
```

#### Cart Endpoints

Source: `Features/Storefront/Cart/`. All require JWT auth (except initial cart creation).

| Method | Route | Subfolder | Codes | Description |
|--------|-------|-----------|-------|-------------|
| `GET` | `api/storefront/cart` | `GetCart/` | 200, 401 | Current cart with line items |
| `POST` | `api/storefront/cart` | `CreateCart/` | 201 | Body: `{ sessionId? }`. Returns cart with empty line items |
| `POST` | `api/storefront/cart/associate` | `AssociateCart/` | 200, 401 | Links guest cart to authenticated user |
| `POST` | `api/storefront/cart/items` | `AddToCart/` | 201, 400, 401 | Body: `{ variantId, quantity }` |
| `DELETE` | `api/storefront/cart/items/{lineItemId:guid}` | `RemoveCartItem/` | 200, 404, 401 | |
| `PUT` | `api/storefront/cart/items/{lineItemId:guid}` | `UpdateCartItemQuantity/` | 200, 400, 404, 401 | Body: `{ quantity }` |
| `POST` | `api/storefront/cart/empty` | `EmptyCart/` | 200, 401 | Clears all items |
| `DELETE` | `api/storefront/cart` | `DeleteCart/` | 200, 401 | Deletes cart entirely |
| `POST` | `api/storefront/cart/checkout` | `Checkout/` | 200, 400, 401 | Converts cart → placed order. Returns order summary |
| `PUT` | `api/storefront/cart` | `UpdateCheckout/` | 200, 400, 401 | Body: `{ billAddressId, shipAddressId, email, specialInstructions }` |
| `POST` | `api/storefront/cart/validate` | `ValidateCheckout/` | 200, 400, 401 | Validates checkout completeness |
| `POST` | `api/storefront/cart/shipping-rate` | `SelectShippingRate/` | 200, 400, 401 | Body: `{ shippingRateId }` |

**Usage** — Full cart interaction:
```
# Create cart (anonymous)
POST /api/storefront/cart → 201
{ "isSuccess": true, "value": { "id": "guid", "sessionId": "sess_abc", "lineItems": [] } }

# Add item
POST /api/storefront/cart/items
{ "variantId": "{variant_id}", "quantity": 2 }
→ 201 { "isSuccess": true, "value": { "id": "guid", "variantId": "...", "quantity": 2, "price": 29.99, "total": 59.98 } }

# Checkout (must be logged in)
POST /api/storefront/cart/checkout
Authorization: Bearer {jwt}
→ 200 { "isSuccess": true, "value": { "orderId": "guid", "number": "R20260708-XXXX", "total": 59.98, "lineItems": [...] } }
```

#### Customer Order Endpoints

Source: `Features/Storefront/Orders/`. Auth required.

| Method | Route | Codes | Description |
|--------|-------|-------|-------------|
| `GET` | `api/storefront/orders` | 200, 401 | Current user's orders (paged) |
| `GET` | `api/storefront/orders/{id:guid}` | 200, 404, 401 | Order detail with line items |
| `PUT` | `api/storefront/orders/{id:guid}/cancel` | 200, 400, 404, 401 | Cancel (own) order |

#### Admin Order Endpoints

Source: `Features/Admin/Orders/`. All require `admin.ordering.orders.*`.

| Method | Route | Subfolder | Permission Suffix |
|--------|-------|-----------|-------------------|
| `POST` | `api/ordering/orders` | `Create/` | `.create` |
| `GET` | `api/ordering/orders` | `List/` | `.list` |
| `GET` | `api/ordering/orders/{id:guid}` | `GetById/` | `.detail` |
| `PUT` | `api/ordering/orders/{id:guid}` | `Update/` | `.update` |
| `PUT` | `api/ordering/orders/{id:guid}/status` | `UpdateStatus/` | `.update` |
| `PUT` | `api/ordering/orders/{id:guid}/ship-address` | `UpdateShipAddress/` | `.update` |
| `PUT` | `api/ordering/orders/{id:guid}/bill-address` | `UpdateBillAddress/` | `.update` |
| `PUT` | `api/ordering/orders/{id:guid}/shipping-method` | `UpdateShippingMethod/` | `.update` |
| `POST` | `api/ordering/orders/{id:guid}/cancel` | `Cancel/` | `.update` |
| `POST` | `api/ordering/orders/{id:guid}/complete` | `Complete/` | `.update` |
| `POST` | `api/ordering/orders/{id:guid}/approve` | `Approve/` | `.update` |
| `POST` | `api/ordering/orders/{id:guid}/resume` | `Resume/` | `.update` |
| `DELETE` | `api/ordering/orders/{id:guid}` | `Delete/` | `.delete` |

**Admin — Line Items** (nested under orders):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/ordering/orders/{id}/line-items` | `.detail` |
| `GET` | `api/ordering/orders/{id}/line-items/{lid}` | `.detail` |
| `POST` | `api/ordering/orders/{id}/line-items` | `.update` |
| `PUT` | `api/ordering/orders/{id}/line-items/{lid}` | `.update` |
| `DELETE` | `api/ordering/orders/{id}/line-items/{lid}` | `.update` |

**Usage** — Admin views order list:
```
GET /api/ordering/orders?page=1&pageSize=20&status=Placed&sortBy=createdAtUtc&sortDirection=desc
Authorization: Bearer {admin_jwt}
→ 200 { "isSuccess": true, "value": {
    "items": [{ "id": "guid", "number": "R20260708-XXXX", "status": "Placed",
      "total": 129.99, "email": "user@example.com", "itemCount": 3,
      "createdAtUtc": "2026-07-08T12:00:00Z" }], "totalCount": 3 } }
```

#### Event Bus Integration

When an order transitions to `Placed` (via checkout or admin complete):
```
Order.Finalize()
  → IOrderEventPublisher.PublishAsync(orderPlacedEvent)
    → Default: NullOrderEventPublisher (no-op)
    → OR: WebhookOrderEventPublisher (swapped in Program.cs:49)
      → IWebhookDispatcher.DispatchAsync("order.placed", payload)
        → WebhookDelivery created (pending)
          → WebhookDeliveryBackgroundService delivers via HTTP POST
```

#### Cart Expiry Background Service

`CartExpiryService` runs as `BackgroundService` every 5 minutes (configurable):
- Queries orders where `Status = Draft` and `CreatedAtUtc < now - 24h`
- Sets `Status = Expired`
- Triggers `ReservationExpiryService` in Inventory to release stock holds

#### Integration
- **Depends on**: Catalog (Variant), Profile (Address), Identity (User), Shipping (Method + Rate), Inventory (stock reserve)
- **Provides**: Order data → Payment (Payment.OrderId FK), Webhooks (OrderPlaced event)
- **Seeded**: `OrderSeeder` (190) → 3 placed orders with line items

---

### 2.6 Payment Module

**Source**: `service/Api/src/Module/Payment/Features/`  
**Schema**: `payment` — Tables: `payments`, `payment_methods`  
**Purpose**: Payment processing, Stripe/Bogus gateway, payment method management.

**Entity Model**:
```
PaymentMethod (Store Credit, Credit Card, PayPal, Bank Transfer)
  └── Payment (1:N)       ← OrderId FK → Ordering.Order
       State machine: Checkout → Processing → Pending → Completed
                                          ↓            ↓
                                        Failed      Void → Invalid
```

**Seeded Data**: 4 payment methods + completed Payments for seeded orders.

**Gateway Config**: `appsettings.json` → `Payment:UseBogusGateway: true` uses Bogus (fake, returns success). `false` uses Stripe.

#### Storefront Endpoints

Source: `Features/Storefront/Payment/`. Auth required (except Stripe webhook).

| Method | Route | Source Folder | Auth | Codes | Description |
|--------|-------|---------------|------|-------|-------------|
| `POST` | `api/storefront/payment/create-intent` | `CreateIntent/` | Yes | 200, 400, 401 | Body: `{ orderId, paymentMethodId }`. Returns `{ clientSecret, paymentId }` |
| `POST` | `api/storefront/payment/confirm/{paymentId:guid}` | `Confirm/` | Yes | 200, 400, 401, 404 | Body: `{ paymentIntentId?, stripePaymentMethodId? }`. Finalizes payment |
| `GET` | `api/storefront/payment/methods` | `ListMethods/` | Yes | 200, 401 | Active methods with `DisplayOn = Both/Frontend` |
| `POST` | `api/storefront/payment/setup-intent` | `SetupIntent/` | Yes | 200, 401 | For saved payment methods |
| `POST` | `api/storefront/webhooks/stripe` | `Webhooks/` | No | 200 | Stripe webhook receiver (public, signature verified) |

**Usage** — Payment after checkout:
```
# 1. Create intent
POST /api/storefront/payment/create-intent
Authorization: Bearer {jwt}
{ "orderId": "{order_id}", "paymentMethodId": "{pm_id}" }
→ { "isSuccess": true, "value": { "clientSecret": "pi_..._secret_...", "paymentId": "guid" } }

# 2. Frontend uses clientSecret with Stripe.js Elements
# 3. Confirm on backend
POST /api/storefront/payment/confirm/{paymentId}
Authorization: Bearer {jwt}
{ "paymentIntentId": "pi_...", "stripePaymentMethodId": "pm_..." }
→ { "isSuccess": true, "value": { "state": "Completed", "number": "PAY-20260708-XXXXXXXX" } }
```

#### Admin Endpoints

Source: `Features/Admin/Payments/` and `PaymentMethods/`.

**Payments** (`admin.payment.payments.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/payment/payments` | `.list` |
| `GET` | `api/payment/payments/{id:guid}` | `.list` |
| `POST` | `api/payment/payments/{id}/capture` | `.capture` |
| `POST` | `api/payment/payments/{id}/void` | `.void` |
| `POST` | `api/payment/payments/{id}/refund` | `.refund` |

**Payment Methods** (`admin.payment.payment_methods.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/payment/payment-methods` | `.detail` |
| `GET` | `api/payment/payment-methods/{id:guid}` | `.detail` |
| `POST` | `api/payment/payment-methods` | `.create` |
| `PUT` | `api/payment/payment-methods/{id:guid}` | `.update` |
| `DELETE` | `api/payment/payment-methods/{id:guid}` | `.delete` |
| `PATCH` | `api/payment/payment-methods/{id}/activate` | `.activate` |
| `PATCH` | `api/payment/payment-methods/{id}/deactivate` | `.deactivate` |

#### Integration
- **Depends on**: Ordering (`Payment.OrderId` → `Ordering.Order.Id`)
- **Provider resolution**: `IPaymentGatewayActionProvider` → `BogusGateway` (dev) or `StripeGateway` (prod)
- **Seeded**: `PaymentMethodSeeder` (160) → `PaymentSeeder` (200)

---

### 2.7 Shipping Module

**Source**: `service/Api/src/Module/Shipping/Features/`  
**Schema**: `shipping` — Tables: `shipping_methods`, `shipping_rates`  
**Purpose**: Shipping method/rate config, cost calculation.

**Entity Model**:
```
ShippingMethod (Standard, Express, Free)
  CalculatorType: "FlatRate" | "FreeShipping"
  └── ShippingRate (1:N)    ← cost, delivery range, weight bounds
```

**Seeded Data**: 3 methods + 3 rates.

#### Storefront Endpoints

Public — no auth. Source: `Features/Storefront/Shipments/`.

| Method | Route | Codes | Description |
|--------|-------|-------|-------------|
| `GET` | `api/storefront/shipping/methods` | 200 | Active shipping methods |
| `POST` | `api/storefront/shipping/calculate` | 200, 400 | Body: `{ shippingMethodId, orderTotal, weight? }`. Returns calculated cost |
| `GET` | `api/storefront/shipping/rates` | 200 | All rates with pricing |

#### Admin Endpoints

Source: `Features/Admin/ShippingMethods/` and `ShippingRates/`.

**Shipping Methods** (`admin.shipping.methods.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `POST` | `api/shipping/shipping-methods` | `.create` |
| `GET` | `api/shipping/shipping-methods` | `.list` |
| `GET` | `api/shipping/shipping-methods/{id:guid}` | `.detail` |
| `PUT` | `api/shipping/shipping-methods/{id:guid}` | `.update` |
| `DELETE` | `api/shipping/shipping-methods/{id:guid}` | `.delete` |
| `PATCH` | `api/shipping/shipping-methods/{id}/activate` | `.activate` |
| `PATCH` | `api/shipping/shipping-methods/{id}/deactivate` | `.deactivate` |

**Shipping Rates** (`admin.shipping.rates.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `POST` | `api/shipping/shipping-rates` | `.create` |
| `GET` | `api/shipping/shipping-rates` | `.list` |
| `GET` | `api/shipping/shipping-rates/{id:guid}` | `.detail` |
| `PUT` | `api/shipping/shipping-rates/{id:guid}` | `.update` |
| `DELETE` | `api/shipping/shipping-rates/{id:guid}` | `.delete` |

#### Integration
- **Consumed by**: Ordering (`Order.ShippingMethodId`, cart shipping rate selection)
- **Seeded**: `ShippingMethodSeeder` (170) → `ShippingRateSeeder` (180)

---

### 2.8 Inventory Module

**Source**: `service/Api/src/Module/Inventory/Features/`  
**Schema**: `inventory` — Tables: `stock_locations`, `stock_items`, `stock_movements`, `stock_reservations`, `stock_transfers`  
**Purpose**: Stock tracking, reservations, transfers.

**Entity Model**:
```
StockLocation (warehouse)
  └── StockItem (per variant per location) ← VariantId → Catalog
       ├── StockMovement (audit trail)
       └── StockReservation (temporary hold)
StockTransfer (between locations) ← StockLocation source + destination
```

**Seeded Data**: 1 default warehouse, stock items for all seeded variants, initial movements.

#### Storefront Endpoints

Source: `Features/Storefront/`. Public (availability) or cart-token auth (reserve).

| Method | Route | Subfolder | Auth | Description |
|--------|-------|-----------|------|-------------|
| `GET` | `api/storefront/availability/{variantId:guid}` | `Availability/` | No | Returns `{ available, countOnHand, backorderable }` |
| `POST` | `api/storefront/cart/reserve` | `Reserve/` | Cart token | Body: `{ sessionId, items: [{ variantId, quantity }] }` |
| `GET` | `api/storefront/cart/reserve` | `Reserve/` | Cart token | Current reservations for session |

#### Admin Endpoints

All require `admin.inventory.*` permissions. Source: `Features/Admin/`.

**Stock Locations** (`admin.inventory.stock_location.*`):

| Method | Route | Subfolder | Permission |
|--------|-------|-----------|------------|
| `POST` | `api/inventory/stock-locations` | `StockLocations/Create/` | `.create` |
| `GET` | `api/inventory/stock-locations` | `StockLocations/List/` | `.list` |
| `GET` | `api/inventory/stock-locations/{id:guid}` | `StockLocations/GetById/` | `.detail` |
| `PUT` | `api/inventory/stock-locations/{id:guid}` | `StockLocations/Update/` | `.update` |
| `DELETE` | `api/inventory/stock-locations/{id:guid}` | `StockLocations/Delete/` | `.delete` |
| `PUT` | `api/inventory/stock-locations/{id}/default` | `StockLocations/SetDefault/` | `.update` |

**Stock Items** (`admin.inventory.stock_items.*`):

| Method | Route | Subfolder | Permission Suffix |
|--------|-------|-----------|-------------------|
| `POST` | `api/inventory/stock-items` | `StockItems/Create/` | `.create` |
| `GET` | `api/inventory/stock-items` | `StockItems/List/` | `.list` |
| `GET` | `api/inventory/stock-items/{id:guid}` | `StockItems/GetById/` | `.detail` |
| `PUT` | `api/inventory/stock-items/{id:guid}` | `StockItems/Update/` | `.update` |
| `DELETE` | `api/inventory/stock-items/{id:guid}` | `StockItems/Delete/` | `.delete` |
| `POST` | `api/inventory/stock-items/bulk-adjust` | `StockItems/BulkAdjust/` | `.adjust` |
| `POST` | `api/inventory/stock-items/{id}/restock` | `StockItems/Restock/` | `.adjust` |
| `GET` | `api/inventory/stock-items/low-stock` | `StockItems/LowStock/` | `.list` |
| `GET` | `api/inventory/stock-items/summary` | `StockItems/Summary/` | `.list` |
| `POST` | `api/inventory/stock-items/import` | `StockItems/Import/` | `.create` |

**Stock Reservations** (`admin.inventory.stock_reservations.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/inventory/stock-reservations` | `.list` |
| `GET` | `api/inventory/stock-reservations/{id:guid}` | `.detail` |
| `POST` | `api/inventory/stock-reservations/{id}/cancel` | `.cancel` |

**Stock Transfers** (`admin.inventory.stock_transfers.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `POST` | `api/inventory/stock-transfers` | `.create` |
| `GET` | `api/inventory/stock-transfers` | `.list` |
| `GET` | `api/inventory/stock-transfers/{id:guid}` | `.detail` |
| `POST` | `api/inventory/stock-transfers/{id}/transfer` | `.update` |
| `POST` | `api/inventory/stock-transfers/{id}/receive` | `.update` |
| `POST` | `api/inventory/stock-transfers/{id}/cancel` | `.cancel` |

**Stock Movements** (read-only, `admin.inventory.stock_movements.*`):

| Method | Route | Permission Suffix |
|--------|-------|-------------------|
| `GET` | `api/inventory/stock-movements` | `.list` |
| `GET` | `api/inventory/stock-movements/{id:guid}` | `.detail` |

#### Integration
- **Depends on**: Catalog (`StockItem.VariantId`), Location (warehouse country)
- **Consumed by**: Ordering (stock reservation during checkout, release on cart expiry)
- **Seeded**: `StockLocationSeeder` (100) → `InventoryStockItemSeeder` (140) → `InventoryStockMovementSeeder` (150)

---

### 2.9 Webhooks Module

**Source**: `service/Api/src/Shared/Operational/Webhooks/`  
**Schema**: `operational` — Tables: `webhook_subscriptions`, `webhook_deliveries`  
**Purpose**: Outbound webhook event delivery.

**Event Types**:
| Event | Trigger | Payload |
|-------|---------|---------|
| `order.placed` | `Order.Finalize()` | `{ orderId, number, total, email, currency, lineItems: [{ sku, quantity, price }] }` |

**Delivery Flow**:
```
Domain Event → IOrderEventPublisher
  → WebhookOrderEventPublisher           (DI-swapped in Program.cs)
    → IWebhookDispatcher.DispatchAsync() (queries matching subscriptions)
      → WebhookDelivery created (state: Pending)
        → WebhookDeliveryBackgroundService (sweeps every 60s)
          → WebhookDeliveryJob: HTTP POST to subscriber URL
            Headers:
              X-Webhook-Signature: HMAC-SHA256(body, secret)
              X-Webhook-Timestamp: unix_epoch_seconds
            → marks Delivery as Delivered or Failed
            → retries up to MaxRetries with exponential backoff
```

#### Admin Endpoints

Source: `Webhooks/Features/`.

| Method | Route | Subfolder | Codes | Description |
|--------|-------|-----------|-------|-------------|
| `POST` | `api/webhooks/subscriptions` | `Create/` | 201, 400 | Body: `{ event, url, secret, maxRetries?, active? }` |
| `GET` | `api/webhooks/subscriptions` | `List/` | 200 | Paged subscriptions |
| `GET` | `api/webhooks/subscriptions/{id:guid}` | `GetById/` | 200, 404 | |
| `PUT` | `api/webhooks/subscriptions/{id:guid}` | `Update/` | 200, 400, 404 | |
| `DELETE` | `api/webhooks/subscriptions/{id:guid}` | `Delete/` | 200, 404 | |
| `POST` | `api/webhooks/subscriptions/{id}/test` | `Test/` | 200, 400, 404 | Fires test payload |

**Usage** — Subscribe to order.placed events:
```
POST /api/webhooks/subscriptions
Authorization: Bearer {admin_jwt}
{ "event": "order.placed", "url": "https://myapp.com/webhooks/orders",
  "secret": "whsec_abc123", "maxRetries": 3, "active": true }
→ 201 { "isSuccess": true, "value": { "id": "guid", "event": "order.placed",
      "url": "https://myapp.com/webhooks/orders", "active": true } }
```

#### Signature Verification

Recipients verify the webhook by:
```python
import hmac, hashlib
signature = request.headers['X-Webhook-Signature']
timestamp = request.headers['X-Webhook-Timestamp']
expected = hmac.new(secret.encode(), request.body, hashlib.sha256).hexdigest()
assert hmac.compare_digest(signature, expected)
```

#### Integration
- **Receives from**: Ordering (`IOrderEventPublisher` → `order.placed` event)
- **Standalone**: No FK dependencies; only reads/writes `operational.webhook_*` tables

---

## 3. End-to-End Data Flows

### 3.1 Full Checkout Flow (Customer)

```
Browser/SPA                    API Server                         Stripe / Services
    │                            │                                 │
    │  GET /storefront/products  │  → Catalog.ListProducts         │
    │◄─────────── products ──────┤  → filters, pagination          │
    │                            │                                 │
    │  POST /storefront/cart     │  → Order.Create("USD")          │
    │◄─────────── cart id ───────┤  → Status: Draft                │
    │                            │                                 │
    │  POST /storefront/cart/items│  → LineItem.Create(order,      │
    │  { variantId, quantity }   │    variant, qty, price)         │
    │◄─────────── line item ─────┤  → Db: ordering.line_items      │
    │                            │                                 │
    │  POST /auth/login/password │  → Identity.Login               │
    │◄─────────── JWT ───────────┤  → Returns accessToken          │
    │                            │                                 │
    │  POST /cart/associate      │  → Order.UserId = user.Id       │
    │◄─────────── ok ────────────┤                                 │
    │                            │                                 │
    │  PUT /storefront/cart      │  → Order.BillAddressId = ...    │
    │  { billAddressId, ship... }│  → Order.ShipAddressId = ...    │
    │◄─────────── ok ────────────┤                                 │
    │                            │                                 │
    │  POST /cart/shipping-rate  │  → Order.ShippingRateId = ...   │
    │  { shippingRateId }        │                                 │
    │◄─────────── ok ────────────┤                                 │
    │                            │                                 │
    │  POST /cart/reserve        │  → Inventory.StockItem.Decr     │
    │◄─────────── ok ────────────┤  → StockReservation.Create()    │
    │                            │                                 │
    │  POST /cart/checkout       │  → Order.Finalize()             │
    │◄──── order + total ────────┤    → Status: Placed             │
    │                            │    → IOrderEventPublisher       │
    │                            │    → WebhookDelivery (pending)  │
    │                            │    → WebhookDeliveryBgSvc       │──► Subscriber
    │                            │                                 │
    │  POST /payment/create-intent│ → PaymentFactory.Create()      │
    │◄──── clientSecret ─────────┤ → Bogus/Stripe gateway          │
    │                            │                                 │
    │  Stripe.js confirm         │────────────────────────────────►│  Stripe API
    │◄─────────── ok ────────────┤◄────────────────────────────────│
    │                            │                                 │
    │  POST /payment/confirm/{id}│ → Payment.Complete()            │
    │◄──── payment done ────────┤ → State: Completed               │
```

### 3.2 Admin Order Fulfillment Flow

```
Admin SPA                    API Server
    │                            │
    │  GET /ordering/orders      │  → Ordering.ListOrders (paged, filtered)
    │◄─── orders list ───────────┤  → 12 statuses, date ranges
    │                            │
    │  GET /ordering/orders/{id} │  → Ordering.GetOrderDetail
    │◄─── order detail ──────────┤  → line items, payments, addresses
    │                            │
    │  POST /payment/payments/   │  → Payment.Capture
    │       {id}/capture         │  → State: Completed
    │◄─── payment captured ──────┤
    │                            │
    │  PUT /ordering/orders/{id} │  → Order.ShipAddressId = new
    │       /ship-address        │
    │◄─── updated ───────────────┤
    │                            │
    │  PUT /ordering/orders/{id} │  → Order.ShippingMethodId = new
    │       /shipping-method     │
    │◄─── updated ───────────────┤
    │                            │
    │  POST /ordering/orders/{id}│  → Order.Approve(adminUserId)
    │       /approve             │  → ApprovedById set
    │◄─── approved ──────────────┤
```

### 3.3 Image Search Flow

```
Client                     API (C#)                     Python Embedding Service
  │                         │                              │
  │  POST /search-by-image  │                              │
  │  (multipart: image)     │                              │
  │─────►───────────────────┤                              │
  │                         │  HTTP POST /embed            │
  │                         │  (image bytes)                │
  │                         │─────────────►─────────────────┤
  │                         │                              │  model.encode()
  │                         │                              │  → float[512] vector
  │                         │◄──── vector ─────────────────┤
  │                         │                              │
  │                         │  EF Core + pgvector:         │
  │                         │  SELECT v.*, i.*             │
  │                         │  FROM image_embeddings e     │
  │                         │  ORDER BY e.embedding        │
  │                         │  <=> {queryVector}           │
  │                         │  LIMIT 10                    │
  │                         │                              │
  │                         │  → Similar Variants found    │
  │                         │  → Group by Product          │
  │                         │                              │
  │◄──── products ──────────┤                              │
```

---

## 4. Permission Reference

All permissions follow the format: `admin.{module}.{resource}.{action}`.

### Complete Permission Matrix

| Module | Resource | Actions |
|--------|----------|---------|
| Catalog | `option_types` | `create`, `list`, `update`, `delete` |
| Catalog | `option_type_option_values` | `create`, `list`, `update`, `delete` |
| Catalog | `taxonomies` | `create`, `list`, `update`, `delete`, `restore` |
| Catalog | `taxons` | `create`, `list`, `update`, `delete`, `restore`, `manage_rules` |
| Catalog | `products` | `create`, `list`, `update`, `delete`, `manage` |
| Catalog | `product_option_types` | `detail`, `assign`, `revoke`, `sync` |
| Catalog | `product_classifications` | `detail`, `assign`, `revoke`, `sync` |
| Catalog | `product_variants` | `create`, `list`, `update`, `delete`, `manage_price` |
| Catalog | `product_variant_option_values` | `list`, `manage` |
| Catalog | `product_variant_images` | `create`, `list`, `update`, `delete`, `manage_assets` |
| Identity | `users` | `create`, `list`, `update`, `delete` |
| Identity | `users_roles` | `assign`, `revoke`, `sync` |
| Identity | `users_permissions` | `assign`, `revoke`, `sync` |
| Identity | `roles` | `create`, `list`, `update`, `delete` |
| Identity | `roles_permissions` | `sync`, `assign`, `revoke` |
| Identity | `permissions` | `list` |
| Location | `countries` | `create`, `list`, `detail`, `update`, `delete` |
| Location | `states` | `create`, `list`, `detail`, `update`, `delete` |
| Payment | `payments` | `list`, `capture`, `void`, `refund` |
| Payment | `payment_methods` | `create`, `detail`, `update`, `delete`, `activate`, `deactivate` |
| Shipping | `methods` | `create`, `list`, `detail`, `update`, `delete`, `activate`, `deactivate` |
| Shipping | `rates` | `create`, `list`, `detail`, `update`, `delete` |
| Inventory | `stock_location` | `create`, `list`, `detail`, `update`, `delete` |
| Inventory | `stock_items` | `create`, `list`, `detail`, `update`, `delete`, `adjust` |
| Inventory | `stock_reservations` | `list`, `detail`, `cancel` |
| Inventory | `stock_transfers` | `create`, `list`, `detail`, `update`, `cancel` |
| Inventory | `stock_movements` | `list`, `detail` |
| Ordering | `orders` | `create`, `list`, `detail`, `update`, `delete` |

---

## 5. DB Schema Map

| Schema | Tables | Module | Seeder Orders |
|--------|--------|--------|---------------|
| `location` | `countries`, `states` | Location | 10, 20 |
| `identity` | `users`, `roles`, `user_roles`, `role_claims`, `user_permissions` | Identity | 30, 40 |
| `profile` | `user_profiles`, `addresses`, `wishlists`, `wishlist_items` | Profile | 50, 60 |
| `catalog` | `products`, `variants`, `prices`, `option_types`, `option_values`, `variant_option_values`, `product_option_types`, `taxonomies`, `taxons`, `classifications`, `variant_images`, `image_embeddings`, `taxon_rules` | Catalog | 100, 110, 120, 130 |
| `ordering` | `orders`, `line_items`, `adjustments` | Ordering | 190 |
| `payment` | `payments`, `payment_methods` | Payment | 160, 200 |
| `shipping` | `shipping_methods`, `shipping_rates` | Shipping | 170, 180 |
| `inventory` | `stock_locations`, `stock_items`, `stock_movements`, `stock_reservations`, `stock_transfers` | Inventory | 100, 140, 150 |
| `operational` | `webhook_subscriptions`, `webhook_deliveries` | Webhooks | (none) |

---

## 6. Seeder Execution Order

| Order | Seeder | Location | Data |
|-------|--------|----------|------|
| 10 | `CountrySeeder` | `Module/Location/Persistence/Seeders/Country.Seeder.cs` | US, Vietnam |
| 20 | `StateSeeder` | `Module/Location/Persistence/Seeders/State.Seeder.cs` | 50 US states + 63 VN provinces |
| 30 | `RoleSeeder` | `Shared/Security/Identity/Seeders/Role.Seeder.cs` | Admin, Manager, User + claims |
| 40 | `UserSeeder` | `Shared/Security/Identity/Seeders/User.Seeder.cs` | 1 admin, 3 mgrs, 3 users |
| 50 | `UserProfileSeeder` | `Module/Profile/Persistence/Seeders/UserProfile.Seeder.cs` | 1 per user |
| 60 | `AddressSeeder` | `Module/Profile/Persistence/Seeders/Address.Seeder.cs` | 1 shipping addr per profile |
| 100 | `CatalogOptionSeeder` | `Module/Catalog/Persistence/Seeders/Option.Seeder.cs` | Size + Color |
| 100 | `StockLocationSeeder` | `Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs` | Default Warehouse |
| 110 | `CatalogTaxonomySeeder` | `Module/Catalog/Persistence/Seeders/Taxonomy.Seeder.cs` | Categories, Brands |
| 120 | `CatalogTaxonSeeder` | `Module/Catalog/Persistence/Seeders/Taxon.Seeder.cs` | 8 taxons |
| 130 | `CatalogDemoSeeder` | `Module/Catalog/Persistence/Seeders/Product.Seeder.cs` | 5 products, 20+ variants |
| 140 | `InventoryStockItemSeeder` | `Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs` | Stock per variant |
| 150 | `InventoryStockMovementSeeder` | `Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs` | Initial movements |
| 160 | `PaymentMethodSeeder` | `Module/Payment/Persistence/Seeders/PaymentMethod.Seeder.cs` | Store Credit, CC, PayPal, Bank |
| 170 | `ShippingMethodSeeder` | `Module/Shipping/Persistence/Seeders/ShippingMethod.Seeder.cs` | Standard, Express, Free |
| 180 | `ShippingRateSeeder` | `Module/Shipping/Persistence/Seeders/ShippingRate.Seeder.cs` | 3 rates with pricing |
| 190 | `OrderSeeder` | `Module/Ordering/Persistence/Seeders/Order.Seeder.cs` | 3 placed orders |
| 200 | `PaymentSeeder` | `Module/Ordering/Persistence/Seeders/Payment.Seeder.cs` | Completed payments |
