=== Technology Stack

The platform uses a version-pinned technology stack across three runtime ecosystems, each selected for domain alignment. .NET 10 provides strongly typed transactional semantics and enterprise API abstractions. Python 3.12 provides access to the deep learning ecosystem (PyTorch, Hugging Face). Vue 3 delivers component-driven reactive user interfaces.

Centralized package management enforces build reproducibility: `Directory.Packages.props` for NuGet, `uv.lock` for Python, and `pnpm-lock.yaml` for JavaScript.

@tbl-framework-matrix details the core technologies grouped by architectural role.

#figure(
  table(
    columns: (1.2fr, 2.8fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6.5pt,

    table.header([*Architectural Role*], [*Technology & Version*]),

    [*Backend Runtime*], [.NET 10 / ASP.NET Core 10.0.9 (C\# 13)],
    [*API Framework*], [Carter 10.0.0, MediatR 14.1.0, FluentValidation 12.1.1],
    [*Object Mapping*], [Mapster 10.0.9],
    [*ORM & Database*], [EF Core 10.0.9, Npgsql 10.0.2, pgvector 0.3.2],
    [*Caching Layer*], [HybridCache 10.6.0, StackExchange.Redis 10.0.9],
    [*Background Jobs*], [Hangfire 1.8.23 (Redis-backed)],
    [*Observability*], [OpenTelemetry 1.16.0],

    [*ML Runtime*], [Python 3.12, PyTorch >= 2.0.0, TorchVision >= 0.15.0],
    [*ML Framework*], [FastAPI >= 0.115.0, Uvicorn, Hugging Face Transformers],
    [*ONNX Runtime*], [onnxruntime >= 1.17.0],

    [*Storefront UI*], [Vue 3.5, TypeScript ~ 6.0, Vite 8, PrimeVue 4 (Aura)],
    [*Admin UI*], [Vue 3.5, TypeScript ~ 6.0, Vite 8, PrimeVue 5 (Sakai), Chart.js 4],
    [*State & HTTP*], [Pinia, Axios 1.x, Zod],

    [*Data Storage*], [PostgreSQL 17 (`pgvector/pgvector:pg17-trixie`)],
    [*Cache Storage*], [Redis 7 (Alpine)],
    [*Orchestration*], [.NET Aspire 13.4.6],
    [*Test Suites*], [xUnit v3 3.2.2, pytest >= 8.0, Vitest 4, Playwright],
  ),
  kind: table,
  caption: [Principal technologies grouped by architectural role with pinned version specifications.],
) <tbl-framework-matrix>

==== Service Orchestration and Containerization

- *Aspire Topology:* The AppHost programmatically defines six system resources with strict startup dependencies: PostgreSQL and Redis initialize first, followed by the Python ML sidecar (validated via its `/health` readiness probe), before the .NET API accepts traffic.

  Connection strings and service URLs are injected dynamically via environment variables. The Vue SPAs run as Vite dev servers with hot module replacement, reverse-proxying `/api/` endpoints to the backend.

// [SCREENSHOT: aspire-dashboard-overview.png] Aspire dashboard showing all six running resources with health status, structured logs, and distributed traces.

- *Multi-Stage Container Builds:* The ML sidecar Dockerfile uses a multi-stage build (`uv sync --frozen`) and a minimal runtime stage executing under a non-root user (`inference`, UID 1001) using `tini` for process signal forwarding.

  The .NET API compiles as a self-contained container via `dotnet publish`. The Vue SPAs build to static assets served by an Nginx reverse proxy.

// [SCREENSHOT: docker-build-output.png] Terminal output of the multi-stage Docker build for the ML sidecar, showing the builder and runtime stages with the final image size.