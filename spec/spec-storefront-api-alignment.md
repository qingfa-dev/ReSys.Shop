---
title: Storefront API Alignment with Spree Commerce v3
version: 1.0
date_created: 2025-08-11
last_updated: 2025-08-11
owner: ReSys.Shop Platform
tags: design, architecture, api, storefront, spree, rest
---

# Introduction

This specification defines the architectural alignment of ReSys.Shop's storefront REST API with the Spree Commerce Store API v3 pattern. It addresses HTTP method misuse, route fragmentation across modules, redundant CQRS mediation layers in the Inventory module, and missing storefront endpoints. The goal is a clean, RESTful, Spree-compatible storefront API while preserving existing admin API structures.

## 1. Purpose & Scope

### Purpose

Define the full storefront API endpoint map, resource grouping, HTTP method conventions, service layer interfaces, and migration plan to align the existing 74 endpoints across 7 modules with Spree v3 patterns.

### Scope

- **In scope:** All 7 storefront modules (Catalog, Identity, Customer, Location, Ordering, Shipping, Billing, Inventory), their routes, feature file organization, service interfaces, frontend API consumers, and cross-module references.
- **Out of scope:** Admin API (unchanged), Identity auth flow redesign, Location/countries data model, third-party payment gateway internals.

### Audience

Backend developers, frontend developers, QA engineers, and platform architects implementing or consuming the storefront API.

### Assumptions

- `.NET 10` runtime, `Carter` minimal APIs, `MediatR` CQRS (retained for admin; dropped for inventory storefront services)
- `FluentValidation` for request validation
- `Result<T>` pattern for all service/endpoint returns
- `Vue 3` / `TypeScript` / `pnpm` Store SPA
- Existing `IStockReservationService` and `IStockItemService` interfaces exist in Inventory module
- All business modules share one `Module` assembly

## 2. Definitions

| Term | Definition |
|------|------------|
| **CQRS** | Command Query Responsibility Segregation — MediatR-based handler pattern in features |
| **Cart Token** | Guest cart identifier sent via `X-Cart-Token` HTTP header |
| **CartTokenMiddleware** | ASP.NET middleware extracting `X-Cart-Token` header into `HttpContext.Items["CartToken"]` |
| **Service Layer** | Domain service interfaces (`IStockReservationService`, `IStockItemService`) with `Result<T>` contracts |
| **Result<T>** | Discriminated union: `Result<T>.Success(value)` or `Result<T>.Failure(errors)` |
| **Vertical Slice** | Feature directory with `{ActionName}.cs` (Handler), `.Request.cs`, `.Response.cs`, `.Endpoint.cs`, `.Validator.cs` |
| **Atomically** | All route constants change in one commit; no gradual old+new coexistence |
| **Option A** | Cart handlers orchestrate inventory internally via services; SPA never calls inventory endpoints directly |

## 3. Requirements, Constraints & Guidelines

### Architectural Requirements

- **ARC-001**: Inventory module must expose `IStockReservationService` and `IStockItemService` as primary contracts. Other modules inject these services directly — no MediatR indirection for inventory operations.
- **ARC-002**: Cart is a top-level resource (`/storefront/cart`), not nested under Ordering module.
- **ARC-003**: Payment intent creation and confirmation are sub-resources of cart (`/storefront/cart/payment/intent`).
- **ARC-004**: Stripe webhook stays standalone under Billing (`/storefront/billing/webhooks/stripe`) — no cart context.
- **ARC-005**: `CartTokenMiddleware` injects `X-Cart-Token` header value into `HttpContext.Items["CartToken"]`. No route param or query param carries cart tokens.
- **ARC-006**: `GET /customer/all` must move to `ProfileFeature.Admin.cs` with admin auth check.

### HTTP Method Requirements

- **MET-001**: `PATCH` for partial resource updates (was `PUT` in 6 endpoints)
- **MET-002**: `POST` for actions and creation (cancel order, create resources)
- **MET-003**: `GET` for idempotent reads (validate checkout, calculate shipping)
- **MET-004**: `DELETE` for resource removal (empty cart = `DELETE /cart/items`)

