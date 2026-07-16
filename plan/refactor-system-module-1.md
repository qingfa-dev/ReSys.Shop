---
goal: Create Module/System — Consolidate Currency Domain & Relocate System Abstractions
version: 1.0
date_created: 2026-07-15
status: Planned
tags: refactor, architecture, system, currency
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create a new `Module/System` business module to own the default currency domain (value object, constants, validation, errors) and host the `ISystemInfo`/`ISystemDateTime` implementations currently split across `Ordering` and `Shared/Application/Systems`. After this plan, all currency defaults, ISO 4217 validation rules, and money formatting concerns are centralized in `Module/System`; `Shared/Application/Systems` retains only the abstraction interfaces while the implementations move to `Module/System`.

## 1. Requirements & Constraints

- **REQ-001**: Create `Module/System/` following the existing module pattern (`Catalog.Extension.cs`, `Ordering.Extension.cs`, etc.)
- **REQ-002**: Centralize all currency defaults (`USD`) and ISO 4217 constants in `Module/System/Domain/Currencies/`
- **REQ-003**: Remove hardcoded `OrderConstant.Defaults.Currency`, `PriceConstant.Default.Currency`, `GatewayConstants.Currency.Usd`, and Shipping mapping `"USD"` literals — all reference `SystemCurrencyConstant` instead
- **REQ-004**: Move `ISystemInfo` implementation and `ISystemDateTime` implementation from `Shared/Application/Systems/` to `Module/System/Domain/SystemInfos/` and `Module/System/Domain/SystemDateTimes/`
- **REQ-005**: Keep `ISystemInfo` interface and `ISystemDateTime` interface in `Shared/Application/Systems/` (Shared cannot depend on Module)
- **REQ-006**: Relocate `Shared/Application/Systems/Systems.Extension.cs` registration logic to `Module/System/System.Extension.cs`; Shared's `Application.Extension.cs` must no longer call `builder.AddSystems()`
- **REQ-007**: Update `appsettings.json` config key from `Ordering:DefaultCurrency` to `System:DefaultCurrency`
- **REQ-008**: Update `Module/GlobalUsing.cs` to include `Module.System.Domain.Currencies` namespace
- **REQ-009**: Update `Program.cs` to call `builder.AddSystemModule()` before all other modules
- **SEC-001**: No breaking changes to public API surface; Currency remains a `string` on all wire DTOs
- **CON-001**: Forward-only dependency — `Module.System` depends on `Shared`; Shared must NOT reference `Module`
- **CON-002**: Warnings-as-errors — every build must pass cleanly (no warnings tolerated)
- **CON-003**: Module isolation — `Module.System` must not reference other modules; other modules communicate with System via MediatR `ISender` or direct reference to `Module.System.Domain.*` value objects
- **PAT-001**: Follow the existing partial-class entity pattern (`Entity.cs`, `Entity.Constant.cs`, `Entity.Validation.cs`, `Entity.Result.cs`, `Entity.Method.cs`)
- **PAT-002**: Follow the existing module extension pattern (`XxxExtension.cs` with `AddXxxModule(this WebApplicationBuilder)`)

## 2. Implementation Steps

### Implementation Phase 1: Create Module/System Skeleton

- GOAL-001: Scaffold the `Module/System/` directory structure, extension class, and empty domain skeleton. Wire into `Program.cs` so the module exists and the build passes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `service/Api/src/Module/System/` directory with subdirectories `Domain/Currencies/`, `Domain/SystemInfos/`, `Domain/SystemDateTimes/`, `Persistence/`, `Features/Admin/Currencies/`, `Features/Admin/SystemInfo/`, `Features/Storefront/` | | |
| TASK-002 | Create `service/Api/src/Module/System/System.Extension.cs` — `public static class SystemExtensions` with `AddSystemModule(this WebApplicationBuilder)` method. Initially empty service registrations. | | |
| TASK-003 | Add `using Module.System;` import and `builder.AddSystemModule();` call in `Program.cs` at line 38, before `builder.AddLocationModule()` | | |
| TASK-004 | Create `service/Api/src/Module/System/README.yaml` with domain description (consistent with other modules) | | |
| TASK-005 | Verify `dotnet build` passes cleanly (warnings-as-errors) | | |

### Implementation Phase 2: Centralize Currency Domain in Module/System

