---
goal: Integration Tests for Identity Module API Endpoints
version: 1.0
date_created: 2026-07-02
status: Planned
tags: feature, testing, integration, identity
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create a comprehensive integration test suite for the `service/Api/src/Module/Identity/` module, covering all storefront (anonymous and authenticated) and admin (permission-protected) API endpoints. Tests follow the established pattern from `Scenarios/Location/` tests, using `ApiFixture`, `ApiIntegrationTestBase`, `PostAsAdminRawAsync`/`DeleteAsAdminRawAsync`, `ReadApiResponseAsync()`, `ReadAsPagedResultAsync<T>()`, and FluentAssertions.

## 1. Requirements & Constraints

- **REQ-001**: Every implemented Identity endpoint must have at least one success-path integration test
- **REQ-002**: Every mutation endpoint must have an error-path test (404, 409, 422, 401)
- **REQ-003**: Anonymous store endpoints must have an unauthenticated success test
- **REQ-004**: Authenticated store endpoints must have both authenticated success and unauthenticated 401 tests
- **REQ-005**: Admin endpoints must use `PostAsAdminRawAsync`/`DeleteAsAdminRawAsync` for authenticated requests
- **REQ-006**: Admin endpoints must have a no-auth 401 test
- **REQ-007**: Test files must use anonymous types for request bodies (same pattern as Location tests)
- **REQ-008**: Tests rely on `ResetDatabaseAsync()` before each test via `ApiIntegrationTestBase`
- **REQ-009**: All response assertions must use FluentAssertions
- **REQ-010**: File paths must follow the pattern `Scenarios/Identity/{Area}/{Feature}/{Action}/{Action}IntegrationTests.cs`
- **REQ-011**: Each test class must be `sealed` and use primary constructor `(ApiFixture fixture) : ApiIntegrationTestBase(fixture)`
- **CON-001**: Password validation requires min 12 chars with uppercase, lowercase, and digit
- **CON-002**: UserName validation requires 3-32 chars matching `^[a-zA-Z0-9](?:[a-zA-Z0-9._-]{1,30}[a-zA-Z0-9])?$`
- **CON-003**: Email validation regex: `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$`
- **CON-004**: Phone validation regex: `^\+[1-9]\d{1,14}$` (E.164 format)
- **CON-005**: No Identity seeders exist; tests must create their own test data via the API
- **PAT-001**: Use `HttpResponseMessage response = await Client.PostAsAdminRawAsync(uri, body)` for admin POST/PUT/PATCH/DELETE
- **PAT-002**: Use `ApiResponse result = await response.ReadApiResponseAsync()` for single-item responses
- **PAT-003**: Use `result.DeserializeValue<T>()` to extract typed values from `ApiResponse`
- **PAT-004**: Use `result.IsSuccess.Should().BeTrue()` / `result.StatusCode.Should().Be(HttpStatusCode.XXX)` for assertions
- **PAT-005**: Use `PagedResult<T> result = await response.ReadAsPagedResultAsync<T>()` for paged list responses
- **PAT-006**: Use `Client.PostAsJsonAsync(uri, body)` (no auth) for 401 tests
- **PAT-007**: Use `Client.DeleteAsync(uri)` (no auth) for 401 tests on DELETE
- **PAT-008**: Use `Client.PutAsJsonAsync(uri, body)` (no auth) for 401 tests on PUT
- **PAT-009**: Use `Client.PatchAsJsonAsync(uri, body)` (no auth) for 401 tests on PATCH

## 2. Implementation Steps

### Implementation Phase 1: Store Auth - Anonymous Endpoints

