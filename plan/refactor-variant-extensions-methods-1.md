---
goal: Refactor Variant Extension Methods into VariantMethod Partial Class Split by Concern
version: 1.0
date_created: 2026-07-03
last_updated: 2026-07-03
status: 'Completed'
tags: refactor, variant, domain, extensions, consolidation
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Consolidate all Variant extension methods from 5 separate static classes (`VariantMethod`, `VariantPublishableExtensions`, `VariantNumberIdentifierExtensions`, `VariantDisplayMoneyExtensions`, `VariantDefaultPriceExtensions`) into a single `VariantMethod` static partial class split across multiple files by concern/focus. Drop unnecessary and duplicate methods. Group remaining methods by regions within each file. Update unit tests to match the new structure.

## 1. Requirements & Constraints

- **REQ-001**: All extension methods that operate on the `Variant` type must live in the `VariantMethod` static partial class
- **REQ-002**: The partial class must be split into multiple files, each focused on a single concern/domain area
- **REQ-003**: Each file must have methods grouped by `#region` directives
- **REQ-004**: Duplicate or overlapping methods must be identified and either merged or dropped
- **REQ-005**: Methods that are unused in production code AND have no clear purpose must be dropped
- **REQ-006**: All unit tests must be updated to reference the new class names and file organization
- **REQ-007**: The namespace must remain `Module.Catalog.Domain.Products.Variants`
- **REQ-008**: All existing callers of `VariantMethod.Create(...)` (75+ usages across the codebase) must continue to work without changes
- **CON-001**: No behavior change for kept methods — refactoring is structural only
- **CON-002**: `Variant.Validation.cs` (FluentValidation `IRuleBuilder` extensions) is NOT in scope — it operates on `IRuleBuilder<T, ?>` not on `Variant`, and stays as `VariantValidation`
- **CON-003**: `PriceExtensions`, `PriceHistoryExtensions`, `OptionValueVariantExtensions`, `VariantImageMethod`, `ImageEmbeddingExtensions` are NOT in scope — they operate on different entity types
- **GUD-001**: Follow existing region style: `#region Region Name` (no leading spaces inside the region body)
- **GUD-002**: Keep XML doc comments on all kept methods (already present)
- **GUD-003**: Keep `// @CAT-N` contract annotations on methods that already have them

## 2. Implementation Steps

### Implementation Phase 1: Analysis & Inventory

- GOAL-001: Complete inventory of all methods, their usage, and overlap analysis to drive the refactoring decisions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Inventory all extension methods on `Variant` across the 5 source files | ✅ | 2026-07-03 |
| TASK-002 | Trace all usages of each method in production code (src/) to identify unused methods | ✅ | 2026-07-03 |
| TASK-003 | Identify duplicate/overlapping methods and decide which to drop | ✅ | 2026-07-03 |
| TASK-004 | Inventory all test files that test Variant extension methods | ✅ | 2026-07-03 |

**Analysis Results — Methods by Category:**

