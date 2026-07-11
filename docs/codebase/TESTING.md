Short summary

Testing strategies and frameworks observed in the repository.

Test types & locations
- Unit tests: `service/Api/tests/Module.UnitTests` and `Shared.UnitTests` use EF Core InMemory / Moq.
- Integration tests: `service/Api/tests/Api.Tests` use Testcontainers (PostgreSQL + Redis) and require Docker.
- HTTP tests: `ApiTests/` contains `.http` files for manual REST Client usage.
- Frontend tests: Vitest in `app/Admin/src/__tests__/` and `app/Store/src/__tests__/`.
- Python tests: pytest in `service/Embedding/`.

Commands
- Build + test: `dotnet build` and `dotnet test`.

Evidence
- `ApiTests/README.md`
- `service/Api/tests/Api.Tests/Api.Tests.csproj`
- `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj`
- `service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj`
- `ReSys.Shop.slnx`

[ASK USER]
- No CI pipeline exists. What is the team's priority and preferred platform (GitHub Actions, Azure DevOps, etc.) for automated build/test?
# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- Primary test frameworks:
  - .NET: xUnit v3 (3.2.2) via Microsoft.Testing.Platform (`global.json:7`, `Directory.Packages.props:110`)
  - Frontend: Vitest (4.1.9) (`app/Admin/package.json:56`, `app/Store/package.json:50`)
  - Python: pytest (>=8) (`service/Embedding/pyproject.toml`)
- Assertion/mocking tools:
  - .NET: FluentAssertions (8.10.0), Moq (4.20.72) (`Directory.Packages.props:104-105`)
  - Frontend: @vue/test-utils (2.4.11), jsdom for DOM simulation
- Commands:

```bash
# Run all .NET tests (unit + integration)
dotnet test

# Unit tests only (fast, no Docker)
dotnet test service/Api/tests/Module.UnitTests

# Integration tests only (requires Docker)
dotnet test service/Api/tests/Api.Tests

# Run filtered tests
dotnet test --filter "FullyQualifiedName~Location"

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Frontend — Admin SPA
cd app/Admin && pnpm run test:unit

# Frontend — Store SPA
cd app/Store && pnpm run test:unit

# Python embedding service
cd service/Embedding && uv run pytest
```

### 2) Test Layout

- Test file placement pattern: Separate test projects (not co-located with source)
  - .NET integration tests: `service/Api/tests/Api.Tests/` — organized as `Scenarios/{Module}/{Admin|Storefront}/{Feature}/`
  - .NET unit tests: `service/Api/tests/Module.UnitTests/` — mirrors `Module/` structure (`Catalog/Features/{Admin|Storefront}/`, `Identity/`, `Location/`, etc.)
  - .NET shared unit tests: `service/Api/tests/Shared.UnitTests/` — mirrors `Shared/` structure (`Application/`, `Governance/`, `Security/`, etc.)
  - Frontend tests: `src/__tests__/` co-located within SPA projects — `app/Admin/src/__tests__/`, `app/Store/src/__tests__/`
  - HTTP tests: `ApiTests/` — organized as `{Module}/{Admin|Storefront}/*.http`
- Naming convention:
  - .NET test classes: `*Tests.cs` or `*.Tests.cs` — e.g., `Country.Extensions.Tests.cs`, `GetProductDetail.Tests.cs`, `Country.Configuration.Tests.cs`
  - .NET test files are typically co-located in feature folders with the format `{Action}.Tests.cs` — e.g., `Create/Country.Create.Tests.cs`
  - Integration tests use `*.IntegrationTests.cs` pattern — e.g., `ListProducts.IntegrationTests.cs`
  - Frontend test files: `*.spec.ts` — e.g., `App.spec.ts`, `cart.store.spec.ts`
- Setup files and where they run:
  - `GlobalUsing.cs` in each test project — shared usings for all test files
  - `Api.Tests/Infrastructure/ApiIntegrationTestBase.cs` — base class for integration tests (provides `WebApplicationFactory`, Respawn DB reset)
  - `Api.Tests/Infrastructure/ModuleIntegrationTestBase.cs` — base class for module-scoped integration tests
  - `Api.Tests/Infrastructure/ApiCollection.cs` + `ApiFixture.cs` — shared test collection for xUnit
  - `xunit.runner.json` in each test project — xUnit v3 configuration
  - Frontend: `vitest.config.ts` per SPA project