- GOAL-002: Create the `Currency` value object and constant definitions in `Module/System/Domain/Currencies/`, consolidating all hardcoded currency defaults and ISO 4217 constraints from Ordering, Catalog, Payment, and Shipping.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `service/Api/src/Module/System/Domain/Currencies/SystemCurrencyConstant.cs` — consolidate `OrderConstant.Constraints.MaxCurrencyLength(3)`, `PriceConstant.Constraints.CurrencyMaxLength(3)`, `PaymentConstant.Constraints.MaxCurrencyLength(3)` into single source: `SystemCurrencyConstant.Constraints.MaxCodeLength = 3`. Consolidate `OrderConstant.Defaults.Currency("USD")`, `PriceConstant.Default.Currency("USD")`, `GatewayConstants.Currency.Usd("USD")` into `SystemCurrencyConstant.Defaults.Code = "USD"`. Add `SystemCurrencyConstant.Defaults.Symbol = "$"`. Add `SystemCurrencyConstant.Defaults.Name = "US Dollar"`. Add `SystemCurrencyConstant.Constraints.MonetaryPrecision = 18` and `SystemCurrencyConstant.Constraints.MonetaryScale = 2`. | | |
| TASK-007 | Create `service/Api/src/Module/System/Domain/Currencies/SystemCurrencyResult.cs` — define `CurrencyInvalid`, `CurrencyTooLong`, `CurrencyNotSupported` error factories with codes `System.Currency.*` | | |
| TASK-008 | Create `service/Api/src/Module/System/Domain/Currencies/SystemCurrencyValidation.cs` — define `ApplyCurrencyRules<T>(this IRuleBuilder<T, string>)` method with `.NotEmpty()` + `.MaximumLength(SystemCurrencyConstant.Constraints.MaxCodeLength)`. This replaces the 3 duplicated `ApplyCurrencyRules` in Ordering, Catalog, Payment. | | |
| TASK-009 | Create `service/Api/src/Module/System/Domain/Currencies/SystemCurrency.cs` — `public sealed record SystemCurrency(string Code, string Symbol, string Name, int NumericCode)`. Include a static `IReadOnlyDictionary<string, SystemCurrency> Supported` with USD, EUR, GBP entries. Include `public static SystemCurrency Default => Supported[SystemCurrencyConstant.Defaults.Code]`. Include `public static bool IsSupported(string code)` check. | | |
| TASK-010 | Create `service/Api/src/Module/System/Domain/Money/SystemMoney.cs` — `public readonly record struct SystemMoney(decimal Amount, string CurrencyCode)` with `ToString()` that formats as `"{Amount:F2} {CurrencyCode}"`. This is a lightweight value object (not a full Money type with arithmetic). | | |
| TASK-011 | Add `using Module.System.Domain.Currencies;` to `service/Api/src/Module/GlobalUsing.cs` | | |
| TASK-012 | Register `SystemCurrencyValidation` in `System.Extension.cs` via `builder.Services.AddScoped` if needed (validation methods are static — no registration needed). | | |

### Implementation Phase 3: Refactor Ordering Module to Use System Currency

