---
goal: Eliminate all inline Error.Validation calls, hardcoded strings, and magic numbers in Ordering handlers/validators by extracting to predefined Result and Constant classes
version: 1.0
date_created: 2026-07-13
status: Planned
tags: refactor, ordering, constants, results, hardcoded-strings
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Replace ~35 inline string/number literals and 4 direct `Error.Validation()` factory calls across 12 Ordering handler/validator files with predefined constants from `OrderResult.Errors`, `LineItemResult.Errors`, `OrderConstant`, `LineItemConstant`, `AdjustmentConstant`, and two new constant classes. After this plan, zero `Error.Validation(...)` calls exist outside Result definitions, zero `"USD"`/`"System"` literals exist in handler code, and all domain discriminator strings are named constants.

## 1. Requirements & Constraints

- **REQ-001**: Every `Error.Validation(...)` call outside a Result definition file must be replaced with a `OrderResult.Errors.*` or `LineItemResult.Errors.*` constant
- **REQ-002**: Every `.WithMessage()` and `.WithErrorCode()` in validators must reference `{ResultClass}.{ErrorName}.Code` / `.Message` — never inline strings
- **REQ-003**: Every `"USD"` literal in handler/response/model code must use `OrderConstant.Defaults.Currency`
- **REQ-004**: Every `"System"` literal in factory/extensions must use `{Entity}Constant.Defaults.CreatedBy`
- **REQ-005**: Every `"paid"`, `"void"`, `"balance_due"`, `"credit_owed"` literal must use `OrderConstant.PaymentState.*`
- **REQ-006**: Every `"Shipping"`, `"Order"` discriminator must use `AdjustmentConstant.SourceTypes.*` / `AdjustmentConstant.AdjustableTypes.*`
- **REQ-007**: Every checkout step string must use `OrderConstant.CheckoutStep.*`
- **REQ-008**: Every shipment state string must use `OrderConstant.ShipmentState.*`
- **REQ-009**: Magic numbers `30` (reservation days, TTL minutes) must be named constants
- **REQ-010**: Cancel reason strings must be named constants
- **CON-001**: Result error definitions live in the entity's `Domain/` Result file (e.g. `OrderResult.Errors.NotDraft` in `Order.Result.cs`)
- **CON-002**: Constant classes live in the entity's `Domain/` Constant file (e.g. `AdjustmentConstant.SourceTypes.Shipping` in `Adjustment.Constant.cs`)
- **CON-003**: New constant classes use `static class` with nested `public static class` groups — same pattern as existing `OrderConstant`
- **CON-004**: Do not change handler business logic — only string literal extraction
- **PAT-001**: Error codes follow `{Entity}.{Category}.{Specific}` — e.g. `Order.Update.NotDraft`
- **PAT-002**: Validator `.WithErrorCode(Xxx.Code)` / `.WithMessage(Xxx.Message)` — already established by `UpdateOrderAdmin.Validator.cs:15-16`

## 2. Implementation Steps

### Implementation Phase 1 — Add New Error Definitions to Result Classes

