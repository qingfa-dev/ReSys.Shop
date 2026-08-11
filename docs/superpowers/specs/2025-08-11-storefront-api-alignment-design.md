# Storefront API Alignment Design

**Status:** Draft  
**Date:** 2025-08-11  
**Reference:** Spree Commerce Store API v3  
**Affected Modules:** Catalog, Inventory, Ordering, Shipping, Billing, Customer

---

## 1. Context

ReSys.Shop storesfront API spans 7 modules with 75 endpoints. Several structural issues exist:

- HTTP method misuse: `PUT` for partial updates, `POST` for state reads
- Route fragmentation: cart/checkout/payment/shipping split across Ordering, Billing, Shipping modules — when they form a single checkout workflow
- Inventory has zero storefront endpoints; stock info requires separate API call
- 5 cross-module namespace references violate modularity (Ordering → Inventory via `IStockItemService` DI)
- 7 features lack endpoint files (MediatR-only, not exposed to SPA)

Spree Commerce Store API v3 provides the reference pattern: cart is a self-contained resource with embedded fulfillments, payments, and delivery rates. This design aligns ReSys.Shop storefront to the Spree model while preserving the existing admin API structure.

---

## 2. Current State Inventory

### 2.1 Endpoint Summary by Module

| Module | Endpoints | Complete | Missing Endpoint | Missing Validator | Cross-Module Ref |
|--------|----------:|:--------:|:----------------:|:-----------------:|:----------------:|
| Catalog | 11 | 11 | 0 | 0 | 0 |
| Identity | 13 | 13 | 0 | 0 | 0 |
| Customer | 19 | 19 | 0 | 0 | 0 |
| Location | 6 | 6 | 0 | 0 | 0 |
| Ordering | 16 | 13 | 3 | 5 | 3 |
| Shipping | 3 | 3 | 0 | 0 | 0 |
| Billing | 6 | 6 | 0 | 0 | 2 |
| Inventory | 0 | 0 | 4 | 4 | 0 |
| **Total** | **74** | **71** | **7** | **9** | **5** |

### 2.2 Current Route Map

#### Catalog (`api/storefront/catalog/`)
```
GET    /products                         # Paged product list
GET    /products/{id}                    # Product detail
GET    /products/related                 # Related products
GET    /products/similar                 # Similar products (vector search)
GET    /products/images/{id}             # Image by ID
POST   /products/images/search           # Image search
GET    /products/images/inferences       # Inference models list
GET    /taxonomies                       # Taxonomy list
GET    /taxonomies/taxons                # Taxon list
GET    /option-types                     # Option type list
GET    /option-types/values              # Option value list
```

#### Identity (`api/storefront/identity/`)
```
POST   /auth/login/password              # Password login
POST   /auth/login/external              # External OAuth login
GET    /auth/login/external/providers    # External provider list
POST   /auth/register                    # Email registration
POST   /auth/logout                      # Logout
GET    /auth/sessions                    # Get current session
POST   /auth/sessions/refresh            # Refresh token
POST   /passwords/change                 # Change password
POST   /passwords/forgot                 # Request password reset
POST   /passwords/reset                  # Reset password
POST   /emails/change                    # Change email
POST   /emails/confirm                   # Confirm email
POST   /emails/resend                    # Resend verification
```

#### Customer (`api/storefront/customer/`)
```
GET    /customer                         # Get profile
GET    /customer/all                     # Admin: list all (should be admin route)
PUT    /customer                         # Update profile
DELETE /customer                         # Delete account
GET    /customer/addresses               # List addresses
POST   /customer/addresses               # Create address
GET    /customer/addresses/{id}          # Get address
PUT    /customer/addresses/{id}          # Update address
DELETE /customer/addresses/{id}          # Delete address
PUT    /customer/addresses/default       # Set default address
GET    /customer/notification-preferences
PUT    /customer/notification-preferences
GET    /customer/wishlists               # List wishlists
GET    /customer/wishlists/{id}          # Get wishlist
POST   /customer/wishlists               # Create wishlist
PUT    /customer/wishlists/{id}          # Update wishlist
DELETE /customer/wishlists/{id}          # Delete wishlist
POST   /customer/wishlists/{id}/items    # Add wishlist item
DELETE /customer/wishlists/{id}/items/{id} # Remove wishlist item
```

