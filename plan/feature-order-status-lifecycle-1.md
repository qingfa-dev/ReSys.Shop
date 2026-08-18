---
goal: Introduce a proper order lifecycle terminal state (OrderStatus.Completed) and rename the terminal checkout state CheckoutState.Complete to CheckoutState.Placed.
version: 1.0
date_created: 2026-08-18
last_updated: 2026-08-18
owner: Ordering
status: 'Completed'
tags: feature, ordering, state-machine, migration
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Ordering domain conflates two distinct state machines on the `Order` aggregate:

- **`CheckoutState`** — the shopper checkout funnel (`Address → PickDeliveryMethod → PickPaymentMethod → Confirm → Complete`). It is only meaningful while the order is a `Draft` cart. Its terminal member is misleadingly named `Complete` even though it is set at the exact moment the order is *placed* (`Order.Method.StateMachine.cs:30,199,217`).
- **`OrderStatus`** — the fulfillment lifecycle (`Draft, Placed, Canceled, Expired`). It has **no normal terminal success state**; only abnormal terminals (`Canceled`, `Expired`). The existing `Order.Complete()` method (`StateMachine.cs:210`) is effectively a stub that only re-sets `CheckoutState.Complete` and never changes `OrderStatus`.

This plan (a) renames `CheckoutState.Complete` → `CheckoutState.Placed` to align the terminal checkout state with `OrderStatus.Placed`, and (b) adds `OrderStatus.Completed = 3` (the reserved value) as the order-level terminal, sets it in `Order.Complete()`, and auto-derives it when all shipments are delivered (the `RecordOrderShipmentState` handler). The pre-existing admin `CompleteOrder` feature (`Features/Admin/Orders/Complete/CompleteOrder.cs`) becomes effective once `Order.Complete()` sets `OrderStatus.Completed`, and remains a valid manual completion path alongside auto-derivation.

## 1. Requirements & Constraints

- **REQ-001**: Rename the `CheckoutState.Complete` enum member to `CheckoutState.Placed` across the codebase (source + tests), preserving enum member order and implicit backing values (`Address=0 … Placed=4`).
- **REQ-002**: Persisted `CheckoutState` strings must be migrated: existing rows with `'Complete'` must become `'Placed'` in the `ordering.orders` table (enums are stored via `HasConversion<string>`).
- **REQ-003**: Add `OrderStatus.Completed = 3` (the currently reserved value `3`), making `OrderStatus` = `Draft=0, Placed=1, Canceled=2, Completed=3, Expired=4`.
- **REQ-004**: `Order.Complete(modifiedBy)` must set `Status = OrderStatus.Completed` (guard: only from `Placed`), set `CompletedAtUtc` and `ModifiedBy`, and keep `CheckoutState = CheckoutState.Placed` as the terminal checkout state.
- **REQ-005**: Auto-derive `OrderStatus.Completed` when all shipments reach `ShipmentState.Delivered`, via the `RecordOrderShipmentStateCommandHandler`; non-`Delivered` fulfillment must not complete the order.
- **REQ-006**: Dashboard surfaces must reflect the new status: `OrderStatusBreakdownData` gains a `Completed` count, `GetOrderingDashboard` counts it, and `GetDashboard.MapOrderStatus` maps `OrderStatus.Completed → ActivityStatus.Completed`.
- **SEC-001**: No new security surface. The auto-completion path uses the fixed actor `"System"` because shipment sync runs outside an authenticated user context; actor attribution must remain tamper-evident in `ModifiedBy`.
- **CON-001**: `TreatWarningsAsErrors=true` globally — any compiler warning fails the build. All switches over `OrderStatus` must remain exhaustive.
- **CON-002**: Enum values are persisted as strings with no DB check constraint; therefore the `Completed` value requires no schema change, only the `CheckoutState` data migration in REQ-002.
- **GUD-001**: Follow the existing result-object pattern: domain methods return `Result`, never throw for business-state transitions.
- **PAT-001**: Preserve the existing forward-only checkout state machine (`Order.Method.Checkout.cs`); rename references without adding or removing transitions.

