# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

| Area | Value | Evidence |
|------|-------|----------|
| Primary language | C# (70%), TypeScript (12%), Python (9%) | `docs/codebase/.codebase-scan.txt` (lines 319-324) |
| .NET SDK | 10.0.301 (rollForward: latestPatch) | `global.json` (line 4) |
| Target framework | net10.0 (C# preview) | `Directory.Build.props` (line 4) |
| .NET runtime versions | 10.0.9 (AspNetCore, EFCore), 10.6.0 (Extensions) | `Directory.Packages.props` (lines 6-10) |
| Node.js (Admin) | ^22.18.0 or >=24.12.0 | `app/Admin/package.json` (engines) |
| Node.js (Store) | ^20.19.0 or >=22.12.0 | `app/Store/package.json` (engines) |
| Python | >=3.12 | `service/Embedding/pyproject.toml` (line 9), `benchmarks/pyproject.toml` (line 6) |
| Package manager (C#) | NuGet (Central Package Management) | `Directory.Packages.props` (line 3) |
| Package manager (TS) | pnpm | `app/Admin/package.json` (scripts), CI workflow at `.github/workflows/ci.yml` (line 30) |
| Package manager (Python) | uv | `service/Embedding/pyproject.toml` (tool.uv), CI at `.github/workflows/ci.yml` (line 52) |
| Build system (Python) | setuptools (Embedding), hatchling (Benchmarks) | `service/Embedding/pyproject.toml` (line 3), `benchmarks/pyproject.toml` (line 41) |

### 2) Production Frameworks and Dependencies

#### .NET (service/Api)

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| Carter | 10.0.0 | Minimal API endpoint routing | `Directory.Packages.props` |
| MediatR | 14.1.0 | CQRS command/query dispatch | `Directory.Packages.props` |
| FluentValidation | 12.1.1 | Request/command validation | `Directory.Packages.props` |
| Mapster | 10.0.9 | Object-object mapping | `Directory.Packages.props` |
| EF Core (Npgsql) | 10.0.2 | PostgreSQL ORM with pgvector | `Directory.Packages.props` |
| PostgreSQL pgvector | 0.3.2 | Vector similarity search | `Directory.Packages.props` |
| EFCore.NamingConventions | 10.0.1 | snake_case DB column mapping | `Directory.Packages.props` |
| HybridCache | 10.6.0 | Two-level cache (memory + Redis) | `Directory.Packages.props` |
| StackExchangeRedis | 10.0.9 | Redis distributed cache | `Directory.Packages.props` |
| Hangfire | 1.8.23 | Background job processing | `Directory.Packages.props` |
| ASP.NET Identity (EF) | 10.0.9 | User authentication/authorization | `Directory.Packages.props` |
| JWT Bearer | 10.0.9 | Token-based API auth | `Directory.Packages.props` |
| Google.Apis.Auth | 1.75.0 | Google OAuth integration | `Directory.Packages.props` |
| Stripe.net | 52.1.0 | Payment gateway SDK | `Directory.Packages.props` |
| FluentEmail (SMTP/SendGrid) | 3.0.2 | Email notifications | `Directory.Packages.props` |
| Sinch | 1.5.0 | SMS notifications | `Directory.Packages.props` |
| SkiaSharp | 3.116.1 | Server-side image processing | `Directory.Packages.props` |
| nClam | 7.0.0 | Malware scanning (ClamAV) | `Directory.Packages.props` |
| YARP | 2.3.0 | Reverse proxy / API gateway | `Directory.Packages.props` |

#### Vue 3 Admin SPA (app/Admin)

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| Vue | ^3.5.38 | UI framework | `app/Admin/package.json` |
| PrimeVue | ^5.0.0 | UI component library | `app/Admin/package.json` |
| Tailwind CSS | ^4.3.3 | Utility CSS framework | `app/Admin/package.json` |
| Pinia | ^3.0.4 | State management | `app/Admin/package.json` |
| Vue Router | ^5.1.0 | Client-side routing | `app/Admin/package.json` |
| Axios | ^1.18.1 | HTTP client | `app/Admin/package.json` |
| Vee-Validate + Zod | ^4.15.1 + ^3.25.76 | Form validation | `app/Admin/package.json` |
| vue-i18n | ^11.4.7 | Internationalization | `app/Admin/package.json` |
| jwt-decode | ^4.0.0 | JWT token decoding | `app/Admin/package.json` |

#### Vue 3 Storefront SPA (app/Store)

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| Vue | ^3.5.38 | UI framework | `app/Store/package.json` |
| PrimeVue | ^5.0.0 | UI component library | `app/Store/package.json` |
| Tailwind CSS | ^4.3.2 | Utility CSS framework | `app/Store/package.json` |
| Pinia | ^3.0.4 | State management | `app/Store/package.json` |
| Vue Router | ^5.1.0 | Client-side routing | `app/Store/package.json` |

#### Python Embedding Service (service/Embedding)

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| FastAPI | >=0.115.0 | Async REST API framework | `service/Embedding/pyproject.toml` |
| Uvicorn | >=0.20.0 | ASGI server | `service/Embedding/pyproject.toml` |
| PyTorch | >=2.0.0 (CPU) | ML inference | `service/Embedding/pyproject.toml` |
| TorchVision | >=0.15.0 | Image transforms | `service/Embedding/pyproject.toml` |
| Transformers | >=4.30.0 | HuggingFace model hosting | `service/Embedding/pyproject.toml` |
| ONNX Runtime | >=1.17.0 | Optimized model inference | `service/Embedding/pyproject.toml` |
| Pydantic | >=2.0 | Data validation | `service/Embedding/pyproject.toml` |
| Pydantic-Settings | >=2.0.0 | Settings management | `service/Embedding/pyproject.toml` |
| SlowAPI | >=0.1.9 | Rate limiting | `service/Embedding/pyproject.toml` |

#### Python Benchmarks (benchmarks/)

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| PyTorch | >=2.3 | ML inference | `benchmarks/pyproject.toml` |
| OpenCLIP | >=2.24 | CLIP model variants | `benchmarks/pyproject.toml` |
| Fashion-CLIP | >=0.2 | Fashion-specific embeddings | `benchmarks/pyproject.toml` |
| FAISS | >=1.8 | Vector similarity search | `benchmarks/pyproject.toml` |
| Matplotlib + Seaborn | >=3.9 + >=0.13 | Result visualization | `benchmarks/pyproject.toml` |
| Typer | >=0.12 | CLI framework | `benchmarks/pyproject.toml` |

#### Aspire Orchestration (infra/Aspire)

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| Aspire AppHost SDK | 13.4.6 | Local dev orchestration | `infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj` |
| Aspire PostgreSQL Hosting | 13.4.6 | PostgreSQL container management | `Directory.Packages.props` |
| Aspire Redis Hosting | 13.4.6 | Redis container management | `Directory.Packages.props` |
| Aspire Python Hosting | 13.4.6 | Python uvicorn app hosting | `Directory.Packages.props` |
| Aspire Node.js Hosting | 13.4.6 | Vite dev server hosting | `Directory.Packages.props` |
| OpenTelemetry | 1.16.0 (Runtime: 1.15.1, EF/Redis instr: 1.16.0-beta.1) | Distributed tracing/metrics | `Directory.Packages.props` |

### 3) Development Toolchain

| Tool | Purpose | Evidence |
|------|---------|----------|
| MSBuild (dotnet CLI) | Build system | `global.json` (SDK), `Directory.Build.props` |
| Vite 8 | Frontend dev server + bundler | `app/Admin/package.json` (vite: ^8.0.16) |
| Vitest 4 | Frontend unit test runner | `app/Admin/package.json` (vitest: ^4.1.9) |
| xUnit v3 | .NET test runner | `Directory.Build.props` (xunit.v3: 3.2.2) |
| Pytest | Python test runner | `service/Embedding/pyproject.toml` (pytest>=8.0.0) |
| ESLint + Oxlint | JS/TS linting (dual linter) | `app/Admin/package.json` (eslint, oxlint) |
| Oxfmt | TS formatting | `app/Admin/package.json` (oxfmt: ^0.54.0) |
| Ruff | Python lint/format | `service/Embedding/pyproject.toml` (ruff) |
| .editorconfig | Cross-language style rules | `.editorconfig` |
| Coverlet | .NET code coverage | `Directory.Build.props` (coverlet.collector: 10.0.1) |
| Moq + FluentAssertions | .NET mocking + assertions | `Directory.Build.props` |
| Respawn | DB reset between integration tests | `Directory.Packages.props` |
| Testcontainers (PostgreSQL/Redis) | Docker-based integration testing | `Directory.Packages.props` |
| vue-tsc | Vue type checking | `app/Admin/package.json` (vue-tsc: ^3.3.5) |

### 4) Key Commands

```bash
# .NET
dotnet build                                          # Build (warnings-as-errors)
dotnet test service/Api/tests/Module.UnitTests        # Fast unit tests (no Docker)
dotnet test service/Api/tests/Shared.UnitTests        # Shared unit tests
dotnet test                                           # All tests (inc. integration — needs Docker)
dotnet test /p:CollectCoverage=true                   # Opt-in coverage

# Admin SPA
cd app/Admin && pnpm install && pnpm run dev          # Dev server
cd app/Admin && pnpm run lint                         # Lint (oxlint + eslint)
cd app/Admin && pnpm run test:unit                    # Unit tests
cd app/Admin && pnpm run build                        # Production build

# Store SPA
cd app/Store && pnpm install && pnpm run dev          # Dev server
cd app/Store && pnpm run lint                         # Lint
cd app/Store && pnpm run test:unit                    # Unit tests

# Python Embedding
cd service/Embedding && uv sync && uv run uvicorn embedding.main:app  # Dev server
cd service/Embedding && uv run ruff check .           # Lint
cd service/Embedding && uv run pytest                 # Tests

# Benchmarks
cd benchmarks && uv run ruff check src/               # Lint
cd benchmarks && uv run pytest --ignore=src/tests/integration/  # Unit tests
cd benchmarks && uv run benchmark --help              # CLI
```

### 5) Environment and Config

- Config sources: `service/Api/src/Api/appsettings.json`, `appsettings.Development.json`, `dotnet user-secrets` (id: `resys.shop.api`), `service/Api/src/Api/.env.template` (placeholders), `service/Embedding/.env.template` (placeholders), `app/Store/.env.development`
- Required env vars: `ConnectionStrings__DefaultConnection`, `Authentication__Jwt__Secret`, `GatewayProviders__SettingsEncryptionKey`, `GatewayProviders__stripe__SecretKey`, `GatewayProviders__stripe__WebhookSecret`, `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`
- `.env.template` files exist at `service/Api/src/Api/.env.template` and `service/Embedding/.env.template` with placeholder values; `appsettings.json` serves as config template
- Deployment/runtime constraints: Net10.0 on Linux (via Aspire), PostgreSQL with pgvector, Redis 7-alpine, all orchestrated by Aspire for local dev; `service/Embedding/Dockerfile` exists for Python sidecar production deployment

### 6) Evidence

- `Directory.Packages.props` — all NuGet versions (Central Package Management)
- `Directory.Build.props` — target framework, build settings, test detection
- `global.json` — SDK version
- `service/Api/src/Api/Api.csproj` — API project references
- `service/Api/src/Shared/Shared.csproj` — all shared infrastructure packages
- `app/Admin/package.json` — Admin SPA dependencies
- `app/Store/package.json` — Store SPA dependencies
- `service/Embedding/pyproject.toml` — Python embedding dependencies
- `benchmarks/pyproject.toml` — Python benchmark dependencies
- `infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj` — Aspire orchestration dependencies
- `service/Api/src/Api/appsettings.json` — full configuration template
- `.github/workflows/ci.yml` — CI pipeline verification
