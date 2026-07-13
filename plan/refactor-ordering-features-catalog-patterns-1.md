---
goal: Refactor Ordering Features to Adopt Catalog Module Patterns
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
status: Planned
tags: refactor, ordering, features, patterns, service/Api
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Ordering module's 22 feature handlers deviate from the Catalog module's established patterns in model design, validation reuse, success messaging, file completeness, and boundary hygiene. This plan refactors all Ordering features to match the Catalog module's `abstract record`/`record` model hierarchy, 3-tier validation chain (feature → shared → domain), generic mapping extension methods, `Result<T>.Created/Ok(value, message)` success wrapping, and complete file counts per feature action directory.

## 1. Requirements & Constraints

### Model Requirements

- **REQ-M01**: Convert all `class` types in `Order.Model.*.cs` and `Cart.Model.*.cs` to `record` types. `OrderParameters` / `CartParameters` become `abstract record`.
- **REQ-M02**: Convert all `set` accessors on response timestamps (`CreatedAtUtc`, `ModifiedAtUtc`, etc.) to `init`.
- **REQ-M03**: Fix `CartDetailResponse` / `CartListItemResponse` to NOT inherit `CartParameters`. Create a new `CartResponseBase` abstract record with shared fields (`Id`, `ItemTotal`, `Total`, `Currency`, `ItemCount`, `CheckoutState`). `CartParameters` stays for request input only.
- **REQ-M04**: Every feature action directory must have files matching the Catalog pattern: `{Action}.cs`, `{Action}.Endpoint.cs`, `{Action}.Request.cs`, `{Action}.Response.cs`, `{Action}.Validator.cs`. Commands with no request body may omit `Request.cs` and `Validator.cs`. Commands with no response body may omit `Response.cs`.
- **REQ-M05**: All `Request` records must inherit from the Shared model (`OrderRequest`, `CartRequest`, or custom when the shared model is inapplicable). Response records inherit from the appropriate Shared response model (`OrderDetailResponse`, `OrderListItemResponse`, `CartDetailResponse`).

### Mapping Requirements

- **REQ-WM01**: Fix `Cart.Mapping.Model.cs` — implement `MapToDetail<T>()` to map all fields from the Order entity (including `Items`, `ItemTotal`, `Total`, `Currency`, `ItemCount`, `CheckoutState`).
- **REQ-WM02**: Add missing `MapToDomain` overloads to `Order.Mapping.Domain.cs` for update operations (accepts `T request, Order order` and calls `order.UpdateDetails(...)`).

### Validation Requirements

- **REQ-V01**: All feature validators must call their Shared validation extension (e.g., `ApplyOrderParametersRules()`) rather than duplicating rules inline.
- **REQ-V02**: All Shared validators must reference domain error codes (`OrderResult.Errors.*`) rather than vanilla FluentValidation defaults.
- **REQ-V03**: Add domain-level FluentValidation extension methods to `Order.Validation.cs` for all Order fields that currently lack them (`Email`, `Currency`, `BillAddressId`, `ShipAddressId`, `ShippingMethodId`, `SpecialInstructions`, `SessionId`).

### Handler Requirements

- **REQ-H01**: Every handler returning `Result<Response>` must wrap the value with `Result<Response>.Created(value, message)` or `Result<Response>.Ok(value, message)` using the appropriate `OrderResult.Success.*` message factory.
- **REQ-H02**: Every handler returning `Result` (no value) must use `Result.Ok(OrderResult.Success.*)`.
- **REQ-H03**: Every handler must call the appropriate `OrderLoggers` method after a successful state mutation.
- **REQ-H04**: Add missing files: `CreateCart.Request.cs`, `CreateCart.Response.cs`, `CreateCart.Validator.cs`, `UpdateOrderStatus.Response.cs`.

### Success Message Requirements

- **REQ-S01**: Add missing `OrderResult.Success` factories for operations that lack them: `CheckoutUpdated`, `ItemAdded`, `ItemRemoved`, `QuantityUpdated`, `ShippingRateSelected`, `CartCreated`, `StatusUpdated`, `ShippingMethodUpdated`.
- **REQ-S02**: Add missing `OrderResult.Errors` factories for validation cases: `SessionIdRequired`, `SessionIdTooLong`, `BillAddressIdRequired`, `ShipAddressIdRequired`, `ShippingMethodIdRequired`, `NotesTooLong`.

### Constraints

