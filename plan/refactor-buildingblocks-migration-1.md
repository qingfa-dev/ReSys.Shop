---
goal: Migrate from external BuildingBlocks package to Shared project and replace domain event consumers with inline handler pattern
version: 1.0
date_created: 2026-07-07
status: 'Completed'
tags: refactor, migration, events, architecture
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The codebase has ~150 files referencing the now-removed external `BuildingBlocks.*` NuGet package, making the project non-compilable. The `Shared` project already defines replacements (`ICommand`, `IResult`, `INotificationService`, `IEvent`, `IDomainEvent`, `HasPermission`, `QueryingModel`, etc.), but modules still reference the old namespaces. This plan replaces the old BuildingBlocks event system (separate event classes + `DomainEventConsumer<T>` handlers) with an **inline event handling pattern** inside command handlers, and fixes all remaining BuildingBlocks namespace references to use `Shared.*` equivalents.

## 1. Requirements & Constraints

- **REQ-001**: Remove all `using BuildingBlocks.*` directives from the codebase (~150 files across Ordering, Shipping, Payment, Promotions modules)
- **REQ-002**: Eliminate the old domain event classes (`OrderPlacedEvent`, `OrderCanceledEvent`, `OrderResumedEvent`, `ShipmentEvent.Lifecycle.Shipped`) that inherit from the missing `BuildingBlocks.Mediators.Events.DomainEvent`
- **REQ-003**: Eliminate the old `DomainEventConsumer<T>` consumer classes (`OrderPlacedConsumer`, `OrderCanceledConsumer`, `OrderResumedConsumer`, `ShipmentShippedConsumer`)
- **REQ-004**: Replace the `entity.AddDomainEvent(new XxxEvent(...))` + auto-dispatch pattern with explicit inline notification calls after `SaveChangesAsync`
- **REQ-005**: New pattern must use `Shared.Operational.Notifications.Services.INotificationService` for sending notifications directly from command handlers
- **REQ-006**: All `BuildingBlocks.Querying.*` references must use `Shared.Operational.Persistence.Specifications.Querying.*`
- **REQ-007**: All `BuildingBlocks.Authorization.Attributes` imports must be removed (the `HasPermission()` extension is already available via `Shared.Security.Authorization.Attributes` in GlobalUsing.cs)
- **REQ-008**: All `BuildingBlocks.Notifications.*` references must use `Shared.Operational.Notifications.*`
- **REQ-009**: All `BuildingBlocks.Identity.Domain.AccessControls` references must use `Shared.Security.Authorization.Features`
- **REQ-010**: All `BuildingBlocks.Persistence.Abstractions` references must use the Shared `IEntityTypeConfiguration` pattern (via `Microsoft.EntityFrameworkCore`)
- **REQ-011**: All `BuildingBlocks.OpenApi.Metadata.Schemas` references must migrate to Shared OpenAPI conventions
- **REQ-012**: All `BuildingBlocks.Calculators` references must inline or remove
- **REQ-013**: All `BuildingBlocks.Models` references must use Shared equivalents
- **REQ-014**: Project must build with zero warnings after migration (`TreatWarningsAsErrors=true`)
- **CON-001**: No changes to the Catalog, Identity, Location, Profile modules (they have no BuildingBlocks references)
- **CON-002**: The `Entity` base class in `Shared.Application.Domain.Models` must NOT be modified — it stays clean of event infrastructure
- **PAT-001**: Follow the existing CQRS pattern: command handler does business logic → `SaveChangesAsync` → inline notification → return result

## 2. Implementation Steps

### Implementation Phase 1: Inline Event Pattern — Pilot (Ordering Module)

