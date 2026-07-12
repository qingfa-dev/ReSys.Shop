# Task 3: Wire Stock Reservation Into AddToCart — Report

## Status

DONE_WITH_CONCERNS

## Commits

```
37a6b0b0 feat(ordering): wire ReserveCartStock into AddToCart handler
```

## Test Results

**New reservation tests (2/2):** PASS
- `AddToCart: Dispatches ReserveCartStock after stock validation` — PASS
- `AddToCart: Returns failure when reservation fails` — PASS

**Existing AddToCart tests (3/3):** PASS
- `AddToCartTests.Handle_ShouldAddItem_WhenVariantExists` — PASS
- `AddToCartTests.Handle_ShouldReturnFailure_WhenVariantNotFound` — PASS
- `AddToCartDefaultsTests.Handle_ShouldCreateCart_WithConfiguredCurrencyAndNoDefaultAddress` — PASS

**Full Module.UnitTests suite (2389/2390):** PASS (1 skipped — pre-existing CartMapping stub)

**Build:** 0 Warning(s), 0 Error(s)

## Concerns

1. **SetPrice does not exist on Variant** — The brief's test code uses `variant.SetPrice(9.99m)` which doesn't exist on the `Variant` class. Replaced with direct property assignment `Price = 9.99m`, matching the existing test pattern.

2. **StockItem internal constructor** — The brief's test used `new StockItem { ... }` which depends on `InternalsVisibleTo`. This compiled but caused the test to fail because the StockLocation navigation property wasn't resolved. Fixed by using `StockItemMethod.Create(...)` with a seeded `StockLocation`, matching the existing test pattern.

3. **Pre-existing `OrderResult.Failure` build errors** — 12 source files and test files referenced `OrderResult.Failure` but `OrderResult` only has `.Errors`. These were pre-existing compilation errors hidden by incremental build caching. A full rebuild (triggered by adding the new test file) exposed them. Fixed all 12 occurrences to `OrderResult.Errors`. Without this fix, no tests could compile or run.

4. **Existing tests needed mock setup** — The `ISender` parameter added to the `CommandHandler` constructor required existing tests (`AddToCartTests`, `AddToCartDefaultsTests`) to provide a mock with a default success setup for `ISender.Send` with `IRequest<Result<ReserveCartStock.Response>>`. Otherwise, the reservation dispatch returned `default(Result<T>)` (IsSuccess=false), causing existing tests to fail.

5. **xUnit v3 filter syntax** — The brief's `dotnet test --filter "FullyQualifiedName~..."` syntax is not supported by xUnit v3's test platform. Used `--filter-class` instead.