- **CON-001**: `dotnet build` must pass with `TreatWarningsAsErrors=true`.
- **CON-002**: No new cross-module references. Existing `AddToCart` cross-module imports stay (they're pre-existing and out of scope).
- **CON-003**: All new types use `record` (not `class`).
- **CON-004**: Follow the 3-tier validation pattern: Feature Validator → Shared extension → Domain extensions.

### Guidelines

- **GUD-001**: Shared model files follow `{Entity}.Model.{Purpose}.cs` naming (Parameters, Request, Response).
- **GUD-002**: Mapping files follow `{Entity}.Mapping.{Direction}.cs` (Domain for request→entity, Model for entity→response).
- **GUD-003**: Mappings use the generic pattern `where T : BaseType, new()` to work with any feature's Response type.

## 2. Implementation Steps

### Implementation Phase 1: Domain Layer — Error & Success Factories + Validation Extensions

**GOAL-PH1**: Add missing result factories and domain-level FluentValidation extension methods that all subsequent phases depend on.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH1-001 | Add missing `OrderResult.Success` factories: `CheckoutUpdated`, `ItemAdded`, `ItemRemoved`, `QuantityUpdated`, `ShippingRateSelected`, `CartCreated`, `StatusUpdated`, `ShippingMethodUpdated` | | |
| TASK-PH1-002 | Add missing `OrderResult.Errors` factories: `SessionIdRequired`, `SessionIdTooLong`, `BillAddressIdRequired`, `ShipAddressIdRequired`, `ShippingMethodIdRequired`, `NotesTooLong` | | |
| TASK-PH1-003 | Add domain-level FluentValidation extensions to `Order.Validation.cs`: `ApplyEmailRules`, `ApplyCurrencyRules`, `ApplyBillAddressIdRules`, `ApplyShipAddressIdRules`, `ApplyShippingMethodIdRules`, `ApplySpecialInstructionsRules`, `ApplySessionIdRules` | | |
| TASK-PH1-004 | Build and commit | | |

### Implementation Phase 2: Shared Models — Record Conversion + Inheritance Fixes

**GOAL-PH2**: Convert all Ordering shared models from `class` to `record`, fix `CartDetailResponse` inheritance, and add missing `init` consistency.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH2-001 | Convert `Order.Model.Parameters.cs`: `abstract class OrderParameters` → `abstract record OrderParameters` | | |
| TASK-PH2-002 | Convert `Order.Model.Request.cs`: `class OrderRequest` → `record OrderRequest : OrderParameters` | | |
| TASK-PH2-003 | Convert `Order.Model.Response.cs`: `class OrderDetailResponse` / `class OrderListItemResponse` → `record`. Fix `CreatedAtUtc`/`ModifiedAtUtc` from `set` to `init` | | |
| TASK-PH2-004 | Fix `Cart.Model.Parameters.cs`: keep `CartParameters` for request input only. Create new `CartResponseBase` abstract record with `Id`, `ItemTotal`, `Total`, `Currency`, `ItemCount`, `CheckoutState` | | |
| TASK-PH2-005 | Fix `Cart.Model.Response.cs`: change `CartDetailResponse` and `CartListItemResponse` to inherit from `CartResponseBase` instead of `CartParameters` | | |
| TASK-PH2-006 | Build and commit (verify no downstream breakage in handlers that use these models) | | |

### Implementation Phase 3: Shared Mappings — Fix + Complete

**GOAL-PH3**: Fix broken Cart mappings and add missing update-mapping overload for Orders.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH3-001 | Fix `Cart.Mapping.Model.cs`: implement full `MapToDetail<T>()` mapping all fields from Order entity (Items as `List<CartItem>`, ItemTotal, Total, Currency, ItemCount, CheckoutState) | | |
| TASK-PH3-002 | Add to `Order.Mapping.Domain.cs`: `MapToDomain<T>(this T request, Order order) where T : OrderRequest` overload that calls `order.UpdateDetails(...)` | | |
| TASK-PH3-003 | Build and commit | | |

### Implementation Phase 4: Shared Validators — Domain Error Codes + Reusability

**GOAL-PH4**: Ensure all shared validators reference domain error codes and all feature validators reuse shared extensions.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH4-001 | Update `Cart.Validator.cs`: use domain error codes (`OrderResult.Errors.*`) for `VariantId.NotEmpty()` and `Quantity.GreaterThan(0)` instead of vanilla FluentValidation defaults | | |
| TASK-PH4-002 | Update `UpdateOrderAdmin.Validator.cs`: replace inline `Email.EmailAddress()` rule with `ApplyOrderParametersRules()` call from shared | | |
| TASK-PH4-003 | Update `AddToCart.Validator.cs`: replace inline `VariantId`/`Quantity` rules with `ApplyCartParametersRules()` call from shared | | |
| TASK-PH4-004 | Add missing domain error references to `UpdateOrderStatus.Validator.cs` (use `OrderResult.Errors.IdRequired` instead of vanilla `.NotEmpty()` error) | | |
| TASK-PH4-005 | Build and commit | | |

### Implementation Phase 5: Handler Success Messages + Logging

**GOAL-PH5**: Add success message wrapping and logging to all handlers that currently lack them.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH5-001 | `CreateOrder.cs`: wrap return with `Result<Response>.Created(value, OrderResult.Success.Created(order.Id))`. Add `OrderLoggers.Created(...)` after save | | |
| TASK-PH5-002 | `CreateCart.cs`: wrap return with `Result<Response>.Created(value, OrderResult.Success.CartCreated(order.Id))`. Add `OrderLoggers.Created(...)` | | |
| TASK-PH5-003 | `UpdateOrderAdmin.cs`: wrap return with `Result<Response>.Ok(value, OrderResult.Success.Updated(order.Id))`. Add `OrderLoggers.Updated(...)` | | |
| TASK-PH5-004 | `EmptyCart.cs`: wrap `Result.Ok()` with `OrderResult.Success.Emptied(order.Id)` | | |
| TASK-PH5-005 | `DeleteCart.cs`: wrap `Result.Ok()` with `OrderResult.Success.Deleted(order.Id)` (after calling domain Delete) | | |
| TASK-PH5-006 | `RemoveCartItem.cs`: wrap return with appropriate success message | | |
| TASK-PH5-007 | `SelectShippingRate.cs`: wrap return with `OrderResult.Success.ShippingRateSelected(order.Id)` | | |
| TASK-PH5-008 | `UpdateCheckout.cs`: wrap return with `OrderResult.Success.CheckoutUpdated(order.Id)` | | |
| TASK-PH5-009 | `UpdateShippingMethod.cs`: wrap with `OrderResult.Success.ShippingMethodUpdated(order.Id)` | | |
| TASK-PH5-010 | `RemoveOrderLineItem.cs`: wrap with success message | | |
| TASK-PH5-011 | `AddOrderLineItem.cs`: wrap with `OrderResult.Success.ItemAdded(order.Id)` | | |
| TASK-PH5-012 | Build and commit all handlers | | |

### Implementation Phase 6: Missing Files — CreateCart + UpdateOrderStatus

**GOAL-PH6**: Add the missing Request/Response/Validator files to bring all feature directories up to 5-file standard.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH6-001 | Create `CreateCart.Request.cs`: `record Request` (empty — command has no body) | | |
| TASK-PH6-002 | Create `CreateCart.Response.cs`: `record Response : OrderDetailResponse` (extract the inline `class Response` from `CreateCart.cs`) | | |
| TASK-PH6-003 | Create `CreateCart.Validator.cs`: `sealed class Validator : AbstractValidator<Command>` (validates nothing — command has no body) | | |
| TASK-PH6-004 | Create `UpdateOrderStatus.Response.cs`: `record Response` with `Id`, `Status`, `UpdatedAt` fields | | |
| TASK-PH6-005 | Move inline `Response` class from `CreateCart.cs` to `CreateCart.Response.cs`. Update `CreateCart.cs` to reference the external file. | | |
| TASK-PH6-006 | Build and commit | | |

### Implementation Phase 7: Build + Full Verification

**GOAL-PH7**: Full build, all tests pass, validation checks.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-PH7-001 | `dotnet build` — must pass with 0 warnings | | |
| TASK-PH7-002 | `dotnet test service/Api/tests/Module.UnitTests --no-restore` — all tests pass | | |
| TASK-PH7-003 | Verify all models use `record` (not `class`): `rg "public (sealed )?(abstract )?class (Order|Cart)(Parameters|Request|Detail|List|Item)" service/Api/src/Module/Ordering/Features/` — zero results | | |
| TASK-PH7-004 | Verify all responses have `init` not `set` for timestamps | | |
| TASK-PH7-005 | Verify CartDetailResponse does NOT inherit CartParameters | | |
| TASK-PH7-006 | Verify all feature directories have 3-5 files each | | |
| TASK-PH7-007 | Verify CreateCart no longer imports Admin Shared (boundary fix) | | |

## 3. Alternatives

- **ALT-001**: Use Mapster for all entity↔DTO mappings instead of hand-written extension methods. Rejected: the codebase convention is hand-written generic mappings; switching libraries mid-stream would create inconsistency.
- **ALT-002**: Use a single `OrderDetailResponse` type instead of per-feature Response records. Rejected: per-feature Response types allow compile-time safety for feature-specific fields (e.g., `ApproveOrder.Response` has `ApprovedById` that `CreateOrder.Response` doesn't need).
- **ALT-003**: Move Storefront Cart models to a separate `Storefront/` shared directory instead of reusing Admin models. Accepted: Task PH2-004 already separates `CartParameters` (input) from `CartResponseBase` (output), fixing the semantic inheritance issue.

## 4. Dependencies

- **DEP-001**: Prior domain specs must be complete: convention migration, dead-code removal, Result pattern compliance, and handler domain-logic extraction.
- **DEP-002**: `Order.Validation.cs` must exist before Phase 1 (Task PH1-003 adds extensions to it).
- **DEP-003**: `Order.Result.cs` must exist before Phase 1 (Tasks PH1-001/002 add factories to it).
- **DEP-004**: Feature handlers must use domain methods (from prior spec) — handlers that still directly set properties will cause the mapping and success-message changes to behave differently.

## 5. Files

### Files Modified (38 total)

**Domain layer:**
- `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` — add 10+ success factories, 6+ error factories
- `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs` — add 7 validation extension methods

**Shared Models:**
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Parameters.cs` — class→record
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Request.cs` — class→record
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` — class→record, set→init
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Parameters.cs` — refactor, create CartResponseBase
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Request.cs` — class→record
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.cs` — fix inheritance, class→record

**Shared Mappings:**
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` — implement full mapping
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Domain.cs` — add update overload

**Shared Validators:**
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Validators/Cart.Validator.cs` — domain error codes
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.Validator.cs` — reuse shared
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Validator.cs` — reuse shared
- `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.Validator.cs` — domain error codes

**Handler Files (success messages + logging):**
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Create/CreateOrder.cs`
- `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/EmptyCart/EmptyCart.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCart.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs`
- `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShippingMethod/UpdateOrderShippingMethod.cs`
- `service/Api/src/Module/Ordering/Features/Admin/Orders/RemoveLineItem/RemoveOrderLineItem.cs`
- `service/Api/src/Module/Ordering/Features/Admin/Orders/AddLineItem/AddOrderLineItem.cs`

### Files Created (5 total)
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.Request.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.Response.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.Validator.cs`
- `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.Response.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.Base.cs` (CartResponseBase)

## 6. Testing

- **TEST-001**: Verify `Cart.Mapping.Model.cs` `MapToDetail` correctly maps Items, ItemTotal, Total, Currency, ItemCount, CheckoutState from an Order entity.
- **TEST-002**: Verify `Order.Mapping.Domain.cs` `MapToDomain(T, Order)` calls `order.UpdateDetails` with correct fields.
- **TEST-003**: Verify `CartDetailResponse` does not have `VariantId` or `Quantity` properties (no longer inherits CartParameters).
- **TEST-004**: Verify all feature handlers return `Result<T>.Created/Ok(value, message)` with the correct success string.
- **TEST-005**: Verify all feature handlers call the appropriate `OrderLoggers.*` method after state mutation.
- **TEST-006**: Verify all feature validators use the shared validation extensions (no duplicate rules).
- **TEST-007**: Verify `init` properties are read-only after construction (compile-time check — `record` enforces this).

## 7. Risks & Assumptions

- **RISK-001**: Changing `class` to `record` for shared models breaks code that uses `new()` constraints on non-generic types. Handlers that create response objects via `new T()` (generic mapping) must have `where T : BaseType, new()` constraints — records with `init`-only properties cannot be created with `new()`. **Mitigation**: the Catalog module's generic mappings use `where T : BaseType, new()` and work with records — `new()` works on `record` types, and `init` properties are set via object initializer syntax in the mapping method.
- **RISK-002**: Changing `CartDetailResponse` inheritance from `CartParameters` to `CartResponseBase` breaks any code that accessed `VariantId`/`Quantity`/`Notes` on a cart response. **Mitigation**: audit all usages of `.VariantId`/`.Quantity`/`.Notes` on response types — if found, add these fields to `CartResponseBase` as appropriate (e.g., `Quantity` maps to `ItemCount`).
- **ASSUMPTION-001**: All prior domain specs (convention migration, dead-code removal, Result compliance, handler extraction) are complete and the codebase is in a clean state.
- **ASSUMPTION-002**: `init` properties work with the `new T { ... }` pattern in generic mapping methods — verified by Catalog module's usage of `record` types with `new()` constraint.
- **ASSUMPTION-003**: The `AddToCart` handler's cross-module imports (`Module.Catalog.Domain.Products.Variants`, `Module.Inventory.Domain.*`) are pre-existing and out of scope for this refactor.

## 8. Related Specifications / Further Reading

- [spec-design-order-domain-concern-consolidation.md](../spec/spec-design-order-domain-concern-consolidation.md) — Dead-code removal and concern consolidation
- [spec-design-order-result-pattern-compliance.md](../spec/spec-design-order-result-pattern-compliance.md) — Result pattern compliance
- [spec-design-ordering-domain-logic-extraction.md](../spec/spec-design-ordering-domain-logic-extraction.md) — Handler domain logic extraction
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md) — Coding conventions
- `service/Api/src/Module/Catalog/Features/Admin/Products/Shared/` — Reference implementation for Catalog patterns
