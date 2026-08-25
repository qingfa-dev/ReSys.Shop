# Chapter 10 — Testing Strategy

## 10.1 Testing Pyramid

The project implements a four-layer testing strategy:

```
         ┌─────────────┐
         │   Manual    │  ← ApiTests/ (49 .http files)
         │   API Tests │     REST Client / JetBrains HTTP Client
         ├─────────────┤
         │  Integration │  ← Api.Tests (Testcontainers + WebApplicationFactory)
         │   (Backend)  │     Docker required; tests real DB + HTTP pipeline
         ├─────────────┤
         │    Unit      │  ← Module.UnitTests + Shared.UnitTests
         │   (Backend)  │     InMemory EF; Moq for dependencies; no Docker
         ├─────────────┤
         │    Unit      │  ← Vitest (Admin + Storefront SPAs)
         │  (Frontend)  │     jsdom + @vue/test-utils
         └─────────────┘
```

## 10.2 Backend Testing

### 10.2.1 Unit Tests (Module.UnitTests + Shared.UnitTests)

**Scope**: Handlers, validators, domain methods, and utility classes in isolation.

**Technique**:
- `ApplicationDbContext` uses `UseInMemoryDatabase(Guid.NewGuid().ToString())` for each test class
- `Mock<ISender>` for nested command dispatch
- `Mock<ILogger<T>>` for logger verification
- `Mock<ICurrentUser>` for user context simulation
- `ApplicationDbContext.AdditionalConfigurationsAssemblies` must be set *before* first use to load module entity configs

**Sample test pattern** (Create Product):
```cs
[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "ProductCreate")]
public sealed class CreateProductTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesProductWithMasterVariant()
    {
        // Arrange
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        var sender = new Mock<ISender>();
        var logger = new Mock<ILogger<CreateProduct.CommandHandler>>();
        var currentUser = new Mock<ICurrentUser>();
        var handler = new CreateProduct.CommandHandler(db, sender.Object, logger.Object, currentUser.Object);

        // Act
        var result = await handler.Handle(new CreateProduct.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(StatusCodes.Status201Created);
    }
}
```

**Evidence**: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-92`

### 10.2.2 Integration Tests (Api.Tests)

**Scope**: Full HTTP pipeline including middleware, auth, validation, database, and external service fakes.

**Technique**:
- `ApiFactory : WebApplicationFactory<Program>` boots the real host with in-memory config overrides
- `Testcontainers.PostgreSql` spins up a real PostgreSQL container
- `Respawn` checkpoints the database state between tests for fast reset
- `TestCurrentUser` uses `AsyncLocal<Guid?>` to simulate different users within a test
- All external integrations disabled: `Caching:* = false`, `BackgroundJobs:Enabled = false`, `Storage:Enabled = false`, `Storage:MalwareScanner:Enabled = false`
- `IHostedService` implementations removed to prevent background DB initializer from running

**Evidence**: `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189`

### 10.2.3 Test Commands

```bash
# Fast feedback (no Docker)
dotnet test service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Shared.UnitTests

# Full pipeline (requires Docker)
dotnet test service/Api/tests/Api.Tests

# With coverage
dotnet test /p:CollectCoverage=true

