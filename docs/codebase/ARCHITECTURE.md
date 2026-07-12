# Architecture

## Core Sections (Required)

### 1) Architectural Style

- **Primary style:** Modular monolith (one deployable `Api` process) decomposed into 8 feature modules, each implemented as **vertical slices** (CQRS-style command/query handlers grouped per action) on top of **MediatR** with explicit **pipeline behaviors** for logging, validation, and exception mapping.
- **Why this classification:**
  - The .NET solution has a single host `service/Api/src/Api/Program.cs:26-66` that wires 8 module registration calls (`AddLocationModule`, `AddIdentityModule`, `AddProfilesModule`, `AddCatalogModule`, `AddInventoryModule`, `AddOrderingModule`, `AddPaymentModule`, `AddShippingModule`) into one process (`Program.cs:38-45`).
  - The `Module` project is a single assembly with one namespace per module (`Module.Catalog`, `Module.Identity`, etc.) — there is no per-module deployable (`Module.csproj:1-21`).
  - Each feature is sliced into handler/endpoint/request/response/validator files under `Features/{Admin|Storefront}/{Feature}/{Action}/` (e.g. `service/Api/src/Module/Catalog/Features/Admin/Products/Create/`).
  - Cross-module references are *forbidden by intent* (`ValidateVerticalSliceIsolation` MSBuild target, currently `Condition="false"` — warning, not error: `Directory.Build.targets:42-53`).
  - CQRS is enforced via shared `ICommand<,>`, `ICommandHandler<,>`, `IQuery<,>`, `IQueryHandler<,>` contracts in `service/Api/src/Shared/Application/Mediators/{Commands,Queries}/`.
- **Primary constraints shaping design:**
  1. **Result objects, not exceptions** — `Result`/`Result<T>` and `Error` are the only API surface; exceptions only at infra boundaries (`Result.cs:1-43`, `Result.Method.cs:1-191`).
  2. **Module isolation** — no direct cross-module references; module-to-module work is done via `ISender` dispatch (`Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:64-65`).
  3. **Warnings as errors** — any warning fails the build (`Directory.Build.props:17`).
  4. **Single backend SDK pinning** — SDK `10.0.301`, `LangVersion=preview`, `Nullable=enable`, `ImplicitUsings=enable` (`global.json:3`, `Directory.Build.props:4-9`).

### 2) System Flow

**HTTP request → MediatR pipeline → EF / external integration → mapped response** (`README.md:21-29`, `service/Api/src/Api/Program.cs:54-65`):

```text
HTTP request
  → Carter endpoint (Mapped via ICarterModule; AddEndpoints scans Module + Shared)
  → Endpoint extension returns result.ToResult() (Result<,> → IResult)
  → MediatR Send(Command/Query)
    → LoggingBehavior<,>            (outer)
    → ValidationBehavior<,>         (FluentValidation; returns failure on errors)
    → ExceptionMappingBehavior<,>   (try/catch → Result.Unexpected)
    → CommandHandler / QueryHandler (uses IApplicationDbContext, ISender, ICurrentUser, ILogger, etc.)
        → Domain logic (Entity factory methods, invariants)
        → EF Core SaveChanges / external calls (storage, notifications, payment gateway, embedding service)
        → Mapster-mapped DTO via XxxMapping
  → Result returned up the pipeline
  → HTTP response (Success = statusCode from Result; Failure = error list with status)
```

**Concrete example — `CreateProduct` (`service/Api/src/Module/Catalog/Features/Admin/Products/Create/`):**
1. `Endpoint.cs:14-32` — `app.MapPost(CatalogFeature.Admin.Products.Create.Route, ...)` → `sender.Send(new Command(request))` → `result.ToResult()`.
2. `Validator.cs:1-15` — `RuleFor(x => x.Request).ApplyProductParametersRules()` (reused validator from `Shared/`).
3. `CreateProduct.cs:36-78` — `CommandHandler.Handle` validates slug uniqueness via EF, creates `Product` via factory method (`MapToDomain()`), `Add` + `SaveChanges`, dispatches `AddVariant.Command` via `ISender`, sets `MasterVariantId`, `SaveChanges`, returns `Result<Response>.Created(...)`.
4. `Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:1-67` — runs validators before handler, converts failures to `Error.Validation`.
5. `Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:1-42` — catches any unhandled exception and returns `Error.Unexpected(code, message)`.
6. `Shared/Application/Mediators/Behaviours/Logging/Logging.Behaviours.cs` (file in scan list) — outer behavior logs request entry/exit.

