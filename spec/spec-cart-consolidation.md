---
title: Cart Consolidation — Top-Level Resource with Payment Sub-Resources
version: 1.0
date_created: 2025-08-11
last_updated: 2025-08-11
owner: ReSys.Shop Platform
tags: design, cart, payment, checkout, api, rest
---

# Introduction

This specification defines the consolidation of cart, checkout, payment, and shipping operations into a single top-level `Cart` resource at `/api/storefront/cart`. Routes previously spread across `Ordering` (12 cart routes), `Billing` (3 payment routes), and `Shipping` (1 rate-selection route) are unified under one resource with proper sub-resource nesting. HTTP method violations are corrected atomically.

## 1. Purpose & Scope

### Purpose

Create a coherent cart-checkout-payment API surface where the cart is a self-contained resource with item, checkout, shipping, and payment sub-resources. This mirrors Spree Commerce v3's `/carts/{id}` pattern and eliminates the fragmented route structure where the checkout lifecycle crossed 3 module boundaries.

### Scope

- Move all cart routes from `api/storefront/ordering/cart` to `api/storefront/cart`
- Move payment intent creation/status/confirmation from `api/storefront/billing/payments/` to `api/storefront/cart/payment/`
- Move shipping rate selection from `api/storefront/ordering/cart/shipping-rate` to `api/storefront/cart/shipping-rate`
- Correct HTTP methods: `PUT` → `PATCH` for partial updates, `POST` → `GET` for reads
- Merge `POST /cart/empty` into `DELETE /cart/items`
- Rename `POST /cart/validate` to `GET /cart/checkout`
- Update `OrderingFeature.Storefront.cs` and `BillingFeature.Storefront.cs` route constants
- Audit `ValidateCheckout` handler for side effects before method change

### Out of Scope

- Cart domain entity changes
- Checkout flow logic changes
- Stripe integration changes
- Order creation logic changes

## 2. Definitions

| Term | Definition |
|------|------------|
| **Cart** | Shopping cart resource — a draft Order entity with `CheckoutState` |
| **Checkout State** | Order lifecycle: `Cart` → `Address` → `Delivery` → `Payment` → `Complete` |
| **Payment Intent** | Stripe PaymentIntent representing an initiated payment awaiting confirmation |
| **Shipping Rate** | Selected delivery method + cost for the cart's fulfillment |
| **Atomic Migration** | All route constants change in one commit — no gradual coexistence of old and new routes |

## 3. Requirements, Constraints & Guidelines

### Route Requirements

- **RT-001**: All cart routes use prefix `api/storefront/cart` (was `api/storefront/ordering/cart`)
- **RT-002**: Cart payment routes use prefix `api/storefront/cart/payment` (was `api/storefront/billing/payments`)
- **RT-003**: `GET /cart/checkout` validates checkout state (was `POST /cart/validate`)
- **RT-004**: `DELETE /cart/items` removes all line items (was `POST /cart/empty`)
- **RT-005**: `PATCH /cart` updates checkout details (was `PUT /cart`)
- **RT-006**: `PATCH /cart/items/{id}` updates line item quantity (was `PUT /cart/items/{id}`)
- **RT-007**: `POST /cart/payment/intent` creates payment intent (was `POST /billing/payments/create-intent`)
- **RT-008**: `GET /cart/payment/intent` gets active payment session (was `GET /billing/payments/status/{orderId}`)
- **RT-009**: `POST /cart/payment/intent/{id}/confirm` confirms payment (was `POST /billing/payments/confirm/{id}`)
- **RT-010**: The `POST /cart/associate` route is acceptable — it's an action on the cart resource (guest → user merge)
- **RT-011**: Stripe webhook stays at `POST /storefront/billing/webhooks/stripe` — it has no cart context

### URL Purity Requirements

- **URL-001**: No action verbs in URLs — `create-intent` removed, `empty` removed, `validate` removed
- **URL-002**: HTTP method encodes the action — `POST` creates, `PATCH` partially updates, `DELETE` removes
- **URL-003**: Resource nesting reflects ownership — payment belongs to cart, not standalone

