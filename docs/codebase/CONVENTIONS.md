Short summary

Coding conventions and repository policies observed.

Key conventions (evidence-based)
- `TreatWarningsAsErrors=true` is enforced globally (`Directory.Build.props`).
- `InternalsVisibleTo` is set via `Directory.Build.props` so tests can access internals.
- Root `.editorconfig` defines C# naming and formatting rules.
- C# partial classes use dot-separated suffixes (e.g., `Country.Extensions.cs`).
- Frontend code uses oxfmt/oxlint + ESLint; Python uses Ruff.

Tooling conventions
- Use `dotnet build` / `dotnet test`; integration tests require Docker/Testcontainers.
- Frontends use `pnpm` and Vite; each SPA has its own pnpm workspace.

Evidence
- `Directory.Build.props`
- `.editorconfig`
- `guide/code-commenting/CommentingRules.xml`
- `AGENTS.md`

[ASK USER]
- No pre-commit hooks or branch protection files were found. Are there any team conventions (husky, pre-commit, GitHub branch rules) that should be documented?
# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| C# files | PascalCase, with dot-separated suffixes for partials | `Country.Extensions.cs`, `Variant.Validation.cs`, `Catalog.Extension.cs` | `service/Api/src/Module/Location/Domain/Countries/` |
| C# classes/types | PascalCase | `Country`, `GetProductDetail`, `ApplicationDbContext` | `.editorconfig:205` |
| C# interfaces | `I` prefix + PascalCase | `IStorageProvider`, `ICorrelationContext`, `IModuleMarker` | `.editorconfig:209` |
| C# methods | PascalCase | `AddCatalogModule()`, `ToResult()`, `UpdateAsync()` | `.editorconfig:217` |
| C# properties | PascalCase | `public string Name { get; set; }` | `.editorconfig:221` |
| C# local variables | camelCase | `var result = ...`, `string countryName` | `.editorconfig:229` |
| C# parameters | camelCase | `(string isoCode, CancellationToken ct)` | `.editorconfig:237` |
| C# private instance fields | `_camelCase` (underscore prefix) | `private readonly IDbContext _context;` | `.editorconfig:244-246` |
| C# private static fields | `s_camelCase` (s_ prefix) | `private static int s_counter;` | `.editorconfig:248-250` |
| C# constants (public + private) | PascalCase | `public const int MaxRetries = 3;` | `.editorconfig:252-258` |
| Vue/TS components | PascalCase | `App.vue`, `HomeView.vue`, `CartView.vue` | `app/Store/src/views/` |
| Vue/TS files (non-component) | camelCase | `main.ts`, `api.ts`, `cart.ts`, `catalog.ts` | `app/Store/src/` |
| .http test files | kebab-case | `products.http`, `auth-login.http`, `option-types.http` | `ApiTests/Catalog/Admin/` |
| Python files | snake_case | `embedding_router.py`, `local_storage.py`, `fashion_clip.py` | `service/Embedding/src/` |
| Python classes | PascalCase | `Settings`, `CORSMiddleware` | `service/Embedding/src/main.py` |
| Database tables/columns | snake_case (via EFCore.NamingConventions) | Schema: `catalog`, table: `variant_images`, column: `created_at` | `Directory.Packages.props:41`, `service/Api/src/Migrations/Migrations/` |

### 2) Formatting and Linting

- .NET formatter + linter: `.editorconfig` with `TreatWarningsAsErrors=true` (`Directory.Build.props:17`), `AnalysisLevel=latest` (`Directory.Build.props:14`), `EnforceCodeStyleInBuild=true` (`Directory.Build.props:16`)
- .NET indentation: 4 spaces for `.cs`, 2 spaces for `.csproj`, `.json`, `.props`, `.targets`, `.slnx` (`editorconfig:28`)
- C# specific formatting: K&R braces (`csharp_new_line_before_open_brace = all`), `using` directives outside namespace, file-scoped namespace declarations preferred, `var` usage discouraged (explicit types preferred)
- Frontend formatter: oxfmt v0.54.0 — `app/Admin/.oxfmtrc.json`, `app/Store/.oxfmtrc.json`
- Frontend linters: oxlint (~1.69.0) + ESLint (10.5.0) with Vue and TypeScript plugins — run via `pnpm run lint` which runs both sequentially
- Python linter: Ruff — rules `E, F, W, I` (pycodestyle, Pyflakes, isort) — `service/Embedding/pyproject.toml:24-25`
- Python line length: 100 characters — `service/Embedding/pyproject.toml:23`
- Most relevant enforced .NET rules:
  - `dotnet_style_readonly_field = true:warning` — mutable fields that could be readonly flagged as warnings
  - `csharp_prefer_static_local_function = true:warning` — non-static local functions captured as warnings
  - `CA1716` and `CA1848` are globally suppressed (`Directory.Build.props:19`)
  - Test projects suppress `CA1707` (xUnit snake_case test names), `CS1591` (no XML docs), `xUnit1051` (CancellationToken), `CA1861`

