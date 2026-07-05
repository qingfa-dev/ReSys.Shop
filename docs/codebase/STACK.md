# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

| Area | Value | Evidence |
|------|-------|----------|
| Primary language | C# (.NET) | `service/Api/src/Api/Api.csproj` |
| .NET SDK version | 10.0.301 (target net10.0) | `global.json:3-4`, `Directory.Build.props:4` |
| C# language version | preview | `Directory.Build.props:7` |
| Frontend language | TypeScript 6.0 | `app/Admin/package.json:52`, `app/Store/package.json:48` |
| Python runtime | >=3.14 | `service/Embedding/pyproject.toml:7` |
| .NET package manager | NuGet (Central Package Management) | `Directory.Packages.props:4` |
| Frontend package manager | pnpm (monorepo via pnpm-workspace.yaml per SPA) | `app/Admin/pnpm-workspace.yaml`, `app/Store/pnpm-workspace.yaml` |
| Python package manager | uv | `service/Embedding/pyproject.toml` |
| .NET solution format | .slnx (XML solution) | `ReSys.Shop.slnx` |

### 2) Production Frameworks and Dependencies

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| ASP.NET Core | 10.0.9 | Web framework, middleware, DI | `Directory.Packages.props:8` |
| Carter | 10.0.0 | Minimal API endpoint routing | `Directory.Packages.props:47` |
| MediatR | 14.1.0 | CQRS command/query pipeline | `Directory.Packages.props:52` |
| FluentValidation | 12.1.1 | Request validation | `Directory.Packages.props:48` |
| Mapster | 10.0.9 | Object-to-object mapping | `Directory.Packages.props:50` |
| EF Core | 10.0.9 | ORM (PostgreSQL provider) | `Directory.Packages.props:10` |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.2 | PostgreSQL EF Core provider | `Directory.Packages.props:42` |
| Pgvector | 0.3.2 | Vector similarity search (pgvector) | `Directory.Packages.props:43` |
| EF Core Naming Conventions | 10.0.1 | snake_case database naming | `Directory.Packages.props:41` |
| Microsoft.Extensions.Caching.Hybrid | 10.6.0 | Multi-tier caching | `Directory.Packages.props:59` |
| StackExchange.Redis (via caching extensions) | 10.0.9 | Redis cache backend | `Directory.Packages.props:60` |
| Hangfire.AspNetCore | 1.8.23 | Background job processing | `Directory.Packages.props:77` |
| Hangfire.Redis.StackExchange | 1.12.0 | Redis-backed Hangfire storage | `Directory.Packages.props:79` |
| Hangfire.InMemory | 1.0.0 | In-memory Hangfire storage (dev) | `Directory.Packages.props:78` |
| ASP.NET Identity (EF Core) | 10.0.9 | Authentication / user management | `Directory.Packages.props:71` |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.9 | JWT token auth | `Directory.Packages.props:69` |
| Google.Apis.Auth | 1.75.0 | Google OAuth external login | `Directory.Packages.props:74` |
| FluentEmail.Core | 3.0.2 | Email abstraction | `Directory.Packages.props:80` |
| FluentEmail.SendGrid | 3.0.2 | SendGrid email provider | `Directory.Packages.props:81` |
| FluentEmail.Smtp | 3.0.2 | SMTP email fallback | `Directory.Packages.props:82` |
| Sinch | 1.5.0 | SMS notification provider | `Directory.Packages.props:83` |
| Stripe.net | 52.1.0 | Payment processing | `Directory.Packages.props:84` |
| SkiaSharp | 3.116.1 | Server-side image processing | `Directory.Packages.props:88` |
| nClam | 7.0.0 | Malware scanning (ClamAV) | `Directory.Packages.props:92` |
| Scalar.AspNetCore | 2.16.6 | OpenAPI UI | `Directory.Packages.props:53` |
| OpenTelemetry (multiple packages) | 1.16.0 | Observability (traces, metrics, logs) | `Directory.Packages.props:26-33` |
| Slugify.Core | 5.1.1 | URL slug generation | `Directory.Packages.props:54` |
| starkbank-ecdsa | 1.3.3 | ECDSA JWT signing | `Directory.Packages.props:73` |
| Vue 3 | ^3.5.38 | Storefront SPA framework | `app/Store/package.json:23` |
| @nuxt/ui | ^4.0.0 | Storefront UI component library | `app/Store/package.json:20` |
| PrimeVue | ^4.5.5 | Admin UI component library | `app/Admin/package.json:23` |
| Pinia | ^3.0.4 | Vue state management | `app/Store/package.json:21`, `app/Admin/package.json:21` |
| vue-router | ^5.1.0 | Vue SPA routing | `app/Store/package.json:24`, `app/Admin/package.json:25` |
| FastAPI | >=0.115 | Python ML sidecar web framework | `service/Embedding/pyproject.toml:9` |
| torch | >=2.0 | ML inference (Fashion-CLIP) | `service/Embedding/pyproject.toml:12` |
| open-clip-torch | >=1.0 | CLIP image embeddings | `service/Embedding/pyproject.toml:13` |
| Aspire AppHost SDK | 13.4.6 | Distributed application orchestration | `Directory.Packages.props:11`, `infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj:1` |

### 3) Development Toolchain

