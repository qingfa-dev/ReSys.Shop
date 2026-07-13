---
goal: Fix inline response construction, missing shared inheritance, and missing shared/domain validator delegation across all 8 business modules
version: 1.0
date_created: 2026-07-14
owner: Platform Team
status: Completed
tags: refactor, all-modules, responses, mappings, validators, catalog, identity, inventory, location, ordering, payment, profile, shipping
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Cross-module audit found 61 inline `new Response` constructions, 43 Response types not inheriting from shared models, and 71 validators not using `Apply*Rules()` delegation across all 8 business modules. This plan fixes violations grouped by module, prioritizing cases where shared models or domain extension methods already exist but aren't used.

## 1. Requirements & Constraints

- **REQ-001**: Every feature `Response` type must inherit from a shared model (`XxxDetailResponse`, `XxxListItemResponse`, or `StoreXxxResponse`)
- **REQ-002**: Every feature handler must use shared mapping methods (`MapToDetail<T>()`, `MapToListItem<T>()`) — never inline `new Response { ... }` when a mapping method exists
- **REQ-003**: Every feature validator must delegate field-level rules to domain `Apply*Rules()` or shared `Apply*ParametersRules()` — no inline duplicate rules when domain extensions exist
- **CON-001**: No behavioral change — response shapes and validation rules remain identical
- **CON-002**: `dotnet build` must pass with 0 warnings after each phase
- **CON-003**: Shared models may gain new properties but must not lose existing ones
- **CON-004**: Delete operations that return only `Id` are exempt from shared-model inheritance (intentional pattern)
- **CON-005**: List-wrapper Response types that wrap a collection of items are exempt from shared-model inheritance if no shared list model exists
- **PAT-001**: Feature Response: `public sealed record Response : SharedModel`
- **PAT-002**: Feature handler: `entity.MapToDetail<Response>()` or `entity.MapToListItem<Response>()`
- **PAT-003**: Feature validator: `RuleFor(x => x.Request).ApplyXxxParametersRules()` or `RuleFor(x => x.Property).ApplyXxxRules()`

## 2. Implementation Steps

### Implementation Phase 1 — Catalog Module (reference module, most violations)

- GOAL-001: Fix Catalog's 23 inline `new Response` violations and non-inheriting Response types where shared models/mappings exist

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Admin/Products/Classifications/Get/GetProductClassifications.cs`: replace inline `new Response { ... }` with shared mapping if `MapToDetail<T>()` or `MapToListItem<T>()` exists for Classification | | |
| TASK-002 | `Admin/Products/Variants/Images/Delete/DeleteVariantImage.Response.cs`: change `public sealed class Response` to `public sealed record Response(Guid Id)` — delete commands return ID only, acceptable but use `record` not `class` | | |
| TASK-003 | `Admin/Products/Variants/Images/ListByVariant/ListVariantImages.cs`: replace inline `new Response { ... }` with `image.MapToDetail<Response>()` if `MapToDetail<T>` exists for VariantImage | | |
| TASK-004 | `Admin/Products/Variants/List/ListVariantsByProduct.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-005 | `Admin/Products/Variants/Prices/Set/SetVariantPrice.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-006 | `Admin/Products/OptionTypes/Get/GetProductOptionTypes.cs`: replace inline `new OptionTypeItem { ... }` with `optionType.MapToListItem<T>()` if exists | | |
| TASK-007 | `Admin/Taxonomies/Taxons/GetAll/GetAllTaxons.cs`: replace inline Response construction with shared mapping | | |
| TASK-008 | `Admin/Taxonomies/Taxons/Reposition/RepositionTaxonUseCase.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-009 | `Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-010 | `Storefront/Products/Get/Similar/GetSimilarProducts.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-011 | `Storefront/Products/SearchByImage/SearchByImage.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-012 | `dotnet build` — verify | | |

### Implementation Phase 2 — Identity Module

- GOAL-002: Fix Identity's 18 inline `new Response` violations and non-inheriting Response types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `Admin/Users/Delete/DeleteUser.cs`: replace inline `new Response(...)` with shared mapping | | |
| TASK-021 | `Admin/Users/Permissions/Get/GetUserPermissions.cs`: replace inline `new Response { ... }` with shared mapping or inherit from shared model | | |
| TASK-022 | `Admin/Users/Roles/Get/GetUserRoles.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-023 | `Admin/Roles/Permissions/Get/GetRolePermissions.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-024 | `Store/Auth/Register/EmailRegister.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-025 | `Store/Auth/Sessions/Get/GetSession.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-026 | `Store/Auth/Login/External/Providers/ExternalProviders.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-027 | `dotnet build` — verify | | |