- GOAL-001: Add 8 new error defs to `OrderResult.Errors` and 1 to `LineItemResult.Errors`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Order.Result.cs`: add `OrderResult.Errors.NotDraft` → `Error.Validation("Order.Update.NotDraft", "Only draft orders can be modified.")` in `#region State` | | |
| TASK-002 | `Order.Result.cs`: add `OrderResult.Errors.NotDraftForBillAddress` → `Error.Validation("Order.BillAddress.Update.NotDraft", "Only draft orders can have billing address modified.")` | | |
| TASK-003 | `Order.Result.cs`: add `OrderResult.Errors.NotDraftForShipAddress` → `Error.Validation("Order.ShipAddress.Update.NotDraft", "Only draft orders can have shipping address modified.")` | | |
| TASK-004 | `Order.Result.cs`: add `OrderResult.Errors.NotDraftForLineItem` → `Error.Validation("Order.LineItem.Update.NotDraft", "Only draft orders can have line items modified.")` | | |
| TASK-005 | `Order.Result.cs`: add `OrderResult.Errors.EmailInvalid` → `Error.Validation("Order.Email.Invalid", "Email address is not valid.")` in `#region Validation` | | |
| TASK-006 | `Order.Result.cs`: add `OrderResult.Errors.CurrencyInvalid` → `Error.Validation("Order.Currency.Invalid", "Currency must be a valid ISO code.")` | | |
| TASK-007 | `Order.Result.cs`: add `OrderResult.Errors.GuestIdRequired` → `Error.Validation("Order.GuestId.Required", "Guest order ID is required.")` | | |
| TASK-008 | `LineItem.Result.cs`: add `LineItemResult.Errors.IdRequired` → `Error.Validation("LineItem.Id.Required", "Line item ID is required.")` in `#region Validation` | | |
| TASK-009 | `dotnet build` — verify 0 warnings, 0 errors | | |

### Implementation Phase 2 — Replace Inline Error.Validation() in Handlers

- GOAL-002: 4 handlers currently call `Error.Validation(...)` directly instead of using OrderResult.Errors.*

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `UpdateOrderAdmin.cs:27`: replace `return Error.Validation("Order.Update.NotDraft", "Only draft orders can be modified.")` → `return OrderResult.Errors.NotDraft` | | |
| TASK-021 | `UpdateOrderBillAddress.cs:27`: replace inline Error.Validation → `OrderResult.Errors.NotDraftForBillAddress` | | |
| TASK-022 | `UpdateOrderShipAddress.cs:27`: replace inline Error.Validation → `OrderResult.Errors.NotDraftForShipAddress` | | |
| TASK-023 | `UpdateOrderLineItem.cs:26`: replace inline Error.Validation → `OrderResult.Errors.NotDraftForLineItem` | | |
| TASK-024 | `dotnet build` — verify | | |

### Implementation Phase 3 — Fix Validator Inline Messages

- GOAL-003: 5 validators reference inline strings instead of OrderResult.Errors.* constants

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | `SelectShippingRate.Validator.cs:17-18`: replace `.WithErrorCode("ShippingRate.Selection.MethodRequired")` → `.WithErrorCode(OrderResult.Errors.DeliveryMethodRequired.Code)` and `.WithMessage("Shipping method is required.")` → `.WithMessage(OrderResult.Errors.DeliveryMethodRequired.Message)` | | |
| TASK-031 | `AssociateCartWithUser.Validator.cs:14`: replace `.WithMessage("Guest order ID is required.")` → `.WithMessage(OrderResult.Errors.GuestIdRequired.Message)` and add `.WithErrorCode(OrderResult.Errors.GuestIdRequired.Code)` | | |
| TASK-032 | `GetOrderLineItemById.Validator.cs:20`: replace `.WithMessage("Line item ID is required.")` → `.WithMessage(LineItemResult.Errors.IdRequired.Message)` and add `.WithErrorCode(LineItemResult.Errors.IdRequired.Code)` | | |
| TASK-033 | `UpdateOrderAdmin.Validator.cs:22-23`: replace `.WithErrorCode("Order.Email.Invalid")` → `.WithErrorCode(OrderResult.Errors.EmailInvalid.Code)` and `.WithMessage("Email address is not valid.")` → `.WithMessage(OrderResult.Errors.EmailInvalid.Message)` | | |
| TASK-034 | `Order.Validator.cs:17-18`: replace `.WithErrorCode("Order.Currency.Invalid")` → `.WithErrorCode(OrderResult.Errors.CurrencyInvalid.Code)` and inline message → `.WithMessage(OrderResult.Errors.CurrencyInvalid.Message)` | | |
| TASK-035 | `Order.Validator.cs:24-25`: replace `.WithErrorCode("Order.Email.Invalid")` → `.WithErrorCode(OrderResult.Errors.EmailInvalid.Code)` and inline message → `.WithMessage(OrderResult.Errors.EmailInvalid.Message)` | | |
| TASK-036 | `dotnet build` — verify | | |

