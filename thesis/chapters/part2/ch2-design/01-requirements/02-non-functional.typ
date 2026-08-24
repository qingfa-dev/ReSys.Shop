=== Non-Functional Requirements

Five quality dimensions define production-readiness constraints with atomic, measurable targets. Verification is assessed in Chapter 3.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left + horizon),
    inset: 5.5pt,

    table.header([*ID*], [*Category*], [*Requirement Target*]),

    [NFR-01a], [Performance], [
      *End-to-End Search Latency:* Complete CBIR search workflows within *1 second* per query.
    ],
    [NFR-01b], [Performance], [
      *API Response Latency:* Non-search REST endpoints within *200 milliseconds* under standard load.
    ],
    [NFR-01c], [Performance], [
      *Non-Blocking Processing:* Enforce asynchronous I/O across all data access and HTTP pipelines.
    ],

    [NFR-02a], [Security], [
      *Token Lifecycle:* JWT access tokens @jones2015jwt expire after *15 minutes* with single-use refresh token rotation.
    ],
    [NFR-02b], [Security], [
      *Endpoint Authorization:* Enforce fine-grained claims (`domain.category.resource.action`) at the API middleware boundary.
    ],
    [NFR-02c], [Security], [
      *Rate Limiting:* Per-IP fixed-window rate-limit policies for authentication, registration, password reset, payment, and webhook endpoints, configurable via the `RateLimit` settings.
    ],
    [NFR-02d], [Security], [
      *Upload Hardening:* Validate files via magic-byte header inspection, enforce extension allowlists, cap at *10 MB*.
    ],
    [NFR-02e], [Security], [
      *Transport Security:* Inject security headers (`CSP`, `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`) via middleware; HSTS is applied by the reverse-proxy layer in deployment.
    ],

    [NFR-03a], [Modularity], [
      *Single-Assembly Modularity:* All business modules compile into one `Module` assembly with forward-only dependencies (`Shared` → `Module` → `Api`); cross-module references are permitted and guarded by build targets.
    ],
    [NFR-03b], [Modularity], [
      *Decoupled Behaviour:* Route inter-module work through MediatR in-process dispatch where pipeline value (validation, logging, transactions) applies; direct service calls and navigations are used where they fit the feature slice.
    ],
    [NFR-03c], [Modularity], [
      *Module Testability:* Each module independently testable without initializing foreign contexts.
    ],

    [NFR-04a], [Observability], [
      *Distributed Tracing:* Propagate OpenTelemetry trace contexts across .NET and Python ML sidecar boundaries.
    ],
    [NFR-04b], [Observability], [
      *Structured Logging:* Unique correlation identifier in every log entry across distributed execution paths.
    ],
    [NFR-04c], [Observability], [
      *Health Monitoring:* .NET `/health/live` and `/health/ready` endpoints verifying database and cache connectivity; the Python sidecar exposes its own readiness (`/health`) and liveness (`/alive`) endpoints.
    ],

    [NFR-05a], [Reliability], [
      *Job Durability:* Background tasks via Hangfire @hangfire-docs backed by Redis @redis-docs to survive process restarts.
    ],
    [NFR-05b], [Reliability], [
      *Automated Maintenance:* Scheduled purge of carts inactive for *7 days* (hourly recurring job) and release of associated inventory reservations.
    ],
    [NFR-05c], [Reliability], [
      *Webhook Idempotency:* Idempotency key checks on Stripe webhooks to prevent duplicate state updates.
    ],
    [NFR-05d], [Reliability], [
      *Reservation Timeouts:* Unconfirmed checkout inventory holds expire after *15 minutes* of inactivity.
    ],
  ),
  kind: table,
  caption: [Non-functional requirements across five quality dimensions.],
) <nfr-all>
