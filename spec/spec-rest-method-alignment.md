---
title: REST Method Alignment — HTTP Verbs, Customer Account, Shipping, Orders
version: 1.0
date_created: 2025-08-11
last_updated: 2025-08-11
owner: ReSys.Shop Platform
tags: design, rest, http, customer, shipping, orders
---

# Introduction

This specification defines corrections to HTTP method usage across storefront endpoints outside the Cart resource. It covers Orders (cancel method), Shipping (calculate method), Customer account (profile, address, wishlist partial updates), and the security fix for `GET /customer/all`.

## 1. Purpose & Scope

### Purpose

Fix all non-Cart storefront endpoints that violate HTTP method semantics. Move `GET /customer/all` to admin with proper permission check — it currently exposes all customers on the storefront without admin auth.

### Scope

- Orders: `PUT /orders/{id}/cancel` → `POST /orders/{id}/cancel`
- Orders: Route prefix `ordering/orders` → `orders`
- Shipping: `POST /shipping/calculate` → `GET /shipping/calculate`
- Customer: 5 `PUT` endpoints → `PATCH`
- Customer: `GET /customer/all` → move to `ProfileFeature.Admin.cs`

### Out of Scope

- Cart HTTP method changes (covered by `spec-cart-consolidation.md`)
- Customer endpoint route prefix changes (already `api/storefront/customer` — no module nesting)
- Business logic changes behind any endpoint

## 2. Definitions

| Term | Definition |
|------|------------|
| **PATCH** | HTTP method for partial resource update — only provided fields change |
| **POST** | HTTP method for creating resources or executing non-idempotent actions |
| **GET** | HTTP method for idempotent reads — must not mutate server state |
| **ProfileFeature.Admin.cs** | Route constant file for admin-scoped customer endpoints |

## 3. Requirements, Constraints & Guidelines

### Orders Requirements

- **ORD-001**: Cancel order → `POST /api/storefront/orders/{id}/cancel` (was `PUT`)
- **ORD-002**: Route prefix `api/storefront/orders` (was `api/storefront/ordering/orders`)
- **ORD-003**: All other orders routes (GET list, GET by ID, GET tracking) unchanged except prefix

### Shipping Requirements

- **SHP-001**: Calculate shipping → `GET /api/storefront/shipping/calculate` (was `POST`)
- **SHP-002**: Query parameters carry `shippingMethodId` and `orderId` — passed via `[FromQuery]`
- **SHP-003**: `ShippingFeature.Storefront.cs` update: change Calculate from POST to GET route definition

### Customer Account Requirements

- **CUS-001**: Update profile → `PATCH /api/storefront/customer` (was `PUT`)
- **CUS-002**: Update address → `PATCH /api/storefront/customer/addresses/{id}` (was `PUT`)
- **CUS-003**: Set default address → `PATCH /api/storefront/customer/addresses/{id}/default` (was `PUT`)
- **CUS-004**: Update wishlist → `PATCH /api/storefront/customer/wishlists/{id}` (was `PUT`)
- **CUS-005**: Update notification prefs → `PATCH /api/storefront/customer/notification-preferences` (was `PUT`)

### Security Requirements

- **SEC-001**: `GET /api/storefront/customer/all` must move to `ProfileFeature.Admin.cs`
- **SEC-002**: New admin route: `GET /api/admin/customer` with `DashboardFeatureMetadata.Customer.List` permission
- **SEC-003**: Storefront `ProfileFeature.Storefront.cs` removes the `/all` route constant

## 4. Interfaces & Data Contracts

### 4.1 Orders Endpoint Map (After)

```
GET    /api/storefront/orders                      List customer orders
GET    /api/storefront/orders/{id}                 Order detail
GET    /api/storefront/orders/{id}/tracking        Tracking timeline
POST   /api/storefront/orders/{id}/cancel          Cancel order
```

### 4.2 Shipping Endpoint Map (After)

