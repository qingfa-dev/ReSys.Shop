# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System | Type | Purpose | Auth model | Criticality | Evidence |
|--------|------|---------|------------|-------------|----------|
| **PostgreSQL 17 + pgvector** | Database (relational + vector) | Primary store for Identity + 8 business modules; vector column for image embeddings (`pgvector`). Image used: `pgvector/pgvector:pg17-trixie` (optimized). | N/A (TCP) | High | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:11-12`, `Directory.Packages.props:42-44` |
| **Redis 7** | Cache + Hangfire job store | L2 cache for HybridCache and Hangfire job persistence; connection resolved from `ConnectionStrings:cache` (Aspire) or `ConnectionStrings:DefaultConnection` (fallback). | N/A (TCP) | High | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:14-15`, `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs:139-154`, `appsettings.json:104-122` |
| **Python Embedding (Uvicorn / FastAPI)** | Internal HTTP API (port 8000) | Image embedding generation (Fashion-CLIP). Endpoints: `GET /health`, `POST /embeddings`, model routes. | None inside the cluster (relies on Aspire service-to-service / internal network) | High | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:20-27`, `service/Embedding/src/main.py:1-29`, `Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs:21-36` |
| **Stripe** | External payment gateway | Card payments, webhooks. Configured via `GatewayProviders.stripe.{Enabled, SecretKey, WebhookSecret, PublishableKey}`. | API key + webhook signing secret (Stripe-Signature) | High (when enabled) | `appsettings.json:6-19`, `appsettings.Development.json:6-19`, `Module/Payment/Services/Provider/Stripe/StripeGateway.cs` (existence), `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:32-36` |
| **Bogus gateway** | In-process payment gateway | Dev/test stand-in for Stripe. `Enabled=true` in `appsettings.Development.json:15-19`. | None | Medium (dev only) | `appsettings.Development.json:15-19`, `Module/Payment/Services/Provider/Bogus/BogusGateway.cs` (existence), `appsettings.json:13-19` |
| **Google OAuth** | External IdP | External login via `Google.Apis.Auth`. Configured via `Authentication:Google:ClientId`. | OAuth 2.0 (Google) | Medium | `Directory.Packages.props:74`, `appsettings.json:45-47`, `Shared/Security/Authentication/External/ExternalLogin.Extensions.cs` (existence) |
| **Facebook OAuth** | External IdP (disabled by default) | External login. `Enabled=false`, requires `ClientId` + `ClientSecret`. | OAuth 2.0 | Low (not configured) | `appsettings.json:48-52` |
| **Microsoft OAuth** | External IdP (disabled by default) | External login. `Enabled=false`. | OAuth 2.0 | Low (not configured) | `appsettings.json:53-57` |
| **SendGrid (email)** | External email API | Transactional email. Configured via `Notification.Channels.Email.Providers.SendGrids.*` (`Priority`, `RetryCount`, `Timeout`, `ApiKey`). | API key | Medium | `Directory.Packages.props:81`, `appsettings.json:206-212` |
| **SMTP (email)** | Generic SMTP relay | Transactional email. `Host=localhost`, `Port=1025` in dev (Papercut SMTP via Aspire community toolkit). | None / default creds in dev | Medium | `appsettings.Development.json:67-75`, `Directory.Packages.props:82` |
| **Sinch (SMS)** | External SMS API | SMS via `Sinch` SDK. `Enabled=false` in default config. | ProjectId + KeyId + KeySecret | Low (not enabled) | `Directory.Packages.props:83`, `appsettings.json:225-239` |
| **ClamAV (`nClam`)** | Malware scanner (TCP `localhost:3310`) | Upload scanning. `Enabled=false` by default, `DisableInDevelopment=true`. | None (local socket) | Medium | `Directory.Packages.props:92`, `appsettings.json:150-155`, `appsettings.Development.json:52-55` |
| **Local storage** | File system provider | Default `Storage.DefaultProvider=Local`, root `./uploads`. | N/A | High (default) | `appsettings.json:129-178`, `appsettings.Development.json:46-51`, `Shared/Operational/Storages/Providers/Local.StorageProvider.Implementation.cs` |
| **S3-compatible storage** | Object storage | Configurable via `Storage.Providers.S3.{ServiceUrl, AccessKey, SecretKey, BucketName, Region, ForcePathStyle}`. `IsEnabled=false` by default. | AccessKey + SecretKey | Medium | `Directory.Packages.props`-`appsettings.json:168-178`, `Shared/Operational/Storages/Providers/S3.StorageProvider.Implementation.cs` |
| **Azure Blob storage** | Object storage (NOT IMPLEMENTED) | Reserved config block (`Storage.Providers.Azure.*`). No `AzureStorageProvider` class in `Shared/Operational/Storages/Providers/`. | Connection string | **TODO — not wired** | `appsettings.json:163-168`, `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:80-82` (only Local + S3 registered) |
| **OTLP collector** | Telemetry sink (optional) | If `OTEL_EXPORTER_OTLP_ENDPOINT` is set, OpenTelemetry exports traces/metrics/logs via OTLP. | N/A | Low (opt-in) | `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:86-103`, `service/Api/src/Api/.env.template:30` |
| **Hangfire dashboard** | Internal UI (`/jobs`) | Job monitor in development only. | N/A | Low (dev) | `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs:117-122` |
| **Aspire service discovery** | Internal service-to-service | HTTP client `AddServiceDiscovery()` enables scheme-based discovery. | N/A | High (internal) | `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:30-39` |
| **Papercut SMTP (Aspire community)** | Dev SMTP server | Local dev email receiver when running under Aspire. | N/A | Low (dev) | `Directory.Packages.props:25` |
| **Scalar** | API reference UI | Interactive OpenAPI explorer at `/scalar/v1`. | N/A | Low | `service/Api/src/Shared/Governance/OpenApi/OpenApi.Extension.cs:54-62`, `Directory.Packages.props:53` |
| **pgvector (benchmarks)** | Vector DB (pgvector extension) | Standalone PostgreSQL for benchmark retrieval tests. Benchmarks own their own connection (`postgresql://benchmark:benchmark@localhost:5432/benchmark`) via `--conn-string` CLI flag. | N/A (TCP) | Medium | `benchmarks/src/benchmark/retrieval/pgvector.py:1-232`, `benchmarks/src/benchmark/cli/benchmark.py:301-303` |
| **FAISS (benchmarks — in-process)** | In-process vector index | IVFFlat approximate nearest-neighbor retrieval for benchmarks. | N/A | Medium | `benchmarks/pyproject.toml:17`, `benchmarks/src/benchmark/retrieval/faiss.py` |
| **HuggingFace (benchmarks)** | External model hub | Download pretrained models (CLIP, SigLIP, EVA-CLIP, EfficientNet, ConvNeXt, ResNet) via `transformers` and `open-clip-torch`. | N/A (public) | Medium | `benchmarks/pyproject.toml:10-11`, `benchmarks/src/benchmark/models/*.py` |

