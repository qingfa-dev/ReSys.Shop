---
goal: Apply Code Commenting Standard v3.0 to all C# API service files in service/Api/src/
version: 1.0
date_created: 2026-07-19
owner: Engineering Standards
status: 'Completed'
tags: feature, commenting, csharp, standards, services
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Apply the structured Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to all C# service files in `service/Api/src/` that are missing proper comments. The standard defines 10 label categories (CAT-1 through CAT-10) plus Temporal Markers and C# XML Documentation Comments (`/// <summary>`). Each comment must explain WHY, never WHAT; use imperative verbs; follow the Semantic Density Principle (every token earns its place).

Target files include: service classes in `Services/` folders, CQRS handler `Handle()` methods, domain service-like methods, and infrastructure service classes across all 9 modules plus Shared.

## 1. Requirements & Constraints

- **REQ-001**: All service class files in `Services/` folders across all modules must receive XML doc comments (`/// <summary>`) on public classes and public methods
- **REQ-002**: All CQRS handler `Handle()` methods must have XML doc comments (`/// <summary>` `<param>` `<returns>`)
- **REQ-003**: All inline comments must use structured labels from CAT-1 through CAT-10 with imperative verbs
- **REQ-004**: CAT-10 agent annotations must use `KEY=VALUE` form for machine parsing
- **REQ-005**: `dotnet build` must pass with TreatWarningsAsErrors=true after all changes
- **REQ-006**: Max line length 100 characters per F3 rule — no new warnings
- **REQ-007**: One label, one action — never join two actions with "and" (F8)
- **REQ-008**: Comments on their own line — never trailing a code statement (F1 exception for inline data literals)
- **REQ-009**: Do NOT modify test files in `service/Api/tests/`
- **REQ-010**: Do NOT modify migration files in `service/Api/src/Migrations/`
- **REQ-011**: Do NOT modify model/response/request DTO files unless they have public methods needing doc
- **PAT-001**: Follow existing commenting patterns in already-well-commented files (e.g., `PaymentProcessingService.cs`, `CartExpiryService.cs`)
- **PAT-002**: Use `/// <summary>...</summary>` for public class summary; `<param>` and `<returns>` for public methods; `<exception>` where applicable
- **GUD-001**: Use `// Label: Imperative sentence.` format for all inline labels per CAT-1 through CAT-9
- **GUD-002**: Use `// Contract: pre=..., post=...` for function contracts (CAT-10)
- **GUD-003**: Use `// Boundary: LAYER → LAYER — REASON` at module boundary points (CAT-10)
- **GUD-004**: Use `// Invariant: CONDITION` on domain entity classes (CAT-10)
- **GUD-005**: Apply F9 rule: CAT-10 agent annotations always use `KEY=VALUE` form

## 2. Implementation Steps

### Implementation Phase 1: Inventory Module Services

- GOAL-001: Add XML doc comments and structured inline labels to all 11 service files in `Module/Inventory/Services/`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `StockAvailabilityService.cs` — add `/// <summary>` on class and `IsAvailableAsync`/`IsAvailableAnyLocationAsync`; ensure existing `// Validate:`, `// Load:`, `// Compute:` labels follow CAT-3/F8 rules | | |
| TASK-002 | `StockAvailabilityCalculator.cs` — add `/// <summary>` on class and public methods; add CAT-3 `Compute:` labels on availability calculations | | |
| TASK-003 | `StockQuantityService.cs` — add `/// <summary>` on class and public methods; add CAT-1 `Validate:` / CAT-3 `Compute:` / CAT-6 `Acquire:` labels | | |
| TASK-004 | `StockReservationService.cs` — add `/// <summary>` on class and public methods; add CAT-2 `Create:` / CAT-4 `Raise:` / CAT-6 `Acquire:` labels; add `// Contract:` on reserve/cancel methods | | |
| TASK-005 | `StockRestockService.cs` — add `/// <summary>` on class and public methods; add CAT-3 `Compute:` / CAT-2 `Update:` / CAT-9 `Log:` labels | | |
| TASK-006 | `StockSummaryService.cs` — add `/// <summary>` on class and public methods; add CAT-3 `Aggregate:` / `Compute:` labels | | |
| TASK-007 | `StockSnapshot.cs` — add `/// <summary>` on class; add `// Invariant:` on snapshot data structure | | |
| TASK-008 | `ReservationExpiryService.cs` — add `/// <summary>` on class and methods; add CAT-5 `Await:` / CAT-6 `Release:` / CAT-5 `Batch:` / CAT-7 `Catch:` labels | | |
| TASK-009 | `CartReservationService.cs` — add `/// <summary>` on class and public methods; add CAT-6 `Acquire:` / CAT-5 `Fallback:` / CAT-2 `Remove:` labels | | |
| TASK-010 | `LowStockThreshold.cs` — add `/// <summary>` on class; add `// Invariant:` on threshold configuration | | |
| TASK-011 | Interface files (`IStockAvailabilityService.cs`, `IStockSummaryService.cs`, `IStockRestockService.cs`, `IStockReservationService.cs`, `ICartReservationService.cs`, `IStockAvailabilityCalculator.cs`) — add `/// <summary>` on each interface and each method signature | | |