### Implementation Phase 3 — Inventory Module

- GOAL-003: Fix Inventory's 9 inline `new Response` violations and non-inheriting Response types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | `Admin/StockItems/Import/ImportStockItems.cs`: replace inline `new Response { ... }` with shared mapping or make Response inherit from shared model | | |
| TASK-031 | `Admin/StockItems/Summary/GetStockSummary.cs`: replace inline `new Response { ... }` with shared mapping — Response inherits from `VariantStockSummary`, ensure a `MapToSummary<T>()` mapping exists | | |
| TASK-032 | `Storefront/StockAvailability/Check/GetStockAvailability.cs`: replace inline `new Response { Id = ..., Name = ..., InStock = ... }` with shared mapping | | |
| TASK-033 | `Storefront/CartReservations/Release/ReleaseCartReservation.cs`: replace `public sealed class Response` with `public sealed record Response` and inherit from shared model if available | | |
| TASK-034 | `dotnet build` — verify | | |

### Implementation Phase 4 — Location Module

- GOAL-004: Fix Location's 8 validators that don't use `Apply*Rules()` — response inheritance is already clean

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Audit Location validators and add `Apply<Field>Rules()` calls where domain validation extensions exist (e.g., `ApplyNameRules`, `ApplyCodeRules`, `ApplyCountryIdRules`) | | |
| TASK-041 | `dotnet build` — verify | | |

### Implementation Phase 5 — Ordering Module

- GOAL-005: Fix Ordering's 3 inline `new Response` violations — response inheritance is already clean

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-050 | `Admin/Orders/Get/LineItems/GetOrderLineItems.cs`: replace `.Select(li => new Response { Id = li.Id, ... })` with `.Select(li => li.MapToLineItemResponse<Response>())` — add `using Module.Ordering.Features.Admin.Orders.Shared.Mappings;` | | |
| TASK-051 | `Storefront/Orders/ListOrders/ListCustomerOrders.cs`: replace `.Select(o => new Response { Id = o.Id, ... })` with `o.MapToStoreListItem<Response>()` — create `OrderStore.Mapping.cs` in `Storefront/Orders/Shared/Mappings/` with `MapToStoreListItem<T>` method | | |
| TASK-052 | `Storefront/Cart/Get/GetCart.cs`: replace `return new Response();` with `CartMapping.EmptyCart<Response>()` — add `EmptyCart<T>()` factory to `Cart.Mapping.Model.cs` | | |
| TASK-053 | `dotnet build` — verify | | |

### Implementation Phase 6 — Payment Module

- GOAL-006: Fix Payment's 2 inline `new Response` violations and 2 non-inheriting Response types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-060 | `PaymentMethodStore.Model.Response.cs`: add `Name` (string) and `Description` (string?) to `StorePaymentMethodListItemResponse` | | |
| TASK-061 | `ListPaymentMethods.Response.cs`: change to `public sealed record Response : StorePaymentMethodListItemResponse` — remove inline Id, Name, Description | | |
| TASK-062 | `ListPaymentMethods.cs`: replace `.Select(m => new Response { Id = m.Id, Name = m.Name, Description = m.Description })` with `.Select(m => m.MapToStoreListItem<Response>())` | | |
| TASK-063 | `CreateSetupIntent.Response.cs`: change `public class Response` to `public sealed record Response : StorePaymentDetailResponse` — remove inline ClientSecret | | |
| TASK-064 | `PaymentStore.Mapping.cs`: add `MapToStoreDetail<T>(this PaymentCapture entity)` mapping ClientSecret | | |
| TASK-065 | `CreateSetupIntent.cs`: replace `return new Response { ClientSecret = ... }` with `setupResult.Value.MapToStoreDetail<Response>()` | | |
| TASK-066 | `CapturePayment.Validator.cs`: replace inline `.GreaterThan(0)` with `.ApplyAmountRules()` | | |
| TASK-067 | `RefundPayment.Validator.cs`: replace inline `.GreaterThan(0)` with `.ApplyAmountRules()` | | |
| TASK-068 | `dotnet build` — verify | | |

