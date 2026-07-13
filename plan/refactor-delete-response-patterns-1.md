---
goal: Replace uninformative delete/restore Response(Guid Id) patterns with either success-message-only (Pattern A) or full detail response (Pattern B) across Catalog, Identity, and Inventory
version: 1.0
date_created: 2026-07-14
owner: Platform Team
status: Completed
tags: refactor, catalog, identity, inventory, delete, responses
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Audit found two categories of delete/restore operations with uninformative response patterns:

1. **Dead Response.cs files** (5 files): Handlers return `Result` (non-generic) with a success message like `Result.Ok(XxxResult.Success.Deleted)`, but a `Response.cs` file with `Response(Guid Id)` was created as dead code. These Response files are unreferenced by handlers.

2. **Uninformative Response(Guid Id)** (4 files): Handlers return `Result<Response>` where `Response` is `sealed record Response(Guid Id)` — returning just a GUID with no success message and no detail. The user explicitly flagged this as "not helpful at all."

Fix both categories: delete dead Response.cs files, and convert uninformative ID-only responses to return either a success message or the restored/deleted entity detail.

## 1. Requirements & Constraints

- **REQ-001**: Delete operations with non-generic `ICommand` (Pattern A) must NOT have a Response.cs file — dead code removal
- **REQ-002**: Delete/restore operations returning `Result<Response>` must either include a success message AND the full item detail, or switch to Pattern A (success-message-only)
- **REQ-003**: `ReleaseCartReservation` must return more than just `Response(Guid Id)` — at minimum include a success message or release details
- **CON-001**: No behavioral change to HTTP status codes — existing `Result.Ok()` (200) and `Result.NoContent()` (204) stay unchanged
- **CON-002**: `dotnet build` must pass with 0 warnings after each phase
- **PAT-001**: Pattern A (fire-and-forget): `ICommand` → `ICommandHandler<Command>` → `Task<Result>` → `Result.Ok(XxxResult.Success.Deleted)`. No Response.cs file.
- **PAT-002**: Pattern B (detail return): `ICommand<Response>` → `ICommandHandler<Command, Response>` → `Task<Result<Response>>` → `Result<Response>.Ok(entity.MapToDetail<Response>(), XxxResult.Success.Deleted)`. Response inherits from shared model.

## 2. Implementation Steps

### Implementation Phase 1 — Delete Dead Response.cs Files (Pattern A)

- GOAL-001: Remove 5 Response.cs files that are dead code — handlers return Result (non-generic), never use the Response type

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `rm Catalog/Admin/OptionTypes/Delete/DeleteOptionType.Response.cs` — handler returns `Task<Result>`, Response is unused | | |
| TASK-002 | `rm Catalog/Admin/Products/Delete/DeleteProduct.Response.cs` — handler returns `Task<Result>`, Response is unused | | |
| TASK-003 | `rm Catalog/Admin/Products/Variants/Delete/DeleteVariant.Response.cs` — handler returns `Task<Result>`, Response is unused | | |
| TASK-004 | `rm Catalog/Admin/Taxonomies/Delete/DeleteTaxonomyUseCase.Response.cs` — handler returns `Task<Result>`, Response is unused | | |
| TASK-005 | `rm Catalog/Admin/Taxonomies/Restore/RestoreTaxonomyUseCase.Response.cs` — handler returns `Task<Result>`, Response is unused | | |
| TASK-006 | `dotnet build` — verify 0 warnings, 0 errors | | |

### Implementation Phase 2 — Fix ReleaseCartReservation (Inventory Pattern B → Pattern A)

- GOAL-002: Replace uninformative `Response(Guid Id)` with success-message-only Pattern A, since release is a side-effect operation with no meaningful detail to return

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | `ReleaseCartReservation.Response.cs`: DELETE this file — no Response type needed | | |
| TASK-011 | `ReleaseCartReservation.cs`: change `Command` from `ICommand<Response>` to `ICommand` (non-generic). Change handler from `ICommandHandler<Command, Response>` to `ICommandHandler<Command>` returning `Task<Result>`. Change `return new Response(reservation.Id)` to `return Result.Ok(CartReservationResult.Success.Released(reservation.Id))` — add success message factory if missing | | |
| TASK-012 | `dotnet build` — verify | | |

### Implementation Phase 3 — Fix DeleteTaxon, DeleteOptionValue, DeleteUser (Catalog/Identity Pattern B → detail return)