### Implementation Phase 2: Catalog Module Services

- GOAL-002: Add XML doc and structured labels to all service files in `Module/Catalog/Features/Admin/Taxonomies/Taxons/Services/` and embedding services

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | `TaxonHierarchyService.cs` (main) — add `/// <summary>` on class and public methods; add `// Boundary: Features → Domain` at layer edges; add CAT-3 `Compute:` / CAT-4 `Enforce:` / CAT-2 `Update:` labels | | |
| TASK-013 | `TaxonHierarchyService.Validation.cs` — add `/// <summary>` on validation methods; add CAT-1 `Validate:` / `Guard:` labels on all validation guards | | |
| TASK-014 | `TaxonHierarchyService.Rebuild.cs` — add `/// <summary>` on rebuild methods; add CAT-3 `Compute:` / CAT-5 `Batch:` / CAT-9 `Log:` labels | | |
| TASK-015 | `TaxonHierarchyService.Permalinks.cs` — add `/// <summary>` on permalink methods; add CAT-3 `Generate:` / `Normalize:` labels | | |
| TASK-016 | `TaxonHierarchyService.Internal.cs` — add `/// <summary>` on internal helpers; add CAT-3 `Compute:` / CAT-1 `Check:` labels | | |
| TASK-017 | `AutoClassificationService.cs` — add `/// <summary>` on class and methods; add CAT-3 `Compute:` / CAT-4 `Enforce:` / CAT-10 `Contract:` labels | | |
| TASK-018 | `TaxonRuleEvaluator.cs` — add `/// <summary>` on class and methods; add CAT-3 `Evaluate:` / CAT-1 `Validate:` labels | | |
| TASK-019 | `QueryingTaxonRuleEvaluator.cs` — add `/// <summary>` on class and methods; add CAT-3 `Filter:` / CAT-8 `Map:` labels | | |
| TASK-020 | `ImageEmbedding.Orchestrator.cs` — add `/// <summary>` on class and methods; add CAT-8 `Call:` / CAT-5 `Retry:` / CAT-6 `Cache:` labels; add `// Contract:` on orchestration entry point | | |
| TASK-021 | `ImageEmbedding.Inference.cs` — add `/// <summary>` on class and methods; add CAT-8 `Call:` / CAT-3 `Compute:` / CAT-7 `Catch:` labels | | |

### Implementation Phase 3: Ordering Module Services

- GOAL-003: Add XML doc and structured labels to all service files in `Module/Ordering/Services/`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | `CartExpiryService.cs` — verify existing comments are complete; add `// Invariant:` on class; add `/// <summary>` if missing on `ExecuteAsync` | | |
| TASK-023 | `CartExpiryService.Loggers.cs` — add `/// <summary>` on logger methods; add CAT-9 `Log:` labels | | |
| TASK-024 | `OrderInventoryService.cs` — add `/// <summary>` on class and methods; add CAT-6 `Acquire:` / CAT-2 `Update:` / CAT-8 `Call:` / CAT-5 `Fallback:` labels | | |

### Implementation Phase 4: Payment Module Services

- GOAL-004: Add XML doc and structured labels to all service files in `Module/Payment/Services/`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | `PaymentProcessingService.cs` — verify existing `// Contract:` and inline labels are complete; add `/// <summary>` on class and all public methods; add `/// <param>` / `/// <returns>` / `/// <exception>` on `ProcessAsync`, `CaptureAsync`, `VoidAsync`, `RefundAsync` | | |
| TASK-026 | `IPaymentProcessingService.cs` — add `/// <summary>` on interface and each method; add `// Contract:` on method signatures | | |
| TASK-027 | `StripeWebhookService.cs` — add `/// <summary>` on class and methods; add CAT-8 `Webhook:` / CAT-1 `Validate:` / CAT-7 `Catch:` / CAT-9 `Audit:` labels | | |
| TASK-028 | `IStripeWebhookService.cs` — add `/// <summary>` on interface and methods | | |
| TASK-029 | `IWebhookHandler.cs` — add `/// <summary>` on interface and method | | |
| TASK-030 | `GatewayRegistry.cs` — add `/// <summary>` on class and methods; add CAT-2 `Add:` / `Create:` / `Remove:` labels; add `// Invariant:` on registry state | | |
| TASK-031 | `IGatewayRegistry.cs` — add `/// <summary>` on interface and methods | | |
| TASK-032 | `StripeGateway.cs` — add `/// <summary>` on class and public methods; add CAT-8 `Call:` / CAT-3 `Transform:` / CAT-5 `Retry:` / CAT-7 `Catch:` labels; add `// Contract:` on payment calls | | |
| TASK-033 | `BogusGateway.cs` — add `/// <summary>` on class and methods; add CAT-3 `Generate:` / `Compute:` labels | | |