## 2. Implementation Steps

### Implementation Phase 1 — Rename CheckoutState.Complete → CheckoutState.Placed

- GOAL-001: Rename the terminal checkout state to `Placed` in the domain, features, tests, and persist the string rename in the database.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs` (CheckoutState enum, lines 14-21): rename the terminal member `Complete` → `Placed` and update the comment on line 13 from `→ Confirm → Complete` to `→ Confirm → Placed`. Do not reorder or renumber members. | ✅ | 2026-08-18 |
| TASK-002 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`: replace `CheckoutState.Complete` with `CheckoutState.Placed` at line 25 (`RequireEmail`) and in the `AdvanceCheckoutState` transition table at lines 53-54. Update the invariant comment on line 5 from `Complete state is terminal` to `Placed state is terminal`. | ✅ | 2026-08-18 |
| TASK-003 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`: replace `CheckoutState.Complete` with `CheckoutState.Placed` at lines 30 (`Finalize`), 199 (`Place`), and 217 (`Complete`). | ✅ | 2026-08-18 |
| TASK-004 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs` line 22: replace `CheckoutState.Complete => true` with `CheckoutState.Placed => true`. | ✅ | 2026-08-18 |
| TASK-005 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` line 35 and `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs` line 52: replace the `CheckoutState.Complete` target in `InvalidCheckoutTransition(...)` with `CheckoutState.Placed`. | ✅ | 2026-08-18 |
| TASK-006 | Update test references to `CheckoutState.Complete`: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` lines 284, 327, 611, 616, and `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs` line 177 — replace with `CheckoutState.Placed`. | ✅ | 2026-08-18 |
| TASK-007 | Add an EF Core migration named `OrderStatusCompletedAndCheckoutRename` (run `dotnet ef migrations add` from `service/Api`); because enums are stored as strings with no model change, the migration contains no schema diff. In its `Up` method add `migrationBuilder.Sql("UPDATE \\"ordering\\".\\"orders\\" SET \\"CheckoutState\\" = 'Placed' WHERE \\"CheckoutState\\" = 'Complete';")`. | ✅ | 2026-08-18 |

### Implementation Phase 2 — Add OrderStatus.Completed + auto-derivation

