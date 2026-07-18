---
goal: Full Unit Test Coverage for All Profile Module Handlers
version: 1.0
date_created: 2026-07-18
last_updated: 2026-07-18
owner: Platform
status: 'Completed'
tags: feature, testing, profile, admin, store, unit-tests
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Profile module has 31 handler features across Admin and Store areas. 14 have tests (Admin Profiles + Store Addresses/Profiles/Wishlist Mappings). 16 handlers lack tests. This plan covers closing the gap with handler-level unit tests following established Store patterns.

## 1. Requirements & Constraints

- **REQ-001**: All 16 untested handlers must have handler-level unit tests covering happy path and all error paths
- **REQ-002**: Tests must use `ApplicationDbContext` with `UseInMemoryDatabase` (real EF Core, not mocked)
- **REQ-003**: Tests must use FluentAssertions for result assertions
- **REQ-004**: Tests must use xUnit v3 with `TestContext.Current.CancellationToken`
- **REQ-005**: Each test class must have `[Trait("Category", "Unit")]`, `[Trait("Module", "Profile")]`, and `[Trait("Feature", "<FeatureName>")]`
- **CON-001**: Admin handlers use `Guid UserId` in request directly (no ICurrentUser) — tests seed User in identity store
- **CON-002**: Store handlers use `ICurrentUser` for auth — tests mock via `IdentityMocks.CreateCurrentUserMock`
- **PAT-001**: Follow the exact pattern from existing tests: `IDisposable`, constructor seeds DB, handler instantiated with real DbContext
- **PAT-002**: Use `ProfileUserFactory` for UserProfile seeding, `AddressMethod` for Address seeding, `WishlistExtensions/WishlistMethod` for Wishlist seeding

## 2. Implementation Steps

### Implementation Phase 1: Admin Addresses (5 handlers)

