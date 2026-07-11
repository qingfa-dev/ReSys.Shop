---
goal: Abstract transaction support in IApplicationDbContext with provider-aware checks
version: 1.0
date_created: 2026-07-11
status: 'In progress'
tags: refactor, infrastructure, transactions, testing
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

Replace the raw `DatabaseFacade Database { get; }` property on `IApplicationDbContext` with an abstracted transaction API that checks whether the current database provider supports transactions. This eliminates the `InMemoryEventId.TransactionIgnoredWarning` suppression hack in tests and provides a clean, testable transaction abstraction.

## 1. Requirements & Constraints

- **REQ-001**: Remove `DatabaseFacade Database { get; }` from `IApplicationDbContext`
- **REQ-002**: Add `Task<IDatabaseTransaction> BeginTransactionAsync(...)` and `bool SupportsTransactions` to the interface
- **REQ-003**: Create `IDatabaseTransaction` with `CommitAsync`, `RollbackAsync`, `IAsyncDisposable`
- **REQ-004**: Create `EfCoreTransaction` wrapping real EF Core transactions
- **REQ-005**: Create `NoOpTransaction` for providers that don't support transactions (e.g. in-memory)
- **REQ-006**: `ApplicationDbContext.SupportsTransactions` returns `false` for in-memory, `true` for relational
- **REQ-007**: Update `ReserveCartStock.cs` to use new abstraction
- **REQ-008**: Remove `ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))` from tests
- **REQ-009**: Remove `using Microsoft.EntityFrameworkCore.Infrastructure;` from wrapper if no longer needed
- **CON-001**: `TreatWarningsAsErrors=true` globally
- **CON-002**: All handlers return `Result<T>` or `Result`
- **PAT-001**: Follow existing `static partial class` vertical slice pattern
- **PAT-002**: New types go in `Shared/Operational/Persistence/Transactions/`

## 2. Implementation Steps

### Implementation Phase 1 — Create transaction abstractions

- GOAL-001: Create transaction types in Shared and update the interface

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `IDatabaseTransaction` interface | | |
| TASK-002 | Create `EfCoreTransaction` implementation | | |
| TASK-003 | Create `NoOpTransaction` implementation | | |
| TASK-004 | Update `IApplicationDbContext` — remove `Database`, add transaction methods | | |
| TASK-005 | Implement transaction methods in `ApplicationDbContext` | | |
| TASK-006 | Update `ReserveCartStock.cs` to use new abstraction | | |
| TASK-007 | Remove `ConfigureWarnings` hack from ReserveCartStock tests | | |
| TASK-008 | Add unit tests for transaction types | | |
| TASK-009 | Build and run full test suite | | |

## 3. Alternatives

- **ALT-001**: Keep `DatabaseFacade` but add extension methods — still exposes raw EF Core type in the interface
- **ALT-002**: Use a separate `ITransactionService` injected via DI — more ceremony for a single consumer
- **ALT-003**: Keep the `ConfigureWarnings` suppression — current state, but it's a hack that masks real issues

## 4. Dependencies

- **DEP-001**: `IApplicationDbContext` in `Shared/Operational/Persistence/Data/AppDbContext.Wrapper.cs`
- **DEP-002**: `ApplicationDbContext` in `Shared/Operational/Persistence/Data/AppDbContext.cs`
- **DEP-003**: `ReserveCartStock.cs` in Inventory module
- **DEP-004**: `ReserveCartStock.Tests.cs` in Module.UnitTests

## 5. Files

- **FILE-001**: `service/Api/src/Shared/Operational/Persistence/Transactions/IDatabaseTransaction.cs` — new
- **FILE-002**: `service/Api/src/Shared/Operational/Persistence/Transactions/EfCoreTransaction.cs` — new
- **FILE-003**: `service/Api/src/Shared/Operational/Persistence/Transactions/NoOpTransaction.cs` — new
- **FILE-004**: `service/Api/src/Shared/Operational/Persistence/Data/AppDbContext.Wrapper.cs` — modify interface
- **FILE-005**: `service/Api/src/Shared/Operational/Persistence/Data/AppDbContext.cs` — add transaction implementation
- **FILE-006**: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs` — use new API
- **FILE-007**: `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Tests.cs` — remove warning suppression

## 6. Testing

- **TEST-001**: `IDatabaseTransaction` — `CommitAsync`/`RollbackAsync` work via real implementations
- **TEST-002**: `NoOpTransaction` — all methods complete without error
- **TEST-003**: `ApplicationDbContext.SupportsTransactions` — `false` for in-memory, `true` for relational
- **TEST-004**: `ReserveCartStock` — all 5 tests pass without `ConfigureWarnings` hack
- **TEST-005**: Full `dotnet build` — 0 warnings, 0 errors
- **TEST-006**: Full `dotnet test Module.UnitTests` — only pre-existing architecture test fails

## 7. Risks & Assumptions

- **RISK-001**: Other code may depend on `DatabaseFacade` from the interface — verified only `ReserveCartStock.cs` uses it
- **ASSUMPTION-001**: `Database.ProviderName` reliably distinguishes in-memory from relational providers
- **ASSUMPTION-002**: The `DatabaseInitializer.cs` uses concrete `ApplicationDbContext`, not the interface

## 8. Related Specifications / Further Reading

- `plan/refactor-fromsqlraw-to-linq-1.md` — Previous refactor of FromSqlRaw in same handler
- `docs/superpowers/plans/2026-07-11-mvp-hardening-02-stock.md` — Plan 2 Task 5 introduced the transaction
- `https://learn.microsoft.com/en-us/ef/core/miscellaneous/in-memory-database` — In-memory provider limitations