#### Location (`api/storefront/location/`)
```
GET    /countries                        # List countries
GET    /countries/{id}                   # Get country by ID
GET    /countries/by-iso/{code}          # Get country by ISO code
GET    /states                           # List states
GET    /states/{id}                      # Get state by ID
GET    /states/by-iso/{code}             # Get state by ISO code
```

#### Ordering — Cart (`api/storefront/ordering/`)
```
POST   /cart                             # Create cart
GET    /cart                             # Get cart
PUT    /cart                             # Update checkout (email, addresses)
DELETE /cart                             # Delete cart
POST   /cart/associate                   # Guest cart → user
POST   /cart/items                       # Add item
PUT    /cart/items/{id}                  # Update line item qty
DELETE /cart/items/{id}                  # Remove line item
POST   /cart/empty                       # Remove all items
POST   /cart/checkout                    # Complete checkout
POST   /cart/validate                    # Validate checkout state
POST   /cart/shipping-rate               # Select shipping rate
```

#### Ordering — Orders (`api/storefront/ordering/`)
```
GET    /orders                           # List customer orders
GET    /orders/{id}                      # Order detail
GET    /orders/{id}/tracking             # Tracking timeline
PUT    /orders/{id}/cancel               # Cancel order
```

#### Ordering — MediatR-Only (no endpoints)
- `AdvanceCheckoutState` — advances cart through checkout steps (Address → Delivery → Payment → Complete)
- `GetCartForCheckout` — returns cart line items + checkout state for checkout UI
- `GetCartForShipping` — returns cart weight/total for shipping calculation

#### Shipping (`api/storefront/shipping/`)
```
GET    /methods                          # Available shipping methods
GET    /rates                            # Shipping rates list
POST   /calculate                        # Calculate shipping cost
```

#### Billing (`api/storefront/billing/`)
```
GET    /payment-methods                  # Available payment methods
POST   /payment-methods/setup-intent     # Create Stripe SetupIntent
POST   /payments/create-intent           # Create payment intent
POST   /payments/confirm/{id}            # Confirm payment
GET    /payments/status/{orderId}        # Payment status
POST   /webhooks/stripe                  # Stripe webhook receiver
```

#### Billing — MediatR-Only (no endpoints)
- `GetPaymentForCheckout` — query payment status for checkout flow
- `MarkPaymentPaid` — mark payment as completed (called by webhook processor)

#### Inventory — MediatR-Only (no endpoints; 0 storefront routes)
- `ReserveCartStock` — batch reserve cart stock (called by Ordering.AddToCart + Billing.CreatePaymentIntent)
- `ConsumeCartStockReservations` — fulfill reservations (called by Ordering.CreateOrderFromCart)
- `ReleaseCartStockReservations` — release cart reservations (called by Billing failure paths)

### 2.3 Known Defects

#### HTTP Method Violations
| Endpoint | Current | Correct | Reason |
|----------|---------|---------|--------|
| Update checkout (cart) | `PUT /ordering/cart` | `PATCH /storefront/cart` | Partial update |
| Update line item qty | `PUT /ordering/cart/items/{id}` | `PATCH /storefront/cart/items/{id}` | Partial update |
| Validate checkout | `POST /ordering/cart/validate` | `GET /storefront/cart/checkout` | Read, not action |
| Cancel order | `PUT /ordering/orders/{id}/cancel` | `POST /storefront/orders/{id}/cancel` | Action, not idempotent |
| Select shipping rate | `POST /ordering/cart/shipping-rate` | `PATCH /storefront/cart/shipping-rate` | State mutation |
| Calculate shipping | `POST /shipping/calculate` | `GET /storefront/shipping/calculate` | Idempotent query |
| Update profile | `PUT /customer` | `PATCH /storefront/customer` | Partial update |
| Update address | `PUT /customer/addresses/{id}` | `PATCH /storefront/customer/addresses/{id}` | Partial update |
| Update wishlist | `PUT /customer/wishlists/{id}` | `PATCH /storefront/customer/wishlists/{id}` | Partial update |