- GOAL-001: Implement integration tests for all anonymous store authentication endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Scenarios/Identity/Store/Auth/Password/PasswordLoginIntegrationTests.cs` with tests for `POST api/store/identity/auth/login/password`: valid credentials flow (expect 200 with `BaseTokenResponseModel` shape), invalid credential (expect 401), missing fields (expect 422), disabled account (expect 401/403) | | |
| TASK-002 | Create `Scenarios/Identity/Store/Auth/External/Providers/ExternalProvidersIntegrationTests.cs` with tests for `GET api/store/identity/auth/login/external/providers`: returns provider list (expect 200 with `PagedResult`), no auth needed | | |
| TASK-003 | Create `Scenarios/Identity/Store/Passwords/Forgot/RequestPasswordResetIntegrationTests.cs` with tests for `POST api/store/identity/passwords/forgot`: valid email sends reset (expect 204), nonexistent email (expect 404), missing email (expect 422) | | |
| TASK-004 | Create `Scenarios/Identity/Store/Passwords/Reset/ResetPasswordIntegrationTests.cs` with tests for `POST api/store/identity/passwords/reset`: invalid token (expect 400/404), missing fields (expect 422) | | |
| TASK-005 | Create `Scenarios/Identity/Store/Emails/Confirm/ConfirmEmailIntegrationTests.cs` with tests for `POST api/store/identity/emails/confirm`: invalid token (expect 400/404), missing UserId (expect 422) | | |
| TASK-006 | Create `Scenarios/Identity/Store/Emails/Resend/ResendEmailVerificationIntegrationTests.cs` with tests for `POST api/store/identity/emails/resend`: valid email resent (expect 200), nonexistent email (expect 404), missing email (expect 422) | | |

### Implementation Phase 2: Store Auth - Authenticated Endpoints

- GOAL-002: Implement integration tests for store authentication endpoints requiring authentication

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create `Scenarios/Identity/Store/Passwords/Change/ChangePasswordIntegrationTests.cs` with tests for `POST api/store/identity/passwords/change`: valid password change (expect 202), wrong current password (expect 400), weak new password (expect 422), no auth (expect 401) | | |
| TASK-008 | Create `Scenarios/Identity/Store/Emails/Change/ChangeEmailIntegrationTests.cs` with tests for `POST api/store/identity/emails/change`: valid email change (expect 200), invalid new email (expect 422), wrong password (expect 400), no auth (expect 401) | | |

### Implementation Phase 3: Admin Users CRUD

- GOAL-003: Implement integration tests for admin user management endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `Scenarios/Identity/Admin/Users/Create/CreateUserIntegrationTests.cs` with tests for `POST api/identity/users`: valid create returns user (expect 201 with `CreateUser.Response` shape having `Id`, `Email`, `UserName`), duplicate email returns 409, missing required fields returns 422, no auth returns 401 | | |
| TASK-010 | Create `Scenarios/Identity/Admin/Users/GetPagedOrAll/GetUsersPagedOrAllIntegrationTests.cs` with tests for `GET api/identity/users`: returns paged users (expect 200 with `PagedResult<GetUsersPagedOrAll.Response>`), respects pagination, filters by `IsActive`, no auth returns 401 | | |
| TASK-011 | Create `Scenarios/Identity/Admin/Users/GetById/GetUserByIdIntegrationTests.cs` with tests for `GET api/identity/users/{id:guid}`: returns user by ID (expect 200 with `GetUserById.Response`), nonexistent ID returns 404, no auth returns 401 | | |
| TASK-012 | Create `Scenarios/Identity/Admin/Users/Update/UpdateUserIntegrationTests.cs` with tests for `PUT api/identity/users/{id:guid}`: valid update returns updated user (expect 200 with `UpdateUser.Response`), nonexistent ID returns 404, conflict on duplicate email returns 409, invalid fields return 422, no auth returns 401 | | |
| TASK-013 | Create `Scenarios/Identity/Admin/Users/Delete/DeleteUserIntegrationTests.cs` with tests for `DELETE api/identity/users/{id:guid}`: delete existing user (expect 200 with `DeleteUser.Response`), nonexistent ID returns 404, no auth returns 401 | | |
| TASK-014 | Create `Scenarios/Identity/Admin/Users/Status/ToggleUserStatusIntegrationTests.cs` with tests for `PATCH api/identity/users/{id:guid}/status`: toggle active status (expect 200), nonexistent ID returns 404, no auth returns 401 | | |

### Implementation Phase 4: Admin User Roles

- GOAL-004: Implement integration tests for admin user role management endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `Scenarios/Identity/Admin/Users/Roles/Get/GetUserRolesIntegrationTests.cs` with tests for `GET api/identity/users/{id:guid}/roles`: returns roles with `IsAssigned` flags (expect 200 with `GetUserRoles.Response`), nonexistent user returns 404, no auth returns 401 | | |
| TASK-016 | Create `Scenarios/Identity/Admin/Users/Roles/Assign/AssignUserRolesIntegrationTests.cs` with tests for `POST api/identity/users/{id:guid}/roles/assign`: assign roles to user (expect 200), invalid role names return 422, nonexistent user returns 404, no auth returns 401 | | |
| TASK-017 | Create `Scenarios/Identity/Admin/Users/Roles/Revoke/RevokeUserRolesIntegrationTests.cs` with tests for `DELETE api/identity/users/{id:guid}/roles/revoke`: revoke roles from user (expect 200), nonexistent user returns 404, no auth returns 401 | | |
| TASK-018 | Create `Scenarios/Identity/Admin/Users/Roles/Sync/SyncUserRolesIntegrationTests.cs` with tests for `PATCH api/identity/users/{id:guid}/roles/sync`: sync roles to exact set (expect 200), empty roles list clears all (expect 200), nonexistent user returns 404, no auth returns 401 | | |

### Implementation Phase 5: Admin User Permissions

- GOAL-005: Implement integration tests for admin user permission management endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Create `Scenarios/Identity/Admin/Users/Permissions/Get/GetUserPermissionsIntegrationTests.cs` with tests for `GET api/identity/users/{id:guid}/permissions`: returns permissions tree with `IsAssigned` flags (expect 200 with `GetUserPermissions.Response`), nonexistent user returns 404, no auth returns 401 | | |
| TASK-020 | Create `Scenarios/Identity/Admin/Users/Permissions/Assign/AssignUserPermissionsIntegrationTests.cs` with tests for `POST api/identity/users/{id:guid}/permissions/assign`: assign permissions to user (expect 200), invalid permission IDs return 422, nonexistent user returns 404, no auth returns 401 | | |
| TASK-021 | Create `Scenarios/Identity/Admin/Users/Permissions/Revoke/RevokeUserPermissionsIntegrationTests.cs` with tests for `DELETE api/identity/users/{id:guid}/permissions/revoke`: revoke permissions from user (expect 200), nonexistent user returns 404, no auth returns 401 | | |
| TASK-022 | Create `Scenarios/Identity/Admin/Users/Permissions/Sync/SyncUserPermissionsIntegrationTests.cs` with tests for `PUT api/identity/users/{id:guid}/permissions/sync`: sync permissions to exact set (expect 200), empty permissions list clears all (expect 200), nonexistent user returns 404, no auth returns 401 | | |

### Implementation Phase 6: Admin Roles CRUD

- GOAL-006: Implement integration tests for admin role management endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Create `Scenarios/Identity/Admin/Roles/Create/CreateRoleIntegrationTests.cs` with tests for `POST api/identity/roles`: valid create returns role (expect 201 with `CreateRole.Response` having `Id`, `Name`, `IsSystem`, `CreatedAtUtc`), duplicate name returns 409, missing name returns 422, no auth returns 401 | | |
| TASK-024 | Create `Scenarios/Identity/Admin/Roles/GetPagedOrAll/GetRolesPagedOrAllIntegrationTests.cs` with tests for `GET api/identity/roles`: returns paged roles (expect 200 with `PagedResult<GetRolesPagedOrAll.Response>`), no auth returns 401 | | |
| TASK-025 | Create `Scenarios/Identity/Admin/Roles/GetById/GetRoleByIdIntegrationTests.cs` with tests for `GET api/identity/roles/{id:guid}`: returns role by ID (expect 200 with `GetRoleById.Response`), nonexistent ID returns 404, no auth returns 401 | | |
| TASK-026 | Create `Scenarios/Identity/Admin/Roles/Update/UpdateRoleIntegrationTests.cs` with tests for `PUT api/identity/roles/{id:guid}`: valid update returns updated role (expect 200 with `UpdateRole.Response`), nonexistent ID returns 404, conflict on duplicate name returns 409, no auth returns 401 | | |
| TASK-027 | Create `Scenarios/Identity/Admin/Roles/Delete/DeleteRoleIntegrationTests.cs` with tests for `DELETE api/identity/roles/{id:guid}`: delete existing role (expect 200 with `DeleteRole.Response`), nonexistent ID returns 404, system role delete returns 403, no auth returns 401 | | |

### Implementation Phase 7: Admin Role Permissions

- GOAL-007: Implement integration tests for admin role permission management endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Create `Scenarios/Identity/Admin/Roles/Permissions/Get/GetRolePermissionsIntegrationTests.cs` with tests for `GET api/identity/roles/{id:guid}/permissions`: returns permissions tree (expect 200 with `GetRolePermissions.Response`), nonexistent role returns 404, no auth returns 401 | | |
| TASK-029 | Create `Scenarios/Identity/Admin/Roles/Permissions/Assign/AssignRolePermissionsIntegrationTests.cs` with tests for `PUT api/identity/roles/{id:guid}/permissions/assign`: assign permissions to role (expect 200), invalid permission IDs return 422, nonexistent role returns 404, no auth returns 401 | | |
| TASK-030 | Create `Scenarios/Identity/Admin/Roles/Permissions/Revoke/RevokeRolePermissionsIntegrationTests.cs` with tests for `DELETE api/identity/roles/{id:guid}/permissions/revoke`: revoke permissions from role (expect 200), nonexistent role returns 404, no auth returns 401 | | |
| TASK-031 | Create `Scenarios/Identity/Admin/Roles/Permissions/Sync/SyncRolePermissionsIntegrationTests.cs` with tests for `PATCH api/identity/roles/{id:guid}/permissions/sync`: sync permissions to exact set (expect 200), nonexistent role returns 404, no auth returns 401 | | |

### Implementation Phase 8: Admin Permissions Catalog

- GOAL-008: Implement integration tests for the system permissions catalog endpoint

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | Create `Scenarios/Identity/Admin/Permissions/Get/GetPermissionsIntegrationTests.cs` with tests for `GET api/identity/permissions`: returns system permissions list (expect 200 with `PagedResult<PermissionMetadata>`), results are not empty, no auth returns 401 | | |

### Implementation Phase 9: Identity Test Infrastructure Helpers

- GOAL-009: Create helper utilities to reduce test duplication for identity-specific patterns

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | Create `Scenarios/Identity/Helpers/IdentityTestHelper.cs` — static helper class with methods: `CreateTestUserAsync(HttpClient)` that creates a valid user via admin POST and returns `(Guid Id, string Email, string UserName)`; `CreateTestRoleAsync(HttpClient)` that creates a valid role and returns `(Guid Id, string Name)`; `GetFirstUserIdAsync(HttpClient)` that gets the first user ID from the paged list; `GetFirstRoleIdAsync(HttpClient)` that gets the first role ID from the paged list; `ValidPassword` constant = `"TestPass1234!"`; `ValidUserName(string prefix)` that generates a unique username | | |

## 3. Alternatives

- **ALT-001**: Place all Identity tests in a single monolithic file — rejected because the Location tests follow the one-file-per-feature pattern, which provides better organization and parallel test execution
- **ALT-002**: Use Moq to mock the Identity services — rejected because integration tests must validate the full pipeline including Carter modules, MediatR handlers, EF Core persistence, and ASP.NET Identity stores
- **ALT-003**: Create Identity seeders for test data — deferred because the current Location pattern creates test data via API calls within each test, which is more explicit and maintainable

## 4. Dependencies

- **DEP-001**: `service/Api/tests/Api.Tests/Infrastructure/` — existing test infrastructure (ApiFactory, ApiFixture, ApiIntegrationTestBase, AuthTokenHelper, HttpClientExtensions, ResponseHelper, ResultExtensions) is already in place
- **DEP-002**: `service/Api/src/Module/Identity/` — all Identity module endpoints must be implemented and compiling
- **DEP-003**: `service/Api/tests/Api.Tests/Api.Tests.csproj` — project file references all required packages
- **DEP-004**: `service/Api/tests/Module.UnitTests/Identity/Fixtures/IdentityMocks.cs` — provides mock factory for Identity dependencies (used by unit tests, not directly by integration tests)
- **DEP-005**: `service/Api/tests/Api.Tests/Usings.cs` — already contains global usings for `FluentAssertions`, `Api.Tests.Infrastructure.Http`, `Shared.Application.Models.Results`, and `System.Net.Http.Json`

## 5. Files

- **FILE-001** to **FILE-032**: Each test file listed in the phases above (32 test files)
- **FILE-033**: `Scenarios/Identity/Helpers/IdentityTestHelper.cs` — shared test helper
- **FILE-034**: `Scenarios/Identity/Helpers/` — directory for helper utilities

## 6. Testing

- **TEST-001**: Each test file must be independently runnable via `dotnet test --filter "FullyQualifiedName~{TestClassName}"`
- **TEST-002**: All tests in a single file must pass when run together (shared state via `ResetDatabaseAsync()` before each test)
- **TEST-003**: The entire Identity test suite must not break existing Location/AntiForgery integration tests
- **TEST-004**: Compilation check: `dotnet build service/Api/tests/Api.Tests/Api.Tests.csproj` must pass before test execution

## 7. Risks & Assumptions

- **RISK-001**: The `HasPermission` attribute on admin endpoints may reject the test admin JWT if the test token's claims do not match the permission system's expectations. **Mitigation**: Verify that `AuthTokenHelper.GenerateAdminToken()` with `ClaimTypes.Role = "Admin"` satisfies the permission check, as it does for Location admin endpoints.
- **RISK-002**: Store endpoints requiring authentication (ChangePassword, ChangeEmail) need a valid user session/token. **Mitigation**: For these tests, either (a) create a user via admin API, then login via `PasswordLogin` to get a token, or (b) generate a user JWT directly using `AuthTokenHelper`-like approach but with user claims. The `IdentityTestHelper` should provide a `GenerateUserToken(Guid userId, string email)` method.
- **RISK-003**: Password reset and email confirmation flows involve tokens generated internally, which are not exposed in API responses. **Mitigation**: Tests for these flows will validate error paths (invalid token, missing fields) rather than full end-to-end success flows, since the success flow requires accessing the token from external email.
- **ASSUMPTION-001**: The `Api.Program` class assembly is discoverable by `WebApplicationFactory<Program>` — verified by existing Location tests
- **ASSUMPTION-002**: The `ResetDatabaseAsync()` call before each test provides a clean state with only identity-related seed data — notably, the Identity module has no seeders, so the database starts empty for User/Role tables

## 8. Related Specifications / Further Reading

- `service/Api/tests/Api.Tests/Scenarios/Location/` — reference implementation for all test patterns
- `service/Api/src/Module/Identity/Features/Identity.Feature.cs` — route constants for all endpoints
- `service/Api/tests/Api.Tests/Infrastructure/Auth/AuthTokenHelper.cs` — JWT generation for admin tests
- `service/Api/tests/Api.Tests/Infrastructure/Auth/AuthenticatedRequestExtensions.cs` — authenticated HTTP extensions
- `service/Api/tests/Api.Tests/Infrastructure/Http/HttpClientExtensions.cs` — unauthenticated HTTP extensions
- `service/Api/tests/Api.Tests/Infrastructure/Http/ResponseHelper.cs` — `ApiResponse` deserialization helpers
- `service/Api/tests/Api.Tests/Infrastructure/Http/ResultExtensions.cs` — `Result<T>` and `PagedResult<T>` deserialization helpers