```
GET    /api/storefront/shipping/methods            Available methods
GET    /api/storefront/shipping/rates              Rate list
GET    /api/storefront/shipping/calculate           Calculate cost (query params: shippingMethodId, orderId)
```

### 4.3 Customer Endpoint Method Changes

| Old | New | Route |
|-----|-----|-------|
| `PUT` | `PATCH` | `/customer` |
| `PUT` | `PATCH` | `/customer/addresses/{id}` |
| `PUT` | `PATCH` | `/customer/addresses/{id}/default` |
| `PUT` | `PATCH` | `/customer/wishlists/{id}` |
| `PUT` | `PATCH` | `/customer/notification-preferences` |

### 4.4 GET /customer/all → Admin Migration

```
OLD (storefront):
  GET /api/storefront/customer/all  → 200 with all customers

NEW (admin):
  GET /api/admin/customer           → 200 with paged customers (auth + permission required)
```

Route constant moves from `ProfileFeature.Storefront.cs` to `ProfileFeature.Admin.cs`.

## 5. Acceptance Criteria

- **AC-001**: `POST /api/storefront/orders/{id}/cancel` cancels order, returns 200
- **AC-002**: `PUT /api/storefront/orders/{id}/cancel` returns 404 (route doesn't exist)
- **AC-003**: `GET /api/storefront/orders` returns paged customer orders (prefix no longer includes `/ordering/`)
- **AC-004**: `GET /api/storefront/shipping/calculate?shippingMethodId=X&orderId=Y` returns calculated cost
- **AC-005**: `POST /api/storefront/shipping/calculate` returns 404
- **AC-006**: `PATCH /api/storefront/customer` with `{ firstName: "New" }` updates only firstName
- **AC-007**: `PATCH /api/storefront/customer/addresses/{id}` with `{ city: "NewCity" }` updates only city
- **AC-008**: `GET /api/storefront/customer/all` returns 404 (route removed from storefront)
- **AC-009**: `GET /api/admin/customer` with admin JWT returns paged customer list
- **AC-010**: `GET /api/admin/customer` with storefront JWT returns 403

## 6. Dependencies & External Integrations

### Internal Dependencies

- Customer account endpoints live in `Module/Customer/Features/Storefront/` — update `ProfileFeature.Storefront.cs`
- Orders endpoints live in `Module/Ordering/Features/Storefront/Orders/` — update `OrderingFeature.Storefront.cs`
- Shipping endpoints live in `Module/Shipping/Features/Storefront/Shipping/` — update `ShippingFeature.Storefront.cs`
- Admin customer list lives in `Module/Customer/Features/Admin/` — create or update admin feature

## 7. Rationale & Context

### Why POST for cancel order?

Cancel is a non-idempotent action — calling it twice on an already-cancelled order should produce an error (or be idempotent by design, but semantically it's an action, not a state replacement). `POST` is the HTTP method for non-idempotent resource actions. `PUT` implies "replace the resource state at this URI with the provided representation" — cancel is not a replacement.

### Why GET for shipping calculation?

Shipping calculation is a pure function: `f(method, order) → cost`. No state change on the server. No side effects. HTTP GET is the correct method for idempotent reads. The old `POST` was likely chosen because the request body carries method/order IDs — but these should be query parameters, not a body. The server performs no mutation.

### Why PATCH for all customer partial updates?

All 5 `PUT` endpoints accept partial JSON bodies (e.g., only `firstName` in profile update, only `city` in address update). This is PATCH semantics — merge the provided fields into the existing resource. `PUT` implies full resource replacement. Changing to `PATCH` aligns the HTTP method with the actual behavior.

### Why move GET /customer/all to admin?

The current `/api/storefront/customer/all` endpoint queries all customers regardless of auth scope. If it lacks admin permission check, any authenticated user can enumerate all platform customers — leaking PII (names, emails, addresses). Moving to admin with `HasPermission(DashboardFeatureMetadata.Customer.List)` closes this gap. The admin route also gets proper paging (it currently returns paged results — makes sense as an admin data table endpoint).
