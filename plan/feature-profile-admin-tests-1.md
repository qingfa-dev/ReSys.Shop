---
goal: Implement Unit Tests for Admin Profile Vertical Slices
version: 1.0
date_created: 2026-07-18
last_updated: 2026-07-18
owner: Platform
status: 'Completed'
tags: feature, testing, profile, admin, unit-tests
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Admin Profile feature set at `service/Api/src/Module/Profile/Features/Admin/Profiles/` has 4 complete vertical-slice features (CreateUserProfile, GetUserProfile, UpdateUserProfile, DeleteUserProfile) with zero unit tests. The Store-side equivalents have full coverage (~10 handler test files). This plan covers implementing handler-level unit tests for all 4 Admin features, following the established Store test pattern.

## 1. Requirements & Constraints

- **REQ-001**: All 4 Admin Profile handlers must have handler-level unit tests covering at minimum the happy path and all error paths
- **REQ-002**: Tests must use `ApplicationDbContext` with `UseInMemoryDatabase` (real EF Core, not mocked)
- **REQ-003**: Tests must use FluentAssertions for result assertions
- **REQ-004**: Tests must use xUnit v3 with `TestContext.Current.CancellationToken`
- **REQ-005**: Each test class must have `[Trait("Category", "Unit")]`, `[Trait("Module", "Profile")]`, and `[Trait("Feature", "Admin<Feature>")]`
- **CON-001**: The `Shared/` model/mapping classes must not be duplicated — reuse existing `ProfileRequest`, `ProfileDetailResponse`, `ProfileRequestExtensions`, `ProfileResponseMapping`
- **PAT-001**: Follow the exact pattern from Store tests: `IDisposable`, constructor seeds DB, handler instantiated with real DbContext, helper methods for request building
- **PAT-002**: Use `ProfileUserFactory` for domain entity seeding (already exists at `tests/Module.UnitTests/Profile/Domain/ProfileUserFactory.cs`)

## 2. Implementation Steps

### Implementation Phase 1: CreateUserProfile Tests

- GOAL-001: Implement handler unit tests for `CreateUserProfile.CommandHandler`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `tests/Module.UnitTests/Profile/Features/Admin/Profiles/CreateUserProfile/CreateUserProfile.Tests.cs` with `IDisposable`, in-memory DB, `User` seeding, handler instantiation | ✅ | 2026-07-18 |
| TASK-002 | Test `Handle_ShouldCreateProfile_WhenUserExists` — valid request, user exists, no existing profile → success, profile persisted with correct fields | ✅ | 2026-07-18 |
| TASK-003 | Test `Handle_ShouldFail_WhenUserNotFound` — non-existent `UserId` → returns `UserProfileResult.Failure.UserNotFound` | ✅ | 2026-07-18 |
| TASK-004 | Test `Handle_ShouldFail_WhenProfileAlreadyExists` — user exists with profile already → returns `UserProfileResult.Failure.AlreadyExists` | ✅ | 2026-07-18 |

### Implementation Phase 2: GetUserProfile Tests

- GOAL-002: Implement handler unit tests for `GetUserProfile.QueryHandler`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create `tests/Module.UnitTests/Profile/Features/Admin/Profiles/GetUserProfile/GetUserProfile.Tests.cs` | ✅ | 2026-07-18 |
| TASK-006 | Test `Handle_ShouldReturnProfile_WhenUserAndProfileExist` — seed both `User` and `UserProfile`, query by `UserId` → success with mapped response | ✅ | 2026-07-18 |
| TASK-007 | Test `Handle_ShouldFail_WhenUserNotFound` — non-existent `UserId` → `UserProfileResult.Failure.UserNotFound` | ✅ | 2026-07-18 |
| TASK-008 | Test `Handle_ShouldFail_WhenProfileNotFound` — existing user but no profile → `UserProfileResult.Failure.NotFound` | ✅ | 2026-07-18 |

### Implementation Phase 3: UpdateUserProfile Tests