### Feature File Requirements

- **FEA-001**: All cart endpoint files remain under `Module/Ordering/Features/Storefront/Cart/` (the Ordering module owns the Cart domain entity)
- **FEA-002**: Cart payment feature files move to `Module/Ordering/Features/Storefront/Cart/Payment/`
- **FEA-003**: Route constants update in `OrderingFeature.Storefront.cs` — all cart routes under `public static class Cart { ... }`
- **FEA-004**: `BillingFeature.Storefront.cs` removes `Payments.CreateIntent`, `Payments.Confirm`, `Payments.Status` route constants

### ValidateCheckout Audit Requirement

- **AUD-001**: Before changing `POST /cart/validate` to `GET /cart/checkout`, verify `ValidateCheckout` handler does not mutate state (no `SaveChangesAsync`, no `AdvanceCheckoutState` call)
- **AUD-002**: If handler mutates state, keep as `POST` but rename route to `POST /cart/checkout/validate`

### Atomic Migration Requirements

- **ATM-001**: All route constant changes in `OrderingFeature.Storefront.cs` committed as single commit
- **ATM-002**: No temporary `#if` or feature flags for old routes — constants are `public const string`, unchangeable at runtime
- **ATM-003**: Phase 9 (SPA migration) must update all frontend API references before deploy — no partial migration

## 4. Interfaces & Data Contracts

### 4.1 Complete Cart Route Map

```
POST   /api/storefront/cart                        Create cart
GET    /api/storefront/cart                        Get current user's cart
PATCH  /api/storefront/cart                        Update checkout (email, addresses)
DELETE /api/storefront/cart                        Delete cart
POST   /api/storefront/cart/associate              Guest cart → user
POST   /api/storefront/cart/items                  Add item
PATCH  /api/storefront/cart/items/{id}             Update line item qty
DELETE /api/storefront/cart/items                  Remove all items
DELETE /api/storefront/cart/items/{id}             Remove single item
GET    /api/storefront/cart/checkout               Validate checkout state
POST   /api/storefront/cart/checkout               Complete order
PATCH  /api/storefront/cart/shipping-rate          Select delivery rate
POST   /api/storefront/cart/payment/intent          Create payment intent
GET    /api/storefront/cart/payment/intent          Get active payment session
POST   /api/storefront/cart/payment/intent/{id}/confirm  Confirm payment
```

### 4.2 Old → New Route Mapping

| Old Route | New Route | Method Change |
|-----------|-----------|:---:|
| `POST /ordering/cart` | `POST /cart` | — |
| `GET /ordering/cart` | `GET /cart` | — |
| `PUT /ordering/cart` | `PATCH /cart` | PUT→PATCH |
| `DELETE /ordering/cart` | `DELETE /cart` | — |
| `POST /ordering/cart/associate` | `POST /cart/associate` | — |
| `POST /ordering/cart/items` | `POST /cart/items` | — |
| `PUT /ordering/cart/items/{id}` | `PATCH /cart/items/{id}` | PUT→PATCH |
| `DELETE /ordering/cart/items/{id}` | `DELETE /cart/items/{id}` | — |
| `POST /ordering/cart/empty` | `DELETE /cart/items` | Merged |
| `POST /ordering/cart/validate` | `GET /cart/checkout` | POST→GET |
| `POST /ordering/cart/checkout` | `POST /cart/checkout` | — |
| `POST /ordering/cart/shipping-rate` | `PATCH /cart/shipping-rate` | POST→PATCH |
| `POST /billing/payments/create-intent` | `POST /cart/payment/intent` | Renamed |
| `GET /billing/payments/status/{orderId}` | `GET /cart/payment/intent` | Merged, orderId from cart |
| `POST /billing/payments/confirm/{id}` | `POST /cart/payment/intent/{id}/confirm` | Renamed |

### 4.3 Cart Payment Intent — Handler Changes