**Outbound calls (e.g. embedding):** `Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs:21-36` posts JSON to `/embeddings` of the Python service (resolved via Aspire service discovery / `IHttpClientFactory` and the resilience pipeline configured in `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:32-39`).

### 3) Layer/Module Responsibilities

| Layer / module | Owns | Must not own | Evidence |
|----------------|------|--------------|----------|
| `service/Api/src/Api` (Host) | Composition root, `Program.cs`, appsettings, launch profile, DI registration of cross-cutting concerns and modules. | Business logic, entities, migrations. | `service/Api/src/Api/Program.cs:26-66` |
| `service/Api/src/Module/<Name>` | Aggregates, domain methods, persistence configurations, feature handlers, endpoints. One `*.Extension.cs` per module. | References to other `Module.<X>` namespaces. | `Module/Catalog/Catalog.Extension.cs:1-38`, `Module/Ordering/Ordering.Extension.cs` (existence), `Directory.Build.targets:42-53` |
| `service/Api/src/Shared/Application` | `Result` / `Error` / `PagedResult` types, `ICommand` / `ICommandHandler` / `IQuery` / `IQueryHandler` contracts, `IEndpointRouteBuilder` extension conventions, `IPagedQuery`, **cross-module contracts** (`Shared/Application/Contracts/{Inventory,Profile}`). | Module-specific logic. | `Shared/Application/Models/Results/Result.cs:1-43`, `Shared/Application/Mediators/Commands/ICommand.cs`, `Shared/Application/Contracts/Inventory/IStockQuantityService.cs:1-7` |
| `service/Api/src/Shared/Governance` | OpenAPI generation + Scalar UI mapping, FluentValidation auto-registration, case/dictionary/enum/base64 converters. | Runtime telemetry, auth. | `Shared/Governance/Governance.Extension.cs:1-57` |
| `service/Api/src/Shared/Observability` | OTel registration, correlation (`X-Correlation-Id`), logging pipeline, health checks, `ObservabilitySetting` (FluentValidated). | Domain code. | `Shared/Observability/Observability.Extension.cs:1-55`, `Shared/Observability/Correlation/CorrelationMiddleware.cs` |
| `service/Api/src/Shared/Operational/Persistence` | `ApplicationDbContext` (Identity + app), schema configurations, interceptors (auditable, soft-deletable, versionable), transactions, seeders, specifications (filtering/sorting/paging/searching). | Storage providers, notifications. | `Shared/Operational/Persistence/Data/AppDbContext.cs:1-60`, `Shared/Operational/Persistence/Interceptors/*.cs` |
| `service/Api/src/Shared/Operational/Storages` | `IStorageProvider` with `Local` and `S3` impls, `IImageProcessor` (SkiaSharp), `IStorageSecurityEnforcer` (magic-bytes / extension / file-size validation), `IStorageAntiForgeryGuard`, `IStorageMalwareScanner` (ClamAV + content signature). | File persistence orchestration in modules. | `Shared/Operational/Storages/Storage.Extensions.cs:1-115` |
| `service/Api/src/Shared/Operational/Notifications` | FluentEmail-based notification hub, templates (`NotificationDefinition`, enumerates for channel/format/param/priority/usecase), providers (SendGrid, Smtp, Sinch). | Direct send in handlers — handlers must call `INotificationService` (or hub). | `Shared/Operational/Notifications/Notification.Extension.cs`, `Shared/Operational/Notifications/Channels/Emails/Providers/{SendGird,Smtp}/` |
| `service/Api/src/Shared/Operational/Backgrounds` | Hangfire registration, queue & server config, dashboard in development, `BackgroundJobSetting` validated. | Job definitions. | `Shared/Operational/Backgrounds/Background.Extension.cs:35-124` |
| `service/Api/src/Shared/Operational/Http` | `HttpClient` factory config, `CorrelationIdPropagationHandler`, resilience pipeline. | Direct outbound calls. | `Shared/Operational/Http/ResilienceExtensions.cs` |
| `service/Api/src/Shared/Operational/Webhooks` | Empty directory tree (per scan) — webhook infrastructure is reserved for future use. | — | `docs/codebase/.codebase-scan.txt`; see also `git log: a91a8c15 chore(modules): delete empty Webhooks module tree` |
| `service/Api/src/Shared/Performance/Caching` | `IHybridCache` wrapper, `CachingSetting` (memory + distributed + hybrid), `CachingEntryOption` converter. | Domain cache keys. | `Shared/Performance/Caching/Wrappers/Caching.Service.Interface.cs` |
| `service/Api/src/Shared/Security/Authentication` | JWT bearer setup, `IAccessTokenService` + `IRefreshTokenService` (with theft detection + blacklist), external auth (`Google.Apis.Auth`), guest session cookie middleware, `ICurrentUser` context. | Per-module auth logic. | `Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:33-88` |
| `service/Api/src/Shared/Security/Authorization` | Permission-based `IAuthorizationPolicyProvider`, `PermissionContext` registry (domains/categories/actions descriptors), `HasPermission` extension method on endpoint conventions. | Module-specific permission metadata (those live in `Features/Shared/...Metadata.cs`). | `Shared/Security/Authorization/Policies/Permission.PolicyProvider.cs:1-31`, `Shared/Security/Authorization/Registry/PermissionContext.cs:1-60` |
| `service/Api/src/Shared/Security/{AntiForgery,Headers,RateLimiting,Cors,Identity}` | Cross-cutting: anti-forgery tokens, security response headers, named rate-limit policies (`auth` 5/min, `register` 3/hr, `forgot-password` 3/hr, `payment` 30/min, `default` 100/min), CORS allow-list, Identity store + seeders. | — | `Shared/Security/RateLimiting/RateLimit.Extensions.cs`, `Shared/Security/AntiForgery/AntiForgery.Extensions.cs`, `Shared/Security/Headers/SecurityHeadersMiddleware.cs`, `Shared/Security/Identity/Identity.Extension.cs` |
| `service/Api/src/Migrations` | EF Core migrations + model snapshot. | Source code. | `service/Api/src/Migrations/Migrations/20260712050728_FixPaymentMethodSettingsColumnType.cs:1-160` |
| `infra/Aspire/src/ReSys.AppHost` | Distributed app: PG + Redis + API + Embedding Uvicorn app + Store Vite + Admin Vite. | Any business code. | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:9-49` |
| `infra/Aspire/src/ReSys.ServiceDefaults` | Reusable Aspire defaults (OTel traces/metrics/logs, health, service discovery, resilience HttpClient). | — | `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:19-132` |
| `service/Embedding` | FastAPI app exposing `/embeddings`, `/models`, `/health`; stateless; uses Fashion-CLIP via `open-clip-torch`. | Any state about the .NET backend. | `service/Embedding/src/main.py:1-29`, `service/Embedding/src/routers/embedding_router.py` |
| `app/Admin/src/{app, shared, features}` | Vue 3 SPA; `app/` = providers/router/auth bootstrap; `shared/` = api client (axios), services, composables, locales, config; `features/<name>/` = per-domain UI. Module boundaries enforced by `eslint-plugin-boundaries`. | Direct feature-to-feature imports. | `app/Admin/eslint.config.ts:32-54` |
| `app/Store/src/{api.ts, router/, stores/, views/, __tests__/}` | Vue 3 + Nuxt UI storefront. `api.ts` likely axios singleton (file exists per scan). Pinia stores for `cart`, `catalog`. | — | `app/Store/src/router/index.ts`, `app/Store/src/stores/cart.ts` |

### 4) Reused Patterns

| Pattern | Where found | Why it exists |
|---------|-------------|---------------|
| **Vertical slice** (one feature = one folder, partial-class split into 5 files: `*.cs`, `*.Endpoint.cs`, `*.Request.cs`, `*.Response.cs`, `*.Validator.cs`) | Every `Features/{Admin|Storefront}/{Feature}/{Action}/` directory, e.g. `Module/Catalog/Features/Admin/Products/Create/`, `Module/Ordering/Features/Storefront/Cart/Checkout/` | Per `AGENTS.md:24-26`, every C# feature is a `static partial class` split across files. Keeps handler cohesive; reviewer can scan one folder for full use case. |
| **CQRS via MediatR** (commands return `ICommand<TResponse>`, queries return `IQuery<TResponse>`, paged queries implement `IPagedQuery<T>`) | `Shared/Application/Mediators/Commands/ICommand.cs`, `Queries/IQuery.cs`, `Queries/IPagedQuery.cs`; consumers in every `*.cs` file (e.g. `CreateProduct.cs:12`) | Decouples transport (Carter) from handler; allows pipeline behaviors to wrap every request. |
| **Pipeline Behaviors (Decorator)** — `LoggingBehavior` → `ValidationBehavior` → `ExceptionMappingBehavior` (outermost → innermost) | `Shared/Application/Mediators/Mediator.Extension.cs:46-50` | Cross-cutting concerns without polluting handlers. |
| **Result object (not exceptions)** — `Result`, `Result<T>`, `PagedResult<T>`, `Error` with implicit conversion from `Error` → `Result` | `Shared/Application/Models/Results/Result.cs:1-43`, `Result.Method.cs:1-191`, `Models/Errors/Error.cs` | Forces explicit failure paths; aligns with `AGENTS.md:21-23` non-negotiable rule. |
| **Module isolation (DI composition root per module)** — `<Name>.Extension.cs` is the *only* registration site for a module's services | `Module/Catalog/Catalog.Extension.cs:16-37`, `Program.cs:38-45` | Hides internals; `eslint-plugin-boundaries` analog on the frontend (`app/Admin/eslint.config.ts:32-54`) enforces similar layering. |
| **Repository / DAO via EF Core** — `IApplicationDbContext` (single, shared) is the unit-of-work; `DbSet<T>` is queried directly in handlers (e.g. `CreateProduct.cs:41-43`); interceptors add cross-cutting behavior | `Shared/Operational/Persistence/Data/AppDbContext.cs:1-60`, `Interceptors/Auditable.Interceptor.cs`, `Interceptors/SoftDeletable.Interceptor.cs`, `Interceptors/Versionable.Interceptor.cs` | Pragmatic: avoid a custom repository layer, but keep cross-cutting via EF interceptors and base entities (`Entity` from `Shared.Application.Domain.Models`). |
| **Specification DSL** — composable expressions for filtering/sorting/paging/searching | `Shared/Operational/Persistence/Specifications/{Filtering,Sorting,Paging,Searching,Querying,Helpers}/` | Lets endpoints build queries declaratively. |
| **Options pattern + FluentValidation** — every settings type has a `*Validator` and is `BindConfiguration().ValidateFluentValidation().ValidateOnStart()` | `Shared/Observability/Observability.Extension.cs:21-28`, `Shared/Operational/Backgrounds/Background.Extension.cs:41-47`, `Shared/Operational/Storages/Storage.Extensions.cs:35-74`, `Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:44-48` | Fail-fast configuration errors at boot. |
| **Multi-provider abstractions (Strategy)** for storage (`IStorageProvider` → `Local`, `S3`; `Azure` is configured but not yet implemented — see `STORAGE` settings block in `appsettings.json:163-178`), email (`FluentEmail.SendGrid` / `FluentEmail.Smtp`), SMS (`Sinch`), payment (`BogusGateway`, `StripeGateway` via `IGatewayRegistry`) | `Shared/Operational/Storages/Providers/Local.StorageProvider.Implementation.cs`, `S3.StorageProvider.Implementation.cs`; `Shared/Operational/Notifications/Channels/Emails/Providers/{SendGird,Smtp}/`; `Module/Payment/Services/Provider/{Stripe,Bogus}/`; `Module/Payment/Services/Provider/GatewayRegistry.cs` | Pluggable providers without touching call sites; priority-based selection per channel. |
| **Async eventing in dev** — `InProcessOrderEventPublisher` over `Channel<T>` (commit `42c4ef14`); wired via `LoggingNullOrderEventPublisher` (commit `ce6a3000`). | Files exist in git history at `service/Api/src/Module/Ordering/Infrastructure/Events/InProcessOrderEventPublisher.cs` and `LoggingNullOrderEventPublisher.cs`, but are deleted from the current working tree (`git status` reports them as deleted). | Lets `Ordering` raise domain events in dev without a broker. |
| **Permission-based authorization** — module declares `*FeatureMetadata` (Route/Summary/Description/Permission), endpoint uses `.HasPermission(...)`; `PermissionPolicyProvider` maps policy name → `PermissionRequirement`; `PermissionContext` enumerates allowed values | `Shared/Security/Authorization/{Registry/PermissionContext.cs, Features/*FeatureMetadata.cs}`; `Shared/Security/Authorization/Attributes/HasPermission.Attribute.Extension.cs` | Decouples permission keys from policy strings; centralized registry. |
| **HTTP error envelope** — all handlers return `Result`/`Result<T>`; the implicit `Error → Result` conversion and `ToResult()` projection ensure the OpenAPI schema can describe the union | `Result.Method.cs:65-186`, `Models/Errors/Error.cs`, `Endpoint.Extension.cs` for Carter | Client gets a uniform `{isSuccess, statusCode, errors[], message?, metadata?}` shape. |
| **HTTP test artifacts as documentation** | `ApiTests/<Module>/...` 49 `.http` files | Curated, runnable examples of every endpoint, served as a manual QA artifact. |

### 5) Known Architectural Risks

- **`ValidateVerticalSliceIsolation` is disabled** (`Directory.Build.targets:42-53`, `Condition="false"`). Cross-module references will not fail the build. Detection depends on convention only.
  - Mitigation today: review; no automated gate.
- **Empty `Webhooks/` directory tree** in `Shared/Operational/Webhooks/` (subdirs `Backgrounds/`, `Domain/`, `Persistence/`, `Services/` all empty per `find ... -type f`) — dead code or WIP? See commit `a91a8c15 chore(modules): delete empty Webhooks module tree` which deleted an *earlier* empty module tree.
  - Impact: confusing for newcomers; a `using Shared.Operational.Webhooks;` would fail at compile time. **TODO** — confirm intent (keep as placeholder vs remove).
- **YARP API gateway is intentionally deferred** (`infra/Aspire/src/ReSys.AppHost/AppHost.cs:5-7` — "YARP API gateway is deferred to v1.x. The Services.Gateway constant is defined in ReSys.ServiceDefaults but not registered as a resource here. Frontends call the API directly via VITE_API_URL."). Frontends currently call the API directly with no gateway.
  - Impact: no central place to enforce rate limits, auth, or CORS for SPA→API traffic; SPAs hard-code the API URL.
- **Azure Blob storage provider is not implemented** — `appsettings.json:163-168` declares `Storage.Providers.Azure.IsEnabled=false` and there's no `AzureStorageProvider` class in `Shared/Operational/Storages/Providers/` (only `Local` and `S3`).
  - Impact: docs say "Pluggable file storage (Local, S3-compatible, Azure Blob)" (`README.md:50`) but only two are real.
- **SendGrid provider has a typo** — `appsettings.json:206-212` configures `Notification.Channels.Email.Providers.SendGrids` (plural + extra `s`), but the code folder is `Channels/Emails/Providers/SendGird/` (one `r`, no plural `s`). Either both are right and the binding tolerates mismatch, or the email channel currently never actually dispatches via SendGrid in the default config. **TODO** — confirm via a smoke test.
- **`Webhooks/Webhook/Empty` tree**: see above.
- **Embedded `feature metadata` for permissions lives in `Shared`** (`Shared/Security/Authorization/Features/*FeatureMetadata.cs`) but is module-specific. This is a minor coupling — every module's permissions are listed in `Shared`. Acceptable trade-off because the authorization policy provider must enumerate them at startup.
- **Module layer is `Shared` (one assembly), not per-module assemblies.** `Directory.Build.targets:5-39` defines reference-validation targets for `.Domain`, `.Application`, `.Infrastructure`, `.Api`, `.Web` project name suffixes — but the actual project is named `Module` (not `.Domain` etc.), so the targets don't apply. Layer rules rely on `ValidateVerticalSliceIsolation` (also disabled) and human review.
- **Empty module gateway folders**: `Module/Payment/Infrastructure/Gateways/Stripe/` and `.../Bogus/` are empty directories; actual gateway code lives in `Module/Payment/Services/Provider/Stripe/` and `.../Bogus/`. Likely stale leftover from a prior structure.

### 6) Evidence

- `service/Api/src/Api/Program.cs:1-66` — composition root & module wiring order
- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` — MediatR + pipeline behaviors
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43`, `Result.Method.cs:1-191` — `Result` pattern
- `service/Api/src/Shared/Application/Mediators/Behaviours/{Logging,Validation,Exceptions}/*.cs` — pipeline behaviors
- `service/Api/src/Shared/Application/Endpoints/Endpoint.Extension.cs:1-63` — Carter scanning
- `service/Api/src/Shared/Security/Authorization/Policies/Permission.PolicyProvider.cs:1-31` — dynamic authz
- `service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs:1-60` — permission registry
- `service/Api/src/Shared/Operational/Persistence/Data/AppDbContext.cs:1-60`, `Interceptors/*.cs`, `Configurations/{DateTimes,Vectors}/...` — EF layer
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:1-115` — storage DI composition
- `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs:35-124` — Hangfire wiring
- `service/Api/src/Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:33-88` — JWT auth
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/{CreateProduct,CreateProduct.Endpoint,CreateProduct.Validator}.cs` — vertical slice anatomy
- `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs:1-99` — concrete CQRS handler with Identity
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs:1-97` — out-of-process integration (Python embedding service)
- `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:1-145` — external payment webhook handling
- `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs:1-30+` — background job example
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49`, `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:1-132` — Aspire
- `service/Embedding/src/main.py:1-29` — Python embedding entry
- `app/Admin/eslint.config.ts:1-57` — frontend module boundary enforcement
- `app/Admin/src/shared/api/http/api.client.ts:1-92` — axios client with token refresh + result unwrap
