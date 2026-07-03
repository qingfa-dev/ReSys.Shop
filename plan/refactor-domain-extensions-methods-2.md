---
goal: Convert All Domain Extension Classes to XxxMethod Naming Convention
version: 1.0
date_created: 2026-07-03
status: 'Completed'
tags: refactor, domain, extensions, consolidation
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Rename 6 remaining extension classes to the `XxxMethod` naming convention (matching the established `ProductMethod`, `VariantMethod`, `VariantImageMethod`, `ProductOptionTypeMethod` pattern). Rename all affected source files, test files, and update production call sites. No behavioral changes — structural only.

## 1. Requirements & Constraints

- **REQ-001**: All domain extension classes must follow the `XxxMethod` naming convention
- **REQ-002**: Source files must follow `Entity.Method.cs` / `Entity.Method.Concern.cs` pattern
- **REQ-003**: Test files must follow `Entity.Method.Tests.cs` / `Entity.Method.Concern.Tests.cs` pattern
- **REQ-004**: All production call sites referencing `XxxExtensions.` must be updated to `XxxMethod.`
- **REQ-005**: No behavioral changes — only class names, file names, and references
- **CON-001**: Extension methods (with `this` parameter) do NOT need call-site changes — only static factory calls do
- **CON-002**: Follow existing region style: `#region Region Name`

## 2. Implementation Steps

### Phase 1: Rename Source Files + Classes

- GOAL-001: Rename 6 source files and update their class declarations

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename `Price.Extensions.cs` → `Price.Method.cs`. Change class `PriceExtensions` → `PriceMethod`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Variants/Prices/` | | |
| TASK-002 | Rename `PriceHistory.Extensions.cs` → `PriceHistory.Method.cs`. Change class `PriceHistoryExtensions` → `PriceHistoryMethod`. Keep `#region Factory Methods` and `#region Methods` regions. Path: same as TASK-001 | | |
| TASK-003 | Rename `OptionValueVariant.Extensions.cs` → `OptionValueVariant.Method.cs`. Change class `OptionValueVariantExtensions` → `OptionValueVariantMethod`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Variants/Options/` | | |
| TASK-004 | Rename `ImageEmbedding.Extensions.cs` → `ImageEmbedding.Method.cs`. Change class `ImageEmbeddingExtensions` → `ImageEmbeddingMethod`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/` | | |
| TASK-005 | Rename `Classification.Extensions.cs` → `Classification.Method.cs`. Change class `ClassificationExtensions` → `ClassificationMethod`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Classifications/` | | |
| TASK-006 | Rename `ProductOptionType.Extensions.cs` → `ProductOptionType.Method.cs`. Class already `ProductOptionTypeMethod` — no class rename needed. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Domain/Products/Options/` | | |

### Phase 2: Rename Test Files

- GOAL-002: Rename 7 test files to match `Entity.Method.Tests.cs` pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Rename `Price.Extensions.Tests.cs` → `Price.Method.Tests.cs` | | |
| TASK-008 | Rename `PriceHistory.Extensions.Tests.cs` → `PriceHistory.Method.Tests.cs` | | |
| TASK-009 | Rename `OptionValueVariant.Extensions.Tests.cs` → `OptionValueVariant.Method.Tests.cs` | | |
| TASK-010 | Rename `VariantImage.Extensions.Tests.cs` → `VariantImage.Method.Tests.cs` | | |
| TASK-011 | Rename `ImageEmbedding.Extensions.Tests.cs` → `ImageEmbedding.Method.Tests.cs` | | |
| TASK-012 | Rename `Classification.Extensions.Tests.cs` → `Classification.Method.Tests.cs` | | |
| TASK-013 | Rename `ProductOptionType.Extensions.Tests.cs` → `ProductOptionType.Method.Tests.cs` | | |

### Phase 3: Update Production Call Sites

