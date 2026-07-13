---
goal: Add structured code comments to Ordering Storefront features following v3.0 Commenting Standard
version: 1.0
date_created: 2026-07-13
status: 'Planned'
tags: documentation, process, commenting, ordering-module
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Systematic application of the Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to all 49 `.cs` files across 15 feature slices in `service/Api/src/Module/Ordering/Features/Storefront/`. Covers Cart (13 sub-features) and Orders (3 sub-features). The standard defines 10 label categories (CAT-1 through CAT-10), temporal markers, doc-comment requirements, and anti-pattern rules.

## 1. Requirements & Constraints

- **REQ-001**: Every `.cs` file must carry appropriate comments per the Commenting Standard label decision tree
- **REQ-002**: Handler files must have `// Contract:` CAT-10 annotation at method entry (already present) plus operation-specific inline labels
- **REQ-003**: Inline labels must document non-obvious WHY: validation checks (CAT-1), object operations (CAT-2), business rules (CAT-4), external calls (CAT-8), logging (CAT-9), error recovery (CAT-7)
- **REQ-004**: Endpoint files must carry CAT-8 `Call:` labels on MediatR dispatch
- **REQ-005**: Validator files must carry `// Validate:` labels on each RuleFor block and `/// <summary>` doc comments on the validator class
- **REQ-006**: Shared mapping/model/validator files must carry doc comments on public types and methods, plus `Boundary:` annotations at layer edges
- **REQ-007**: Request/Response DTOs with non-obvious properties must carry `/// <summary>` doc comments
- **CON-001**: Must not add comments that restate what the code already expresses (AP-1 Redundancy)
- **CON-002**: Must not add comments on trivial lines (AP-3 Over-commenting)
- **CON-003**: All comment bodies must use imperative-mood verbs (F10, AP-8)
- **CON-004**: Max 100 characters per comment line (F3)
- **CON-005**: One label, one action — never join two with "and" (F8)
- **CON-006**: No trailing code-statement comments except inline data literals (F1)
- **GUD-001**: Follow the CommentingRules.xml label decision tree
- **PAT-001**: Vertical slice file pattern per feature: `{Name}.cs` (handler) + `{Name}.Endpoint.cs` + `{Name}.Validator.cs` + `{Name}.Request.cs` + `{Name}.Response.cs`

## 2. Implementation Steps

### Implementation Phase 1 — Handler Files: Add Operation-Specific Inline Labels

- GOAL-001: Add operation-specific inline labels (CAT-1 through CAT-9) to all 15 handler files to document non-obvious WHY behind validation, creation, update, mapping, external calls, compensation, and logging

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001.01 | `Cart/AddItem/AddToCart.cs` — Add `// Check:` on FindOrCreate cart logic; `// Validate:` on stock availability; `// Merge:` on existing line item quantity; `// Create:` on new line item; `// Log:` on LineItemLoggers.Created | | |
| TASK-001.02 | `Cart/AssociateCart/AssociateCartWithUser.cs` — Add `// Merge:` on OrderMerger.Merge; `// Remove:` on dbContext.Set.Order.Remove for guest cart deletion; `// Update:` on RecalculateTotals | | |
| TASK-001.03 | `Cart/Checkout/CreateOrderFromCart.cs` — Add `// Validate:` on each checkout prerequisite guard; `// Update:` on cart.Status/CheckoutState; `// Deduct:` on stock deduction loop; `// Create:` on StockReservation; `// Notify:` on SendOrderPlacedNotificationAsync; `// Log:` on OrderLoggers.Placed; `// Suppress:` on notification failure; `// Explain:` on Serializable transaction isolation | | |
| TASK-001.04 | `Cart/CreateCart/CreateCart.cs` — Add `// Check:` on existingCart lookup; `// Create:` on OrderExtensions.Create | | |
| TASK-001.05 | `Cart/DeleteCart/DeleteCart.cs` — Add `// Check:` on cart lookup; `// Update:` on soft-delete | | |
| TASK-001.06 | `Cart/EmptyCart/EmptyCart.cs` — Add `// Check:` on cart lookup; `// Update:` on cart.Empty + RecalculateTotals | | |
| TASK-001.07 | `Cart/Get/GetCart.cs` — Add `// Check:` on cart lookup; `// Map:` on Response construction with variant lookup | | |
| TASK-001.08 | `Cart/RemoveItem/RemoveCartItem.cs` — Add `// Check:` on cart lookup; `// Remove:` on Remove/Delete; `// Update:` on RecalculateTotals | | |
| TASK-001.09 | `Cart/SelectShippingRate/SelectShippingRate.cs` — Add `// Update:` on ShippingMethodId; `// Compute:` on order weight calculation; `// Compute:` on shipping cost via ShippingRateCalculator; `// Remove:` on clearing old shipping adjustments; `// Create:` on new Adjustment | | |
| TASK-001.10 | `Cart/UpdateCheckout/UpdateCheckout.cs` — Add `// Update:` on each field assignment; `// Compute:` on shipping recalculation; `// Remove:` on clearing old adjustments; `// Create:` on new Adjustment | | |
| TASK-001.11 | `Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` — Add `// Validate:` on stock availability; `// Update:` on quantity/total; `// Log:` on LineItemLoggers.QuantityUpdated | | |
| TASK-001.12 | `Cart/ValidateCheckout/ValidateCheckout.cs` — Add `// Validate:` on each checkout prerequisite check | | |
| TASK-001.13 | `Orders/Cancel/CancelOrder.cs` — Add `// Validate:` on AlreadyCanceled check; `// Call:` on payment void via ISender; `// Compensate:` on inventory release; `// Notify:` on SendOrderCanceledNotificationAsync; `// Log:` on OrderLoggers.Canceled; `// Suppress:` on notification failure | | |
| TASK-001.14 | `Orders/Get/ById/GetCustomerOrder.cs` — Add `// Check:` on lookup; `// Map:` on MapToDetail | | |
| TASK-001.15 | `Orders/ListOrders/ListCustomerOrders.cs` — Add `// Validate:` on ParseAll; `// Filter:` on Where clause excluding drafts | | |