- GOAL-003: Convert ID-only responses to return the full deleted/restored entity mapped to detail response

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `DeleteTaxon.cs` (Catalog): change `return new Response(entity.Id)` to `return entity.MapToDetail<Response>()` using existing `MapToDetail<T>()` shared mapping method — add success message via `Result<Response>.Ok(value, message)` | | |
| TASK-021 | `DeleteOptionValue.cs` (Catalog): change `return new Response(entity.Id)` to use shared mapping `entity.MapToDetail<Response>()` with success message | | |
| TASK-022 | `DeleteUser.cs` (Identity): change `return new Response(user.Id)` to return user detail via `user.MapToDetail<Response>()` with success message | | |
| TASK-023 | `dotnet build` — verify | | |

### Implementation Phase 4 — Build + Full Verification

- GOAL-004: Verify all changes compile, no dead Response files remain, all tests pass

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | `dotnet build` — 0 warnings, 0 errors | | |
| TASK-031 | Verify no `sealed record Response(Guid Id)` remains in Features/ directories: `grep -rn 'record Response(Guid Id)' service/Api/src/Module/ --include='*.cs'` — zero results except in domains (non-Features) | | |
| TASK-032 | `dotnet test service/Api/tests/Module.UnitTests --no-build` — all pass | | |
| TASK-033 | `dotnet test service/Api/tests/Shared.UnitTests --no-build` — all pass | | |

## 3. Alternatives

- **ALT-001**: Keep `Response(Guid Id)` for all delete operations. Rejected: returning just a GUID with no context is unhelpful to API consumers. The ID is already returned in the URL/route and the message `Result.Ok(message)` conveys the success/failure.
- **ALT-002**: Return the full deleted entity detail for all delete operations. Rejected: for simple delete operations (like `DeleteOptionType`), the handler is fire-and-forget with no entity loaded after deletion. Loading the entity before deletion just for the response adds unnecessary DB overhead. Pattern A (message-only) is the right fit here.
- **ALT-003**: Use `Result.NoContent()` (204) for all deletes. Rejected: existing codebase convention mixes `Result.Ok()` (200 with message) and `Result.NoContent()` (204). Let's not change existing status codes.

## 4. Dependencies

- **DEP-001**: Phase 1 must complete before Phase 3 (avoids conflicts)
- **DEP-002**: Phase 2 is independent and can run in parallel with Phase 1
- **DEP-003**: Shared models and mapping methods must exist for Pattern B detail returns (confirmed: `Taxon.MapToDetail<T>()`, `OptionValue.MapToDetail<T>()`, `User.MapToDetail<T>()` exist)

## 5. Files

### Deleted (6 files)
- **FILE-001**: `DeleteOptionType.Response.cs` — dead code
- **FILE-002**: `DeleteProduct.Response.cs` — dead code
- **FILE-003**: `DeleteVariant.Response.cs` — dead code
- **FILE-004**: `DeleteTaxonomyUseCase.Response.cs` — dead code
- **FILE-005**: `RestoreTaxonomyUseCase.Response.cs` — dead code
- **FILE-006**: `ReleaseCartReservation.Response.cs` — convert to Pattern A, delete Response

### Modified (4 files)
- **FILE-007**: `ReleaseCartReservation.cs` — change `ICommand<Response>` to `ICommand`, use `Result.Ok(message)`
- **FILE-008**: `DeleteTaxon.cs` — use `MapToDetail<Response>()` + success message
- **FILE-009**: `DeleteOptionValue.cs` — use `MapToDetail<Response>()` + success message
- **FILE-010**: `DeleteUser.cs` — use `MapToDetail<Response>()` + success message

## 6. Testing

- **TEST-001**: `dotnet build` after each phase — warnings-as-errors catches dead code references
- **TEST-002**: `grep -rn 'record Response(Guid Id)'` — zero hits in Features/ directories
- **TEST-003**: `dotnet test` for Module.UnitTests and Shared.UnitTests — all pass

## 7. Risks & Assumptions

- **RISK-001**: Changing `DeleteTaxon.cs` from `return new Response(entity.Id)` to `return entity.MapToDetail<Response>()` adds a DB query (.Include/.ThenInclude may be needed). Mitigation: verify the handler already includes navigation properties before deletion, or use `AsNoTracking()` for a separate read query.
- **RISK-002**: Changing `ReleaseCartReservation` from `ICommand<Response>` to `ICommand` changes the API contract — callers expecting `{ "id": "..." }` in the response body will get a different shape. Mitigation: this is a storefront-internal API endpoint; if external callers exist, they must be updated.
- **ASSUMPTION-001**: `CartReservationResult.Success.Released(Guid id)` or equivalent success message factory exists; if not, add it inline or create a simple message.
- **ASSUMPTION-002**: The `DeleteTaxon.cs` handler already loads the entity fully (with includes) before deletion, so `MapToDetail<T>()` can access all needed properties.

## 8. Related Specifications / Further Reading

- `plan/refactor-all-modules-response-mapping-1.md` — prior plan that introduced the `Response(Guid Id)` pattern being reverted
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs` — reference handler returning full detail + success message