- GOAL-003: Update all 8 static factory call sites that reference old `XxxExtensions.` class names

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | In `Price.Mapping.cs` line 19: replace `PriceExtensions.Create(` with `PriceMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Shared/Mappings/` | | |
| TASK-015 | In `SyncVariantPrices.cs` line 81: replace `PriceExtensions.Create(` with `PriceMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Sync/` | | |
| TASK-016 | In `SyncVariantOptionValues.cs` line 49: replace `OptionValueVariantExtensions.Create(` with `OptionValueVariantMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Sync/` | | |
| TASK-017 | In `AssignVariantOptionValues.cs` line 38: replace `OptionValueVariantExtensions.Create(` with `OptionValueVariantMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Assign/` | | |
| TASK-018 | In `AddVariant.cs` line 61: replace `OptionValueVariantExtensions.Create(` with `OptionValueVariantMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Add/` | | |
| TASK-019 | In `ProductClassification.Mapping.Domain.cs` line 13: replace `ClassificationExtensions.Create(` with `ClassificationMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Shared/Mappings/` | | |
| TASK-020 | In `AutoClassificationService.cs` lines 58, 118: replace `ClassificationExtensions.Create(` with `ClassificationMethod.Create(`. Path: `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Services/AutoClassification/` | | |
| TASK-021 | Update test class names in renamed test files: `PriceExtensionsTests` → `PriceMethodTests`, `PriceHistoryExtensionsTests` → `PriceHistoryMethodTests`, `OptionValueVariantExtensionsTests` → `OptionValueVariantMethodTests`, `ClassificationExtensionsTests` → `ClassificationMethodTests`, `ImageEmbeddingExtensionsTests` → `ImageEmbeddingMethodTests` | | |

### Phase 4: Build & Test Verification

- GOAL-004: Verify compilation and full test suite

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Run `dotnet build service/Api/src/Module/` — confirm 0 errors | ✅ | 2026-07-03 |
| TASK-023 | Run `dotnet test service/Api/tests/Module.UnitTests/` — 1781 tests, 1778 passed, 0 failed, 3 skipped | ✅ | 2026-07-03 |

## 3. Alternatives

- **ALT-001**: Keep `XxxExtensions` names. Rejected — inconsistent with `VariantMethod`, `ProductMethod`, `VariantImageMethod`, `ProductOptionTypeMethod` already using `XxxMethod`.

## 4. Dependencies

- **DEP-001**: .NET SDK for building and testing

## 5. Files

- **FILE-001**: `Price.Method.cs` — rename + class rename from `PriceExtensions` to `PriceMethod`
- **FILE-002**: `PriceHistory.Method.cs` — rename + class rename from `PriceHistoryExtensions` to `PriceHistoryMethod`
- **FILE-003**: `OptionValueVariant.Method.cs` — rename + class rename from `OptionValueVariantExtensions` to `OptionValueVariantMethod`
- **FILE-004**: `ImageEmbedding.Method.cs` — rename + class rename from `ImageEmbeddingExtensions` to `ImageEmbeddingMethod`
- **FILE-005**: `Classification.Method.cs` — rename + class rename from `ClassificationExtensions` to `ClassificationMethod`
- **FILE-006**: `ProductOptionType.Method.cs` — rename only (class already correct)
- **FILE-007 to FILE-013**: 7 test files renamed
- **FILE-014 to FILE-020**: 7 production source files updated (call sites)

## 6. Testing

- **TEST-001**: All existing unit tests in `Module.UnitTests.Catalog.Domain` pass after renames
- **TEST-002**: `dotnet build` succeeds with 0 errors

## 7. Risks & Assumptions

- **RISK-001**: Extension methods (`this Price`, `this VariantImage`, etc.) do NOT need call-site changes because C# resolves extension methods by namespace, not class name. No risk.
- **ASSUMPTION-001**: The `dotnet test --filter-namespace` option exists in xUnit.net v3 (confirmed working earlier).
