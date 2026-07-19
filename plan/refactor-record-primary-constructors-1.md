---
goal: Refactor C# records from primary-constructor syntax to block-body { get; init; } property syntax
version: 1.0
date_created: 2026-07-19
owner: Platform Team
status: Planned
tags: refactor, records, csharp, code-style
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Transform ~79 non-Command/Query C# records from primary-constructor parameter syntax to explicit block-body `{ get; init; }` property syntax. This improves XML-doc placement, property-level attributes, default-value readability, and consistency with already-refactored records (e.g., `OptionValueParameters` at `OptionValue.Model.Parameters.cs:6`). MediatR `Command`/`Query` feature records (named exactly `Command` or `Query`) are excluded from this refactoring.

## Transformation Rules

| Rule | From | To |
|------|------|----|
| RULE-001 | `record Foo(string A, int B)` | `record Foo { public string A { get; init; } public int B { get; init; } }` |
| RULE-002 | `record Foo(string A = "")` with existing `{...}` body | Move params to properties at top of body |
| RULE-003 | `abstract record Foo(string A, string B)` | Same as RULE-001 with `abstract record` |
| RULE-004 | `sealed partial record Foo(...)` with body | Same as RULE-002 |
| RULE-005 | String type default `= ""` | `= string.Empty` |
| RULE-006 | Non-string type default `= value` | Preserved verbatim: `= value` |
| RULE-007 | `string?` param with no default | `public string? A { get; init; }` (no default) |
| RULE-008 | XML `<param>` docs on primary constructor | Move to `<inheritdoc>` or inline `<summary>` on property |
| RULE-009 | Record inheritance: `record A(...) : Base(...)` | `record A : Base` + properties; remove base call |

## 1. Requirements & Constraints

- **REQ-001**: Every refactored record must preserve semantic equality (value-based `Equals`/`GetHashCode`)
- **REQ-002**: Every refactored record must preserve all existing constructors, factory methods, and static members
- **REQ-003**: Properties must use `{ get; init; }` (not `{ get; set; }` or `{ get; private set; }`)
- **REQ-004**: String fields with `= ""` default → `= string.Empty`
- **REQ-005**: Boolean fields with `= false` default → `= false` (unchanged, explicit)
- **REQ-006**: Nullable reference type `= null` → `= null`
- **CON-001**: Primary-constructor records auto-generate `Deconstruct()` — block-body records do NOT. Search for deconstruction usage before/after each phase.
- **CON-002**: Primary-constructor records auto-generate `Clone()` for `with` expressions — preserved in block-body form.
- **CON-003**: Records inheriting from other records (e.g., `BaseLogOutRequest(...) : BaseRefreshTokenRequestModel(...)`) must have base properties replicated in child — no more base-constructor delegation.
- **CON-004**: Records already having a `{...}` body must merge properties into existing body, preserving member order.
- **CON-005**: XML documentation comments currently on constructor parameters must be relocated to the corresponding property.
- **PAT-001**: Follow the pattern established at `OptionValue.Model.Parameters.cs:6-11`:
  ```csharp
  public abstract record OptionValueParameters
  {
      public string Name { get; init; } = string.Empty;
      public string Presentation { get; init; } = string.Empty;
      public int Position { get; init; } = 0;
  }
  ```

## 2. Implementation Steps

### Phase 1: Shared Infrastructure — `service/Api/src/Shared/`

- GOAL-001: Refactor all 24 non-Command/Query records in Shared across 18 files