- GOAL-003: Replace all hardcoded currency references in Ordering with `SystemCurrencyConstant.Defaults.Code`. Update `OrderConstant` to reference System currency. Update feature handlers to read config from `System:DefaultCurrency`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Update `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs` — replace `OrderConstant.Constraints.MaxCurrencyLength` with reference to `SystemCurrencyConstant.Constraints.MaxCodeLength`. Replace `OrderConstant.Defaults.Currency = "USD"` with `OrderConstant.Defaults.Currency = SystemCurrencyConstant.Defaults.Code`. Keep the field as a redirect constant for backward compat. | | |
| TASK-014 | Update `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs` — replace the body of `ApplyCurrencyRules` to delegate to `SystemCurrencyValidation.ApplyCurrencyRules` (or simply remove the Ordering version and have callers use `SystemCurrencyValidation.ApplyCurrencyRules` directly). Keep the method signature for backward compat if needed, but delegate internally. | | |
| TASK-015 | Update `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` — keep `CurrencyInvalid` and `CurrencyTooLong` errors but change the `code` from `Order.Currency.*` to reference `SystemCurrencyResult` errors, or keep them and update messages to reference the centralized constant. Decision: keep local error codes for Ordering-specific context but delegate message content. | | |
| TASK-016 | Update `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — change `configuration["Ordering:DefaultCurrency"]` to `configuration["System:DefaultCurrency"]` | | |
| TASK-017 | Verify `dotnet build` passes cleanly (warnings-as-errors) | | |

### Implementation Phase 4: Refactor Catalog Module to Use System Currency

- GOAL-004: Replace hardcoded `PriceConstant.Default.Currency` and `PriceValidation.ApplyCurrencyRules` with System currency references.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Update `service/Api/src/Module/Catalog/Domain/Products/Variants/Prices/Price.Constant.cs` — replace `PriceConstant.Default.Currency = "USD"` with `PriceConstant.Default.Currency = SystemCurrencyConstant.Defaults.Code`. Replace `PriceConstant.Constraints.CurrencyMaxLength(3)` with reference to `SystemCurrencyConstant.Constraints.MaxCodeLength`. Keep the local constant fields as redirects. | | |
| TASK-019 | Update `service/Api/src/Module/Catalog/Domain/Products/Variants/Prices/Price.Validation.cs` — replace body of `ApplyCurrencyRules` to delegate to `SystemCurrencyValidation.ApplyCurrencyRules` | | |
| TASK-020 | Update `service/Api/src/Module/Catalog/Domain/Products/Variants/Prices/Price.Result.cs` — update `CurrencyRequired` and `CurrencyTooLong` error messages to reference centralized system validation | | |
| TASK-021 | Verify `dotnet build` passes cleanly | | |

### Implementation Phase 5: Refactor Payment Module to Use System Currency

- GOAL-005: Replace `GatewayConstants.Currency.Usd` and `PaymentCaptureValidation.ApplyCurrencyRules` with System currency references.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Update `service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs` — replace `GatewayConstants.Currency.Usd = "USD"` with `GatewayConstants.Currency.Usd = SystemCurrencyConstant.Defaults.Code`. Keep the GatewayConstants field as a redirect for backward compat. | | |
| TASK-023 | Update `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs` — replace body of `ApplyCurrencyRules` to delegate to `SystemCurrencyValidation.ApplyCurrencyRules`. Update `Length()` to use `SystemCurrencyConstant.Constraints.MaxCodeLength`. | | |
| TASK-024 | Update `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCaptureResult.cs` — update `CurrencyRequired` and `CurrencyInvalid` error messages | | |
| TASK-025 | Verify `dotnet build` passes cleanly | | |

### Implementation Phase 6: Refactor Shipping Module to Use System Currency

- GOAL-006: Replace hardcoded `"USD"` in Shipping mappings with `SystemCurrencyConstant.Defaults.Code`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-026 | Update `service/Api/src/Module/Shipping/Features/Storefront/Shared/Mappings/ShippingMethod.Mapping.Model.cs` — replace `Currency = "USD"` with `Currency = SystemCurrencyConstant.Defaults.Code` in both `MapToDetail` and `MapToListItem` methods | | |
| TASK-027 | Verify `dotnet build` passes cleanly | | |

### Implementation Phase 7: Move SystemInfo Implementation to Module/System

- GOAL-007: Move `ISystemInfo` and `ISystemDateTime` implementations from `Shared/Application/Systems/` to `Module/System/Domain/`. Keep interfaces in Shared as abstractions.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Copy `service/Api/src/Shared/Application/Systems/SystemInfos/SystemInfo.Implementation.cs` to `service/Api/src/Module/System/Domain/SystemInfos/SystemInfo.cs` — update namespace to `Module.System.Domain.SystemInfos`. Class remains `public sealed class SystemInfo(IHostEnvironment environment) : ISystemInfo`. No interface changes. | | |
| TASK-029 | Copy `service/Api/src/Shared/Application/Systems/SystemDateTimes/SystemDateTime.Implementation.cs` to `service/Api/src/Module/System/Domain/SystemDateTimes/SystemDateTime.cs` — update namespace to `Module.System.Domain.SystemDateTimes`. Class remains `public sealed class SystemDateTime : ISystemDateTime`. No interface changes. | | |
| TASK-030 | Update `service/Api/src/Module/System/System.Extension.cs` — add registration calls: `builder.Services.TryAddSingleton<ISystemInfo, SystemInfo>()` and `builder.Services.TryAddSingleton<ISystemDateTime, SystemDateTime>()` | | |
| TASK-031 | Update `service/Api/src/Shared/Application/Systems/Systems.Extension.cs` — remove the implementation registrations, keeping only `#region Service Registration` and `#region Pipeline Configuration` with the `UseSystems()` method that warms up `ISystemDateTime`. The `AddSystems()` method should become empty (or removed entirely and callers updated). Decision: keep `AddSystems()` but remove the TryAddSingleton calls; `UseSystems()` remains. | | |
| TASK-032 | Update `service/Api/src/Shared/Application/Application.Extension.cs` — remove the call to `builder.AddSystems()` since System registrations now happen in `Module/System/System.Extension.cs` via `builder.AddSystemModule()` in Program.cs. The `app.UseSystems()` call in `UseApplication()` stays (it warms up the service). | | |
| TASK-033 | Verify that `ISystemInfo` interface and `ISystemDateTime` interface remain in Shared (`Shared/Application/Systems/SystemInfos/SystemInfo.Interface.cs` and `Shared/Application/Systems/SystemDateTimes/SystemDateTime.Interface.cs`) — no changes needed to interface files. | | |
| TASK-034 | Verify `dotnet build` passes cleanly — all modules and Shared reference interfaces by namespace which hasn't changed. Implementations are resolved from the new location via DI. | | |