### Implementation Phase 7 — Profile Module

- GOAL-007: Fix Profile's 3 inline `new Response` violations and 3 non-inheriting Response types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-070 | `Store/NotificationPreferences/Get/GetNotificationPreferences.cs`: replace inline `new Response { ... }` with shared mapping — make Response inherit from shared model if one exists | | |
| TASK-071 | `Store/NotificationPreferences/Update/UpdateNotificationPreferences.cs`: same pattern | | |
| TASK-072 | `Store/Addresses/Delete/DeleteAddress.cs`: change `public record Response(Guid Id, string Label)` — delete response, acceptable | | |
| TASK-073 | `dotnet build` — verify | | |

### Implementation Phase 8 — Shipping Module

- GOAL-008: Fix Shipping's 3 inline `new Response` violations and 3 non-inheriting Response types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-080 | `Storefront/Shipping/Methods/GetShippingMethods.cs`: replace inline `new Response { ... }` with shared mapping — make Response inherit from shared model | | |
| TASK-081 | `Storefront/Shipping/Calculate/CalculateShipping.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-082 | `Storefront/Shipping/Rates/ListShippingRates.cs`: replace inline `new Response { ... }` with shared mapping | | |
| TASK-083 | `dotnet build` — verify | | |

### Implementation Phase 9 — Validator Audit Pass (All Modules)

- GOAL-009: Fix validators that inline domain rules without calling `Apply*Rules()` — across all modules where domain extensions exist

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-090 | Inventory: replace inline `GreaterThan(0)` / `NotEmpty()` in validators with domain `ApplyAmountRules()`, `ApplyQuantityRules()` where they exist | | |
| TASK-091 | Location: replace inline rules with domain `ApplyNameRules()`, `ApplyCodeRules()`, `ApplyCountryIdRules()` where they exist | | |
| TASK-092 | Ordering: remaining simple validators (ApproveOrder, CompleteOrder, DeleteOrder, etc.) have `RuleFor(x => x.Id).NotEmpty()` — this is acceptable per Catalog pattern for ID-only commands | | |
| TASK-093 | Profile: replace inline rules with domain `Apply*Rules()` where they exist | | |
| TASK-094 | Shipping: replace inline rules with domain `Apply*Rules()` where they exist | | |
| TASK-095 | `dotnet build` — verify | | |

### Implementation Phase 10 — Build + Full Verification

- GOAL-010: Verify all modules compile, zero inline violations remain per automated checks

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-100 | `dotnet build` — 0 warnings, 0 errors across Api.csproj | | |
| TASK-101 | Verify zero `new Response` in handlers: `grep -rn 'new Response\b' service/Api/src/Module/{Catalog,Identity,Inventory,Location,Ordering,Payment,Profile,Shipping}/Features/ --include='*.cs' \| grep -v 'Response.cs'` — count should be near zero | | |
| TASK-102 | Verify zero `public class Response` across all modules: `grep -rn 'public class Response' service/Api/src/Module/ --include='*.cs'` — zero results | | |
| TASK-103 | `dotnet test service/Api/tests/Module.UnitTests --no-build` — all pass | | |
| TASK-104 | `dotnet test service/Api/tests/Shared.UnitTests --no-build` — all pass | | |

## 3. Alternatives

- **ALT-001**: Leave inline construction as-is for simple projections in EF `.Select()` queries. Rejected: inline projections duplicate mapping logic and are brittle on field additions. The Catalog pattern consistently uses `MapToDetail<T>()` / `MapToListItem<T>()`.
- **ALT-002**: Fix only Payment and Ordering (original scope). Rejected: cross-module consistency requires fixing all 8 modules to establish a uniform codebase convention.
- **ALT-003**: Fix all violations in a single pass. Rejected: per-module phases allow independent verification and reduce risk of cascading failures.

## 4. Dependencies

- **DEP-001**: Phases 1-8 are independent and can run in parallel (different modules)
- **DEP-002**: Phase 9 (validators) is independent from Phases 1-8
- **DEP-003**: Phase 10 (verification) must run last
- **DEP-004**: Each phase requires its module's shared models, mappings, and domain validation to exist first

## 5. Files

Across all 8 modules, ~61 handler files with inline `new Response`, ~43 Response.cs files with missing shared inheritance, and ~71 validator files needing `Apply*Rules()` migration.

### Catalog (~23 handler files, ~20 Response.cs files)
- **FILE-001 to FILE-023**: Handler files with inline `new Response`
- **FILE-024 to FILE-043**: Response.cs files needing inheritance

### Identity (~18 handler files, ~8 Response.cs files)
- **FILE-044 to FILE-061**: Handler + Response files

### Inventory (~9 handler files, ~4 Response.cs files)
- **FILE-062 to FILE-074**: Handler + Response files

### Location (~8 validator files)
- **FILE-075 to FILE-082**: Validator files

### Ordering (~3 handler files)
- **FILE-083**: `GetOrderLineItems.cs` — use `MapToLineItemResponse<T>()`
- **FILE-084**: `ListCustomerOrders.cs` — use new `MapToStoreListItem<T>()`
- **FILE-085**: `Cart.Mapping.Model.cs` — add `EmptyCart<T>()`
- **FILE-086**: `GetCart.cs` — use `CartMapping.EmptyCart<Response>()`

### Payment (~2 handler files, ~2 Response.cs files, ~2 validator files)
- **FILE-087**: `PaymentMethodStore.Model.Response.cs` — add Name, Description
- **FILE-088**: `ListPaymentMethods.Response.cs` — inherit from shared
- **FILE-089**: `ListPaymentMethods.cs` — use `MapToStoreListItem<T>()`
- **FILE-090**: `CreateSetupIntent.Response.cs` — record + inherit
- **FILE-091**: `PaymentStore.Mapping.cs` — add `MapToStoreDetail<T>()`
- **FILE-092**: `CreateSetupIntent.cs` — use mapping
- **FILE-093**: `CapturePayment.Validator.cs` — use `ApplyAmountRules()`
- **FILE-094**: `RefundPayment.Validator.cs` — use `ApplyAmountRules()`

### Profile (~3 handler files, ~3 Response.cs files)
- **FILE-095 to FILE-100**: Handler + Response files

### Shipping (~3 handler files, ~3 Response.cs files)
- **FILE-101 to FILE-106**: Handler + Response files

## 6. Testing

- **TEST-001**: `dotnet build` after each phase — warnings-as-errors catches undefined symbols
- **TEST-002**: Grep sweep for `new Response` in handler files — count should drop to near zero
- **TEST-003**: Grep sweep for `public class Response` — zero results
- **TEST-004**: `dotnet test` for Module.UnitTests and Shared.UnitTests — all pass

## 7. Risks & Assumptions

- **RISK-001**: Catalog has 23 inline `new Response` violations despite being the reference module — some may be intentional for EF projection efficiency (`.Select()` inside IQueryable). Mitigation: verify that mapping methods use expression-friendly code (no method calls inside EF `.Select()`). If a mapping method calls external methods (like `variantNameLookup[id]`), keep inline for those specific cases.
- **RISK-002**: 61 handler files across 8 modules is a large surface area. Mitigation: per-module phases allow independent verification. Each phase can be built and tested independently.
- **ASSUMPTION-001**: Delete operations returning only `Id` are intentionally exempt from shared-model inheritance (CON-004).
- **ASSUMPTION-002**: Some inline `new Response` in EF `.Select()` projections cannot use mapping methods because mapping methods contain code that EF cannot translate to SQL. These cases require keeping inline projections but can use anonymous types or explicit `new T {}`.

## 8. Related Specifications / Further Reading

- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs` — reference handler with shared mapping
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.Response.cs` — reference Response inheritance
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.Validator.cs` — reference validator delegation