| Task | File | Line | Record | Transformation | Completed | Date |
|------|------|------|--------|---------------|-----------|------|
| TASK-001 | `Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs` | 5 | `CreateUserProfileCommand(Guid UserId, string FirstName, string? LastName, string Email) : ICommand<CreateUserProfileResult>` | 4 params → properties; `: ICommand<CreateUserProfileResult>` unchanged | | |
| TASK-002 | `Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs` | 11 | `CreateUserProfileResult(Guid ProfileId)` | 1 param → property | | |
| TASK-003 | `Shared/Application/Domain/Currencies/SystemCurrency.cs` | 3 | `SystemCurrency(string Code, string Symbol, string Name, int NumericCode)` | 4 params → properties; merge into existing body | | |
| TASK-004 | `Shared/Operational/Notifications/Models/Notification.Parameter.Model.cs` | 10 | `NotificationParameter(NotificationParameterType Key, string? Value, bool IsRequired = true)` | 3 params → properties; `IsRequired = true` preserved; merge into existing body with factory method; relocate XML docs from `<param>` to property `<summary>` | | |
| TASK-005 | `Shared/Operational/Persistence/Specifications/Filtering/Filter.Condition.cs` | 23 | `FilterCondition(string Field, FilterOperator Operator, string Value)` | 3 params → properties; merge into existing partial record body; relocate XML docs | | |
| TASK-006 | `Shared/Operational/Persistence/Specifications/Filtering/Filter.Group.cs` | 23 | `FilterGroup(FilterLogic Logic, IReadOnlyList<FilterCondition> Conditions, IReadOnlyList<FilterGroup> Groups)` | 3 params → properties; merge into existing partial record body | | |
| TASK-007 | `Shared/Operational/Persistence/Specifications/Paging/Page.Bounds.cs` | 30 | `PageBounds(int DefaultPage, int DefaultPageSize, int MaxPageSize)` | 3 params → properties; merge into existing partial record body | | |
| TASK-008 | `Shared/Operational/Persistence/Specifications/Querying/Querying.Model.cs` | 16 | `QueryingModel(FilterModel Filter, SearchModel Search, SortModel Sort, PageModel Page)` | 4 params → properties | | |
| TASK-009 | `Shared/Operational/Persistence/Specifications/Searching/Search.Model.Result.cs` | 4 | `SearchValidationResult(bool IsValid, IReadOnlyList<string> Violations, IReadOnlyList<string>? AllowedFields)` | 3 params → properties | | |
| TASK-010 | `Shared/Operational/Persistence/Specifications/Searching/Search.Term.cs` | 27 | `SearchTerm(string Value, bool CaseSensitive = false)` | 2 params → properties; merge into existing partial record body | | |
| TASK-011 | `Shared/Operational/Persistence/Specifications/Sorting/Sort.Clause.cs` | 22 | `SortClause(string Field, SortDirection Direction, SortNulls? Nulls)` | 3 params → properties; merge into existing partial record body | | |
| TASK-012 | `Shared/Operational/Storages/Models/Storage.Request.Model.cs` | 8 | `UploadRequest(string Key, Stream Content, string ContentType, IReadOnlyDictionary<string, string>? Metadata, UploadOptions? Options)` | 5 params → properties | | |
| TASK-013 | `Shared/Operational/Storages/Models/Storage.Response.Model.cs` | 9 | `UploadResult(string Key, string Provider, Uri? Uri, long SizeBytes, DateTimeOffset StoredAtUtc)` | 5 params → properties | | |
| TASK-014 | `Shared/Operational/Storages/Models/Storage.Response.Model.cs` | 22 | `StoredObjectInfo(string Key, string Provider, long SizeBytes, DateTimeOffset LastModifiedUtc, string? ContentType)` | 5 params → properties | | |
| TASK-015 | `Shared/Operational/Storages/Models/Storage.Response.Model.cs` | 32 | `DownloadResult(Stream Content, StoredObjectInfo Info)` | 2 params → properties | | |
| TASK-016 | `Shared/Operational/Storages/Security/Scanners/Storage.MalwareScanner.Model.cs` | 10 | `MalwareScanResult(bool IsClean, string? ThreatName, string? ScanEngine, long? ScanDurationMs)` | 4 params → properties | | |
| TASK-017 | `Shared/Security/AntiForgery/Endpoints/TokenResponse.cs` | 3 | `TokenResponse(string Token, string HeaderName)` | 2 params → properties | | |
| TASK-018 | `Shared/Security/Authentication/External/Models/ExternalUser.Model.cs` | 11 | `ExternalUserInfo(string Provider, string ProviderSubjectId, string Email, string FirstName, string? LastName)` | 5 params → properties | | |
| TASK-019 | `Shared/Security/Authentication/External/Providers/Facebook/Facebook.TokenValidator.Interface.cs` | 8 | `FacebookUserInfo(string Id, string Email, string? Name)` | 3 params → properties | | |
| TASK-020 | `Shared/Security/Authentication/External/Providers/Microsoft/Microsoft.TokenValidator.Interface.cs` | 8 | `MicrosoftUserInfo(string Id, string Mail, string? DisplayName)` | 3 params → properties | | |
| TASK-021 | `Shared/Security/Authentication/Tokens/Models/Token.Request.Model.cs` | 3 | `TokenRequestModel(Guid UserId, string Email, string FullName)` | 3 params → properties | | |
| TASK-022 | `Shared/Security/Authentication/Tokens/Models/Token.Request.Model.cs` | 5 | `RevokeTokenRequestModel(string Token, string? Reason)` | 2 params → properties | | |
| TASK-023 | `Shared/Security/Authentication/Tokens/Models/Token.Response.Model.cs` | 3 | `TokenResponseModel(string Token, long ExpiresIn)` | 2 params → properties | | |
| TASK-024 | `Shared/Security/Authentication/Tokens/Models/Token.Response.Model.cs` | 4-13 | `RefreshTokenResponseModel(Guid Id, string Token, Guid UserId, DateTime CreatedAt, DateTime ExpiresAt, DateTime? RevokedAt, string? RevokedReason, string? ReplacedByToken, bool IsActive)` | 9 params → properties | | |

