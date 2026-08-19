# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- **Primary .NET test framework**: xUnit v3 (3.2.2) via Microsoft.Testing.Platform
- **Assertion/mocking tools**: FluentAssertions (8.10.0), Moq (4.20.72)
- **Coverage tool**: Coverlet (10.0.1) — opt-in via `/p:CollectCoverage=true`
- **Python test framework**: Pytest (>=8.0.0) with `pytest-asyncio`, `pytest-env`
- **Vue test framework**: Vitest (^4.1.9) with jsdom environment, @vue/test-utils (^2.4.11)

**Commands:**

```bash
# .NET
dotnet test service/Api/tests/Module.UnitTests        # Fast unit tests (no Docker, InMemory DB)
dotnet test service/Api/tests/Shared.UnitTests        # Shared infrastructure unit tests
dotnet test service/Api/tests/Api.Tests               # API integration tests (requires Docker)
dotnet test                                           # All tests
dotnet test /p:CollectCoverage=true                   # Opt-in coverage (cobertura+json)

# Admin SPA
cd app/Admin && pnpm run test:unit                    # Vitest unit tests

# Store SPA
cd app/Store && pnpm run test:unit                    # Vitest unit tests

# Python Embedding
cd service/Embedding && uv run pytest                 # All tests

# Python Benchmarks
cd benchmarks && uv run pytest --ignore=src/tests/integration/  # Unit tests only
cd benchmarks && uv run pytest                                      # All tests including integration
cd benchmarks && uv run pytest --cov=benchmark --cov-fail-under=60  # Coverage (CI requirement)
```

### 2) Test Layout

- **Test file placement pattern**: Separate test projects, not co-located with source.
  - `.NET`: `tests/Module.UnitTests/`, `tests/Shared.UnitTests/`, `tests/Api.Tests/` — each mirrors the source namespace structure
  - **Vue**: Tests in `src/**/__tests__/` directories alongside source files (co-located test files)
  - **Python**: Django-like `tests/` directory per project
- **Naming convention**: `{Feature}Tests.cs` (C#), `*.test.ts` or `*.spec.ts` (Vue), `test_*.py` (Python)
- **Setup files and where they run**:
  - `xunit.runner.json` in each test project root (copied to output) — minimal config (Module.UnitTests, Shared.UnitTests: empty; Api.Tests: `parallelAlgorithm: Aggressive`)
  - Auto-detected test projects via `Directory.Build.props` (projects ending in `.Tests`, `.UnitTests`, `.IntegrationTests`, `.Specs`)
  - No shared test fixtures or base classes detected across projects

### 3) Test Scope Matrix

| Scope | Covered? | Typical target | Notes |
|-------|----------|----------------|-------|
| Unit (.NET) | Yes | Module and Shared feature handlers, domain logic | Uses EF Core InMemory DB (not mocking), Moq for external interfaces (ICurrentUser, INotificationService, IPaymentGatewayActionProvider) |
| Unit (Vue) | Yes | Vue components, composables, stores | Vitest + jsdom + @vue/test-utils |
| Unit (Python) | Yes | Embedding service models/schemas, benchmark logic | Pytest with standard fixtures |
| Integration (.NET) | Yes | API endpoints with real PostgreSQL/Redis | `Api.Tests` project uses Testcontainers (PostgreSql) + Respawn (7.0.0) for DB reset. Redis is NOT tested via Testcontainers in Api.Tests csproj. |
| Integration (Python) | Yes | Benchmark integration tests with pgvector | `benchmarks/src/tests/integration/` — excluded from CI unit run |
| Integration (Python Embedding) | Yes | Full FastAPI stack | Marked with `@pytest.mark.integration` |
| E2E | No | — | `ApiTests/` contains 49 `.http` files for manual endpoint testing but no automated E2E tests |

### 4) Mocking and Isolation Strategy

- **Main mocking approach (.NET)**:
  - **Database**: EF Core InMemory database (not mocked) — handlers test against real EF Core behavior. Each test creates a unique DB via `Guid.NewGuid().ToString()` for isolation.
  - **Interfaces**: Moq (`Mock<ICurrentUser>`, `Mock<INotificationService>`, `Mock<IPaymentGatewayActionProvider>`, etc.) with `Setup()` for behavior and `Verify()` for assertions
  - **Cross-module calls**: NOT mocked in unit tests (modules in same assembly) — features use real `ISender`/MediatR or direct service/navigation access against InMemory DB
- **Mocking approach (Python)**: Standard pytest fixtures, `unittest.mock` or `pytest-mock` for external dependencies
- **Mocking approach (Vue)**: Vitest built-in mocking (`vi.mock()`, `vi.fn()`) with `@vue/test-utils` for component mounting
- **Isolation guarantees**:
  - `.NET unit tests`: Fresh InMemory DB per test class (constructor creates new DB). `IDisposable` disposes context.
  - `.NET integration tests (`Api.Tests`)`: Testcontainers spin up real PostgreSQL/Redis per test run; Respawn resets DB state between tests
  - Vue tests: `jsdom` environment provides clean DOM per test
- **Common failure mode**: Cross-module dependencies in unit tests mean tests for one module may fail due to issues in another module's configuration (e.g., Payment tests requiring Order domain assembly for EF Core configuration). Tests use `[Trait("Category", "Unit")]`, `[Trait("Module", "Ordering")]`, `[Trait("Feature", "CreateOrderFromCart")]` for filtering.

### 5) Coverage and Quality Signals

- **Coverage tool + threshold**: Coverlet (C#), `@vitest/coverage-v8` (Vue), `pytest-cov` (Python benchmarks)
- **Current reported coverage**: [TODO] — no coverage reports found in repository
- **Coverage thresholds**:
  - Python benchmarks CI: `--cov-fail-under=60` (minimum 60% line coverage)
  - C#: No threshold enforced (opt-in only)
  - Vue: No threshold enforced
- **Known gaps/flaky areas**:
  - No E2E/acceptance test suite — 49 `.http` files for manual testing only
  - Stripe webhook tests exist as unit tests (mocked gateway) but no integration test against Stripe test mode
  - Embedding service integration tests are marked but [TODO] — coverage extent unknown
  - `ValidateVerticalSliceIsolation` build target guards against future module-split ProjectReference cycles (all modules share one `Module.csproj`; cross-module references are permitted)
  - [TODO] Flaky test catalog unknown — need to analyze CI history

### 6) Evidence

- `Directory.Build.props` — auto test project detection, test package references, coverlet config, InternalsVisibleTo
- `Directory.Packages.props` — test package versions (xunit.v3, Moq, FluentAssertions, Testcontainers, Respawn, etc.)
- `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — test project setup
- `service/Api/tests/Api.Tests/Api.Tests.csproj` — integration test project with Testcontainers
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs` — representative unit test
- `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` — representative unit test
- `app/Admin/vitest.config.ts` — Vue unit test config
- `benchmarks/pyproject.toml` — Python benchmark test config with coverage threshold
- `.github/workflows/ci.yml` — CI test execution commands