#### Action Verbs in URLs
| Endpoint | Issue | Proposed |
|----------|-------|----------|
| `/billing/payments/create-intent` | `create-intent` is verb | `/cart/payment/intent` (POST encodes create) |
| `/billing/payment-methods/setup-intent` | `setup-intent` is verb | `/billing/payment-methods/setup-intent` (acceptable — Stripe convention) |
| `/ordering/cart/empty` | `empty` is verb | `DELETE /storefront/cart/items` (DELETE encodes removal) |
| `/ordering/cart/associate` | `associate` is verb | `POST /storefront/cart/associate` (acceptable — action on resource) |

#### Cross-Module Namespace Violations
| File | Violation | Fix |
|------|----------|-----|
| `Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` | Injects `IStockItemService` directly | Replace with MediatR query to Inventory module |
| `Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` | Uses `OrderInventoryService` with direct `StockItem` entity access | Replace with MediatR command to Inventory |
| `Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` | Same `OrderInventoryService` DI | Same fix as above |
| `Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs` | Same `OrderInventoryService` | Same fix as above |
| `Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` | References `Module.Inventory.*` namespaces (3 imports) | Already uses MediatR for ReserveCartStock — clean path |
| `Billing/Features/Storefront/Payment/Status/GetPaymentStatus.cs` | References `Module.Ordering.Domain.Orders.Order` directly | Replace with MediatR query |

---

## 3. Spree Commerce Reference Pattern

Spree v3 Store API (`/api/v3/store/`) groups resources as:

```
/carts/{id}               # Complete cart resource — embedded items, payments, 
                          # fulfillments, addresses, delivery rates, discounts
/carts/{id}/items         # Line item CRUD
/carts/{id}/payment/      # Payment session lifecycle (create → get → update → complete)
/carts/{id}/select_delivery_rate  # Rate selection
/orders/{id}              # Read-only order detail
/account/orders           # Customer order history
/account/addresses        # Customer addresses
/account/profile          # Customer profile
/account/payment_methods  # Saved payment cards
/products                 # Product catalog (stock info embedded in variant)
/delivery_methods         # Available delivery methods
```

Key design decisions:

1. **Cart is flat resource** — not nested under `/ordering/cart`. Cart belongs to customer session, not to ordering module.
2. **Payment lives under cart** — payment session is a cart sub-resource, not standalone. Spree creates payment sessions under `/carts/{id}/payment/session`.
3. **Shipping rate selection lives under cart** — `/carts/{id}/select_delivery_rate`.
4. **Stock embedded in product** — `GET /products` returns `in_stock`, `backorderable`, `available` on each variant. No separate availability call.
5. **HTTP methods precise** — `PATCH` for partial updates, `POST` for creation and actions, `DELETE` for removal.
6. **Account is separate** — customer-facing profile, addresses, order history, payment methods scoped to `/account/`.

---

## 4. Proposed Architecture

### 4.1 Route Map — After Alignment

#### Cart + Checkout (consolidated from Ordering + Billing + Shipping) — 12 routes

Cart is now a top-level resource. Checkout, payment, and shipping are sub-resources.

```
POST   /storefront/cart                        Create cart
GET    /storefront/cart                        Get cart
PATCH  /storefront/cart                        Update checkout (email, addresses)
DELETE /storefront/cart                        Delete cart
POST   /storefront/cart/associate              Guest cart → user
POST   /storefront/cart/items                  Add item (handles stock reserve internally)
PATCH  /storefront/cart/items/{id}             Update line item qty
DELETE /storefront/cart/items                  Remove all items
DELETE /storefront/cart/items/{id}             Remove single item

GET    /storefront/cart/checkout               Validate checkout state
POST   /storefront/cart/checkout               Complete order
PATCH  /storefront/cart/shipping-rate          Select delivery rate
POST   /storefront/cart/payment/intent          Create payment intent
GET    /storefront/cart/payment/intent          Get active payment session
POST   /storefront/cart/payment/intent/{id}/confirm  Confirm payment
```