- GOAL-001: Migrate the Ordering module from the old event/consumer pattern to inline event handling. This is the pilot that establishes the pattern for all other modules.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `OrderPlacedEvent.cs`, `OrderCanceledEvent.cs`, `OrderResumedEvent.cs` from `Module/Ordering/Domain/Orders/Events/` | | |
| TASK-002 | Delete `OrderPlacedConsumer.cs`, `OrderCanceledConsumer.cs`, `OrderResumedConsumer.cs` from `Module/Ordering/Infrastructure/Notifications/` | | |
| TASK-003 | Refactor `CreateOrderFromCart.cs` — remove `cart.AddDomainEvent(new OrderPlacedEvent(...))` call; add inline private method `SendOrderPlacedNotificationAsync(Order)` that constructs a `NotificationMessage` via builder and sends via `INotificationService` | | |
| TASK-004 | Refactor `CancelOrderAdmin.cs` — remove `order.AddDomainEvent(new OrderCanceledEvent(...))` call; add inline `SendOrderCanceledNotificationAsync(Order)` | | |
| TASK-005 | Refactor `CancelOrder.cs` (Storefront) — same as TASK-004 | | |
| TASK-006 | Refactor `ResumeOrder.cs` — remove `order.AddDomainEvent(new OrderResumedEvent(...))` call; add inline `SendOrderResumedNotificationAsync(Order)` | | |
| TASK-007 | Refactor `ResendOrderConfirmationEmail.cs` — remove `cart.AddDomainEvent(new OrderPlacedEvent(...))` call; inline the notification send | | |

#### Concrete Pattern for Inline Event Handling

Every command handler in Ordering that previously raised a domain event follows this exact pattern:

```csharp
// BEFORE (old pattern):
order.AddDomainEvent(new OrderPlacedEvent(
    order.Id, order.Number, order.UserId!.Value,
    order.Email ?? string.Empty, order.Total, order.CompletedAtUtc!.Value));
await dbContext.SaveChangesAsync(cancellationToken);

// AFTER (new inline pattern):
await dbContext.SaveChangesAsync(cancellationToken);
await SendOrderPlacedNotificationAsync(order, cancellationToken);

// ---- private helper at the bottom of the handler class ----
private async Task SendOrderPlacedNotificationAsync(Order order, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(order.Email))
        return;

    var message = NotificationMessage.Create(
        useCase: NotificationUseCase.OrderConfirmed,
        recipient: NotificationRecipient.Create(order.Email, order.Number),
        channel: NotificationChannel.Email,
        context: NotificationContext.Create(
            (NotificationParameterType.OrderNumber, order.Number),
            (NotificationParameterType.OrderTotal, order.Total.ToString("F2", CultureInfo.InvariantCulture)),
            (NotificationParameterType.UserFirstName, order.Email!.Split('@')[0])));

    var result = await notificationService.SendAsync(message, ct);
    if (result.IsFailure)
        logger.LogWarning("Failed to send order confirmation for {OrderId}: {Errors}",
            order.Id, string.Join("; ", result.Failures.Select(f => f.Description)));
}
```

The `CommandHandler` constructor must inject `INotificationService notificationService` and `ILogger<CommandHandler> logger`.

### Implementation Phase 2: Inline Event Pattern — Shipping Module

- GOAL-002: Migrate the Shipping module to the same inline pattern.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Delete `ShipmentShippedEvent.cs` from `Module/Shipping/Domain/Shipments/Events/` | | |
| TASK-009 | Delete `ShipmentShippedConsumer.cs` from `Module/Shipping/Infrastructure/Notifications/` | | |
| TASK-010 | Delete empty `Module/Shipping/Domain/Shipments/Events/` directory | | |
| TASK-011 | Refactor `MarkShipmentShipped.cs` — remove `shipment.AddDomainEvent(...)` call; add inline `SendShipmentShippedNotificationAsync(Shipment)` | | |

### Implementation Phase 3: Fix BuildingBlocks.Authorization.Attributes imports

- GOAL-003: Remove all stale `using BuildingBlocks.Authorization.Attributes;` imports. The `HasPermission()` extension is already global via `Shared.Security.Authorization.Attributes` in `Module/GlobalUsing.cs`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Remove `using BuildingBlocks.Authorization.Attributes;` from all `.Endpoint.cs` files in Payment module (~9 files) | | |
| TASK-013 | Remove `using BuildingBlocks.Authorization.Attributes;` from all `.Endpoint.cs` files in Ordering module (~14 files) | | |
| TASK-014 | Remove `using BuildingBlocks.Authorization.Attributes;` from all `.Endpoint.cs` files in Shipping module (~14 files) | | |
| TASK-015 | Remove `using BuildingBlocks.Authorization.Attributes;` from all `.Endpoint.cs` files in Promotions module (~30 files) | | |

### Implementation Phase 4: Fix BuildingBlocks.Notifications.* imports