### Implementation Phase 4 — Replace Hardcoded "USD" with OrderConstant.Defaults.Currency

- GOAL-004: 6 `"USD"` literals replaced with `OrderConstant.Defaults.Currency`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | `Cart.Model.Response.cs:34`: `public string Currency { get; init; } = OrderConstant.Defaults.Currency;` | | |
| TASK-041 | `GetCart.Response.cs:11`: `public string Currency { get; init; } = OrderConstant.Defaults.Currency;` | | |
| TASK-042 | `GetCart.cs:46`: `Currency = OrderConstant.Defaults.Currency,` | | |
| TASK-043 | `CreateCart.cs:35`: `OrderExtensions.Create(OrderConstant.Defaults.Currency, ...)` | | |
| TASK-044 | `AddToCart.cs:59`: `var currency = configuration["Ordering:DefaultCurrency"] ?? OrderConstant.Defaults.Currency;` | | |
| TASK-045 | `LineItem.cs:18`: `public string Currency { get; set; } = OrderConstant.Defaults.Currency;` | | |
| TASK-046 | `dotnet build` — verify | | |

### Implementation Phase 5 — Replace Hardcoded "System" with CreatedBy Constants

- GOAL-005: 3 `"System"` literals replaced; add `CreatedBy` defaults to LineItemConstant and AdjustmentConstant

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-050 | `LineItem.Constant.cs`: add `public static class Defaults { public const string CreatedBy = "System"; }` | | |
| TASK-051 | `Adjustment.Constant.cs`: add to existing `Defaults` class: `public const string CreatedBy = "System";` | | |
| TASK-052 | `Order.Extensions.cs:41`: replace `CreatedBy = "System"` → `CreatedBy = OrderConstant.Defaults.CreatedBy` | | |
| TASK-053 | `LineItem.Method.Factory.cs:32`: replace `CreatedBy = "System"` → `CreatedBy = LineItemConstant.Defaults.CreatedBy` | | |
| TASK-054 | `Adjustment.Method.Factory.cs:39`: replace `CreatedBy = "System"` → `CreatedBy = AdjustmentConstant.Defaults.CreatedBy` | | |
| TASK-055 | `dotnet build` — verify | | |

### Implementation Phase 6 — Replace Hardcoded Payment State Strings

- GOAL-006: Replace `"void"`, `"balance_due"`, `"credit_owed"`, `"paid"` with `OrderConstant.PaymentState.*` across 3 files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-060 | `Order.Extensions.cs:334`: `Order.PaymentState = OrderConstant.PaymentState.Void;` | | |
| TASK-061 | `Order.Extensions.cs:336`: `Order.PaymentState = OrderConstant.PaymentState.BalanceDue;` | | |
| TASK-062 | `Order.Extensions.cs:338`: `Order.PaymentState = OrderConstant.PaymentState.CreditOwed;` | | |
| TASK-063 | `Order.Extensions.cs:340`: `Order.PaymentState = OrderConstant.PaymentState.Paid;` | | |
| TASK-064 | `OrderUpdater.cs:120`: `Order.PaymentState = OrderConstant.PaymentState.Void;` | | |
| TASK-065 | `OrderUpdater.cs:124`: `Order.PaymentState = OrderConstant.PaymentState.BalanceDue;` | | |
| TASK-066 | `OrderUpdater.cs:128`: `Order.PaymentState = OrderConstant.PaymentState.CreditOwed;` | | |
| TASK-067 | `OrderUpdater.cs:132`: `Order.PaymentState = OrderConstant.PaymentState.Paid;` | | |
| TASK-068 | `CreateOrderFromCart.cs:88`: `cart.PaymentState = OrderConstant.PaymentState.Paid;` | | |
| TASK-069 | `dotnet build` — verify | | |