**Design rationale:** The cart token (`X-Cart-Token` header for guests, auth JWT for logged-in users) identifies the cart — no `cartId` in URL. Endpoints that require a specific payment session (confirm) include the `{id}`. The `POST /cart/payment/intent` creates the intent automatically for the identified cart.

**Middleware:** A new `CartTokenMiddleware` extracts `X-Cart-Token` from the request header and sets `HttpContext.Items["CartToken"]`. Handlers read from Items — no per-endpoint header parsing. Why middleware: reused by all cart-scoped endpoints, avoids copy-paste.

#### Orders — 4 routes

```
GET    /storefront/orders                      List customer orders
GET    /storefront/orders/{id}                 Order detail
GET    /storefront/orders/{id}/tracking        Tracking timeline
POST   /storefront/orders/{id}/cancel          Cancel order
```

Route prefix changes from `api/storefront/ordering/orders` to `api/storefront/orders`. Module name (`ordering/`) removed — the resource name (`orders`) is sufficient.

#### Shipping (read-only info) — 3 routes

```
GET    /storefront/shipping/methods            Available methods
GET    /storefront/shipping/rates              Rate list
GET    /storefront/shipping/calculate           Calculate cost for order/method
```

`POST /shipping/calculate` becomes `GET` — it's an idempotent computation, not a state change. Query parameters carry `shippingMethodId` and `orderId`.

#### Billing (standalone) — 3 routes

```
GET    /storefront/billing/payment-methods          Available payment types
POST   /storefront/billing/payment-methods/setup-intent   Stripe SetupIntent
POST   /storefront/billing/webhooks/stripe            Stripe webhook
```

Payment creation and confirmation moved under cart. Only payment method discovery and webhook remain standalone.

#### Inventory (new) — 4 routes

```
GET    /storefront/inventory/stock-items/{variantId}/availability    Per-variant per-location stock
POST   /storefront/inventory/stock-reservations                       Reserve single item
GET    /storefront/inventory/stock-reservations                       List cart reservations
DELETE /storefront/inventory/stock-reservations/{id}                  Release reservation
```

**Design rationale for `stock-items/{variantId}/availability`:** The store SPA's `ProductDetailView` needs per-location stock breakdown for a single variant (showing "Available in 3 locations", per-location counts, backorder availability). This is a variant-scoped resource, hence `stock-items/{variantId}/availability`.

**Design rationale for `stock-reservations`:** The `POST` endpoint creates a single-item reservation (different from the inter-module batch `ReserveCartStock` command). The `GET` endpoint lists active reservations for the cart identified by `X-Cart-Token` header. `DELETE` releases one reservation. Inter-module batch operations (`ReserveCartStock`, `ConsumeCartStockReservations`, `ReleaseCartStockReservations`) remain as MediatR-only — they are consumed by Ordering and Billing modules, not the SPA.

**Why separate single-reserve handler:** The existing `ReserveCartStock` handler uses a batch cart-level transaction with `RepeatableRead` isolation and takes `CartId: Guid` + `IReadOnlyList<ReserveLineItem>`. The storefront needs per-item reserve with `X-Cart-Token: string` header and `ReserveStockRequest` (single variant + single location + quantity). Creating a new handler avoids:
- Awkward mapping of `CartToken:string` → `CartId:Guid`
- Wrapping single item into a list
- Unwrapping `IReadOnlyList<Guid>` response to single `CartReservation` detail
- Unnecessary `RepeatableRead` transaction for 1 item

Both handlers call the same domain factory method `StockReservationMethod.Reserve(...)` — no business logic duplication.

