---
goal: Refactor Product Extension Methods into ProductMethod Partial Class Split by Concern
version: 1.0
date_created: 2026-07-03
last_updated: 2026-07-03
status: 'Completed'
tags: refactor, product, domain, extensions, consolidation
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Consolidate all Product extension methods from 4 separate static classes (`ProductMethod`, `ProductScopesExtensions`, `ProductSlugsExtensions`, `ProductSearchableExtensions`) into a single `ProductMethod` static partial class split across 6 files by concern/focus. Drop duplicate `Available` (identical to `IsAvailable`). Group remaining methods by regions within each file. Update unit tests to match the new structure.

## 1. Requirements & Constraints

- **REQ-001**: All extension methods that operate on the `Product` type must live in the `ProductMethod` static partial class
- **REQ-002**: The partial class must be split into multiple files, each focused on a single concern
- **REQ-003**: Each file must have methods grouped by `#region` directives
- **REQ-004**: `Available` method is an exact duplicate of `IsAvailable` (identical logic) — must be dropped
- **REQ-005**: Commented-out `CanSupply` method must be dropped (dead code)
- **REQ-006**: All other methods kept with exact same implementation — refactoring is structural only
- **REQ-007**: The namespace must remain `Module.Catalog.Domain.Products`
- **REQ-008**: All existing callers of `ProductMethod.Create(...)` must continue to work
- **CON-001**: `Product.Validation.cs` (FluentValidation IRuleBuilder extensions) is NOT in scope
- **CON-002**: Follow existing region style: `#region Region Name`

### Implementation Pattern

This plan follows the exact same pattern established by `plan/refactor-variant-extensions-methods-1.md`:
- Make `ProductMethod` a `public static partial class` (was `public static class`)
- Split by concern into 6 files
- Each file has `#region` grouping
- Drop unused/duplicate methods

## 2. Implementation Steps

### Implementation Phase 1: Create Source Files

- GOAL-001: Create 6 new `ProductMethod` partial class files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `ProductMethod.cs` with `#region Factory Methods` (Create) and `#region Lifecycle Methods` (Update, Delete). Copy exact implementations from `Product.Extensions.cs` lines 26-61 (Create), 82-108 (Update), 225-237 (Delete). | | |
| TASK-002 | Create `ProductMethod.Status.cs` with `#region Status Methods`. Copy Activate (lines 116-129), Archive (lines 137-148), Draft (lines 155-166), Discontinue (lines 174-185), ChangeStatus (lines 206-216). | | |
| TASK-003 | Create `ProductMethod.Availability.cs` with `#region Availability Queries`. Copy IsAvailable (lines 193-198), DefaultVariant (lines 257-262), HasVariants (lines 270-273). DROP `Available` (lines 281-286 — exact duplicate of IsAvailable). | | |
| TASK-004 | Create `ProductMethod.Scopes.cs` with `#region Scope Queries`. Copy all 8 methods from `ProductScopesExtensions` (Product.Scopes.cs lines 6-70): IsDraft, IsActive, IsArchived, IsOnSale, IsPurchasable, IsBackorderable, IsInStock, ResolveStatus. Change class declaration to `public static partial class ProductMethod`. | | |
| TASK-005 | Create `ProductMethod.Slugs.cs` with `#region Slug Generation` and `#region Slug Validation`. Copy all 5 methods from `ProductSlugsExtensions` (Product.Slugs.cs lines 8-71): GenerateSlug, GenerateSlugFromName, IsSlugAvailable, EnsureSlugIsUnique, NormalizeSlug. Keep `using System.Text.RegularExpressions;`. | | |
| TASK-006 | Create `ProductMethod.Searchable.cs` with `#region Search Methods`. Copy all 3 methods from `ProductSearchableExtensions` (Product.Searchable.cs lines 6-50): SearchIndexText, SearchTokens, MatchesSearchQuery. | | |

### Implementation Phase 2: Delete Obsolete Files

- GOAL-002: Remove old source files whose methods have been moved and/or dropped

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | DELETE `Product.Extensions.cs` — all kept methods moved to ProductMethod.cs, Status.cs, Availability.cs | | |
| TASK-008 | DELETE `Product.Scopes.cs` — all methods moved to ProductMethod.Scopes.cs | | |
| TASK-009 | DELETE `Product.Slugs.cs` — all methods moved to ProductMethod.Slugs.cs | | |
| TASK-010 | DELETE `Product.Searchable.cs` — all methods moved to ProductMethod.Searchable.cs | | |

### Implementation Phase 3: Update Unit Tests

- GOAL-003: Refactor test files to match new source structure

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | RENAME `Product.Extensions.Tests.cs` to `ProductMethod.Tests.cs`. Keep test class `ProductMethodTests`. Keep all existing tests (Create, Update, ChangeStatus, Delete). | | |
| TASK-012 | Create `ProductMethod.Status.Tests.cs` with `ProductMethodStatusTests`. Add tests: Activate (success, already-active, archived), Archive (success, already-archived), Draft (success, already-draft), Discontinue (success, already-discontinued). | | |
| TASK-013 | Create `ProductMethod.Availability.Tests.cs` with `ProductMethodAvailabilityTests`. Add tests: IsAvailable (active, deleted, future-available-on), DefaultVariant (non-master preferred, master fallback, no variants), HasVariants (true, false). | | |
| TASK-014 | Create `ProductMethod.Scopes.Tests.cs` with `ProductMethodScopesTests`. Add tests: IsDraft, IsActive, IsArchived, IsOnSale, IsPurchasable, IsBackorderable, IsInStock, ResolveStatus. | | |
| TASK-015 | Create `ProductMethod.Slugs.Tests.cs` with `ProductMethodSlugsTests`. Add tests: GenerateSlug, GenerateSlugFromName, IsSlugAvailable, EnsureSlugIsUnique, NormalizeSlug. | | |
| TASK-016 | Create `ProductMethod.Searchable.Tests.cs` with `ProductMethodSearchableTests`. Add tests: SearchIndexText, SearchTokens, MatchesSearchQuery. | | |

