---
goal: Clean up stale domain event tests, rename references, and document the inline event handling pattern
version: 1.0
date_created: 2026-07-07
status: 'Completed'
tags: refactor, events, testing, cleanup
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The previous migration (refactor-buildingblocks-migration-1) deleted domain event classes (`OrderPlacedEvent`, `OrderCanceledEvent`, `OrderResumedEvent`, `ShipmentEvent.Lifecycle.Shipped`) and their consumers, replacing them with inline notification calls in command handlers. However, 5 test files still reference the deleted types, and additional test files reference renamed types (`PaymentMethod` → `PaymentFactory`, `PageIndex` → `PageNumber`). This plan removes/updates those test files, cleans up stale documentation, and establishes the canonical inline event pattern for future reference.

## 1. Requirements & Constraints

- **REQ-001**: Delete 3 test files that solely test deleted domain event record types (no logic worth preserving)
- **REQ-002**: Rewrite 1 test file to test the new inline notification behavior instead of old event dispatch
- **REQ-003**: Rewrite 1 test file that tested consumer invocation via `DomainEvents` collection
- **REQ-004**: Fix 8+ test files that reference `PaymentMethod` → renamed to `PaymentFactory`
- **REQ-005**: Fix 2+ test files that reference `QueryingParameters.PageIndex` → now `.PageNumber`
- **REQ-006**: Remove stale `BuildingBlocks` dependency mentions from all `README.xml` files (Module root + Domain subdirectories)
- **REQ-007**: Update/remove stale comments referencing deleted domain events in `Order.Checkout.cs`
- **REQ-008**: Add `Notifications/NotificationsExtensions.cs` with canonical inline event helper to Shared project for reuse
- **REQ-009**: Test project must build cleanly (`dotnet test --no-restore` passes or at least compiles)
- **CON-001**: Do NOT re-introduce `AddDomainEvent`, `DomainEvents`, or `DomainEventConsumer` patterns
- **CON-002**: The inline pattern must inject `INotificationService` + `ILogger` via constructor, not static helpers
- **PAT-001**: Every command handler with notification side-effects adds a private `SendXxxNotificationAsync` method at the bottom of the handler class
- **PAT-002**: Notification method checks for null/empty email before sending

## 2. Implementation Steps

### Implementation Phase 1: Delete stale domain event test files

- GOAL-001: Remove 3 test files that tested the deleted domain event record classes (no logic to preserve)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `tests/Module.UnitTests/Ordering/Domain/Orders/Events/OrderPlacedEventTests.cs` | | |
| TASK-002 | Delete `tests/Module.UnitTests/Ordering/Domain/Orders/Events/OrderCanceledEventTests.cs` | | |
| TASK-003 | Delete `tests/Module.UnitTests/Ordering/Domain/Orders/Events/OrderResumedEventTests.cs` | | |
| TASK-004 | Delete `tests/Module.UnitTests/Shipping/Domain/Shipments/Events/ShipmentShippedEventTests.cs` | | |
| TASK-005 | Remove empty directories: `tests/.../Events/` dirs under Ordering and Shipping test domains | | |

### Implementation Phase 2: Rewrite EventHandlerInvocationTests.cs

- GOAL-002: The `EventHandlerInvocationTests` tested that `Entity.DomainEvents` collection was populated and that consumers were invoked automatically. Since domain events are now inline, this test must be rewritten to verify the command handler calls `INotificationService.SendAsync`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Read `tests/Module.UnitTests/Ordering/Infrastructure/Notifications/EventHandlerInvocationTests.cs` in full | | |
| TASK-007 | Replace its content with a test that creates a `CancelOrderAdmin.CommandHandler` with a mocked `INotificationService`, invokes `Handle()`, and asserts `SendAsync` was called once with a `NotificationMessage` having `UseCase == OrderCancelled` | | |
| TASK-008 | The new test should inject mocked `IApplicationDbContext` (using `DbSetMock` helper from existing test infrastructure), mocked `ICurrentUser`, mocked `INotificationService`, and mocked `ILogger<CancelOrderAdmin.CommandHandler>` | | |