#### Catalog — 10 routes (1 removed)

```
GET    /storefront/catalog/products                     List (response now includes `inStock`, `availableQuantity`, `backorderable`)
GET    /storefront/catalog/products/{id}                Detail (response now includes variant-level stock)
GET    /storefront/catalog/products/related             Related products
GET    /storefront/catalog/products/similar             Similar products
GET    /storefront/catalog/products/images/{id}         Image by ID
POST   /storefront/catalog/products/images/search       Image search
GET    /storefront/catalog/taxonomies                   Taxonomy list
GET    /storefront/catalog/taxonomies/taxons            Taxon list
GET    /storefront/catalog/option-types                 Option type list
GET    /storefront/catalog/option-types/values          Option value list
```

`POST /products/images/inferences` removed — it was a legacy debug endpoint listing ML models available for image inference. Not needed in production.

**Stock embedding:** `GetStorefrontProducts` and `GetProductDetail` handlers already call `IStockItemService.GetStockAvailabilityAsync` internally. The response DTO gains `inStock`, `availableQuantity`, `backorderable` fields at both product and variant level. The store SPA drops its separate `checkAvailability()` API call from `useAvailability` composable — the product list/detail responses already carry stock status.

#### Customer Account — 19 routes (3 method changes)

```
GET    /storefront/customer                              Get profile
PATCH  /storefront/customer                              Update profile (was PUT)
DELETE /storefront/customer                              Delete account
GET    /storefront/customer/addresses                    List addresses
POST   /storefront/customer/addresses                    Create address
GET    /storefront/customer/addresses/{id}               Get address
PATCH  /storefront/customer/addresses/{id}               Update address (was PUT)
DELETE /storefront/customer/addresses/{id}               Delete address
PATCH  /storefront/customer/addresses/{id}/default       Set default address (was PUT)
GET    /storefront/customer/notification-preferences
PATCH  /storefront/customer/notification-preferences     (was PUT)
GET    /storefront/customer/wishlists                    List wishlists
POST   /storefront/customer/wishlists                    Create wishlist
GET    /storefront/customer/wishlists/{id}               Get wishlist
PATCH  /storefront/customer/wishlists/{id}               Update wishlist (was PUT)
DELETE /storefront/customer/wishlists/{id}               Delete wishlist
POST   /storefront/customer/wishlists/{id}/items         Add item
DELETE /storefront/customer/wishlists/{id}/items/{id}    Remove item
```

`GET /customer/all` reviewed for removal — if it exposes all customers to storefront (no admin auth), it's a security issue. If it has admin auth, it belongs under `/admin/`.

#### Unchanged Modules

| Module | Keep Routes As-Is | Reason |
|--------|:---:|--------|
| Identity | 13 | Auth endpoints follow established convention. Routes already well-structured. |
| Location | 6 | Countries/states lookup. Clean GET-only resource. |

### 4.2 Endpoint Count Summary

| Module | Before | After | Delta | Notes |
|--------|-------:|------:|------:|-------|
| Catalog | 11 | 10 | -1 | Removed inferences endpoint; embedded stock |
| Cart | 0 | 15 | +15 | New top-level resource (moved from Ordering + Billing) |
| Orders | 0 | 4 | +4 | Extracted from Ordering |
| Ordering | 16 | 0 | -16 | Cart and Orders moved out |
| Billing | 6 | 3 | -3 | Payment creation moved under Cart |
| Shipping | 3 | 3 | 0 | Calculate method changed POST→GET |
| Inventory | 0 | 4 | +4 | New resource |
| Customer | 19 | 19 | 0 | Method changes only (PUT→PATCH) |
| Identity | 13 | 13 | 0 | Unchanged |
| Location | 6 | 6 | 0 | Unchanged |
| **Total** | **74** | **77** | **+3** | |

---

## 5. Implementation Plan

### 5.1 Feature File Reorganization

#### Cart (consolidated under `Module/Ordering/Features/Storefront/Cart/`)