### Implementation Phase 7 — Add AdjustmentConstant Discriminator Constants + Replace Inline Strings

- GOAL-007: Add `SourceTypes` and `AdjustableTypes` and `Labels` to `Adjustment.Constant.cs`; replace `"Shipping"`/`"Order"` literals in 3 files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-070 | `Adjustment.Constant.cs`: add `public static class SourceTypes { public const string Shipping = "Shipping"; public const string Order = "Order"; }` | | |
| TASK-071 | `Adjustment.Constant.cs`: add `public static class AdjustableTypes { public const string Order = "Order"; public const string LineItem = "LineItem"; public const string Shipment = "Shipment"; }` | | |
| TASK-072 | `Adjustment.Constant.cs`: add `public static class Labels { public const string Shipping = "Shipping"; }` | | |
| TASK-073 | `SelectShippingRate.cs:67`: replace `a.SourceType == "Shipping"` → `a.SourceType == AdjustmentConstant.SourceTypes.Shipping` | | |
| TASK-074 | `SelectShippingRate.cs:79`: replace `label: "Shipping"` → `label: AdjustmentConstant.Labels.Shipping` | | |
| TASK-075 | `SelectShippingRate.cs:82`: replace `adjustableType: "Order"` → `adjustableType: AdjustmentConstant.AdjustableTypes.Order` | | |
| TASK-076 | `SelectShippingRate.cs:84`: replace `sourceType: "Shipping"` → `sourceType: AdjustmentConstant.SourceTypes.Shipping` | | |
| TASK-077 | `UpdateCheckout.cs:74`: replace `a.SourceType == "Shipping"` → `a.SourceType == AdjustmentConstant.SourceTypes.Shipping` | | |
| TASK-078 | `UpdateCheckout.cs:86`: replace `label: "Shipping"` → `label: AdjustmentConstant.Labels.Shipping` | | |
| TASK-079 | `UpdateCheckout.cs:89`: replace `adjustableType: "Order"` → `adjustableType: AdjustmentConstant.AdjustableTypes.Order` | | |
| TASK-080 | `UpdateCheckout.cs:91`: replace `sourceType: "Shipping"` → `sourceType: AdjustmentConstant.SourceTypes.Shipping` | | |
| TASK-081 | `Order.Extensions.cs:220`: replace `a.SourceType == "Shipping"` → `a.SourceType == AdjustmentConstant.SourceTypes.Shipping` | | |
| TASK-082 | `dotnet build` — verify | | |

### Implementation Phase 8 — Replace Hardcoded Checkout Step and Shipment State Strings

- GOAL-008: Replace all `"address"`, `"delivery"`, `"payment"`, `"confirm"`, `"complete"` and shipment state strings in `Order.Checkout.cs` with `OrderConstant.CheckoutStep.*` / `OrderConstant.ShipmentState.*`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-090 | `Order.Checkout.cs:9`: replace array literals with `OrderConstant.CheckoutStep.Address` / `Delivery` / `Payment` / `Confirm` / `Complete` | | |
| TASK-091 | `Order.Checkout.cs:16`: replace `steps.Add("delivery")` → `steps.Add(OrderConstant.CheckoutStep.Delivery)` | | |
| TASK-092 | `Order.Checkout.cs:17`: replace `steps.Add("payment")` → `steps.Add(OrderConstant.CheckoutStep.Payment)` | | |
| TASK-093 | `Order.Checkout.cs:18`: replace `steps.Add("confirm")` → `steps.Add(OrderConstant.CheckoutStep.Confirm)` and `steps.Add("complete")` → `steps.Add(OrderConstant.CheckoutStep.Complete)` | | |
| TASK-094 | `Order.Checkout.cs:30`: replace `"address"` with `OrderConstant.CheckoutStep.Address` | | |
| TASK-095 | `Order.Checkout.cs:37`: replace `s != "complete"` → `s != OrderConstant.CheckoutStep.Complete` | | |
| TASK-096 | `Order.Checkout.cs:86`: replace `"ready" or "backorder" or "pending" or "canceled"` with `OrderConstant.ShipmentState.Ready or OrderConstant.ShipmentState.Backorder or OrderConstant.ShipmentState.Pending or OrderConstant.ShipmentState.Canceled` | | |
| TASK-097 | `dotnet build` — verify | | |