### Implementation Phase 5: Shared Infrastructure Services

- GOAL-005: Add XML doc and structured labels to all service-like classes in `Shared/` (Caching, Security, Operational, Performance)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-034 | `RefreshToken.Service.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-2 `Create:` / `Update:` / CAT-1 `Validate:` / CAT-6 `Purge:` / CAT-9 `Audit:` labels | | |
| TASK-035 | `RefreshTokenStore.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-6 `Acquire:` / `Release:` / `Cache:` / `Purge:` labels | | |
| TASK-036 | `Permission.Service.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-1 `Check:` / CAT-3 `Compute:` / CAT-6 `Cache:` / CAT-9 `Log:` labels | | |
| TASK-037 | `Permission.Cache.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-6 `Cache:` / `Purge:` / CAT-3 `Compute:` labels; add `// Contract:` on cache-hit guarantee | | |
| TASK-038 | `Storage.Service.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-8 `Serialize:` / `Deserialize:` / CAT-6 `Acquire:` / CAT-1 `Validate:` / CAT-7 `Catch:` / `Compensate:` labels | | |
| TASK-039 | `Storage.SecurityEnforcer.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-1 `Guard:` / CAT-1 `Validate:` / CAT-4 `Enforce:` labels (name threats: path traversal, MIME bypass) | | |
| TASK-040 | `Storage.MalwareScanner.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-1 `Check:` / CAT-9 `Audit:` / CAT-5 `Skip:` labels | | |
| TASK-041 | `StorageAntiForgeryGuard.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-1 `Guard:` / CAT-1 `Validate:` / CAT-4 `Enforce:` labels; name security threats explicitly | | |
| TASK-042 | `AesEncryptionService.cs` — add `/// <summary>` on class and methods; add CAT-3 `Encrypt:` / `Decrypt:` / CAT-1 `Validate:` labels; add `// Contract:` with pre/post conditions | | |
| TASK-043 | `ImageProcessor.Implementation.cs` — add `/// <summary>` on class and methods; add CAT-3 `Transform:` / `Resize:` / CAT-1 `Validate:` labels | | |
| TASK-044 | `DatabaseInitializerService.cs` — add `/// <summary>` on class and methods; add CAT-2 `Initialize:` / CAT-9 `Log:` / CAT-7 `Catch:` / CAT-7 `Degrade:` labels | | |
| TASK-045 | `Caching.Service.Implement.cs` — add `/// <summary>` on class and methods; add CAT-6 `Cache:` / `Get:` / `Set:` / `Purge:` labels; add `// Contract:` on TTL semantics | | |

### Implementation Phase 6: CQRS Handler `Handle()` Methods — Missing XML Doc

- GOAL-006: Audit and add XML doc comments (`/// <summary>` `<param>` `<returns>`) to all CQRS handler `Handle()` methods across all modules that lack them

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-046 | **Catalog** — audit all ~80 handler files under `Module/Catalog/Features/` for missing `/// <summary>` on `Handle()`; add `<summary>`, `<param name="request">`, `<returns>` on each | | |
| TASK-047 | **Identity** — audit all ~40 handler files under `Module/Identity/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-048 | **Inventory** — audit all ~30 handler files under `Module/Inventory/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-049 | **Location** — audit all ~15 handler files under `Module/Location/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-050 | **Ordering** — audit all ~40 handler files under `Module/Ordering/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-051 | **Payment** — audit all ~20 handler files under `Module/Payment/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-052 | **Profile** — audit all ~15 handler files under `Module/Profile/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-053 | **Shipping** — audit all ~15 handler files under `Module/Shipping/Features/` for missing XML doc on `Handle()`; add full XML doc block | | |
| TASK-054 | **Dashboard** — add XML doc on handle files under `Module/Dashboard/Features/` | | |

### Implementation Phase 7: Domain Entity Classes — Invariant and XML Doc

