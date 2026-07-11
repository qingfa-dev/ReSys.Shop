---
goal: Adopt Code Commenting Standard v3.0 across all Query Handlers, Command Handlers, and Service Implementations
version: 1.0
date_created: 2026-07-11
status: Planned
tags: refactor, chore, documentation, commenting
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Apply the Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to all ~350 C# handler and service files across 9 modules and 6 Shared pillars. Fixes non-standard labels, missing inline annotations, weak doc comments, and absent agent annotations. Based on audit sampling of 12+ files across Catalog, Identity, Inventory, Ordering, Payment, Profile, and Shared.

## 1. Requirements & Constraints

- **REQ-001**: Every public `Handle` method on `ICommandHandler<T>` and `IQueryHandler<T,R>` must carry a `Contract:` label with `pre=`/`post=`/`throws=` clauses
- **REQ-002**: Every public service method must carry a `Contract:` label
- **REQ-003**: Non-standard labels (`Persist:`, `Query:`, `Process:`) must be replaced with v3.0 standard equivalents
- **REQ-004**: Legacy `@CAT-N Label:` format must be replaced with standard `// Label:` format
- **REQ-005**: Doc comments must have substantive `<summary>` (not "Handles the command"), all `<param>` descriptions, all `<exception>` tags
- **REQ-006**: Domain service classes must carry `Invariant:` annotations
- **REQ-007**: External integration boundaries must carry `Webhook:`, `Call:`, or `Receive:` labels with system name and contract version
- **GUD-001**: Follow `guide/code-commenting/SKILL.md` label decision tree for every label choice
- **GUD-002**: Apply `F10` — imperative-mood verbs only
- **GUD-003**: Apply `F8` — one label, one action
- **GUD-004**: Apply `P6` — Semantic Density; every token earns its place
- **CON-001**: Build must remain clean (`/warningsaserrors`); no accidental syntax changes
- **CON-002**: No behavior changes — comments only, zero code modification
- **CON-003**: Preserve existing `Contract:` and `Invariant:` labels that are already correct

## 2. Implementation Steps

### Implementation Phase 1 — Audit & Label-Migration Pattern (Catalog)

- GOAL-001: Fix 2 most-anomalous patterns in Catalog (non-standard `Persist:` label, missing comments in TaxonHierarchyService) to establish repeatable pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001-AUDIT | Audit all 44 catalog handler + 5 service files; catalog violations by type (non-standard labels, missing Contract, missing Invariant, weak doc comments) | | |
| TASK-001-FIX-PERSIST | Replace `// Persist:` with `// Save:` (new CAT-2 label) in CreateProduct, AddVariant, and any catalog files using it | | |
| TASK-001-FIX-CONTRACT | Add `Contract:` pre/post/throws to CommandHandler/QueryHandler Handle methods in all 44 catalog handler files | | |
| TASK-001-FIX-SERVICE | Add `Contract:` + inline labels to TaxonHierarchyService (5 partial files), ImageEmbedding.Orchestrator, AutoClassificationService | | |
| TASK-001-FIX-DOC | Strengthen doc comments: replace "Handles X" summaries with behavioral descriptions; add exception tags | | |
| TASK-001-VERIFY | `dotnet build service/Api/Api.sln` — zero warnings | | |

### Implementation Phase 2 — Inventory

- GOAL-002: Bring all 23 inventory handlers + 7 services into v3.0 compliance

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002-AUDIT | Audit all 23 handler + 7 service files in Inventory | | |
| TASK-002-FIX-CONTRACT | Add `Contract:` labels to Handle methods in all inventory handlers | | |
| TASK-002-FIX-SERVICE | Add full commenting to 7 service files (StockReservationService, StockQuantityService, CartReservationService, StockSummaryService, StockRestockService, StockAvailabilityService, ReservationExpiryService) | | |
| TASK-002-FIX-LABELS | Replace any `Persist:` → `Save:` and fix `@CAT-` legacy format | | |
| TASK-002-FIX-DOC | Strengthen doc comments across all inventory files | | |
| TASK-002-VERIFY | `dotnet build service/Api/Api.sln` — zero warnings | | |
| TASK-002-AGENT | Add `Boundary:` annotations at integration points (CartReservationService → Ordering) | | |

### Implementation Phase 3 — Identity

- GOAL-003: Bring all 31 identity handlers + 0 services into v3.0 compliance

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003-AUDIT | Audit all 31 handler files in Identity | | |
| TASK-003-FIX-CONTRACT | Add `Contract:` labels to Handle methods in all identity handlers | | |
| TASK-003-FIX-LABELS | Replace `Query:` → `Load:`; replace `Persist:` → `Save:`; fix `@CAT-` format | | |
| TASK-003-FIX-DOC | Strengthen doc comments (EmailRegister, ResetPassword, etc. have weak summaries) | | |
| TASK-003-VERIFY | `dotnet build service/Api/Api.sln` — zero warnings | | |

### Implementation Phase 4 — Ordering

- GOAL-004: Bring all 31 ordering handlers + 5 services into v3.0 compliance

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004-AUDIT | Audit all 31 handler + 5 service files in Ordering | | |
| TASK-004-FIX-CONTRACT | Add `Contract:` labels to Handle methods in all ordering handlers | | |
| TASK-004-FIX-SERVICE | Add `Contract:` + inline labels to CartExpiryService, OrderInventoryService, OrderMerger, OrderUpdater, OrderContents | | |
| TASK-004-FIX-LEGACY | Fix `@CAT-N` format in OrderMerger (`@CAT-5 Compute:` → `// Compute:`) | | |
| TASK-004-FIX-LABELS | Replace any `Persist:` → `Save:` in ordering files | | |
| TASK-004-VERIFY | `dotnet build service/Api/Api.sln` — zero warnings | | |
| TASK-004-AGENT | Add `Invariant:` to OrderMerger, OrderUpdater domain service classes | | |