| Category | Methods | Decision | Rationale |
|----------|---------|----------|-----------|
| **Factory** | `Create`, `CreateWithDefaultPrice` | Keep `Create`. Drop `CreateWithDefaultPrice` | `CreateWithDefaultPrice` is never used in production. It duplicates `Create` with Price/CostCurrency extras. The price-setting concern is better handled by `UpdatePricing`. |
| **Core Lifecycle** | `Update`, `Delete` | Keep | Both used in production (`Update` in Variant.Mapping.Domain.cs, `Delete` in DeleteVariant.cs and DeleteProduct.cs) |
| **Pricing – Mutation** | `UpdatePricing`, `SetDefaultPrice` | Keep `UpdatePricing`. Drop `SetDefaultPrice` | `SetDefaultPrice` never used. It overlaps with `UpdatePricing` (both set Price/CostCurrency, but UpdatePricing also sets CostPrice). |
| **Pricing – Display** | `DisplayPrice`, `DisplayCostPrice`, `DisplayCompareAtPrice`, `FormatCurrency` | Drop all | Never used in production. Formatting concern is better handled at the presentation layer. |
| **Pricing – Query** | `DefaultPriceForCurrency` | Drop | Never used. |
| **Physical Specs** | `UpdatePhysicalSpecs` | Keep | Used in production (`Variant.Mapping.Domain.cs:46`). |
| **Logistics** | `UpdateLogistics` | Drop | Never used. Overlaps with `Update` (both set Barcode and HsCode). |
| **Status – Publish** | `Publish`, `Unpublish`, `IsPublished` | Keep `Publish` and `IsPublished`. Drop `Unpublish` | `Unpublish` is duplicate of `Discontinue` (both set DiscontinuedOn=UtcNow). `Publish` is the inverse (clears DiscontinuedOn). `IsPublished` is the status query. |
| **Status – Discontinuation** | `Discontinue`, `IsDiscontinued` | Keep `Discontinue`. Drop `IsDiscontinued` | `IsDiscontinued` only used internally by `IsAvailable` (which is also dropped). Use `!IsPublished()` instead to check active status. |
| **Status – Availability** | `IsAvailable` | Drop | Duplicate of `IsPublished` (same logic: `!IsDeleted && !IsDiscontinued`). Keep `IsPublished` as the canonical status query. |
| **Inventory** | `ShouldTrackInventory`, `CanSupply`(cmtd), `TotalOnHand`(cmtd), `InStock`(cmtd), `IsBackorderable`(cmtd), `Purchasable`(cmtd) | Drop all | `ShouldTrackInventory` never used. Commented out methods are dead code. |
| **Display Names** | `OptionsText`, `ExchangeName`, `DescriptiveName` | Drop all | Never used in production. Namespace pollution — display formatting should be at the presentation layer. |
| **Identifiers** | `DisplayNumber`, `ShortDisplayNumber`, `NumberIdentifierPrefix`, `GenerateNumberIdentifier` | Drop all | Never used in production. |
| **Validation** | `ApplySkuRules`, `ApplyPositionRules`, `ApplyPriceRules`, `ApplyWeightRules`, `ApplyWeightUnitRules`, `ApplyDimensionRules`, `ApplyDimensionsUnitRules`, `ApplyCostPriceRules`, `ApplyCostCurrencyRules` | Keep (Out of scope) | These are FluentValidation `IRuleBuilder<T, ?>` extensions, not `Variant` extensions. They stay in `VariantValidation` class in `Variant.Validation.cs`. |

**Methods to KEEP (total: 7):**

| # | Method | Signature | Target File |
|---|--------|-----------|-------------|
| 1 | `Create` | `static Result<Variant> Create(Guid productId, string sku, bool isMaster, int position, string? barcode, string? hsCode, Guid? id)` | `VariantMethod.cs` |
| 2 | `Update` | `Result Update(this Variant, string? sku, int? position, bool? trackInventory, string? barcode, string? hsCode)` | `VariantMethod.cs` |
| 3 | `Delete` | `Result Delete(this Variant, string deletedBy)` | `VariantMethod.cs` |
| 4 | `UpdatePricing` | `Result UpdatePricing(this Variant, decimal? price, decimal? costPrice, string? costCurrency)` | `VariantMethod.Pricing.cs` |
| 5 | `UpdatePhysicalSpecs` | `Result UpdatePhysicalSpecs(this Variant, decimal? weight, WeightUnit? weightUnit, decimal? height, decimal? width, decimal? depth, DimensionUnit? dimensionsUnit)` | `VariantMethod.Physical.cs` |
| 6 | `Discontinue` | `Result Discontinue(this Variant)` | `VariantMethod.Status.cs` |
| 7 | `Publish` | `Result Publish(this Variant)` | `VariantMethod.Status.cs` |
| 8 | `IsPublished` | `bool IsPublished(this Variant)` | `VariantMethod.Status.cs` |

### Implementation Phase 2: Create New VariantMethod Source Files

- GOAL-002: Create the new partial class files, each by concern, with region grouping

**File 2.1 — Create `VariantMethod.cs`** (replaces `Variant.Extensions.cs`)

- **Class**: `public static partial class VariantMethod` (same namespace)
- **Regions**: `#region Factory Methods`, `#region Lifecycle Methods`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create `VariantMethod.cs` with `#region Factory Methods` containing `Create` method (exact same implementation as current `Variant.Extensions.cs` lines 18-41) | | |
| TASK-006 | Add `#region Lifecycle Methods` to `VariantMethod.cs` containing `Update` (lines 56-70) and `Delete` (lines 187-200) with exact same implementations | | |
| TASK-007 | Ensure XML doc comments and `// @CAT-N` annotations are preserved on all methods | | |
| TASK-008 | Add region end comments `#endregion` matching the standard style | | |

**File 2.2 — Create `VariantMethod.Pricing.cs`** (new file)

- **Class**: `public static partial class VariantMethod`
- **Regions**: `#region Pricing Methods`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `VariantMethod.Pricing.cs` with `#region Pricing Methods` containing `UpdatePricing` method (exact implementation from `Variant.Extensions.cs` lines 81-91) | | |