### 2) Data Stores

| Store | Role | Access layer | Key risk | Evidence |
|-------|------|--------------|----------|----------|
| **PostgreSQL (pgvector)** | Primary OLTP + vector store. EF Core migrations in `Migrations/Migrations/` (`InitialCreate`, `FixPaymentMethodSettingsColumnType`). Schemas: `catalog`, `ordering`, `payment`, `inventory`, etc. (per `Migration` SQL). | `ApplicationDbContext : IdentityDbContext<...>` (single shared DbContext), per-module configurations, EF interceptors (auditable, soft-deletable, versionable). | Migrations are large (`InitialCreate.cs` ~104 KB, Designer files ~150 KB); one column-type fix in `FixPaymentMethodSettingsColumnType.cs:49-56` (changed `payment.payment_method.settings` from `jsonb` to `text`). | `Shared/Operational/Persistence/Data/AppDbContext.cs:1-60`, `service/Api/src/Migrations/Migrations/` (4 files), `appsettings.json:27-29` |
| **Redis 7** | (a) HybridCache L2 (when `Caching.Distributed.Enabled=true` & `Type=redis`); (b) Hangfire storage (when `BackgroundJobs.CachingEnabled=true`); (c) Aspire-resolved `ConnectionStrings:cache` is also preferred. | `Microsoft.Extensions.Caching.StackExchangeRedis` (L2), `Hangfire.Redis.StackExchange`. | `Caching:Distributed:Enabled=true` is on by default (`appsettings.json:113`) but `BackgroundJobs.CachingEnabled=false` in dev (`appsettings.Development.json:79`) — Hangfire uses in-memory. | `appsettings.json:104-122`, `appsettings.Development.json:78-80`, `Directory.Packages.props:60,79` |
| **In-process caches** | L1 in-memory cache + per-request scoped caches. | `Microsoft.Extensions.Caching.Memory` via HybridCache. | Disabled in tests (`ApiFactory.cs:43-46` flips all four caching toggles to `false`). | `Directory.Build.props:108-110`, `appsettings.json:104-112`, `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:43-46` |
| **File system (local storage)** | Default file store under `./uploads/`. | `LocalStorageProvider` (`Shared/Operational/Storages/Providers/Local.StorageProvider.Implementation.cs`). | Magic-byte + extension allowlist enforced; max 10 MB default; if path is wrong the file write fails silently at runtime. | `appsettings.json:129-178`, `Shared/Operational/Storages/Storage.Extensions.cs:80` |
| **S3 / Azure Blob (object storage)** | Pluggable alternative to local. Only S3 implemented; Azure is configured but unimplemented. | `S3StorageProvider`; `IStorageService` resolves from `Storage.DefaultProvider`. | Azure will fail at runtime if enabled; S3 has no retry/backoff visible at this layer. | `appsettings.json:163-178`, `Shared/Operational/Storages/Storage.Extensions.cs:79-108` |
| **Hangfire storage** | In-memory by default; Redis optional. | `Hangfire.InMemory` or `Hangfire.Redis.StackExchange`. | Loss of scheduled jobs on restart when in-memory. | `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs:54-80` |
| **EF interceptors (not a store)** | Auditable, SoftDeletable, Versionable — write-time cross-cutting. | `Shared/Operational/Persistence/Interceptors/*.cs`. | No concurrency token is exposed for in-memory tests (`AdditionalConfigurationsAssemblies` is set in test base). | `Shared/Operational/Persistence/Interceptors/Auditable.Interceptor.cs` etc. |
| **Dev user-secrets** | `dotnet user-secrets` (id `resys.shop.api`) for `Authentication:Jwt:Secret` and `GatewayProviders:SettingsEncryptionKey`. | `Api.csproj:7` (`<UserSecretsId>resys.shop.api</UserSecretsId>`); `appsettings.Development.json:2` instructs setup. | A `setup-dev-secrets.sh` script exists (`service/Api/scripts/setup-dev-secrets.sh`); CI must inject the secret. | `service/Api/src/Api/Api.csproj:7`, `service/Api/src/Api/appsettings.Development.json:1-2`, `service/Api/scripts/setup-dev-secrets.sh` |