### Implementation Phase 5 — Payment, Profile, Shipping, Webhooks, Location

- GOAL-005: Bring remaining 5 modules (85+ handlers + 1 service) into v3.0 compliance

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005-AUDIT | Audit all files in Payment (15), Profile (19), Shipping (12), Webhooks (7), Location (12) | | |
| TASK-005-FIX-CONTRACT | Add `Contract:` labels to Handle methods across all 5 modules | | |
| TASK-005-FIX-LABELS | Replace non-standard labels; fix `Process:` → specific label in StripeWebhook.cs | | |
| TASK-005-FIX-WEBHOOK | Add `Webhook:` with system name "Stripe v2025-02" in StripeWebhook.cs | | |
| TASK-005-FIX-DOC | Strengthen doc comments across all 5 modules | | |
| TASK-005-VERIFY | `dotnet build service/Api/Api.sln` — zero warnings | | |

### Implementation Phase 6 — Shared (Security, Operational, Performance, Application, Governance, Observability)

- GOAL-006: Bring all ~30 Shared service implementations into v3.0 compliance

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006-AUDIT | Audit all files: Security (~10), Operational (~12), Performance (~2), Application (~2), Governance, Observability | | |
| TASK-006-FIX-CONTRACT | Add `Contract:` labels to all public service methods in Shared implementations | | |
| TASK-006-FIX-IMPL | Add inline labels to: AccessToken, RefreshToken, TokenBlacklist, Permission, Cache, Storage, Notification, ImageProcessor, S3Provider, AntiForgeryGuard, MalwareScanner | | |
| TASK-006-FIX-BOUNDARY | Add `Boundary:` annotations at Shared→Module transition points | | |
| TASK-006-AGENT | Add `Invariant:` and `Context:` for security-critical services (TokenBlacklist, AntiForgeryGuard) | | |
| TASK-006-VERIFY | `dotnet build service/Api/Api.sln` — zero warnings | | |

### Implementation Phase 7 — Final Verification

- GOAL-007: Comprehensive final audit and quality gate

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007-FULL-BUILD | `dotnet build service/Api/Api.sln` — zero warnings | | |
| TASK-007-TESTS | `dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests` — all passing | | |
| TASK-007-GREP-AUDIT | `rg "// (Persist|Query|Process):" service/Api/src/` — confirms zero non-standard labels remain | | |
| TASK-007-GREP-LEGACY | `rg "@CAT-" service/Api/src/` — confirms zero legacy `@CAT-N` format labels remain | | |
| TASK-007-GREP-CONTRACT | `rg "// Contract:" service/Api/src/Module/*/Features/*/*/` — confirms coverage on all handlers | | |

## 3. Alternatives

- **ALT-001**: Auto-fix with regex script — rejected because label choice requires semantic understanding; a script would mislabel and create AP-5 stale comments
- **ALT-002**: Big-bang rewrite — rejected because 350+ files in one pass is too large for reliable review; modular per-phase approach allows per-module verification
- **ALT-003**: Only fix new code / PR review gate — rejected because current codebase has 4 distinct anti-patterns (Persist, @CAT-N, missing Contract, weak doc) that compound when AI agents or humans edit the files later

## 4. Dependencies

- **DEP-001**: All changes are comments-only; zero NuGet or build toolchain dependencies
- **DEP-002**: Build verification requires `dotnet build` (must complete with zero warnings)

## 5. Files

- **FILE-001** through **FILE-091**: All 91 Query Handler files in `service/Api/src/Module/*/Features/**/Get*/*.cs`
- **FILE-092** through **FILE-266**: All ~175 Command Handler files in `service/Api/src/Module/*/Features/**/*.cs` (excluding Get paths)
- **FILE-267** through **FILE-282**: All ~16 Module Domain/Services files
- **FILE-283** through **FILE-312**: All ~30 Shared service implementation files

## 6. Testing

- **TEST-001**: `dotnet build service/Api/Api.sln` passes with zero warnings after each phase
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` passes (no behavior change)
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` passes (no behavior change)
- **TEST-004**: `rg "// (Persist|Query|Process):"` returns zero matches after Phase 7
- **TEST-005**: `rg "@CAT-"` returns zero matches across `service/Api/src/`
- **TEST-006**: `rg "// Contract:" service/Api/src/Module/*/Features/*/*/*.cs | wc -l` >= total handler count

## 7. Risks & Assumptions

- **RISK-001**: Accidental code change during comment editing — mitigated by per-phase `dotnet build` verification
- **RISK-002**: Inconsistent label selection by different editors — mitigated by REQ rules and the label decision tree in SKILL.md
- **RISK-003**: Missed files in large modules (Catalog, Ordering) — mitigated by grep-audit tasks at end of each phase
- **ASSUMPTION-001**: All 350+ files follow similar patterns to the 12 sampled files (>90% confidence)
- **ASSUMPTION-002**: Pre-existing build failures (AppDbContext RollbackTransactionAsync) remain unrelated — will not mask new issues

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — full v3.0 standard
- `guide/code-commenting/SKILL.md` — label decision tree and workflows
- `guide/code-commenting/README.md` — human-readable reference
- `plan/refactor-inline-error-declarations-1.md` — sibling refactoring plan