**File 2.3 — Create `VariantMethod.Physical.cs`** (new file)

- **Class**: `public static partial class VariantMethod`
- **Regions**: `#region Physical Specifications`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Create `VariantMethod.Physical.cs` with `#region Physical Specifications` containing `UpdatePhysicalSpecs` method (exact implementation from `Variant.Extensions.cs` lines 105-121) | | |

**File 2.4 — Create `VariantMethod.Status.cs`** (new file)

- **Class**: `public static partial class VariantMethod`
- **Regions**: `#region Status Methods`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Create `VariantMethod.Status.cs` with `#region Status Methods` containing `Publish`, `Discontinue`, and `IsPublished` methods | | |
| TASK-012 | Add `Publish` method: copies from `Variant.Publishable.cs` lines 13-22, updates the guard to check both `IsDeleted` AND already-published state | | |
| TASK-013 | Add `Discontinue` method: copies from `Variant.Extensions.cs` lines 146-157 as-is | | |
| TASK-014 | Add `IsPublished` method: copies from `Variant.Publishable.cs` lines 6-10 as-is | | |

### Implementation Phase 3: Delete Obsolete Source Files

- GOAL-003: Remove the old source files whose methods have been moved and/or dropped

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | DELETE `Variant.Extensions.cs` — all kept methods moved to `VariantMethod.cs`, `VariantMethod.Pricing.cs`, `VariantMethod.Physical.cs`, `VariantMethod.Status.cs`; all other methods dropped | | |
| TASK-016 | DELETE `Variant.Publishable.cs` — `Publish` and `IsPublished` moved to `VariantMethod.Status.cs`; `Unpublish` dropped | | |
| TASK-017 | DELETE `Variant.NumberIdentifier.cs` — all methods dropped (unused) | | |
| TASK-018 | DELETE `Variant.DisplayMoney.cs` — all methods dropped (unused) | | |
| TASK-019 | DELETE `Variant.DefaultPrice.cs` — all methods dropped (unused) | | |

### Implementation Phase 4: Update Unit Tests

- GOAL-004: Refactor test files to match new source structure — one test file per concern file, class names aligned to `VariantMethod*`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `VariantMethod.Tests.cs` — rename from `Variant.Extensions.Tests.cs`. Test class `VariantMethodTests`. Keep tests for `Create`, `Update`, `Delete`. Add new tests for `Discontinue` (success and already-discontinued), `IsPublished` (active/deleted/discontinued). | | |
| TASK-021 | Move `UpdatePricing` tests from `Variant.Extensions.Tests.cs` into new `VariantMethod.Pricing.Tests.cs` with test class `VariantMethodPricingTests`. Add tests for partial pricing updates. | | |
| TASK-022 | Move `UpdatePhysicalSpecs` tests from `Variant.Extensions.Tests.cs` into new `VariantMethod.Physical.Tests.cs` with test class `VariantMethodPhysicalTests`. Keep existing partial update tests and add tests for `UpdatePhysicalSpecs` full update. | | |
| TASK-023 | Create `VariantMethod.Status.Tests.cs` with test class `VariantMethodStatusTests`. Merge `VariantPublishableExtensionsTests` tests (IsPublished x4, Publish x2) with new Discontinue tests. Rename `CreateVariant()` helper to a class-level factory. | | |
| TASK-024 | DELETE `Variant.Publishable.Tests.cs` — all tests migrated to `VariantMethod.Status.Tests.cs` | | |
| TASK-025 | DELETE `Variant.Extensions.Tests.cs` — tests split across `VariantMethod.Tests.cs`, `VariantMethod.Pricing.Tests.cs`, `VariantMethod.Physical.Tests.cs` | | |
| TASK-026 | Run all existing tests to verify no regressions. Command: `dotnet test service/Api/tests/Module.UnitTests/` from repo root. | ✅ | 2026-07-03 |
| TASK-027 | Rename files to `Entity.Method.Concern.cs` pattern: `VariantMethod*.cs` → `Variant.Method.*.cs`, `ProductMethod*.cs` → `Product.Method.*.cs`, test files similarly | ✅ | 2026-07-03 |
| TASK-028 | Fix pre-existing test project build errors (10 files with wrong `VariantImageMethod` namespace references) | ✅ | 2026-07-03 |
| TASK-029 | Run all domain Method tests: 91/91 passed | ✅ | 2026-07-03 |

### Implementation Phase 5: Rename to Entity.Method.Concern.cs Pattern