### Implementation Phase 3: Fix PaymentMethod → PaymentFactory rename in tests

- GOAL-003: 2 test references to `PaymentMethod` static class (renamed to `PaymentFactory`) need updating. Check for additional references.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Read and fix `tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayTests.cs` line 22: change `PaymentMethod.Create(...)` → `PaymentFactory.Create(...)` | | |
| TASK-010 | Read and fix `tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayAuthorizeTests.cs` line 21: same rename | | |
| TASK-011 | Read and fix `tests/Module.UnitTests/Payment/Domain/Payments/PaymentProcessingAsyncTests.cs` line 24: same rename | | |
| TASK-012 | Run `grep -rn "PaymentMethod" tests/` to find any remaining stale references and fix them | | |

### Implementation Phase 4: Fix QueryingParameters.PageIndex in tests

- GOAL-004: Test files use `.PageIndex` which was an extension method on `QueryingParameters`. Replace with `.PageNumber` (the actual record property).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Read and fix `tests/Module.UnitTests/Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethodsHandlerTests.cs` lines 46, 56: replace `Parameters.PageIndex` with `Parameters.PageNumber` | | |
| TASK-014 | Run `grep -rn "PageIndex" tests/` to find any remaining references and fix them | | |

### Implementation Phase 5: Remove stale BuildingBlocks README.xml references

- GOAL-005: Stale documentation references to "BuildingBlocks" as a dependency exist in README.xml files across modules. Remove them.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Search all `README.xml` files for "BuildingBlocks" mentions; replace with "ReSys Shared" or remove the dependency line | | |
| TASK-016 | Fix `service/Api/src/Module/Promotions/README.xml` line ~39 | | |
| TASK-017 | Fix `service/Api/src/Module/Ordering/README.xml` line ~32 | | |
| TASK-018 | Fix `service/Api/src/Module/Payment/README.xml` lines ~35, 54 | | |
| TASK-019 | Fix `service/Api/src/Module/Shipping/README.xml` line ~36 | | |
| TASK-020 | Fix remaining Domain-level README.xml files (Catalog/Domain, Location/Domain, Promotions/Domain, Ordering/Domain, Shipping/Domain) | | |

### Implementation Phase 6: Update stale comments in Order.Checkout.cs

- GOAL-006: Remove comments referencing deleted domain events.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Read `Module/Ordering/Domain/Orders/Order.Checkout.cs` lines 100-115; update comments that say "Domain events (OrderCanceledEvent/OrderResumedEvent) trigger notification consumers" to note that notifications are now sent inline from command handlers | | |

### Implementation Phase 7: Create canonical inline event helper

- GOAL-007: Add a reusable `NotificationHelper` class to Shared so that command handlers don't duplicate the null-email check and the warning-logging pattern.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Create `Shared/Operational/Notifications/Services/NotificationHelper.cs` with a static `TrySendAsync` method: `public static async Task TrySendAsync(this INotificationService service, ILogger logger, NotificationMessage message, object contextId, CancellationToken ct)` that checks `message.Recipient.Identifier` for null/empty, calls `SendAsync`, and logs warning on failure | | |
| TASK-023 | Update the 6 refactored command handlers to use the helper: `await notificationService.TrySendAsync(logger, message, order.Id, ct);` instead of the manual pattern | | |
| TASK-024 | Create `docs/patterns/inline-event-handling.md` documenting the canonical pattern with a code example | | |

### Implementation Phase 8: Verify build and tests

- GOAL-008: Ensure both main and test projects build with zero errors.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Run `dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` and fix any remaining compilation errors | | |
| TASK-026 | Run `dotnet build service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj` and fix any errors | | |
| TASK-027 | Run `dotnet build service/Api/src/Api/Api.csproj` and confirm zero errors | | |

## 3. Alternatives

