---
goal: Restructure Payment Module to Match Catalog Standard
version: 1.0
date_created: 2026-07-11
last_updated: 2026-07-11
owner: Payment Module Team
status: Planned
tags: refactor, architecture, structure, payment
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Restructure the Payment module (`service/Api/src/Module/Payment`) to follow the same directory layout and namespace conventions as the Catalog module. Catalog places all service/implementation code inside `Features/`, has no `Infrastructure/` directory, and keeps persistence schema files directly in `Persistence/` (not in a `Constants/` subdirectory). This refactor moves files only — no business logic changes.

## 1. Requirements & Constraints

- **REQ-001**: Remove `Infrastructure/` directory entirely — move all its contents into `Features/Admin/PaymentMethods/Services/`.
- **REQ-002**: Move `Persistence/Constants/PaymentSchema.cs` to `Persistence/PaymentSchema.cs` (match Catalog's `Persistence/CatalogSchema.cs`).
- **REQ-003**: Gateway implementations live at `Features/Admin/PaymentMethods/Services/Gateways/{Provider}/`.
- **REQ-004**: Options/config classes live alongside the services they configure.
- **REQ-005**: All namespaces updated to reflect new locations. No stale `using` directives.
- **REQ-006**: `Domain/Gateways/` directory preserved — domain abstractions (`IPaymentGatewayActionProvider`, `IGatewayRegistry`, etc.) stay in the domain layer.
- **REQ-007**: Zero business logic changes. Moves only. Build passes with 0 warnings.
- **CON-001**: Catalog's pattern: services live under `Features/Admin/{Entity}/Services/{Group}/` with `Abstractions/` sub-folder for interfaces. All concrete implementations in the parent directory.
- **CON-002**: `TreatWarningsAsErrors=true`.

## 2. Implementation Steps

### Implementation Phase 1: Move Persistence Schema

- GOAL-001: Flatten `Persistence/Constants/` to match Catalog's `Persistence/Schema.cs` pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Move `Persistence/Constants/PaymentSchema.cs` → `Persistence/PaymentSchema.cs` | | |
| TASK-002 | Update namespace from `Module.Payment.Persistence.Constants` → `Module.Payment.Persistence` | | |
| TASK-003 | Update all `using` directives in consumers: `PaymentMethodConfiguration.cs`, `PaymentRecordConfiguration.cs` | | |
| TASK-004 | Delete empty `Persistence/Constants/` directory | | |

### Implementation Phase 2: Create Services Directory Structure

- GOAL-002: Establish the new `Features/Admin/PaymentMethods/Services/` tree

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create empty directories: `Features/Admin/PaymentMethods/Services/Gateways/Bogus/` | | |
| TASK-006 | Create empty directories: `Features/Admin/PaymentMethods/Services/Gateways/Stripe/` | | |
| TASK-007 | Create empty directories: `Features/Admin/PaymentMethods/Services/Gateways/Webhooks/` | | |
| TASK-008 | Create empty directories: `Features/Admin/PaymentMethods/Services/Registry/` | | |

### Implementation Phase 3: Move Gateway Implementations

- GOAL-003: Relocate all `Infrastructure/` files into the Services tree

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Move `Infrastructure/Gateways/Bogus/BogusGateway.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Bogus/BogusGateway.cs` | | |
| TASK-010 | Move `Infrastructure/Gateways/Bogus/BogusGateway.Result.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Bogus/BogusGateway.Result.cs` | | |
| TASK-011 | Move `Infrastructure/Gateways/Bogus/BogusOptions.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Bogus/BogusOptions.cs` | | |
| TASK-012 | Update namespace for Bogus files: `Module.Payment.Infrastructure.Gateways.Bogus` → `Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Bogus` | | |
| TASK-013 | Move `Infrastructure/Gateways/Stripe/StripeGateway.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Stripe/StripeGateway.cs` | | |
| TASK-014 | Move `Infrastructure/Gateways/Stripe/StripeGateway.Result.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Stripe/StripeGateway.Result.cs` | | |
| TASK-015 | Move `Infrastructure/Gateways/Stripe/StripeOptions.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Stripe/StripeOptions.cs` | | |
| TASK-016 | Update namespace for Stripe files: `Module.Payment.Infrastructure.Gateways.Stripe` → `Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Stripe` | | |
| TASK-017 | Move `Infrastructure/Gateways/Stripe/IStripeWebhookService.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Webhooks/IStripeWebhookService.cs` | | |
| TASK-018 | Move `Infrastructure/Gateways/Stripe/StripeWebhookService.cs` → `Features/Admin/PaymentMethods/Services/Gateways/Webhooks/StripeWebhookService.cs` | | |
| TASK-019 | Update namespace for webhook files: `Module.Payment.Features.Storefront.Payment.Webhooks` → `Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Webhooks` | | |
| TASK-020 | Move `Infrastructure/GatewayProvidersOptions.cs` → `Features/Admin/PaymentMethods/Services/Registry/GatewayProvidersOptions.cs` | | |
| TASK-021 | Update namespace: `Module.Payment.Infrastructure` → `Module.Payment.Features.Admin.PaymentMethods.Services.Registry` | | |

### Implementation Phase 4: Fix All References

- GOAL-004: Update all `using` directives and DI registrations

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Update `Payment.Extension.cs` — change all `using Module.Payment.Infrastructure.*` to new Services namespaces | | |
| TASK-023 | Update `CreatePaymentIntent.cs` — change `using Module.Payment.Infrastructure.Gateways.Stripe` if present | | |
| TASK-024 | Update `ConfirmPayment.cs` — change gateway `using` directives | | |
| TASK-025 | Update `CreateSetupIntent.cs` — remove direct Stripe SDK `using` (already done) | | |
| TASK-026 | Update `StripeWebhook.cs` — update `using` for `StripeWebhookService` | | |
| TASK-027 | Update `StripeWebhook.Endpoint.cs` — update `using` if needed | | |
| TASK-028 | Update admin feature handlers (`CapturePayment.cs`, `VoidPayment.cs`, `RefundPayment.cs`) — update gateway `using` directives | | |
| TASK-029 | Update `PaymentCapture.Method.Processing.cs` — update if references `BogusGateway` or `StripeGateway` | | |
| TASK-030 | Delete the empty `Infrastructure/` directory | | |

### Implementation Phase 5: Tests

- GOAL-005: Update all test references

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | Update `BogusGatewayTests.cs` — change `using` to new namespace | | |
| TASK-032 | Update `StripeGatewayTests.cs` — change `using` to new namespace | | |
| TASK-033 | Update `StripeGatewayAuthorizeTests.cs` — change `using` to new namespace | | |
| TASK-034 | Update `CreatePaymentIntentTests.cs` — change `using` to new namespace | | |
| TASK-035 | Update `ConfirmPaymentTests.cs` — change `using` to new namespace | | |
| TASK-036 | Update `StripeWebhookTests.cs` — change `using` to new namespace | | |
| TASK-037 | Update all admin payment tests (`CapturePaymentTests.cs`, `VoidPaymentTests.cs`, `RefundPaymentTests.cs`) — change `using` | | |
| TASK-038 | Update `PaymentProcessingAsyncTests.cs` — change `using` | | |
| TASK-039 | Run `dotnet build` — verify 0 errors, 0 warnings | | |
| TASK-040 | Run `dotnet test service/Api/tests/Module.UnitTests` — verify no new failures | | |
| TASK-041 | Run `dotnet test service/Api/tests/Shared.UnitTests` — verify no new failures | | |
| TASK-042 | Add EF Core migration if schema namespace change affects snapshot: `dotnet ef migrations add RestructurePaymentModule` | | |

## 3. Alternatives

- **ALT-001**: Keep `Infrastructure/` but rename to match Catalog's `Services/` pattern. Rejected: Catalog has zero `Infrastructure/` directories; adding one would create an inconsistency. Gateway implementations are payment providers, not application infrastructure (which lives in `Shared`).
- **ALT-002**: Move gateway interfaces from `Domain/Gateways/` into the Services tree too. Rejected: `IPaymentGatewayActionProvider` is used by domain code (`PaymentCapture.Method.Processing.cs`). Domain must not depend on Features.

## 4. Dependencies

- **DEP-001**: `GatewayConstants` (in `Domain/Gateways/`) — no move needed. Domain abstractions stay.
- **DEP-002**: `IPaymentGatewayActionProvider` and `IGatewayRegistry` — no move needed. Domain stays.
- **DEP-003**: `AesEncryptionService` and `IEncryptionService` (in `Shared`) — no change.

## 5. Files

**Moved (existing → new path):**

| Old Path (under `Module/Payment/`) | New Path (under `Module/Payment/`) |
|-------------------------------------|-------------------------------------|
| `Persistence/Constants/PaymentSchema.cs` | `Persistence/PaymentSchema.cs` |
| `Infrastructure/GatewayProvidersOptions.cs` | `Features/Admin/PaymentMethods/Services/Registry/GatewayProvidersOptions.cs` |
| `Infrastructure/Gateways/Bogus/BogusGateway.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Bogus/BogusGateway.cs` |
| `Infrastructure/Gateways/Bogus/BogusGateway.Result.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Bogus/BogusGateway.Result.cs` |
| `Infrastructure/Gateways/Bogus/BogusOptions.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Bogus/BogusOptions.cs` |
| `Infrastructure/Gateways/Stripe/StripeGateway.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Stripe/StripeGateway.cs` |
| `Infrastructure/Gateways/Stripe/StripeGateway.Result.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Stripe/StripeGateway.Result.cs` |
| `Infrastructure/Gateways/Stripe/StripeOptions.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Stripe/StripeOptions.cs` |
| `Infrastructure/Gateways/Stripe/IStripeWebhookService.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Webhooks/IStripeWebhookService.cs` |
| `Infrastructure/Gateways/Stripe/StripeWebhookService.cs` | `Features/Admin/PaymentMethods/Services/Gateways/Webhooks/StripeWebhookService.cs` |

**Modified (using-directive updates):**

| File | Change |
|------|--------|
| `Payment.Extension.cs` | Update all `using Module.Payment.Infrastructure.*` |
| `Persistence/Configurations/PaymentMethods/PaymentMethodConfiguration.cs` | Update `using Module.Payment.Persistence.Constants` |
| `Persistence/Configurations/Payments/PaymentRecordConfiguration.cs` | Update `using Module.Payment.Persistence.Constants` |
| `Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` | Update gateway `using` |
| `Features/Storefront/Payment/Confirm/ConfirmPayment.cs` | Update gateway `using` |
| `Features/Storefront/Payment/SetupIntent/CreateSetupIntent.cs` | Update gateway `using` |
| `Features/Storefront/Payment/Webhooks/StripeWebhook.cs` | Update webhook service `using` |
| `Features/Admin/Payments/Capture/CapturePayment.cs` | Update gateway `using` |
| `Features/Admin/Payments/Void/VoidPayment.cs` | Update gateway `using` |
| `Features/Admin/Payments/Refund/RefundPayment.cs` | Update gateway `using` |
| `Domain/PaymentCaptures/PaymentCapture.Method.Processing.cs` | Update gateway `using` |

**Deleted:**

| Path | Reason |
|------|--------|
| `Persistence/Constants/` (directory) | Empty after schema move |
| `Infrastructure/` (directory) | All contents moved to Features tree |

## 6. Testing

- **TEST-001**: Build all projects: `dotnet build` — 0 errors, 0 warnings.
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — same pass rate as before (2297/2301).
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` — 2378/2378 pass (no impact).
- **TEST-004**: Verify no imports of old namespaces: `rg "using Module\.Payment\.Infrastructure"` returns zero results.
- **TEST-005**: Verify new namespaces exist: `rg "using Module\.Payment\.Features\.Admin\.PaymentMethods\.Services"` matches moved files.

## 7. Risks & Assumptions

- **RISK-001**: EF Core model snapshot may reference old namespace for `PaymentSchema` — verify migration tooling works after move.
- **RISK-002**: Integration tests (`Api.Tests`) may reference old `Infrastructure` paths — check and fix.
- **ASSUMPTION-001**: Catalog's pattern of no `Infrastructure/` directory applies cleanly to Payment. Gateways are "services" in Catalog's terms.
- **ASSUMPTION-002**: No external code (outside `service/Api/`) references `Module.Payment.Infrastructure` directly.

## 8. Related Specifications / Further Reading

- `service/Api/src/Module/Catalog/` — reference module structure
- `spec/design-payment-gateway-abstraction.md` — current Payment architecture spec
