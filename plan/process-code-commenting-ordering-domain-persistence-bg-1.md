---
goal: Apply Code Commenting Standard v3.0 to Ordering Module — Domain, Persistence, Backgrounds, Services, Shared Features, and Module Registration
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
status: 'Planned'
tags: process, documentation, commenting, ordering-module
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Systematic application of the Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to ~46 `.cs` files in `service/Api/src/Module/Ordering/` not covered by existing storefront/admin feature plans. Covers Domain entities (Orders, LineItems, Adjustments — 27 files), Persistence (configurations, seeders, schema — 6 files), Backgrounds (CartExpiryJob — 5 files), Services (CartExpiryService — 2 files), Features/Shared (OrderingFeature constants, OrderInventoryService — 4 files), and the Module registration extension (1 file). The standard defines 10 label categories (CAT-1 through CAT-10), temporal markers, doc-comment requirements, and anti-pattern rules.

## 1. Requirements & Constraints

- **REQ-001**: Every `.cs` file must carry appropriate comments per the Commenting Standard label decision tree
- **REQ-002**: Domain entity class definitions (`Order.cs`, `LineItem.cs`, `Adjustment.cs`) must carry `@CAT-10 Invariant:` annotations documenting class-level invariants
- **REQ-003**: Domain method partial classes (`.Method.*.cs`) must carry inline labels: `// Guard:` on precondition checks, `// Enforce:` on business rules, `// Compute:` on calculations, `// Assign:` on mutations
- **REQ-004**: Domain `Constant.cs` files must carry `// @CAT-10 Invariant:` or `// Initialize:` labels documenting constraints and defaults
- **REQ-005**: Domain `Validation.cs` files must carry `// Validate:` labels (already present in Order.Validation.cs; verify LineItem.Validation.cs and Adjustment.Validation.cs)
- **REQ-006**: Domain `Result.cs` files must carry `// Contract:` annotations on error/success factory classes
- **REQ-007**: Domain `Loggers.cs` files must carry `// Log:` labels (already present; verify consistency)
- **REQ-008**: Persistence Configuration files (EF Core `IEntityTypeConfiguration`) must carry `// @CAT-10 Boundary: Persistence → Domain` annotations
- **REQ-009**: Persistence schema constants (`OrderingSchema.cs`) must carry `// Context:` annotation
- **REQ-010**: Background job and service files must carry operation-specific inline labels: `// Filter:` on expiry queries, `// Update:` on status transitions, `// Await:` on delay loops, `// Catch:` on error recovery
- **REQ-011**: Module registration extension (`Ordering.Extension.cs`) must carry `// @CAT-10 Boundary:` at the entry point (already present; verify completeness)
- **REQ-012**: Features/Shared files (`OrderingFeature.*.cs`) must carry `// Initialize:` or `// Context:` annotations
- **REQ-013**: `OrderInventoryService.cs` must carry `// Call:` on cross-module dispatch and `// @CAT-10 Boundary:` on module edge
- **REQ-014**: `IAdjustable.cs` must carry `// Contract:`, `// Boundary:`, `// AgentHint:` (already present; verify completeness)
- **REQ-015**: `Adjuster/AdjusterBase.cs` must carry `// Compute:` or `// Explain:` on abstract calculation methods
- **CON-001**: Must not add comments that restate what the code already expresses (AP-1 Redundancy)
- **CON-002**: Must not add comments on trivial lines (AP-3 Over-commenting)
- **CON-003**: All comment bodies must use imperative-mood verbs (F10, AP-8)
- **CON-004**: Max 100 characters per comment line (F3)
- **CON-005**: One label, one action — never join two with "and" (F8)
- **CON-006**: No trailing code-statement comments except inline data literals (F1)
- **CON-007**: CAT-10 agent annotations must use KEY=VALUE form for reliable machine parsing (F9)
- **CON-008**: All `/// <summary>` XML-doc comments must be preserved and updated if stale — never removed
- **GUD-001**: Follow the CommentingRules.xml label decision tree (label categories CAT-1 through CAT-10)
- **GUD-002**: Domain entity inline labels use `// @CAT-10 Label:` convention (existing pattern) for class-level annotations, `// Label:` for implementation-level annotations
- **PAT-001**: Partial file pattern per domain entity: `{Entity}.cs` + `{Entity}.Constant.cs` + `{Entity}.Loggers.cs` + `{Entity}.Result.cs` + `{Entity}.Validation.cs` + `{Entity}.Method.*.cs`
- **PAT-002**: Background job partial pattern: `CartExpiryJob.cs` + `.Constants.cs` + `.Loggers.cs` + `.Result.cs` + `.Scheduler.cs`
- **PAT-003**: Vertical slice feature pattern for `Features/Shared/`: constants in `OrderingFeature.*.cs`, service in `Services/OrderInventoryService.cs`

