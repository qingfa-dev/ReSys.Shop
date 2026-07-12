---
goal: Add structured code comments to Ordering admin Orders features following v3.0 Commenting Standard
version: 1.0
date_created: 2026-07-13
status: 'Completed'
tags: documentation, process, commenting, ordering-module
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Systematic application of the Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to all 71 `.cs` files across 16 feature slices in `service/Api/src/Module/Ordering/Features/Admin/Orders/`. The standard defines 10 label categories (CAT-1 through CAT-10), temporal markers, doc-comment requirements, and anti-pattern rules. Each comment must explain WHY, never WHAT (P1), use imperative verbs (F10), and pass the semantic density test (P6).

## 1. Requirements & Constraints

- **REQ-001**: Every `.cs` file must carry appropriate comments per the Commenting Standard label decision tree
- **REQ-002**: Handler files must have `///` XML doc-comments on public Handle methods (`<summary>`, `<param>`, `<returns>`, `<exception>`) plus `// Contract:` CAT-10 annotation at method entry
- **REQ-003**: Inline labels must document non-obvious WHY: validation checks (CAT-1), object operations (CAT-2), business rules (CAT-4), mapping (CAT-8), logging (CAT-9)
- **REQ-004**: Endpoint files must carry CAT-8 `Call:` / `Send:` labels on MediatR dispatch
- **REQ-005**: Validator files must carry CAT-1 `Validate:` labels on each rule block
- **REQ-006**: Shared mapping/model/validator files must carry doc comments on public types and methods, plus `Boundary:` annotations at layer edges
- **CON-001**: Must not add comments that restate what the code already expresses (AP-1 Redundancy)
- **CON-002**: Must not add comments on trivial lines (AP-3 Over-commenting)
- **CON-003**: All comment bodies must use imperative-mood verbs (F10, AP-8)
- **CON-004**: Max 100 characters per comment line (F3)
- **CON-005**: One label, one action — never join two with "and" (F8)
- **CON-006**: No trailing code-statement comments except inline data literals (F1)
- **GUD-001**: Follow the CommentingRules.xml label decision tree: PUBLIC API → doc comments, time-sensitive → temporal marker, AI-agent → CAT-10, then CAT-1..9 by operation type
- **PAT-001**: Vertical slice file pattern per feature: `{Name}.cs` (handler) + `{Name}.Endpoint.cs` + `{Name}.Validator.cs` + `{Name}.Request.cs` + `{Name}.Response.cs`

## 2. Implementation Steps

### Implementation Phase 1 — Handler Files: Complete Contract & Inline Labels