### 3) Secrets and Credentials Handling

- **Credential sources:**
  - **`dotnet user-secrets`** (id `resys.shop.api`) for dev-only secrets — `service/Api/src/Api/Api.csproj:7`, bootstrapped by `service/Api/scripts/setup-dev-secrets.sh`.
  - **`appsettings.{Environment}.json`** for non-secret config (DB host, payment toggles, host names, port numbers) — `appsettings.json`, `appsettings.Development.json`.
  - **Environment variables** documented in `service/Api/src/Api/.env.template:1-33` (use `__` for nested keys, e.g. `Authentication__Jwt__Secret`).
  - **Aspire connection-string injection** — `AppHost.cs:30` calls `.WithReference(database)` and `.WithReference(redis)` so the API receives the resolved connection string via Aspire.
- **Hardcoding checks:**
  - `appsettings.json` and `appsettings.Development.json` declare *empty* values for all secrets (`Authentication.Jwt.Secret=""`, `GatewayProviders.SettingsEncryptionKey=""`, all `SecretKey=""`, `WebhookSecret=""`, `PublishableKey=""`, `ClientSecret=""`, `EncryptionKey=""`, `ApiKey=""`) — they are templates, not real values.
  - `.env.template:8` is `REPLACE_ME_WITH_LONG_RANDOM_SECRET`.
  - **One historic risk** was a hardcoded dev JWT secret in `appsettings.Development.json` — commit `770b6a06` ("feat(security): reject dev JWT secret in non-Development environments") and `37170ef7` ("chore(security): move dev secrets to user-secrets, document setup") indicate the dev secret has been moved to user-secrets, and `JwtSettingsValidator` refuses a dev literal in non-Development environments (referenced in `Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:38-43`).
- **Test-time secret handling:** Integration tests inject values via `WebApplicationFactory.ConfigureAppConfiguration` (`service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:32-85`); the secret values are committed in test source (`integration-test-secret-key-32-chars!!`) because the test is self-contained.
- **Rotation / lifecycle notes:** No automated rotation; settings are loaded once at boot via `BindConfiguration().ValidateOnStart()`. JWT refresh tokens support rotation + reuse detection (`appsettings.json:38-43`).

### 4) Reliability and Failure Behavior

- **Retry / backoff:**
  - **HttpClient:** Standard resilience pipeline via `AddStandardResilienceHandler()` — `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:32-39`. Applies to all `HttpClient` instances by default (`AttachResiliencePipelineByDefault=true` in `appsettings.json:125`).
  - **Notification providers:** Per-provider `RetryCount` (default `3`) and `Timeout` (default `00:00:30`) — `appsettings.json:208-239`.
  - **Embedding inference client:** Catches `OperationCanceledException` → `ImageEmbeddingResult.Errors.RequestTimeout`, generic `Exception` → `CommunicationFailed` — `ImageEmbedding.Inference.cs:21-36`.