## 2. Implementation Steps

### Implementation Phase 1 — Domain: Entity Class-Level Annotations (Orders, LineItems, Adjustments)

- GOAL-001: Add `@CAT-10 Invariant:`, `// Boundary:`, and `// AgentHint:` annotations to all 3 domain entity class definitions and the `IAdjustable` interface

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Domain/Orders/Order.cs` — Verify existing `@CAT-10 Invariant:` covers Total invariant, CheckoutState progression, immutability after Finalize, Total>=0. Add missing invariants if any. Add `// Boundary: Domain → Persistence` class-level annotation. | | |
| TASK-002 | `Domain/LineItems/LineItem.cs` — Add `@CAT-10 Invariant:` documenting: Quantity >= 1, UnitPrice >= 0, Total = UnitPrice * Quantity, OrderId != default. Add `// @CAT-10 Boundary: Domain → Persistence` class-level annotation. | | |
| TASK-003 | `Domain/Adjustments/Adjustment.cs` — Add `@CAT-10 Invariant:` documenting: Amount != 0, Type is never null, OrderId != default, immutable after creation. Add `// @CAT-10 Boundary: Domain → Persistence` class-level annotation. | | |
| TASK-004 | `Domain/Adjustments/IAdjustable.cs` — Verify existing `// Contract:`, `// Boundary:`, `// AgentHint:` annotations are accurate and complete per standard v3.0 KEY=VALUE format | | |

### Implementation Phase 2 — Domain: Method Partial Classes (Factory, State Machine, Computation, Operations, Checkout)

- GOAL-002: Add operation-specific inline labels to all 10 domain method partial files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | `Domain/Orders/Order.Method.Factory.cs` — Add `// Create:` label before factory method body documenting purpose (draft order with defaults). Add `// Assign:` on each property initialization | | |
| TASK-006 | `Domain/Orders/Order.Method.StateMachine.cs` — Replace `/// <summary>`-only methods with inline labels: `// Guard:` before each status precondition check, `// Enforce:` on business rules, `// Assign:` on status mutations. Keep existing `/// <summary>` doc comments. | | |
| TASK-007 | `Domain/Orders/Order.Method.Computation.cs` — Add `// Compute:` labels documenting recalculation formulas (ItemTotal, AdjustmentTotal, ShipmentTotal -> Total). Add `// Assert:` on post-conditions | | |
| TASK-008 | `Domain/Orders/Order.Method.Checkout.cs` — Add `// Validate:` on checkout prerequisite checks, `// Enforce:` on checkout step sequencing, `// Assign:` on CheckoutState transitions | | |
| TASK-009 | `Domain/Orders/Order.Method.Operations.cs` — Add `// Update:` on address/shipping method mutations, `// Guard:` on draft-status precondition | | |
| TASK-010 | `Domain/LineItems/LineItem.Method.Factory.cs` — Add `// Create:` label, `// Validate:` on quantity/price preconditions | | |
| TASK-011 | `Domain/LineItems/LineItem.Method.Compute.cs` — Add `// Compute:` label documenting Total = UnitPrice * Quantity | | |
| TASK-012 | `Domain/LineItems/LineItem.Method.Quantity.cs` — Add `// Validate:` on quantity constraints, `// Guard:` on max quantity, `// Compute:` on recalculation | | |
| TASK-013 | `Domain/Adjustments/Adjustment.Method.Factory.cs` — Add `// Create:` label, `// Validate:` on amount != 0 precondition | | |
| TASK-014 | `Domain/Adjustments/Adjustment.Method.State.cs` — Add `// Enforce:` on open/close state transitions, `// Guard:` on closed-to-open validation | | |
| TASK-015 | `Domain/Adjustments/Adjustment.Method.Eligible.cs` — Add `// Validate:` or `// Filter:` labels on eligibility logic. Add `// Explain:` if eligibility algorithm is non-obvious | | |
| TASK-016 | `Domain/Adjustments/Adjuster/AdjusterBase.cs` — Add `// Compute:` or `// Explain:` on abstract `Calculate` method. Add `// Contract: pre=order!=null, post=adjustments applied` | | |

