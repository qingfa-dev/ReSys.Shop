---
goal: Remove ICurrentUser from Store Handlers — Extract UserId at Endpoint Level
version: 1.0
date_created: 2026-07-19
status: Planned
tags: refactor, handlers, icurrentuser, profile, address, wishlist, notification-preferences
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Remove `ICurrentUser` injection from all 16 Store handler classes. Every handler resolves `UserId` from the Command/Query record instead. Endpoints inject `ICurrentUser` and pass `UserId` (and optionally `DeletedBy`) into the command. This makes handlers testable without auth mocking, removes redundant `Guid.Parse`/`string.IsNullOrEmpty` guards from handler logic, and cleans up the Admin delegation pattern (Admin passes `UserId` directly, no fallback needed).

## 1. Requirements & Constraints

- **REQ-001**: Every Store handler Command/Query record MUST include a `Guid UserId` field
- **REQ-002**: Every Store handler constructor MUST NOT inject `ICurrentUser` or reference `currentUser`
- **REQ-003**: Every Store endpoint MUST inject `ICurrentUser`, extract `UserId`, and pass it to the Command/Query
- **REQ-004**: Admin endpoints that delegate to Store handlers MUST pass `UserId` explicitly (already done for Addresses and Profiles)
- **REQ-005**: `DeleteWishlist` handler needs `DeletedBy` string — add optional field to Command, extracted from `ICurrentUser.UserName` at endpoint level
- **CON-001**: Handlers that currently have optional `Guid? UserId` (Address 5 files, UpdateProfile) — change to required `Guid UserId`, remove `??` fallback and `ICurrentUser`
- **CON-002**: `GetUserProfile` handler currently checks `currentUser.IsAuthenticated` and `request.UserId != currentUserId` — move only the ownership check to the endpoint, handler trusts the passed `UserId`
- **CON-003**: `UpdateProfile` handler has `IsAdminBypass` flag — keep flag, remove `ICurrentUser` check entirely (ownership check moves to endpoint or flag bypasses it)
- **PAT-001**: Follow same pattern as existing `CreateProfile` handler (takes `Guid UserId` on Command, no `ICurrentUser`)
- **BUG-001**: `GetAddresses.cs` is missing `using Shared.Security.Identity.Domain.Users;` but still compiles via global using — after removal, verify no residual reference

## 2. Implementation Steps

### Implementation Phase 1: Address Handlers — Make UserId Required, Remove ICurrentUser