`CreatePaymentIntent` handler currently receives `Command(OrderId, PaymentMethodId, ...)`. After migration:
- Handler injected with `IHttpContextAccessor`
- Cart identified by `HttpContext.Items["CartToken"]` (guest) or `currentUser.UserId` (auth)
- Handler queries the cart, validates `CheckoutState == Delivery`
- Proceeds with existing payment logic unchanged

### 4.4 Route Constant Definitions

```csharp
// OrderingFeature.Storefront.cs — after migration
public static partial class OrderingFeature
{
    public static class Storefront
    {
        public static class Cart
        {
            public static class Create
            {
                public const string Route = "api/storefront/cart";
                public const string Summary = "Create a new shopping cart";
                public const string Description = "Create a new shopping cart for the current user or guest";
            }
            public static class Get
            {
                public const string Route = "api/storefront/cart";
                public const string Summary = "Get the current cart";
                public const string Description = "Retrieve the current user's or guest's shopping cart";
            }
            // ... 13 more nested classes
        }
    }
}
```

## 5. Acceptance Criteria

- **AC-001**: `GET /api/storefront/cart` returns 200 with cart data for authenticated user or guest with `X-Cart-Token` header
- **AC-002**: `PATCH /api/storefront/cart` with `{ email: "test@example.com" }` updates only the email field — other fields unchanged
- **AC-003**: `DELETE /api/storefront/cart/items` removes all line items from cart, returns 200
- **AC-004**: `GET /api/storefront/cart/checkout` returns checkout validation state without mutating cart
- **AC-005**: `POST /api/storefront/cart/payment/intent` creates Stripe PaymentIntent and returns client secret
- **AC-006**: `POST /api/storefront/cart/payment/intent/{id}/confirm` confirms payment and marks it completed
- **AC-007**: No endpoint responds at `api/storefront/ordering/cart` — all routes moved
- **AC-008**: No endpoint responds at `api/storefront/billing/payments/create-intent` — route removed
- **AC-009**: `ValidateCheckout` handler verified side-effect-free before route method change to GET
- **AC-010**: `RemoveAllCartItems` handler (replacing `EmptyCart`) correctly calls `IStockReservationService.ReleaseReservationsAsync` when clearing items

## 6. Test Automation Strategy

### Unit Tests

- **Cart endpoints:** Verify correct HTTP method + route + response types
- **Payment intent creation:** Mock Stripe gateway, verify handler creates PaymentCapture entity with correct cart-derived order ID
- **ValidateCheckout audit:** Assert no `SaveChangesAsync` call in handler if changing to GET

### Integration Tests

- Full cart lifecycle: create → add item → update checkout → select shipping → create payment intent → checkout
- Guest cart association: create guest cart → login → associate → verify merged

## 7. Rationale & Context

### Why cart as top-level resource?

The cart is a session-scoped resource, not an ordering sub-resource. Spree places `/carts/{id}` at the same level as `/products`, `/orders`, `/account`. Our old nesting under `/ordering/cart` was an artifact of the Ordering module owning the Cart entity. But the checkout lifecycle spans Ordering, Shipping, and Billing. Making cart top-level acknowledges its cross-module role.

### Why DELETE /cart/items instead of POST /cart/empty?

`DELETE /cart/items` follows REST: delete all items from the items collection. `POST /cart/empty` uses an action verb in the URL (`empty`) and POST for a removal operation. The generic `DELETE` on the collection is clearer — it removes the sub-resource entirely.

### Why GET /cart/checkout instead of POST /cart/validate?

Validating checkout readiness is a read operation — it inspects state, checks completeness, returns requirements list. It does not (should not) mutate the cart. GET is the correct verb for idempotent reads. If the current handler does mutate state, the design is wrong and should be fixed rather than accepting POST.

### Why PATCH for partial updates?

`PUT /cart` currently replaces the entire checkout state (email, addresses, special instructions). But the frontend often updates only one field (e.g., shipping address). `PATCH` signals partial update — only provided fields change. This matches the HTTP specification and Spree's `PATCH /carts/{id}` convention.