- GOAL-004: Replace all `BuildingBlocks.Notifications.*` imports with `Shared.Operational.Notifications.*` equivalents. The notification model APIs have different signatures so code must be adapted.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Refactor `OrderCanceledConsumer.cs` → replaced by inline handling (TASK-004, TASK-005), but if retained temporarily: fix `using BuildingBlocks.Notifications.Models` → `Shared.Operational.Notifications.Models`, `BuildingBlocks.Notifications.Services` → `Shared.Operational.Notifications.Services`, `BuildingBlocks.Notifications.Templates` → `Shared.Operational.Notifications.Templates` | | |
| TASK-017 | Same for `OrderResumedConsumer.cs` → superseded by TASK-006 | | |
| TASK-018 | Same for `ShipmentShippedConsumer.cs` → superseded by TASK-011 | | |
| TASK-019 | Fix `NotificationTemplateUseCase.SystemOrderConfirmation` → `NotificationUseCase.OrderConfirmed` | | |
| TASK-020 | Fix `NotificationTemplateUseCase.SystemOrderFailed` → `NotificationUseCase.OrderCancelled` (note double-L spelling difference) | | |
| TASK-021 | Fix `BuildingBlocks.Notifications.Models.NotificationMessage.Create(...)` → `Shared.Operational.Notifications.Models.NotificationMessage.Create(useCase, recipient, channel, context, ...)` (note: takes 4+ args now, not 3) | | |
| TASK-022 | Fix `NotificationRecipient.Create(email, name)` → shared is same signature, verify namespace | | |
| TASK-023 | Fix `NotificationContext.Create(...)` → shared is same signature `(NotificationParameterType, string? value)[]` | | |

### Implementation Phase 5: Fix BuildingBlocks.Querying.* imports

- GOAL-005: Replace all `BuildingBlocks.Querying.*` imports with `Shared.Operational.Persistence.Specifications.Querying.*`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Replace `using BuildingBlocks.Querying.Extensions;` → `using Shared.Operational.Persistence.Specifications.Querying;` in all affected files (~15 files across Payment, Ordering, Shipping, Promotions) | | |
| TASK-025 | Replace `using BuildingBlocks.Querying.Models;` → `using Shared.Operational.Persistence.Specifications.Querying;` (~20 files) | | |
| TASK-026 | Replace `using BuildingBlocks.Querying.Helpers;` → check if Shared has equivalent; if not, inline the helper logic (~3 files in Ordering, Shipping validators) | | |
| TASK-027 | Fix `new BuildingBlocks.Querying.Models.QueryingParameters()` → use `QueryingParametersExtensions.ParseAll()` from Shared | | |

### Implementation Phase 6: Fix Remaining BuildingBlocks imports

