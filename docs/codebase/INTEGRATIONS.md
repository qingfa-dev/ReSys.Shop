# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System | Type | Purpose | Auth model | Criticality | Evidence |
|--------|------|---------|------------|-------------|----------|
| PostgreSQL (pgvector) | Database | Primary data store + vector similarity | Connection string (`ConnectionStrings__DefaultConnection`) | High | `Directory.Packages.props` (Npgsql 10.0.2, Pgvector 0.3.2), `appsettings.json` |
| Redis | Cache | Distributed caching + Hangfire job store + HybridCache L2 | Connection string (Aspire-managed) | Medium | `Directory.Packages.props` (StackExchangeRedis 10.0.9), Aspire `AppHost.cs` |
| Stripe | Payment API | Payment gateway (intents, captures, webhooks) | API key (`GatewayProviders__stripe__SecretKey` + `WebhookSecret`) | High | `Directory.Packages.props` (Stripe.net 52.1.0), `appsettings.json` |
| Google OAuth | Auth API | External login provider | Client ID + Secret (`Authentication__Google__ClientId`/`ClientSecret`) | Medium | `Directory.Packages.props` (Google.Apis.Auth 1.75.0), `appsettings.json` |
| SendGrid | Email API | Email notification delivery (optional) | API key (`Notification__Channels__Email__Providers__SendGrid__ApiKey`) | Low | `Directory.Packages.props` (FluentEmail.SendGrid 3.0.2), `appsettings.json` |
| SMTP | Email | Email notification delivery (primary) | Config-based: host, port, credentials | Medium | `Directory.Packages.props` (FluentEmail.Smtp 3.0.2), `appsettings.json` |
| Sinch | SMS API | SMS notification delivery (optional) | Project ID + Key ID + Key Secret | Low | `Directory.Packages.props` (Sinch 1.5.0), `appsettings.json` |
| ClamAV | Malware scan | File upload malware scanning (optional) | Network (host:port) | Low | `Directory.Packages.props` (nClam 7.0.0), `appsettings.json` |
| Facebook OAuth | Auth API | External login provider (disabled by default) | Client ID + Secret | Low | `appsettings.json` |
| Microsoft OAuth | Auth API | External login provider (disabled by default) | Client ID + Secret | Low | `appsettings.json` |
| Bogus (test gateway) | Payment simulation | Development/test payment processing | [ASK USER] | Low | `appsettings.Development.json` |
| Python Embedding Service | HTTP API | Image embedding generation (Fashion-CLIP via FastAPI) | None (internal service) | Medium | Aspire `AppHost.cs`, `Directory.Packages.props` (Aspire.Hosting.Python) |
| OpenTelemetry Collector | Observability | Distributed tracing + metrics export | OTLP endpoint (`OTEL_EXPORTER_OTLP_ENDPOINT`) | Low | `ServiceDefaults/Extensions.cs`, `Directory.Packages.props` |

### 2) Data Stores

| Store | Role | Access layer | Key risk | Evidence |
|-------|------|--------------|----------|----------|
| PostgreSQL (resys_shop database) | Primary relational store + vector embeddings | EF Core via `IApplicationDbContext` (Npgsql.EntityFrameworkCore.PostgreSQL + Pgvector.EntityFrameworkCore) | Single point of failure for all transactional data | `ConnectionStrings.DefaultConnection` in `appsettings.Development.json`, `Directory.Packages.props` |
| Redis | HybridCache L2 + Hangfire job storage + session state | `HybridCache` API (Microsoft.Extensions.Caching.Hybrid), Hangfire.Redis.StackExchange | Data loss on restart if not persisted; cache inconsistency | `appsettings.json` sections `Caching.Distributed`, `BackgroundJobs` |
| Local file storage | Uploaded files (images, documents) | `Storage.Providers.Local` (path `./uploads` or `../../../infra/Storage/demo` in dev) | Not production-ready; no replication or backup strategy | `appsettings.json` `Storage.Providers.Local` |

### 3) Secrets and Credentials Handling

- **Credential sources**:
  - Development: `dotnet user-secrets` (id: `resys.shop.api`) — bootstrapped via `service/Api/scripts/setup-dev-secrets.sh`
  - Production (expected): Environment variables (e.g., `Authentication__Jwt__Secret`, `ConnectionStrings__DefaultConnection`)
  - No `.env.example` or `.env.template` exists; `appsettings.json` serves as config template with empty placeholder values
- **Hardcoding checks**: `appsettings.Development.json` contains a hardcoded PostgreSQL connection string (`Host=localhost;Database=resys_shop;Username=postgres;Password=postgres`) — acceptable for local dev only
- **Rotation or lifecycle notes**: JWT token rotation is enabled (`TokenSecurity.RotationEnabled = true`), reuse detection enabled, max token age 30 days. No automated secret rotation mechanism detected.

### 4) Reliability and Failure Behavior

- **Retry/backoff**: Standard HTTP resilience handler via `Microsoft.Extensions.Http.Resilience` (attached to all `HttpClient` instances by `ServiceDefaults`). Email/SMS providers have configurable retry counts (default 3) with configurable timeouts.
- **Timeout policy**: HTTP clients default to 30s (`Http.DefaultTimeoutSeconds = 30` in `appsettings.json`). Payment/notification providers have individual timeouts (e.g., `00:00:30`).
- **Circuit-breaker**: Standard resilience pipeline includes circuit breaker via `AddStandardResilienceHandler()`. Payment provider retry + timeout is configurable.
- **Health checks**: `/health` (readiness — all health checks pass for traffic), `/alive` (liveness — tagged `live` checks only). Database initialization health check via `database_initialization` tag.
- **Payment idempotency**: Payment intents tracked via `PaymentIntentId` — duplicate submission detection via `PaymentCapture.ResponseCode` lookup in database.

### 5) Observability for Integrations

- **Logging around external calls**: OpenTelemetry tracing instruments all HTTP and database calls (`Instrumentation.AspNetCore`, `Instrumentation.Http`, `Instrumentation.EntityFrameworkCore`, `Instrumentation.StackExchangeRedis`, `Npgsql.OpenTelemetry`). Structured JSON logging via `python-json-logger` on Python side.
- **Metrics/tracing coverage**: ASP.NET, HTTP client, EF Core, Redis, Npgsql, and runtime metrics are instrumented. Stripe API calls are NOT explicitly instrumented (relies on generic `HttpClient` instrumentation).
- **Missing visibility gaps**:
  - Stripe SDK calls may not propagate correlation IDs (SDK-managed HTTP client)
  - Python Embedding service health check exists (`/health`) but no detailed error rate or latency metrics
  - Email/SMS delivery success/failure not instrumented beyond standard HTTP calls

### 6) Evidence

- `service/Api/src/Api/appsettings.json` — all integration configuration sections
- `service/Api/src/Api/appsettings.Development.json` — dev defaults
- `Directory.Packages.props` — all integration NuGet packages
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire PostgreSQL + Redis + Embedding orchestration
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs` — default resilience + OpenTelemetry setup
- `service/Api/src/Shared/Operational/` — background jobs, notifications, file storage infrastructure
- `service/Api/src/Shared/Performance/Caching/` — HybridCache configuration
