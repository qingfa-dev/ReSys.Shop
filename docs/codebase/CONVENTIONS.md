# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

#### C# (.NET)

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| Files | PascalCase, one type per file (except partial classes) | `Order.cs`, `CreateOrderFromCart.cs`, `ICommand.cs` | `service/Api/src/Module/Ordering/Domain/Orders/` |
| Classes / Records / Structs | PascalCase | `Order`, `LineItem`, `CreateOrderFromCart`, `Result<T>` | `service/Api/src/Module/Ordering/Domain/Orders/Order.cs` |
| Interfaces | PascalCase with `I` prefix | `ICommand<T>`, `ICarterModule`, `ISender`, `IApplicationDbContext` | `service/Api/src/Shared/Application/Mediators/Commands/ICommand.cs` |
| Methods | PascalCase | `CreateOrderFromCart.Handle()`, `OrderMethod.Create()` | `service/Api/src/Module/Ordering/Features/` |
| Fields (private) | `_camelCase` prefix | `_dbContext`, `_currentUserMock` | `.editorconfig` (dotnet_naming_style._camelcase) |
| Fields (private static) | `s_camelCase` prefix | `s_defaultExpiration` | `.editorconfig` (dotnet_naming_style.s_camelcase) |
| Local variables | camelCase | `cart`, `paymentIntentId`, `variantIds` | Representative handler files |
| Parameters | camelCase | `command`, `cancellationToken`, `userId` | Representative handler files |
| Properties | PascalCase | `IsSuccess`, `StatusCode`, `Errors`, `Id` | `Result.cs`, `Order.cs` |
| Constants (public/private) | PascalCase | `ResultConstant.DefaultValues.StatusCode` | `Result.cs` |
| Namespaces | PascalCase, mirrors folder structure | `Module.Ordering.Domain.Orders`, `Shared.Application.Models.Results` | Feature files |
| Test files | Suffix `.Tests.cs`, mirror source namespace | `CreateOrderFromCartTests.cs` | `service/Api/tests/Module.UnitTests/` |

#### TypeScript (Vue SPAs)

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| Files | kebab-case or PascalCase for components | `PageShell.vue`, `auth.service.ts` | Vue SPA source structure |
| Components | PascalCase (Vue SFC) | `PageShell`, `PageHeader` | Git log (commit messages) |
| Imports | `@/` path alias → `./src/` | `import { Foo } from '@/features/...'` | `tsconfig.app.json` |

### 2) Formatting and Linting

#### C#
- **Formatter**: `.editorconfig` at repo root (MSBuild integrated)
- **Linter**: .NET analyzers (`AnalysisLevel=latest`, `EnableNETAnalyzers=true`), `TreatWarningsAsErrors=true`
- **Most relevant enforced rules**:
  - File-scoped namespaces preferred (`csharp_style_namespace_declarations = file_scoped`)
  - No `var` for primitives/built-in types (`csharp_style_var_elsewhere = false`)
  - Private fields `_camelCase`, private static `s_camelCase`
  - Braces required (`csharp_prefer_braces = true`)
  - Expression-bodied properties = true, methods = false
  - Unused parameters = suggestion
- **Run command**: `dotnet build` (warnings-as-errors, so any warning breaks build)

#### TypeScript (Vue SPAs)
- **Formatter**: Oxfmt (`oxfmt src/`) — no Prettier config
- **Linter**: Dual linter — ESLint (`eslint . --fix --cache`) + Oxlint (`oxlint . --fix`)
- **ESLint plugins**: `eslint-plugin-vue`, `eslint-config-prettier` (disables formatting rules), `eslint-plugin-boundaries` (Admin only, layer enforcement)
- **Admin module boundaries enforced**: shared cannot import features/app; features cannot import other features or app; app can import shared and features
- **TypeScript strictness**: `noUncheckedIndexedAccess: true` in both SPAs
- **Run commands**: `cd app/Admin && pnpm run lint` / `cd app/Store && pnpm run lint`

#### Python
- **Formatter/Linter**: Ruff — `line-length=100`, `target-version=py312`, select `["E", "F", "W", "I"]` (Embedding), `["E", "F", "I", "UP", "B", "SIM"]` (Benchmarks)
- **Run command**: `cd service/Embedding && uv run ruff check .` / `cd benchmarks && uv run ruff check src/`

### 3) Import and Module Conventions