### Phase 2: Catalog Module — `service/Api/src/Module/Catalog/`

- GOAL-002: Refactor 11 non-Command/Query records in Catalog module

| Task | File | Line | Record | Transformation | Completed | Date |
|------|------|------|--------|---------------|-----------|------|
| TASK-025 | `Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Constant.cs` | 29 | `ModelSpecification(string Name, int Dimensions, ModelRole Role, ComputeProfile ComputeProfile, bool SupportsText, bool SupportsImage, int ExpectedLatencyMs, string UseCase, string Strengths, string Weaknesses)` | 10 params → properties; nested inside `ImageEmbeddingConstants` class — add semicolons to outer class if needed; note: no defaults | | |
| TASK-026 | `Catalog/Features/Admin/OptionTypes/Shared/Models/OptionType.Model.Parameters.cs` | 4 | `OptionTypeParameters(string Name = "", string Presentation = "", int Position = 0, bool Filterable = false)` | 4 params → properties; `"" → string.Empty`; abstract record; no body | | |
| TASK-027 | `Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Parameters.cs` | 3 | `ImageEmbeddingParameters(string ModelName, string ModelVersion)` | 2 params → properties; abstract record | | |
| TASK-028 | `Catalog/Features/Admin/Products/Variants/Images/Shared/Models/VariantImage.Model.Parameters.cs` | 3 | `VariantImageParameters(string? Alt, int Position, string Type)` | 3 params → properties; abstract record | | |
| TASK-029 | `Catalog/Features/Admin/Products/Variants/Prices/Shared/Models/Price.Model.Parameters.cs` | 3 | `PriceParameters(decimal? Amount, string Currency, decimal? CompareAtAmount, string? CountryIso)` | 4 params → properties; abstract record; note: no defaults | | |
| TASK-030 | `Catalog/Features/Admin/Taxonomies/Shared/Models/Taxonomy.Model.Parameters.cs` | 4 | `TaxonomyParameters(string Name, string? Presentation, int Position)` | 3 params → properties; abstract record; note: no defaults | | |
| TASK-031 | `Catalog/Features/Admin/Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Parameters.cs` | 3 | `TaxonRuleParameter(string Type, string MatchPolicy, string Value)` | 3 params → properties; abstract record; note: record is named `TaxonRuleParameter` (singular) | | |
| TASK-032 | `Catalog/Features/Admin/Products/Variants/Images/Delete/DeleteVariantImage.Response.cs` | 6 | `Response(string Message)` | 1 param → property | | |
| TASK-033 | `Catalog/Features/Admin/Products/Variants/Prices/Set/SetVariantPrice.Response.cs` | 6 | `Response(Guid VariantId)` | 1 param → property | | |
| TASK-034 | `Catalog/Features/Admin/Taxonomies/Taxons/Reposition/RepositionTaxonUseCase.Response.cs` | 4 | `Response(Guid Id)` | 1 param → property | | |
| TASK-035 | `Catalog/Features/Admin/Dashboard/Get/GetCatalogDashboard.Response.cs` | 16 | `RecentProductData(Guid Id, string Name, string Slug, DateTime CreatedAtUtc)` | 4 params → properties | | |

