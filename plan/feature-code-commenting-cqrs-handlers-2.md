---
goal: Apply Code Commenting Standard v3.0 to remaining 9 CQRS handler files with gaps
version: 1.0
date_created: 2026-07-19
owner: Engineering Standards
status: 'Completed'
tags: feature, commenting, csharp, handlers, cqrs, standards
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Fresh audit of all 234 CQRS handler files reveals 9 files still have commenting gaps after previous passes. Target: 100% on Handle() XML doc, class XML doc, and structured inline labels (CAT-1 through CAT-9 per `guide/code-commenting/CommentingRules.xml`).

## 1. Requirements & Constraints

- **REQ-001**: Add `/// <summary>` XML doc on Handle() method where missing (1 file: VoidOrderPayments.cs)
- **REQ-002**: Add `/// <summary>` XML doc on handler class where missing (1 file: VoidOrderPayments.cs)
- **REQ-003**: Add structured inline labels (CAT-1 through CAT-9) in Handle() method bodies where absent (8 files)
- **REQ-004**: Use imperative verbs: "Load variant by ID" not "Variant loading by ID"
- **REQ-005**: `dotnet build` must pass with TreatWarningsAsErrors=true
- **REQ-006**: `dotnet test` must pass after all changes
- **CON-001**: Do NOT modify test files
- **PAT-001**: Follow existing inline label patterns from fully-documented handlers in same module
- **GUD-001**: Use `// Validate:` for input guards, `// Load:` for DB queries, `// Call:` for service calls, `// Transform:` for mapping

## 2. Implementation Steps

### Implementation Phase 1: Payment — VoidOrderPayments (missing Handle + class XML doc)

- GOAL-001: Add `/// <summary>` on both the handler class and Handle() method in VoidOrderPayments.cs, plus inline labels

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Read `service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs` — add `/// <summary>Handles voiding all pending payments for an order.</summary>` on `VoidOrderPaymentsCommandHandler` class and `/// <summary>Voids all non-completed payments for the specified order within a transaction scope.</summary>` on `Handle()` method. Add inline labels: `// Validate:` on precondition checks, `// Load:` on payment queries, `// Call:` on gateway void, `// Log:` on logging | | |

### Implementation Phase 2: Catalog — 4 files missing inline labels

- GOAL-002: Add structured inline labels to 4 Catalog handler files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002 | Read `Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.cs` — add `// Load:` on DB queries, `// Aggregate:` on count computations, `// Transform:` on response mapping | | |
| TASK-003 | Read `Catalog/Features/Admin/Products/Variants/GetById/GetVariantById.cs` — add `// Load:` on variant query, `// Check:` on null guard, `// Transform:` on Mapster mapping | | |
| TASK-004 | Read `Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs` — add `// Validate:` on input check, `// Call:` on orchestrator, `// Transform:` on response mapping | | |
| TASK-005 | Read `Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs` — add `// Validate:` on input check, `// Call:` on orchestrator, `// Transform:` on response mapping | | |

### Implementation Phase 3: Inventory — 3 files missing inline labels

- GOAL-003: Add structured inline labels to 3 Inventory handler files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Read `Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.cs` — add `// Load:` on DB queries, `// Aggregate:` on count/sum computations, `// Compute:` on low-stock percentage | | |
| TASK-007 | Read `Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs` — add `// Validate:` on file checks, `// Parse:` on CSV line parsing, `// Create:` on new stock items, `// Update:` on existing stock items, `// Log:` on completion | | |
| TASK-008 | Read `Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs` — add `// Validate:` on input, `// Load:` on variant/location, `// Compute:` on availability calculation | | |

### Implementation Phase 4: Shipping — 1 file missing inline labels

- GOAL-004: Add structured inline labels to 1 Shipping handler file

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Read `Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.cs` — add `// Validate:` on parameters, `// Load:` on rates query, `// Filter:` on applicable rates, `// Sort:` on price ordering, `// Transform:` on response mapping | | |

### Implementation Phase 5: Verification

- GOAL-005: Full build and test verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | `dotnet build` — verify 0 warnings, 0 errors | | |
| TASK-011 | `dotnet test service/Api/tests/Module.UnitTests` — verify all tests pass | | |
| TASK-012 | Confirm grep audit: `grep -rn "Handle(Command\|Handle(Query" service/Api/src/Module/ --include="*.cs" \| while read f; do ...` — verify zero handlers remain without `/// <summary>` on Handle() | | |

## 3. Alternatives

- **ALT-001**: Skip trivial handlers (3-line handlers like ImageEmbedding.Create/Regenerate) — rejected; consistency requires labels on all handlers per REQ-001
- **ALT-002**: Classify dashboard handlers as exempt — rejected; all other modules' dashboard handlers have inline labels

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` — authoritative standard
- **DEP-002**: `dotnet build` with TreatWarningsAsErrors=true

## 5. Files

- **FILE-001**: `Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs` — add Handle() XML doc + class doc + inline labels
- **FILE-002**: `Module/Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.cs` — add inline labels
- **FILE-003**: `Module/Catalog/Features/Admin/Products/Variants/GetById/GetVariantById.cs` — add inline labels
- **FILE-004**: `Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs` — add inline labels
- **FILE-005**: `Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs` — add inline labels
- **FILE-006**: `Module/Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.cs` — add inline labels
- **FILE-007**: `Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs` — add inline labels
- **FILE-008**: `Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs` — add inline labels
- **FILE-009**: `Module/Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.cs` — add inline labels

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests --no-build` — all tests pass
- **TEST-003**: Grep audit — zero handlers missing `/// <summary>` on Handle()

## 7. Risks & Assumptions

- **RISK-001**: VoidOrderPayments.cs uses a non-standard pattern (standalone sealed class instead of `static partial class`) — ensure XML doc is placed on the correct class declaration
- **ASSUMPTION-001**: All 9 files are smaller/simpler handlers where inline labels will be brief (2-5 labels each)
- **ASSUMPTION-002**: No new handlers were added during the time between audit and execution

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — CAT-1 through CAT-9 label categories
- `plan/feature-code-commenting-cqrs-handlers-1.md` — previous pass (225 handlers now documented)