### Implementation Phase 3 — Domain: Constant, Validation, Result, and Logger Files

- GOAL-003: Add `// Contract:`, `// Initialize:`, `// Validate:`, and `// Log:` annotations as appropriate to all 12 domain utility partial files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | `Domain/Orders/Order.Constant.cs` — Add `// Initialize:` or `// @CAT-10 Invariant:` documenting constraints and defaults | | |
| TASK-018 | `Domain/Orders/Order.Validation.cs` — Verify existing `// Validate:` label. Add per-method labels if missing: `// Validate:` before each validation rule method | | |
| TASK-019 | `Domain/Orders/Order.Result.cs` — Verify existing `// Contract:` label. Add `// Create:` on error/success factory methods if appropriate | | |
| TASK-020 | `Domain/Orders/Order.Loggers.cs` — Verify existing `// Log:` label is present | | |
| TASK-021 | `Domain/Orders/Order.Enumerate.cs` — Add `// Enumerate:` label documenting OrderStatus and CheckoutState lifecycle | | |
| TASK-022 | `Domain/Orders/OrderNumber.cs` — Add `// Generate:` label documenting order number generation strategy | | |
| TASK-023 | `Domain/LineItems/LineItem.Constant.cs` — Add `// Initialize:` or `@CAT-10 Invariant:` documenting max quantity, price constraints | | |
| TASK-024 | `Domain/LineItems/LineItem.Validation.cs` — Add `// Validate:` label at class level documenting validation purpose | | |
| TASK-025 | `Domain/LineItems/LineItem.Result.cs` — Add `// Contract:` label at class level documenting error/success factory pattern | | |
| TASK-026 | `Domain/LineItems/LineItem.Loggers.cs` — Verify existing `// Log:` label or add one | | |
| TASK-027 | `Domain/Adjustments/Adjustment.Constant.cs` — Add `// Initialize:` or `@CAT-10 Invariant:` documenting amount constraints | | |
| TASK-028 | `Domain/Adjustments/Adjustment.Validation.cs` — Add `// Validate:` label at class level | | |
| TASK-029 | `Domain/Adjustments/Adjustment.Result.cs` — Add `// Contract:` label at class level | | |
| TASK-030 | `Domain/Adjustments/Adjustment.Loggers.cs` — Verify existing `// Log:` label or add one | | |

### Implementation Phase 4 — Persistence Layer: Configuration, Schema, and Seeders

- GOAL-004: Add `// @CAT-10 Boundary: Persistence → Domain` annotations to all EF Core configuration files and schema constants

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | `Persistence/Configurations/Orders/OrderConfiguration.cs` — Add `// @CAT-10 Boundary: Persistence → Domain — reserved for EF Core materialization; do not add domain logic` at class level. Add `// Initialize:` on property configuration sections | | |
| TASK-032 | `Persistence/Configurations/LineItems/LineItemConfiguration.cs` — Add `// @CAT-10 Boundary:` annotation. Add `// Initialize:` on property configuration | | |
| TASK-033 | `Persistence/Configurations/Adjustments/AdjustmentConfiguration.cs` — Add `// @CAT-10 Boundary:` annotation. Add `// Initialize:` on property configuration | | |
| TASK-034 | `Persistence/OrderingSchema.cs` — Add `// Context:` annotation at class level documenting schema purpose and namespace | | |
| TASK-035 | `Persistence/Seeders/Order.Seeder.cs` — Add `// Initialize:` label documenting seeding purpose. Add `// Create:` on sample order generation | | |
| TASK-036 | `Persistence/Seeders/Payment.Seeder.cs` — Add `// Initialize:` label. Add `// Create:` on sample payment generation | | |