- GOAL-001: Convert 5 Store Address handlers from optional `Guid? UserId = null` + `ICurrentUser` fallback to required `Guid UserId` with no `ICurrentUser`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Store/Addresses/Create/CreateAddress.cs` — change `Command(Request Request, Guid? UserId = null)` to `Command(Guid UserId, Request Request)`; remove `ICurrentUser currentUser` from CommandHandler constructor; remove `string.IsNullOrEmpty` guard; replace `var userId = command.UserId ?? Guid.Parse(currentUser.UserId)` with direct `command.UserId` usage | | |
| TASK-002 | `Store/Addresses/Update/UpdateAddress.cs` — change `Command(Guid Id, Request Request, Guid? UserId = null)` to `Command(Guid UserId, Guid Id, Request Request)`; remove `ICurrentUser` from constructor; remove guard; use `command.UserId` directly | | |
| TASK-003 | `Store/Addresses/Delete/DeleteAddress.cs` — change `Command(Guid Id, Guid? UserId = null)` to `Command(Guid UserId, Guid Id)`; remove `ICurrentUser` from constructor; remove guard; use `command.UserId` directly | | |
| TASK-004 | `Store/Addresses/Get/ById/GetAddressById.cs` — change `Query(Guid Id, Guid? UserId = null)` to `Query(Guid UserId, Guid Id)`; remove `ICurrentUser` from QueryHandler constructor; remove guard; use `request.UserId` directly | | |
| TASK-005 | `Store/Addresses/Get/PagedOrAll/GetAddresses.cs` — change `Query(Parameters Parameters, Guid? UserId = null)` to `Query(Guid UserId, Parameters Parameters)`; remove `ICurrentUser` from PagedQueryHandler constructor; remove guard; use `request.UserId` directly | | |
| TASK-006 | `Store/Addresses/Create/CreateAddress.Endpoint.cs` — add `ICurrentUser currentUser` param, extract `UserId`, pass as `new CreateAddress.Command(Guid.Parse(currentUser.UserId), request)` | | |
| TASK-007 | `Store/Addresses/Update/UpdateAddress.Endpoint.cs` — add `ICurrentUser currentUser` param, pass `new UpdateAddress.Command(Guid.Parse(currentUser.UserId), id, request)` | | |
| TASK-008 | `Store/Addresses/Delete/DeleteAddress.Endpoint.cs` — add `ICurrentUser currentUser` param, pass `new DeleteAddress.Command(Guid.Parse(currentUser.UserId), id)` | | |
| TASK-009 | `Store/Addresses/Get/ById/GetAddressById.Endpoint.cs` — add `ICurrentUser currentUser` param, pass `new GetAddressById.Query(Guid.Parse(currentUser.UserId), id)` | | |
| TASK-010 | `Store/Addresses/Get/PagedOrAll/GetAddresses.Endpoint.cs` — add `ICurrentUser currentUser` param, pass `new GetAddresses.Query(Guid.Parse(currentUser.UserId), parameters)` | | |
| TASK-011 | `Admin/Addresses/Create/CreateUserAddress.Endpoint.cs` — update to match new Command signature: `new CreateAddress.Command(request.UserId, request)` (UserId first param now) | | |
| TASK-012 | `Admin/Addresses/Update/UpdateUserAddress.Endpoint.cs` — update to `new UpdateAddress.Command(request.UserId, id, request)` | | |
| TASK-013 | `Admin/Addresses/Delete/DeleteUserAddress.Endpoint.cs` — update to `new DeleteAddress.Command(userId, id)` | | |
| TASK-014 | `Admin/Addresses/Get/ById/GetUserAddressById.Endpoint.cs` — update to `new GetAddressById.Query(userId, id)` | | |
| TASK-015 | `Admin/Addresses/Get/All/GetAllAddresses.Endpoint.cs` — keep as-is (admin-specific handler, no Store delegation) | | |

Validation: `dotnet build service/Api/src/Module/Module.csproj` — 0 errors, 0 warnings

### Implementation Phase 2: Profile Handlers — Remove ICurrentUser

- GOAL-002: Remove `ICurrentUser` from `UpdateProfile` and `GetUserProfile` handlers, move auth/ownership checks to endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | `Store/Profiles/Update/UpdateProfile.cs` — remove `ICurrentUser currentUser` from CommandHandler constructor; remove line `if (!command.IsAdminBypass && (!Guid.TryParse(currentUser.UserId, out var currentUserId) \|\| userId != currentUserId))` — this ownership check moves to endpoint; keep `IsAdminBypass` flag on Command but handler uses `command.UserId` directly | | |
| TASK-017 | `Store/Profiles/Update/UpdateProfile.Endpoint.cs` — add `ICurrentUser currentUser` param; before sending Command, if NOT admin (default), verify `Guid.Parse(currentUser.UserId) == userId` and return 401 on mismatch; pass `new UpdateProfile.Command(userId, request)` | | |
| TASK-018 | `Store/Profiles/Get/Detail/GetUserProfile.cs` — remove `ICurrentUser currentUser` from QueryHandler constructor; remove `IsAuthenticated` check; remove `request.UserId != currentUserId` check; handler trusts the passed `UserId` in the Query | | |
| TASK-019 | `Store/Profiles/Get/Detail/GetProfile.Endpoint.cs` — add `ICurrentUser currentUser` param; extract `userId` from `currentUser.UserId`; verify auth before sending (same pattern as Store DeleteProfile endpoint); pass `new GetProfile.Query(userId)` | | |
| TASK-020 | `Admin/Profiles/Update/UpdateUserProfile.Endpoint.cs` — update to `new UpdateProfile.Command(request.UserId, request, IsAdminBypass: true)` (already done, verify signature matches) | | |
| TASK-021 | `Admin/Profiles/Delete/DeleteUserProfile.Endpoint.cs` — already delegates to `DeleteProfile.Command(userId)` (no ICurrentUser), verify no change needed | | |

Validation: `dotnet build` succeeds. Store Update and GetProfile endpoints still enforce ownership before sending command.

### Implementation Phase 3: Wishlist Handlers — Add UserId, Remove ICurrentUser

- GOAL-003: Add `Guid UserId` to all 7 Wishlist commands/queries, remove `ICurrentUser` from handlers, update endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | `Store/Wishlists/Create/CreateWishlist.cs` — change `Command(Request Request)` to `Command(Guid UserId, Request Request)`; remove `ICurrentUser` from constructor; remove guard; replace `Guid.Parse(currentUser.UserId)` with `command.UserId` | | |
| TASK-023 | `Store/Wishlists/Get/GetWishlists.cs` — change `Query(Parameters Parameters)` to `Query(Guid UserId, Parameters Parameters)`; remove `ICurrentUser` from constructor; remove guard; replace `Guid.Parse(currentUser.UserId)` with `request.UserId` | | |
| TASK-024 | `Store/Wishlists/GetById/GetWishlistById.cs` — change `Query(Guid Id)` to `Query(Guid UserId, Guid Id)`; remove `ICurrentUser`; replace `Guid.Parse(currentUser.UserId)` with `request.UserId` | | |
| TASK-025 | `Store/Wishlists/Update/UpdateWishlist.cs` — change `Command(Guid Id, Request Request)` to `Command(Guid UserId, Guid Id, Request Request)`; remove `ICurrentUser`; replace `Guid.Parse(currentUser.UserId)` with `command.UserId` | | |
| TASK-026 | `Store/Wishlists/Delete/DeleteWishlist.cs` — change `Command(Guid Id)` to `Command(Guid UserId, Guid Id, string? DeletedBy = null)`; remove `ICurrentUser`; replace `Guid.Parse(currentUser.UserId)` with `command.UserId`; replace `currentUser.UserName` with `command.DeletedBy ?? command.UserId.ToString()` | | |
| TASK-027 | `Store/Wishlists/AddItem/AddWishlistItem.cs` — change `Command(Guid Id, Request Request)` to `Command(Guid UserId, Guid Id, Request Request)`; remove `ICurrentUser`; replace `Guid.Parse(currentUser.UserId)` with `command.UserId` | | |
| TASK-028 | `Store/Wishlists/RemoveItem/RemoveWishlistItem.cs` — change `Command(Guid Id, Guid ItemId)` to `Command(Guid UserId, Guid Id, Guid ItemId)`; remove `ICurrentUser`; replace `Guid.Parse(currentUser.UserId)` with `command.UserId` | | |
| TASK-029 | `Store/Wishlists/Create/CreateWishlist.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new CreateWishlist.Command(Guid.Parse(currentUser.UserId), request)` | | |
| TASK-030 | `Store/Wishlists/Get/GetWishlists.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new GetWishlists.Query(Guid.Parse(currentUser.UserId), parameters)` | | |
| TASK-031 | `Store/Wishlists/GetById/GetWishlistById.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new GetWishlistById.Query(Guid.Parse(currentUser.UserId), id)` | | |
| TASK-032 | `Store/Wishlists/Update/UpdateWishlist.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new UpdateWishlist.Command(Guid.Parse(currentUser.UserId), id, request)` | | |
| TASK-033 | `Store/Wishlists/Delete/DeleteWishlist.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new DeleteWishlist.Command(Guid.Parse(currentUser.UserId), id, DeletedBy: currentUser.UserName)` | | |
| TASK-034 | `Store/Wishlists/AddItem/AddWishlistItem.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new AddWishlistItem.Command(Guid.Parse(currentUser.UserId), id, request)` | | |
| TASK-035 | `Store/Wishlists/RemoveItem/RemoveWishlistItem.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new RemoveWishlistItem.Command(Guid.Parse(currentUser.UserId), id, itemId)` | | |

Validation: `dotnet build` succeeds. Wishlist endpoints still enforce auth via ICurrentUser before sending commands.

### Implementation Phase 4: Notification Preferences Handlers — Add UserId, Remove ICurrentUser

- GOAL-004: Add `Guid UserId` to 2 Notification Preferences commands/queries, remove `ICurrentUser` from handlers, update endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | `Store/NotificationPreferences/Get/GetNotificationPreferences.cs` — change `Query` (no params) to `Query(Guid UserId)`; remove `ICurrentUser` from constructor; remove guard; replace `Guid.Parse(currentUser.UserId)` with `request.UserId` | | |
| TASK-037 | `Store/NotificationPreferences/Update/UpdateNotificationPreferences.cs` — change `Command(Request Request)` to `Command(Guid UserId, Request Request)`; remove `ICurrentUser` from constructor; remove guard; replace `Guid.Parse(currentUser.UserId)` with `command.UserId` | | |
| TASK-038 | `Store/NotificationPreferences/Get/GetNotificationPreferences.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new GetNotificationPreferences.Query(Guid.Parse(currentUser.UserId))` | | |
| TASK-039 | `Store/NotificationPreferences/Update/UpdateNotificationPreferences.Endpoint.cs` — add `ICurrentUser currentUser`, pass `new UpdateNotificationPreferences.Command(Guid.Parse(currentUser.UserId), request)` | | |

Validation: `dotnet build` succeeds.

### Implementation Phase 5: Final Verification

- GOAL-005: Full build, full test run, grep for residual `ICurrentUser` in handler files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Run `grep -rn "ICurrentUser" service/Api/src/Module/Profile/Features/Store/` — verify only Endpoint files (`.Endpoint.cs`) contain `ICurrentUser`; zero handler files contain it | | |
| TASK-041 | Run `grep -rn "currentUser" service/Api/src/Module/Profile/Features/Store/` — verify only Endpoint files reference `currentUser` | | |
| TASK-042 | Run `dotnet build service/Api/src/Module/Module.csproj` — 0 warnings, 0 errors | | |
| TASK-043 | Run `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — all tests pass | | |

