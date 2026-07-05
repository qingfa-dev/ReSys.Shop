# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- Primary test framework (.NET): **xUnit v3** (3.2.2)
- Primary test framework (Vue): **Vitest** (4.1.9)
- Primary test framework (Python): **pytest** (>=8)
- Assertion/mocking tools (.NET): **FluentAssertions** (8.10.0), **Moq** (4.20.72), **Testcontainers** (4.12.0), **Respawn** (7.0.0)
- Assertion/mocking tools (Vue): **@vue/test-utils** (2.4.11), **jsdom** (29.1.1)
- Commands:

```bash
# .NET — run all tests
dotnet test

# .NET — run specific project tests
dotnet test service/Api/tests/Module.UnitTests

# Vue — run all frontend tests
cd app/Admin && pnpm run test:unit
cd app/Store && pnpm run test:unit

# Python — run all embedding tests
cd service/Embedding && uv run pytest

# Coverage (opt-in)
dotnet test /p:CollectCoverage=true
```

### 2) Test Layout

- Test file placement pattern (.NET): Separate `tests/` directory mirroring `src/` structure (e.g., `src/Module/Catalog/Features/.../Handler.cs` → `tests/Module.UnitTests/Catalog/Features/.../Handler.Tests.cs`). Three test projects: `Api.Tests` (integration), `Module.UnitTests` (module-level unit), `Shared.UnitTests` (shared infrastructure unit).
- Test file placement pattern (Vue): `__tests__/` directory co-located under `src/` (e.g., `src/__tests__/App.spec.ts`)
- Test file placement pattern (Python): `tests/` at project root with `unit/`, `integration/`, `e2e/` subdirectories
- Naming convention (.NET): `{ClassName}.Tests.cs` or `{UseCaseName}.Tests.cs`. Tests mirror handler naming (e.g., `CreateTaxonUseCase.Tests.cs`, `Permission.Requirement.Handler.Tests.cs`)
- Setup files: `xunit.runner.json` in each test project, `GlobalUsing.cs` for shared usings, `conftest.py` for pytest fixtures

### 3) Test Scope Matrix

| Scope | Covered? | Typical target | Notes |
|-------|----------|----------------|-------|
| Unit | Yes | Handlers, validators, services, domain methods, mappings, middleware | `Module.UnitTests` (handlers + validators + services), `Shared.UnitTests` (auth, security, cache, permissions), Vitest (Vue components + stores) |
| Integration | Yes (partial) | Full request pipeline with real DB via Testcontainers | `Api.Tests` uses `Microsoft.AspNetCore.Mvc.Testing` + `Testcontainers.PostgreSql` + `Respawn` for DB cleanup |
| E2E | Partial | HTTP endpoints via REST Client (.http files) | `ApiTests/` contains 41 `.http` files covering all 4 modules, run manually via VS Code/JetBrains HTTP Client |
| E2E (Python) | Stub | `tests/e2e/` directory exists but is empty | `service/Embedding/tests/e2e/__init__.py` only |

### 4) Mocking and Isolation Strategy

- Main mocking approach (.NET unit): **Moq** for interface-based mocking (repositories, services, DbContext). `InternalsVisibleTo` + `DynamicProxyGenAssembly2` for mocking internal types.
- Main mocking approach (Python): **TestClient** (FastAPI) with dependency override pattern | `test_health.py`, `test_embedding.py`
- Isolation guarantees (.NET): Integration tests use **Testcontainers** (real PostgreSQL + Redis containers). **Respawn** resets database state between test runs. EF Core InMemory used in module unit tests where DB interaction is needed but persistence correctness is not the focus.
- Common failure mode: Testcontainers require Docker runtime. Tests fail if Docker is unavailable. `.http` files require running API instance with seeded data.

### 5) Coverage and Quality Signals

- Coverage tool + threshold: **coverlet** is configured but opt-in (`CollectCoverage=true` must be passed explicitly). Output: Cobertura + JSON format. No enforcement threshold detected.
- Current reported coverage: `[TODO]` — coverage not collected in normal runs
- Known gaps/flaky areas: Embedding service tests (`test_embedding.py`) return hardcoded empty results (stubs). Admin SPA has only 1 spec file. Integration tests (Api.Tests) may be flaky due to Testcontainers startup time. `.http` tests require manual environment setup.

### 6) Evidence

- `global.json` — Microsoft.Testing.Platform runner configured
- `Directory.Build.props:100-118` — test project references (xUnit, FluentAssertions, Moq, coverlet)
- `service/Api/tests/Api.Tests/Api.Tests.csproj` — integration test dependencies (Testcontainers, Respawn, Mvc.Testing)
- `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` — EF Core InMemory for unit tests
- `ApiTests/README.md` — HTTP test file documentation
- `app/Admin/vitest.config.ts` — Vitest configuration (jsdom environment)
- `app/Store/vitest.config.ts` — Vitest configuration (jsdom environment)
- `service/Embedding/tests/conftest.py` — pytest fixture (TestClient)