- **ALT-001**: Keep the old test files and stub out the missing types. Rejected because the test logic tested a pattern that no longer exists; stubs would be misleading.
- **ALT-002**: Keep `IEvent`/`IDomainEvent` interfaces in Shared and support both patterns. Rejected because the interfaces are unused and keeping them creates confusion about which pattern to follow.
- **ALT-003**: Skip the README.xml cleanup. Rejected because stale docs lead to incorrect assumptions about dependencies.

## 4. Dependencies

- **DEP-001**: Completion of `refactor-buildingblocks-migration-1.md` (Phases 1-7 must be done before this plan executes)
- **DEP-002**: Moq mocking library available in test projects (already present)
- **DEP-003**: `AutoFixture` or similar for generating test data (check existing test conventions)

## 5. Files

- **FILE-001** (DELETE): `tests/Module.UnitTests/Ordering/Domain/Orders/Events/OrderPlacedEventTests.cs`
- **FILE-002** (DELETE): `tests/Module.UnitTests/Ordering/Domain/Orders/Events/OrderCanceledEventTests.cs`
- **FILE-003** (DELETE): `tests/Module.UnitTests/Ordering/Domain/Orders/Events/OrderResumedEventTests.cs`
- **FILE-004** (DELETE): `tests/Module.UnitTests/Shipping/Domain/Shipments/Events/ShipmentShippedEventTests.cs`
- **FILE-005** (MODIFY): `tests/Module.UnitTests/Ordering/Infrastructure/Notifications/EventHandlerInvocationTests.cs`
- **FILE-006** (MODIFY): `tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayTests.cs`
- **FILE-007** (MODIFY): `tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayAuthorizeTests.cs`
- **FILE-008** (MODIFY): `tests/Module.UnitTests/Payment/Domain/Payments/PaymentProcessingAsyncTests.cs`
- **FILE-009** (MODIFY): `tests/Module.UnitTests/Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethodsHandlerTests.cs`
- **FILE-010** through **FILE-018** (MODIFY): 9 README.xml files
- **FILE-019** (MODIFY): `Module/Ordering/Domain/Orders/Order.Checkout.cs`
- **FILE-020** (CREATE): `Shared/Operational/Notifications/Services/NotificationHelper.cs`
- **FILE-021** (CREATE): `docs/patterns/inline-event-handling.md`

## 6. Testing

- **TEST-001**: `dotnet build tests/Module.UnitTests` — zero compilation errors
- **TEST-002**: `dotnet build tests/Shared.UnitTests` — zero compilation errors
- **TEST-003**: `dotnet build src/Api/Api.csproj` — zero compilation errors (no regression)
- **TEST-004**: `dotnet test tests/Module.UnitTests --filter "FullyQualifiedName~EventHandlerInvocation"` — the rewritten test passes

## 7. Risks & Assumptions

- **RISK-001**: The test build may have additional pre-existing errors beyond the known 5+ files. The plan must handle these as discovered.
- **RISK-002**: Some test files may import `BuildingBlocks` namespaces that were already removed from source. Unused imports cause warnings (which fail with `TreatWarningsAsErrors=true`).
- **RISK-003**: The `INotificationService` interface may not be easily mockable (check if it's an interface vs concrete class).
- **ASSUMPTION-001**: Test files use Moq for mocking (check existing test projects for mocking library).
- **ASSUMPTION-002**: `AutoFixture` or `Faker` is available for generating test entities.
- **ASSUMPTION-003**: The `EventHandlerInvocationTests` is the only test that tested the old event dispatch mechanism end-to-end.

## 8. Related Specifications / Further Reading

- `plan/refactor-buildingblocks-migration-1.md` — previous migration that deleted event classes
- `docs/codebase/ARCHITECTURE.md` — layer responsibilities
- `service/Api/src/Shared/Operational/Notifications/Services/Notification.Service.Interface.cs` — INotificationService
- `service/Api/src/Shared/Operational/Notifications/Models/Notification.Message.Model.cs` — NotificationMessage record