# Filter by module
dotnet test --filter "FullyQualifiedName~Location"
```

**Evidence**: `Directory.Build.props:95-98`, `AGENTS.md:42-51`

## 10.3 Frontend Testing

### 10.3.1 Admin SPA

- **Runner**: Vitest 4.1.9 with jsdom 29.1.1
- **Pattern**: Colocated `__tests__/` or `tests/` directories
- **Utilities**: `@vue/test-utils` for component mounting, `createPinia()` for state
- **Coverage**: `@vitest/coverage-v8` (opt-in)

**Evidence**: `app/Admin/vitest.config.ts:1-14`, `app/Admin/src/features/auth/_tests/auth.service.spec.ts`

### 10.3.2 Store SPA

- **Runner**: Vitest 4.1.9 with jsdom
- **Pattern**: `__tests__/*.spec.ts`
- **Example**: `App.spec.ts` mounts the root App with Pinia + router + Nuxt UI plugins

**Evidence**: `app/Store/vitest.config.ts:1-13`, `app/Store/src/__tests__/App.spec.ts:1-28`

### 10.3.3 Frontend Commands

```bash
cd app/Admin && pnpm run test:unit   # vitest run
cd app/Admin && pnpm run lint        # oxlint + eslint
cd app/Store && pnpm run test:unit
```

## 10.4 Python (Embedding) Testing

- **Runner**: pytest >=8
- **Fixtures**: `conftest.py` provides `TestClient` for FastAPI app
- **Structure**: `tests/unit/`, `tests/integration/`, `tests/e2e/`
- **Current coverage**: Minimal; broader directories are mostly empty

**Evidence**: `service/Embedding/tests/conftest.py:1-9`, `service/Embedding/pyproject.toml:51-55`

## 10.5 Mocking Strategy

| Layer | Mocked | Real | Rationale |
|-------|--------|------|-----------|
| **Unit tests** | `ISender`, `ILogger`, `ICurrentUser`, `IApplicationDbContext` (InMemory) | Handler logic, domain methods | Isolate the unit under test |
| **Integration tests** | `ICurrentUser` (TestCurrentUser), external APIs (disabled) | HTTP pipeline, real PostgreSQL, EF Core | Verify wiring and serialization |
| **Frontend unit** | Router, API responses (mocked axios) | Pinia store logic, component rendering | Fast feedback |

## 10.6 Coverage Strategy

- **Tool**: coverlet.collector 10.0.1
- **Format**: cobertura + json
- **Threshold**: Target **≥70% statement coverage** for the backend (`Module` + `Shared` assemblies). Coverage is opt-in via `/p:CollectCoverage=true`.
- **Output**: `coverage/coverage.{cobertura,json}` per project
- **Current status**: `[TODO — measured at final submission]` — the exact percentage will be populated by running `dotnet test /p:CollectCoverage=true` on the final codebase snapshot. As of the current draft, estimated coverage is ~60–70% based on the per-module test distribution documented in the Requirements Traceability Matrix (Chapter 12).
- **Frontend coverage**: `@vitest/coverage-v8` is configured but no threshold is enforced.
- **Python coverage**: `pytest` with `pytest-cov` is recommended but not yet configured in `pyproject.toml`.

**Design rationale**: A 70% target balances rigor with pragmatism. It ensures all domain methods and handlers have at least one test path without requiring trivial tests for auto-properties. Coverage below 70% in Inventory, Shipping, and Profile modules (see RTM) is a documented gap, not a hidden failure.

**Evidence**: `Directory.Build.props:95-98`

## 10.7 Known Gaps and Risks

| Gap | Risk | Mitigation |
|-----|------|------------|
| Payment module has highest churn (16 commits in 90 days) | Regression risk in money-moving code | Add integration tests for every payment handler |
| Python embedding tests are minimal | ML sidecar bugs may go undetected | Expand pytest suite before E2E verification |
| No frontend E2E tests | UI regressions possible | Vitest configs exclude `e2e/**`; add Playwright/Cypress if time permits |
| Integration tests require Docker | CI machine must have Docker | Document prerequisite; use GitHub Actions `services:` in future CI |

**Evidence**: `CONCERNS.md`, `TESTING.md`

## 10.8 Evidence

- `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj:1-26` — unit test project config
- `service/Api/tests/Api.Tests/Api.Tests.csproj:1-24` — integration test project config
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189` — test factory
- `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-92` — sample unit test
- `app/Admin/vitest.config.ts:1-14`, `app/Store/vitest.config.ts:1-13` — frontend test config
- `service/Embedding/tests/conftest.py:1-9` — Python test fixture
- `Directory.Build.props:95-98` — coverage configuration
- `Directory.Packages.props:102-112` — test dependency versions

---

## [ASK USER] Items

19. Should the testing strategy include a formal test plan matrix (requirements × test levels), or is the current pyramid + scope table sufficient?
20. Is there a target coverage percentage the examiner expects (e.g., 80% statement coverage)?