Move cart feature files from Ordering module and create new payment sub-features:

```
Cart/
  Shared/
    Models/       Cart.Model.Response.cs (add payment session fields)
    Mappings/     Cart.Mapping.Model.cs, Cart.Mapping.Domain.cs
    Validators/   Cart.Validator.cs
  Create/
    CreateCart.cs, CreateCart.Response.cs, CreateCart.Endpoint.cs, CreateCart.Validator.cs
  Get/
    GetCart.cs, GetCart.Response.cs, GetCart.Endpoint.cs, GetCart.Validator.cs
  Update/
    UpdateCart.cs, UpdateCart.Request.cs, UpdateCart.Endpoint.cs, UpdateCart.Validator.cs
  Delete/
    DeleteCart.cs, DeleteCart.Endpoint.cs
  Associate/
    AssociateCart.cs, AssociateCart.Request.cs, AssociateCart.Response.cs, AssociateCart.Endpoint.cs, AssociateCart.Validator.cs
  AddItem/
    AddToCart.cs, AddToCart.Request.cs, AddToCart.Response.cs, AddToCart.Endpoint.cs, AddToCart.Validator.cs
  RemoveItem/
    RemoveCartItem.cs, RemoveCartItem.Endpoint.cs, RemoveCartItem.Validator.cs
  RemoveAllItems/
    RemoveAllCartItems.cs, RemoveAllCartItems.Endpoint.cs  # Replaces EmptyCart
  UpdateItemQuantity/
    UpdateCartItemQuantity.cs, UpdateCartItemQuantity.Request.cs, UpdateCartItemQuantity.Endpoint.cs, UpdateCartItemQuantity.Validator.cs
  Checkout/
    Validate/
      ValidateCheckout.cs, ValidateCheckout.Endpoint.cs    # Replaces GET /cart/checkout
    Complete/
      CreateOrderFromCart.cs, CreateOrderFromCart.Request.cs, CreateOrderFromCart.Response.cs, CreateOrderFromCart.Endpoint.cs, CreateOrderFromCart.Validator.cs
  ShippingRate/
    SelectShippingRate.cs, SelectShippingRate.Request.cs, SelectShippingRate.Endpoint.cs, SelectShippingRate.Validator.cs
  Payment/
    Shared/
      Models/       CartPayment.Model.Response.cs, CartPayment.Model.Request.cs
      Mappings/     CartPayment.Mapping.cs
    CreateIntent/
      CreatePaymentIntent.cs, CreatePaymentIntent.Request.cs, CreatePaymentIntent.Response.cs, CreatePaymentIntent.Endpoint.cs, CreatePaymentIntent.Validator.cs
    GetIntent/
      GetPaymentIntent.cs, GetPaymentIntent.Response.cs, GetPaymentIntent.Endpoint.cs
    Confirm/
      ConfirmPayment.cs, ConfirmPayment.Request.cs, ConfirmPayment.Response.cs, ConfirmPayment.Endpoint.cs, ConfirmPayment.Validator.cs
```

#### Orders (under `Module/Ordering/Features/Storefront/Orders/`)

Existing ordering order features stay. Route prefix moves from `ordering/orders` to `orders`.

#### Inventory — Four New Features (under `Module/Inventory/Features/Storefront/`)

