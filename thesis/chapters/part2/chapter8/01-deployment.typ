== Deployment Architecture

=== Local Development (Aspire Orchestration)

Aspire provides a single-command local development environment. The `AppHost` project orchestrates all services:

#figure(
  table(
    columns: (auto, auto, auto),
    align: (start, start, start),
    [*Service*], [*Port*], [*Runtime*],
    [PostgreSQL pgvector:17], [5432], [Container (Docker)],
    [Redis 7-alpine], [6379], [Container (Docker)],
    [API], [5035], [.NET 10 (Kestrel)],
    [Embedding sidecar], [8000], [Python FastAPI],
    [Store SPA (Vite dev)], [5174], [Node.js],
    [Admin SPA (Vite dev)], [5173], [Node.js],
  ),
  caption: [Aspire-orchestrated local development services],
)

All containers and processes are managed by the Aspire AppHost. The frontends are dev-served by Vite and proxy API requests to `localhost:5035`.

*Evidence*: `infra/Aspire/src/ReSys.AppHost/AppHost.cs:9-49`

=== Service Defaults

All Aspire-managed services share `ReSys.ServiceDefaults` which registers:

- OpenTelemetry (traces, metrics, logs)
- Health checks (`/health`, `/alive`)
- Service discovery
- HTTP client resilience (Polly pipeline)

*Evidence*: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:19-132`

=== Production Deployment (Conceptual)

Since Dockerfiles and CI/CD are explicitly out of scope (deferred), the production deployment design is conceptual. The architecture assumes:

#figure(
  table(
    columns: (auto, auto),
    align: (start, start),
    [*Component*], [*Deployment Target*],
    [API container], [Single .NET 10 container (modular monolith)],
    [Store SPA], [Static bundle on CDN or object storage],
    [Admin SPA], [Static bundle on CDN or object storage],
    [Embedding sidecar], [Separate Python container],
    [PostgreSQL 17], [Managed cloud database],
    [Redis 7], [Managed cloud cache/job queue],
    [Load balancer], [YARP reverse proxy (deferred)],
    [External services], [Stripe API, SendGrid API, S3 bucket],
  ),
  caption: [Production deployment targets],
)

*Design rationale*: A modular monolith is naturally containerizable as a single API container. The frontends are static SPA bundles served from a CDN or object storage. The embedding sidecar is a separate container because Python/.NET runtimes don't share a process. Redis and PostgreSQL are best managed by cloud providers in production.

=== Deployment Diagram (Conceptual)

#figure(
  {
    set text(size: 8pt)
    let box-w = 2.6cm
    let box-h = 1.2cm
    let col-gap = 0.8cm
    let row-gap = 0.7cm

    // --- Client Tier ---
    let browser-box = rect(width: box-w, height: box-h, stroke: 0.5pt)[Browser\ Customer / Admin]
    let cdn-box = rect(width: box-w, height: box-h, stroke: 0.5pt)[CDN / Static Hosting\ Store SPA + Admin SPA]

    // --- Application Tier ---
    let lb-box = rect(width: box-w, height: box-h, stroke: 0.5pt)[Load Balancer\ (Ingress / YARP)]
    let api1-box = rect(width: box-w, height: box-h, stroke: 0.5pt)[API Container 1\ .NET 10 Modular Monolith]
    let api2-box = rect(width: box-w, height: box-h, stroke: 0.5pt)[API Container 2\ .NET 10 Modular Monolith]
    let emb-box = rect(width: box-w, height: box-h, stroke: 0.5pt)[Embedding Container\ Python + FastAPI + PyTorch]

    // --- Data Tier ---
    let pg-box = rect(width: box-w, height: box-h, stroke: 0.5pt, fill: luma(230))[PostgreSQL 17\ Primary + Replica\ pgvector]
    let redis-box = rect(width: box-w, height: box-h, stroke: 0.5pt, fill: luma(230))[Redis 7\ Cache + Hangfire]

    // --- External ---
    let stripe-box = rect(width: box-w, height: box-h, stroke: 0.5pt, fill: luma(245))[Stripe API]
    let sendgrid-box = rect(width: box-w, height: box-h, stroke: 0.5pt, fill: luma(245))[SendGrid / SMTP]
    let s3-box = rect(width: box-w, height: box-h, stroke: 0.5pt, fill: luma(245))[S3 Storage]
    let google-box = rect(width: box-w, height: box-h, stroke: 0.5pt, fill: luma(245))[Google OAuth]

    // Layout using grid for tier labels + boxes
    grid(
      columns: (auto, 1fr, 1fr, 1fr),
      rows: (auto, auto, auto),
      column-gutter: col-gap,
      row-gutter: row-gap,
      align: (start, center, center, center),

      // Row 1: Client Tier
      [Client Tier:], browser-box, cdn-box, [],

      // Row 2: Application Tier
      [Application Tier:], lb-box, grid(
        columns: (1fr, 1fr),
        column-gutter: col-gap,
        api1-box, api2-box,
      ), emb-box,

      // Row 3: Data Tier + External
      [Data Tier + External:], grid(
        columns: (1fr, 1fr),
        column-gutter: col-gap,
        pg-box, redis-box,
      ), grid(
        columns: (1fr, 1fr),
        column-gutter: col-gap,
        stripe-box, sendgrid-box,
      ), grid(
        columns: (1fr, 1fr),
        column-gutter: col-gap,
        s3-box, google-box,
      ),
    )

    // Connection notes
    v(0.3cm)
    set text(size: 7pt, style: "italic")
    [Connections: Browser → HTTPS → CDN; Browser → HTTPS → LB → API (round-robin); API → TCP 5432 → PG; API → TCP 6379 → Redis; API → HTTP 8000 → Embedding; API → HTTPS → Stripe / SendGrid / S3 / Google OAuth]
  },
  caption: [Conceptual Production Deployment Diagram],
)

== Configuration per Environment

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: (start, start, start, start),
    [*Source*], [*Development*], [*Testing*], [*Production*],
    [`appsettings.json`], [Base], [Base], [Base],
    [`appsettings.Development.json`], [Override], [Ignored], [Ignored],
    [`appsettings.Testing.json`], [Ignored], [Override], [Ignored],
    [`dotnet user-secrets`], [Secrets], [Ignored], [Ignored],
    [Environment variables], [Optional], [Injected by test factory], [Primary secret source],
  ),
  caption: [Configuration source precedence by environment],
)

*Evidence*: `service/Api/src/Api/appsettings.json`, `appsettings.Development.json`, `appsettings.Testing.json`, `.env.template`

== Health Checks

Three health checks are registered:

#figure(
  table(
    columns: (auto, auto, auto),
    align: (start, start, start),
    [*Check*], [*Endpoint*], [*Purpose*],
    [`self`], [`/alive`], [Liveness --- process is running],
    [`npgsql`], [`/health`], [Readiness --- database connectivity],
    [`redis`], [`/health`], [Readiness --- cache connectivity],
    [`database_initialization`], [`/health`], [Readiness --- migrations have run],
  ),
  caption: [Health check endpoints and purposes],
)

*Note*: `MapDefaultEndpoints` only exposes these in non-production environments (`Extensions.cs:114-131`). In production, health checks should be exposed on a separate port or via a sidecar.

*Evidence*: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:114-131`

== Evidence

- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49` --- Aspire orchestration
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:1-132` --- service defaults
- `service/Api/src/Api/appsettings.json:1-237` --- environment config hierarchy
- `service/Api/src/Api/.env.template:1-33` --- environment variable documentation