### Service Layer Requirements

- **SVC-001**: `IStockReservationService` must expose: `ReserveAsync`, `ReleaseReservationsAsync`, `ExpireReservationsAsync`, `GetReservationsForCartAsync`, `ConsumeForOrderAsync`, `ReleaseReservationAsync`
- **SVC-002**: `IStockItemService` must expose: `GetStockAvailabilityAsync`, `GetAvailabilityForCartAsync`, `IsAvailableAsync`, `AdjustStockAsync`, `RestockAsync`, `GetSnapshotForVariantAsync`, `GetStockSummaryAsync`
- **SVC-003**: All service methods return `Result<T>` with domain error codes.

### Deletion Requirements

- **DEL-001**: Delete `OrderInventoryService.cs` — replaced by `IStockItemService.AdjustStockAsync`
- **DEL-002**: Delete 3 CQRS handler directories: `ReserveCart/`, `ConsumeCart/`, `ReleaseCart/` (10 files)
- **DEL-003**: Delete `ApiTests/Ordering/demo-flow.http` — breaks with new routes, rewritten later
- **DEL-004**: Delete `Storefront/Shared/Models/Store.StockItem.Model.cs` — empty file (0 bytes)
- **DEL-005**: Delete `Catalog/Products/Images/Inferences/` — legacy debug endpoint

### Guidelines

- **GUD-001**: Storefront REST endpoints for inventory are thin wrappers — Endpoint → Service → Result. No Handler.cs needed if zero business logic (simple delegation).
- **GUD-002**: Admin feature pattern (Handler + MediatR CQRS) stays unchanged for admin endpoints.
- **GUD-003**: All route constants must live in `{Module}Feature.Storefront.cs` files, matching admin convention.
- **GUD-004**: Response DTOs inherit from shared base types in `Shared/Models/` directory.

## 4. Interfaces & Data Contracts

### 4.1 Complete Storefront Endpoint Map (After Alignment)

#### Cart (`/storefront/cart`) — 15 routes

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/cart` | Create cart |
| GET | `/cart` | Get current user's cart |
| PATCH | `/cart` | Update checkout (email, addresses) |
| DELETE | `/cart` | Delete cart |
| POST | `/cart/associate` | Guest cart → authenticated user |
| POST | `/cart/items` | Add item to cart |
| PATCH | `/cart/items/{id}` | Update line item quantity |
| DELETE | `/cart/items` | Remove all items |
| DELETE | `/cart/items/{id}` | Remove single item |
| GET | `/cart/checkout` | Validate checkout state |
| POST | `/cart/checkout` | Complete order from cart |
| PATCH | `/cart/shipping-rate` | Select delivery rate |
| POST | `/cart/payment/intent` | Create payment intent |
| GET | `/cart/payment/intent` | Get active payment session |
| POST | `/cart/payment/intent/{id}/confirm` | Confirm payment |

#### Orders (`/storefront/orders`) — 4 routes

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/orders` | List customer orders |
| GET | `/orders/{id}` | Order detail |
| GET | `/orders/{id}/tracking` | Tracking timeline |
| POST | `/orders/{id}/cancel` | Cancel order |

#### Catalog (`/storefront/catalog`) — 10 routes

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/products` | Paged product list (stock embedded) |
| GET | `/products/{id}` | Product detail (stock per variant embedded) |
| GET | `/products/related` | Related products |
| GET | `/products/similar` | Similar products |
| GET | `/products/images/{id}` | Image by ID |
| POST | `/products/images/search` | Image-based search |
| GET | `/taxonomies` | Taxonomy list |
| GET | `/taxonomies/taxons` | Taxon list |
| GET | `/option-types` | Option type list |
| GET | `/option-types/values` | Option value list |

#### Inventory (`/storefront/inventory`) — 4 routes

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/stock-items/{variantId}/availability` | Per-variant per-location stock |
| POST | `/stock-reservations` | Reserve stock for variant |
| GET | `/stock-reservations` | List active cart reservations |
| DELETE | `/stock-reservations/{id}` | Release single reservation |