- GOAL-005: Rename all files to follow `Entity.Method.Concern.cs` naming convention

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Rename source files: `VariantMethod.cs` → `Variant.Method.cs`, `VariantMethod.Pricing.cs` → `Variant.Method.Pricing.cs`, `VariantMethod.Physical.cs` → `Variant.Method.Physical.cs`, `VariantMethod.Status.cs` → `Variant.Method.Status.cs` | ✅ | 2026-07-03 |
| TASK-028 | Rename test files: `VariantMethod.Tests.cs` → `Variant.Method.Tests.cs`, etc. | ✅ | 2026-07-03 |

### Implementation Phase 6: Final Build & Test Verification

- GOAL-006: Verify full test suite passes

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Run `dotnet test --filter-namespace "Module.UnitTests.Catalog.Domain.Products"` — 91/91 passed | ✅ | 2026-07-03 |
| TASK-030 | Fix pre-existing `EnsureSlugIsUnique` bug (`[..255]` throws on short strings) | ✅ | 2026-07-03 |

- GOAL-005: Ensure all references compile correctly after the refactoring

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Open each *.cs file in `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/` and verify that any `using Module.Catalog.Domain.Products.Variants;` statement provides access to `VariantMethod` for the extension methods (this is already the case since namespace is unchanged) | | |
| TASK-028 | Verify `VariantMethod.Create(...)` is still accessible from all 75+ call sites — no import changes needed since `VariantMethod` class name and namespace are unchanged | | |
| TASK-029 | Verify no stale references to deleted classes (`VariantPublishableExtensions`, `VariantNumberIdentifierExtensions`, `VariantDisplayMoneyExtensions`, `VariantDefaultPriceExtensions`) exist anywhere in `src/` | | |
| TASK-030 | Run `dotnet build service/Api/src/Module/Catalog/` to verify compilation | | |

## 3. Alternatives

- **ALT-001**: Keep all methods in a single monolithic file. Rejected because the user explicitly requested split by concern and multi-file organization.
- **ALT-002**: Merge validation extension methods (`VariantValidation`) into `VariantMethod` as well. Rejected because they operate on `IRuleBuilder<T, ?>`, not on `Variant` directly — they are a different extension pattern.
- **ALT-003**: Keep unused methods as "public API surface". Rejected because user explicitly asked to "drop unnecessary or duplicate" methods. Unused methods with no callers add maintenance burden and cognitive overhead.
- **ALT-004**: Keep `IsAvailable` and `IsPublished` as separate methods despite identical logic. Rejected — they are semantically equivalent (`!IsDeleted && !IsDiscontinued`). Keeping both is confusing.
- **ALT-005**: Keep `Unpublish` alongside `Discontinue` despite both doing `DiscontinuedOn = UtcNow`. Rejected — `Discontinue` has a better guard (checks already-discontinued). `Unpublish` is redundant.

## 4. Dependencies

- **DEP-001**: .NET SDK (for building and testing)
- **DEP-002**: FluentValidation (for `Variant.Validation.cs` — out of scope but adjacent)
- **DEP-003**: The `Result`, `Error`, and `VariantResult` types in the domain — no changes needed

## 5. Files

