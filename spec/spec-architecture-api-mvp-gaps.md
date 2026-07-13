---
title: API Service — MVP Readiness Gaps
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
tags: architecture, api, mvp, gap-closure, service/Api
---

# Introduction

The `service/Api` backend is structurally complete — 8 business modules, CQRS pipeline, JWT auth, cart-to-checkout flow, and seeded demo data. However, six gaps prevent it from being correct under concurrent use, secure in its admin surface, truthful in its sidecar responses, and clean in its configuration. This specification enumerates those gaps and defines the acceptance criteria for closing each one. The scope is limited to `service/Api/src/` and `app/Admin/src/app/auth/` (admin auth guard, which gates the API's admin endpoints).

## 1. Purpose & Scope

**Purpose**: Define the minimum set of corrections required for the API service to be demo-MVP-ready — correct under concurrent cart operations, secure at the admin boundary, honest in embedding responses, and free of dead configuration.

**Scope**: C# code in `service/Api/src/Module/`, `service/Api/src/Shared/`, `app/Admin/src/app/auth/`, `service/Api/src/Api/appsettings.json`, `service/Embedding/src/routers/embedding_router.py`. Excludes frontend feature views, CI/CD, and the embedding ML model swap (those are follow-on specs).

**Audience**: backend developers and coding agents implementing the fixes.

**Assumptions**:
- The API compiles and runs (`dotnet build` passes).
- Seeded data (7 users, 5 products, 3 orders) is accurate.
- The Bogus payment gateway is in use for demo (Stripe is not wired).

## 2. Definitions

| Term | Definition |
|------|-----------|
| **Cart reservation** | A `StockReservation` record with a TTL (30 min) that temporarily locks inventory while a cart is active. |
| **Optimistic concurrency** | Detecting conflicting writes at commit time via a row-version column (`xmin` / `[Timestamp]`), rather than relying solely on transaction isolation levels. |
| **Admin auth guard** | A Vue Router `beforeEach` hook that redirects unauthenticated requests to `/login`. |
| **No-op stub** | A function that compiles but performs no operation — `void app` or `_generate_dummy_vector(512)`. |

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: `AddToCart` handler must dispatch `ReserveCartStock.Command` after validating stock availability.
- **REQ-002**: `StockItem` entity must carry a concurrency token. The checkout handler must catch `DbUpdateConcurrencyException` and return a retryable error.
- **REQ-003**: Admin SPA must redirect unauthenticated users to `/login`. No admin route may render without a valid `accessToken`.
- **REQ-004**: Stripe webhook handler must acknowledge the HTTP request immediately (200) and defer processing to a Hangfire job.
- **REQ-005**: Embedding service must not return `"model_version": "v1.0-stub"` in production responses. Stub label must be gated on an environment variable or removed.
- **REQ-006**: Empty directories (`Payment/Infrastructure/Gateways/Stripe/`, `Payment/Infrastructure/Gateways/Bogus/`, `Shared/Operational/Webhooks/*/`) must be removed or populated with a README explaining intent.
- **REQ-007**: `AzureStorageProvider` config block in `appsettings.json` must be removed OR an `AzureStorageProvider` class must be implemented and registered in `Storage.Extensions.cs`.
- **CON-001**: All changes must keep `TreatWarningsAsErrors=true` — `dotnet build` must pass cleanly.
- **CON-002**: No new direct cross-module references. Communication stays via `ISender`.
- **CON-003**: All new domain operations return `Result` / `Result<T>`, not exceptions.
- **GUD-001**: Prefer the existing vertical-slice file layout (Handler + Request + Response + Endpoint + Validator).
- **GUD-002**: New concurrency error codes follow the `{Aggregate}.{Action}.ConcurrencyConflict` pattern.

## 4. Interfaces & Data Contracts

### 4.1 Stock Concurrency Token

`StockItem` gains a concurrency column:

```
Database: stock_item.row_version (type: xid / bigint, auto-managed by EF)
C#:       public uint RowVersion { get; set; }   // [Timestamp] attribute
Config:   builder.Property(s => s.RowVersion).IsRowVersion();
```

### 4.2 Cart Reservation Command

Already exists as `Module.Inventory.Features.Storefront.CartReservations.Reserve.ReserveCartStock.Command`. `AddToCart` will dispatch it:

```
var reserveCommand = new ReserveCartStock.Command(
    variantId, quantity, cartToken, stockLocationId);
var reserveResult = await sender.Send(reserveCommand, ct);
if (reserveResult.IsFailure) return reserveResult.Errors;
```

### 4.3 Retryable Concurrency Error

New factory on `StockResult.Errors`:

```
public static Error ConcurrencyConflict(Guid variantId)
    => Error.Conflict(
        code: "Stock.ConcurrencyConflict",
        message: $"Stock for variant {variantId} changed during checkout. Retry.");
```

### 4.4 Admin Auth Guard Contract

```
// router.beforeEach in installAuthBootstrap
if (!to.meta.public && !localStorage.getItem('accessToken')) {
    return { name: 'Login', query: { redirect: to.fullPath } };
}
```

Routes that are public (login, register) set `meta: { public: true }`.

### 4.5 Embedding Stub Gate

Add to `embedding_router.py`:

```
import os
VERSION = os.getenv("EMBEDDING_MODEL_VERSION", "v1.0")
# use VERSION in model_version field
```

`model_version` defaults to `"v1.0"` unless `EMBEDDING_MODEL_VERSION` is explicitly set.

## 5. Acceptance Criteria

- **AC-001**: Given a product with 1 unit stock and two concurrent guests adding it to cart, When both send `AddToCart` within the reservation TTL, Then exactly one succeeds and the other receives `Stock.Unavailable`.
- **AC-002**: Given two concurrent checkouts consuming the last unit of the same variant, When both submit within the same second, Then one succeeds and the other receives `Stock.ConcurrencyConflict` (not a 500).
- **AC-003**: Given a browser with no `accessToken` in localStorage, When navigating to `/admin/catalog/products`, Then the user is redirected to `/login?redirect=/admin/catalog/products`.
- **AC-004**: Given a valid `accessToken`, When navigating to any admin route, Then the page renders without redirect.
- **AC-005**: Given a Stripe webhook POST with valid signature, When the handler receives it, Then the HTTP response is 200 within 2 seconds, and the event processing is enqueued as a Hangfire job.
- **AC-006**: Given the embedding service running without `EMBEDDING_MODEL_VERSION` set, When `POST /embeddings` is called, Then `model_version` in the response is `"v1.0"` (not `"v1.0-stub"`).
- **AC-007**: `Payment/Infrastructure/Gateways/Stripe/` and `Payment/Infrastructure/Gateways/Bogus/` directories must not exist on disk after fix.
- **AC-008**: `appsettings.json` must not contain `Storage.Providers.Azure` block, OR an `AzureStorageProvider` class must exist and `Storage.Extensions.cs` must register it.
- **AC-009**: `dotnet build` passes with zero warnings (TreatWarningsAsErrors).
- **AC-010**: Existing integration tests in `Api.Tests/Scenarios/Ordering/` and `Api.Tests/Scenarios/Catalog/` pass unchanged.

## 6. Test Automation Strategy

- **Test Levels**: Unit (xUnit v3 + Moq + InMemory EF), Integration (Testcontainers.PostgreSql + WebApplicationFactory)
- **Frameworks**: xUnit v3, FluentAssertions, Moq, Respawn
- **Test Data Management**: InMemory DB seeded per test class (existing pattern). Integration tests use Respawn checkpoint.
- **CI/CD Integration**: GitHub Actions `.github/workflows/ci.yml` runs `dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Api.Tests` on push.
- **Coverage Requirements**: New handlers must have unit tests for both success and failure paths. Concurrency tests require integration-test level (real PostgreSQL).
- **Performance Testing**: Not in scope for this spec.

### New Tests Required

| Test | Level | Covers |
|------|-------|--------|
| `AddToCart_ShouldReserveStock_WhenAvailable` | Unit | REQ-001 |
| `AddToCart_ShouldFail_WhenReservationFails` | Unit | REQ-001 edge |
| `Checkout_ShouldReturnConcurrencyConflict_OnRowVersionMismatch` | Integration | REQ-002 |
| `AdminRouter_ShouldRedirect_WhenNoToken` | Unit (Vitest) | REQ-003 |
| `AdminRouter_ShouldRender_WhenTokenPresent` | Unit (Vitest) | REQ-003 edge |
| `StripeWebhook_ShouldReturn200_BeforeProcessing` | Unit | REQ-004 |
| `AzureConfig_ShouldNotExist_Or_ShouldBeImplemented` | — | REQ-007 (manual verification) |

## 7. Rationale & Context

**Why REQ-001 (cart reservation) is the highest priority correct-bug fix**: The checkout pipeline is the only place money changes hands. Without a reservation between add and checkout, two users can add the last unit of a product to their carts, both see success, and one will get a mysterious failure at checkout. The demo seed data has only 5 products — in a live demo, hitting this is unlikely but possible if two demo users browse simultaneously.

**Why REQ-002 (concurrency token)**: `IsolationLevel.Serializable` works for correctness but produces 500 errors instead of retryable errors. A `[Timestamp]` column lets the handler return a clean `Conflict` result instead of a crash. For an MVP demo this is low-probability; for any scale beyond demo it is mandatory.

**Why REQ-003 (admin auth) is demo-critical**: The admin SPA currently loads all pages without authentication. A demo audience navigating to `/admin` without logging in will see data they shouldn't. This is the most visible gap.

**Why REQ-005 (stub label) matters**: The embedding response contains `"model_version": "v1.0-stub"`. Any API consumer or demo script inspecting the response body will immediately know the ML is fake. The stub behavior is acceptable for demo; the stub label is not.

**Why REQ-006/007 (dead config/dirs)**: Dead code and ghost directories confuse agents and new contributors. Every empty directory or dead config block is a question mark that costs time.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe — webhook signature validation uses `Stripe.net` 52.1.0. No change to integration.
- **EXT-002**: PostgreSQL 17+pgvector — concurrency token requires a real PG instance for integration tests. InMemory EF is insufficient for REQ-002 tests.

### Third-Party Services
- **SVC-001**: Embedding sidecar (Python FastAPI) — REST call from `InferenceClient`. Only the response `model_version` field changes; no protocol change.

### Infrastructure Dependencies
- **INF-001**: Hangfire — required for REQ-004 (deferred webhook processing). Already configured; no new setup needed.
- **INF-002**: Redis — optional for Hangfire distributed storage. In-memory is sufficient for single-instance demo.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 / EF Core 10 — `IsRowVersion()` on `StockItem` requires EF Core provider support (Npgsql supports `xid`).

## 9. Examples & Edge Cases

### REQ-001: Reservation in AddToCart

```csharp
// AddToCart.cs — after AvailabilityValidator passes
var reserveCommand = new ReserveCartStock.Command(
    command.VariantId,
    command.Quantity,
    cart.Token,        // guest session ID or user ID
    stockLocationId,
    TimeSpan.FromMinutes(30));

var reserveResult = await sender.Send(reserveCommand, ct);
if (reserveResult.IsFailure)
    return reserveResult.Errors;  // e.g. InsufficientStock

// Success — proceed with cart update
```

### REQ-002: Concurrency in Checkout

```csharp
// CreateOrderFromCart.cs — catch block around SaveChangesAsync
try
{
    await dbContext.SaveChangesAsync(ct);
}
catch (DbUpdateConcurrencyException)
{
    return StockResult.Errors.ConcurrencyConflict(variantId);
}
```

### REQ-003: Admin Auth Guard Edge Cases

```
Edge: Token present but expired → 401 from API → axios interceptor tries
refresh → if refresh fails, clear tokens and redirect to /login.
Edge: Token present, navigating to /login → skip guard (already public).
Edge: Deep link to /admin/orders/123?token=expired → guard fires before
component mount.
```

### REQ-004: Stripe Webhook Async

```
Stripe Webhook POST → Validate signature → Return 200 immediately →
Enqueue Hangfire job: ProcessStripeEventJob(eventPayload, eventType).
Job signature: idempotent (check Stripe event ID before processing).
```

### REQ-005: Embedding Stub Gate

```python
VERSION = os.getenv("EMBEDDING_MODEL_VERSION", "v1.0")
# If EMBEDDING_MODEL_VERSION is set, use it.
# Default is "v1.0" (clean). No "stub" string anywhere in the codebase.
```

## 10. Validation Criteria

- **VAL-001**: `dotnet build` exits 0 with zero warnings.
- **VAL-002**: `dotnet test service/Api/tests/Module.UnitTests` passes — all existing + new unit tests.
- **VAL-003**: `dotnet test service/Api/tests/Api.Tests` passes — concurrency integration test included.
- **VAL-004**: `cd app/Admin && pnpm run lint && pnpm run test:unit` passes — auth guard test included.
- **VAL-005**: Manual smoke: `admin:Admin@123!` login → navigate to `/admin/catalog/products` → page renders. Logout → navigate to same URL → redirect to `/login`.
- **VAL-006**: `curl -X POST http://localhost:8000/embeddings -H "Content-Type: application/json" -d '{"model":"efficientnet_b0","inputs":[]}'` → response contains `"model_version":"v1.0"`.
- **VAL-007**: `find service/Api/src/Module/Payment/Infrastructure/Gateways -type d -empty` returns nothing.
- **VAL-008**: `grep -c "Azure" service/Api/src/Api/appsettings.json` returns 0 OR `AzureStorageProvider` class exists.

## 11. Related Specifications / Further Reading

- [ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md) — layer map, data flow, CQRS pipeline
- [CONCERNS.md](../docs/codebase/CONCERNS.md) — full tech debt inventory
- [TESTING.md](../docs/codebase/TESTING.md) — test commands, mock strategies
- [.harness/principles.yml](../.harness/principles.yml) — 8 golden principles
- [.harness/enforcement.yml](../.harness/enforcement.yml) — file limits, naming, logging rules
- [.harness/domains.yml](../.harness/domains.yml) — 14 domain definitions with LOC
- [.harness/quality.yml](../.harness/quality.yml) — per-domain quality scores