- GOAL-007: Add `// Invariant:` annotations and `/// <summary>` on domain entity classes across all modules (Product, Order, Variant, User, etc.)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-055 | **Catalog Domain** — add `// Invariant:` on `Product.cs`, `Variant.cs`, `Taxon.cs`, `Taxonomy.cs`, `Price.cs`, `VariantImage.cs`, `ImageEmbedding.cs`, `Classification.cs`, `OptionType.cs`, `OptionValue.cs`; add `/// <summary>` on each class | | |
| TASK-056 | **Identity Domain** — add `// Invariant:` on `User.cs`, `Role.cs`, `RefreshToken.cs`, `PermissionMetadata.cs`; add `/// <summary>` on each class | | |
| TASK-057 | **Inventory Domain** — add `// Invariant:` on `StockItem.cs`, `StockReservation.cs`, `StockTransfer.cs`, `StockMovement.cs`, `StockLocation.cs`; add `/// <summary>` on each class | | |
| TASK-058 | **Ordering Domain** — add `// Invariant:` on `Order.cs`, `LineItem.cs`, `Adjustment.cs`; add `/// <summary>` on each class | | |
| TASK-059 | **Payment Domain** — add `// Invariant:` on `PaymentCapture.cs`, `PaymentMethod.cs`; add `/// <summary>` on each class | | |
| TASK-060 | **Profile Domain** — add `// Invariant:` on `UserProfile.cs`, `Address.cs`, `Wishlist.cs`, `WishedItem.cs`, `UserPreference.cs`; add `/// <summary>` on each class | | |
| TASK-061 | **Shipping Domain** — add `// Invariant:` on `ShippingRate.cs`, `ShippingMethod.cs`; add `/// <summary>` on each class | | |
| TASK-062 | **Location Domain** — add `// Invariant:` on `Country.cs`, `State.cs`; add `/// <summary>` on each class | | |

### Implementation Phase 8: Background Job & Scheduler Files

- GOAL-008: Add XML doc and structured labels to background job and scheduler files across all modules

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-063 | `Module/Ordering/Backgrounds/CartExpiryJob.cs` — add `/// <summary>` and CAT-5 `Await:` / CAT-6 `Acquire:` / CAT-9 `Log:` / CAT-7 `Catch:` labels | | |
| TASK-064 | `Module/Ordering/Backgrounds/CartExpiryJob.Scheduler.cs` — add `/// <summary>` and CAT-5 `Defer:` / CAT-5 `Throttle:` labels | | |
| TASK-065 | `Module/Ordering/Backgrounds/CartExpiryJob.Result.cs` — add `/// <summary>` on result types | | |
| TASK-066 | `Module/Ordering/Backgrounds/CartExpiryJob.Loggers.cs` — add `/// <summary>` and CAT-9 `Log:` labels | | |
| TASK-067 | `Module/Inventory/Backgrounds/ReservationExpiryJob.cs` — add `/// <summary>` and CAT-5 `Await:` / CAT-6 `Release:` / CAT-6 `Purge:` / CAT-9 `Log:` / CAT-7 `Catch:` labels | | |
| TASK-068 | `Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs` — add `/// <summary>` and CAT-8 `Webhook:` / CAT-5 `Retry:` / CAT-7 `Catch:` / CAT-9 `Log:` labels | | |
| TASK-069 | `Shared/Operational/Persistence/Initializers/DatabaseInitializerHostedService.cs` — add `/// <summary>` and CAT-2 `Initialize:` / CAT-7 `Catch:` / CAT-7 `Degrade:` labels | | |
| TASK-070 | `Shared/Operational/Backgrounds/` — audit and add XML doc/labels to any background service registrations | | |

### Implementation Phase 9: Middleware & Operational Pipeline

- GOAL-009: Add XML doc and structured labels to middleware and pipeline classes

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-071 | `SecurityHeadersMiddleware.cs` — add `/// <summary>` on class and `InvokeAsync`; add CAT-1 `Check:` / CAT-4 `Enforce:` / CAT-9 `Log:` labels; name security threats | | |
| TASK-072 | `CorrelationMiddleware.cs` — add `/// <summary>` on class and `InvokeAsync`; add CAT-2 `Initialize:` / CAT-9 `Trace:` labels | | |
| TASK-073 | `GlobalExceptionHandler.cs` — add `/// <summary>` on class and `TryHandleAsync`; add CAT-7 `Catch:` / `Escalate:` / CAT-9 `Log:` labels on each exception type caught | | |
| TASK-074 | `CorrelationIdPropagationHandler.cs` — add `/// <summary>` on class; add CAT-9 `Trace:` / CAT-2 `Assign:` labels | | |