- **Timeout policy:** `Http.DefaultTimeoutSeconds = 30` (`appsettings.json:124`); per-channel timeouts in notifications; embedding sidecar uses HttpClient default + Aspire resilience.
- **Circuit-breaker / fallback:** Polly resilience pipeline is on by default (`Extensions.cs:35`); notification providers fall back by `Priority` ordering (`appsettings.json:208,215`). No explicit per-integration circuit-breakers beyond Polly's default pipeline.
- **Database initialization:** `DatabaseInitializerHostedService` runs migrations on startup when `DatabaseInitialization.RunMigrations=true` (dev only — `appsettings.Development.json:3-5`); production expects migrations run out-of-band.
- **Stripe webhook handling:** Signature validation first, then parse, then dispatch (`Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:32-36`); invalid signature → `StripeWebhookResult.Errors.InvalidSignature`.
- **Health checks:** Default `self` liveness check + Postgres + Redis + `database_initialization` (recently added — commit `e6891d7e` "refactor(host): move database initialization to hosted service with health check" + `da94985d` "feat(shared): add IDatabaseInitializationState and health check"). `MapDefaultEndpoints` exposes `/health` and `/alive` only in non-production (`Extensions.cs:114-131`).

### 5) Observability for Integrations

- **Logging around external calls:** OTel logs include formatted message and scopes (`Extensions.cs:51-56`); correlation id (`X-Correlation-Id`) is propagated on outbound HTTP via `CorrelationIdPropagationHandler`. `appsettings.json:101` redacts `Authorization`, `Cookie`, `X-Api-Key` from traces.
- **Metrics / tracing coverage:**
  - AspNetCore + HttpClient + Runtime + Redis + Npgsql instrumentation registered (`Extensions.cs:58-79`).
  - Postgres + Redis auto-instrumentation (Npgsql.OpenTelemetry, OpenTelemetry.Instrumentation.StackExchangeRedis).
  - EF Core instrumentation is `1.16.0-beta.1` (pre-release).
  - Health endpoints are filtered out of tracing (`Extensions.cs:68-73`).
- **OTLP export:** opt-in via `OTEL_EXPORTER_OTLP_ENDPOINT` (`Extensions.cs:86-93`).
- **Missing visibility:**
  - No `Logging:LogLevel` overrides for the embedding sidecar.
  - No `Hangfire` activity source registered — Hangfire job execution is not currently traced.
  - Stripe webhook failures (signature, payload) are logged at handler level but no `ActivitySource` is wired to them (verified from `StripeWebhook.cs:32-38`).

### 6) Evidence

- `Directory.Packages.props:36-93` — production integrations (EF, Npgsql, Pgvector, Redis, Hangfire, Stripe, FluentEmail, Sinch, SkiaSharp, nClam, Aspire, OpenTelemetry, Google.Apis.Auth)
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49` — orchestrated resources
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:1-132` — OTel, health, resilience, service discovery
- `service/Api/src/Api/Program.cs:1-66` — composition root referencing all integrations
- `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs:1-155` — Hangfire (Redis vs in-memory)
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:1-115` — storage providers + security
- `service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs` + `Channels/{Emails,Sms}/Providers/` — multi-channel notifications
- `service/Api/src/Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:33-88` — JWT
- `service/Api/src/Shared/Security/Authentication/External/ExternalLogin.Extensions.cs` — external IdPs
- `service/Api/src/Shared/Operational/Http/CorrelationIdPropagationHandler.cs`, `ResilienceExtensions.cs` — outbound HTTP
- `service/Api/src/Shared/Operational/Persistence/Data/AppDbContext.cs:1-113` — DbContext
- `service/Api/src/Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs:1-30+` — pgvector convention
- `service/Api/src/Migrations/Migrations/20260712050728_FixPaymentMethodSettingsColumnType.cs:1-160` — sample migration
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs:1-97` — embedding service client
- `service/Api/src/Module/Payment/Services/Provider/{Stripe,Bogus}/` — payment gateways
- `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:1-145` — Stripe webhook endpoint
- `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs` — Hangfire job example
- `service/Api/src/Api/appsettings.json:1-243` — runtime configuration
- `service/Api/src/Api/appsettings.Development.json:1-84` — dev overrides
- `service/Api/src/Api/.env.template:1-33` — env-var documentation
- `service/Api/scripts/setup-dev-secrets.sh` — dev secret bootstrapper
- `service/Api/src/Shared/Observability/Correlation/CorrelationMiddleware.cs` — correlation propagation
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189` — integration test wiring
- `benchmarks/pyproject.toml:1-64` — benchmark dependencies (torch, faiss, pgvector, open-clip, transformers)
- `benchmarks/src/benchmark/retrieval/pgvector.py:1-232` — pgvector retrieval client
- `benchmarks/src/benchmark/retrieval/faiss.py` — FAISS retrieval client
- `benchmarks/src/benchmark/cli/benchmark.py:301-303` — pgvector connection string config
