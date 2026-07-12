# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

| Area | Value | Evidence |
|------|-------|----------|
| Primary backend language | C# (LangVersion `preview`, .NET 10) | `Directory.Build.props:7`, `global.json:3` |
| Backend runtime + SDK | .NET 10 (`net10.0`), SDK `10.0.301` (rollForward `latestPatch`) | `global.json:3`, `Directory.Build.props:4` |
| Package manager (backend) | NuGet with Central Package Management (`ManagePackageVersionsCentrally=true`, transitive pinning on) | `Directory.Packages.props:3-5` |
| Backend test runner | Microsoft.Testing.Platform (configured via `global.json` `test.runner`) | `global.json:6-8` |
| Module/build system | MSBuild via solution `ReSys.Shop.slnx` (SDK-style projects) | `ReSys.Shop.slnx:207-228` |
| Frontend (Admin) | Vue 3.5 + TypeScript 6 + Vite 8 + pnpm | `app/Admin/package.json:1-71` |
| Frontend (Store) | Vue 3.5 + TypeScript 6 + Vite 8 + pnpm | `app/Store/package.json:1-56` |
| ML sidecar | Python 3.14+ with `uv` (FastAPI + open-clip-torch) | `service/Embedding/pyproject.toml:9-18` |
| Orchestration | .NET Aspire 13.4 (AppHost + ServiceDefaults) | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49`, `Directory.Packages.props:11` |
| Database | PostgreSQL 17 with pgvector (image used: `pgvector/pgvector:pg17-trixie` optimized image) | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:11-12` |
| Cache / Job store | Redis 7 (HybridCache, Hangfire) | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:14-15`, `Directory.Packages.props:60-79` |

### 2) Production Frameworks and Dependencies

#### 2.1 Backend (Shared / Module / Api) — high-impact

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| Carter | 10.0.0 | Minimal API endpoint module discovery (`ICarterModule`) | `Directory.Packages.props:47` |
| MediatR | 14.1.0 | CQRS command/query dispatch + pipeline behaviors | `Directory.Packages.props:52`, `Shared/Application/Mediators/Mediator.Extension.cs:35-50` |
| FluentValidation | 12.1.1 | Request validation pipeline + Options validation | `Directory.Packages.props:48-49`, `Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs` |
| Mapster (+ DI) | 10.0.9 | DTO ↔ Domain mapping | `Directory.Packages.props:50-51` |
| Microsoft.EntityFrameworkCore | 10.0.9 | ORM | `Directory.Packages.props:36-40` |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.2 | PG provider | `Directory.Packages.props:42` |
| EFCore.NamingConventions | 10.0.1 | Snake_case column naming | `Directory.Packages.props:41` |
| Pgvector + Pgvector.EntityFrameworkCore | 0.3.2 / 0.3.0 | Vector column for image embeddings | `Directory.Packages.props:43-44` |
| Microsoft.Extensions.Caching.Hybrid (HybridCache) | 10.6.0 | L1+L2 cache abstraction | `Directory.Packages.props:59` |
| Microsoft.Extensions.Caching.StackExchangeRedis | 10.0.9 | L2 Redis cache | `Directory.Packages.props:60` |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.9 | Identity store over EF Core | `Directory.Packages.props:71` |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.9 | JWT bearer authentication | `Directory.Packages.props:69` |
| System.IdentityModel.Tokens.Jwt | 8.19.1 | JWT token generation/validation | `Directory.Packages.props:72` |
| starkbank-ecdsa | 1.3.3 | ECDSA token signing support | `Directory.Packages.props:73` |
| Google.Apis.Auth | 1.75.0 | Google OAuth external login | `Directory.Packages.props:74` |
| Hangfire.AspNetCore | 1.8.23 | Background jobs host | `Directory.Packages.props:77` |
| Hangfire.Redis.StackExchange | 1.12.0 | Distributed job storage | `Directory.Packages.props:79` |
| Hangfire.InMemory | 1.0.0 | Dev / in-memory job storage | `Directory.Packages.props:78` |
| FluentEmail.Core / SendGrid / Smtp | 3.0.2 | Multi-provider email abstraction | `Directory.Packages.props:80-82` |
| Sinch | 1.5.0 | SMS provider | `Directory.Packages.props:83` |
| Stripe.net | 52.1.0 | Stripe payment gateway client | `Directory.Packages.props:84` |
| Microsoft.AspNetCore.OpenApi | 10.0.9 | OpenAPI generation (built-in) | `Directory.Packages.props:55` |
| Scalar.AspNetCore | 2.16.6 | Interactive API reference UI | `Directory.Packages.props:53` |
| Slugify.Core | 5.1.1 | URL slug generation | `Directory.Packages.props:54` |
| SkiaSharp + Linux native | 3.116.1 | Image processing | `Directory.Packages.props:88-89` |
| nClam | 7.0.0 | ClamAV malware scanning client | `Directory.Packages.props:92` |
| AspNetCore.HealthChecks.Npgsql / Redis / UI | 9.0.0 | Liveness/readiness checks | `Directory.Packages.props:95-97` |
| Microsoft.Extensions.Http.Resilience | 10.6.0 | HttpClient resilience pipelines | `Directory.Packages.props:62` |
| Microsoft.Extensions.Resilience | 10.6.0 | Polly resilience for app code | `Directory.Packages.props:63` |
| Microsoft.Extensions.ServiceDiscovery (+Yarp) | 10.6.0 | Aspire service discovery | `Directory.Packages.props:64-65` |
| Yarp.ReverseProxy | 2.3.0 | Reverse proxy (services gate in AppHost comments) | `Directory.Packages.props:66` |
| MessagePack | 3.1.7 | Serialization for Hangfire / internal transport | `Directory.Packages.props:85` |

#### 2.2 Observability (Aspire + OpenTelemetry)

| Dependency | Version | Evidence |
|------------|---------|----------|
| Aspire.Hosting.JavaScript / NodeJs / PostgreSQL / Python / Redis | 13.4.6 | `Directory.Packages.props:17-22` |
| Aspire.Hosting.Testing | 13.4.6 | `Directory.Packages.props:22` |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 13.4.6 | `Directory.Packages.props:23` |
| CommunityToolkit.Aspire.Hosting.PostgreSQL.Extensions | 13.4.6 | `Directory.Packages.props:24` |
| CommunityToolkit.Aspire.Hosting.PapercutSmtp | 13.4.6 | `Directory.Packages.props:25` |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.16.0 | `Directory.Packages.props:26` |
| OpenTelemetry.Instrumentation.AspNetCore | 1.16.0 | `Directory.Packages.props:28` |
| OpenTelemetry.Instrumentation.Http | 1.16.0 | `Directory.Packages.props:30` |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.16.0-beta.1 | `Directory.Packages.props:29` |
| OpenTelemetry.Instrumentation.Runtime | 1.15.1 | `Directory.Packages.props:31` |
| OpenTelemetry.Instrumentation.StackExchangeRedis | 1.16.0-beta.1 | `Directory.Packages.props:32` |
| Npgsql.OpenTelemetry | 10.0.2 | `Directory.Packages.props:33` |

#### 2.3 Frontend — Admin (Vue 3 + PrimeVue) — production deps

| Dependency | Version | Role | Evidence |
|------------|---------|------|----------|
| vue | ^3.5.38 | UI framework | `app/Admin/package.json:31` |
| vue-router | ^5.1.0 | SPA router | `app/Admin/package.json:32` |
| pinia | ^3.0.4 | State store | `app/Admin/package.json:26` |
| @primevue/core + primevue | ^4.5.5 | UI components | `app/Admin/package.json:20,28` |
| @primevue/forms | ^4.5.5 | Form helpers | `app/Admin/package.json:21` |
| @primeuix/themes | ^2.0.3 | Aura theme preset | `app/Admin/package.json:19` |
| primeicons | ^7.0.0 | Icon font | `app/Admin/package.json:27` |
| vee-validate + @vee-validate/zod | ^4.15.1 | Form validation | `app/Admin/package.json:30,22` |
| zod | ^3.25.75 | Schema validation | `app/Admin/package.json:33` |
| chart.js | ^4.5.1 | Charts | `app/Admin/package.json:24` |
| axios | ^1.18.1 | HTTP client | `app/Admin/package.json:23` |
| jwt-decode | ^4.0.0 | JWT inspection | `app/Admin/package.json:25` |
| tailwindcss-primeui | ^0.6.1 | PrimeVue/Tailwind integration | `app/Admin/package.json:29` |

#### 2.4 Frontend — Store (Vue 3 + Nuxt UI) — production deps

| Dependency | Version | Role | Evidence |
|------------|---------|------|----------|
| vue | ^3.5.38 | UI framework | `app/Store/package.json:23` |
| vue-router | ^5.1.0 | SPA router | `app/Store/package.json:24` |
| pinia | ^3.0.4 | State store | `app/Store/package.json:21` |
| @nuxt/ui | ^4.0.0 | Component library | `app/Store/package.json:20` |
| @iconify/vue | ^4.3.0 | Icons | `app/Store/package.json:19` |
| tailwindcss | ^4.3.2 | Utility CSS | `app/Store/package.json:22` |

#### 2.5 ML sidecar — Python

| Dependency | Version | Role | Evidence |
|------------|---------|------|----------|
| fastapi | >=0.115 | HTTP framework | `service/Embedding/pyproject.toml:11` |
| uvicorn[standard] | >=0.34 | ASGI server | `service/Embedding/pyproject.toml:12` |
| pydantic | >=2.0 | Validation / settings | `service/Embedding/pyproject.toml:13-14` |
| torch | >=2.0 | Inference runtime | `service/Embedding/pyproject.toml:15` |
| open-clip-torch | >=1.0 | Fashion-CLIP model | `service/Embedding/pyproject.toml:16` |
| python-multipart | >=0.0.32 | Multipart upload parsing | `service/Embedding/pyproject.toml:17` |

### 3) Development Toolchain

| Tool | Purpose | Evidence |
|------|---------|----------|
| dotnet user-secrets (id: `resys.shop.api`) | Local secret store for dev JWT/encryption keys | `service/Api/src/Api/Api.csproj:7`, `appsettings.Development.json:2` |
| `setup-dev-secrets.sh` | One-shot dev-secret bootstrapper | `service/Api/scripts/setup-dev-secrets.sh` (existence) |
| dotnet-ef 10.0.9 | EF Core migrations CLI (manifest: `dotnet-tools.json`) | `dotnet-tools.json:5-11` |
| dotnet-aspnet-codegenerator 10.0.2 | Code scaffolding | `dotnet-tools.json:12-18` |
| Microsoft.NET.Test.Sdk 18.7.0 | xUnit v3 host | `Directory.Packages.props:102` |
| xunit.v3 + runner.visualstudio + analyzers | 3.2.2 / 3.1.5 / 1.27.0 | `Directory.Packages.props:110-112` |
| FluentAssertions 8.10.0 | Test assertion library | `Directory.Packages.props:104` |
| Moq 4.20.72 | Test mocking | `Directory.Packages.props:105` |
| Polly.Core 8.6.6 | Resilience patterns in tests | `Directory.Packages.props:106` |
| Respawn 7.0.0 | DB checkpointing in integration tests | `Directory.Packages.props:107` |
| Testcontainers.PostgreSql / Redis 4.12.0 | Real containers for integration tests | `Directory.Packages.props:108-109` |
| coverlet.collector 10.0.1 | Code coverage collector | `Directory.Packages.props:103` |
| Vitest 4.1.9 | Frontend unit test runner (Admin + Store) | `app/Admin/package.json:65`, `app/Store/package.json:50` |
| @vitest/coverage-v8 | Frontend coverage | `app/Admin/package.json:43`, `app/Store/package.json:33` |
| jsdom 29.1.1 | DOM env for Vitest | `app/Admin/package.json:54`, `app/Store/package.json:43` |
| @vue/test-utils 2.4.11 | Vue component testing | `app/Admin/package.json:46`, `app/Store/package.json:36` |
| oxlint + oxfmt | Fast linter/formatter for JS/TS | `app/Admin/package.json:56-57`, `app/Admin/.oxlintrc.json:1-9`, `app/Admin/.oxfmtrc.json:1-5` |
| ESLint 10 + eslint-plugin-vue, oxlint, boundaries, prettier | Vue/TS linting | `app/Admin/eslint.config.ts:1-57`, `app/Admin/package.json:48-52` |
| vite 8.0.16 | Build/dev server | `app/Admin/package.json:63`, `app/Store/package.json:49` |
| vue-tsc 3.3.5 | Type-check Vue SFCs | `app/Admin/package.json:66`, `app/Store/package.json:51` |
| pytest >=8, httpx >=0.28, ruff >=0.15.20 | Embedding-side tests + lint | `service/Embedding/pyproject.toml:53-55` |
| `.editorconfig` (root) | Cross-language formatting rules | `.editorconfig:1-389` |

### 4) Key Commands

```bash
# Backend build / test
dotnet build                                                          # warnings-as-errors per Directory.Build.props:17
dotnet test service/Api/tests/Module.UnitTests                       # unit tests, no Docker
dotnet test service/Api/tests/Shared.UnitTests
dotnet test service/Api/tests/Api.Tests                              # integration (Testcontainers)
dotnet test /p:CollectCoverage=true