### Implementation Phase 5 — Background Jobs and Services

- GOAL-005: Add operation-specific inline labels and CAT-10 annotations to all 7 background job and service files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-037 | `Backgrounds/CartExpiryJob.cs` — Add `// Filter:` on expired cart query, `// Update:` on status/deletion mutation, `// Log:` on logger calls | | |
| TASK-038 | `Backgrounds/CartExpiryJob.Constants.cs` — Add `// Initialize:` or context label | | |
| TASK-039 | `Backgrounds/CartExpiryJob.Loggers.cs` — Add `// Log:` label | | |
| TASK-040 | `Backgrounds/CartExpiryJob.Result.cs` — Add `// Contract:` label | | |
| TASK-041 | `Backgrounds/CartExpiryJob.Scheduler.cs` — Add `// Trigger:` on Hangfire registration, `// Schedule:` on job scheduling | | |
| TASK-042 | `Services/CartExpiryService.cs` — Add `// Await:` on delay loop, `// Catch:` on error recovery (shutdown), `// @CAT-10 Contract:` on RunAsync entry, `// @CAT-10 Boundary: Service → Background` | | |
| TASK-043 | `Services/CartExpiryService.Loggers.cs` — Add `// Log:` label | | |

### Implementation Phase 6 — Shared Features and Module Registration

- GOAL-006: Add context, boundary, and initialize annotations to Features/Shared and the module extension

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-044 | `Features/Shared/OrderingFeature.Admin.cs` — Add `// Initialize:` on each route/description/summary constant group. Add `// Context:` referencing OpenAPI tag source | | |
| TASK-045 | `Features/Shared/OrderingFeature.Storefront.cs` — Add `// Initialize:` on each route/description/summary constant group | | |
| TASK-046 | `Features/Shared/OrderingFeature.Tags.cs` — Add `// Initialize:` on tag constants. Add `// @CAT-10 Boundary: Module → API` on class to mark OpenAPI boundary | | |
| TASK-047 | `Features/Shared/Services/OrderInventoryService.cs` — Add `// Call:` on each cross-module MediatR dispatch, `// @CAT-10 Boundary: Ordering → Inventory` at class level, `// Compensate:` on rollback operations | | |
| TASK-048 | `Ordering.Extension.cs` — Verify existing `// @CAT-10 Boundary:` annotations cover all entry points. Add `// Register:` on each DI registration | | |

## 3. Alternatives

- **ALT-001**: Apply commenting inline without `@CAT-10` prefix — rejected because the codebase already uses `@CAT-10` prefix for class-level CAT-10 annotations, making it the established convention
- **ALT-002**: Rewrite all `/// <summary>` doc comments with inline labels instead — rejected because doc comments and inline labels serve different purposes (public API contract vs implementation intent)
- **ALT-003**: Single monolithic pass across all 46 files — rejected because domain concepts, persistence, and infrastructure have different commenting needs and should be reviewed independently for correctness

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` — authoritative source for all label definitions, categories, and formatting rules
- **DEP-002**: `guide/code-commenting/README.md` — human-readable overview with examples
- **DEP-003**: Existing commenting conventions in the Ordering module (e.g. `@CAT-10` prefix, `// Contract:` format) must be preserved as they already follow v3.0
- **DEP-004**: No code logic changes are required — commenting is non-functional and must not alter behavior

## 5. Files