- GOAL-006: Fix all remaining scattered BuildingBlocks namespace references.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | `Module/Ordering/Features/Shared/OrderingFeature.Admin.cs`: remove `using BuildingBlocks.Identity.Domain.AccessControls;` and `using BuildingBlocks.Identity.Domain.AccessControls.Stores;` (unused — code already uses `Shared.Security.Authorization.Features` and `Shared.Security.Identity.Domain.Permissions`) | | |
| TASK-029 | `Module/Shipping/Features/Shared/ShippingFeature.Admin.cs`: same as TASK-028 | | |
| TASK-030 | `Module/Promotions/Features/Shared/PromotionsFeature.Admin.cs`: same as TASK-028 | | |
| TASK-031 | `Module/Payment/Domain/Gateways/Gateway.cs` and `Module/Payment/Infrastructure/Gateways/Stripe/StripeGateway.cs`: remove `using BuildingBlocks.Models;` — verify if types exist in Shared or inline | | |
| TASK-032 | `Module/Payment/Persistence/PaymentModelConfiguration.cs`: remove `using BuildingBlocks.Persistence.Abstractions;` — EF Core `IEntityTypeConfiguration` is from `Microsoft.EntityFrameworkCore` | | |
| TASK-033 | `Module/Shipping/Persistence/ShippingModelConfiguration.cs`: same as TASK-032 | | |
| TASK-034 | `Module/Promotions/Persistence/PromotionsModelConfiguration.cs`: same as TASK-032 | | |
| TASK-035 | `Module/Payment/Persistence/Configurations/PaymentMethods/PaymentMethodConfiguration.cs`: remove `using BuildingBlocks.Persistence.Configurations.Dictionaries;` — replace with direct EF config or Shared equivalent if exists | | |
| TASK-036 | `Module/Promotions/Persistence/Configurations/PromotionActions/PromotionActionConfiguration.cs`: same as TASK-035 | | |
| TASK-037 | `Module/Promotions/Persistence/Configurations/PromotionRules/PromotionRuleConfiguration.cs`: same as TASK-035 | | |
| TASK-038 | Promotions domain calculators (`FlatPercentItemTotalCalculator.cs`, `FlatRateCalculator.cs`, `FlexiRateCalculator.cs`, `PercentOnLineItemCalculator.cs`, `PriceSackCalculator.cs`, `PromotionsCalculatorResult.cs`, `TieredFlatRateCalculator.cs`, `TieredPercentCalculator.cs`): remove `using BuildingBlocks.Calculators;` — either remove unused imports or replace with Shared equivalent | | |
| TASK-039 | All `.SchemaDoc.cs` files in Shipping and Promotions (~17 files): remove `using BuildingBlocks.OpenApi.Metadata.Schemas;` — if Shared has OpenAPI conventions, update; otherwise remove unused import | | |
| TASK-040 | `Module/Catalog/Features/Admin/Taxonomies/Taxons/Services/AutoClassification/QueryingTaxonRuleEvaluator.cs`: update comment referencing `BuildingBlocks.Querying.Helpers` | | |

### Implementation Phase 7: Verify build and tests

- GOAL-007: Ensure the project compiles with zero warnings and existing tests pass.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-041 | Run `dotnet build service/Api/src/Api/Api.csproj` and fix any remaining compilation errors | | |
| TASK-042 | Run `dotnet test service/Api/tests/Module.UnitTests` to verify unit tests pass | | |
| TASK-043 | Run `dotnet test service/Api/tests/Shared.UnitTests` to verify Shared unit tests pass | | |

## 3. Alternatives

- **ALT-001**: Keep the BuildingBlocks package as a NuGet dependency. Rejected because the package is gone/unsupported and the Shared project already has the replacements.
- **ALT-002**: Keep the event/consumer pattern but port to MediatR INotificationHandler. Rejected because the inline pattern is simpler (fewer files, less indirection, better traceability) and matches the existing "command handler does everything" convention.
- **ALT-003**: Batch-replace all BuildingBlocks namespaces with a script. Rejected because each namespace needs manual verification of API differences (especially Notifications and Querying signatures).
- **ALT-004**: Add domain event collection to the `Entity` base class. Rejected per CON-002 — the user explicitly wants inline handling without Entity-level event infrastructure.

## 4. Dependencies

- **DEP-001**: No new NuGet packages required. Shared project already has all necessary types.
- **DEP-002**: `Shared.Operational.Notifications.Models.NotificationChannel` enum must be verified to exist and be importable.
- **DEP-003**: `INotificationService` is registered in DI via `AddNotifications()` in `Shared/Operational/Notifications/Notification.Extension.cs`. The API's `Program.cs` must call `builder.AddNotifications()` — verify this is already the case (check `AddOperational` chain).

## 5. Files