- GOAL-001: Add missing `// Contract:` CAT-10 annotations and operation-level inline labels (CAT-1, CAT-2, CAT-3, CAT-4, CAT-5, CAT-8, CAT-9) to all 16 handler files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001.01 | `Get/ById/GetOrderById.cs` — Add `// Contract:` to QueryHandler.Handle; add `// Check:` on FirstOrDefaultAsync; add `// Map:` on MapToDetail call | | |
| TASK-001.02 | `Get/LineItemById/GetOrderLineItemById.cs` — Add `// Contract:` to QueryHandler.Handle; add `// Check:` on FirstOrDefaultAsync; add `// Map:` on Response construction | | |
| TASK-001.03 | `Get/LineItems/GetOrderLineItems.cs` — Add `// Contract:` to PagedQueryHandler.Handle; add `// Validate:` on ParseAll; add `// Filter:` on Where/ApplyQuerying | | |
| TASK-001.04 | `Get/Paged/GetPagedOrders.cs` — Add `// Contract:` to PagedQueryHandler.Handle; add `// Validate:` on ParseAll; add `// Filter:` on ApplyQuerying; add `// Map:` on MapToListItem | | |
| TASK-001.05 | `AddLineItem/AddOrderLineItem.cs` — Add `// Check:` on FirstOrDefaultAsync for order lookup; add `// Update:` on RecalculateTotals; add `// Map:` on Response construction | | |
| TASK-001.06 | `Approve/ApproveOrder.cs` — Add `// Check:` on FirstOrDefaultAsync; add `// Update:` on ApprovedAtUtc/ModifiedAtUtc; add `// Map:` on Response construction | | |
| TASK-001.07 | `Cancel/CancelOrderAdmin.cs` — Add `// Check:` on wasPlaced; add `// Call:` on void payment via ISender; add `// Compensate:` on inventory release; add `// Suppress:` on notification failure catch; add `// Log:` on warning logs | | |
| TASK-001.08 | `Complete/CompleteOrder.cs` — Add `// Enforce:` on status=Placed guard; add `// Update:` on CheckoutState timestamps; add `// Map:` on MapToDetail call | | |
| TASK-001.09 | `Create/CreateOrder.cs` — Add `// Create:` on MapToDomain call; add `// Map:` on MapToDetail call | | |
| TASK-001.10 | `Delete/DeleteOrder.cs` — Add `// Enforce:` on not-placed guard; add `// Update:` on IsDeleted/DeletedAtUtc | | |
| TASK-001.11 | `RemoveLineItem/RemoveOrderLineItem.cs` — Add `// Enforce:` on draft-status guard; add `// Remove:` on Remove call; add `// Update:` on RecalculateTotals | | |
| TASK-001.12 | `Resume/ResumeOrder.cs` — Add `// Check:` on FirstOrDefaultAsync; add `// Call:` on order.Resume(); add `// Notify:` on SendOrderResumedNotificationAsync; add `// Suppress:` on notification failure | | |
| TASK-001.13 | `Update/UpdateOrderAdmin.cs` — Add `// Enforce:` on draft-status; add `// Update:` on each field assignment; add `// Map:` on MapToDetail | | |
| TASK-001.14 | `UpdateBillAddress/UpdateOrderBillAddress.cs` — Add `// Enforce:` on draft-status; add `// Update:` on BillAddressId; add `// Map:` on MapToDetail | | |
| TASK-001.15 | `UpdateLineItem/UpdateOrderLineItem.cs` — Add `// Enforce:` on draft-status; add `// Update:` on UpdateQuantity/RecalculateTotals; add `// Map:` on Response construction | | |
| TASK-001.16 | `UpdateShipAddress/UpdateOrderShipAddress.cs` — Add `// Enforce:` on draft-status; add `// Update:` on ShipAddressId; add `// Map:` on MapToDetail | | |
| TASK-001.17 | `UpdateShippingMethod/UpdateOrderShippingMethod.cs` — Add `// Check:` on FirstOrDefaultAsync; add `// Update:` on ShippingMethodId/RecalculateTotals; add `// Compute:` on RecalculateTotals; add `// Map:` on MapToDetail | | |
| TASK-001.18 | `UpdateStatus/UpdateOrderStatus.cs` — Add `// Check:` on FirstOrDefaultAsync; add `// Update:` on status switch branches; add `// Compensate:` on inventory release for cancel; add `// Log:` on OrderLoggers.Canceled | | |

### Implementation Phase 2 — Endpoint Files: Add CAT-8 Call / CAT-5 Await Labels

- GOAL-002: Add operation-level inline labels to all 16 Endpoint files — label MediatR `Send` calls and route registration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002.01 | `AddLineItem/AddOrderLineItem.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.02 | `Approve/ApproveOrder.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.03 | `Cancel/CancelOrderAdmin.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.04 | `Complete/CompleteOrder.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.05 | `Create/CreateOrder.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.06 | `Delete/DeleteOrder.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.07 | `Get/ById/GetOrderById.Endpoint.cs` — Add `// Call:` on sender.Send(new Query(...)) | | |
| TASK-002.08 | `Get/LineItemById/GetOrderLineItemById.Endpoint.cs` — Add `// Call:` on sender.Send(new Query(...)) | | |
| TASK-002.09 | `Get/LineItems/GetOrderLineItems.Endpoint.cs` — Add `// Call:` on sender.Send(new Query(...)) | | |
| TASK-002.10 | `Get/Paged/GetPagedOrders.Endpoint.cs` — Add `// Call:` on sender.Send(new Query(...)) | | |
| TASK-002.11 | `RemoveLineItem/RemoveOrderLineItem.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.12 | `Resume/ResumeOrder.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.13 | `Update/UpdateOrderAdmin.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.14 | `UpdateBillAddress/UpdateOrderBillAddress.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.15 | `UpdateLineItem/UpdateOrderLineItem.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.16 | `UpdateShipAddress/UpdateOrderShipAddress.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.17 | `UpdateShippingMethod/UpdateOrderShippingMethod.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |
| TASK-002.18 | `UpdateStatus/UpdateOrderStatus.Endpoint.cs` — Add `// Call:` on sender.Send(new Command(...)) | | |