## 3. Alternatives

- **ALT-001**: Use automated tool (StyleCop or roslyn-analyzers SA1600) to enforce doc comment presence — rejected because it would not add structured inline label content; only enforces presence/absence
- **ALT-002**: Big-bang rewrite all files at once — rejected because the codebase has ~770 module files + ~250 Shared files; phased approach per module is safer and allows incremental review
- **ALT-003**: Skip domain entity classes and only annotate service/handler classes — rejected because `// Invariant:` on entity classes is essential for AI agent reasoning per CAT-10 guidance
- **ALT-004**: Use AI bulk-generation of all comments — rejected because per the Semantic Density Principle (arXiv:2604.07502), human-curated annotations outperform LLM-generated ones; each comment must be manually verified for correctness

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` — the authoritative standard defining all label categories and formatting rules
- **DEP-002**: `guide/code-commenting/README.md` — human-readable reference for label selection and anti-pattern avoidance
- **DEP-003**: `dotnet build` with `TreatWarningsAsErrors=true` — must pass after each phase
- **DEP-004**: No external library dependencies — commenting is source-code-only metadata

## 5. Files

- **FILE-001** to **FILE-010**: 11 Inventory service files in `Module/Inventory/Services/` (see TASK-001 to TASK-011)
- **FILE-011** to **FILE-021**: 11 Catalog service files in `Module/Catalog/Features/Admin/Taxonomies/Taxons/Services/` + `Embeddings/Shared/` (see TASK-012 to TASK-022)
- **FILE-022** to **FILE-024**: 3 Ordering service files in `Module/Ordering/Services/` (see TASK-022 to TASK-024)
- **FILE-025** to **FILE-033**: 9 Payment service files in `Module/Payment/Services/` (see TASK-025 to TASK-033)
- **FILE-034** to **FILE-045**: 12 Shared infrastructure service files (see TASK-034 to TASK-045)
- **FILE-046** to **FILE-054**: ~255 CQRS handler files across all 9 modules (see TASK-046 to TASK-054)
- **FILE-055** to **FILE-062**: ~30 domain entity files across all modules (see TASK-055 to TASK-062)
- **FILE-063** to **FILE-070**: 8 background job files (see TASK-063 to TASK-070)
- **FILE-071** to **FILE-074**: 4 middleware/pipeline files (see TASK-071 to TASK-074)

## 6. Testing

- **TEST-001**: `dotnet build` — verify no warnings (TreatWarningsAsErrors=true). Must pass after each phase
- **TEST-002**: Manual review — verify each label follows the CommentingRules.xml decision tree and is not vague (AP-2 anti-pattern check)
- **TEST-003**: Manual review — verify CAT-10 annotations use `KEY=VALUE` form per F9 rule
- **TEST-004**: Manual review — verify no commented-out code remains (AP-6 check)
- **TEST-005**: Manual review — verify all public classes/methods have `/// <summary>` XML doc blocks

## 7. Risks & Assumptions

- **RISK-001**: Over-commenting (AP-3) — some files currently well-commented; agents may add redundant comments. Mitigation: follow the naming → structure → context → comments hierarchy (P2); skip adding labels where code is self-documenting
- **RISK-002**: Stale comments (AP-5) — existing comments may describe behaviour the code has outgrown. Mitigation: update stale comments when found, but scope creep is contained by strict per-file task boundaries
- **RISK-003**: Build warnings from new XML doc — `/// <param>` tags must match actual parameter names; `/// <exception>` must reference real exception types. Mitigation: verify with `dotnet build` after each file change
- **ASSUMPTION-001**: The codebase already follows structured inline label conventions (see `PaymentProcessingService.cs`, `CartExpiryService.cs`, `StockAvailabilityService.cs`); new comments must match existing style
- **ASSUMPTION-002**: Domain entity partial files (`.Method.cs`, `.Validation.cs`, `.Result.cs`) do not need full XML doc on every method — only the main class file gets the `/// <summary>` and `// Invariant:` annotation

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — authoritative standard (CAT-1 through CAT-10, Temporal Markers, XML doc specs)
- `guide/code-commenting/README.md` — human-readable overview with label decision tree and anti-patterns
- `guide/code-commenting/SKILL.md` — code-commenting skill with application workflow and minimum viable annotations per file type
- `guide/code-commenting/references/label-quick-reference.md` — full label table reference
- `guide/code-commenting/references/anti-patterns.md` — anti-pattern checklist for review
- `plan/feature-code-commenting-benchmarks-1.md` — companion plan for Python benchmarks commenting