- GOAL-001: Implement handler unit tests for all 5 Admin Address features

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Admin/Addresses/Create/CreateAddress.Tests.cs` — 7 tests: first-address-auto-default, profile-not-found, total-limit, per-type-limit, duplicate, unset-other-defaults, user-not-found | ✅ | 2026-07-18 |
| TASK-002 | Create `Admin/Addresses/Delete/DeleteAddress.Tests.cs` — 4 tests: delete-success, profile-not-found, address-not-found, promote-default-on-delete | ✅ | 2026-07-18 |
| TASK-003 | Create `Admin/Addresses/Get/All/GetAllAddresses.Tests.cs` — 3 tests: return-all-addresses, profile-not-found, empty-addresses | ✅ | 2026-07-18 |
| TASK-004 | Create `Admin/Addresses/Get/ById/GetAddressById.Tests.cs` — 3 tests: return-address, address-not-found | ✅ | 2026-07-18 |
| TASK-005 | Create `Admin/Addresses/Update/UpdateAddress.Tests.cs` — 8 tests: update-details, profile-not-found, address-not-found, duplicate, per-type-limit-on-type-change, old-type-default-promotion, set-as-default-when-only-one-of-type, all-fields-update | ✅ | 2026-07-18 |

### Implementation Phase 2: Store NotificationPreferences (2 handlers)

- GOAL-002: Implement handler unit tests for Store NotificationPreferences features

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `Store/NotificationPreferences/Get/GetNotificationPreferences.Tests.cs` — 3 tests: return-preferences, unauthenticated, profile-not-found | ✅ | 2026-07-18 |
| TASK-007 | Create `Store/NotificationPreferences/Update/UpdateNotificationPreferences.Tests.cs` — 4 tests: update-preferences, unauthenticated, profile-not-found, all-off | ✅ | 2026-07-18 |

### Implementation Phase 3: Store UpdateUserProfile (1 handler)

- GOAL-003: Implement handler unit tests for Store UpdateUserProfile (duplicate of UpdateProfile with ICurrentUser auth)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Create `Store/Profiles/Update/UpdateUserProfile.Tests.cs` — 4 tests: update-existing, create-when-not-exists, unauthorized, auth-mismatch | ✅ | 2026-07-18 |

### Implementation Phase 4: Store Wishlists (8 handlers)

- GOAL-004: Implement handler unit tests for all 8 Store Wishlist features

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `Store/Wishlists/Create/CreateWishlist.Tests.cs` — 3 tests: create-success, unauthenticated, create-private | ✅ | 2026-07-18 |
| TASK-010 | Create `Store/Wishlists/Get/GetWishlists.Tests.cs` — 4 tests: return-wishlists-paginated, empty, not-return-deleted, unauthenticated | ✅ | 2026-07-18 |
| TASK-011 | Create `Store/Wishlists/GetById/GetWishlistById.Tests.cs` — 4 tests: return-wishlist-with-items, unauthenticated, not-found, not-owned-by-user | ✅ | 2026-07-18 |
| TASK-012 | Create `Store/Wishlists/Update/UpdateWishlist.Tests.cs` — 4 tests: update-name-privacy, unauthenticated, not-found, update-only-name | ✅ | 2026-07-18 |
| TASK-013 | Create `Store/Wishlists/Delete/DeleteWishlist.Tests.cs` — 3 tests: soft-delete, unauthenticated, not-found | ✅ | 2026-07-18 |
| TASK-014 | Create `Store/Wishlists/AddItem/AddWishlistItem.Tests.cs` — 4 tests: add-item, unauthenticated, not-found, default-quantity | ✅ | 2026-07-18 |
| TASK-015 | Create `Store/Wishlists/RemoveItem/RemoveWishlistItem.Tests.cs` — 4 tests: remove-item, unauthenticated, not-found, item-not-found | ✅ | 2026-07-18 |

## 3. Alternatives

- **ALT-001**: Use mocked IApplicationDbContext instead of real in-memory DB — rejected because existing tests use real EF Core to catch mapping/configuration issues
- **ALT-002**: Skip UpdateUserProfile since it duplicates UpdateProfile — rejected; both handlers exist in source and need independent coverage

## 4. Dependencies

- **DEP-001**: `ProfileUserFactory` at `tests/Module.UnitTests/Profile/Domain/ProfileUserFactory.cs` — exists
- **DEP-002**: `AddressMethod` and `AddressResult` — defined in source Domain
- **DEP-003**: `WishlistExtensions`, `WishlistMethod`, `WishlistResult` — defined in source Domain
- **DEP-004**: `IdentityMocks` at `tests/Module.UnitTests/Identity/Fixtures/IdentityMocks.cs` — exists

## 5. Files

- **FILE-001**: `tests/Module.UnitTests/Profile/Features/Admin/Addresses/Create/CreateAddress.Tests.cs` — new
- **FILE-002**: `tests/Module.UnitTests/Profile/Features/Admin/Addresses/Delete/DeleteAddress.Tests.cs` — new
- **FILE-003**: `tests/Module.UnitTests/Profile/Features/Admin/Addresses/Get/All/GetAllAddresses.Tests.cs` — new
- **FILE-004**: `tests/Module.UnitTests/Profile/Features/Admin/Addresses/Get/ById/GetAddressById.Tests.cs` — new
- **FILE-005**: `tests/Module.UnitTests/Profile/Features/Admin/Addresses/Update/UpdateAddress.Tests.cs` — new
- **FILE-006**: `tests/Module.UnitTests/Profile/Features/Store/NotificationPreferences/Get/GetNotificationPreferences.Tests.cs` — new
- **FILE-007**: `tests/Module.UnitTests/Profile/Features/Store/NotificationPreferences/Update/UpdateNotificationPreferences.Tests.cs` — new
- **FILE-008**: `tests/Module.UnitTests/Profile/Features/Store/Profiles/Update/UpdateUserProfile.Tests.cs` — new
- **FILE-009**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/Create/CreateWishlist.Tests.cs` — new
- **FILE-010**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/Get/GetWishlists.Tests.cs` — new
- **FILE-011**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/GetById/GetWishlistById.Tests.cs` — new
- **FILE-012**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/Update/UpdateWishlist.Tests.cs` — new
- **FILE-013**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/Delete/DeleteWishlist.Tests.cs` — new
- **FILE-014**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/AddItem/AddWishlistItem.Tests.cs` — new
- **FILE-015**: `tests/Module.UnitTests/Profile/Features/Store/Wishlists/RemoveItem/RemoveWishlistItem.Tests.cs` — new

## 6. Testing

- **TEST-001**: Run `dotnet test service/Api/tests/Module.UnitTests/` — all Profile tests pass, zero regressions
- **TEST-002**: Run `dotnet build service/Api/src/Api/` — build succeeds with 0 warnings

## 7. Risks & Assumptions

- **ASSUMPTION-001**: Admin Address handlers follow the same logic as Store Address handlers but without ICurrentUser — can validate against Store Address test patterns
- **ASSUMPTION-002**: `WishlistExtensions.Create()`, `Wishlist.AddItem()`, `Wishlist.RemoveItem()`, `Wishlist.Update()` are domain methods tested by `WishlistMethods.Tests.cs` — handler tests focus on handler orchestration
- **RISK-001**: Some handlers have complex default-promotion logic (UpdateAddress, DeleteAddress) — each branch must be tested independently

## 8. Related Specifications / Further Reading

- Store test patterns: `tests/Module.UnitTests/Profile/Features/Store/Addresses/Create/CreateAddress.Tests.cs`
- Admin Profile tests: `tests/Module.UnitTests/Profile/Features/Admin/Profiles/CreateUserProfile/CreateUserProfile.Tests.cs`
- Domain result codes: `Domain/Addresses/Address.Result.cs`, `Domain/Wishlists/Wishlist.Result.cs`, `Domain/UserProfile.Result.cs`