- GOAL-002: Add the `Completed` order lifecycle status, make `Order.Complete()` transition to it, and auto-derive it from full delivery.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs` (`OrderStatus` enum, lines 5-11): add `Completed = 3,` between `Canceled = 2` and `Expired = 4`; replace the comment on line 4 (`Value 3 intentionally unused — reserved for future status`) with a note that `Completed` occupies value 3. | ✅ | 2026-08-18 |
| TASK-009 | In `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs` lines 209-223 (`Complete` method): change the body so that, after the existing `if (order.Status != OrderStatus.Placed) return OrderResult.Errors.InvalidStatusTransition;` guard, it sets `order.Status = OrderStatus.Completed;`, keeps `order.CheckoutState = CheckoutState.Placed;`, sets `order.CompletedAtUtc = DateTimeOffset.UtcNow;`, `order.ModifiedAtUtc = DateTimeOffset.UtcNow;`, and `order.ModifiedBy = modifiedBy;`. Update the summary/guard comments (lines 209, 212-215) from `Complete state is terminal`/`not in Placed state` wording to describe the Placed→Completed transition. | ✅ | 2026-08-18 |
| TASK-010 | In `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentState.cs` (handler, lines 6-25): after `order.ShipmentState = command.FulfillmentState;` (line 21) and before `SaveChangesAsync`, add: `if (command.FulfillmentState == ShipmentState.Delivered && order.Status == OrderStatus.Placed) order.Complete("System");`. Add `using Module.Ordering.Domain.Orders;` if not already present. | ✅ | 2026-08-18 |
| TASK-011 | In `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.cs` (statusBreakdown initializer, lines 28-34): add `Completed = await baseQuery.CountAsync(o => o.Status == OrderStatus.Completed, cancellationToken),`. | ✅ | 2026-08-18 |
| TASK-012 | In `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/OrderingDashboard.Model.cs` (`OrderStatusBreakdownData`, lines 25-31): add `public int Completed { get; init; }`. | ✅ | 2026-08-18 |
| TASK-013 | In `service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.cs` (`MapOrderStatus`, lines 232-239): add the case `OrderStatus.Completed => ActivityStatus.Completed,` before the `_ => throw` arm. | ✅ | 2026-08-18 |
| TASK-014 | Confirm the pre-existing admin feature `service/Api/src/Module/Ordering/Features/Admin/Orders/Complete/CompleteOrder.cs` requires no change: it calls `order.Complete(...)` (line 26), which now correctly sets `OrderStatus.Completed`. Do not modify it. | ✅ | 2026-08-18 |

### Implementation Phase 3 — Tests and verification

- GOAL-003: Update and add unit tests for the renamed enum and the new lifecycle status, then verify build, test suite, and feature-convention checks.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | In `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`: update `Complete_WhenPlaced_ShouldSucceed` (lines 319-328) to assert `order.Status.Should().Be(OrderStatus.Completed);` in addition to the existing `CheckoutState` assertion (now `CheckoutState.Placed`). Keep `Complete_WhenDraft_ShouldFail` (lines 330-336) asserting failure. | ✅ | 2026-08-18 |
| TASK-016 | In `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentStateTests.cs`: add a test that a `Placed` order receiving `FulfillmentState = ShipmentState.Delivered` results in `Status == OrderStatus.Completed`. Set `order.Status = OrderStatus.Placed;` on the created order (created via `OrderMethod.Create(...)`) before saving, because `Order.Complete()` guards against non-`Placed` orders. | ✅ | 2026-08-18 |
| TASK-017 | In `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentStateTests.cs`: add a test that a `Placed` order receiving `FulfillmentState = ShipmentState.Shipped` (non-delivered) keeps `Status == OrderStatus.Placed`. | ✅ | 2026-08-18 |
| TASK-018 | In `service/Api/tests/Module.UnitTests/Dashboard/Features/Admin/Get/GetDashboardHandlerTests.cs`: add a case asserting `MapOrderStatus(OrderStatus.Completed)` maps to `ActivityStatus.Completed` if the mapping is exercised by an existing test fixture; otherwise verify the switch remains exhaustive by compiling. | ✅ | 2026-08-18 |
| TASK-019 | Run `dotnet build service/Api/src/Api/Api.csproj` (expect 0 warnings/0 errors) and `dotnet test service/Api/tests/Module.UnitTests` (expect all pass). Then run `bash scripts/check-feature-conventions.sh` and confirm no drift. | ✅ | 2026-08-18 |

## 3. Alternatives

- **ALT-001**: Name the new terminal status `OrderStatus.Delivered` instead of `Completed`. Rejected: `Delivered` is the `ShipmentState`/shipment-level concept (`ShipmentState.Delivered` already exists); `Completed` is the order-level terminal and matches the existing `Order.Complete()` method, `CompletedAtUtc` timestamp, `OrderResult.Success.Completed`, and the existing `ActivityStatus.Completed` dashboard value.
- **ALT-002**: Manual-only completion via the existing admin `CompleteOrder` feature, with no auto-derivation from shipments. Rejected: the user selected automatic derivation when all shipments are delivered; auto-derivation is a single hook in `RecordOrderShipmentState`. Both paths are retained.
- **ALT-003**: Remove the terminal `CheckoutState.Complete` member entirely rather than renaming it. Rejected: `CheckoutState` is relied upon as the terminal checkout state by `RequireEmail`, `ValidateCheckoutPrerequisites`, and the storefront cart/order models; renaming preserves the state machine without deleting a member.

## 4. Dependencies

- **DEP-001**: .NET SDK + EF Core tooling (`dotnet ef`) to add the `OrderStatusCompletedAndCheckoutRename` migration.
- **DEP-002**: Existing enums already present in the codebase — `OrderStatus`, `CheckoutState` (`Order.Enumerate.cs`), `ShipmentState` (`Shared.Application.Domain.Orders`), and `ActivityStatus` (`Dashboard/Features/Admin/Shared/Models/Activity.Enumerate.cs`). No new NuGet packages.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs` — rename `CheckoutState.Complete`→`Placed`; add `OrderStatus.Completed = 3`; update comments.
- **FILE-002**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` — rename `CheckoutState.Complete` references and invariant comment.
- **FILE-003**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs` — rename `CheckoutState.Complete`; make `Complete()` set `OrderStatus.Completed`.
- **FILE-004**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs` — rename `CheckoutState.Complete` switch arm.
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — rename `CheckoutState.Complete` target.
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs` — rename `CheckoutState.Complete` target.
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentState.cs` — auto-complete on `Delivered`.
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.cs` — add `Completed` count.
- **FILE-009**: `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/OrderingDashboard.Model.cs` — add `Completed` to `OrderStatusBreakdownData`.
- **FILE-010**: `service/Api/src/Module/Dashboard/Features/Admin/Get/GetDashboard.cs` — add `Completed` mapping.
- **FILE-011**: `service/Api/src/Migrations/` — new `OrderStatusCompletedAndCheckoutRename` migration with the data `UPDATE`.
- **FILE-012**: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs` — rename references; assert `Status == Completed`.
- **FILE-013**: `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Tests.cs` — rename `CheckoutState.Complete` reference.
- **FILE-014**: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentStateTests.cs` — add delivered/non-delivered completion tests.
- **FILE-015**: `service/Api/tests/Module.UnitTests/Dashboard/Features/Admin/Get/GetDashboardHandlerTests.cs` — cover `Completed` mapping if fixture exists.

