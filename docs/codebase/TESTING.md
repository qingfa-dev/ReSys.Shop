# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- **Primary test framework (backend):** **xUnit v3** `3.2.2` + `xunit.runner.visualstudio 3.1.5` + `xunit.analyzers 1.27.0` (`Directory.Packages.props:110-112`); runner = `Microsoft.Testing.Platform` (`global.json:6-8`).
- **Primary test framework (frontend):** **Vitest 4.1.9** (`app/Admin/package.json:65`, `app/Store/package.json:50`) with `jsdom 29.1.1` env (`app/Admin/vitest.config.ts:8-12`, `app/Store/vitest.config.ts:5-11`).
- **Primary test framework (Python — Embedding):** **pytest >=8** (`service/Embedding/pyproject.toml:53`).
- **Primary test framework (Python — Benchmarks):** **pytest >=8.2** + `pytest-cov >=5.0` (`benchmarks/pyproject.toml:38-39`); runner configured in `[tool.pytest.ini_options]` with `testpaths=["src/tests"]`, `pythonpath=["src"]` (`benchmarks/pyproject.toml:52-55`).
- **Assertion libraries:**
  - Backend: `FluentAssertions 8.10.0` (`Directory.Packages.props:104`).
  - Frontend: `expect` from `vitest`.
  - Python: built-in `pytest` assertions.