#### Shipping (`/storefront/shipping`) — 3 routes

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/methods` | Available shipping methods |
| GET | `/rates` | Shipping rates list |
| GET | `/calculate` | Calculate shipping cost |

#### Billing (`/storefront/billing`) — 3 routes

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/payment-methods` | Available payment methods |
| POST | `/payment-methods/setup-intent` | Stripe SetupIntent |
| POST | `/webhooks/stripe` | Stripe webhook receiver |

#### Customer (`/storefront/customer`) — 18 routes

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/customer` | Get profile |
| PATCH | `/customer` | Update profile |
| DELETE | `/customer` | Delete account |
| GET | `/customer/addresses` | List addresses |
| POST | `/customer/addresses` | Create address |
| GET | `/customer/addresses/{id}` | Get address |
| PATCH | `/customer/addresses/{id}` | Update address |
| DELETE | `/customer/addresses/{id}` | Delete address |
| PATCH | `/customer/addresses/{id}/default` | Set default address |
| GET | `/customer/notification-preferences` | Get preferences |
| PATCH | `/customer/notification-preferences` | Update preferences |
| GET | `/customer/wishlists` | List wishlists |
| POST | `/customer/wishlists` | Create wishlist |
| GET | `/customer/wishlists/{id}` | Get wishlist |
| PATCH | `/customer/wishlists/{id}` | Update wishlist |
| DELETE | `/customer/wishlists/{id}` | Delete wishlist |
| POST | `/customer/wishlists/{id}/items` | Add item |
| DELETE | `/customer/wishlists/{id}/items/{id}` | Remove item |

#### Identity (`/storefront/identity`) — 13 routes (unchanged)
#### Location (`/storefront/location`) — 6 routes (unchanged)

**Total: 76 endpoints** (was 74)

### 4.2 Inventory Service Interfaces

```csharp
// IStockReservationService — complete interface
public interface IStockReservationService
{
    Task<Result<StockReservation>> ReserveAsync(
        Guid variantId, int quantity, Guid stockLocationId,
        Guid? orderId = null, string? cartToken = null,
        int ttlMinutes = 30, CancellationToken ct = default);

    Task<Result<int>> ReleaseReservationsAsync(
        Guid? orderId = null, string? cartToken = null,
        CancellationToken ct = default);

    Task<Result<int>> ExpireReservationsAsync(CancellationToken ct = default);

    Task<Result<List<(StockReservation Reservation, int RemainingSeconds)>>>
        GetReservationsForCartAsync(string cartToken, CancellationToken ct = default);

    // NEW — added in Phase 1
    Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default);

    // NEW — added in Phase 1
    Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default);
}
```

```csharp
// IStockItemService — complete interface
public interface IStockItemService
{
    Task<Result> AdjustStockAsync(
        Guid variantId, int delta, Guid stockLocationId,
        Guid orderId, CancellationToken ct = default);

    Task<Result<RestockResult>> RestockAsync(
        Guid stockItemId, int quantity, string? reference = null,
        string? reason = null, CancellationToken ct = default);

    Task<Result<bool>> IsAvailableAsync(
        Guid variantId, int quantity, Guid? stockLocationId = null,
        CancellationToken ct = default);

    Task<Result<StockSnapshot>> GetSnapshotForVariantAsync(
        Guid variantId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<VariantStockAvailability>>>
        GetStockAvailabilityAsync(IEnumerable<Guid> variantIds,
        CancellationToken ct = default);

    Task<Result<List<VariantStockSummary>>>
        GetStockSummaryAsync(CancellationToken ct = default);

    // NEW — added in Phase 1
    Task<Result<VariantStockAvailability>> GetAvailabilityForCartAsync(
        Guid variantId, string? cartToken, CancellationToken ct = default);
}
```

### 4.3 CartTokenMiddleware Contract

```
Input:  X-Cart-Token HTTP header (optional, string)
Output: HttpContext.Items["CartToken"] = string | null
Auth:   None (runs before auth middleware)
```

### 4.4 Catalog Stock DTO Changes

```csharp
// StoreVariant.Model.cs — add fields
public record StoreVariantResponse
{
    // existing fields...