### Implementation Phase 9 — Add Constants for Magic Numbers and Cancel Reasons

- GOAL-009: Name magic numbers `30` and hardcoded cancel reason strings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-100 | `CreateOrderFromCart.cs:140`: extract `30` → `const int StockReservationExpiryDays = 30;` (inline local const in handler, or add to a `StockReservationConstant` class) | | |
| TASK-101 | `AddToCart.cs:96`: extract `TtlMinutes = 30` → `const int CartReservationTtlMinutes = 30;` (local const) | | |
| TASK-102 | `Order.Constant.cs`: add `public static class CancelReasons { public const string Customer = "Order cancelled by customer"; public const string Admin = "Order cancelled by admin"; }` | | |
| TASK-103 | `CancelOrder.cs:60`: replace `"Order cancelled by customer"` → `OrderConstant.CancelReasons.Customer` | | |
| TASK-104 | `CancelOrderAdmin.cs:47`: replace `"Order cancelled by admin"` → `OrderConstant.CancelReasons.Admin` | | |
| TASK-105 | `CreateOrderFromCart.cs:150`: replace `action: "ship"` → `action: OrderConstant.StockAction.Ship` | | |
| TASK-106 | `dotnet build` — verify | | |

### Implementation Phase 10 — Final Verification

- GOAL-010: Full build + grep sweep to confirm zero remaining inline violations

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-110 | `dotnet build` — 0 warnings, 0 errors | | |
| TASK-111 | `grep -rn 'Error\.(NotFound\|Validation\|Conflict\|BadRequest\|Unauthorized)' service/Api/src/Module/Ordering/ --include='*.cs' | grep -v 'Result.cs'` — confirm zero results outside Result definition files | | |
| TASK-112 | `grep -rn '"USD"' service/Api/src/Module/Ordering/ --include='*.cs'` — confirm zero (excluding test files) | | |
| TASK-113 | `grep -rn '"System"' service/Api/src/Module/Ordering/Domain/ --include='*.cs'` — confirm zero (the constant defs are fine) | | |
| TASK-114 | `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-build` — verify existing tests still pass | | |

## 3. Alternatives

- **ALT-001**: Leave inline strings as-is. Rejected — they drift independently from Result definitions and validators lose the single-source-of-truth that `.Code`/`.Message` refs provide.
- **ALT-002**: Use `nameof()` or `[CallerArgumentExpression]` for discriminator strings. Rejected — these are serialized to the DB and must be exact strings; constants are the right abstraction.
- **ALT-003**: Extract all constants to a shared `OrderingConstants` monolith class. Rejected — existing pattern scopes constants per entity (`OrderConstant`, `LineItemConstant`, `AdjustmentConstant`), keep it.

## 4. Dependencies

- **DEP-001**: Phase 1 must complete before Phase 2 (handlers) and Phase 3 (validators) since they reference the new error definitions
- **DEP-002**: Phase 7 (AdjustmentConstant) blocks the discriminator replacements in SelectShippingRate, UpdateCheckout, Order.Extensions
- **DEP-003**: Phase 5 (CreatedBy constants) blocks the `"System"` replacements
- **DEP-004**: Phases 4–9 are independent of each other and can run in parallel after Phase 1

## 5. Files