- **FILE-001** to **FILE-003**: `Domain/Orders/Order.cs`, `Domain/LineItems/LineItem.cs`, `Domain/Adjustments/Adjustment.cs` — entity class definitions
- **FILE-004**: `Domain/Adjustments/IAdjustable.cs` — adjustable interface
- **FILE-005** to **FILE-016**: `Domain/*/{Entity}.Method.*.cs` (10 method partial files)
- **FILE-017**: `Domain/Adjustments/Adjuster/AdjusterBase.cs` — abstract adjuster base
- **FILE-018** to **FILE-029**: `Domain/*/{Entity}.Constant.cs`, `{Entity}.Validation.cs`, `{Entity}.Result.cs`, `{Entity}.Loggers.cs` (12 utility files)
- **FILE-030**: `Domain/Orders/Order.Enumerate.cs` — enum definitions
- **FILE-031**: `Domain/Orders/OrderNumber.cs` — order number generator
- **FILE-032** to **FILE-034**: `Persistence/Configurations/*/{Entity}Configuration.cs` (3 EF configs)
- **FILE-035**: `Persistence/OrderingSchema.cs` — schema constants
- **FILE-036** to **FILE-037**: `Persistence/Seeders/Order.Seeder.cs`, `Payment.Seeder.cs`
- **FILE-038** to **FILE-042**: `Backgrounds/CartExpiryJob.cs`, `.Constants.cs`, `.Loggers.cs`, `.Result.cs`, `.Scheduler.cs`
- **FILE-043** to **FILE-044**: `Services/CartExpiryService.cs`, `CartExpiryService.Loggers.cs`
- **FILE-045** to **FILE-047**: `Features/Shared/OrderingFeature.Admin.cs`, `OrderingFeature.Storefront.cs`, `OrderingFeature.Tags.cs`
- **FILE-048**: `Features/Shared/Services/OrderInventoryService.cs`
- **FILE-049**: `Ordering.Extension.cs` — module DI registration

## 6. Testing

- **TEST-001**: `dotnet build service/Api/` — must pass with TreatWarningsAsErrors=true (no new warnings)
- **TEST-002**: Manual review of each changed file against CommentingRules.xml label decision tree to ensure correct label selection
- **TEST-003**: Verify no commented-out code was introduced (AP-6 check via `rg '^\s*//\s*(public|private|internal|var|if|for|foreach|while|using)\s' service/Api/src/Module/Ordering/`)
- **TEST-004**: Verify no stale `/// <summary>` comments were removed or degraded (all existing XML-doc must be preserved)
- **TEST-005**: Verify max line length compliance (100 chars) on all new comment lines

## 7. Risks & Assumptions

- **RISK-001**: Inconsistent label selection — mitigated by strict adherence to the CommentingRules.xml decision tree and existing codebase patterns
- **RISK-002**: Over-commenting trivial code (AP-3) — mitigated by GUD-002 rule: comment only where naming and structure cannot carry intent
- **RISK-003**: Stale XML-doc comments discovered during audit — marked as `issue (blocking)` per AP-5; must be updated in-scope
- **RISK-004**: Build fails due to inadvertently altered code (not comments) — mitigated by DEP-004 no-logic-change constraint
- **ASSUMPTION-001**: All existing `@CAT-10`, `// Contract:`, `// Boundary:`, and inline labels in the codebase already conform to v3.0 standard and will be preserved
- **ASSUMPTION-002**: The order of phases (Domain → Persistence → Backgrounds → Shared → Extension) minimizes rework by treating domain concepts first
- **ASSUMPTION-003**: No merging conflicts with parallel storefront/admin commenting plans since this plan covers disjoint file sets

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — authoritative standard
- `guide/code-commenting/README.md` — human-readable overview
- `guide/code-commenting/references/label-quick-reference.md` — condensed label lookup
- `guide/code-commenting/references/anti-patterns.md` — anti-pattern checklist
- `plan/process-code-commenting-ordering-storefront-1.md` — existing plan for Storefront features
- `plan/process-code-commenting-ordering-admin-orders-1.md` — existing plan for Admin features