### 3) Test Scope Matrix

| Scope | Covered? | Typical target | Notes |
|-------|----------|----------------|-------|
| Unit | Yes | Domain entity methods, validators, mappers, pipeline behaviors | `Module.UnitTests` and `Shared.UnitTests` — EF Core InMemory for data access, Moq for interfaces. ~750+ test classes (`*Tests.cs`) across all .NET test projects. |
| Integration | Yes | Full API request/response flows, database interactions | `Api.Tests` — uses Testcontainers (PostgreSQL + Redis) with real infrastructure. DB state reset via Respawn per test class. |
| E2E | No | Cross-service user journeys | [TODO] — No end-to-end tests covering Aspire-orchestrated multi-service flows (API + Embedding + Frontends) |
| HTTP (manual) | Partial | API endpoints via .http files | `ApiTests/` directory with organized .http files for manual testing in REST Client. Requires running API with seeded data. |
| Frontend component | Partial | Vue component rendering and store logic | Store SPA has `App.spec.ts` and `cart.store.spec.ts`; Admin SPA has only `App.spec.ts`. No comprehensive component test suite. |
| Python | Yes | Embedding service routes and logic | pytest with httpx for HTTP testing; [TODO] — exact coverage unknown, tests not inspected |

### 4) Mocking and Isolation Strategy

- Main mocking approach:
  - .NET: Moq for interface mocking, EF Core InMemory for database in unit tests (no real database)
  - .NET integration tests: Real PostgreSQL and Redis via Testcontainers Docker containers
  - Frontend: @vue/test-utils with jsdom for component mounting and DOM simulation
- Isolation guarantees:
  - Integration tests: DB state reset via **Respawn** per test class (`ApiIntegrationTestBase`), ensuring clean state for each test class
  - Unit tests: EF Core InMemory database recreated per test (state not shared between tests)
  - xUnit collections: `ApiCollection` groups tests sharing the same Testcontainers instance to avoid per-test container startup overhead
- Common failure mode in tests: Integration tests require Docker to be running; without Docker, all `Api.Tests` will fail with container startup errors

### 5) Coverage and Quality Signals

- Coverage tool + threshold:
  - .NET: Coverlet (10.0.1) — coverage is **opt-in** (`/p:CollectCoverage=true`). Output format: Cobertura + JSON to `coverage/` directory (`Directory.Build.props:95-97`)
  - Frontend: @vitest/coverage-v8 (4.1.7) — available but not yet configured with thresholds
  - No minimum coverage threshold is enforced in CI (no CI pipeline exists)
- Current reported coverage: [TODO] — no recent coverage reports available
- Known gaps/flaky areas:
  - Admin SPA has almost no tests (only placeholder `App.spec.ts`)
  - Store SPA has minimal test coverage (2 spec files)
  - Embedding service test coverage is unknown
  - HTTP tests (.http files) are manual-only, not automated
  - No E2E test infrastructure exists
  - No CI pipeline to run tests automatically on push/PR

### 6) Evidence

- `service/Api/tests/Api.Tests/Api.Tests.csproj` — Integration test project with Testcontainers + Respawn dependencies
- `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — Unit test project
- `service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj` — Shared infrastructure unit tests
- `service/Api/tests/Api.Tests/Infrastructure/ApiIntegrationTestBase.cs` — Integration test base class
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs` — WebApplicationFactory for API
- `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs` — Shared test collection fixture
- `Directory.Build.props:87-98` — Test project auto-detection, coverage configuration, suppressed warnings
- `global.json:6-8` — Test runner configuration (Microsoft.Testing.Platform)
- `Directory.Packages.props:108-112` — Testcontainers, Respawn, xUnit versions
- `app/Admin/vitest.config.ts` — Admin SPA test configuration
- `app/Store/vitest.config.ts` — Store SPA test configuration
- `ApiTests/README.md` — HTTP test documentation
