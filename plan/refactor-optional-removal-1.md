---
goal: Remove custom Optional<T> type and replace all usages with standard C# nullable pattern
version: 1.0
date_created: 2026-07-02
owner: Platform Team
status: Planned
tags: refactor, migration, cleanup
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Remove the hand-rolled `Optional<T>` struct and all its references across the codebase. Replace the "not provided vs. provided" semantics with standard C# nullable types (`T?`) — `Null` means "not provided", a non-null value means "update with this value". This eliminates a custom abstraction, reduces maintenance surface, and relies on built-in language features.

## 1. Requirements & Constraints

- **REQ-001**: All `Optional<T>` usage in domain `Update` method parameters must be replaced with `T?` (nullable).
- **REQ-002**: The `Optional<T>` struct definition (5 partial files) must be deleted.
- **REQ-003**: The 4 Optional test files must be deleted.
- **REQ-004**: The `global using Shared.Application.Models.Optionals` directive must be removed.
- **REQ-005**: The `ToOptional<T>()` extension on `Result<T>` must be removed.
- **REQ-006**: `Optional<T>` extension methods (`Apply`, `ApplyIfChanged`, `ApplyIf`, `ApplyValidated`, `ToResult`, `Match`, `SelectMany`) must be removed — callers must inline the logic.
- **REQ-007**: Build must succeed with zero errors after all changes.
- **CON-001**: No new external dependencies (e.g., LanguageExt) may be introduced.
- **CON-002**: Domain `Update` method signatures must preserve the "skip this field" semantics: `null` parameter = field not provided (skip), non-null = update with this value.
- **PAT-001**: For reference type parameters (`string?`, `List<string>?`): use `if (param is not null) { ... param ... }` pattern.
- **PAT-002**: For nullable value type parameters (`bool?`, `int?`, `DateTimeOffset?`, `Guid?`): use `if (param.HasValue) { ... param.Value ... }` pattern.

## 2. Implementation Steps

### Implementation Phase 1: Replace Optional<T> in domain method signatures and bodies

- GOAL-001: Migrate all domain `Update` methods from `Optional<T>` parameters to `T?`, inline extension method calls, and update callers in mapping code.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | **User.Method.cs** — Replace `Optional<string?>` params with `string?`, `Optional<bool>` with `bool?`. Inline all `ApplyIfChanged` calls with direct null-check + value-comparison + assignment. Remove `using Shared.Application.Models.Optionals` (via global using removal in Phase 2). | | |
| TASK-002 | **Role.Method.cs** — Replace `Optional<string>` params with `string?`. Inline `Apply` in `Create` to `if (description is not null)`. Inline both `ApplyIfChanged` calls in `Update`. | | |
| TASK-003 | **UserProfile.Method.cs** — Replace all 14 `Optional<T>` params (mixed `string?`, `DateTimeOffset?`, `Guid?`, `bool`, `UserPreferences`, `NotificationPreferences`) with `T?`. Replace `.HasValue` with `is not null` (ref types) or `.HasValue` (value types). Replace `.Value` with direct access. | | |
| TASK-004 | **Address.Method.cs** — Replace all 16 `Optional<T>` params with `T?`. Same pattern as TASK-003. | | |
| TASK-005 | **Wishlist.Method.cs** — Replace 3 `Optional<T>` params (`string`, `bool`, `bool`) with `string?`, `bool?`, `bool?`. | | |
| TASK-006 | **WishedItem.Method.cs** — Replace `Optional<int>` with `int?`. | | |
| TASK-007 | **UserPreference.Method.cs** — Replace 8 `Optional<T>` params with `T?`. | | |
| TASK-008 | **NotificationPreferences.Extensions.cs** — Replace 3 `Optional<bool>` params with `bool?`. | | |
| TASK-009 | **User.Mapping.Domain.cs** — Remove `ToOptional` helper method. Inline null-coalescing at each call site: `string.IsNullOrEmpty(v) ? null : v`. Remove `using Shared.Application.Models.Optionals` (via global using removal in Phase 2). | | |

### Implementation Phase 2: Remove Optional infrastructure

- GOAL-002: Delete all Optional definition files, test files, and related infrastructure after no remaining references exist.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | **Delete** `service/Api/src/Shared/Application/Models/Optionals/Optional.cs` | | |
| TASK-011 | **Delete** `service/Api/src/Shared/Application/Models/Optionals/Optional.Constant.cs` | | |
| TASK-012 | **Delete** `service/Api/src/Shared/Application/Models/Optionals/Optional.Method.cs` | | |
| TASK-013 | **Delete** `service/Api/src/Shared/Application/Models/Optionals/Optional.Operator.cs` | | |
| TASK-014 | **Delete** `service/Api/src/Shared/Application/Models/Optionals/Optional.Extension.cs` | | |
| TASK-015 | **Delete** `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Tests.cs` | | |
| TASK-016 | **Delete** `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Method.Tests.cs` | | |
| TASK-017 | **Delete** `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Extension.Tests.cs` | | |
| TASK-018 | **Delete** `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Constant.Tests.cs` | | |
| TASK-019 | **Remove** line 7 (`global using Shared.Application.Models.Optionals;`) from `service/Api/src/Shared/GlobalUsings.cs` | | |
| TASK-020 | **Remove** `ToOptional<T>()` method from `service/Api/src/Shared/Application/Models/Results/Result.Extension.cs` (entire file if no other content remains) | | |
| TASK-021 | **Delete** `service/Api/tests/Shared.UnitTests/Application/Models/Results/Result.Extension.Tests.cs` (only contained `ToOptional` tests) | | |

### Implementation Phase 3: Update remaining Optional construction in tests