### Phase 3: Dashboard + Identity + Inventory — `service/Api/src/Module/{Dashboard,Identity,Inventory}/`

- GOAL-003: Refactor 13 records across Dashboard, Identity, and Inventory

| Task | File | Line | Record | Transformation | Completed | Date |
|------|------|------|--------|---------------|-----------|------|
| TASK-036 | `Dashboard/Features/Admin/Get/GetDashboard.Response.cs` | 22 | `TrendPoint(DateOnly Date, decimal Revenue)` | 2 params → properties | | |
| TASK-037 | `Dashboard/Features/Admin/Get/GetDashboard.Response.cs` | 42 | `RecentProductData(Guid Id, string Name, string Slug, DateTime CreatedAtUtc)` | 4 params → properties | | |
| TASK-038 | `Identity/Features/Admin/Permissions/Shared/Models/Permission.Model.Group.cs` | 5 | `ResourceGroup(string ResourceName, IReadOnlyList<PermissionMetadata> Permissions, string? Description)` | 3 params → properties | | |
| TASK-039 | `Identity/Features/Admin/Permissions/Shared/Models/Permission.Model.Group.cs` | 7 | `PermissionGroup(string Category, IReadOnlyList<ResourceGroup> Resources, string? Description)` | 3 params → properties | | |
| TASK-040 | `Identity/Features/Admin/Users/Roles/Get/GetUserRoles.Response.cs` | 6 | `Response(List<RoleItemResponse> Roles)` | 1 param → property | | |
| TASK-041 | `Identity/Features/Store/Auth/Login/External/Shared/Models/External.Model.Request.cs` | 3 | `BaseExternalLoginRequest(string Provider, string IdToken)` | 2 params → properties; abstract record | | |
| TASK-042 | `Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.cs` | 15 | `Response(string Message)` | 1 param → property | | |
| TASK-043 | `Identity/Features/Store/Shared/Models/Auth.Request.Model.cs` | 3 | `BasePasswordLoginRequest(string Credential = "", string Password = "")` | 2 params → properties; abstract record; `"" → string.Empty` | | |
| TASK-044 | `Identity/Features/Store/Shared/Models/Auth.Request.Model.cs` | 5 | `BaseRefreshTokenRequestModel(string? RefreshToken = null)` | 1 param → property; abstract record | | |
| TASK-045 | `Identity/Features/Store/Shared/Models/Auth.Request.Model.cs` | 7 | `BaseLogOutRequest(string? RefreshToken = null, bool RevokeAll = false) : BaseRefreshTokenRequestModel(RefreshToken)` | 2 params → properties; abstract record; REMOVE `: BaseRefreshTokenRequestModel(RefreshToken)` — parent is abstract, duplicate `RefreshToken` property in child | | |
| TASK-046 | `Inventory/Features/Admin/Dashboard/Get/GetInventoryDashboard.Response.cs` | 16 | `RecentMovementData(Guid Id, int Quantity, string? Action, string? Reason, DateTime CreatedAtUtc)` | 5 params → properties | | |
| TASK-047 | `Inventory/Services/StockSnapshot.cs` | 3 | `StockSnapshot(int TotalOnHand, int TotalReserved, int TotalAvailable, bool Backorderable, IReadOnlyList<LocationStockSnapshot> Locations)` | 5 params → properties | | |
| TASK-048 | `Inventory/Services/StockSnapshot.cs` | 10 | `LocationStockSnapshot(Guid StockLocationId, string LocationName, int CountOnHand, int ReservedCount, int AvailableCount, bool Active, bool Backorderable)` | 7 params → properties | | |