# Frontend (Admin)
cd app/Admin && pnpm install
pnpm run dev             # Vite dev (port 5173)
pnpm run lint            # oxlint + eslint
pnpm run test:unit       # Vitest
pnpm run type-check      # vue-tsc
pnpm run build

# Frontend (Store) — same scripts
cd app/Store && pnpm install && pnpm run dev && pnpm run test:unit

# Embedding (Python)
cd service/Embedding && uv sync
uv run uvicorn embedding.main:app --reload
uv run ruff check .
uv run pytest

# One-command orchestration (Aspire)
dotnet run --project infra/Aspire/src/ReSys.AppHost
```

### 5) Environment and Config

- **Config sources (backend):**
  - `service/Api/src/Api/appsettings.json` (default settings + Redis/Hangfire policies) — `service/Api/src/Api/appsettings.json:1-243`
  - `service/Api/src/Api/appsettings.Development.json` (dev-only, secrets come from `dotnet user-secrets`) — `service/Api/src/Api/appsettings.Development.json:1-84`
  - `service/Api/src/Api/appsettings.Testing.json` (test override) — `service/Api/src/Api/appsettings.Testing.json` (existence)
  - `service/Api/src/Api/.env.template` documents env-var equivalents — `service/Api/src/Api/.env.template:1-33`
  - Aspire: `infra/Aspire/src/ReSys.AppHost/appsettings.json`, `appsettings.Development.json` (existence)
- **Aspire wiring (PG/Redis/API/Vite/Embedding)**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs:9-49`
- **Service-defaults (OTel, health, resilience, service discovery)**: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:19-132`
- **Required env vars (from template):** `ConnectionStrings__DefaultConnection`, `Authentication__Jwt__Secret`, `Authentication__Google__ClientId`, `Cors__Origins`, `Cors__AllowCredentials`, `Storage__Providers__Local__LocalPath`, `Notification__Channels__Email__*`, `BackgroundJobs__CachingEnabled`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `VITE_API_URL` — `service/Api/src/Api/.env.template:5-33`
- **Frontend env files:** `app/Admin/.env.development`, `app/Store/.env.development` (Vite proxy target via `VITE_API_URL`, default `http://localhost:5035` per `app/Admin/vite.config.ts:51`)
- **Launch profile ports:** `service/Api/src/Api/Properties/launchSettings.json:6-18` (http `http://localhost:5035`, https `https://localhost:7106`)
- **Deployment/runtime constraints:** Aspire orchestrates locally; no Dockerfiles, no CI pipeline (`docs/codebase/.codebase-scan.txt:336-339`).

