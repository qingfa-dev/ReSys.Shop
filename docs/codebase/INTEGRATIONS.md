Short summary

External systems and integrations used or expected by the repo.

Databases & caches
- PostgreSQL (pgvector mentioned) — primary persistence.
- Redis — caching and session-like storage.

Messaging / background
- Hangfire used for background jobs (referenced in AGENTS.md).

Third-party services
- Email: SendGrid/SMTP mentioned as notification options in AGENTS.md.
- Storage: Local/S3/Azure storage abstraction noted in AGENTS.md.

# Evidence
- [AGENTS.md](AGENTS.md)
- [service/Api/src/Shared](service/Api/src/Shared)

[Decision]
- Secrets management (team): Adopt local development secrets via environment files and Aspire user secrets as the primary development approach. Action: remove hardcoded secrets from committed `appsettings.Development.json`, add a `.env.template` / `.env.example` documenting required env vars, and prefer Aspire user secrets for per-developer credentials.

[Done]
- `.env.template` created at `service/Api/src/Api/.env.template` containing the canonical development environment variables discovered in `appsettings.Development.json`, SPA env files, and notification/storage configuration.

[TODO]
- Review `.env.template` values and sanitize any remaining hardcoded development secrets from `appsettings.Development.json` (e.g., remove dev JWT secret). Consider moving to Aspire user secrets for developers.
# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System | Type (API/DB/Queue/etc) | Purpose | Auth model | Criticality | Evidence |
|--------|---------------------------|---------|------------|-------------|----------|
| PostgreSQL 17 + pgvector | Database | Primary data store; relational data + vector similarity search | Connection string | High | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:8` |
| Redis 7 | Cache / Queue | HybridCache backend; Hangfire job storage (optional) | Connection string | High | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:12` |
| Embedding Service (Python) | Internal API | Image vector embedding generation (Fashion-CLIP) | [TODO] — auth between services not defined | Medium | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:18` |
| SendGrid | External API (email) | Transactional email delivery | API key | Medium | `Directory.Packages.props:81` |
| SMTP | External (email) | Email fallback provider | Credentials / None | Medium | `Directory.Packages.props:82` |
| Sinch | External API (SMS) | SMS notifications | API key | Low | `Directory.Packages.props:83` |
| Google OAuth | External API (auth) | Social login (Google sign-in) | Client ID/Secret | Medium | `Directory.Packages.props:74` |
| Stripe | External API (payments) | Payment processing | API key | Medium | `Directory.Packages.props:84` |
| ClamAV (nClam) | External (malware scan) | File upload malware scanning | None (socket/network) | Medium | `Directory.Packages.props:92` |
| OpenTelemetry Collector | External (observability) | Traces, metrics, logs export | OTLP endpoint | Low | `Directory.Packages.props:26` |
| S3-compatible Storage | External API (storage) | Cloud file storage | Access key / Secret | [TODO] | `Shared/Operational/Storages/Providers/` |
| Azure Blob Storage | External API (storage) | Cloud file storage | Connection string / Managed identity | [TODO] | `Shared/Operational/Storages/Providers/` |
| Local Filesystem | Internal (storage) | Development file storage | None (filesystem path) | High (dev) | `service/Api/src/Api/appsettings.Development.json:15-18` |

### 2) Data Stores

| Store | Role | Access layer | Key risk | Evidence |
|-------|------|--------------|----------|----------|
| PostgreSQL | Primary relational database, vector store (pgvector), ASP.NET Identity tables, Hangfire job storage | EF Core via Npgsql (with pgvector EF provider) | Schema migration failures can block deployments; single database for all modules (no isolation) | `Shared/Operational/Persistence/`, `Migrations/Migrations/` |
| Redis | Distributed cache, session state, Hangfire job queue backend | `IDistributedCache` / `HybridCache`, `Hangfire.Redis.StackExchange` | Redis unavailability affects caching, job processing, and potentially sessions | `Shared/Performance/Caching/`, `Shared/Operational/Backgrounds/` |
| Local Filesystem | Uploaded file storage (images, assets); dev-only | `IStorageProvider` with `LocalStorageProvider` implementation | Filesystem path traversal; file permissions; not scalable for production | `Shared/Operational/Storages/Providers/` |
| S3-compatible (planned) | Production file storage | `IStorageProvider` with S3 provider implementation | Credential management; bucket permissions | `Shared/Operational/Storages/Providers/` |
| Azure Blob (planned) | Production file storage | `IStorageProvider` with Azure Blob provider implementation | Credential management; container permissions | `Shared/Operational/Storages/Providers/` |

### 3) Secrets and Credentials Handling

- Credential sources:
  - Development: `appsettings.Development.json` (committed — contains hardcoded JWT secret and DB connection string)
  - Aspire: User Secrets (`UserSecretsId` in `infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj:8`)
  - Frontend: `.env.development` files (`VITE_API_URL`)
  - Standard .NET configuration pipeline: environment variables, `appsettings.{Environment}.json`
- Hardcoding checks:
  - **Dev JWT secret is hardcoded** in `service/Api/src/Api/appsettings.Development.json:11` (`ThisIsADevelopmentJwtSecretKeyThatIsLongEnough32!`)
  - DB connection string is hardcoded in dev settings (`Host=localhost;Database=resys_shop;Username=postgres;Password=postgres`)
  - No `.env.example` or `.env.template` exists for backend — `service/Api/src/Api/` has no env files
- Rotation or lifecycle notes: No automated rotation mechanism detected; JWT secret rotation would require manual config change and token invalidation

### 4) Reliability and Failure Behavior

- Retry/backoff behavior:
  - HTTP resilience: `Microsoft.Extensions.Http.Resilience` configured via `Shared/Operational/Http/ResilienceExtensions.cs` — StandardResilienceHandler with defaults
  - Service discovery: `Microsoft.Extensions.ServiceDiscovery` via Aspire — automatic endpoint resolution
  - No custom retry logic found for database, cache, or storage operations
- Timeout policy:
  - HTTP client resilience pipeline in ServiceDefaults includes default timeouts
  - [TODO] — explicit timeout configuration for database connections, cache operations, or external API calls not found in code
- Circuit-breaker or fallback behavior:
  - HTTP resilience pipeline includes circuit breaker (via Polly, configured in ServiceDefaults)
  - Storage provider fallback (S3 > Azure > Local) not detected in code
  - Email provider fallback chain: SendGrid primary, SMTP fallback — `Shared/Operational/Notifications/`
  - No circuit breaker found for database or cache calls

### 5) Observability for Integrations

- Logging around external calls: Yes — OpenTelemetry instrumentation includes ASP.NET Core, HTTP client, Redis, and Npgsql traces (`ReSys.ServiceDefaults/Extensions.cs:68-86`)
- Metrics/tracing coverage:
  - ASP.NET Core request metrics and traces
  - HTTP client outbound tracing
  - Redis instrumentation (traces for cache operations)
  - Npgsql/PostgreSQL tracing (database query traces)
  - Runtime metrics (GC, CPU, memory)
- Missing visibility gaps:
  - No custom spans for storage operations (file upload/download timing)
  - No Hangfire job execution tracing
  - No custom business metrics (orders placed, products searched)
  - No log aggregation pipeline defined (OTLP endpoint is optional, no fallback)

### 6) Evidence

- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Service topology and wiring
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs` — OpenTelemetry, health checks, resilience configuration
- `infra/Aspire/src/ReSys.ServiceDefaults/Constants/` — Named references for services and infrastructure images
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs` — Storage provider registration
- `service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs` — Notification provider registration
- `service/Api/src/Shared/Operational/Http/ResilienceExtensions.cs` — HTTP resilience pipeline
- `service/Api/src/Shared/Security/Authentication/Authentication.Extension.cs` — JWT and OAuth configuration
- `service/Api/src/Api/appsettings.Development.json` — Dev configuration (DB, JWT, storage, notifications, CORS)
- `Directory.Packages.props` — All integration package references
