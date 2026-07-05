# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| C# files | PascalCase | `Program.cs`, `CatalogExtension.cs`, `GetProductDetailPage.Tests.cs` | `service/Api/src/` |
| C# classes/types | PascalCase | `Product`, `IStorageProvider`, `CatalogExtensions` | `service/Api/src/` |
| C# methods | PascalCase | `AddCatalogModule()`, `InitializeDatabaseAsync()` | `service/Api/src/` |
| C# private fields | `_camelCase` | `_context`, `_logger`, `_repository` | `service/Api/src/` |
| Vue files | PascalCase | `App.vue`, `HomeView.vue`, `CartView.vue` | `app/Store/src/views/` |
| TS/JS variables | camelCase | `isLoading`, `items`, `fetchProducts()` | `app/Store/src/stores/` |
| Python files | snake_case | `main.py`, `health_router.py`, `embedding_service.py` | `service/Embedding/src/` |
| Python functions | snake_case | `register_exception_handlers()`, `health()` | `service/Embedding/src/` |
| .csproj files | PascalCase with dots | `Api.csproj`, `Api.Migrations.csproj`, `Module.UnitTests.csproj` | `service/Api/` |
| Config JSON | PascalCase | `ConnectionStrings`, `Authentication:Jwt:Secret` | `service/Api/src/Api/appsettings.json` |
| Env vars | `VITE_UPPER_SNAKE` (frontend), JSON path (backend) | `VITE_API_URL` | `app/Admin/.env.development` |
| .http test files | kebab-case | `products.http`, `auth-login.http`, `auth-register.http` | `ApiTests/` |

### 2) Formatting and Linting

- Formatter (.NET): Built-in `.editorconfig` + IDE code-style enforcement
- Formatter (JS/TS): **oxfmt** (`pnpm run format` runs `oxfmt src/`)
- Linter (JS/TS): **oxlint** + **ESLint** (run via `pnpm run lint`)
- Formatter (Python): **Ruff** configured in `pyproject.toml`
- Most relevant enforced rules (.NET): `TreatWarningsAsErrors=true`, `AnalysisLevel=latest`, `EnableNETAnalyzers=true`, `EnforceCodeStyleInBuild=true`
- Most relevant enforced rules (Python): Ruff selection `E`, `F`, `W`, `I` (pycodestyle, Pyflakes, pycodestyle warnings, isort)
- Most relevant enforced rules (JS/TS): ESLint via `eslint-plugin-vue` + `@vue/eslint-config-typescript`
- Run commands: `dotnet build` (build-time analysis), `pnpm run lint`, `pnpm run format`, `uv run ruff check`

### 3) Import and Module Conventions

- C# imports: `ImplicitUsings` enabled globally in `Directory.Build.props`; explicit `using` statements in files where needed; grouped by System → NuGet → Project
- Vue/TS imports: `@` path alias maps to `./src`; lazy-loaded route imports via `() => import('../views/...')`
- Python imports: Standard library → third-party → local (isort-enforced by Ruff)
- .NET `InternalsVisibleTo` configured centrally in `Directory.Build.props` for test assemblies (`*.Tests`, `*.UnitTests`, `*.IntegrationTests`, `DynamicProxyGenAssembly2`)

### 4) Error and Logging Conventions

- Error strategy by layer:
  - **Domain**: Result object pattern (`Result<T>.Success()`, `Result<T>.Failure()`) — exceptions not thrown from domain logic
  - **Application/MediatR**: `ExceptionMappingBehavior` catches unhandled exceptions and maps to structured API errors
  - **Presentation**: Carter endpoints delegate to MediatR pipeline; error responses use standard HTTP status codes
- Logging style: Structured logging via `ILogger<T>` throughout .NET code. Correlation ID propagated via `X-Correlation-Id` header (`Observability/Correlation/CorrelationMiddleware.cs`). Minimum log level configurable in settings.
- Sensitive-data redaction: `Observability.SensitiveHeaders` in `appsettings.json` redacts `Authorization`, `Cookie`, `X-Api-Key` from logs
- Python logging: `src/utils/logger.py` provides structured logging (still a stub)

### 5) Testing Conventions

- Test file naming/location rule (C#): Tests mirror source directory structure under `tests/` with `.Tests.cs` suffix. E.g., handler `Module/Catalog/Features/.../Handler.cs` → test at `tests/Module.UnitTests/Catalog/Features/.../Handler.Tests.cs`
- Test file naming/location rule (Vue): `__tests__/` directory co-located near source. E.g., `src/__tests__/App.spec.ts`
- Test file naming/location rule (Python): `tests/` at project root, mirrors `src/` structure
- Mocking strategy norm: Moq for .NET unit tests; Testcontainers for integration tests; `client: TestClient` for FastAPI tests
- Coverage expectation: `CollectCoverage` opt-in (not enforced by default), coverlet outputs Cobertura + JSON format

### 6) Evidence

- `.editorconfig` — formatting rules
- `Directory.Build.props` — code quality settings (lines 12-20)
- `Directory.Build.targets` — architecture validation targets
- `app/Admin/eslint.config.ts` — ESLint config
- `service/Embedding/pyproject.toml` — Ruff configuration
- `service/Api/src/Api/Program.cs` — startup conventions
- `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/GetDetailPage/GetProductDetailPage.Tests.cs` — representative test structure
