# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

| Area | Value | Evidence |
|------|-------|----------|
| Primary language (API) | C# (.NET 10.0) | `service/Api/src/Api/Api.csproj:4` |
| Primary language (Admin) | TypeScript 6.0 (Vue 3) | `app/Admin/package.json:52` |
| Primary language (Store) | TypeScript 6.0 (Vue 3) | `app/Store/package.json:48` |
| Primary language (Embedding) | Python 3.14 | `service/Embedding/.python-version:1` |
| Runtime + version | .NET SDK 10.0.301 | `global.json:3` |
| Package manager (.NET) | NuGet (central package mgmt) | `Directory.Packages.props:4` |
| Package manager (Admin) | pnpm | `app/Admin/package.json` + `pnpm-lock.yaml` |
| Package manager (Store) | pnpm | `app/Store/package.json` + `pnpm-lock.yaml` |
| Package manager (Python) | uv | `service/Embedding/pyproject.toml` + `uv.lock` |
| Module/build system | MSBuild / Vite / Setuptools | `.slnx` + `vite.config.ts` + `pyproject.toml` |

### 2) Production Frameworks and Dependencies

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| ASP.NET Core | 10.0.9 | Web API host | `Directory.Packages.props:7` |
| Entity Framework Core | 10.0.9 | ORM / data access | `Directory.Packages.props:36` |
| Npgsql (PostgreSQL) | 10.0.2 | PostgreSQL EF Core provider | `Directory.Packages.props:42` |
| Pgvector | 0.3.2 | Vector similarity search extension | `Directory.Packages.props:43-44` |
| Carter | 10.0.0 | Minimal API endpoint framework | `Directory.Packages.props:47` |
| MediatR | 14.1.0 | CQRS / mediator pattern | `Directory.Packages.props:49` |
| FluentValidation | 12.1.1 | Input validation | `Directory.Packages.props:48` |
| Mapster | 10.0.9 | Object mapping | `Directory.Packages.props:50-51` |
| Scalar.AspNetCore | 2.16.6 | OpenAPI UI | `Directory.Packages.props:53` |
| Hangfire | 1.8.23 | Background job processing | `Directory.Packages.props:77-79` |
| StackExchange.Redis | bundled | Redis client | `Directory.Packages.props:60` |
| HybridCache | 10.6.0 | Multi-tier caching abstraction | `Directory.Packages.props:59` |
| JWT Bearer | 10.0.9 | Token authentication | `Directory.Packages.props:69` |
| ASP.NET Identity | 10.0.9 | User/role management | `Directory.Packages.props:71` |
| FluentEmail + SendGrid | 3.0.2 | Email delivery | `Directory.Packages.props:80-82` |
| Sinch | 1.5.0 | SMS delivery | `Directory.Packages.props:83` |
| Stripe.net | 52.1.0 | Payment processing | `Directory.Packages.props:84` |
| SkiaSharp | 3.116.1 | Image processing | `Directory.Packages.props:88-89` |
| nClam | 7.0.0 | Malware scanning (ClamAV) | `Directory.Packages.props:92` |
| Aspire | 13.4.6 | Cloud-native orchestrator | `Directory.Packages.props:11` |
| OpenTelemetry | 1.16.0 | Observability (traces, metrics, logs) | `Directory.Packages.props:12` |
| Vue 3 | ^3.5.38 | Frontend framework (Admin + Store) | `app/Admin/package.json:24`, `app/Store/package.json:23` |
| Pinia | ^3.0.4 | State management (Admin + Store) | `app/Admin/package.json:21`, `app/Store/package.json:21` |
| Vue Router | ^5.1.0 | Client-side routing | `app/Admin/package.json:25`, `app/Store/package.json:24` |
| PrimeVue | ^4.5.5 | Admin UI component library | `app/Admin/package.json:23` |
| Nuxt UI | ^4.0.0 | Store UI component library | `app/Store/package.json:20` |
| Tailwind CSS | ^4.3.2 | Utility CSS framework | `app/Admin/package.json:29`, `app/Store/package.json:22` |
| FastAPI | >=0.115 | Python ML service web framework | `service/Embedding/pyproject.toml:11` |
| PyTorch | >=2.0 | ML model runtime | `service/Embedding/pyproject.toml:14` |
| open-clip-torch | >=1.0 | Fashion-CLIP vision model | `service/Embedding/pyproject.toml:15` |

### 3) Development Toolchain

| Tool | Purpose | Evidence |
|------|---------|----------|
| oxlint | JavaScript/TypeScript linter (Admin + Store) | `app/Admin/package.json:48` |
| oxfmt | JavaScript/TypeScript formatter (Admin + Store) | `app/Admin/package.json:47` |
| ESLint | JS/TS linting (Admin + Store) | `app/Admin/package.json:39` |
| Vue-tsc | TypeScript type-checking (Admin + Store) | `app/Admin/package.json:57` |
| Vitest | Unit test runner (Admin + Store) | `app/Admin/package.json:56` |
| xUnit v3 | .NET test framework | `Directory.Packages.props:110` |
| FluentAssertions | .NET assertion library | `Directory.Packages.props:104` |
| Moq | .NET mocking library | `Directory.Packages.props:105` |
| Testcontainers | Integration test containers | `Directory.Packages.props:108` |
| Ruff | Python linter | `service/Embedding/pyproject.toml:47` |
| coverlet | .NET code coverage | `Directory.Packages.props:103` |
| Vite | Frontend build tool | `app/Admin/package.json:54` |
| dotnet-ef | EF Core CLI tool | `dotnet-tools.json:6` |

### 4) Key Commands

```bash
# .NET API
dotnet build                                     # Build all .NET projects
dotnet test                                      # Run all .NET tests
dotnet test --collect:"XPlat Code Coverage"      # Run tests with coverage

# Frontend (Admin / Store)
pnpm install                                     # Install dependencies
pnpm run dev                                     # Start dev server (Admin :5173, Store :5174)
pnpm run build                                   # Production build
pnpm run test:unit                               # Run vitest tests
pnpm run lint                                    # Run oxlint + eslint
pnpm run format                                  # Run oxfmt

# Embedding (Python)
uv sync                                          # Sync virtual environment
uv run pytest                                    # Run Python tests
uv run uvicorn embedding.main:app               # Start dev server

# Infrastructure (Aspire)
dotnet run --project infra/Aspire/src/ReSys.AppHost  # Start Aspire orchestration
```

### 5) Environment and Config

- Config sources: `appsettings.json`, `appsettings.Development.json`, `app/Admin/.env.development`, `app/Store/.env.development`
- Required env vars: `ConnectionStrings:DefaultConnection`, `Authentication:Jwt:Secret`, `VITE_API_URL` (frontend). Many others are configured with defaults in `appsettings.json`.
- Deployment/runtime constraints: .NET 10.0, Python 3.14, Node >=20.19.0. PostgreSQL (with pgvector extension), Redis, and optionally ClamAV required. Docker not yet containerized.

### 6) Evidence

- `service/Api/src/Api/Api.csproj`
- `Directory.Packages.props`
- `global.json`
- `app/Admin/package.json`
- `app/Store/package.json`
- `service/Embedding/pyproject.toml`
- `service/Embedding/.python-version`
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs`
