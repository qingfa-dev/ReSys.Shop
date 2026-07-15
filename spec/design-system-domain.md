---
title: System Domain — Shared Currency Value Objects & Infrastructure Abstractions
version: 1.0
date_created: 2026-07-15
owner: Platform Team
tags: shared, domain, currency, infrastructure
---

# System Domain

Centralize currency value objects (`Currency`, `Money`), ISO 4217 constants/validation, and `ISystemInfo`/`ISystemDateTime` implementations in `Shared/Application/` — the lowest architectural layer. `Module/SystemModule` was rejected because it creates a C# namespace conflict (`Module.System.*` clashes with `System.*` framework namespaces) and violates the forward-only dependency rule (Shared cannot depend on Module).

## 1. Purpose & Scope

Define the canonical home for cross-cutting system concepts that must be available to every module without circular dependencies. Scope includes currency value objects, monetary precision/scale constants, ISO 4217 validation rules, and the concrete implementations of `ISystemInfo` and `ISystemDateTime`.

## 2. Definitions

| Term | Definition |
|------|-----------|
| `SystemCurrency` | Immutable value object (record) carrying ISO 4217 code/symbol/name/numeric code |
| `SystemMoney` | Lightweight `Amount + CurrencyCode` record struct |
| `SystemCurrencyConstant` | Static class with `Constraints.MaxCodeLength`, `Defaults.Code`, `MonetaryPrecision/Scale` |
| `SystemCurrencyValidation` | FluentValidation extension method `ApplyCurrencyRules` |
| `SystemCurrencyResult` | `Error` factories for currency validation failures |
| Shared | `service/Api/src/Shared/` — the lowest .NET project, zero dependencies on Module |

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: `SystemCurrencyConstant` is the single source of truth for `MaxCodeLength=3`, `MonetaryPrecision=18`, `MonetaryScale=2`, default code `"USD"`
- **REQ-002**: `SystemCurrencyValidation.ApplyCurrencyRules` is the canonical FluentValidation extension; modules delegate to it rather than duplicating rules
- **REQ-003**: `SystemInfo` and `SystemDateTime` implementations live in `Shared/Application/Systems/` alongside their interfaces — no Module involvement
- **REQ-004**: `Ordering`, `Catalog`, `Payment`, `Shipping` modules' currency constants are redirected to `SystemCurrencyConstant` (e.g., `OldConstant = SystemCurrencyConstant.Defaults.Code`)
- **REQ-005**: `appsettings.json` config key moves from `Ordering:DefaultCurrency` to `System:DefaultCurrency`
- **CON-001**: Forward-only dependency — Shared depends on nothing inside `service/`; Module depends on Shared
- **CON-002**: No namespace conflict — `Shared.Application.Domain.*` does not collide with `System.*` framework namespaces
- **PAT-001**: Value objects (records) for Currency/Money, static constants/validation/result classes follow existing Shared conventions

## 4. Interfaces & Data Contracts

```csharp
// ——— Value Objects ———

namespace Shared.Application.Domain.Currencies;

public sealed record SystemCurrency(string Code, string Symbol, string Name, int NumericCode);

// ——— Constants ———

public static class SystemCurrencyConstant
{
    public static class Constraints { public const int MaxCodeLength = 3; /* ... */ }
    public static class Defaults    { public const string Code = "USD"; /* ... */ }
}

// ——— Validation ———

public static class SystemCurrencyValidation
{
    public static IRuleBuilderOptions<T, string> ApplyCurrencyRules<T>(
        this IRuleBuilder<T, string> ruleBuilder);
}

// ——— Results ———

public static class SystemCurrencyResult
{
    public static Error CurrencyInvalid    => /* code: "System.Currency.Invalid" */;
    public static Error CurrencyTooLong    => /* code: "System.Currency.TooLong" */;
    public static Error CurrencyNotSupported => /* code: "System.Currency.NotSupported" */;
}

// ——— Money ———

namespace Shared.Application.Domain.Money;

public readonly record struct SystemMoney(decimal Amount, string CurrencyCode);
```

## 5. Acceptance Criteria

- **AC-001**: `dotnet build` passes with 0 warnings/errors (warnings-as-errors)
- **AC-002**: All 3 duplicated `ApplyCurrencyRules` methods (Ordering, Catalog, Payment) delegate to or reference `SystemCurrencyValidation`/`SystemCurrencyConstant`
- **AC-003**: `ShippingMethod.Mapping.Model.cs` uses `SystemCurrencyConstant.Defaults.Code` instead of hardcoded `"USD"`
- **AC-004**: `AddToCart.cs` reads `configuration["System:DefaultCurrency"]` instead of `"Ordering:DefaultCurrency"`
- **AC-005**: `ISystemDateTime` and `ISystemInfo` resolve at runtime without ambiguity (only Shared implementations registered)

## 6. Test Automation Strategy

- **Unit tests**: Existing `Module.UnitTests` (2471 tests, 1 pre-existing failure) and `Shared.UnitTests` (2424 tests, 12 pre-existing failures) continue to pass
- **No new tests needed**: Zero behavioral change — only constant source relocation and validation delegation
- **CI gate**: `dotnet build` on PR must report 0 warnings/errors