### 3) Import and Module Conventions

- C# using directive grouping: System directives first, then separated by groups — `dotnet_separate_import_directive_groups = true` (`.editorconfig:38`)
- C# using placement: outside namespace — `csharp_using_directive_placement = outside_namespace` (`.editorconfig:150`)
- Frontend path aliases: `@` alias maps to `./src` in both SPAs — `app/Admin/vite.config.ts:13`, `app/Store/vite.config.ts:12`
- Frontend import order: enforced by oxlint isort-style rules; ESLint `import/order` plugin
- Python: Ruff's `I` (isort) rules enforce import ordering — `service/Embedding/pyproject.toml:24`
- Barrel exports: Not widely used; each feature action is self-contained with its own files
- Module DI registration: Each C# module exposes an `Add{Module}Module()` extension on `IServiceCollection` — e.g., `Catalog.Extension.cs`, `Identity.Extensions.cs`

### 4) Error and Logging Conventions

- Error strategy: Result object pattern (`Result<T>` from `Shared/Application/Models/Results/`). Domain operations return explicit Success/Failure rather than throwing exceptions. Exception mapping behavior in MediatR pipeline catches unhandled exceptions and converts them to structured API error responses.
- Result types available: `Result` (void), `Result<T>` (value), `ValueResult<T>` (value with less allocation), `PagedResult<T>` (paginated)
- Error descriptors: `Shared/Application/Models/Descriptors/` — `Descriptor` and `OptionDescriptor` for label/code/value error payloads
- MediatR pipeline guarantees: Validation errors return before handler execution (ValidationBehavior), unhandled exceptions are caught and mapped (ExceptionMappingBehavior)
- Logging: `ILogger<T>` via standard Microsoft.Extensions.Logging — structured logging through OpenTelemetry pipeline (`Shared/Observability/Logging/Logging.Extension.cs`)
- Logger conventions: Domain entities define logger messages in `{Entity}.Loggers.cs` partials (e.g., `UserProfile.Loggers.cs`, `Country.Loggers.cs`)
- Logging context: Correlation ID propagated via `Shared/Observability/Correlation/CorrelationMiddleware.cs`
- Sensitive-data redaction: [TODO] — no explicit redaction policy found in code or configuration

### 5) Testing Conventions

- Test file naming: `*Tests.cs` or `*.Tests.cs` — e.g., `Country.Extensions.Tests.cs`, `Country.Configuration.Tests.cs`
- Test location: Separate test projects under `service/Api/tests/`:
  - `Api.Tests/` — Integration tests (requires Docker, hits real PostgreSQL + Redis via Testcontainers)
  - `Module.UnitTests/` — Unit tests (EF Core InMemory + Moq, no Docker)
  - `Shared.UnitTests/` — Unit tests for Shared infrastructure
- Test project auto-detection: Projects ending in `.Tests`, `.UnitTests`, `.IntegrationTests`, `.Specs` are auto-detected (`Directory.Build.props:72-77`)
- Mocking strategy: Moq for interfaces, EF Core InMemory for database in unit tests; Testcontainers + Respawn for integration tests
- Coverage expectation: Opt-in only — run with `/p:CollectCoverage=true` (`Directory.Build.props:95`)
- Frontend tests: Co-located in `src/__tests__/` with `*.spec.ts` naming — e.g., `App.spec.ts`, `cart.store.spec.ts`
- Python tests: pytest with `httpx` for HTTP testing — `service/Embedding/pyproject.toml` dev deps

### 6) Evidence

- `.editorconfig` — C# naming, formatting, and style rules (389 lines)
- `Directory.Build.props` — Build-wide MSBuild settings (analysis level, warnings-as-errors, InternalsVisibleTo, test detection)
- `Directory.Build.targets` — Architecture validation targets
- `Directory.Packages.props` — Central Package Management version definitions
- `service/Api/src/Shared/Application/Models/Results/` — Result object pattern (Result, ValueResult, PagedResult)
- `service/Api/src/Shared/Application/Mediators/Behaviours/` — Pipeline behaviors (Logging, Validation, ExceptionMapping)
- `service/Api/src/Module/Location/Domain/Countries/Country.Extensions.cs` — Domain partial class pattern
- `service/Api/src/Module/Catalog/Catalog.Extension.cs` — Module DI registration pattern
- `app/Admin/.oxfmtrc.json` — Admin SPA formatter config
- `app/Admin/.oxlintrc.json` — Admin SPA linter config
- `app/Store/.oxfmtrc.json` — Store SPA formatter config
- `app/Store/.oxlintrc.json` — Store SPA linter config
- `service/Embedding/pyproject.toml` — Python tooling (Ruff, pytest)
- `guide/code-commenting/CommentingRules.xml` — Temporal marker conventions (TODO, FIXME, HACK)
