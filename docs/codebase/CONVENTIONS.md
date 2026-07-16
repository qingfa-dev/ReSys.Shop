# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| **C# files (features)** | `static partial class` split across `Name.cs`, `Name.Endpoint.cs`, `Name.Request.cs`, `Name.Response.cs`, `Name.Validator.cs` (all in the same folder). | `Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs`, `…Endpoint.cs`, `…Request.cs`, `…Response.cs`, `…Validator.cs` | `service/Api/src/Module/Catalog/Features/Admin/Products/Create/` directory listing |
| **C# files (domain)** | One aggregate per folder, with split files: `Type.cs`, `Type.Constant.cs`, `Type.Enumerate.cs`, `Type.Method.cs`, `Type.Result.cs`, `Type.Validation.cs`, `Type.Loggers.cs`. | `Module/Ordering/Domain/Orders/Order.{cs,Constant.cs,Checkout.cs,Enumerate.cs,Extensions.cs,Loggers.cs,Method.*.cs,Result.cs,Validation.cs}` | `service/Api/src/Module/Ordering/Domain/Orders/` |
| **C# files (persistence)** | `Persistence/<Module>Schema.cs` exposes `Name` + `TableNames` constants; `Persistence/Configurations/<Aggregate>/` for entity config; `Persistence/Seeders/` for seeders. | `Module/Catalog/Persistence/CatalogSchema.cs:1-31` | (same) |
| **C# module entry** | `<Name>.Extension.cs` (one per module) exporting `Add<Name>Module(this WebApplicationBuilder builder)`. | `Module/Catalog/Catalog.Extension.cs:16` | `Program.cs:38-45` |
| **Namespaces** | File-scoped namespaces (`csharp_style_namespace_declarations = file_scoped:suggestion`). | `namespace Module.Catalog.Features.Admin.Products.Create;` | `.editorconfig:129`, `CreateProduct.cs:5` |
| **Types / classes / records** | PascalCase. | `public static partial class CreateProduct`, `public sealed record Command(...)`, `public sealed class CommandHandler(...)` | `.editorconfig:204-206`, `CreateProduct.cs:10-24` |
| **Interfaces** | `IPascalCase`. | `ICarterModule`, `IInferenceClient`, `IRefreshTokenService`, `IApplicationDbContext` | `.editorconfig:208-210`, `CreateProduct.Endpoint.cs:12`, `ImageEmbedding.Inference.cs:6-8` |
| **Generic type parameters** | `TPascalCase`. | `TRequest`, `TResponse` | `.editorconfig:212-214` |
| **Methods** | PascalCase (instance and static). | `Handle`, `MapToDomain`, `MapToDetail`, `SaveChangesAsync` | `.editorconfig:216-218`, `CreateProduct.cs:36-78` |
| **Properties** | PascalCase. | `IsSuccess`, `StatusCode`, `IsFailure`, `MasterVariantId` | `.editorconfig:220-222`, `Result.cs:8-19` |
| **Events** | PascalCase. | (n/a in this repo) | `.editorconfig:224-226` |
| **Public / static fields** | PascalCase for `public` and `public static readonly`; private static fields use `s_camelCase`. | `public const string Name = "catalog";`, `public static readonly OptionDescriptor<string> Catalog = ...` | `.editorconfig:240-254`, `CatalogSchema.cs:11`, `PermissionContext.cs:13-17` |
| **Private instance fields** | `_camelCase`. | `private readonly IApplicationDbContext _dbContext;` | `.editorconfig:244-246`, `CartExpiryJob.cs:8-9` |
| **Constants** | PascalCase for `public const` / `public static readonly`; private `const` fields PascalCase per rule; *local* constants and locals `camelCase`. | `public const string Name = "catalog";`; `public const int DefaultValuesIsSuccess = 200;` | `.editorconfig:228-258`, `CatalogSchema.cs:11` |
| **Local variables & parameters** | `camelCase`. | `var request = command.Request;`; `var slugExists = await dbContext...` | `.editorconfig:228-238`, `CreateProduct.cs:38-54` |
| **Enums** | PascalCase (type and members). | `OrderStatus.Draft`, `OrderStatus.Expired` | `.editorconfig:268-274`, `CartExpiryJob.cs:27` |
| **Local functions** | PascalCase. | (n/a in this repo) | `.editorconfig:272-274` |
| **Logger / log message class** | Same name as containing class, suffixed `.Loggers.cs`, with `static partial class Loggers` containing `[LoggerMessage(...)]` methods. | `CreateProduct.cs` uses `ProductLoggers.Created(...)`; logger file would be in `Domain/Products/Product.Loggers.cs` | `CreateProduct.cs:73`, `Module/Catalog/Domain/Products/Product.Loggers.cs` (existence), `Shared/Application/Mediators/Behaviours/Logging/Logging.Behaviour.Logger.cs` (existence) |
| **TS/JS file naming (frontend)** | kebab-case or PascalCase components; tests as `*.spec.ts` colocated; Pinia stores as `<feature>.store.ts`; services as `<feature>.service.ts`; types as `<feature>.types.ts`; schemas as `<feature>.schema.ts`. | `app/Admin/src/features/auth/services/auth.service.ts`, `…/auth.store.spec.ts`, `…/auth.types.ts`, `…/auth.schema.ts`; `app/Store/src/stores/cart.ts` | `app/Admin/src/features/auth/`, `app/Store/src/stores/` |
| **Vue components** | PascalCase; `*View.vue` for pages, `*.spec.ts` for specs, `*.store.ts` for stores, `*.service.ts` for services. | `CartView.vue`, `ProductDetailView.vue` | `app/Store/src/views/` |
| **TS path alias** | `@/*` → `./src/*` (used in both SPAs). | `import App from '@/App.vue'` | `app/Admin/tsconfig.app.json:11`, `app/Store/tsconfig.app.json:11`, `app/Admin/vite.config.ts:45` |
| **Env vars (C#)** | Double-underscore delimited (e.g. `Authentication__Jwt__Secret`, `ConnectionStrings__DefaultConnection`, `VITE_API_URL`). | `appsettings.json` keys, `.env.template` env-var names | `service/Api/src/Api/.env.template:5-33` |
| **HTTP test files** | `<concern>.http` inside `ApiTests/<Module>/{Admin|Store|Storefront}/`. | `ApiTests/Catalog/Admin/products.http`, `ApiTests/Identity/Store/auth-login.http` | `ApiTests/Identity/Store/auth-login.http:1-15` |
| **HTTP test step names** | `### <Description>` (REST Client / JetBrains HTTP Client convention). | `### Login With Email - Success (200)` | `ApiTests/Identity/Store/auth-login.http:13` |

### 2) Formatting and Linting

- **C# formatter:** Built-in `dotnet format` driven by the root `.editorconfig` (`root = true`, `csharp_new_line_before_open_brace = all`, `csharp_style_namespace_declarations = file_scoped`, `csharp_prefer_braces = true`, etc.). Settings file: `.editorconfig:1-389`.
- **C# linter:** `EnableNETAnalyzers=true`, `AnalysisLevel=latest`, `EnforceCodeStyleInBuild=true`, `TreatWarningsAsErrors=true` (`Directory.Build.props:13-20`).
  - Suppressed globally: `CA1716` (reserved-language terms), `CA1848` (LoggerMessage vs ILogger) — `Directory.Build.props:19`.
  - Suppressed for test projects: `CS1591`, `CS1573`, `CA1707`, `xUnit1051`, `CA1861` — `Directory.Build.props:91`.
- **Frontend formatter (Admin & Store):** `oxfmt` (`oxfmt src/`). Config: `app/Admin/.oxfmtrc.json:1-5` (`"semi": false`, `"singleQuote": true`).
- **Frontend linter:** Two-step `oxlint` + `eslint` (`pnpm run lint` runs both with `--fix`).
  - `app/Admin/.oxlintrc.json:1-9`: plugins `eslint`, `typescript`, `unicorn`, `oxc`, `vue`, `vitest`; `correctness: error`.
  - `app/Admin/eslint.config.ts:1-57`: `eslint-plugin-vue` flat/essential, `vueTsConfigs.recommended`, `@vitest/eslint-plugin/recommended` (test files), `eslint-plugin-oxlint` (built from `.oxlintrc.json`), `eslint-plugin-boundaries` (see Import Conventions), `eslint-config-prettier/flat` skip.
- **TypeScript strictness (Admin):** `tsconfig.app.json:7` sets `noUncheckedIndexedAccess: true` (extends `@vue/tsconfig/tsconfig.dom.json`).
- **ESLint rules of note (Admin):** `boundaries/element-types` enforces `shared ⊥ features,app`; `features ⊥ features,app`; `app → shared,features` (`app/Admin/eslint.config.ts:32-54`).
- **Python (embedding):** `ruff` with `line-length = 100`, `target-version = "py313"`, lint select `["E", "F", "W", "I"]` (`service/Embedding/pyproject.toml:44-49`).
- **Test coverage threshold:** No enforced threshold; coverage is opt-in via `/p:CollectCoverage=true` (`Directory.Build.props:95-98`: `CoverletOutputFormat=cobertura,json`, output under `coverage/`).
- **Run commands:**
  ```bash
  # C#
  dotnet build
  dotnet format
  # TS/JS
  cd app/Admin && pnpm run lint   # oxlint + eslint
  cd app/Admin && pnpm run format # oxfmt
  # Python
  cd service/Embedding && uv run ruff check .
  ```

### 3) Import and Module Conventions

- **C# `using` ordering:** `dotnet_separate_import_directive_groups = true`, `dotnet_sort_system_directives_first = true`, `csharp_using_directive_placement = outside_namespace:silent` (`.editorconfig:38-40`, `.editorconfig:150`). Visible in every `*.cs` file (e.g. `CreateProduct.cs:1-5` — system `using` first, then `Module.*`, then `Shared.*`).
- **C# import aliasing:** No aliases; always use full namespace. Sample `CreateProduct.cs:1-4` shows fully qualified `using Module.Catalog.Domain.Products; using Module.Catalog.Features.Admin.Products.Shared.Mappings; using Module.Catalog.Features.Admin.Products.Variants.Add;`.
- **C# public-exports / barrel policy:** No barrel files. Each `*FeatureMetadata` type lives next to its feature (`Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs` etc.) and is referenced by its concrete namespace.
- **C# module isolation rule:** Modules never reference each other directly. Communication is via MediatR `ISender` only. Enforced in intent by the vertical-slice isolation target (`Directory.Build.targets:42-53`, currently `Condition="false"`).
- **TS imports:** Order is not strictly enforced by ESLint config; `eslint-config-prettier/flat` is applied to skip formatting disputes. `unplugin-auto-import` generates `src/auto-imports.d.ts` for `vue`, `vue-router` and `src/shared/composables` (`app/Admin/vite.config.ts:34-41`).
- **TS barrel / public-exports policy:** `app/Admin/src/shared/api/index.ts` and `app/Admin/src/shared/api/services/crud.service.ts` + `module-api.factory.ts` act as the public entry points for the API layer; `app/Admin/eslint.config.ts:35-40` exposes them to features via the boundaries rule (`features → shared` allowed).
- **Frontend module-boundary enforcement:** `eslint-plugin-boundaries` with the `shared | features | app` element map (`app/Admin/eslint.config.ts:32-54`).
- **TS path alias:** `@/*` → `./src/*` (`app/Admin/tsconfig.app.json:11`, `app/Store/tsconfig.app.json:11`); Vite alias mirror (`app/Admin/vite.config.ts:43-47`).
- **Python imports:** Standard `from embedding.<package> import …` (e.g. `service/Embedding/src/main.py:4-8`); packages listed in `pyproject.toml:23-42` map to the `src/` directory via `tool.setuptools.package-dir` (`pyproject.toml:20-21`).
- **Python (benchmarks):** `from benchmark.<package> import …` using absolute imports; `from __future__ import annotations` at top of all modules; line-length 100, target-version py312; ruff selects `["E", "F", "I", "UP", "B", "SIM"]` (`benchmarks/pyproject.toml:57-64`).

### 4) Error and Logging Conventions

- **Error strategy by layer:**
  - **Domain / Handlers:** Always return `Result` or `Result<T>`. No exceptions are thrown for control flow. Examples:
    - `Result.NotFound`, `Result.Conflict`, `Result.Validation`, `Result.Unexpected` (status-mapped factory methods) — `Shared/Application/Models/Results/Result.Method.cs:84-152`.
    - `Error` factory methods (`Shared/Application/Models/Errors/Error.Method.cs`) with implicit conversion to `Result` (`Result.Method.cs:172-185`).
  - **Pipeline behavior:** `ExceptionMappingBehavior<,>` catches any unhandled exception and converts it to `Error.Unexpected` with a structured code `$"{requestType}.Unexpected"` — `Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:11-42`.
  - **HTTP layer:** `Endpoint.cs` calls `result.ToResult()`; the `IResult` mapping in Carter returns the appropriate status code and JSON envelope (`CreateProduct.Endpoint.cs:14-32`).
  - **Storage/Notifications/Persistence:** Errors are returned as `Result`/`Error` from service interfaces (`IStorageService`, `INotificationService`, `IDatabaseTransaction`).
  - **Antiforgery / Headers / Rate limiting / CORS:** ASP.NET Core middleware; failures surface as 400/401/403/429 with the standard error envelope.
- **Logging style and required context fields:**
  - **Style:** `static partial class Loggers` containing `[LoggerMessage(...)]` source-generated methods, named after the action and including event id, level, and message template. Example: `Shared/Application/Mediators/Behaviours/Logging/Logging.Behaviour.Logger.cs` (file in scan list). `ExceptionMappingBehavior` defines `Loggers.UnhandledException` (`Exception.Behavior.cs:32`).
  - **Required context fields** in handler logs: `UserId`, `UserName`, `IpAddress` (via `ICurrentUser`), plus action-specific ids. Example: `CreateProduct.cs:73` calls `ProductLoggers.Created(logger, Name: product.Name, Id: product.Id, ActionBy: currentUser.UserName)`.
  - **Correlation:** every request gets a correlation id from `X-Correlation-Id` (`Shared/Observability/Correlation/CorrelationMiddleware.cs`); `appsettings.json:99` (`"CorrelationHeader": "X-Correlation-Id"`) and `Shared/Operational/Http/CorrelationIdPropagationHandler.cs` propagate it on outbound HTTP calls.
  - **Sensitive-data redaction:** observability settings include `SensitiveHeaders: ["Authorization", "Cookie", "X-Api-Key"]` (`appsettings.json:101`); the test host sets `Observability:SensitiveDataLogging=false` (`service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:40`).
- **Validation errors:** FluentValidation `Error.Validation(code, message, ("fields", propertyName))` — `Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:55-63`.

### 5) Testing Conventions

- **Test file naming (C#):** `<Subject>.Tests.cs` and `<Subject>.Validator.Tests.cs`, located under `service/Api/tests/Module.UnitTests/<Module>/Features/<Admin|Storefront>/<Feature>/<Action>/` mirroring production path. Example: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-5`.
- **Test file naming (frontend):** `*.spec.ts` colocated with the unit. Admin uses `__tests__/` (e.g. `app/Admin/src/features/auth/_tests/auth.service.spec.ts`) *or* inline `tests/` folders (e.g. `app/Admin/src/features/ordering/tests/order.service.spec.ts`, `app/Admin/src/features/catalog/products/tests/product.store.spec.ts`). Store uses `__tests__/` (e.g. `app/Store/src/__tests__/App.spec.ts`).
- **Test file naming (Python):** `test_*.py` (pytest discovery), in `service/Embedding/tests/` (unit, integration, e2e subdirs).
- **Test file naming (benchmarks):** `test_*.py` colocated in `benchmarks/src/tests/<domain>/` (cli, datasets, evaluation, integration, metrics, models, reporting, retrieval, utils); pytest via `pythonpath = ["src"]` in `benchmarks/pyproject.toml:54`.
- **Test scope (C#):** `Module.UnitTests` and `Shared.UnitTests` are pure unit tests using `Microsoft.EntityFrameworkCore.InMemory` (`service/Api/tests/Module.UnitTests/Module.UnitTests.csproj:18-20`); `Api.Tests` is integration (`Microsoft.AspNetCore.Mvc.Testing` + `Testcontainers.PostgreSql` + `Respawn`, `service/Api/tests/Api.Tests/Api.Tests.csproj:15-18`).
- **Test framework:** xUnit v3 (`xunit.v3 3.2.2`, runner.visualstudio 3.1.5, analyzers 1.27.0) with `Microsoft.NET.Test.Sdk 18.7.0`, `Microsoft.Testing.Platform` as the test runner (`global.json:6-8`), `TestingPlatformDotnetTestSupport=true` per test csproj, `FluentAssertions 8.10.0`, `Moq 4.20.72` (`Directory.Packages.props:102-112`).
- **Test traits:** Each unit test class is annotated with `[Trait("Category", "Unit")]`, `[Trait("Module", "Catalog")]`, `[Trait("Feature", "ProductCreate")]` (e.g. `CreateProduct.Tests.cs:7-9`).
- **Mocking strategy:**
  - **Handlers:** `Mock<ISender>` for nested dispatch; `Mock<ILogger<T>>`; `Mock<ICurrentUser>` — `CreateProduct.Tests.cs:13-35`.
  - **Integration:** Custom `ApiFactory : WebApplicationFactory<Program>` swaps configuration in-memory and replaces `ICurrentUser` with a `TestCurrentUser` (AsyncLocal-based) — `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:17-189`.
  - **Frontend:** Vitest with `jsdom`, `@vue/test-utils` (`mount`), real router/Pinia/ui plugins (`app/Store/src/__tests__/App.spec.ts:1-28`).
- **HTTP test artifacts (`ApiTests/`):** Manual test specs for REST Client / HTTP Client, not run by any test runner.
- **Coverage:** opt-in via `/p:CollectCoverage=true`; outputs `coverage/coverage.{cobertura,json}` per project (`Directory.Build.props:96-97`).
- **Parallelization:** `ParallelizeTestCollections=true`, `MaxParallelThreads=0` (all available) — `Directory.Build.props:92-93`.

### 6) Evidence

- `.editorconfig:1-389` — formatting + naming rules (single source of truth for C#)
- `Directory.Build.props:1-124` — analyzers, warnings-as-errors, InternalsVisibleTo, test project config
- `Directory.Build.targets:1-68` — architecture validation targets
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/*.cs` — vertical-slice anatomy
- `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs:1-99` — concrete handler
- `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:1-67` — pipeline behavior
- `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:1-42` — exception mapping
- `service/Api/src/Shared/Application/Models/Results/Result.Method.cs:1-191` — Result factories
- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` — pipeline order
- `app/Admin/.oxlintrc.json`, `app/Admin/.oxfmtrc.json`, `app/Admin/eslint.config.ts`, `app/Admin/tsconfig.app.json` — frontend formatting/lint config
- `app/Admin/src/app/main.ts:1-23`, `app/Admin/vite.config.ts:1-54` — Admin SPA bootstrap
- `app/Admin/src/shared/api/http/api.client.ts:1-92` — axios client w/ response unwrap + token refresh
- `app/Store/src/__tests__/App.spec.ts:1-28`, `app/Store/vitest.config.ts:1-13` — Store test pattern
- `service/Embedding/pyproject.toml:44-49` — Python lint config
- `benchmarks/pyproject.toml:57-64` — benchmark lint config
- `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-60+` — C# unit test
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189` — integration test factory
- `ApiTests/Identity/Store/auth-login.http:1-15`, `ApiTests/_shared/variables.http:1-20` — HTTP test conventions
- `service/Api/src/Api/appsettings.json:95-103` — observability config (correlation id, sensitive headers)