### Phase 4: Ordering + Payment + Profile + Shipping — `service/Api/src/Module/{Ordering,Payment,Profile,Shipping}/`

- GOAL-004: Refactor 9 records across Ordering, Payment, Profile, and Shipping

| Task | File | Line | Record | Transformation | Completed | Date |
|------|------|------|--------|---------------|-----------|------|
| TASK-049 | `Ordering/Features/Admin/Dashboard/Get/GetOrderingDashboard.Response.cs` | 16 | `RecentOrderData(Guid Id, string Number, decimal Total, string Status, DateTime CreatedAtUtc)` | 5 params → properties | | |
| TASK-050 | `Payment/Features/Shared/Commands/VoidOrderPayments.cs` | 10 | `VoidOrderPaymentsCommand(Guid OrderId, string Reason) : ICommand` | 2 params → properties; `: ICommand` unchanged | | |
| TASK-051 | `Profile/Features/Admin/Addresses/Delete/DeleteUserAddress.Response.cs` | 5 | `Response(Guid Id, string? Label)` | 2 params → properties | | |
| TASK-052 | `Profile/Features/Store/Addresses/Delete/DeleteAddress.Response.cs` | 7 | `Response(Guid Id, string Label)` | 2 params → properties | | |
| TASK-053 | `Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.Response.cs` | 7 | `Response(Guid ShippingMethodId, string MethodName, decimal Cost, string Currency, bool IsFreeShipping)` | 5 params → properties | | |
| TASK-054 | `Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Response.cs` | 7 | `Response(List<ShippingMethodDto> Methods)` | 1 param → property | | |
| TASK-055 | `Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Response.cs` | 9 | `ShippingMethodDto(Guid Id, string Name, string? AdminName, string? Code, string CalculatorType, int Position)` | 6 params → properties | | |
| TASK-056 | `Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.Response.cs` | 7 | `Response(Guid Id, Guid ShippingMethodId, string Name, decimal Cost, decimal FinalPrice, string? DeliveryRange, decimal? MinWeight, decimal? MaxWeight, decimal? FreeShippingThreshold)` | 9 params → properties | | |

### Phase 5: Test Records — `service/Api/tests/Shared.UnitTests/`

- GOAL-005: Refactor 7 test-only records

| Task | File | Line | Record | Transformation | Completed | Date |
|------|------|------|--------|---------------|-----------|------|
| TASK-057 | `Shared.UnitTests/Application/Extensions/Validations/OptionsBuilder.Validation.Extension.Tests.cs` | 16 | `TestOptions(string? Name, int Value)` | 2 params → properties; inline in test file | | |
| TASK-058 | `Shared.UnitTests/Application/Mediators/Behaviours/Logging/Logging.Behavior.Tests.cs` | 103 | `TestRequest(string Data) : IRequest<Result>` | 1 param → property; inline in test file | | |
| TASK-059 | `Shared.UnitTests/Application/Mediators/Behaviours/Validation/Validation.Behavior.Tests.cs` | 226 | `TestRequestWithValue(string Value) : IRequest<Result<string>>` | 1 param → property; inline in test file | | |
| TASK-060 | `Shared.UnitTests/Application/Mediators/Fixtures/Test.Logger.cs` | 36 | `LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception)` | 4 params → properties | | |
| TASK-061 | `Shared.UnitTests/Application/Mediators/Fixtures/Test.Request.cs` | 7 | `TestRequest(string Value) : IRequest<Result>` | 1 param → property | | |
| TASK-062 | `Shared.UnitTests/Application/Mediators/Fixtures/Test.Request.cs` | 9 | `TestRequestWithValue(string Value) : IRequest<Result<string>>` | 1 param → property | | |
| TASK-063 | `Shared.UnitTests/Application/Mediators/Fixtures/Test.Request.cs` | 11 | `TestRequestMultipleValidations(string Value, string Name) : IRequest<Result>` | 2 params → properties | | |

