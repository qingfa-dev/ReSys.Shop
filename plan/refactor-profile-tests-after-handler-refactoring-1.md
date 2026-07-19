---
goal: Update Profile Unit Tests After Handler Refactoring — Remove ICurrentUser, Fix Command Signatures
version: 1.0
date_created: 2026-07-19
status: Planned
tags: refactor, tests, profile, handlers, icurrentuser
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Update all 59 Profile-related test files after the handler refactoring that removed `ICurrentUser` from Store handlers and moved `UserId` into Command/Query records. Every Store handler test must be updated to pass `UserId` in the command (not via `ICurrentUser` mock), and all Admin handler tests for deleted handlers must be removed or redirected to Store handlers.

## 1. Requirements & Constraints

- **REQ-001**: Every Store handler test must construct its Command/Query with `Guid UserId` as the first parameter (instead of injecting `ICurrentUser` mock into the handler constructor)
- **REQ-002**: Every Store handler test must remove `ICurrentUser` mock from handler constructor call
- **REQ-003**: Admin feature tests for handlers that were deleted (Create/Update/Delete for Profiles and Addresses) must be removed — Admin now delegates to Store handlers via endpoints, not via separate handlers
- **REQ-004**: Admin feature tests for Admin-only handlers still existing (`GetUserProfilesPagedOrAll`) must be kept if the handlers still exist
- **REQ-005**: `GetNotificationPreferences.Query` now takes `Guid UserId` — update test to pass it
- **REQ-006**: `GetAddresses.Parameters` now has `UserId` instead of separate `Query.UserId` — update test to use `with` expression
- **REQ-007**: `DeleteWishlist.Command` now takes `(Guid UserId, Guid Id, string? DeletedBy = null)` — update test constructor call
- **CON-001**: Domain tests (10 files) must NOT be changed — they test domain methods, not handlers
- **CON-002**: Mapping tests must NOT be changed — they test mapping extension methods, not handlers
- **CON-003**: Shared validator tests must NOT be changed — they test validator classes, not handlers
- **CON-004**: Integration tests use DI container and `TestCurrentUser` — should work without changes

## 2. Implementation Steps

### Implementation Phase 1: Store Address Handler Tests (5 files)

- GOAL-001: Update 5 Store Address test files to pass `UserId` in Command/Query and remove `ICurrentUser` mock

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `CreateAddress.Tests.cs` — change `new CreateAddress.Command(request)` to `new CreateAddress.Command(_userId, request)`; remove `ICurrentUser` mock from handler constructor | | |
| TASK-002 | `UpdateAddress.Tests.cs` — change `new UpdateAddress.Command(address.Id, request)` to `new UpdateAddress.Command(_userId, address.Id, request)`; remove `ICurrentUser` mock | | |
| TASK-003 | `DeleteAddress.Tests.cs` — change `new DeleteAddress.Command(address.Id)` to `new DeleteAddress.Command(_userId, address.Id)`; remove `ICurrentUser` mock | | |
| TASK-004 | `GetAddressById.Tests.cs` — change `new GetAddressById.Query(address.Id)` to `new GetAddressById.Query(_userId, address.Id)`; remove `ICurrentUser` mock | | |
| TASK-005 | `GetAddresses.Tests.cs` — change `new GetAddresses.Query(_userId, parameters)` to `new GetAddresses.Query(parameters with { UserId = _userId })`; remove `ICurrentUser` mock; remove `Handle_ShouldReturnUnauthorized_WhenUserNotAuthenticated` test | | |

Validation: `dotnet test --filter "FullyQualifiedName~Address"` — all pass

### Implementation Phase 2: Store Profile Handler Tests (6 files)

