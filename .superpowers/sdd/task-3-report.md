# Task 3 Report: Rename Order.Checkout.cs → Order.Method.Checkout.cs + fix bugs

## Status: DONE

## Commits
- `1bd83710` — refactor: rename Order.Checkout.cs, remove dead stubs, fix negations

## Changes
1. **Renamed** `Order.Checkout.cs` → `Order.Method.Checkout.cs` via `git mv`
2. **Removed** `AfterCancel()` and `AfterResume()` stubs (including `#region State Machine Callbacks`, `#pragma warning` lines)
3. **Fixed REQ-020 (double negation):** `EnsureLineItemVariantsAreNotDiscontinued` changed from `!LineItems.Any(...)` to `LineItems.All(...)` with proper `!discontinuedVariantIds.Contains` check
4. **Fixed REQ-019:** `EnsureLineItemsPresent` changed from multi-line if-block to single-expression `return LineItems.Count > 0`
5. **Verified zero callers** of `AfterCancel`/`AfterResume` across `service/Api/src/` and `service/Api/tests/`
6. **Fixed orphaned `#endregion`** after removing the `State Machine Callbacks` region

## Test Results
```
total: 5, failed: 0, succeeded: 5
```
- OrderCheckoutTests.AssignDefaultAddresses_Should_Set_Addresses_When_Null ✓
- OrderCheckoutTests.AssignDefaultAddresses_Should_Not_Overwrite_Existing_Addresses ✓
- OrderCheckoutTests.AssignDefaultAddresses_Should_Not_Set_When_Null_Provided ✓
- OrderDiscontinuedTests.EnsureLineItemVariantsAreNotDiscontinued_Should_Return_False_When_Variant_Discontinued ✓
- OrderDiscontinuedTests.EnsureLineItemVariantsAreNotDiscontinued_Should_Return_True_When_No_Variant_Discontinued ✓

## Concerns
- The `#region State Machine Callbacks` block's closing `#endregion` became orphaned after removing the `#region` opening directive — this was caught by the build (CS1028) and fixed.
- `dotnet test --filter` flag is deprecated in xUnit v3; used `--filter-class` with wildcard instead.
