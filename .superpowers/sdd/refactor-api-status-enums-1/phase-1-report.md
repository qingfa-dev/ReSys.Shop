# Phase 1 Report — Billing payment DTO cluster → PaymentRecordState

Date: 2026-08-17
Branch: `feature/implement-storefront`
Status: DONE

## Goal
Convert the Billing payment DTO cluster to type `State` as `PaymentRecordState` and remove all `.ToString()` emission while keeping JSON string serialization via the existing `JsonStringEnumConverter`.

## Changes

### TASK-001 — DTO model
- `service/Api/src/Module/Billing/Features/Admin/Shared/Models/Payment.Model.cs`
  - Added `using Module.Billing.Domain.PaymentCaptures;`
  - `PaymentParameters.State`: `string` → `PaymentRecordState` (no default initializer; `PaymentStatus` stays `string?`).
  - All inheriting records (`PaymentRequest`, `PaymentDetailResponse`, `PaymentListItemResponse`, `StorePaymentRequest`, `StorePaymentDetailResponse`, `StorePaymentListItemResponse`, and every endpoint `Response` in `Admin/Payments/*` and `Storefront/Payment/*`) inherit the change with no edits.

### TASK-002 — Admin mappings
- `service/Api/src/Module/Billing/Features/Admin/Shared/Mappings/Payment.Mapping.cs`
  - `MapToDetail` and `MapToListItem`: `State = payment.State.ToString()` → `State = payment.State`.

### TASK-003 — Storefront mappings + handler
- `service/Api/src/Module/Billing/Features/Storefront/Shared/Mappings/Storefront.Payment.Mapping.cs`
  - `MapToStoreDetail` and `MapToStoreListItem`: `State = payment.State.ToString()` → `State = payment.State`.
- `service/Api/src/Module/Billing/Features/Storefront/Payment/Status/GetPaymentStatus.cs`
  - Response assignment: `State = payment.State.ToString()` → `State = payment.State`.

### Test updates (required by the type change — these classes exercise the touched handlers/mappings)
- `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/Status/GetPaymentStatusTests.cs`
  - `State.Should().Be("Completed")` → `Be(PaymentRecordState.Completed)`; `Be("Processing")` → `Be(PaymentRecordState.Processing)`.
- `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/Confirm/ConfirmPaymentTests.cs`
  - `State.Should().Be("Completed")` → `Be(PaymentRecordState.Completed)` (2 occurrences).
- `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`
  - `State.Should().Be(PaymentRecordState.Pending.ToString())` → `Be(PaymentRecordState.Pending)`.

Note: `Api.Tests/Scenarios/Billing/CreateIntent/CreateIntent.IntegrationTests.cs` deserializes into a local `CreateIntentResponse` record with `public string State` — the JSON wire format is unchanged (enum name serialized as string), so no edit was required there.

## Constraints honored
- CON-001: TreatWarningsAsErrors — build clean (0 warnings / 0 errors).
- CON-002: Vertical-slice file structure preserved; changes stay in feature files.
- CON-004: `Program.cs` and serializer config untouched; `JsonStringEnumConverter` still emits member names as JSON strings.
- PAT-001 / REQ-003: mappings assign `payment.State` directly (no `.ToString()`).
- REQ-002: JSON wire format unchanged — enum member names serialize to the same strings the old `.ToString()` produced.
- `PaymentRecordState` enum and `PaymentRecordConfiguration.cs` untouched.

## Verification
1. `dotnet build service/Api/src/Api/Api.csproj -v q --nologo` → Build succeeded, 0 Warnings, 0 Errors.
2. `dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo` → Build succeeded, 0 Warnings, 0 Errors.
3. Unit tests (xunit v3 MTP runner binary `./Module.UnitTests`):
   - Touched Billing test classes (`PaymentMappingTests`, `PaymentStoreMappingTests`, `GetPaymentStatusTests`, `ConfirmPaymentTests`, `CreatePaymentIntentTests`): 31 tests, 0 failed.
   - Full `Module=Payment` trait run: 276 tests, 0 failed, 0 skipped.
   - Full-suite run: 2775 total, 3 failed — all three are pre-existing failures in `Ordering/Persistence/OrderStatusValueConverterTests` (NullReferenceException), unrelated to this phase and untouched by it.

## Concerns
- None for this phase. The 3 failing Ordering converter tests are pre-existing and out of scope.