    // Stock-embedded fields (Phase 7)
    public bool InStock { get; init; }
    public int AvailableQuantity { get; init; }
    public bool Backorderable { get; init; }
}
```

## 5. Acceptance Criteria

### Service Layer

- **AC-001**: Given an Ordering module handler, when it adds an item to cart, then it calls `IStockReservationService.ReserveAsync()` directly (no MediatR command sent)
- **AC-002**: Given a completed order checkout, when stock is consumed, then `IStockReservationService.ConsumeForOrderAsync()` is called and all reservations for that order transition to `Fulfilled` state
- **AC-003**: Given a cancelled order, when stock is released, then `IStockReservationService.ReleaseReservationsAsync(orderId)` is called and all reservations transition to `Released` state
- **AC-004**: No feature directory exists at `Inventory/Features/Storefront/StockReservations/ReserveCart/`, `ConsumeCart/`, or `ReleaseCart/`

### Route Alignment

- **AC-005**: Given a cart operation, the route prefix is `/api/storefront/cart` (not `/api/storefront/ordering/cart`)
- **AC-006**: Given a partial update to cart checkout (email only), the HTTP method is `PATCH /cart` (not `PUT /cart`)
- **AC-007**: Given a cancel order action, the HTTP method is `POST /orders/{id}/cancel` (not `PUT`)
- **AC-008**: Given a validate checkout request, the HTTP method is `GET /cart/checkout` (not `POST /cart/validate`)
- **AC-009**: No route parameter or query parameter carries a cart token. All handlers read `HttpContext.Items["CartToken"]`.
- **AC-010**: `GET /api/storefront/customer/all` responds with 403 Forbidden for storefront users and 200 with data for authorized admins (moved to admin route)

### Catalog Stock

- **AC-011**: Given a product list query, each product in the response contains `inStock`, `availableQuantity`, and `backorderable` fields
- **AC-012**: Given a product detail query, each variant in the response contains `inStock`, `availableQuantity`, and `backorderable` fields
- **AC-013**: The Store SPA does not call `checkAvailability()` separately — stock data comes from product response

### Cross-Module Integrity

- **AC-014**: `scripts/check-cross-module-refs.sh` reports zero violations between Inventory and Ordering modules. Service interface imports are permitted.
- **AC-015**: `OrderInventoryService.cs` file does not exist
- **AC-016**: No `using Module.Inventory.Domain.StockItems` in Ordering module files (domain entity access replaced by service calls)

## 6. Test Automation Strategy

### Test Levels

- **Unit Tests:** Service layer methods (`ReserveAsync`, `ConsumeForOrderAsync`, `GetAvailabilityForCartAsync`) with mocked `IApplicationDbContext`
- **Integration Tests:** Endpoint-to-service flow with Testcontainers PostgreSQL
- **End-to-End:** Store SPA checkout flow — create cart → add items → checkout → verify order

### Frameworks

- `xUnit` / `MSTest` for .NET tests
- `FluentAssertions` for assertion clarity
- `Testcontainers.PostgreSql` for integration tests
- `Vitest` for Store SPA unit tests

### Test Data Management

- Use `StockReservationMethod.Reserve` domain factory in tests (same factory service calls)
- Test cart tokens use deterministic GUID strings
- Cleanup: rollback transactions in integration tests

### CI/CD Integration

- `dotnet test service/Api/tests/Module.UnitTests` — fast, no Docker
- `dotnet test` — all tests including integration (requires Docker)
- `scripts/check-cross-module-refs.sh` — run as CI gate
- `scripts/check-feature-conventions.sh` — run as CI gate

### Coverage Requirements

- Service layer methods: 90%+ branch coverage
- New endpoint validators: 100% coverage (happy path + each validation rule)
- Existing endpoints (route-only changes): no coverage regression

## 7. Rationale & Context

### Why drop CQRS for inventory storefront?

The current `ReserveCartStock`, `ConsumeCartStockReservations`, and `ReleaseCartStockReservations` CQRS handlers duplicate logic already present in `IStockReservationService`. The service already has:
- `ReserveAsync()` — per-item reserve with `Serializable` isolation (stronger than handler's `RepeatableRead`)
- `ReleaseReservationsAsync()` — bulk release by cart token or order ID
- `GetReservationsForCartAsync()` — list active reservations with TTL remaining

The handlers add no business logic — they wrap Entity Framework queries identical to the service. Pruning them removes 10 files of indirection.

### Why Service DI instead of MediatR for inventory?

MediatR between modules adds unnecessary async dispatch overhead. The pattern `sender.Send(new Command(...))` is useful within a module for CQRS separation but between modules it's just indirection. Service interfaces are the established pattern for inter-module communication in this codebase (`IStockItemService` already used by Catalog). The refactoring extends this pattern to Ordering and Billing.

### Why Spree v3 cart model?

Spree's cart-as-top-level-resource is proven across thousands of commerce stores. Nesting cart under Ordering made sense when the codebase had fewer modules. With Billing and Shipping as separate modules, the cart needed to cross 3 module boundaries. Making cart top-level with sub-resources keeps the checkout workflow coherent while preserving module separation.

### Why Serializable isolation for reservations?

`Serializable` isolation prevents phantom reads in PostgreSQL. Two concurrent `ReserveAsync` calls for the same variant-location pair cannot both read `count - reserved >= quantity` and both insert reservations. `RepeatableRead` (used by the old CQRS handler) allows phantom reads — both can succeed, resulting in oversold inventory. The service already uses `Serializable` — this is correct.

## 8. Dependencies & External Integrations

### External Systems

- **EXT-001**: Stripe Payment Gateway — payment intent creation, confirmation, webhook events. Unchanged by this spec.
- **EXT-002**: Stripe Webhook Signature Validation — `Stripe-Signature` header validated by `IStripeWebhookService`. Unchanged.

### Third-Party Services

- **SVC-001**: SendGrid / SMTP — order confirmation emails. Unchanged.
- **SVC-002**: Fashion-CLIP embedding service (Python sidecar) — image search. Unchanged.

### Infrastructure Dependencies

- **INF-001**: PostgreSQL 17 with pgvector — all domain data including stock_items, stock_reservations, stock_movements. Unchanged.
- **INF-002**: Redis 7 (HybridCache) — session data. Unchanged.
- **INF-003**: Hangfire — reservation expiry background job. Unchanged.

### Data Dependencies

- **DAT-001**: `stock_items` table — variant_id, stock_location_id, count_on_hand, backorderable. Queried by both `IStockItemService` and `IStockReservationService`.
- **DAT-002**: `stock_reservations` table — variant_id, stock_location_id, quantity, state, cart_token, expires_at_utc. State transitions: Reserved → Fulfilled (Consume), Reserved → Released (Release), Reserved → Expired (Expire).

### Technology Platform Dependencies

- **PLT-001**: .NET 10 — `System.Data.IsolationLevel.Serializable` for reservation transactions
- **PLT-002**: ASP.NET Core middleware pipeline — `CartTokenMiddleware` registered before `UseAuthentication()`
- **PLT-003**: Carter minimal APIs — all storefront endpoints use `ICarterModule`

### Compliance Dependencies

- None — no PCI, GDPR, or regulatory requirements apply to the route/service restructuring. Payment card data handled by Stripe (PCI Level 1 compliant).

## 9. Examples & Edge Cases

### Example: Ordering.AddToCart handler after refactoring

```csharp
// Before: MediatR command to Inventory
var reserveResult = await sender.Send(
    new ReserveCartStock.Command(new ReserveCartStock.Request
    {
        CartId = cart.Id,
        LineItems = ...
    }), ct);

// After: Direct service call
var reserveResult = await stockReservationService.ReserveAsync(
    variantId: request.VariantId,
    quantity: request.Quantity,
    stockLocationId: locationId,
    cartToken: httpContextAccessor.HttpContext?.Items["CartToken"]?.ToString(),
    ct: ct);
```

### Example: Inventory storefront endpoint (thin wrapper)

```csharp
// ReserveStockReservation.Endpoint.cs — no Handler.cs needed
app.MapPost(InventoryFeature.Storefront.StockReservations.Reserve.Route, async (
    [FromBody] ReserveStockReservation.Request request,
    IStockReservationService reservationService,
    IHttpContextAccessor httpContext,
    CancellationToken ct) =>
{
    var cartToken = httpContext.HttpContext?.Items["CartToken"]?.ToString();
    var result = await reservationService.ReserveAsync(
        request.VariantId, request.Quantity, request.StockLocationId,
        cartToken: cartToken, ttlMinutes: request.TtlMinutes, ct: ct);
    return result.ToResult();
});
```

### Edge Case: Concurrent add-to-cart with Serializable isolation

```
Thread A: SELECT COUNT(*) FROM stock_reservations WHERE variant=X → 3 reserved
Thread A: stock_item.count_on_hand(10) - reserved(3) = 7 >= quantity(5) → OK
Thread B: SELECT COUNT(*) FROM stock_reservations WHERE variant=X → 3 reserved (blocked by Serializable)
Thread A: INSERT reservation → COMMIT
Thread B: RESUMES: SELECT COUNT(*) → 4 reserved (A's insert now visible under Serializable)
Thread B: 10 - 4 = 6 >= 5 → OK (correct — did not oversell)
```

With `RepeatableRead` (old handler): B would have read 3 (phantom), both succeed → oversold.

### Edge Case: Cart with no cart token (authenticated user)

```
Request: GET /storefront/cart
Headers: Authorization: Bearer <jwt>
         X-Cart-Token: (absent)
Behavior: CartTokenMiddleware sets Items["CartToken"] = null.
Handler: HttpContext.Items["CartToken"] is null, user is authenticated.
Handler looks up cart by currentUser.UserId. Returns cart.
```

### Edge Case: Guest cart association

```
Request: POST /storefront/cart/associate
Headers: X-Cart-Token: guest_token_abc
Body: { guestOrderId: "..." }
Behavior: AssociateCart handler reads cart from guest token.
If user authenticated, merges guest cart into user's cart.
```

## 10. Validation Criteria

- **VAL-001**: `dotnet build` passes with zero warnings (TreatWarningsAsErrors)
- **VAL-002**: `dotnet test service/Api/tests/Module.UnitTests` — all pass
- **VAL-003**: `dotnet test service/Api/tests/Shared.UnitTests` — all pass
- **VAL-004**: `scripts/check-cross-module-refs.sh` returns zero violations
- **VAL-005**: `scripts/check-feature-conventions.sh` passes for all storefront features
- **VAL-006**: `cd app/Store && pnpm run lint && pnpm run test:unit` — all pass
- **VAL-007**: No route constant references old `api/storefront/ordering/cart` prefix
- **VAL-008**: No route constant references old `api/storefront/billing/payments/create-intent` prefix
- **VAL-009**: `OrderInventoryService.cs` does not exist on disk
- **VAL-010**: Store SPA builds successfully with `pnpm run build`

## 11. Related Specifications / Further Reading

- [spec-inventory-services-consolidation.md](spec-inventory-services-consolidation.md) — Phase 1: Prune CQRS, extend services
- [spec-cart-consolidation.md](spec-cart-consolidation.md) — Phase 4: Cart as top-level resource
- [spec-rest-method-alignment.md](spec-rest-method-alignment.md) — Phase 5/6: HTTP methods + customer account
- [spec-catalog-stock-embedding.md](spec-catalog-stock-embedding.md) — Phase 7: Embed stock in product responses
- [spec-storefront-spa-migration.md](spec-storefront-spa-migration.md) — Phase 9: SPA migration
- [Spree Commerce Store API v3](https://spreecommerce.org/docs/api-reference/store-api/)
- [ReSys.Shop Architecture](docs/codebase/ARCHITECTURE.md)
- [ReSys.Shop Conventions](docs/codebase/CONVENTIONS.md)
