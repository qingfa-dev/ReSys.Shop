<div align="center">

# ReSys.Shop

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Vue.js](https://img.shields.io/badge/Vue-3.5-4FC08D?style=flat-square&logo=vue.js)](https://vuejs.org)
[![Python](https://img.shields.io/badge/Python-3.14-3776AB?style=flat-square&logo=python)](https://python.org)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-pgvector-4169E1?style=flat-square&logo=postgresql)](https://postgresql.org)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

A modular e-commerce platform built with .NET 10, Vue 3, and PostgreSQL — featuring CQRS, vector-based image search, multi-provider storage, and Aspire-powered orchestration.

[Architecture](#architecture) . [Features](#features) . [Getting Started](#getting-started) . [Project Structure](#project-structure) . [Tech Stack](#tech-stack) . [Documentation](#documentation)

</div>

## Architecture

ReSys.Shop is a **modular monolith** with 8 business modules sharing common infrastructure. The API uses **CQRS via MediatR** with Carter minimal API endpoints, backed by **EF Core + PostgreSQL** (with pgvector for vector similarity search).

```
HTTP request
  -> Carter endpoint
    -> MediatR pipeline (Logging -> Validation -> ExceptionMapping)
      -> Command/Query Handler
        -> Domain logic + EF Core / Storage / External APIs
          -> Mapster-mapped DTO response
  -> HTTP response
```

Each business module is a self-contained vertical slice organized as `Features/{Admin|Storefront}/{Feature}/{Action}/`. Modules share infrastructure but never reference each other. The system is orchestrated via **.NET Aspire**, which wires PostgreSQL (pgvector), Redis, the .NET API, a Python ML sidecar, and two Vue 3 frontends.

### Business Modules

| Module | Purpose |
|--------|---------|
| **Catalog** | Products, variants, taxonomies, option types, image embeddings |
| **Identity** | Users, roles, permissions, auth flows, OAuth, session management |
| **Inventory** | Stock items, stock locations, reservations, transfers |
| **Location** | Countries, states/provinces, ISO code lookups |
| **Ordering** | Cart management, checkout, orders, cart expiry jobs |
| **Payment** | Payment intents, methods, refunds, Stripe webhook handlers |
| **Profile** | User profiles, addresses, wishlists, notification preferences |
| **Shipping** | Shipping methods, rates, calculation, address estimation |

## Features

- **CQRS pipeline** — MediatR commands/queries with logging, validation, and exception mapping behaviors
- **Image vector search** — Fashion-CLIP embedding sidecar for visual product similarity search
- **Multi-provider storage** — Pluggable file storage (Local, S3-compatible, Azure Blob) with malware scanning and anti-forgery
- **Multi-channel notifications** — Email (SendGrid/SMTP), SMS (Sinch), with provider fallback and retry
- **Background jobs** — Hangfire for scheduled and async processing (Redis or in-memory)
- **Multi-tier caching** — HybridCache + Redis + in-memory with configurable expiration
- **Full auth stack** — JWT tokens with refresh/rotation, guest sessions, Google OAuth, permission-based authorization
- **Rate limiting** — Named policies for auth, registration, password resets, and payment endpoints
- **Outbound webhooks** — Hangfire job POSTs `order.placed` events to configured URLs
- **OpenAPI-first** — Scalar UI, FluentValidation auto-registration, structured API error responses
- **Specification-based querying** — DSL-driven filtering, sorting, paging, and full-text search with composable expressions

## Getting Started

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) (SDK 10.0.301+)
- [Node.js 20+](https://nodejs.org) and [pnpm](https://pnpm.io) (for frontends)
- [Python 3.14+](https://python.org) and [uv](https://docs.astral.sh/uv/) (for the embedding service, optional)
- [Docker](https://docker.com) (required for integration tests and Aspire orchestration)

### One-command start (Aspire)

Start everything — API, frontends, embedding service, PostgreSQL, and Redis — via the Aspire AppHost:

```bash
dotnet run --project infra/Aspire/src/ReSys.AppHost
```

### Run components individually

```bash
# .NET API
dotnet run --project service/Api/src/Api

# Admin SPA (port 5173)
cd app/Admin && pnpm install && pnpm run dev

# Store SPA (port 5174)
cd app/Store && pnpm install && pnpm run dev

# Embedding service (port 8000)
cd service/Embedding && uv sync && uv run uvicorn embedding.main:app --reload
```

> [!TIP]
> Frontends proxy `/api` to `http://localhost:5035` by default. Aspire overrides this with service discovery. See `app/*/.env.development`.

### Build and test

```bash
dotnet build                          # Build all projects (warnings-as-errors enforced)
dotnet test                           # Run all .NET tests
dotnet test service/Api/tests/Module.UnitTests  # Unit tests only (fast, no Docker)
dotnet test /p:CollectCoverage=true   # With code coverage

cd app/Store && pnpm run test:unit    # Store SPA tests
cd app/Admin && pnpm run test:unit    # Admin SPA tests
cd service/Embedding && uv run pytest # Embedding tests
```

### Configuration

The API uses the standard .NET configuration pipeline (`appsettings.json` + environment-specific overrides). A `.env.template` is provided at `service/Api/src/Api/.env.template` documenting all environment variables needed for development.

## Project Structure

```
├── service/
│   ├── Api/src/
│   │   ├── Api/           # Host — Program.cs, middleware, config
│   │   ├── Module/        # 8 business modules
│   │   │   ├── Catalog/   ├── Identity/   ├── Inventory/
│   │   │   ├── Location/  ├── Ordering/   ├── Payment/
│   │   │   ├── Profile/   └── Shipping/
│   │   ├── Shared/        # Infrastructure (persistence, auth, storage, caching, jobs, notifications)
│   │   └── Migrations/    # EF Core migrations
│   ├── Api/tests/         # .NET tests (unit + integration)
│   └── Embedding/         # Python FastAPI ML sidecar (Fashion-CLIP)
├── app/
│   ├── Admin/             # Vue 3 admin SPA (PrimeVue + Tailwind CSS)
│   └── Store/             # Vue 3 storefront SPA (Nuxt UI + Tailwind CSS)
├── infra/Aspire/          # .NET Aspire orchestration (AppHost + ServiceDefaults)
├── ApiTests/              # HTTP test files (.http)
├── docs/codebase/         # Architecture, stack, conventions, concerns
└── plan/                  # Implementation plans
```

## Tech Stack

| Layer | Technology |
|-------|----------|
| **Backend** | .NET 10, ASP.NET Core, C# |
| **API** | Carter, MediatR 14.1, FluentValidation, Mapster |
| **Database** | PostgreSQL 17 + pgvector, EF Core 10, EF Core Naming Conventions |
| **Caching** | HybridCache, Redis 7 |
| **Auth** | JWT Bearer, ASP.NET Identity, Google OAuth, ECDSA signing |
| **Jobs** | Hangfire (Redis or in-memory) |
| **Storage** | Local, S3-compatible, Azure Blob (pluggable via IStorageProvider) |
| **Notifications** | SendGrid, SMTP, Sinch (FluentEmail abstraction) |
| **Payments** | Stripe.net (planned), BogusGateway (dev) |
| **Observability** | OpenTelemetry (traces, metrics, logs), health checks |
| **Admin SPA** | Vue 3, PrimeVue 4, Pinia, Tailwind CSS 4, Vite 8 |
| **Store SPA** | Vue 3, Nuxt UI 4, Pinia, Tailwind CSS 4, Vite 8 |
| **ML sidecar** | Python 3.14, FastAPI, PyTorch, open-clip-torch |
| **Orchestration** | .NET Aspire 13.4 |
| **Testing** | xUnit v3, Testcontainers, Respawn, Vitest, pytest |

## Documentation

In-depth documentation lives in [`docs/codebase/`](docs/codebase/):

| Document | Covers |
|----------|--------|
| [STACK.md](docs/codebase/STACK.md) | Framework versions, all dependencies, dev toolchain, commands |
| [ARCHITECTURE.md](docs/codebase/ARCHITECTURE.md) | Layers, CQRS pipeline, design patterns, architectural risks |
| [STRUCTURE.md](docs/codebase/STRUCTURE.md) | Directory layout, entry points, module boundaries |
| [CONVENTIONS.md](docs/codebase/CONVENTIONS.md) | Naming, formatting, error handling, import rules |
| [INTEGRATIONS.md](docs/codebase/INTEGRATIONS.md) | External services, data stores, secrets, reliability |
| [TESTING.md](docs/codebase/TESTING.md) | Test frameworks, layout, mocking strategy, coverage |
| [CONCERNS.md](docs/codebase/CONCERNS.md) | Known issues, tech debt, security risks, WIP items |

## Work in Progress

> [!NOTE]
> The following components are under active development and not yet feature-complete:

- **Admin SPA** — Layout infrastructure (topbar, sidebar, breadcrumb) and auth routes are in place; feature views are being implemented
- **Embedding service** — Module structure is defined; runtime imports are resolved; end-to-end verification pending
- **Dockerfiles** — No container images yet; deployment uses CLI commands
- **CI/CD** — No pipeline configured; builds and tests run manually

---

<div align="center">
  <sub>Built with .NET, Vue, and Python</sub>
</div>
