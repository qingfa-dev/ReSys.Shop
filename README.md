<div align="center">

# ReSys.Shop

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Vue.js](https://img.shields.io/badge/Vue-3.5-4FC08D?style=flat-square&logo=vue.js)](https://vuejs.org)
[![Python](https://img.shields.io/badge/Python-3.14-3776AB?style=flat-square&logo=python)](https://python.org)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-pgvector-4169E1?style=flat-square&logo=postgresql)](https://postgresql.org)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)
[![PRs](https://img.shields.io/badge/PRs-welcome-brightgreen?style=flat-square)](https://github.com/qingfa-dev/ReSys.Shop/pulls)

A modular e-commerce platform built with .NET 10, Vue 3, and PostgreSQL — featuring CQRS, vector-based image search, multi-provider storage, and Aspire-powered orchestration.

[Architecture](#architecture) • [Features](#features) • [Prerequisites](#prerequisites) • [Getting Started](#getting-started) • [Project Structure](#project-structure) • [Tech Stack](#tech-stack) • [Documentation](#documentation)

</div>

## Architecture

ReSys.Shop is a **modular monolith** organized into distinct business modules sharing common infrastructure. The API uses **CQRS via MediatR** with Carter minimal API endpoints, backed by **EF Core + PostgreSQL** (with pgvector for vector similarity search).

```text
HTTP request
  → Carter endpoint
    → MediatR pipeline (Logging → Validation → ExceptionMapping)
      → Command/Query Handler
        → Domain logic + EF Core / Storage / External APIs
          → Mapster-mapped DTO response
  → HTTP response
```

Each of the four business modules — **Catalog**, **Identity**, **Location**, **Profile** — is a self-contained vertical slice. Modules never reference each other. A shared infrastructure layer handles persistence, authentication, caching, storage, notifications, and background jobs.

The system is orchestrated via **.NET Aspire**, which wires PostgreSQL (pgvector), Redis, the .NET API, a Python ML sidecar, and two Vue 3 frontends.

## Features

- **Modular monolith** — 4 business modules with enforced isolation at build time
- **CQRS pipeline** — MediatR commands/queries with logging, validation, and exception mapping behaviors
- **Image vector search** — Fashion-CLIP embedding sidecar for visual product similarity search
- **Multi-provider storage** — Pluggable file storage (Local filesystem, S3-compatible, Azure Blob) with malware scanning and anti-forgery
- **Multi-channel notifications** — Email (SendGrid/SMTP), SMS (Sinch), with provider fallback and retry
- **Background jobs** — Hangfire for scheduled and async processing (Redis or in-memory)
- **Multi-tier caching** — HybridCache + Redis + in-memory with configurable expiration
- **Full auth stack** — JWT tokens with refresh/rotation, guest sessions, Google OAuth, ASP.NET Identity with permission-based authorization
- **OpenAPI-first** — Scalar UI, FluentValidation auto-registration, structured API error responses
- **Specification-based querying** — DSL-driven filtering, sorting, paging, and full-text search with composable expressions

> [!NOTE]
> Components marked as work in progress: **Admin SPA** (scaffold only) and **Embedding service** (structure defined, runtime imports pending). See [Concerns](docs/codebase/CONCERNS.md) for details.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) (SDK 10.0.301+)
- [Node.js 20+](https://nodejs.org) (for frontends)
- [pnpm](https://pnpm.io/installation) (for frontends)
- [Python 3.14](https://python.org) + [uv](https://docs.astral.sh/uv/) (for embedding service, optional)
- [Docker](https://docker.com) (required for integration tests and Aspire orchestration)

## Getting Started

### One-command start (Aspire)

This starts everything — API, frontends, embedding service, PostgreSQL, and Redis — via the Aspire AppHost:

```bash
dotnet run --project infra/Aspire/src/ReSys.AppHost
```

### Run components individually

```bash
# .NET API (dev)
dotnet run --project service/Api/src/Api

# Admin SPA (dev)
cd app/Admin && pnpm install && pnpm run dev

# Storefront SPA (dev)
cd app/Store && pnpm install && pnpm run dev

# Embedding service (dev)
cd service/Embedding && uv sync && uv run uvicorn embedding.main:app --reload
```

> [!TIP]
> Frontends proxy `/api` to `http://localhost:5035` by default. Aspire overrides this with service discovery. See `app/*/.env.development`.

### Build and test

```bash
# Build all .NET projects (warnings-as-errors enforced)
dotnet build

# Run all .NET tests
dotnet test

# Unit tests only (fast, no Docker)
dotnet test service/Api/tests/Module.UnitTests

# Frontend tests
cd app/Store && pnpm run test:unit

# Embedding tests
cd service/Embedding && uv run pytest
```

## Project Structure

```
├── service/
│   ├── Api/src/           # .NET backend (modular monolith)
│   │   ├── Api/           # Entry point, middleware, config
│   │   ├── Module/        # Business modules
│   │   │   ├── Catalog/   # Products, variants, taxonomies, option types
│   │   │   ├── Identity/  # Users, roles, permissions
│   │   │   ├── Location/  # Countries, states
│   │   │   └── Profile/   # Profiles, addresses, wishlists, notifications
│   │   ├── Shared/        # Infrastructure (auth, persistence, storage, caching...)
│   │   └── Migrations/    # EF Core migrations
│   ├── Api/tests/         # .NET test projects (unit + integration)
│   └── Embedding/         # Python FastAPI ML sidecar (Fashion-CLIP)
├── app/
│   ├── Admin/             # Vue 3 admin SPA (PrimeVue, scaffold)
│   └── Store/             # Vue 3 storefront SPA (Nuxt UI)
├── infra/
│   └── Aspire/            # .NET Aspire orchestration
├── ApiTests/              # HTTP API tests (.http files)
└── docs/codebase/         # Architecture, stack, concerns, conventions
```

## Tech Stack

| Layer | Technology |
|-------|----------|
| **Backend** | .NET 10, ASP.NET Core, C# |
| **API framework** | Carter (minimal APIs), MediatR, FluentValidation, Mapster |
| **Database** | PostgreSQL 17 + pgvector, EF Core 10 |
| **Caching** | HybridCache, Redis 7 |
| **Auth** | JWT Bearer, ASP.NET Identity, Google OAuth |
| **Jobs** | Hangfire (Redis or in-memory) |
| **Storage** | Local filesystem, S3-compatible, Azure Blob |
| **Notifications** | SendGrid, SMTP, Sinch |
| **Observability** | OpenTelemetry (traces, metrics, logs) |
| **Admin frontend** | Vue 3, PrimeVue, Pinia, Tailwind CSS |
| **Storefront frontend** | Vue 3, Nuxt UI, Pinia, Tailwind CSS |
| **ML sidecar** | Python 3.14, FastAPI, PyTorch, open-clip-torch |
| **Orchestration** | .NET Aspire 13.4 |
| **Testing** | xUnit v3, Testcontainers, Vitest, pytest |

## Documentation

In-depth documentation is in `docs/codebase/`:

| Doc | What it covers |
|-----|---------------|
| [STACK.md](docs/codebase/STACK.md) | Full framework versions, dependencies, dev toolchain |
| [ARCHITECTURE.md](docs/codebase/ARCHITECTURE.md) | Layers, patterns, data flow, design decisions |
| [STRUCTURE.md](docs/codebase/STRUCTURE.md) | Directory layout, entry points, module boundaries |
| [CONVENTIONS.md](docs/codebase/CONVENTIONS.md) | Naming, formatting, error handling, import rules |
| [INTEGRATIONS.md](docs/codebase/INTEGRATIONS.md) | External services, secrets, reliability, observability |
| [TESTING.md](docs/codebase/TESTING.md) | Test frameworks, scope, mocking, coverage |
| [CONCERNS.md](docs/codebase/CONCERNS.md) | Known issues, tech debt, security risks, work in progress |

## Work in Progress

- **Admin SPA** — Routes and feature views are not yet implemented (placeholder scaffold only)
- **Embedding service** — Module structure is defined but runtime imports are incomplete; service cannot start as-is
- **Dockerfiles** — No container images exist yet; deployment relies on CLI commands
- **CI/CD** — Pipeline configuration has not been set up

---

<div align="center">
  <sub>Built with .NET, Vue, and Python</sub>
</div>