### Phase 6: Private / Seeder Records — across multiple projects

- GOAL-006: Refactor 16 private/internal records in persistence seeders and helpers

| Task | File | Line | Record | Transformation | Completed | Date |
|------|------|------|--------|---------------|-----------|------|
| TASK-064 | `Shared/Operational/Persistence/Specifications/Paging/Parsing/PageJsonParser.cs` | 12 | `PageJsonDto(int? Page, int? PageSize)` | 2 params → properties; private record inside class | | |
| TASK-065 | `Catalog/Persistence/Seeders/Embedding.Seeder.cs` | 92 | `DemoEmbeddingJson(string VariantImageId, string ModelName, string ModelVersion, float[] Vector, int Dimensions)` | 5 params → properties; private | | |
| TASK-066 | `Catalog/Persistence/Seeders/Option.Seeder.cs` | 43 | `DemoOptionTypeJson(string Id, string Name, string Presentation, int Position, bool Filterable)` | 5 params → properties; private | | |
| TASK-067 | `Catalog/Persistence/Seeders/Option.Seeder.cs` | 44 | `DemoOptionValueJson(string Id, string OptionTypeId, string Name, string Presentation, int Position)` | 5 params → properties; private | | |
| TASK-068 | `Catalog/Persistence/Seeders/Product.Seeder.cs` | 132 | `DemoProductJson(string Id, string Name, string Slug, string Description, string Status, string GenderTarget, string MetaTitle, string MetaKeywords, string MasterVariantId, string? StyleCode, string? SeasonName, string? MaterialComposition, string? CareInstructions, string? Department)` | 14 params → properties; private | | |
| TASK-069 | `Catalog/Persistence/Seeders/Product.Seeder.cs` | 136 | `DemoVariantJson(string Id, string ProductId, string Sku, bool IsMaster, int Position, decimal Price, string? Barcode, string? HsCode)` | 8 params → properties; private | | |
| TASK-070 | `Catalog/Persistence/Seeders/Product.Seeder.cs` | 138 | `DemoVariantImageJson(string Id, string VariantId, string ContentType, string FileName, string StoragePath, int Position, string Alt, string Type)` | 8 params → properties; private | | |
| TASK-071 | `Catalog/Persistence/Seeders/Product.Seeder.cs` | 140 | `DemoOptionAssignmentJson(string VariantId, string OptionValueName, string OptionTypeId)` | 3 params → properties; private | | |
| TASK-072 | `Catalog/Persistence/Seeders/Taxon.Seeder.cs` | 61 | `DemoTaxonJson(string Id, string TaxonomyId, string? ParentId, string Name, string? Presentation, string Slug, int Depth, int Lft, int Rgt, int Position)` | 10 params → properties; private | | |
| TASK-073 | `Catalog/Persistence/Seeders/Taxonomy.Seeder.cs` | 30 | `DemoTaxonomyJson(string Id, string Name, string Presentation, int Position)` | 4 params → properties; private | | |
| TASK-074 | `Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs` | 43 | `DemoStockItemJson(string VariantId, string StockLocationCode, int CountOnHand, bool Backorderable)` | 4 params → properties; private | | |
| TASK-075 | `Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs` | 44 | `DemoStockMovementJson(string VariantId, string StockLocationCode, int Quantity, int PreviousCountOnHand, string OriginatorType, string Reason, string Action)` | 7 params → properties; private | | |
| TASK-076 | `Inventory/Persistence/Seeders/StockLocation.Seeder.cs` | 37 | `DemoStockLocationJson(string Id, string Name, string? Presentation, string Code, bool IsDefault, bool Active, string? Address1, string? City, string? PostalCode, string? Phone, bool BackorderableDefault, bool PropagateAllVariants, int Position, string CountryIso)` | 14 params → properties; private | | |
| TASK-077 | `Shared.UnitTests/Application/Models/Descriptors/OptionDescriptor.Tests.cs` | 118 | `SampleRecord(string Key, int Count)` | 2 params → properties; private sealed | | |
| TASK-078 | `Shared.UnitTests/Operational/Persistence/Specifications/Paging/Extensions/PageModelEfCoreExtensions.Tests.cs` | 19 | `TestDto(int Id, string Name)` | 2 params → properties; private sealed | | |
| TASK-079 | `Shared.UnitTests/Operational/Persistence/Specifications/Paging/Extensions/PageModelInMemoryExtensions.Tests.cs` | 11 | `TestEntity(int Id, string Name)` | 2 params → properties; private sealed | | |