- GOAL-002: Update Store Profile test files — remove `ICurrentUser` mock from handler constructors

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | `UpdateProfile.Tests.cs` — remove `ICurrentUser` mock from `UpdateProfile.CommandHandler` constructor; `new UpdateProfile.Command(_userId, request)` already correct | | |
| TASK-007 | `GetProfile.Tests.cs` — remove `ICurrentUser` mock from `GetProfile.QueryHandler` constructor; remove `Handle_ShouldReturnUnauthorized_WhenNotAuthenticated` and `Handle_ShouldReturnNotFound_WhenUserNotFound` tests | | |
| TASK-008 | `CreateProfile.Tests.cs` — verify `new CreateProfile.Command(_userId, request)` already correct; remove `ICurrentUser` mock if present | | |
| TASK-009 | `DeleteProfile.Tests.cs` — verify no changes needed (handler never used `ICurrentUser`) | | |
| TASK-010 | `UpdateUserProfile.Tests.cs` — remove `ICurrentUser` mock from handler constructor | | |

Validation: `dotnet test --filter "FullyQualifiedName~Profile"` — all pass

### Implementation Phase 3: Store Wishlist Handler Tests (7 files)

- GOAL-003: Update Wishlist test files to pass `UserId` in Command/Query and remove `ICurrentUser` mock

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | `CreateWishlist.Tests.cs` — change `new CreateWishlist.Command(request)` to `new CreateWishlist.Command(_userId, request)`; remove `ICurrentUser` mock | | |
| TASK-012 | `UpdateWishlist.Tests.cs` — change `new UpdateWishlist.Command(id, request)` to `new UpdateWishlist.Command(_userId, id, request)`; remove `ICurrentUser` mock | | |
| TASK-013 | `DeleteWishlist.Tests.cs` — change `new DeleteWishlist.Command(wishlist.Id)` to `new DeleteWishlist.Command(_userId, wishlist.Id)`; remove `ICurrentUser` mock (including `UserName` setup) | | |
| TASK-014 | `GetWishlists.Tests.cs` — change `new GetWishlists.Query(parameters)` to `new GetWishlists.Query(_userId, parameters)`; remove `ICurrentUser` mock | | |
| TASK-015 | `GetWishlistById.Tests.cs` — change `new GetWishlistById.Query(wishlist.Id)` to `new GetWishlistById.Query(_userId, wishlist.Id)`; remove `ICurrentUser` mock | | |
| TASK-016 | `AddWishlistItem.Tests.cs` — change `new AddWishlistItem.Command(id, request)` to `new AddWishlistItem.Command(_userId, id, request)`; remove `ICurrentUser` mock | | |
| TASK-017 | `RemoveWishlistItem.Tests.cs` — change `new RemoveWishlistItem.Command(wishlist.Id, itemId)` to `new RemoveWishlistItem.Command(_userId, wishlist.Id, itemId)`; remove `ICurrentUser` mock | | |

Validation: `dotnet test --filter "FullyQualifiedName~Wishlist"` — all pass

### Implementation Phase 4: Store Notification Preferences Tests (2 files)

- GOAL-004: Update Notification Preferences test files to pass `UserId` and remove `ICurrentUser` mock

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | `GetNotificationPreferences.Tests.cs` — change `new GetNotificationPreferences.Query()` to `new GetNotificationPreferences.Query(_userId)`; remove `ICurrentUser` mock | | |
| TASK-019 | `UpdateNotificationPreferences.Tests.cs` — change `new UpdateNotificationPreferences.Command(request)` to `new UpdateNotificationPreferences.Command(_userId, request)`; remove `ICurrentUser` mock | | |

Validation: `dotnet test --filter "FullyQualifiedName~Notification"` — all pass

### Implementation Phase 5: Remove Deleted Admin Handler Test Files (9 files)

