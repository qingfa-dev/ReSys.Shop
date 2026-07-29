=== Non-Functional Requirements

Five quality dimensions define the operational constraints required for production readiness. Each requirement is specified as an atomic, measurable target that directly influenced key architectural decisions: the 1-second CBIR latency limit (NFR-01a) mandated synchronous vector generation over queued pipelines; the strict modularity constraints (NFR-03a–c) drove the in-process MediatR dispatch architecture; and the reliability baseline (NFR-05a) required Redis-backed persistence for Hangfire background processing. Verification against each target is evaluated in Chapter 3.

==== Performance

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6pt,

    table.header([*ID*], [*Requirement Target*]),

    [NFR-01a], [
      *End-to-End Search Latency:* Complete CBIR search workflows (image upload, embedding generation, pgvector similarity lookup, and response assembly) within *1 second* per query.
    ],
    [NFR-01b], [
      *API Response Latency:* Respond to non-search REST API endpoints within *200 milliseconds* under standard operating load.
    ],
    [NFR-01c], [
      *Non-Blocking Processing:* Enforce asynchronous I/O across all data access and HTTP pipelines to eliminate thread pool starvation under concurrent loads.
    ],
  ),
  kind: table,
  caption: [Performance latency targets for CBIR search and standard API endpoints.],
) <nfr-performance>

==== Security

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6pt,

    table.header([*ID*], [*Requirement Target*]),

    [NFR-02a], [
      *Token Lifecycle:* Expire JWT access tokens @jones2015jwt after *15 minutes* and enforce single-use refresh token rotation with automated reuse detection.
    ],
    [NFR-02b], [
      *Endpoint Authorization:* Enforce fine-grained claims (`domain:category:action`) at the API middleware boundary using dynamic policy resolution.
    ],
    [NFR-02c], [
      *Rate Limiting:* Restrict authentication requests to *5 attempts per minute* and account registration to *3 attempts per hour* per client IP.
    ],
    [NFR-02d], [
      *Upload Hardening:* Validate uploaded files via magic-byte header inspection, strictly enforce file extension allowlists, and cap upload sizes at *10 MB*.
    ],
    [NFR-02e], [
      *Transport Security:* Inject security headers (`CSP`, `HSTS`, `X-Frame-Options`, `X-Content-Type-Options`) across all HTTP responses.
    ],
  ),
  kind: table,
  caption: [Security requirements for authentication, authorization, rate limiting, and transport safety.],
) <nfr-security>

==== Modularity

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6pt,

    table.header([*ID*], [*Requirement Target*]),

    [NFR-03a], [
      *Compile-Time Isolation:* Maintain business modules within a single .NET assembly while enforcing *zero direct cross-references* via build policy checks.
    ],
    [NFR-03b], [
      *Decoupled Messaging:* Route all inter-module communications exclusively through MediatR in-process message dispatch.
    ],
    [NFR-03c], [
      *Module Testability:* Ensure every module is independently testable without initializing foreign contexts or external database schemas.
    ],
  ),
  kind: table,
  caption: [Modularity requirements governing architectural isolation and communication boundaries.],
) <nfr-modularity>

==== Observability

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6pt,

    table.header([*ID*], [*Requirement Target*]),

    [NFR-04a], [
      *Distributed Tracing:* Propagate OpenTelemetry trace contexts across .NET application and Python ML sidecar boundaries via standard HTTP headers.
    ],
    [NFR-04b], [
      *Structured Logging:* Inject a unique correlation identifier into every log entry to track requests across distributed execution paths.
    ],
    [NFR-04c], [
      *Health Monitoring:* Expose dedicated health endpoints verifying database, cache, and sidecar connectivity for orchestrator probes.
    ],
  ),
  kind: table,
  caption: [Observability requirements for tracing, correlation logging, and health monitoring.],
) <nfr-observability>

==== Reliability

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    inset: 6pt,

    table.header([*ID*], [*Requirement Target*]),

    [NFR-05a], [
      *Job Durability:* Execute background tasks via Hangfire @hangfire-docs backed by Redis @redis-docs storage to survive process restarts.
    ],
    [NFR-05b], [
      *Automated Maintenance:* Run daily cleanup routines to purge carts inactive for *7 days* and release associated inventory reservations.
    ],
    [NFR-05c], [
      *Webhook Idempotency:* Enforce idempotency key checks on Stripe webhook payloads to prevent duplicate transaction state updates.
    ],
    [NFR-05d], [
      *Reservation Timeouts:* Automatically expire unconfirmed checkout inventory holds after *15 minutes* of user inactivity.
    ],
  ),
  kind: table,
  caption: [Reliability constraints covering background durability, idempotency, and state timeouts.],
) <nfr-reliability>