### Implementation Phase 8: Update Configuration and Tests

- GOAL-008: Update appsettings.json config key, verify unit tests pass, update .harness domain definitions.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-035 | Update `service/Api/src/Api/appsettings.json` — move `"DefaultCurrency": "USD"` from `Ordering:` section to a new `System: DefaultCurrency` section. Remove from Ordering section. | | |
| TASK-036 | Update `service/Api/src/Api/appsettings.Development.json` — same move as TASK-035 | | |
| TASK-037 | Clean up `OrderConstant.Constraints` — remove `MaxCurrencyLength` and `Precision`/`Scale` if they duplicate `SystemCurrencyConstant`. Keep `OrderConstant.Constraints` for Ordering-specific constants only. | | |
| TASK-038 | Remove the now-unnecessary `OrderConstant.Defaults.Currency` if all Ordering code uses `SystemCurrencyConstant.Defaults.Code` directly. Decision: keep `OrderConstant.Defaults.Currency` as a `public const string Currency = SystemCurrencyConstant.Defaults.Code` redirected constant to minimize diff churn in feature files. | | |
| TASK-039 | Update `service/Api/tests/Module.UnitTests/GlobalUsing.cs` — add `using Module.System.Domain.Currencies;` if any test references System currency types directly | | |
| TASK-040 | Verify all tests pass: `dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests` | | |
| TASK-041 | Update `.harness/domains.yml` — add a new `System` domain entry under `domains:` with description "System configuration — currency, application metadata, system clock". Set `size_loc` to estimated initial size. | | |
| TASK-042 | Update `docs/codebase/ARCHITECTURE.md` — add `Module/System/` to the module list, update the System Flow description if needed, update Layer/Module Responsibilities table. | | |

## 3. Alternatives

- **ALT-001**: Keep currency defaults in each module — rejected because it violates DRY and creates maintenance burden when adding new currencies. Each module independently hardcodes "USD", making multi-currency support harder.
- **ALT-002**: Place currency types in Shared instead of Module.System — rejected because Shared already has `DisplayMoney` concern and adding currency domain logic would bloat it. Currency is a business concept that belongs in the Module layer, and Module.System provides a natural home.
- **ALT-003**: Keep ISystemInfo/ISystemDateTime implementations in Shared — rejected because the user explicitly requested moving System informations. Moving implementations to Module.System aligns with creating a proper system domain while keeping the abstraction interfaces in Shared (preserving the dependency rule).
- **ALT-004**: Move ISystemInfo/ISystemDateTime interfaces to Module.System — rejected because Shared (AppDbContext, interceptors, etc.) uses these interfaces and cannot reference Module. The interfaces must stay in Shared as the lowest-layer abstraction.

## 4. Dependencies

- **DEP-001**: No external NuGet packages required — all types are in-net10.0
- **DEP-002**: No changes to EF Core migrations required — `Order.Currency` column definition (varchar(3), default "USD") stays the same; only the constant source changes
- **DEP-003**: No changes to frontend SPAs required — wire DTOs still carry `string Currency`; frontend formatting remains hardcoded USD (future work)

## 5. Files

### New files (10)
- **FILE-001**: `service/Api/src/Module/System/System.Extension.cs` — module DI registration
- **FILE-002**: `service/Api/src/Module/System/Domain/Currencies/SystemCurrencyConstant.cs` — centralized constants
- **FILE-003**: `service/Api/src/Module/System/Domain/Currencies/SystemCurrency.cs` — Currency value object
- **FILE-004**: `service/Api/src/Module/System/Domain/Currencies/SystemCurrencyValidation.cs` — validation rules
- **FILE-005**: `service/Api/src/Module/System/Domain/Currencies/SystemCurrencyResult.cs` — error factories
- **FILE-006**: `service/Api/src/Module/System/Domain/Money/SystemMoney.cs` — Money value object
- **FILE-007**: `service/Api/src/Module/System/Domain/SystemInfos/SystemInfo.cs` — moved implementation
- **FILE-008**: `service/Api/src/Module/System/Domain/SystemDateTimes/SystemDateTime.cs` — moved implementation
- **FILE-009**: `service/Api/src/Module/System/README.yaml` — domain metadata
- **FILE-010**: (empty directories) `Persistence/`, `Features/Admin/Currencies/`, `Features/Admin/SystemInfo/`, `Features/Storefront/`