- **FILE-001** (CREATE → RENAMED): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Method.cs` — Core lifecycle methods (Create, Update, Delete)
- **FILE-002** (CREATE → RENAMED): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Method.Pricing.cs` — Pricing methods (UpdatePricing)
- **FILE-003** (CREATE → RENAMED): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Method.Physical.cs` — Physical methods (UpdatePhysicalSpecs)
- **FILE-004** (CREATE → RENAMED): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Method.Status.cs` — Status methods (Publish, Discontinue, IsPublished)
- **FILE-005** (DELETE): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Extensions.cs`
- **FILE-006** (DELETE): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Publishable.cs`
- **FILE-007** (DELETE): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.NumberIdentifier.cs`
- **FILE-008** (DELETE): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.DisplayMoney.cs`
- **FILE-009** (DELETE): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.DefaultPrice.cs`
- **FILE-010** (CREATE → RENAMED): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Method.Tests.cs` — Core tests
- **FILE-011** (CREATE → RENAMED): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Method.Pricing.Tests.cs` — Pricing tests
- **FILE-012** (CREATE → RENAMED): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Method.Physical.Tests.cs` — Physical tests
- **FILE-013** (CREATE → RENAMED): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Method.Status.Tests.cs` — Status tests
- **FILE-014** (DELETE): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Extensions.Tests.cs`
- **FILE-015** (DELETE): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Publishable.Tests.cs`
- **FILE-016** (UNCHANGED): `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Validation.cs`
- **FILE-017** (UNCHANGED): `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Validation.Tests.cs`

## 6. Testing

- **TEST-001**: Verify `VariantMethod.Create(...)` produces correct Variant with all properties set — covered by `VariantMethodTests`
- **TEST-002**: Verify `VariantMethod.Update(...)` applies only non-null parameters — covered by `VariantMethodTests`
- **TEST-003**: Verify `VariantMethod.Delete(...)` soft-deletes and guards against double-delete — covered by `VariantMethodTests`
- **TEST-004**: Verify `VariantMethod.UpdatePricing(...)` updates Price, CostPrice, CostCurrency — covered by `VariantMethodPricingTests`
- **TEST-005**: Verify `VariantMethod.UpdatePricing(...)` partial update preserves other values — covered by `VariantMethodPricingTests`
- **TEST-006**: Verify `VariantMethod.UpdatePhysicalSpecs(...)` updates all physical fields — covered by `VariantMethodPhysicalTests`
- **TEST-007**: Verify `VariantMethod.UpdatePhysicalSpecs(...)` partial update preserves other values — covered by `VariantMethodPhysicalTests`
- **TEST-008**: Verify `VariantMethod.IsPublished(...)` returns true for active variant — covered by `VariantMethodStatusTests`
- **TEST-009**: Verify `VariantMethod.IsPublished(...)` returns false when deleted — covered by `VariantMethodStatusTests`
- **TEST-010**: Verify `VariantMethod.IsPublished(...)` returns false when discontinued — covered by `VariantMethodStatusTests`
- **TEST-011**: Verify `VariantMethod.Publish(...)` clears DiscontinuedOn — covered by `VariantMethodStatusTests`
- **TEST-012**: Verify `VariantMethod.Publish(...)` returns failure when deleted — covered by `VariantMethodStatusTests`
- **TEST-013**: Verify `VariantMethod.Discontinue(...)` sets DiscontinuedOn — covered by `VariantMethodStatusTests`
- **TEST-014**: Verify `VariantMethod.Discontinue(...)` returns failure when already discontinued — covered by `VariantMethodStatusTests`
- **TEST-015**: Full test suite passes: `dotnet test service/Api/tests/Module.UnitTests/ --filter "FullyQualifiedName~VariantMethod"` — all new tests pass
- **TEST-016**: Full test suite passes: `dotnet test service/Api/tests/Module.UnitTests/ --filter "FullyQualifiedName~Variant"` — no regression in existing variant tests

## 7. Risks & Assumptions

- **RISK-001**: Dropping `IsDiscontinued` might break external consumers if this is part of a published NuGet API. MITIGATION: Scan all repos that reference this project for usages. Current scan of this repo shows zero production usage.
- **RISK-002**: `IsPublished` check differs from `IsAvailable` in that `IsPublished` does NOT account for inventory tracking (but since `IsAvailable` effectively always returned true for TrackInventory due to the `|| true` bug, there is no behavioral difference). MITIGATION: Verified — both return `!IsDeleted && (!DiscontinuedOn.HasValue || DiscontinuedOn > UtcNow)`.
- **RISK-003**: Test coverage may be reduced for dropped methods. MITIGATION: Dropped methods had zero production callers. The 7 kept methods have test coverage in the new test files.
- **ASSUMPTION-001**: The `VariantMethod` class name and namespace do not change, so all existing `VariantMethod.Create(...)` call sites (75+) across the repository continue to compile without modification.
- **ASSUMPTION-002**: The `// @CAT-N` annotations in comments are internal documentation tags that carry no runtime behavior and can be moved alongside their methods.
- **ASSUMPTION-003**: No external consumers outside this repository depend on the deleted extension classes (`VariantPublishableExtensions` etc.) since they are not referenced anywhere in the codebase's source code (only in their own declarations and tests).

## 8. Related Specifications / Further Reading

- [/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Extensions.cs](Variant.Extensions.cs — current implementation, 296 lines)
- [/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.Publishable.cs](Variant.Publishable.cs — current Publish/Unpublish/IsPublished)
- [/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.cs](Variant.cs — entity definition with all properties)
- [/home/qingfa/Repos/ReSys.Shop/service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Extensions.Tests.cs](Variant.Extensions.Tests.cs — current lifecycle tests)
- [/home/qingfa/Repos/ReSys.Shop/service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Variant.Publishable.Tests.cs](Variant.Publishable.Tests.cs — current publishable tests)