```
Storefront/
  Shared/
    Models/
      StockItem.Model.Response.cs       # AvailabilityEntry DTO
      StockReservation.Model.Request.cs  # ReserveStockRequest DTO
      StockReservation.Model.Response.cs # CartReservation, CartReservationStatus DTOs
    Mappings/
      StockItem.Mapping.Model.cs        # domain → availability DTO
      StockReservation.Mapping.Model.cs  # domain → reservation DTOs
    Validators/
      Inventory.Storefront.Validator.cs  # Extend with stock location & quantity rules
  StockItems/
    GetAvailability/
      GetStockAvailability.cs           # Handler + Query
      GetStockAvailability.Parameters.cs
      GetStockAvailability.Response.cs
      GetStockAvailability.Endpoint.cs
      GetStockAvailability.Validator.cs
  StockReservations/
    Reserve/
      ReserveCartReservation.cs         # New single-item handler
      ReserveCartReservation.Request.cs
      ReserveCartReservation.Response.cs
      ReserveCartReservation.Endpoint.cs
      ReserveCartReservation.Validator.cs
    Get/
      GetCartReservations.cs            # List cart reservations
      GetCartReservations.Response.cs
      GetCartReservations.Parameters.cs
      GetCartReservations.Endpoint.cs
      GetCartReservations.Validator.cs
    Release/
      ReleaseCartReservation.cs         # Single reservation release
      ReleaseCartReservation.Endpoint.cs
      ReleaseCartReservation.Validator.cs
    ReserveCart/                        # EXISTING — inter-module only, no endpoint
      ReserveCartStock.cs, Request, Response, Validator  (unchanged)
    ReleaseCart/                        # EXISTING — inter-module only, no endpoint
      ReleaseCartStockReservations.cs, Request, Validator  (unchanged)
    ConsumeCart/                        # EXISTING — inter-module only, no endpoint
      ConsumeCartStockReservations.cs, Request, Validator  (unchanged)
```

### 5.2 Route Constant Files to Update

| File | Change |
|------|--------|
| `Inventory/Features/Shared/InventoryFeature.Storefront.cs` | Add StockItems.GetAvailability, StockReservations.Reserve/Get/Release routes + summaries |
| `Inventory/Features/Shared/InventoryFeature.Tags.cs` | Unchanged (already has StockItem, StockReservation tags) |
| `Ordering/Features/Shared/OrderingFeature.Storefront.cs` | Rewrite — move all cart routes to `api/storefront/cart`, orders to `api/storefront/orders` |
| `Shipping/Features/Shared/ShippingFeature.Storefront.cs` | Update Calculate from POST to GET |
| `Billing/Features/Shared/BillingFeature.Storefront.cs` | Remove CreateIntent, Confirm, Status (moved under Cart). Keep PaymentMethods and Webhooks. |
| Catalog product response DTOs | Add `inStock: bool`, `availableQuantity: int`, `backorderable: bool` to variant DTO |

### 5.3 Add CartTokenMiddleware