Validation: All 4 tasks succeed.

## 3. Alternatives

- **ALT-001** (Keep ICurrentUser in handlers, add optional UserId for Admin): Current state for Address handlers. Rejected because handlers still depend on `ICurrentUser` for the default path, making unit tests harder and code less clean.
- **ALT-002** (Inject ICurrentUser via method parameter instead of constructor): Would work but deviates from convention. Commands/queries are the established carrier of handler input.
- **ALT-003** (ICurrentUser everywhere, skip Admin delegation): Would keep the duplicate Admin handlers we just removed. Not viable.

## 4. Dependencies

- **DEP-001**: All changes within `service/Api/src/Module/Profile/` — single assembly
- **DEP-002**: All Store endpoints that are updated must have `using Shared.Security.Identity.Domain.Users;` for `ICurrentUser`

## 5. Files

- **FILE-001** to **FILE-016**: 16 Store handler files (Address 5, Profile 2, Wishlist 7, Notification 2)
- **FILE-017** to **FILE-031**: 15 Store endpoint files (Address 5, Profile 2, Wishlist 7, Notification 2)
- **FILE-032** to **FILE-035**: 4 Admin endpoint files (Address Create/Update/Delete/GetById)
- Total: ~35 files modified

## 6. Testing

- **TEST-001**: `dotnet build` with 0 warnings and 0 errors
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all Profile tests pass
- **TEST-003**: Manual grep: no `ICurrentUser` references in any handler file under `Features/Store/`
- **TEST-004**: Manual code review: every Store endpoint passes `UserId` to its Command/Query
- **TEST-005**: Manual code review: every handler uses `command.UserId` or `request.UserId` instead of `currentUser.UserId`

## 7. Risks & Assumptions

- **RISK-001**: `Guid.Parse(currentUser.UserId)` at endpoint level could throw if `UserId` is not a valid GUID — all endpoints already guard via `Guid.TryParse` or `string.IsNullOrEmpty` checks in existing code; verify each endpoint has this guard
- **RISK-002**: Ownership check moves from `UpdateProfile` handler to endpoint — ensure Store UpdateProfile.Endpoint correctly verifies `Guid.Parse(currentUser.UserId) == userId` before sending command
- **RISK-003**: `DeleteWishlist.DeletedBy` — endpoint passes `currentUser.UserName` which could be null; handler falls back to `command.UserId.ToString()`
- **ASSUMPTION-001**: All Store endpoints are authenticated (`.RequireAuthorization()` or equivalent) — endpoints that add `ICurrentUser` injection can safely access `currentUser.UserId`

## 8. Related Specifications / Further Reading

- `plan/refactor-profile-module-1.md` — previous Profile module refactoring plan (Addressed shared consolidation + Admin delegation)
- `service/Api/src/Module/Profile/README.yaml` — module documentation