### Modified files (16)
- **FILE-011**: `service/Api/src/Api/Program.cs` — add `using Module.System;` + `builder.AddSystemModule()`
- **FILE-012**: `service/Api/src/Module/GlobalUsing.cs` — add `using Module.System.Domain.Currencies;`
- **FILE-013**: `service/Api/src/Shared/Application/Systems/Systems.Extension.cs` — remove implementation registrations
- **FILE-014**: `service/Api/src/Shared/Application/Application.Extension.cs` — remove `builder.AddSystems()` call
- **FILE-015**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs` — redirect defaults
- **FILE-016**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs` — delegate to SystemCurrencyValidation
- **FILE-017**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — update config key
- **FILE-018**: `service/Api/src/Module/Catalog/Domain/Products/Variants/Prices/Price.Constant.cs` — redirect defaults
- **FILE-019**: `service/Api/src/Module/Catalog/Domain/Products/Variants/Prices/Price.Validation.cs` — delegate to SystemCurrencyValidation
- **FILE-020**: `service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs` — redirect Currency.Usd
- **FILE-021**: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs` — delegate to SystemCurrencyValidation
- **FILE-022**: `service/Api/src/Module/Shipping/Features/Storefront/Shared/Mappings/ShippingMethod.Mapping.Model.cs` — use constant
- **FILE-023**: `service/Api/src/Api/appsettings.json` — move DefaultCurrency to System section
- **FILE-024**: `service/Api/src/Api/appsettings.Development.json` — move DefaultCurrency to System section
- **FILE-025**: `.harness/domains.yml` — add System domain entry
- **FILE-026**: `docs/codebase/ARCHITECTURE.md` — update module list

### Unchanged but verified
- **FILE-NC-001**: `Shared/Application/Systems/SystemInfos/SystemInfo.Interface.cs` — stays in Shared
- **FILE-NC-002**: `Shared/Application/Systems/SystemDateTimes/SystemDateTime.Interface.cs` — stays in Shared
- **FILE-NC-003**: All EF Core configuration files — column definitions unchanged (constant values are same)

## 6. Testing

- **TEST-001**: `dotnet build` — must pass with zero warnings (warnings-as-errors)
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all existing tests pass
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` — all existing tests pass
- **TEST-004**: Manual verification that `configuration["System:DefaultCurrency"]` resolves `"USD"` from appsettings.json
- **TEST-005**: Manual verification that `ISystemDateTime` and `ISystemInfo` resolve correctly from Module.System registrations (not Shared)

## 7. Risks & Assumptions

- **RISK-001**: Order of registration — `AddSystemModule()` must be called before any module that uses `ISystemDateTime` or `ISystemInfo`. Currently all modules use `TryAddSingleton` for these, but if Shared registers first via `AddSystems()`, the Module.System registration would be skipped. **Mitigation**: TASK-032 removes `builder.AddSystems()` from `Application.Extension.cs`, ensuring only Module.System registers implementations.
- **RISK-002**: Test project `GlobalUsing.cs` references `Shared.Application.Systems.SystemDateTimes` — this namespace is unchanged (interfaces stay in Shared), so tests continue to compile.
- **RISK-003**: The `UseSystems()` method in `Shared/Application/Systems/Systems.Extension.cs` warms up `ISystemDateTime` — after the move, the resolved implementation comes from Module.System (not Shared) but this is transparent to the consumer.
- **ASSUMPTION-001**: No code reads `configuration["Ordering:DefaultCurrency"]` except `AddToCart.cs` (verified by codebase exploration). If any other code reads this key, it will break silently (return null, fall back to default).
- **ASSUMPTION-002**: The `System` module domain does not need persistence initially — currency is a value object, not an entity. If a persisted Currency entity is needed later, it can be added to `System/Persistence/` in a follow-up plan.

## 8. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — module responsibility table
- `.harness/domains.yml` — domain layer definitions
- `.harness/principles.yml` — vertical slice isolation, forward-only dependency, domain entity behavior
- `docs/codebase/CONVENTIONS.md` — partial class entity pattern conventions
- `service/Api/src/Module/GlobalUsing.cs` — module-wide using directives