### Implementation Phase 3 — Validator Files: Add CAT-1 Validate Labels

- GOAL-003: Add `// Validate:` inline labels on each FluentValidation `RuleFor` block in all 8 validator files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003.01 | `Approve/ApproveOrder.Validator.cs` — Add `// Validate:` on RuleFor(x => x.Id).NotEmpty() | | |
| TASK-003.02 | `Create/CreateOrder.Validator.cs` — Add `// Validate:` on RuleFor(x => x.Request).ApplyOrderParametersRules() | | |
| TASK-003.03 | `Get/ById/GetOrderById.Validator.cs` — Add `// Validate:` on RuleFor(x => x.Id).NotEmpty() | | |
| TASK-003.04 | `Get/LineItemById/GetOrderLineItemById.Validator.cs` — Add `// Validate:` labels on both RuleFor blocks | | |
| TASK-003.05 | `Resume/ResumeOrder.Validator.cs` — Add `// Validate:` on RuleFor(x => x.Id).NotEmpty() | | |
| TASK-003.06 | `Update/UpdateOrderAdmin.Validator.cs` — Add `// Validate:` on each RuleFor block (Id, Email) | | |
| TASK-003.07 | `UpdateStatus/UpdateOrderStatus.Validator.cs` — Add `// Validate:` on each RuleFor block | | |
| TASK-003.08 | `Shared/Validators/Order.Validator.cs` — Add `// Validate:` labels on each RuleFor in OrderParametersValidator; add doc comments on class and extension method | | |

### Implementation Phase 4 — Shared Files: Add Doc Comments, Boundary, and Inline Labels

- GOAL-004: Add XML doc comments on public types and methods, plus CAT-10 `Boundary:` annotations and inline labels, to shared mapping, model, and validator files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004.01 | `Shared/Mappings/Order.Mapping.Domain.cs` — Add `/// <summary>` on static class and MapToDomain method; add `// Boundary: Features → Domain` at class level; add `// Map:` on extension method body | | |
| TASK-004.02 | `Shared/Mappings/Order.Mapping.Model.cs` — Add `/// <summary>` on MapToDetail and MapToListItem; add `// Boundary:` at class level; add `// Map:` on each method body | | |
| TASK-004.03 | `Shared/Models/Order.Model.Parameters.cs` — Add `/// <summary>` on class; add `// Invariant:` documenting that Currency defaults to OrderConstant.Defaults.Currency | | |
| TASK-004.04 | `Shared/Models/Order.Model.Request.cs` — Add `/// <summary>` on class | | |
| TASK-004.05 | `Shared/Models/Order.Model.Response.cs` — Add `/// <summary>` on OrderDetailResponse and OrderListItemResponse; add property doc comments for PaymentState, ShipmentState, and other non-obvious fields | | |

### Implementation Phase 5 — Request/Response DTO Files: Add Doc Comments on Non-Obvious Properties

