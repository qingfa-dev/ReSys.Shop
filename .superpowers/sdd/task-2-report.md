# Task 2: Remove Hardcoded Cart Defaults — Implementation Report

## Changes Made

### 1. Configuration (`appsettings.json` + `appsettings.Development.json`)
- Added `"DefaultCurrency": "USD"` under the `"Ordering"` section in both files.

### 2. `Order.Extensions.cs` — `Create` method
- Added `Guid? shipAddressId = null` parameter (last position).
- Sets `ShipAddressId = shipAddressId` on the created `Order` entity.
- Existing callers unchanged (defaults to `null`).

### 3. `AddToCart.cs` — `CommandHandler`
- Injected `IConfiguration configuration` via primary constructor.
- Changed `"USD"` literal to `configuration["Ordering:DefaultCurrency"] ?? "USD"`.
- Added explicit `shipAddressId: null` argument to `OrderExtensions.Create` call.

### 4. `AddToCartTests.cs` — existing test
- Added `Mock<IConfiguration>` setup returning `"USD"` for `["Ordering:DefaultCurrency"]`.
- Passes `_configurationMock.Object` to handler constructor.

### 5. `AddToCartDefaultsTests.cs` — new test
- Verifies cart `Currency` equals configured value (`"USD"`).
- Verifies `ShipAddressId` is `null`.

## Verification
- Build: 0 warnings, 0 errors.
- Tests: 2315 passed, 2 pre-existing failures (architecture isolation, admin update-status).
- New test: passes.