- GOAL-003: Update the one remaining test file that directly constructs `Optional<T>` variables.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | **UserPreferences.Methods.Tests.cs** — Lines 127-129: Replace `Optional<string?> topOpt = sizeTop is not null ? sizeTop : default;` with direct `string?` passing (e.g., `sizeTop: sizeTop`). | | |

## 3. Alternatives

- **ALT-001**: Keep `Optional<T>` and add LanguageExt NuGet package — rejected because it adds an external dependency for the same functionality.
- **ALT-002**: Replace with `Func<T>?` factory pattern — rejected because it is less readable and more awkward for value types.
- **ALT-003**: Replace with a different custom struct like `Maybe<T>` or `Change<T>` — rejected because it just renames the same abstraction; the goal is to eliminate it entirely.
- **ALT-004**: Use nullable attributes (`[MaybeNull]`, `[AllowNull]`) — rejected because they only affect static analysis, not runtime semantics.

## 4. Dependencies

- **DEP-001**: No NuGet package changes required — solution relies on standard C# nullable feature only.
- **DEP-002**: C# language version must support nullable reference types (already configured in project — check `Directory.Build.props` or `.csproj` for `<Nullable>enable</Nullable>` or `<Nullable>annotations</Nullable>`).

## 5. Files

- **FILE-001** (DELETE): `service/Api/src/Shared/Application/Models/Optionals/Optional.cs`
- **FILE-002** (DELETE): `service/Api/src/Shared/Application/Models/Optionals/Optional.Constant.cs`
- **FILE-003** (DELETE): `service/Api/src/Shared/Application/Models/Optionals/Optional.Method.cs`
- **FILE-004** (DELETE): `service/Api/src/Shared/Application/Models/Optionals/Optional.Operator.cs`
- **FILE-005** (DELETE): `service/Api/src/Shared/Application/Models/Optionals/Optional.Extension.cs`
- **FILE-006** (DELETE): `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Tests.cs`
- **FILE-007** (DELETE): `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Method.Tests.cs`
- **FILE-008** (DELETE): `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Extension.Tests.cs`
- **FILE-009** (DELETE): `service/Api/tests/Shared.UnitTests/Application/Models/Optionals/Optional.Constant.Tests.cs`
- **FILE-010** (DELETE): `service/Api/tests/Shared.UnitTests/Application/Models/Results/Result.Extension.Tests.cs`
- **FILE-011** (MODIFY): `service/Api/src/Shared/GlobalUsings.cs` — remove line 7
- **FILE-012** (MODIFY/DELETE): `service/Api/src/Shared/Application/Models/Results/Result.Extension.cs` — remove `ToOptional`
- **FILE-013** (MODIFY): `service/Api/src/Shared/Security/Identity/Domain/Users/User.Method.cs` — replace Optional params
- **FILE-014** (MODIFY): `service/Api/src/Shared/Security/Identity/Domain/Roles/Role.Method.cs` — replace Optional params
- **FILE-015** (MODIFY): `service/Api/src/Module/Profile/Domain/UserProfile.Method.cs` — replace Optional params
- **FILE-016** (MODIFY): `service/Api/src/Module/Profile/Domain/Addresses/Address.Method.cs` — replace Optional params
- **FILE-017** (MODIFY): `service/Api/src/Module/Profile/Domain/Wishlists/Wishlist.Method.cs` — replace Optional params
- **FILE-018** (MODIFY): `service/Api/src/Module/Profile/Domain/Wishlists/WishedItems/WishedItem.Method.cs` — replace Optional params
- **FILE-019** (MODIFY): `service/Api/src/Module/Profile/Domain/Preferences/UserPreference.Method.cs` — replace Optional params
- **FILE-020** (MODIFY): `service/Api/src/Module/Profile/Domain/Notifications/NotificationPreferences.Extensions.cs` — replace Optional params
- **FILE-021** (MODIFY): `service/Api/src/Module/Identity/Features/Admin/Users/Shared/Mappings/User.Mapping.Domain.cs` — remove ToOptional, inline null checks
- **FILE-022** (MODIFY): `service/Api/tests/Module.UnitTests/Profile/Domain/Preferences/UserPreferences.Methods.Tests.cs` — replace Optional construction

## 6. Testing

- **TEST-001**: Run `dotnet build` across the entire solution after Phase 1 to verify all Optional references still resolve.
- **TEST-002**: Run `dotnet build` after Phase 2 to verify no broken references to now-deleted files.
- **TEST-003**: Run all unit tests (`dotnet test`) to verify behavioral equivalence.
- **TEST-004**: Manually verify that `Update` methods with all `null` parameters produce no side effects (semantic preserve).

## 7. Risks & Assumptions

- **RISK-001**: If a nullable-reference-type annotation (`string?` vs `string`) is mismatched with an `Update` caller that passes a non-null literal, no behavioral change occurs — the value is still applied correctly.
- **RISK-002**: The implicit `bool` operator on `Optional<T>` (`if (optional)`) is only used via `.HasValue` directly, not via the implicit bool conversion. Grep confirms no `if (optionalVar)` patterns — all use `.HasValue`.
- **ASSUMPTION-001**: `Optional<T>.Some(null)` was never used — the `Some` method throws `ArgumentNullException` for null values, and the implicit `T → Optional<T>` operator converts null to `None`. Therefore, `Some(null)` semantics do not exist and the replacement `T?` with null = "not provided" is semantically equivalent.
- **ASSUMPTION-002**: No external code references `Shared.Application.Models.Optionals` (verified via grep — all references accounted for in the 22 tracked files).

## 8. Related Specifications / Further Reading

- [C# Nullable Reference Types documentation](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [C# Nullable Value Types documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types)