- GOAL-005: Add `/// <summary>` doc comments on public DTO properties in feature-specific Request and Response files where the field purpose is not self-evident from the name alone

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005.01 | `Cancel/CancelOrderAdmin.Request.cs` — Add `/// <summary>` on Reason property | | |
| TASK-005.02 | `AddLineItem/AddOrderLineItem.Request.cs` — Add `/// <summary>` on Price property (explain it's the unit price at time of addition) | | |
| TASK-005.03 | `AddLineItem/AddOrderLineItem.Response.cs` — Add `/// <summary>` on Id, VariantId, Quantity, Total | | |
| TASK-005.04 | `UpdateLineItem/UpdateOrderLineItem.Response.cs` — Add `/// <summary>` on Id, Quantity, Total | | |
| TASK-005.05 | `Get/LineItemById/GetOrderLineItemById.cs` inner Response — Add `/// <summary>` on AdjustmentTotal, Currency, CreatedAtUtc | | |
| TASK-005.06 | `Get/LineItems/GetOrderLineItems.Response.cs` — Add `/// <summary>` on AdjustmentTotal, Currency, CreatedAtUtc | | |
| TASK-005.07 | `Approve/ApproveOrder.cs` inner Response — Add `/// <summary>` on ApprovedById, ApprovedAtUtc | | |
| TASK-005.08 | `Resume/ResumeOrder.cs` inner Response — Add `/// <summary>` on Status (documenting it's the resumed order status) | | |

### Implementation Phase 6 — Verification & QA

- GOAL-006: Verify all comments comply with the Commenting Standard formatting rules and anti-pattern checklist; ensure build passes with warnings-as-errors

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006.01 | Run `dotnet build` to verify no warnings or errors are introduced | | |
| TASK-006.02 | Audit all added comments against anti-pattern checklist: AP-1 (redundancy), AP-2 (vagueness), AP-3 (over-commenting), AP-4 (inconsistent style), AP-8 (passive voice) | | |
| TASK-006.03 | Verify max 100 chars per comment line (F3), capitalised first word (F2), one space after delimiter (F4) | | |
| TASK-006.04 | Run `dotnet test` on Ordering-related unit tests to confirm no regressions | | |

## 3. Alternatives

- **ALT-001**: Big-bang comment pass on all 71 files at once — rejected due to higher risk of introducing build warnings/errors and difficulty reviewing
- **ALT-002**: Use automated comment-generation tool — rejected because machine-generated comments violate the Semantic Density Principle (P6) and ETH Zurich AGENTbench findings show they reduce agent success rates
- **ALT-003**: Skip Request/Response DTOs entirely — rejected because non-obvious properties (AdjustmentTotal, OutstandingBalance, PaymentState) benefit from WHY explanations for both human and AI readers

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` v3.0 — the authoritative source for all label definitions, formatting rules, and anti-patterns
- **DEP-002**: `guide/code-commenting/README.md` — human-readable label decision tree and examples
- **DEP-003**: .NET SDK 10 — `dotnet build` and `dotnet test` commands for verification
- **DEP-004**: No new NuGet packages required — comments are source-only changes

## 5. Files

- **FILE-001** to **FILE-071**: All 71 `.cs` files under `service/Api/src/Module/Ordering/Features/Admin/Orders/` — 16 handler files (Phase 1), 16 endpoint files (Phase 2), 8 validator files (Phase 3), 5 shared files (Phase 4), 8 Request/Response DTOs with non-obvious properties (Phase 5)
- **FILE-072**: No new files — all changes are in-place edits to existing files

## 6. Testing

- **TEST-001**: `dotnet build` — must pass with zero warnings (TreatWarningsAsErrors=true)
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all existing Ordering tests must pass
- **TEST-003**: Manual audit of each commented file against the Commenting Standard anti-pattern checklist
- **TEST-004**: Line-length check — grep for comment lines exceeding 100 characters

## 7. Risks & Assumptions

- **RISK-001**: Adding comments may trigger CS1591 (missing XML doc on public members) if the project has `<GenerateDocumentationFile>` enabled — mitigation: verify with `dotnet build` in Phase 6
- **RISK-002**: Stale or incorrect comments created if code is refactored between plan creation and execution — mitigation: plan execution should verify comments match current code
- **RISK-003**: Over-commenting (AP-3) if labels are applied mechanically without semantic judgment — mitigation: Phase 6 audit enforces semantic density (P6) and redundancy checks
- **ASSUMPTION-001**: All 16 handler files currently have `///` doc comments on Handle methods — verified true from code audit
- **ASSUMPTION-002**: The build uses `TreatWarningsAsErrors=true` as stated in AGENTS.md — verified true from `Directory.Build.props`
- **ASSUMPTION-003**: No existing comments violate the commenting standard in a blocking way — pre-existing `// Contract:` and `// Map:` labels follow the standard correctly

## 8. Related Specifications / Further Reading

- [`guide/code-commenting/CommentingRules.xml`](../guide/code-commenting/CommentingRules.xml) — authoritative spec
- [`guide/code-commenting/README.md`](../guide/code-commenting/README.md) — human-readable overview
- [`guide/code-commenting/references/anti-patterns.md`](../guide/code-commenting/references/anti-patterns.md) — anti-pattern definitions
- [`guide/code-commenting/references/label-quick-reference.md`](../guide/code-commenting/references/label-quick-reference.md) — quick label lookup
- [`AGENTS.md`](../AGENTS.md) — repository conventions (TreatWarningsAsErrors, vertical slice patterns)
- [`docs/codebase/CONVENTIONS.md`](../docs/codebase/CONVENTIONS.md) — coding conventions