## 7. Rationale & Context

The initial approach placed Currency/Money in `Module/SystemModule` with namespace `Module.SystemModule.Domain.Currencies`. However:

1. **Namespace conflict**: `Module.System` (even as `Module.SystemModule`) creates C# resolution ambiguity with `System.*` framework types used pervasively (e.g., `System.ComponentModel`, `System.Globalization`, `System.Security`). Every file in the Module assembly would need `global::System.*` prefixes — fragile and high-churn.

2. **Dependency inversion**: Currency is a fundamental value object used by every module. Placing it in `Module` forces all consumers to depend on a Module assembly for a simple value type. Shared is the correct layer for cross-cutting value objects.

3. **Precedent**: `ISystemDateTime` and `ISystemInfo` already live in Shared. Their implementations belong alongside their interfaces.

## 8. Dependencies & External Integrations

### External Systems
None.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 — all types are BCL (`System`, `FluentValidation`)
- **PLT-002**: FluentValidation (already in Shared.csproj) — `SystemCurrencyValidation`

### Infrastructure Dependencies
None.

## 9. Files

### New (Shared)
| File | Purpose |
|------|---------|
| `Shared/Application/Domain/Currencies/SystemCurrency.cs` | Currency value object |
| `Shared/Application/Domain/Currencies/SystemCurrencyConstant.cs` | ISO 4217 constants & defaults |
| `Shared/Application/Domain/Currencies/SystemCurrencyValidation.cs` | FluentValidation extension |
| `Shared/Application/Domain/Currencies/SystemCurrencyResult.cs` | Error factories |
| `Shared/Application/Domain/Money/SystemMoney.cs` | Money value object |

### Restored (Shared)
| File | Purpose |
|------|---------|
| `Shared/Application/Systems/SystemInfos/SystemInfo.Implementation.cs` | Restored from original |
| `Shared/Application/Systems/SystemDateTimes/SystemDateTime.Implementation.cs` | Restored from original |
| `Shared/Application/Systems/Systems.Extension.cs` | Restored `AddSystems()` with registrations |
| `Shared/Application/Application.Extension.cs` | Restored `builder.AddSystems()` call |

### Modified
| File | Change |
|------|--------|
| `Module/Ordering/Domain/Orders/Order.Constant.cs` | Currency → `SystemCurrencyConstant.Defaults.Code` |
| `Module/Ordering/Domain/Orders/Order.Validation.cs` | Delegates to `SystemCurrencyValidation` |
| `Module/Ordering/Domain/Orders/Order.Result.cs` | Message uses `SystemCurrencyConstant.Constraints.MaxCodeLength` |
| `Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` | Config key → `System:DefaultCurrency` |
| `Module/Catalog/Domain/Products/Variants/Prices/Price.Constant.cs` | Currency/MaxLength/Presision/Scale → SystemCurrencyConstant |
| `Module/Catalog/Domain/Products/Variants/Prices/Price.Validation.cs` | MaxLength → `SystemCurrencyConstant.Constraints.MaxCodeLength` |
| `Module/Catalog/Domain/Products/Variants/Prices/Price.Result.cs` | Message uses `SystemCurrencyConstant.Constraints.MaxCodeLength` |
| `Module/Payment/Services/Provider/GatewayConstants.cs` | Usd → `SystemCurrencyConstant.Defaults.Code` |
| `Module/Payment/Domain/PaymentCaptures/PaymentCapture.Constant.cs` | MaxCurrencyLength/Presision/Scale/Defaults → SystemCurrencyConstant |
| `Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs` | Length → `SystemCurrencyConstant.Constraints.MaxCodeLength` |
| `Module/Shipping/Features/Storefront/Shared/Mappings/ShippingMethod.Mapping.Model.cs` | `"USD"` → `SystemCurrencyConstant.Defaults.Code` |
| `Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs` | `"USD"` → `SystemCurrencyConstant.Defaults.Code` |
| `Api/appsettings.json` | `Ordering:DefaultCurrency` → `System:DefaultCurrency` |
| `Api/appsettings.Development.json` | `Ordering:DefaultCurrency` → `System:DefaultCurrency` |

### Deleted
| File | Reason |
|------|--------|
| `Module/SystemModule/` (entire tree) | Replaced by Shared location |

## 10. Validation Criteria

- Build with zero warnings: `dotnet build service/Api/src/Api/Api.csproj`
- Module unit tests: `dotnet test service/Api/tests/Module.UnitTests --no-build` (1 pre-existing failure allowed)
- Shared unit tests: `dotnet test service/Api/tests/Shared.UnitTests --no-build` (12 pre-existing failures allowed)

## 11. Related Specifications / Further Reading

- `plan/refactor-system-module-1.md` — original implementation plan (superseded by this spec)
- `.harness/domains.yml` — Shared.Infrastructure domain definition
- `.harness/principles.yml` — forward-only dependency, domain entity behavior
- `docs/codebase/ARCHITECTURE.md` — layer/module responsibility table