### 6) Evidence

- `Directory.Build.props` (target framework, nullable, analyzers, TreatWarningsAsErrors, version metadata, InternalsVisibleTo, test project setup) — `Directory.Build.props:1-124`
- `Directory.Build.targets` (Domain/Application/Presentation reference validation targets; `ValidateVerticalSliceIsolation` currently disabled) — `Directory.Build.targets:1-68`
- `Directory.Packages.props` (all NuGet versions) — `Directory.Packages.props:1-117`
- `global.json` (SDK pinning + test runner) — `global.json:1-9`
- `ReSys.Shop.slnx` (solution map) — `ReSys.Shop.slnx:1-30`
- `service/Api/src/Api/Api.csproj` (entry host) — `service/Api/src/Api/Api.csproj:1-34`
- `service/Api/src/Module/Module.csproj` (module aggregator) — `service/Api/src/Module/Module.csproj:1-21`
- `service/Api/src/Shared/Shared.csproj` (cross-cutting infra) — `service/Api/src/Shared/Shared.csproj:1-72`
- `service/Api/src/Migrations/Api.Migrations.csproj` (EF migrations, exists per `ReSys.Shop.slnx:218`)
- `service/Embedding/pyproject.toml` (Python deps) — `service/Embedding/pyproject.toml:1-56`
- `app/Admin/package.json` (Admin SPA deps) — `app/Admin/package.json:1-71`
- `app/Store/package.json` (Store SPA deps) — `app/Store/package.json:1-56`
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` (Aspire wiring) — `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49`
- `service/Api/src/Api/Program.cs` (composition root) — `service/Api/src/Api/Program.cs:1-66`
