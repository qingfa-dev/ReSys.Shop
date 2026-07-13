---
goal: Standardize Ordering Feature Request/Response Models and HTTP Status Codes
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
status: Planned
tags: refactor, ordering, endpoints, http, response-models, service/Api
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

An audit of 33 Ordering feature endpoints revealed systematic gaps versus the Catalog module's canonical pattern: 13 endpoints missing `[FromRoute]` attributes, 13 endpoints missing error `Produces<T>()` declarations (400/404/401), 1 endpoint missing `[FromBody]`, and 9 Request types still using `class` instead of `record`. Additionally, several feature Response types are defined inline in handler files rather than in dedicated `Response.cs` files, and the `UpdateOrderStatus.Response.cs` exists but is unused (handler returns `Result` not `Result<Response>`).

This plan standardizes all request/response models, HTTP status code declarations, and endpoint patterns to match the Catalog module's conventions.

## 1. Requirements & Constraints

### Route Parameter Requirements

- **REQ-EP01**: Add `[FromRoute]` attribute to ALL `Guid` route parameters in EVERY endpoint. 13 endpoints currently omit it.
- **REQ-EP02**: Add `[FromBody]` attribute to the request parameter in `CreateOrderFromCart.Endpoint.cs`.

### Error Produces Requirements

- **REQ-EP03**: Add `Produces<Result>(StatusCodes.Status400BadRequest)` to all endpoints that return validation or domain errors: GetCart, CreateCart, EmptyCart, DeleteCart, ValidateCheckout.
- **REQ-EP04**: Add `Produces<Result>(StatusCodes.Status404NotFound)` to all endpoints that query or mutate a specific Order/LineItem: GetCart, CreateCart, RemoveCartItem, UpdateCartItemQuantity, AssociateCartWithUser, CreateOrderFromCart, UpdateCheckout, SelectShippingRate.
- **REQ-EP05**: Add `Produces<Result>(StatusCodes.Status400BadRequest)` to `ApproveOrder.Endpoint.cs` (handler returns domain state transition errors).
- **REQ-EP06**: Add `Produces<Result>(StatusCodes.Status400BadRequest)` and `Produces<Result>(StatusCodes.Status401Unauthorized)` to `ListCustomerOrders.Endpoint.cs` (matches Admin `GetPagedOrders` pattern).

### Model Requirements

- **REQ-MD01**: Convert all `class` Request types to `record`: `UpdateOrderStatus.Request`, `UpdateOrderLineItem.Request`, `UpdateOrderShipAddress.Request`, `UpdateOrderBillAddress.Request`, `UpdateOrderShippingMethod.Request`, `AddOrderLineItem.Request`, `CancelOrderAdmin.Request`, `UpdateCartItemQuantity.Request`, `UpdateCheckout.Request`, `SelectShippingRate.Request`, `CreateOrderFromCart.Request`.
- **REQ-MD02**: Extract inline `Response` classes from handler files into dedicated `Response.cs` files:
  - `ApproveOrder.Response` (inline in `ApproveOrder.cs`)
  - `ResumeOrder.Response` (inline in `ResumeOrder.cs`)
  - `GetOrderLineItemById.Response` (inline in `GetOrderLineItemById.cs`)
  - `AssociateCartWithUser.Response` (inline in `AssociateCartWithUser.cs`)
- **REQ-MD03**: Convert extracted Response types to `sealed record` matching the module convention.
- **REQ-MD04**: Convert all standalone `class` Response types to `record` in existing `Response.cs` files: `GetOrderLineItems.Response`, `AddOrderLineItem.Response`, `UpdateOrderLineItem.Response`, `AddToCart.Response`, `ListCustomerOrders.Response`.
- **REQ-MD05**: Fix `UpdateOrderStatus.Response.cs` — the handler returns `Result` (no body). Either change the handler to `ICommand<Response>` and use `Response.cs`, or delete `Response.cs` if the handler intentionally returns no body. **Decision: keep `Result` (no body) and delete `Response.cs`** — status updates don't return data in the Catalog pattern.

### Unit Test Requirements

- **REQ-T01**: Update `Order.Method.Tests.cs` after model changes to fix any compile errors from record immutability or `init` accessor visibility.
- **REQ-T02**: Verify all integration tests in `service/Api/tests/Api.Tests/` compile and pass after model changes (specifically serialization tests for record types).

### Constraints

- **CON-001**: `dotnet build` must pass with `TreatWarningsAsErrors=true`.
- **CON-002**: No behavioral change — all endpoint logic remains identical.
- **CON-003**: All new types use `record` (not `class`).
- **CON-004**: All `[FromRoute]` additions are declarative only — no routing behavior change.

