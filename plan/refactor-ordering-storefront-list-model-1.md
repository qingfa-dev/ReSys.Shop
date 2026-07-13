---
goal: ListCustomerOrders must inherit from shared model base — create StorefrontOrderListItemResponse
version: 1.1
date_created: 2026-07-13
last_updated: 2026-07-13
status: 'Planned'
tags: [refactor, ordering, models, unification, response-standardization]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create `StorefrontOrderListItemResponse` in `Storefront/Orders/Shared/Models/` — a shared base for the storefront order list response. Change `ListCustomerOrders.Response` to inherit from it. Status changes from `string` to `OrderStatus` (consistent with `GetCustomerOrder.Response : OrderDetailResponse` which already uses `OrderStatus`).

**Before:** `ListCustomerOrders.Response` is a standalone type with `string Status`
**After:** `ListCustomerOrders.Response : StorefrontOrderListItemResponse` with `OrderStatus Status`

## 1. Requirements & Constraints

- **REQ-001**: ALL feature-level Request/Response types must inherit from a shared base in a `*/Shared/` directory — zero standalone response types
- **REQ-002**: `StorefrontOrderListItemResponse` must be a top-level record in `Storefront/Orders/Shared/Models/`
- **REQ-003**: `Status` property uses `OrderStatus` enum (not string) — consistent with the shared model hierarchy (used by `OrderListItemResponse`, `OrderDetailResponse`)
- **REQ-004**: Build: 0 warnings, 0 errors
- **REQ-005**: All 2404 Module.UnitTests pass
- **CON-001**: Must not break the storefront detail order endpoint (`GetCustomerOrder`) which already uses `OrderStatus Status`
- **PAT-001**: Follow same pattern as `Storefront/Cart/Shared/Models/`

## 2. Implementation Steps

### Implementation Phase 1: Create StorefrontOrderListItemResponse + update feature

- GOAL-001: Create storefront order list shared model. Update `ListCustomerOrders.Response` to inherit from it. Update handler to use `OrderStatus Status`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Storefront/Orders/Shared/Models/Order.Model.Response.cs` — top-level `public record StorefrontOrderListItemResponse` with 5 fields: `Guid Id`, `string Number`, `OrderStatus Status`, `decimal Total`, `DateTimeOffset CreatedAtUtc`. Use `using Module.Ordering.Domain.Orders;` for OrderStatus. Default Number/Status with `= string.Empty`. | | |
| TASK-002 | Change `ListCustomerOrders.Response` in `Storefront/Orders/ListOrders/ListCustomerOrders.Response.cs`: replace standalone type with `public sealed record Response : StorefrontOrderListItemResponse`. Add `using Module.Ordering.Features.Storefront.Orders.Shared.Models;`. Remove doc comment about "intentionally standalone". | | |
| TASK-003 | Update `ListCustomerOrders.cs` handler: change `.Select(o => new Response { Status = o.Status.ToString(), ... })` to use `Status = o.Status` (direct enum assignment, no `.ToString()`). The LINQ projection handles `OrderStatus` directly — EF Core translates enum to DB comparison. | | |
| TASK-004 | Build `dotnet build service/Api/src/Module/ --no-restore` — fix any compile errors. | | |

### Implementation Phase 2: Final verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Full build: `dotnet build` — 0W/0E. | | |
| TASK-006 | Unit tests: `dotnet test service/Api/tests/Module.UnitTests` — all pass. | | |
| TASK-007 | Verify zero standalone Response types: `rg "sealed record Response\b" -g "*.cs" service/Api/src/Module/Ordering/Features/ | grep -v " : "` — must show zero results. | | |

## 3. Alternatives

- **ALT-001**: Inherit from admin `OrderListItemResponse` — rejected, would leak PaymentTotal, PaymentState, ShipmentState, addresses, email to storefront API.
- **ALT-002**: Keep `string Status` in the shared model — rejected, inconsistent with `GetCustomerOrder.Response : OrderDetailResponse` which uses `OrderStatus` and is already consumed by storefront frontend.
- **ALT-003**: Have `StorefrontOrderListItemResponse` inherit from `OrderParameters` — rejected, would leak Currency, Email, SpecialInstructions, address IDs to storefront.

## 4. Dependencies

- **DEP-001**: `OrderStatus` enum already used by `OrderDetailResponse` and `OrderListItemResponse` — no new dependency.
- **DEP-002**: Namespace `Module.Ordering.Features.Storefront.Orders.Shared.Models` — new namespace, no conflicts.

## 5. Files

- **FILE-001**: `Storefront/Orders/Shared/Models/Order.Model.Response.cs` — create
- **FILE-002**: `Storefront/Orders/ListOrders/ListCustomerOrders.Response.cs` — change to inherit
- **FILE-003**: `Storefront/Orders/ListOrders/ListCustomerOrders.cs` — fix Status assignment

## 6. Testing

- **TEST-001**: `dotnet build` — 0W/0E
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all pass

## 7. Risks & Assumptions

- **RISK-001**: `OrderStatus` serialization changes from string to integer in JSON. Mitigation: `GetCustomerOrder.Response : OrderDetailResponse` already uses `OrderStatus` — if the frontend works with the detail endpoint, it can handle integer statuses for the list endpoint too.
- **ASSUMPTION-001**: The storefront frontend maps order status as an integer (or via a lookup table) — consistent with how the detail endpoint already works.

## 8. Related Specifications / Further Reading

- [Response unification plan](./refactor-ordering-response-unification-1.md) — original plan (now superseded on this point)
- [Model consolidation plan](./refactor-ordering-shared-models-standardization-1.md)
- [AGENTS.md](../AGENTS.md) — modular monolith rules