#### C#
- **Import grouping**: System directives first, then non-System, with blank line separator (`dotnet_separate_import_directive_groups = true`, `dotnet_sort_system_directives_first = true`)
- **Using placement**: Outside namespace (`csharp_using_directive_placement = outside_namespace`)
- **Global usings**: `Shared/GlobalUsings.cs` provides common imports to all projects
- **Public exports**: No barrel file pattern; each type in its own file
- **Module boundaries**: Modules MUST NOT reference each other (MediatR `ISender` only)
- **Assembly reference chain**: `Api` → `Module` + `Shared` + `Migrations`; `Module` → `Shared`; `Shared` → `ServiceDefaults`
- **InternalsVisibleTo**: Non-test projects expose internals to `{Name}.Tests`, `{Name}.UnitTests`, `{Name}.IntegrationTests`, and `DynamicProxyGenAssembly2` (Moq) — see `Directory.Build.props`

#### TypeScript
- **Import aliasing**: `@/` → `./src/` in both Admin and Store SPAs
- **Import grouping**: ESLint `import/order` not detected — [TODO]
- **Auto-imports**: Admin uses `unplugin-auto-import` for Vue APIs; Store uses similar pattern
- **Boundary rules** (Admin only): `eslint-plugin-boundaries` enforces shared/features/app layer isolation

### 4) Error and Logging Conventions

#### C#
- **Error strategy**: Result monad everywhere in domain and application layers.
  - All domain operations return `Result<T>` or `Result` — never throw for expected failures
  - Error codes are typed strings: `"Order.NotFound"`, `"LineItem.VariantNotFound"`, `"Payment.AlreadyCaptured"`
  - Error factories are static classes: `OrderResult.Errors.NotFound(id)`, `PaymentResult.Errors.AlreadyCaptured(id)`
  - Endpoints convert `Result<T>` to HTTP responses via `result.ToResult()` extension
  - Exceptions only for unrecoverable infrastructure failures
- **Logging style**: Structured logging via `ILogger<T>`. Log messages use string interpolation with named placeholders
  - Each domain entity has a `{Entity}.Loggers.cs` partial class defining log message templates
  - Log level defaults: Information (app), Warning (framework), Debug (Development)
- **Sensitive data redaction**: `Observability.SensitiveHeaders` lists `["Authorization", "Cookie", "X-Api-Key"]`
- **Correlation**: `X-Correlation-Id` header propagated across HTTP calls

#### TypeScript
- [TODO] — error handling conventions not fully determinable from config files alone

#### Python
- Logging: `python-json-logger` for structured JSON logs; OpenTelemetry integration

### 5) Testing Conventions

- **Test file naming**: `{Feature}Tests.cs` — mirrors the feature name, placed in matching namespace under `tests/{Project}/`
- **Test file location**: Separate test projects (`tests/Module.UnitTests/`, `tests/Shared.UnitTests/`, `tests/Api.Tests/`)
- **Mocking (C#)**: Moq (`Mock<T>`) for interfaces; EF Core InMemory database for isolated handler tests
- **Test traits**: `[Trait("Category", "Unit")]`, `[Trait("Module", "Ordering")]`, `[Trait("Feature", "CreateOrderFromCart")]`
- **Test pattern**: AAA (Arrange/Act/Assert) with InMemory database reset per test (`Guid.NewGuid().ToString()` as DB name)
- **Coverage**: Opt-in via `dotnet test /p:CollectCoverage=true`; Coverlet outputs cobertura+json to `coverage/` directory
- **Python tests**: pytest with `asyncio_mode=auto`; benchmarks have 60% coverage minimum in CI
- **Vue tests**: Vitest with `jsdom` environment, tests in `src/**/__tests__/`

### 6) Evidence

- `.editorconfig` — all C# naming, formatting, and style rules
- `Directory.Build.props` — target framework, warnings-as-errors, test detection, InternalsVisibleTo
- `Directory.Build.targets` — architecture validation build targets
- `service/Api/src/Shared/GlobalUsings.cs` — global using directives
- `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` — typed error factory pattern
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — handler pattern
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs` — test pattern
- `app/Admin/eslint.config.ts` — Admin ESLint configuration with boundary rules
- `app/Admin/.oxlintrc.json` — oxlint configuration (referenced from eslint config)
- `guide/code-commenting/CommentingRules.xml` — TemporalMarker format (TODO, FIXME, HACK, etc.)