### Phase 7: Build Verification & Fix

- GOAL-007: Build the solution, verify warnings-as-errors passes, fix any issues

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-080 | Run `dotnet build service/Api/src/Api/` — verify build passes with zero warnings | | |
| TASK-081 | Fix any build errors from deconstruction usage (add manual `Deconstruct` methods where needed) | | |
| TASK-082 | Run `dotnet test service/Api/tests/Shared.UnitTests` | | |
| TASK-083 | Run `dotnet test` (or `dotnet test --no-build` after building) | | |

## 3. Alternatives

- **ALT-001**: Keep all records in primary-constructor form. Rejected because it prevents property-level XML docs, property-level attributes, and creates inconsistency with already-refactored records.
- **ALT-002**: Refactor only abstract `*Parameters/*Parameter` records. Rejected because the user requested all non-command/query records for consistency.
- **ALT-003**: Use a Roslyn analyzer + code fix to automate transformation. Rejected as over-engineering for a one-time refactoring of ~79 records.

## 4. Dependencies

- **DEP-001**: .NET 10 SDK (C# 13 features for block-body records)
- **DEP-002**: No external NuGet packages — pure C# syntax change
- **DEP-003**: No Deconstruct method callers (search for pattern: `var (a, b) = recordExpr` across codebase before execution)

## 5. Files

- **FILE-001** to **FILE-058**: All files listed in the task tables above (58 files across Shared + 8 modules + Tests)

## 6. Testing

- **TEST-001**: `dotnet build` must pass with zero warnings (TreatWarningsAsErrors=true) — validates all syntax is correct
- **TEST-002**: `dotnet test service/Api/tests/Shared.UnitTests` — validates test-level records don't break assertions
- **TEST-003**: `dotnet test` — full test suite to catch any behavioral regressions
- **TEST-004**: Manual search for `var (` (deconstruction patterns) that may reference refactored records — add manual `Deconstruct` if found

## 7. Risks & Assumptions

- **RISK-001**: Deconstruction usage (`var (a, b) = record`) will break — block-body records don't auto-generate `Deconstruct`. Mitigation: search codebase for deconstruction patterns before each phase.
- **RISK-002**: Serialization (System.Text.Json, Newtonsoft) is unaffected — `{ get; init; }` properties serialize identically to positional record params.
- **RISK-003**: Mapster mapping — `Adapt<Source, Dest>()` uses property names, not constructor params, so unaffected.
- **RISK-004**: EF Core mapping — records used as value objects may be affected if EF relies on constructor binding. Mitigation: verify any owned entity types that reference these records have parameterless constructors.
- **RISK-005**: Records with inheritance (e.g., `BaseLogOutRequest : BaseRefreshTokenRequestModel`) — removing base constructor delegation means each record must independently declare all properties. Duplicate properties (like `RefreshToken` in both parent and child) is fine since they're separate `init` properties.
- **ASSUMPTION-001**: No code uses positional `Deconstruct` on these records. Validate before execution.
- **ASSUMPTION-002**: No code uses positional construction (`new Record(a, b)`) that would break — `init` properties still support object initializer syntax (`new Record { A = a, B = b }`), and positional construction is only auto-generated when primary constructor params exist.

## 8. Related Specifications / Further Reading

- [C# Records positional syntax vs block-body](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [C# 13 Required Members / Init-only properties](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/init)
- Already-refactored reference: `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/OptionValues/Shared/Models/OptionValue.Model.Parameters.cs:6-11`