- **Mocking tools:**
  - Backend: `Moq 4.20.72` (`Directory.Packages.props:105`).
  - Integration: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`), `Respawn 7.0.0` (DB checkpoint), `Testcontainers.PostgreSql 4.12.0`, `Testcontainers.Redis 4.12.0` (`Directory.Packages.props:101,107-109`).
  - Frontend: `@vue/test-utils 2.4.11` (`mount`, global plugins), `@vitest/eslint-plugin 1.6.20` for lint hints.
  - Python: `httpx >=0.28` for ASGI test client.
- **Commands:**
  ```bash
  # Backend
  dotnet build                                                       # warnings-as-errors
  dotnet test service/Api/tests/Shared.UnitTests                     # Shared unit
  dotnet test service/Api/tests/Module.UnitTests                     # Module unit
  dotnet test service/Api/tests/Api.Tests                            # Integration (Testcontainers)
  dotnet test /p:CollectCoverage=true                                # opt-in coverage
  dotnet test --filter "FullyQualifiedName~Location"                 # scope by module

  # Admin SPA
  cd app/Admin && pnpm run test:unit                                 # vitest run
  pnpm run lint

  # Store SPA
  cd app/Store && pnpm run test:unit
  pnpm run lint

  # Embedding
  cd service/Embedding && uv run pytest
  uv run ruff check .

  # Benchmarks
  cd benchmarks && uv run pytest
  uv run ruff check .

  # Manual HTTP tests
  # Open ApiTests/run-all.http in VS Code (REST Client) or JetBrains HTTP Client
  ```

- **CI automation:** `.github/workflows/ci.yml` runs `dotnet build`, `dotnet test` (unit only), `pnpm run lint && pnpm run test:unit` for both SPAs, and `uv run ruff check . && uv run pytest` on every PR/push to `dev` and `main`. Integration tests (Testcontainers) are **not** currently run in CI.

### 2) Test Layout

- **C# unit tests (Module / Shared):** Mirror the source tree under `service/Api/tests/Module.UnitTests/<Module>/Features/<Admin|Storefront>/<Feature>/<Action>/` (e.g. `Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-5`).
- **C# integration tests:** `service/Api/tests/Api.Tests/Scenarios/<Concern>/...` (Catalog, Identity, Location, Ordering, Payment, Profile, Shipping, Webhooks, Shared, Host, AntiForgery) + `service/Api/tests/Api.Tests/Infrastructure/` for the shared harness.
- **Frontend Admin tests:** Colocated `__tests__/` or `tests/` directories next to source — `app/Admin/src/features/auth/_tests/auth.{service,store}.spec.ts`, `app/Admin/src/features/catalog/products/tests/product.store.spec.ts`, `app/Admin/src/features/ordering/tests/order.{service,store}.spec.ts`, `app/Admin/src/shared/api/http/api.client.spec.ts`.
- **Frontend Store tests:** `app/Store/src/__tests__/{App.spec.ts, cart.store.spec.ts}`.
- **Python tests:** `service/Embedding/tests/` with `unit/`, `integration/`, `e2e/` subdirectories and top-level `test_*.py` files (`test_embedding.py`, `test_exception_handler.py`, `test_health.py`).
- **Benchmarks tests:** `benchmarks/src/tests/` with per-domain subdirectories: `cli/` (CLI command tests), `datasets/` (ground truth + loader validation), `evaluation/` (thesis + pipeline + stats), `integration/` (pgvector), `metrics/` (precision, recall, mAP, nDCG), `models/` (registry + per-model), `reporting/` (JSON, CSV, Markdown, Typst), `retrieval/` (pgvector extended), `utils/` (timing).
- **Setup files:**
  - Backend: `service/Api/tests/Api.Tests/ApiCollection.cs`, `ApiFixture.cs`, `ApiFactory.cs`, `ModuleIntegrationTestBase.cs`, `ApiIntegrationTestBase.cs`, `Auth/AuthTokenHelper.cs`, `Http/{HttpClientExtensions,ResponseHelper,ResultExtensions}.cs`, `AuthenticatedRequestExtensions.cs`.
  - Frontend: `app/Admin/vitest.config.ts`, `app/Store/vitest.config.ts` (both extend `vite.config.ts`).
  - Python: `service/Embedding/tests/conftest.py:1-9` (provides `client: TestClient`).
- **Naming:**
  - C#: `X.Tests.cs`, `X.Validator.Tests.cs` (mirror source file names).
  - Frontend: `*.spec.ts`.
  - Python: `test_*.py`.

### 3) Test Scope Matrix

| Scope | Covered? | Typical target | Notes |
|-------|----------|----------------|-------|
| **Unit (backend)** | Yes | Handlers, validators, option-config classes. | `Module.UnitTests` & `Shared.UnitTests` use `Microsoft.EntityFrameworkCore.InMemory` (`*.csproj:18-20`); `ApplicationDbContext.AdditionalConfigurationsAssemblies` is set in the test base to scan module entity configs (`CreateProduct.Tests.cs:24`). |
| **Unit (frontend)** | Yes | Pinia stores, services, axios client behaviour, smoke mount of `App.vue`. | `app/Store/src/__tests__/App.spec.ts:1-28` mounts with Pinia + router + Nuxt UI; `app/Admin/src/shared/api/http/api.client.spec.ts` exercises the axios interceptors. |
| **Unit (Python — Embedding)** | Yes | Routes, exception handlers, models. | `test_embedding.py`, `test_exception_handler.py`, `test_health.py` (existence per scan). |
| **Unit (Python — Benchmarks)** | Yes | Model registration, metrics computation, dataset loading, reporting formatters. | 29 test files in `benchmarks/src/tests/` across 8 domains; run via `cd benchmarks && uv run pytest`. |
| **Integration (backend)** | Yes | API endpoints, JWT auth, anti-forgery, webhooks. Requires Docker for `Testcontainers.PostgreSql`. | `Api.Tests` projects use `WebApplicationFactory<Program>` + `Respawn` for DB resets. |
| **Integration (Python — Embedding)** | Yes | Full app via `TestClient`. | `service/Embedding/tests/conftest.py:1-9`. |
| **Integration (Python — Benchmarks)** | Yes | pgvector retrieval tests against a real PostgreSQL instance. | `benchmarks/src/tests/integration/test_pgvector.py`, `benchmarks/src/tests/retrieval/test_pgvector_extended.py`. |
| **E2E (Python — Embedding)** | Folder present, content sparse | `service/Embedding/tests/e2e/` exists (existence per scan); coverage details `[TODO]`. | — |
| **E2E (frontend)** | No | — | Vitest config explicitly excludes `e2e/**` (`app/Admin/vitest.config.ts:9`, `app/Store/vitest.config.ts:6`). |
| **Manual API tests** | Yes | All endpoints, organized by module. | `ApiTests/` (49 `.http` files); top-level `run-all.http` orchestrates a flow. |

### 4) Mocking and Isolation Strategy

- **Backend unit-test mocks (Moq):** `Mock<ISender>` for nested command dispatch, `Mock<ILogger<T>>`, `Mock<ICurrentUser>`, `Mock<IOptions<T>>` as needed. `ApplicationDbContext` is created with `UseInMemoryDatabase(Guid.NewGuid().ToString())` so each test class has an isolated store. Example: `CreateProduct.Tests.cs:13-42`.
- **Backend integration-test isolation:** `WebApplicationFactory<Program>` overrides config in-memory; the integration test host sets `Caching:* = false`, `BackgroundJobs:Enabled = false`, `Storage:Enabled = false`, `Storage:MalwareScanner:Enabled = false`, etc. (`service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:32-85`). `services.RemoveAll<IHostedService>()` prevents the background DB initializer from running during tests (line 89). `Respawn` is used to checkpoint/reset the PostgreSQL DB between tests.
- **Current-user stub:** `TestCurrentUser` exposes `AsyncLocal<Guid?>` so non-HTTP tests can switch the user between MediatR dispatches; falls back to `HttpContext` claims when AsyncLocal is unset (`ApiFactory.cs:98-189`).
- **Hangfire disabled in tests** (`BackgroundJobs:Enabled = false`) so scheduled jobs don't run.
- **Frontend test isolation:** Vitest uses `jsdom`; `mount(App, { global: { plugins: [createPinia(), router, ui] } })` provides a self-contained context.
- **Common failure mode in tests:**
  - `TestContext.Current.CancellationToken` is required on handler calls (xUnit v3 token; e.g. `CreateProduct.Tests.cs:54`); older xUnit signatures would fail. The `xunit1051` analyzer is suppressed for this reason (`Directory.Build.props:91`).
  - `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly]` must be set *before* the DbContext is used, otherwise the in-memory provider won't apply module entity configs (`CreateProduct.Tests.cs:24`).
  - `MaximumKeyLength` / `MaximumPayloadBytes` constraints in `Caching.Hybrid` are not enforced in tests because caching is fully disabled.

### 5) Coverage and Quality Signals

- **Coverage tool:** `coverlet.collector 10.0.1` (`Directory.Packages.props:103`).
- **Threshold:** No enforced threshold; coverage is opt-in (`/p:CollectCoverage=true`). Output format `cobertura,json` to `coverage/` directory (`Directory.Build.props:96-97`).
- **Current reported coverage:** `[TODO]` — not measured in this codebase dump; commands above are how to opt in.
- **Known gaps / flaky areas:**
  - Integration tests require Docker (Testcontainers). On hosts without Docker, `dotnet test` on `Api.Tests` will fail.
  - Two Payment test files are the highest-churn areas in the last 90 days (see `CONCERNS.md`).
  - The embedding Python tests are minimal (`tests/test_*.py` + `conftest.py` only); broader unit/integration directories are mostly empty.
  - The `.http` files in `ApiTests/` are *not* executed by any test runner — they're manual.

### 6) Evidence

- `Directory.Packages.props:99-112` — testing dependency versions
- `Directory.Build.props:70-118` — `IsTestProject` detection, InternalsVisibleTo, test project config, `CollectCoverage` toggle
- `global.json:6-8` — `Microsoft.Testing.Platform` runner
- `service/Api/tests/Api.Tests/Api.Tests.csproj:1-24` — integration test project
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189` — `WebApplicationFactory` with `TestCurrentUser`
- `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj:1-26` — unit test project
- `service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj:1-25` — unit test project
- `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-92` — sample Moq-based unit test
- `service/Api/tests/Api.Tests/Scenarios/HealthCheckTests.cs` (existence) — health-check integration test (added by commit `6d08980b`)
- `app/Admin/vitest.config.ts:1-14`, `app/Store/vitest.config.ts:1-13` — frontend test config
- `app/Store/src/__tests__/App.spec.ts:1-28` — frontend mount test pattern
- `app/Admin/src/features/auth/_tests/auth.service.spec.ts` (existence) — frontend service test
- `app/Admin/src/shared/api/http/api.client.spec.ts` (existence) — axios interceptor test
- `service/Embedding/tests/conftest.py:1-9` — Python test fixture
- `service/Embedding/tests/test_embedding.py`, `test_exception_handler.py`, `test_health.py` (existence) — Python tests
- `service/Embedding/pyproject.toml:51-55` — pytest/httpx/ruff dev deps
- `benchmarks/pyproject.toml:38-39,52-55` — benchmarks test config
- `benchmarks/src/tests/` — 29 benchmark test files across 8 domains
- `ApiTests/README.md:1-30`, `ApiTests/_shared/variables.http:1-20`, `ApiTests/Identity/Store/auth-login.http:1-15` — manual HTTP tests