## 6. Testing

- **TEST-001**: `Order.Method.Tests.Complete_WhenPlaced_ShouldSucceed` — after `order.Complete("tester")` on a placed order, `order.Status == OrderStatus.Completed` and `order.CheckoutState == CheckoutState.Placed`.
- **TEST-002**: `Order.Method.Tests.Complete_WhenDraft_ShouldFail` — completion of a `Draft` order returns failure (guard preserved).
- **TEST-003**: `RecordOrderShipmentState` — a `Placed` order receiving `FulfillmentState == Delivered` becomes `OrderStatus.Completed`.
- **TEST-004**: `RecordOrderShipmentState` — a `Placed` order receiving `FulfillmentState == Shipped` (non-delivered) stays `OrderStatus.Placed`.
- **TEST-005**: `GetDashboardHandlerTests` — `MapOrderStatus(OrderStatus.Completed)` maps to `ActivityStatus.Completed`.
- **TEST-006**: Full verification — `dotnet build` (0 warnings/0 errors), full `Module.UnitTests` suite green, `scripts/check-feature-conventions.sh` no drift.

## 7. Risks & Assumptions

- **RISK-001**: The `CheckoutState` data migration (`'Complete'` → `'Placed'`) assumes pre-existing rows store the literal string `'Complete'`. If any environment stores a different casing, the `UPDATE` will miss rows. Mitigation: the migration is scoped to the exact string `'Complete'`; verify row counts before applying in a non-empty environment.
- **RISK-002**: Auto-derivation (via shipment sync) and the manual admin `CompleteOrder` feature both set `OrderStatus.Completed`. They are idempotent (guard requires `Placed`), so no double-transition risk; the only effect is that an admin cannot manually re-complete an already-completed order (correct).
- **ASSUMPTION-001**: The shipment sync auto-completion path uses the fixed actor `"System"` because `RecordOrderShipmentStateCommandHandler` has no `ICurrentUser`; this is acceptable for an unattended background transition.
- **ASSUMPTION-002**: Completion is derived only from `ShipmentState.Delivered` (all shipments delivered). `ShipmentState.Partial`/`Shipped`/`Pending` do not complete the order.

## 8. Related Specifications / Further Reading

- [refactor-api-status-enums-1.md](./refactor-api-status-enums-1.md) — prior (completed) plan that introduced `ActivityStatus` (including `Completed`) and the `OrderStatus` enum.
- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md) — Ordering domain state machine and layer responsibilities.