- **FILE-001** — `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` — add 7 new error defs
- **FILE-002** — `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Result.cs` — add 1 new error def
- **FILE-003** — `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.cs` — replace inline Error.Validation
- **FILE-004** — `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateBillAddress/UpdateOrderBillAddress.cs` — replace inline Error.Validation
- **FILE-005** — `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipAddress/UpdateOrderShipAddress.cs` — replace inline Error.Validation
- **FILE-006** — `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs` — replace inline Error.Validation
- **FILE-007** — `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.Validator.cs` — use constant refs
- **FILE-008** — `service/Api/src/Module/Ordering/Features/Admin/Orders/SelectShippingRate/SelectShippingRate.Validator.cs` — use constant refs
- **FILE-009** — `service/Api/src/Module/Ordering/Features/Admin/Orders/Get/LineItemById/GetOrderLineItemById.Validator.cs` — use constant refs
- **FILE-010** — `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` — replace cancel reason
- **FILE-011** — `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` — replace cancel reason
- **FILE-012** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.Validator.cs` — use constant refs
- **FILE-013** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.cs` — replace "USD"
- **FILE-014** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.Response.cs` — replace "USD"
- **FILE-015** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — replace "USD" + magic number
- **FILE-016** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.cs` — replace "USD"
- **FILE-017** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — replace "paid", "ship", magic number
- **FILE-018** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs` — replace discriminators
- **FILE-019** — `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` — replace discriminators
- **FILE-020** — `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.cs` — replace "USD"
- **FILE-021** — `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Constant.cs` — add CreatedBy default
- **FILE-022** — `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Factory.cs` — replace "System"
- **FILE-023** — `service/Api/src/Module/Ordering/Domain/Adjustments/Adjustment.Constant.cs` — add CreatedBy, SourceTypes, AdjustableTypes, Labels
- **FILE-024** — `service/Api/src/Module/Ordering/Domain/Adjustments/Adjustment.Method.Factory.cs` — replace "System"
- **FILE-025** — `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs` — add CancelReasons
- **FILE-026** — `service/Api/src/Module/Ordering/Domain/Orders/Order.Extensions.cs` — replace "System", payment states, discriminator
- **FILE-027** — `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs` — replace checkout step + shipment state strings
- **FILE-028** — `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderUpdater.cs` — replace payment states
- **FILE-029** — `service/Api/src/Module/Ordering/Features/Shared/DependencyInjection/Models/Cart.Model.Response.cs` — replace "USD"
- **FILE-030** — `service/Api/src/Module/Ordering/Features/Shared/Validators/Order.Validator.cs` — use constant refs

## 6. Testing

- **TEST-001**: `dotnet build` after each phase — warnings-as-errors catches any undefined symbol or type mismatch
- **TEST-002**: `grep -rn 'Error\.(NotFound\|Validation\|Conflict\|BadRequest\|Unauthorized)' ... | grep -v Result.cs` — verify zero inline Error factory calls outside Result files
- **TEST-003**: `grep -rn '"USD"' service/Api/src/Module/Ordering/ --include='*.cs'` — verify zero USD literals (excluding test files)
- **TEST-004**: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — verify all existing tests pass

## 7. Risks & Assumptions

- **RISK-001**: String-matching greps may miss literals in interpolated strings or multi-line expressions. Mitigation: use `rg --multiline` for multi-line patterns.
- **RISK-002**: Replacing `"paid"` and similar short strings may inadvertently match unrelated identifiers (e.g. variable names). Mitigation: each replacement scoped to exact line context — inspect each diff.
- **ASSUMPTION-001**: `AdjustmentConstant.AdjustableTypes.Order`/`.LineItem`/`.Shipment` values match the strings used in `Adjustment.Result.cs` (`"Order"`, `"LineItem"`, `"Shipment"`). Confirm during TASK-071.
- **ASSUMPTION-002**: Magic numbers `30` in CreateOrderFromCart.cs:140 and AddToCart.cs:96 are unrelated — one is reservation expiry days, the other is TTL minutes. Extract as separate constants.

## 8. Related Specifications / Further Reading

- `plan/refactor-ordering-results-1.md` — prior plan that established OrderResult.Errors naming, regions, XML comments, and cross-module fix
- `docs/codebase/CONVENTIONS.md` — coding conventions for partial class structure and domain constants