- **FILE-001** (DELETE): `Module/Ordering/Domain/Orders/Events/OrderPlacedEvent.cs`
- **FILE-002** (DELETE): `Module/Ordering/Domain/Orders/Events/OrderCanceledEvent.cs`
- **FILE-003** (DELETE): `Module/Ordering/Domain/Orders/Events/OrderResumedEvent.cs`
- **FILE-004** (DELETE): `Module/Ordering/Infrastructure/Notifications/OrderPlacedConsumer.cs`
- **FILE-005** (DELETE): `Module/Ordering/Infrastructure/Notifications/OrderCanceledConsumer.cs`
- **FILE-006** (DELETE): `Module/Ordering/Infrastructure/Notifications/OrderResumedConsumer.cs`
- **FILE-007** (DELETE): `Module/Shipping/Domain/Shipments/Events/ShipmentShippedEvent.cs`
- **FILE-008** (DELETE): `Module/Shipping/Infrastructure/Notifications/ShipmentShippedConsumer.cs`
- **FILE-009** (MODIFY): `Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — remove AddDomainEvent, add inline notification send
- **FILE-010** (MODIFY): `Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` — same pattern
- **FILE-011** (MODIFY): `Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` — same pattern
- **FILE-012** (MODIFY): `Module/Ordering/Features/Admin/Orders/Resume/ResumeOrder.cs` — same pattern
- **FILE-013** (MODIFY): `Module/Ordering/Features/Admin/Orders/ResendConfirmationEmail/ResendOrderConfirmationEmail.cs` — same pattern
- **FILE-014** (MODIFY): `Module/Shipping/Features/Admin/Shipments/Ship/MarkShipmentShipped.cs` — same pattern
- **FILE-015** through **FILE-075** (MODIFY): ~60 `.Endpoint.cs` files across Payment, Ordering, Shipping, Promotions — remove stale `using BuildingBlocks.Authorization.Attributes;`
- **FILE-076** through **FILE-100** (MODIFY): ~25 files with `BuildingBlocks.Querying.*` references
- **FILE-101** through **FILE-104** (MODIFY): 4 feature admin files removing `BuildingBlocks.Identity.Domain.AccessControls` imports
- **FILE-105** through **FILE-112** (MODIFY): ~8 domain calculator files removing `BuildingBlocks.Calculators` imports
- **FILE-113** through **FILE-120** (MODIFY): ~6 persistence config files, ~2 Payment gateway files, ~17 SchemaDoc files

## 6. Testing

- **TEST-001**: `dotnet build` must succeed with zero warnings (`TreatWarningsAsErrors=true`)
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all existing unit tests pass (no functional change to business logic)
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` — all existing Shared tests pass
- **TEST-004**: `dotnet test service/Api/tests/Api.Tests` — integration tests pass (if Docker available)
- **TEST-005**: Manual verification that `NotificationChannel.Email` is the correct enum member name in Shared (verify against `NotificationEnumerate.Channel.cs`)

## 7. Risks & Assumptions

- **RISK-001**: The old `DomainEventConsumer<T>` base class from BuildingBlocks may have provided automatic retry/Hangfire integration. The inline pattern does not include retry — failed notifications are logged but not retried. If retry is needed, a follow-up plan must add Hangfire job creation in the inline handler.
- **RISK-002**: The `BuildingBlocks.Notifications.Models.NotificationMessage` and `Shared.Operational.Notifications.Models.NotificationMessage` have different constructors. Shared requires `NotificationChannel` parameter. Code must be adapted carefully.
- **RISK-003**: The `BuildingBlocks.Querying.Helpers` namespace has no clear Shared equivalent. If the helpers provided expression-building utilities, they must be replaced or inlined.
- **RISK-004**: The old `entity.AddDomainEvent()` method is defined in the now-removed BuildingBlocks package. After deleting, compilation will fail until all call sites are migrated. This means TASK-001 through TASK-011 must be executed as a single block (not incrementally).
- **RISK-005**: Some of the `BuildingBlocks.*` imports in SchemaDoc and Persistence config files may be unused imports that won't cause compilation errors (just warnings that fail the build). Each must be verified.
- **ASSUMPTION-001**: `builder.AddOperational()` in Program.cs already calls `AddNotifications()` transitively. If not, `AddNotifications()` must be added separately.
- **ASSUMPTION-002**: `Shared.Operational.Notifications.Models.NotificationChannel.Email` exists (verify in `NotificationEnumerate.Channel.cs`).

## 8. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — layer responsibilities and data flow
- `docs/codebase/CONCERNS.md` — tech debt including the BuildingBlocks migration
- `service/Api/src/Shared/Operational/Notifications/Models/Notification.Message.Model.cs` — new NotificationMessage API
- `service/Api/src/Shared/Operational/Notifications/Models/Notification.Message.Builder.cs` — fluent builder
- `service/Api/src/Shared/Operational/Notifications/Templates/NotificationEnumerate.Usecase.cs` — NotificationUseCase enum
- `service/Api/src/Shared/Operational/Notifications/Templates/NotificationEnumerate.Param.cs` — NotificationParameterType enum
- `service/Api/src/Shared/Operational/Notifications/Templates/NotificationEnumerate.Channel.cs` — NotificationChannel enum
