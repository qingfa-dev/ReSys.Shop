=== Technology Stack

The platform uses pinned versions across three ecosystems: .NET 10 for transactional semantics and API abstractions, Python 3.12 for deep learning (PyTorch, Hugging Face), and Vue 3 for reactive UIs. Centralized package management enforces reproducibility via `Directory.Packages.props` (NuGet), `uv.lock` (Python), and `pnpm-lock.yaml` (JavaScript).

@tbl-framework-matrix details the core technologies grouped by architectural role.

#figure(
  table(
    columns: (1.2fr, 2.8fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6.5pt,
    table.header([*Architectural Role*], [*Technology and Version*]),
    [*Backend Runtime*], [.NET 10 / ASP.NET Core 10.0.9 (C\# 13)],
    [*API Framework*], [Carter 10.0.0, MediatR 14.1.0, FluentValidation 12.1.1],
    [*Object Mapping*], [Mapster 10.0.9],
    [*ORM and Database*], [EF Core 10.0.9, Npgsql 10.0.2, pgvector 0.3.2],
    [*Caching Layer*], [HybridCache 10.6.0, StackExchange.Redis 10.0.9],
    [*Background Jobs*], [Hangfire 1.8.23 (Redis-backed)],
    [*Observability*], [OpenTelemetry 1.16.0],
    [*ML Runtime*], [Python 3.12, PyTorch >= 2.0.0, TorchVision >= 0.15.0],
    [*ML Framework*], [FastAPI >= 0.115.0, Uvicorn, Hugging Face Transformers],
    [*ONNX Runtime*], [onnxruntime >= 1.17.0],
    [*Storefront UI*], [Vue 3.5, TypeScript ~ 6.0, Vite 8, PrimeVue 5 (Aura)],
    [*Admin UI*], [Vue 3.5, TypeScript ~ 6.0, Vite 8, PrimeVue 5 (Sakai), Chart.js 4],
    [*State and HTTP*], [Pinia, Axios 1.x, Zod],
    [*Data Storage*], [PostgreSQL 17 (pgvector/pgvector:pg17-trixie)],
    [*Cache Storage*], [Redis 7 (Alpine)],
    [*Orchestration*], [.NET Aspire 13.4.6],
    [*Test Suites*], [xUnit v3 3.2.2, pytest >= 8.0, Vitest 4, Playwright],
  ),
  kind: table,
  caption: [Principal technologies grouped by architectural role with pinned version specifications.],
) <tbl-framework-matrix>

==== Service Orchestration and Containerization

The AppHost defines six resources with startup dependencies: PostgreSQL and Redis initialize first, followed by the Python ML sidecar with `/health` readiness probe, then the .NET API. Vue SPAs run as Vite dev servers reverse-proxying `/api/` endpoints. Multi-stage Docker builds isolate runtime dependencies with non-root execution via `tini`.

// [SCREENSHOT: implementation-aspire-dashboard.png] .NET Aspire dashboard showing all six running resources with health status indicators, structured log stream, and distributed trace waterfall for a CBIR search request spanning the Vue storefront, .NET API, Python sidecar, and PostgreSQL.

// [SCREENSHOT: implementation-docker-build.png] Terminal output showing the multi-stage Docker build for the ML sidecar: builder stage downloading PyTorch and HuggingFace dependencies, runtime stage copying only the virtual environment, and final image size annotation.