### Guidelines

- **GUD-001**: Match the Catalog module pattern exactly for all aspects not explicitly diverged.
- **GUD-002**: When adding `Produces<T>(status)`, use `StatusCodes.Status400BadRequest` (not bare `400` or `StatusCodes.Status400BadRequest`) — follow the existing convention in the file.
- **GUD-003**: All `Produces<T>()` declarations must appear on separate lines, stacked after the route handler chain call.

## 2. Implementation Steps

### Implementation Phase 1: Add `[FromRoute]` and `[FromBody]` to all 14 endpoints

**GOAL-PH1**: Eliminate all missing `[FromRoute]` and `[FromBody]` attributes. 13 endpoint files modified.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH1-001 | Add `[FromRoute]` to `GetOrderLineItems.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-002 | Add `[FromRoute]` to `GetOrderLineItemById.Endpoint.cs`: `[FromRoute] Guid id, [FromRoute] Guid lineItemId` | | |
| TASK-PH1-003 | Add `[FromRoute]` to `AddOrderLineItem.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-004 | Add `[FromRoute]` to `UpdateOrderLineItem.Endpoint.cs`: `[FromRoute] Guid id, [FromRoute] Guid lineItemId` | | |
| TASK-PH1-005 | Add `[FromRoute]` to `RemoveOrderLineItem.Endpoint.cs`: `[FromRoute] Guid id, [FromRoute] Guid lineItemId` | | |
| TASK-PH1-006 | Add `[FromRoute]` to `ApproveOrder.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-007 | Add `[FromRoute]` to `ResumeOrder.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-008 | Add `[FromRoute]` to `DeleteOrder.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-009 | Add `[FromRoute]` to `UpdateOrderShipAddress.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-010 | Add `[FromRoute]` to `UpdateOrderBillAddress.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-011 | Add `[FromRoute]` to `UpdateOrderShippingMethod.Endpoint.cs`: `[FromRoute] Guid id` | | |
| TASK-PH1-012 | Add `[FromRoute]` to `RemoveCartItem.Endpoint.cs`: `[FromRoute] Guid lineItemId` | | |
| TASK-PH1-013 | Add `[FromRoute]` to `UpdateCartItemQuantity.Endpoint.cs`: `[FromRoute] Guid lineItemId` | | |
| TASK-PH1-014 | Add `[FromBody]` to `CreateOrderFromCart.Endpoint.cs`: `[FromBody] Request request` | | |
| TASK-PH1-015 | Build and commit | | |

### Implementation Phase 2: Add missing error `Produces<T>()` to all 13 endpoints

**GOAL-PH2**: Every endpoint that can return errors declares them. 13 endpoint files modified.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH2-001 | `GetCart.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)`, `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-002 | `CreateCart.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)` | | |
| TASK-PH2-003 | `RemoveCartItem.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-004 | `UpdateCartItemQuantity.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-005 | `EmptyCart.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)` | | |
| TASK-PH2-006 | `DeleteCart.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)` | | |
| TASK-PH2-007 | `AssociateCartWithUser.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-008 | `CreateOrderFromCart.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-009 | `UpdateCheckout.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-010 | `ValidateCheckout.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)` | | |
| TASK-PH2-011 | `SelectShippingRate.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status404NotFound)` | | |
| TASK-PH2-012 | `ListCustomerOrders.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)`, `Produces<Result>(StatusCodes.Status401Unauthorized)` | | |
| TASK-PH2-013 | `ApproveOrder.Endpoint.cs`: add `Produces<Result>(StatusCodes.Status400BadRequest)` | | |
| TASK-PH2-014 | Build and commit | | |

### Implementation Phase 3: Convert Request types from `class` to `record`

**GOAL-PH3**: All 11 Request files use `record` (not `class`). 11 files modified.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH3-001 | `UpdateOrderStatus.Request.cs`: `class` → `sealed record` | | |
| TASK-PH3-002 | `UpdateOrderLineItem.Request.cs`: `class` → `sealed record` | | |
| TASK-PH3-003 | `UpdateOrderShipAddress.Request.cs`: `class` → `record` | | |
| TASK-PH3-004 | `UpdateOrderBillAddress.Request.cs`: `class` → `record` | | |
| TASK-PH3-005 | `UpdateOrderShippingMethod.Request.cs`: `class` → `record` | | |
| TASK-PH3-006 | `AddOrderLineItem.Request.cs`: `class` → `record` | | |
| TASK-PH3-007 | `CancelOrderAdmin.Request.cs`: `class` → `record` | | |
| TASK-PH3-008 | `UpdateCartItemQuantity.Request.cs`: `class` → `sealed record` | | |
| TASK-PH3-009 | `UpdateCheckout.Request.cs`: `class` → `record` | | |
| TASK-PH3-010 | `SelectShippingRate.Request.cs`: `class` → `record` | | |
| TASK-PH3-011 | `CreateOrderFromCart.Request.cs`: already `sealed record` — verify and skip | | |
| TASK-PH3-012 | Build and commit | | |

### Implementation Phase 4: Extract inline Response types + convert remaining `class` Responses

**GOAL-PH4**: All inline Response classes extracted to dedicated files. All remaining `class` Response types converted to `record`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH4-001 | Create `ApproveOrder.Response.cs`: extract inline `class Response` from `ApproveOrder.cs` → `sealed record Response` | | |
| TASK-PH4-002 | Create `ResumeOrder.Response.cs`: extract inline `class Response` from `ResumeOrder.cs` → `sealed record Response` | | |
| TASK-PH4-003 | Create `GetOrderLineItemById.Response.cs`: extract inline `class Response` from `GetOrderLineItemById.cs` → `sealed record Response` | | |
| TASK-PH4-004 | Create `AssociateCartWithUser.Response.cs`: extract inline `class Response` from `AssociateCartWithUser.cs` → `sealed record Response` | | |
| TASK-PH4-005 | Remove inline Response definitions from the 4 source handler files. Update handler files to reference external Response type. | | |
| TASK-PH4-006 | `GetOrderLineItems.Response.cs`: convert standalone `class Response` to `sealed record Response` | | |
| TASK-PH4-007 | `AddOrderLineItem.Response.cs`: convert `class Response` to `record Response` | | |
| TASK-PH4-008 | `UpdateOrderLineItem.Response.cs`: convert `class Response` to `sealed record Response` | | |
| TASK-PH4-009 | `AddToCart.Response.cs`: convert `class Response` to `sealed record Response` | | |
| TASK-PH4-010 | `ListCustomerOrders.Response.cs`: convert `class Response` to `sealed record Response` | | |
| TASK-PH4-011 | Delete `UpdateOrderStatus.Response.cs` (handler returns `Result` with no body; Response.cs is dead code) | | |
| TASK-PH4-012 | Build and commit | | |

### Implementation Phase 5: Update unit tests for model changes

**GOAL-PH5**: All unit tests compile and pass after model changes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH5-001 | Fix any compile errors in `Order.Method.Tests.cs` from record `init` accessor changes | | |
| TASK-PH5-002 | Fix any compile errors in `OrderCheckoutTests.cs` from model changes | | |
| TASK-PH5-003 | Fix any compile errors in `OrderDiscontinuedTests.cs` from model changes | | |
| TASK-PH5-004 | Run `dotnet test service/Api/tests/Module.UnitTests --no-restore` — verify all pass | | |
| TASK-PH5-005 | Commit | | |

### Implementation Phase 6: Full build + verification

**GOAL-PH6**: Complete build validation and endpoint pattern verification.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH6-001 | `dotnet build` — 0 warnings, 0 errors | | |
| TASK-PH6-002 | `dotnet test service/Api/tests/Module.UnitTests --no-restore` — all pass | | |
| TASK-PH6-003 | Verify no endpoints lack `[FromRoute]`: `rg "Map(Get|Post|Put|Delete).*Guid (id|lineItemId)\)" service/Api/src/Module/Ordering/Features/` — if these matches lack `[FromRoute]`, fix them | | |
| TASK-PH6-004 | Verify all `class` Request types are gone: `rg "public (sealed )?class Request" service/Api/src/Module/Ordering/Features/` — zero results | | |
| TASK-PH6-005 | Verify all `class` Response types are gone: `rg "public (sealed )?class Response\b" service/Api/src/Module/Ordering/Features/` — zero results | | |
| TASK-PH6-006 | Verify no inline Response in handler: `rg "public class Response|public record Response" service/Api/src/Module/Ordering/Features/*/`.cs files — Response only in `Response.cs` files (not handlers) | | |
| TASK-PH6-007 | `UpdateOrderStatus.Response.cs` does not exist | | |

## 3. Alternatives

- **ALT-001**: Change `UpdateOrderStatus` handler to `ICommand<Response>` and use `Response.cs`. Rejected: the Catalog pattern for status updates (e.g., `ActivateProduct`) returns no body — this aligns with the "command without response" convention.
- **ALT-002**: Use `[FromRoute]` only on endpoints with `Guid` parameters that are NOT in the route template. Rejected: the Catalog module uses `[FromRoute]` unconditionally on all route parameters for explicitness, even when ASP.NET infers it automatically.
- **ALT-003**: Declare `Produces<Result<Response>>(StatusCodes.Status201Created)` for POST endpoints that create resources. Rejected: the Catalog module doesn't use 201 — `ToResult()` maps StatusCode dynamically but all endpoints declare default 200 `Produces<T>()`.

## 4. Dependencies

- **DEP-001**: Phase 1-6 of the prior spec (`refactor-ordering-features-catalog-patterns-1.md`) must be complete — models must already use `record` where applicable.
- **DEP-002**: `Order.Validation.cs` must have `ApplyEmailRules`, `ApplyCurrencyRules`, etc. added in Phase 1 of the prior spec.
- **DEP-003**: No new NuGet packages or external libraries required.

## 5. Files

### Files Modified (by phase)

- **Phase 1** (13 files): GetOrderLineItems, GetOrderLineItemById, AddOrderLineItem, UpdateOrderLineItem, RemoveOrderLineItem, ApproveOrder, ResumeOrder, DeleteOrder, UpdateOrderShipAddress, UpdateOrderBillAddress, UpdateOrderShippingMethod, RemoveCartItem, UpdateCartItemQuantity + CreateOrderFromCart (all `Endpoint.cs` files)
- **Phase 2** (13 files): GetCart, CreateCart, RemoveCartItem, UpdateCartItemQuantity, EmptyCart, DeleteCart, AssociateCartWithUser, CreateOrderFromCart, UpdateCheckout, ValidateCheckout, SelectShippingRate, ListCustomerOrders, ApproveOrder (all `Endpoint.cs` files)
- **Phase 3** (11 files): All `Request.cs` files listed in PH3 tasks
- **Phase 4** (9 files created, 4 files modified, 1 file deleted): New Response.cs files (4), existing Response.cs files modified (5), handler files modified (4, removing inline Response), UpdateOrderStatus.Response.cs deleted (1)
- **Phase 5** (3 test files): Order.Method.Tests.cs, OrderCheckoutTests.cs, OrderDiscontinuedTests.cs

### Total: ~50 files touched across 6 phases

## 6. Testing

- **TEST-001**: Verify all endpoints still compile and route correctly after `[FromRoute]` additions.
- **TEST-002**: Verify Swagger/OpenAPI documentation shows correct response status codes after `Produces<T>()` additions.
- **TEST-003**: Verify record Request types still deserialize correctly from JSON bodies.
- **TEST-004**: Verify record Response types still serialize correctly to JSON.
- **TEST-005**: Verify `UpdateOrderStatus.Response.cs` is deleted and handler tests still pass.
- **TEST-006**: Verify all integration tests in `Api.Tests/Scenarios/Ordering/` pass.

## 7. Risks & Assumptions

- **RISK-001**: Adding `[FromRoute]` to parameters that already work without it could cause no change or break binding if the parameter name doesn't exactly match the route template. **Mitigation**: verify each endpoint's route template matches the parameter name; if mismatch found, align the route template name.
- **RISK-002**: Converting `class` to `record` for Request types could break JSON deserialization if the JSON serializer requires a parameterless constructor (records with `init`-only properties use a constructor with all properties). **Mitigation**: the codebase already uses `record` for `CreateOrder.Request` and it works — JSON deserializers support record constructors.
- **RISK-003**: Extracting inline Response types could cause namespace conflicts (e.g., `ApproveOrder.Response` now defined in two files). **Mitigation**: remove the inline definition from the handler file in the same commit that creates the Response.cs file.
- **ASSUMPTION-001**: The prior spec (`refactor-ordering-features-catalog-patterns-1.md`) Phase 2 changes (converting `class` → `record` for shared models) have been completed and no `class` shared models remain.
- **ASSUMPTION-002**: All endpoint routes are correctly defined in `OrderingFeature` constants and don't need modification.

## 8. Related Specifications / Further Reading

- [plan/refactor-ordering-features-catalog-patterns-1.md](./refactor-ordering-features-catalog-patterns-1.md) — Prior plan for model record conversion and handler standardization.
- [spec/spec-design-ordering-domain-logic-extraction.md](../spec/spec-design-ordering-domain-logic-extraction.md) — Handler domain logic extraction.
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.Endpoint.cs` — Canonical Catalog endpoint pattern.
- `service/Api/src/Shared/Application/Extensions/Results/Result.Http.Extensions.cs` — `ToResult()` extension mapping.