### Implementation Phase 2 — Endpoint Files: Add CAT-8 Call Labels

- GOAL-002: Add `// Call:` inline labels on each MediatR `sender.Send` call in all 15 Endpoint files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002.01 | `Cart/AddItem/AddToCart.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.02 | `Cart/AssociateCart/AssociateCartWithUser.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.03 | `Cart/Checkout/CreateOrderFromCart.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.04 | `Cart/CreateCart/CreateCart.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.05 | `Cart/DeleteCart/DeleteCart.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.06 | `Cart/EmptyCart/EmptyCart.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.07 | `Cart/Get/GetCart.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.08 | `Cart/RemoveItem/RemoveCartItem.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.09 | `Cart/SelectShippingRate/SelectShippingRate.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.10 | `Cart/UpdateCheckout/UpdateCheckout.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.11 | `Cart/UpdateItemQuantity/UpdateCartItemQuantity.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.12 | `Cart/ValidateCheckout/ValidateCheckout.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.13 | `Orders/Cancel/CancelOrder.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.14 | `Orders/Get/ById/GetCustomerOrder.Endpoint.cs` — Add `// Call:` on sender.Send | | |
| TASK-002.15 | `Orders/ListOrders/ListCustomerOrders.Endpoint.cs` — Add `// Call:` on sender.Send | | |

### Implementation Phase 3 — Validator Files: Add CAT-1 Validate Labels & Doc Comments

- GOAL-003: Add `// Validate:` inline labels on each `RuleFor` block and `/// <summary>` doc comments on validator classes in all 9 validator files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003.01 | `Cart/AddItem/AddToCart.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on each RuleFor (Request.NotNull, VariantId.NotEmpty, Quantity.GreaterThan) | | |
| TASK-003.02 | `Cart/AssociateCart/AssociateCartWithUser.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on RuleFor GuestOrderId | | |
| TASK-003.03 | `Cart/Checkout/CreateOrderFromCart.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on RuleFor Request.NotNull | | |
| TASK-003.04 | `Cart/Get/GetCart.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on RuleFor x.NotNull | | |
| TASK-003.05 | `Cart/SelectShippingRate/SelectShippingRate.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on each RuleFor block | | |
| TASK-003.06 | `Cart/UpdateCheckout/UpdateCheckout.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on RuleFor Request.NotNull | | |
| TASK-003.07 | `Cart/Shared/Validators/Cart.Validator.cs` — Add `/// <summary>` on shared validator class and extension method; `// Validate:` on each RuleFor | | |
| TASK-003.08 | `Orders/Cancel/CancelOrder.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on RuleFor Id.NotEmpty | | |
| TASK-003.09 | `Orders/Get/ById/GetCustomerOrder.Validator.cs` — Add `/// <summary>` on Validator class; `// Validate:` on RuleFor Id.NotEmpty | | |

### Implementation Phase 4 — Shared Files: Add Doc Comments, Boundary & Inline Labels

- GOAL-004: Add XML doc comments on public types and methods, plus CAT-10 `Boundary:` annotations and inline labels, to shared mapping, model, and validator files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004.01 | `Cart/Shared/Mappings/Cart.Mapping.Domain.cs` — Add `// Boundary: Features → Domain` at class level | | |
| TASK-004.02 | `Cart/Shared/Mappings/Cart.Mapping.Model.cs` — Add `// Boundary:` on class; `// Map:` on MapToDetail method body | | |
| TASK-004.03 | `Cart/Shared/Models/Cart.Model.Parameters.cs` — Add `/// <summary>` on CartParameters class; doc comments on VariantId, Quantity, Notes | | |
| TASK-004.04 | `Cart/Shared/Models/Cart.Model.Request.cs` — Add `/// <summary>` on CartRequest class | | |
| TASK-004.05 | `Cart/Shared/Models/Cart.Model.Response.cs` — Add `/// <summary>` on CartItem, CartDetailResponse, CartListItemResponse; doc comments on non-obvious properties (ItemTotal, CheckoutState) | | |