| Tool | Purpose | Evidence |
|------|---------|----------|
| dotnet-ef (10.0.9) | EF Core CLI migrations | `dotnet-tools.json:5` |
| dotnet-aspnet-codegenerator (10.0.2) | ASP.NET scaffolding | `dotnet-tools.json:12` |
| Vite 8 | Frontend dev server and build | `app/Admin/package.json:54`, `app/Store/package.json:49` |
| TypeScript (~6.0) | Frontend type checking | `app/Admin/package.json:52`, `app/Store/package.json:48` |
| vue-tsc | Vue template type checking | `app/Admin/package.json:57`, `app/Store/package.json:51` |
| oxfmt | Frontend formatter | `app/Admin/package.json:47`, `app/Store/package.json:45` |
| oxlint (~1.69.0) | Frontend linter | `app/Admin/package.json:48`, `app/Store/package.json:46` |
| ESLint (10.5.0) | Frontend static analysis | `app/Admin/package.json:40`, `app/Store/package.json:38` |
| Ruff (>=0.15.20) | Python linter and formatter | `service/Embedding/pyproject.toml:24` |
| pytest (>=8) | Python test framework | `service/Embedding/pyproject.toml` dev deps |
| Vitest (4.1.9) | Frontend test runner | `app/Admin/package.json:56`, `app/Store/package.json:50` |
| @vue/test-utils (2.4.11) | Vue component testing | `app/Admin/package.json:36`, `app/Store/package.json:34` |
| jsdom | Frontend DOM simulation for tests | `app/Admin/package.json:45`, `app/Store/package.json:43` |
| xUnit v3 (3.2.2) | .NET test framework | `Directory.Packages.props:110` |
| FluentAssertions (8.10.0) | .NET fluent assertions | `Directory.Packages.props:104` |
| Moq (4.20.72) | .NET mocking library | `Directory.Packages.props:105` |
| Testcontainers (4.12.0) | Docker-based integration tests | `Directory.Packages.props:108-109` |
| Respawn (7.0.0) | DB state reset for integration tests | `Directory.Packages.props:107` |
| Coverlet (10.0.1) | .NET code coverage instrumentation | `Directory.Packages.props:103` |
| .editorconfig | Cross-language code style enforcement | `.editorconfig` |
| TreatWarningsAsErrors | Warnings fail builds | `Directory.Build.props:17` |
| Package locks | All .NET projects use `RestorePackagesWithLockFile=true` | `Directory.Build.props:59` |

### 4) Key Commands

```bash
# Build all .NET projects (warnings-as-errors enforced)
dotnet build

# Run all .NET tests
dotnet test

# Run unit tests only (fast, no Docker required)
dotnet test service/Api/tests/Module.UnitTests

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run filtered tests
dotnet test --filter "FullyQualifiedName~Location"

# EF Core migrations
dotnet ef migrations add <MigrationName> --project service/Api/src/Migrations --startup-project service/Api/src/Api

# Frontend — Admin SPA (port 5173)
cd app/Admin && pnpm install && pnpm run dev
cd app/Admin && pnpm run lint
cd app/Admin && pnpm run test:unit

# Frontend — Store SPA (port 5174)
cd app/Store && pnpm install && pnpm run dev
cd app/Store && pnpm run lint
cd app/Store && pnpm run test:unit

# Python embedding service (port 8000)
cd service/Embedding && uv sync && uv run pytest
uv run uvicorn embedding.main:app --reload

# Aspire orchestration (starts all services)
dotnet run --project infra/Aspire/src/ReSys.AppHost
```

### 5) Environment and Config

- Config sources: `appsettings.json`, `appsettings.Development.json` (per environment), Aspire injects service discovery
- Required env vars (from code and config):
  - `ConnectionStrings__DefaultConnection` — PostgreSQL connection string (default in `appsettings.Development.json:8`)
  - `Authentication__Jwt__Secret` — JWT signing secret
  - `Authentication__Google__ClientId` — Google OAuth client ID
  - `Storage__Providers__Local__LocalPath` — Local storage root path
  - `VITE_API_URL` — Frontend API base URL (`app/Admin/.env.development`, `app/Store/.env.development`; Aspire overrides)
  - `Cors__Origins` — Allowed CORS origins
  - `OTEL_EXPORTER_OTLP_ENDPOINT` — OpenTelemetry collector endpoint (optional)
  - `Notification__Channels__Email__*` — Email notification provider config
  - `BackgroundJobs__CachingEnabled` — Hangfire with Redis or in-memory
- Deployment/runtime constraints: PostgreSQL 16+ with pgvector extension, Redis 7+, Docker for integration tests and Aspire, Python 3.14+ for embedding service

### 6) Evidence

- `global.json` — SDK version pinning
- `Directory.Packages.props` — Central NuGet package versions
- `Directory.Build.props` — Build settings (TargetFramework net10.0, LangVersion preview, TreatWarningsAsErrors)
- `dotnet-tools.json` — .NET CLI tools
- `ReSys.Shop.slnx` — Solution structure
- `app/Admin/package.json` — Admin SPA dependencies and scripts
- `app/Store/package.json` — Store SPA dependencies and scripts
- `service/Embedding/pyproject.toml` — Python dependencies and tooling
- `infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj` — Aspire orchestration
- `infra/Aspire/src/ReSys.ServiceDefaults/ReSys.ServiceDefaults.csproj` — Service defaults (OpenTelemetry, resilience)
- `app/Admin/.env.development` — Frontend env vars
- `app/Store/.env.development` — Frontend env vars
- `service/Api/src/Api/appsettings.Development.json` — Backend dev configuration
