---
goal: Integration Tests for AntiForgery Token Endpoint
version: 1.0
date_created: 2026-07-02
owner: ReSys Team
status: Planned
tags: test, integration, security
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add integration tests for the `GET /api/v1/antiforgery/token` endpoint defined in `Shared/Security/AntiForgery/Endpoints/AntiForgeryEndpoints.cs`. The endpoint is anonymous and returns a CSRF token via `IAntiforgery.GetAndStoreTokens()`.

## 1. Requirements & Constraints

- **REQ-001**: Tests must follow the existing pattern: inherit from `ApiIntegrationTestBase`, accept `ApiFixture` via primary constructor, use `[Collection("ApiIntegration")]` via the base class
- **REQ-002**: Test file must be placed at `service/Api/tests/Api.Tests/Scenarios/AntiForgery/AntiForgeryTokenTests.cs`
- **REQ-003**: All tests must use `FluentAssertions` for assertions and `HttpClient` from the base class
- **REQ-004**: The endpoint `GET /api/v1/antiforgery/token` is `AllowAnonymous()` — no auth token is needed
- **REQ-005**: The endpoint returns `Result<TokenResponse>` — use `ReadApiResponseAsync()` to parse the response, then `DeserializeValue<TokenResponse>()` to extract the value
- **REQ-006**: The test configuration (`appsettings.Testing.json` + `ApiFactory` in-memory overrides) sets `AntiForgery:HeaderName` to `"X-XSRF-TOKEN"` (from `AntiForgerySettingConstant.Defaults.HeaderName`) — the response's `HeaderName` must match
- **REQ-007**: The test configuration (`appsettings.Testing.json`) sets `AntiForgery:CookieName` to `"XSRF-TOKEN"` — the response `Set-Cookie` header must contain this cookie name
- **REQ-008**: Do not modify any source code — only add test files
- **CON-001**: The test environment has `AntiForgery:Required=false` and `AntiForgery:IsEnabled=false` — this does not affect the token generation endpoint since `IAntiforgery.GetAndStoreTokens()` works regardless
- **CON-002**: Test containers require Docker/Podman — the `ApiFixture.ConfigureContainerRuntime()` handles socket detection
- **PAT-001**: Follow the exact naming and structure pattern from existing tests like `CreateCountryIntegrationTests.cs`

## 2. Implementation Steps

### Implementation Phase 1 — Create AntiForgery Integration Tests

- GOAL-001: Implement all integration test methods for the AntiForgery token endpoint

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create directory `service/Api/tests/Api.Tests/Scenarios/AntiForgery/` | | |
| TASK-002 | Create `service/Api/tests/Api.Tests/Scenarios/AntiForgery/AntiForgeryTokenTests.cs` with the following test class: `public sealed class AntiForgeryTokenTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)` | | |
| TASK-003 | Add test `GetToken_ReturnsOk` — sends `GET /api/v1/antiforgery/token`, asserts response status code is `HttpStatusCode.OK` | | |
| TASK-004 | Add test `GetToken_ReturnsSuccessResult` — parses response via `ReadApiResponseAsync()`, asserts `IsSuccess` is `true` | | |
| TASK-005 | Add test `GetToken_ReturnsTokenResponse` — deserializes value via `DeserializeValue<TokenResponse>()`, asserts result is not null, `Token` is not null or empty, `HeaderName` is not null or empty | | |
| TASK-006 | Add test `GetToken_ReturnsExpectedHeaderName` — asserts `HeaderName` equals `"X-XSRF-TOKEN"` (the value from `AntiForgerySettingConstant.Defaults.HeaderName`) | | |
| TASK-007 | Add test `GetToken_SetsAntiforgeryCookie` — inspects response `Set-Cookie` headers, asserts at least one `Set-Cookie` header contains `"XSRF-TOKEN"` (the configured cookie name) | | |

### Implementation Phase 2 — Verify Tests Compile and Run

- GOAL-002: Validate the test project builds and the new tests execute successfully

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Run `dotnet build service/Api/tests/Api.Tests/Api.Tests.csproj` and confirm 0 errors | | |
| TASK-009 | Run `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~AntiForgeryTokenTests" --no-restore` and confirm all tests pass | | |

## 3. Alternatives

- **ALT-001**: Write unit tests mocking `IAntiforgery` — not chosen because the endpoint behavior (cookie setting, token generation) relies on ASP.NET Core middleware state which is better tested via integration tests
- **ALT-002**: Create a separate test fixture with anti-forgery enabled — not needed because `IAntiforgery.GetAndStoreTokens()` works regardless of the `Required`/`IsEnabled` flags in the test config
- **ALT-003**: Test in a dedicated class with `IClassFixture<ApiFixture>` instead of collection — not chosen because the existing pattern (`Collection("ApiIntegration")` via `ApiIntegrationTestBase`) provides database reset between tests and is the project convention

## 4. Dependencies

- **DEP-001**: `ApiFixture` — provides `PostgreSqlContainer`, `ApiFactory`, `HttpClient`, `ResetDatabaseAsync()`
- **DEP-002**: `ApiIntegrationTestBase` — abstract base class with `[Collection("ApiIntegration")]` and automatic `ResetDatabaseAsync()` on `InitializeAsync()`
- **DEP-003**: `TokenResponse` record at `Shared/Security/AntiForgery/Endpoints/TokenResponse.cs`
- **DEP-004**: `ApiResponse` helpers at `Api.Tests.Infrastructure.Http.ResponseHelper`
- **DEP-005**: Docker/Podman runtime for `PostgreSqlContainer` (handled by `ApiFixture.ConfigureContainerRuntime()`)

## 5. Files

- **FILE-001**: `service/Api/tests/Api.Tests/Scenarios/AntiForgery/AntiForgeryTokenTests.cs` — new integration test file

## 6. Testing

- **TEST-001**: `dotnet build` on the test project produces 0 errors
- **TEST-002**: `dotnet test` with the `AntiForgeryTokenTests` filter passes all 5 test methods
- **TEST-003**: Each test method validates a specific aspect: status code, success flag, response shape, header name, and cookie presence

## 7. Risks & Assumptions

- **RISK-001**: If Docker/Podman is not available, `PostgreSqlContainer` will fail to start and all tests will fail — this is not a code issue but a runtime prerequisite shared by all integration tests
- **RISK-002**: The `Set-Cookie` header assertion may fail if ASP.NET Core changes the cookie default name — the expected cookie name (`"XSRF-TOKEN"`) comes from `appsettings.Testing.json:AntiForgery:CookieName`, which matches the test config
- **ASSUMPTION-001**: `IAntiforgery.GetAndStoreTokens()` works without a database connection (it stores tokens in a cookie, not the database)
- **ASSUMPTION-002**: The `HeaderName` in the response will be `"X-XSRF-TOKEN"` — this is the value from `AntiForgerySettingConstant.Defaults.HeaderName` since no test config override exists for `AntiForgery:HeaderName` in `appsettings.Testing.json` or `ApiFactory`

## 8. Related Specifications / Further Reading

- [ASP.NET Core AntiForgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery)
- [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Existing test pattern: `CreateCountryIntegrationTests.cs`](file://service/Api/tests/Api.Tests/Scenarios/Location/Admin/Countries/Create/CreateCountryIntegrationTests.cs)