### Implementation Phase 5 — Request/Response DTOs: Add Doc Comments on Non-Obvious Properties

- GOAL-005: Add `/// <summary>` doc comments on Request/Response DTO properties where the field purpose is not self-evident from the name alone

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005.01 | `Cart/AddItem/AddToCart.Request.cs` — Add `/// <summary>` on Quantity (default=1) | | |
| TASK-005.02 | `Cart/AddItem/AddToCart.Response.cs` — Add `/// <summary>` on LineItemId | | |
| TASK-005.03 | `Cart/UpdateItemQuantity/UpdateCartItemQuantity.Request.cs` — Add `/// <summary>` on Quantity | | |
| TASK-005.04 | `Orders/ListOrders/ListCustomerOrders.Response.cs` — Add `/// <summary>` on Status (string representation of OrderStatus enum) | | |

### Implementation Phase 6 — Verification

- GOAL-006: Verify all comments comply with the Commenting Standard formatting rules and anti-pattern checklist; ensure build passes with warnings-as-errors

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006.01 | Run `dotnet build` to verify zero warnings or errors | | |
| TASK-006.02 | Audit all added comments against anti-pattern checklist: AP-1 (redundancy), AP-2 (vagueness), AP-3 (over-commenting), AP-4 (inconsistent style), AP-8 (passive voice) | | |
| TASK-006.03 | Verify max 100 chars per comment line (F3), capitalised first word (F2), one space after delimiter (F4) | | |
| TASK-006.04 | Run `dotnet test service/Api/tests/Module.UnitTests --no-build` to confirm no regressions | | |

## 3. Alternatives

- **ALT-001**: Big-bang comment pass on all 49 files at once — rejected due to higher risk of introducing build warnings and difficulty reviewing
- **ALT-002**: Use automated comment-generation tool — rejected because machine-generated comments violate the Semantic Density Principle (P6) and reduce agent success rates per ETH AGENTbench findings

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` v3.0 — authoritative source for all label definitions and formatting rules
- **DEP-002**: .NET SDK 10 — `dotnet build` and `dotnet test` for verification
- **DEP-003**: No new NuGet packages required — comments are source-only changes

## 5. Files

- **FILE-001** to **FILE-049**: All 49 `.cs` files under `service/Api/src/Module/Ordering/Features/Storefront/` — 15 handler files (Phase 1), 15 endpoint files (Phase 2), 9 validator files (Phase 3), 5 shared files (Phase 4), 4 DTO files (Phase 5)
- **FILE-050**: No new files — all changes are in-place edits to existing files

## 6. Testing

- **TEST-001**: `dotnet build` — must pass with zero warnings (TreatWarningsAsErrors=true)
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests --no-build` — all existing tests must pass
- **TEST-003**: Manual audit of each commented file against the Commenting Standard anti-pattern checklist
- **TEST-004**: Line-length check — grep for comment lines exceeding 100 characters

## 7. Risks & Assumptions

- **RISK-001**: Adding comments may trigger CS1591 if `<GenerateDocumentationFile>` is enabled — mitigation: verify with `dotnet build` in Phase 6
- **RISK-002**: Stale comments if code is refactored between plan creation and execution — mitigation: execution must verify comments match current code
- **ASSUMPTION-001**: All 15 handler files currently have `///` doc comments and `// Contract:` labels — verified true from code audit
- **ASSUMPTION-002**: The build uses `TreatWarningsAsErrors=true` — verified true from `Directory.Build.props`
- **ASSUMPTION-003**: No existing comments violate the commenting standard in a blocking way

## 8. Related Specifications / Further Reading

- [`guide/code-commenting/CommentingRules.xml`](../guide/code-commenting/CommentingRules.xml) — authoritative spec
- [`guide/code-commenting/README.md`](../guide/code-commenting/README.md) — human-readable overview
- [`guide/code-commenting/references/anti-patterns.md`](../guide/code-commenting/references/anti-patterns.md) — anti-pattern definitions
- [`guide/code-commenting/references/label-quick-reference.md`](../guide/code-commenting/references/label-quick-reference.md) — quick label lookup
- [`plan/process-code-commenting-ordering-admin-orders-1.md`](./process-code-commenting-ordering-admin-orders-1.md) — previous admin Orders commenting plan (completed)
