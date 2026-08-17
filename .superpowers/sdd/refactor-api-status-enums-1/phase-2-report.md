# Phase 2 Report — Ordering status DTOs → enums

Date: 2026-08-17
Branch: `feature/implement-storefront`
Status: DONE

## Goal
Convert the remaining Ordering status fields (`RecentOrderData.Status`, `OrderTimelineEvent.Type`) to enums and fix their mappings, keeping JSON string serialization via the existing `JsonStringEnumConverter` (GOAL-002, TASK-005..008).

## Changes

### TASK-005 — `OrderTimelineEventType` enum + `OrderTimelineEvent.Type` retyped
- `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs`
  - Added `OrderTimelineEventType` enum (PascalCase members: `Created, Placed, Approved, PaymentProcessing, PaymentCompleted, PaymentFailed, Shipped, Delivered, Canceled`), appended to the existing `Order.Enumerate.cs` per the module's enum-file convention. `OrderStatus` untouched (still `Draft=0, Placed=1, Canceled=2, Expired=4`).
  - Added a trailing newline (file previously lacked one).
- `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/Order.Model.cs`
  - `OrderTimelineEvent.Type`: `string` → `OrderTimelineEventType` (removed `= string.Empty` default; enum defaults to `Created`).

### TASK-006 — `BuildTimeline` enum assignment
- `service/Api/src/Module/Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs` (`BuildTimeline`)
  - Replaced the 9 hardcoded lowercase strings (`"created"`, `"placed"`, `"approved"`, `"payment_processing"`, `"payment_completed"`, `"payment_failed"`, `"shipped"`, `"delivered"`, `"canceled"`) with direct `OrderTimelineEventType.*` enum assignments.

### TASK-007 — `RecentOrderData.Status` → `OrderStatus`
- `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/OrderingDashboard.Model.cs`
  - Added `using Module.Ordering.Domain.Orders;`.
  - `RecentOrderData.Status`: `string` → `OrderStatus` (removed `= default!`).
- `service/Api/src/Module/Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.cs`
  - `Status = o.Status.ToString()` → `Status = o.Status`.

### Test updates (required by the type change)
- `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Shared/Mappings/OrderMappingTests.cs`
  - Line 41: `timeline.Select(e => e.Type).Should().Equal("created", "payment_completed", "placed", "shipped")` → `.Equal(OrderTimelineEventType.Created, OrderTimelineEventType.PaymentCompleted, OrderTimelineEventType.Placed, OrderTimelineEventType.Shipped)`.
- Searched the test project for other references to the old timeline strings (`"created"`, `"placed"`, `"approved"`, `"payment_processing"`, `"payment_completed"`, `"payment_failed"`, `"shipped"`, `"delivered"`, `"canceled"`, `OrderTimelineEvent`): the only remaining matches are `Inventory/Services/StockReservationServiceTests.cs:673,718` asserting `movement.Reason` (a free-text audit field intentionally kept as string), which are unrelated to the timeline. No other test constructs `OrderTimelineEvent` or asserts `RecentOrderData.Status` as a string.

## Constraints honored
- CON-001: TreatWarningsAsErrors — build clean (0 warnings / 0 errors).
- CON-002: Vertical-slice file structure preserved; changes stay in feature files.
- CON-004: `Program.cs` and the global `JsonStringEnumConverter` untouched — enum member names serialize as JSON strings.
- REQ-002: JSON wire format remains strings.
- REQ-003: mappings assign enum values directly (no `.ToString()`).
- REQ-004: new PascalCase enum member names serialize to the exact strings emitted today for `RecentOrderData.Status` (unchanged wire value); `OrderTimelineEvent.Type` changes from lowercase to PascalCase by design (RISK-002).

## RISK-002 (accepted wire change) — documented for future consumers
`OrderTimelineEvent.Type` wire value changes from lowercase (`"created"`) to PascalCase (`"Created"`). Verified safe: both SPAs render `label`, never `type`:
- `app/Admin/src/features/ordering/views/OrderDetail.vue:441` renders `item.label` only.
- `app/Store` order timeline is built from the tracking timestamps in `OrderDetailView.vue:49-61` (`label`/`date` pairs), not from `OrderTimelineEvent.type`.
- `RecentOrderData.Status` wire value is byte-identical (`"Placed"`, `"Draft"`, etc.) — the Admin SPA's `status: string` typing and rendering are unaffected.

## Verification
1. `dotnet build service/Api/src/Api/Api.csproj -v q --nologo` → Build succeeded, 0 Warnings, 0 Errors (run twice: pre- and post-trailing-newline).
2. `dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo` → Build succeeded, 0 Warnings, 0 Errors.
3. Unit tests (xunit v3 MTP runner binary `./Module.UnitTests`):
   - `-class "Module.UnitTests.Ordering.Features.Admin.Shared.Mappings.OrderMappingTests"` → Total: 2, Failed: 0.
   - `-class "Module.UnitTests.Ordering.Features.Admin.Dashboard.Get.GetOrderingDashboardHandlerTests"` → Total: 5, Failed: 0.
   - Broader `-class "Module.UnitTests.Ordering*"` run → Total: 196, Failed: 3 — all three are the pre-existing `OrderStatusValueConverterTests` legacy-string failures (`CheckoutState`, `FulfillmentState`, `PaymentState` legacy mapping), unrelated to this phase and untouched by it.

## Concerns
- None for this phase. The 3 failing Ordering converter tests are pre-existing and out of scope.
- RISK-002 (`OrderTimelineEvent.Type` now PascalCase) is accepted per the brief and documented above for future consumers.