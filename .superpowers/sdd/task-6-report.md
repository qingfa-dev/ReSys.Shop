# Task 6: Check Void Results in VoidOrderPayments — Report

## Summary

`VoidOrderPaymentsCommandHandler.Handle` was swallowing the result of `VoidTransactionAsync`. If the gateway declined the void, the handler would still return `Result.Ok()` and call `SaveChangesAsync`. Fixed by checking the result and short-circuiting on failure.

## Changes

### `service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs`

- Added result check after `VoidTransactionAsync` call (line 47-49)
- If `voidResult.IsFailure` → returns `voidResult.Errors` immediately (implicit conversion `List<Error>` → `Result`)
- `SaveChangesAsync` and `Result.Ok()` now only reached if all voids succeed

### `service/Api/tests/Module.UnitTests/Payment/Features/Shared/Commands/VoidOrderPaymentsTests.cs`

- Follows existing `IDisposable` pattern with in-memory EF Core + Moq (same as `VoidPaymentTests`)
- Tests two scenarios:
  - `Handle_Should_Fail_When_Void_Fails` — sets up `VoidTransactionAsync` to return an error, asserts `IsFailure`
  - `Handle_Should_Succeed_When_Void_Succeeds` — sets up success return, asserts `IsSuccess`

## Verification

- RED: Test failed with "Expected result.IsFailure to be True, but found False."
- GREEN: Both tests pass
- All 131 Payment unit tests pass (no regressions)