- GOAL-003: Implement handler unit tests for `UpdateUserProfile.CommandHandler`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `tests/Module.UnitTests/Profile/Features/Admin/Profiles/UpdateUserProfile/UpdateUserProfile.Tests.cs` | ✅ | 2026-07-18 |
| TASK-010 | Test `Handle_ShouldUpdateExistingProfile` — existing profile, valid update request → fields updated in DB, response matches | ✅ | 2026-07-18 |
| TASK-011 | Test `Handle_ShouldCreateProfile_WhenNotExists` — upsert behavior: no existing profile → new profile created and returned | ✅ | 2026-07-18 |

### Implementation Phase 4: DeleteUserProfile Tests

- GOAL-004: Implement handler unit tests for `DeleteUserProfile.CommandHandler`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Create `tests/Module.UnitTests/Profile/Features/Admin/Profiles/DeleteUserProfile/DeleteUserProfile.Tests.cs` | ✅ | 2026-07-18 |
| TASK-013 | Test `Handle_ShouldSoftDeleteProfile` — existing active profile → `IsActive = false`, `ModifiedAtUtc` touched, returns success | ✅ | 2026-07-18 |
| TASK-014 | Test `Handle_ShouldFail_WhenProfileNotFound` — non-existent `UserId` → `UserProfileResult.Failure.NotFound` | ✅ | 2026-07-18 |

## 3. Alternatives

- **ALT-001**: Use mocked `IApplicationDbContext` instead of real in-memory DB — rejected because Store tests use real EF Core to catch mapping and configuration issues at test time
- **ALT-002**: Combine all 4 feature tests into a single test class — rejected; violates single-responsibility and makes test discovery harder

## 4. Dependencies

- **DEP-001**: `ProfileUserFactory` at `tests/Module.UnitTests/Profile/Domain/ProfileUserFactory.cs` — already exists
- **DEP-002**: `UserProfileResult.Failure` error codes — defined in source
- **DEP-003**: `ProfileRequest` / `ProfileDetailResponse` / mapping extensions — defined in `Features/Admin/Profiles/Shared/`

## 5. Files

- **FILE-001**: `tests/Module.UnitTests/Profile/Features/Admin/Profiles/CreateUserProfile/CreateUserProfile.Tests.cs` — new file
- **FILE-002**: `tests/Module.UnitTests/Profile/Features/Admin/Profiles/GetUserProfile/GetUserProfile.Tests.cs` — new file
- **FILE-003**: `tests/Module.UnitTests/Profile/Features/Admin/Profiles/UpdateUserProfile/UpdateUserProfile.Tests.cs` — new file
- **FILE-004**: `tests/Module.UnitTests/Profile/Features/Admin/Profiles/DeleteUserProfile/DeleteUserProfile.Tests.cs` — new file

## 6. Testing

- **TEST-001**: Run `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Admin"` to verify all 4 new test files pass
- **TEST-002**: Run `dotnet build service/Api/src/Api/` to confirm zero regressions
- **TEST-003**: Run `dotnet test service/Api/tests/Module.UnitTests/` to confirm no existing tests broken

## 7. Risks & Assumptions

- **ASSUMPTION-001**: The `Shared/Admin/Profiles/Shared/` mapping and model classes expose the fields needed for test assertions
- **ASSUMPTION-002**: `AuditableBehavior.Touch` does not require a real HTTP context or user claim (tested in Store tests without one)
- **RISK-001**: `ProfileRequest` base class fields may change — tests should reference field-level assertions that fail safely during refactoring

## 8. Related Specifications / Further Reading

- Store test pattern reference: `tests/Module.UnitTests/Profile/Features/Store/Profile/Create/CreateProfile.Tests.cs`
- `UserProfileResult.Failure` codes: `service/Api/src/Module/Profile/Domain/UserProfile.Result.cs`
- Profile domain factory: `tests/Module.UnitTests/Profile/Domain/ProfileUserFactory.cs`