### Implementation Phase 4: Build Verification

- GOAL-004: Verify compilation succeeds with 0 errors

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Run `dotnet build service/Api/src/Module/` and confirm 0 errors | ✅ | 2026-07-03 |
| TASK-018 | Rename files to `Entity.Method.Concern.cs` pattern: `ProductMethod*.cs` → `Product.Method.*.cs`, test files similarly | ✅ | 2026-07-03 |
| TASK-019 | Run all domain Method tests: 91/91 passed | ✅ | 2026-07-03 |

## 3. Alternatives

- **ALT-001**: Keep scopes/slugs/searchable in separate extension classes. Rejected — user explicitly requested consolidation into `ProductMethod` partial class, same as Variant pattern.
- **ALT-002**: Keep `Available` as alias for `IsAvailable`. Rejected — identical code, causes confusion. Drop it.

## 4. Dependencies

- **DEP-001**: .NET SDK
- **DEP-002**: `System.Text.RegularExpressions` (for slug generation — keep using directive in Slugs file)

## 5. Files

- **FILE-001** (CREATE → RENAMED): `Product.Method.cs` — Core: Create, Update, Delete
- **FILE-002** (CREATE → RENAMED): `Product.Method.Status.cs` — Status: Activate, Archive, Draft, Discontinue, ChangeStatus
- **FILE-003** (CREATE → RENAMED): `Product.Method.Availability.cs` — Availability: IsAvailable, DefaultVariant, HasVariants
- **FILE-004** (CREATE → RENAMED): `Product.Method.Scopes.cs` — Scopes: IsDraft, IsActive, IsArchived, IsOnSale, IsPurchasable, IsBackorderable, IsInStock, ResolveStatus
- **FILE-005** (CREATE → RENAMED): `Product.Method.Slugs.cs` — Slugs: GenerateSlug, GenerateSlugFromName, IsSlugAvailable, EnsureSlugIsUnique, NormalizeSlug
- **FILE-006** (CREATE → RENAMED): `Product.Method.Searchable.cs` — Search: SearchIndexText, SearchTokens, MatchesSearchQuery
- **FILE-007** (DELETE): `Product.Extensions.cs`
- **FILE-008** (DELETE): `Product.Scopes.cs`
- **FILE-009** (DELETE): `Product.Slugs.cs`
- **FILE-010** (DELETE): `Product.Searchable.cs`
- **FILE-011** (RENAME): `Product.Extensions.Tests.cs` → `Product.Method.Tests.cs`
- **FILE-012** (CREATE → RENAMED): `Product.Method.Status.Tests.cs`
- **FILE-013** (CREATE → RENAMED): `Product.Method.Availability.Tests.cs`
- **FILE-014** (CREATE → RENAMED): `Product.Method.Scopes.Tests.cs`
- **FILE-015** (CREATE → RENAMED): `Product.Method.Slugs.Tests.cs`
- **FILE-016** (CREATE → RENAMED): `Product.Method.Searchable.Tests.cs`

## 6. Testing

- **TEST-001**: `ProductMethodTests` — Create, Update (full + partial), ChangeStatus (new + same), Delete (new + already-deleted)
- **TEST-002**: `ProductMethodStatusTests` — Activate, Archive, Draft, Discontinue (success + guard failure for each)
- **TEST-003**: `ProductMethodAvailabilityTests` — IsAvailable (3 states), DefaultVariant (3 scenarios), HasVariants (2 scenarios)
- **TEST-004**: `ProductMethodScopesTests` — IsDraft, IsActive, IsArchived, IsOnSale, IsPurchasable, IsBackorderable, IsInStock, ResolveStatus
- **TEST-005**: `ProductMethodSlugsTests` — GenerateSlug, GenerateSlugFromName, IsSlugAvailable, EnsureSlugIsUnique, NormalizeSlug
- **TEST-006**: `ProductMethodSearchableTests` — SearchIndexText, SearchTokens, MatchesSearchQuery

## 7. Risks & Assumptions

- **RISK-001**: Dropping `Available` might break external consumers. MITIGATION: No production usage found. IsAvailable provides identical logic.
- **ASSUMPTION-001**: The `ProductMethod` class name and namespace do not change, so all existing callers continue working.

## 8. Related Specifications / Further Reading

- [plan/refactor-variant-extensions-methods-1.md](Same pattern used for Variant refactoring)
- [Product.Extensions.cs](Product.Extensions.cs — 288 lines, current monolithic ProductMethod class)
- [Product.Scopes.cs](Product.Scopes.cs — ProductScopesExtensions, 71 lines)
- [Product.Slugs.cs](Product.Slugs.cs — ProductSlugsExtensions, 72 lines)
- [Product.Searchable.cs](Product.Searchable.cs — ProductSearchableExtensions, 51 lines)