File: `Shared/Security/Cart/CartTokenMiddleware.cs`
- Reads `X-Cart-Token` header
- Sets `HttpContext.Items["CartToken"]` as string
- Registered in `Program.cs` before auth middleware
- No-op if header absent (auth users don't need cart token)

### 5.4 Cross-Module Reference Fixes

| Issue | Fix |
|-------|-----|
| `Ordering.UpdateCartItemQuantity` uses `IStockItemService.IsAvailableAsync` | Replace with MediatR query `CheckStockAvailability.Query` via `ISender` |
| `Ordering.CancelOrder` uses `OrderInventoryService` | Replace with MediatR command `ReleaseCartStockReservations.Command` via `ISender` |
| `Ordering.CancelOrderAdmin` uses `OrderInventoryService` | Same as above |
| `Ordering.UpdateOrderStatus` uses `OrderInventoryService` | Same as above |
| `Billing.GetPaymentStatus` references `Module.Ordering.Domain.Orders.Order` | Query order existence via MediatR `GetOrderExistence.Query` or accept that the Domain reference is unavoidable (same assembly?) — verify |

### 5.5 Implementation Phases

**Phase 1: Inventory Storefront (3 features, 4 endpoints)**
- Create `Storefront/Shared/` models, mappings, validators
- Create `StockItems/GetAvailability/` — GET endpoint + handler
- Create `StockReservations/Reserve/` — POST endpoint + new single-item handler
- Create `StockReservations/Get/` — GET endpoint + handler
- Create `StockReservations/Release/` — DELETE endpoint + handler
- Create `CartTokenMiddleware`
- Update `InventoryFeature.Storefront.cs` with route constants
- Update Store SPA: `cartReservationApi.ts` and `availabilityApi.ts` to call new endpoints

**Phase 2: Cart Consolidation (move 12 Ordering + 3 Billing features under Cart)**
- Update `OrderingFeature.Storefront.cs` — rewrite route constants for cart under `api/storefront/cart`
- Update cart endpoints: method corrections (PUT→PATCH), route changes
- Move `CreatePaymentIntent`, `GetPaymentStatus`, `ConfirmPayment` into Cart/Payment/
- Update `BillingFeature.Storefront.cs` — remove moved routes
- Update handlers to read `CartToken` from `HttpContext.Items` instead of route params
- Remove `GET /ordering/cart/empty`, merge into `DELETE /storefront/cart/items`
- Remove `POST /ordering/cart/validate`, replace with `GET /storefront/cart/checkout`

**Phase 3: Orders + Shipping Cleanup**
- Update Orders route prefix: `api/storefront/ordering/orders` → `api/storefront/orders`
- Change `PUT /orders/{id}/cancel` to `POST /orders/{id}/cancel`
- Change `POST /shipping/calculate` to `GET /shipping/calculate`
- Update `ShippingFeature.Storefront.cs`

**Phase 4: Cross-Module Reference Fixes**
- Replace `IStockItemService` DI in Ordering handlers with MediatR queries
- Remove `OrderInventoryService` — replace with MediatR commands
- Verify Billing.Order domain reference
- Remove `Module.Inventory.Services` and `Module.Inventory.Domain` imports from Ordering

**Phase 5: Catalog Stock Embedding**
- Add stock fields to `StoreVariant.Model.cs` DTO
- Update `Store.Variant.Mapping.cs` to include stock from `IStockItemService.GetStockAvailabilityAsync`
- Update Store SPA: remove `useAvailability` separate API call, read from product response

**Phase 6: Customer Account Method Fixes**
- Change `PUT /customer` → `PATCH /storefront/customer`
- Change `PUT /customer/addresses/{id}` → `PATCH /storefront/customer/addresses/{id}`
- Change `PUT /customer/wishlists/{id}` → `PATCH /storefront/customer/wishlists/{id}`
- Review `GET /customer/all` for removal or move to admin

**Phase 7: Store SPA Alignment**
- Update all frontend API service calls to new routes
- Update types/interfaces to match new DTO shapes
- Remove `availabilityApi.ts` and `useAvailability.ts` composable (embedded in product)
- Update `checkoutApi.ts` routes for cart/payment/webhook changes
- Update `cartApi.ts` routes for cart changes

---

## 6. Risks and Mitigations

| Risk | Severity | Mitigation |
|------|:--------:|------------|
| Breaking all Store SPA API calls | High | Phase 7 updates all frontend service files. Run `pnpm run test:unit` after each phase. |
| Inter-module MediatR calls break | Medium | Each phase uses feature flags: old and new routes coexist temporarily. Remove old after full migration verified. |
| `CreatePaymentIntent` handler complex — moving under Cart may break | Medium | Handler only imports namespace references. Route change and `CartToken` middleware read is the only code change. Business logic stays. |
| 5 cross-module DI references | Medium | Each fix is a 1:1 replacement — inject `ISender` instead of `IStockItemService`, send MediatR query. |
| Response DTO breaking changes (catalog) | Low | Additive change only — new fields, no field removal. |

---

## 7. Success Criteria

1. All storefront endpoints follow REST conventions — correct HTTP methods, no action verbs in URLs
2. Cart is a self-contained top-level resource with payment + shipping as sub-resources
3. Inventory has 4 functional storefront REST endpoints with complete feature files (Handler, Request, Response, Endpoint, Validator, route constants)
4. Zero cross-module namespace references in Ordering and Billing module storefront handlers
5. All 77 endpoints have complete feature file sets (no missing endpoints or validators)
6. Store SPA compiles and all unit tests pass against new route structure
7. `scripts/check-cross-module-refs.sh` reports zero violations
8. `scripts/check-feature-conventions.sh` passes for all storefront features
