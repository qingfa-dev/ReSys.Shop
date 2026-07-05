# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System | Type (API/DB/Queue/etc) | Purpose | Auth model | Criticality | Evidence |
|--------|---------------------------|---------|------------|-------------|----------|
| PostgreSQL | Database (primary) | All application data persistence | Username/password (connection string) | High | `Directory.Packages.props:42-44` |
| Redis | Cache / job store | Distributed caching, Hangfire job storage, session store | None (trusted network) | Medium | `Directory.Packages.props:60,78-79` |
| pgvector | PostgreSQL extension | Vector similarity search for image embeddings | N/A (DB extension) | Medium | `Directory.Packages.props:43-44` |
| ClamAV | TCP socket | Malware scanning on uploaded files | None (local socket) | Low | `Directory.Packages.props:92` |
| SendGrid | REST API | Transactional email delivery (email channel provider) | API key | Low | `Directory.Packages.props:81` |
| SMTP | TCP protocol | Email delivery (local dev / self-hosted) | Optional credentials | Low | `Directory.Packages.props:82` |
| Sinch | REST API | SMS notification delivery | Project ID + Key ID + Key Secret | Low | `Directory.Packages.props:83` |
| Google | OAuth 2.0 / REST | External login / Google authentication | Client ID | Low | `Directory.Packages.props:74` |
| Stripe | REST API | Payment processing | API keys (not yet wired) | Low | `Directory.Packages.props:84` |
| OpenTelemetry | OTLP protocol | Traces, metrics, logs export to OTLP collector | OTLP endpoint URL | Low | `Directory.Packages.props:26-29` |

### 2) Data Stores

| Store | Role | Access layer | Key risk | Evidence |
|-------|------|--------------|----------|----------|
| PostgreSQL | Primary application database (users, products, orders, profiles, etc.) | EF Core via `ApplicationDbContext` + Npgsql provider | Schema migration errors; connection pool exhaustion | `Persistence.Extensions.cs`, `AppDbContext.cs` |
| Redis | Distributed cache + Hangfire job storage + refresh token store | `IDistributedCache`, `StackExchange.Redis`, `HybridCache` | Data loss on restart (if not persisted); single-point-of-failure | `appsettings.json:75`, `Directory.Packages.props:60` |
| Local filesystem | File storage for product images / uploads (Local provider) | `IStorageProvider.Local` | Not scalable across instances; data loss on host failure | `appsettings.json:93,121-124` |

### 3) Secrets and Credentials Handling

- Credential sources: `.env` files (git-ignored), `appsettings.json` (defaults/empty), `appsettings.Development.json` (dev overrides), Aspire User Secrets
- Hardcoding checks: Development secrets are hardcoded in `appsettings.Development.json` (e.g., `"Secret": "ThisIsADevelopmentJwtSecretKey..."`, DB password `"postgres"`) — acceptable for local dev only
- Rotation or lifecycle notes: No automated rotation mechanism detected. Secrets are static until manually rotated.

### 4) Reliability and Failure Behavior

- Retry/backoff behavior: `Microsoft.Extensions.Http.Resilience` configured via `AddStandardResilienceHandler()` in `ServiceDefaults/Extensions.cs`. Custom `CorrelationIdPropagationHandler`. Timeout default 30s (`Http:DefaultTimeoutSeconds`).
- Timeout policy: Configured per HTTP client and per notification provider (`Notification.Channels.Email.Providers.SendGrids.Timeout: 00:00:30`)
- Circuit-breaker or fallback behavior: Standard resilience handler includes circuit breaker (from `Microsoft.Extensions.Http.Resilience`). Notification channels have priority-based fallback (e.g., SendGrid falls back to SMTP; SMS falls back to logging provider).

### 5) Observability for Integrations

- Logging around external calls: Structured `ILogger` logs throughout. Correlation IDs propagate via `X-Correlation-Id`. OpenTelemetry instruments EF Core, HTTP client, ASP.NET Core, Redis, and Npgsql.
- Metrics/tracing coverage: OpenTelemetry configured in `ServiceDefaults/Extensions.cs` — traces and metrics for ASP.NET Core, HTTP client, Redis, Npgsql, and runtime. OTLP exporter optional.
- Missing visibility gaps: Hangfire dashboard is accessible at `/jobs` (dev only). No dedicated integration health check dashboards beyond standard `/health` + `/alive`.

### 6) Evidence

- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs` — resilience + OpenTelemetry setup
- `service/Api/src/Shared/Operational/Notifications/` — notification channel providers (SendGrid, SMTP, Sinch)
- `service/Api/src/Shared/Operational/Storages/Providers/` — storage provider options (Local, S3, Azure)
- `service/Api/src/Shared/Operational/Http/ResilienceExtensions.cs` — HTTP resilience config
- `service/Api/src/Api/appsettings.json` — all integration configuration with defaults
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire resource wiring (PostgreSQL, Redis, Embedding)