- GOAL-005: Remove test files for Admin handlers that were deleted (handlers now delegate to Store)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Delete `Admin/Profiles/CreateUserProfile/CreateUserProfile.Tests.cs` | | |
| TASK-021 | Delete `Admin/Profiles/UpdateUserProfile/UpdateUserProfile.Tests.cs` | | |
| TASK-022 | Delete `Admin/Profiles/DeleteUserProfile/DeleteUserProfile.Tests.cs` | | |
| TASK-023 | Delete `Admin/Profiles/GetUserProfile/GetUserProfile.Tests.cs` | | |
| TASK-024 | Delete `Admin/Addresses/Create/CreateUserAddress.Tests.cs` | | |
| TASK-025 | Delete `Admin/Addresses/Update/UpdateUserAddress.Tests.cs` | | |
| TASK-026 | Delete `Admin/Addresses/Delete/DeleteUserAddress.Tests.cs` | | |
| TASK-027 | Delete `Admin/Addresses/Get/ById/GetUserAddressById.Tests.cs` | | |
| TASK-028 | Delete `Admin/Addresses/Get/All/GetAllAddresses.Tests.cs` | | |

Validation: `dotnet build tests/Module.UnitTests/` — 0 errors (no residual references to deleted types)

### Implementation Phase 6: Verify Full Suite

- GOAL-006: Full build and test suite verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | `dotnet build tests/Module.UnitTests/Module.UnitTests.csproj` — 0 errors, 0 warnings | | |
| TASK-030 | `dotnet test tests/Module.UnitTests/` — all tests pass | | |

Validation: Build and tests pass

## 3. Alternatives

- **ALT-001** (Keep Admin handler tests, redirect to Store handlers): Would require rewriting each test to construct Store commands. More work than deleting since Store handlers are already tested.
- **ALT-002** (Keep `ICurrentUser` mock in handler constructors with unused parameter): Would keep dead code patterns in tests.

## 4. Dependencies

- **DEP-001**: `IdentityMocks.cs` shared mock factory must be preserved for Identity/Catalog module tests
- **DEP-002**: All changes within `tests/Module.UnitTests/Profile/` only

## 5. Files

- **FILE-001** to **FILE-005**: 5 Store Address test files (`Features/Store/Addresses/*/`)
- **FILE-006** to **FILE-010**: 5 Store Profile test files (`Features/Store/Profile/*/`)
- **FILE-011** to **FILE-017**: 7 Store Wishlist test files (`Features/Store/Wishlists/*/`)
- **FILE-018** to **FILE-019**: 2 Store Notification Preferences test files
- **FILE-020** to **FILE-028**: 9 Admin test files to delete

## 6. Testing

- **TEST-001**: `dotnet test --filter "FullyQualifiedName~Address"` — all Address tests pass
- **TEST-002**: `dotnet test --filter "FullyQualifiedName~Profile"` — all Profile tests pass
- **TEST-003**: `dotnet test --filter "FullyQualifiedName~Wishlist"` — all Wishlist tests pass
- **TEST-004**: `dotnet test tests/Module.UnitTests/` — full module suite passes

## 7. Risks & Assumptions

- **RISK-001**: `GetAddresses.Tests.cs` seeds data via `UserProfile.Addresses` collection, but the new handler queries `dbContext.Set<Address>()` directly. Tests may need different seeding (add `Address` entities directly to context instead of through `profile.Addresses`).
- **RISK-002**: `GetProfile.Tests.cs` has auth-related tests (`Handle_ShouldReturnUnauthorized_WhenNotAuthenticated`, `Handle_ShouldReturnNotFound_WhenUserNotFound`) that must be removed since the handler no longer validates auth.
- **RISK-003**: After removing Admin test files, some test types/utilities referenced only by Admin tests may cause build warnings — verify clean removal.
- **ASSUMPTION-001**: Cross-module Identity tests reference `CreateProfile.Command` via `IMediator.Send()` and will compile without changes.

## 8. Related Specifications / Further Reading

- `plan/refactor-profile-module-1.md` — Admin delegation + shared consolidation
- `plan/refactor-remove-icurrentuser-from-handlers-1.md` — ICurrentUser removal (executed)
