=== Non-Functional Requirements

Five quality dimensions define constraints the system must satisfy to be fit for production use. Each requirement is specified as an atomic, measurable target. These constraints drove architecture decisions: the one-second CBIR target (NFR-01a) ruled out queued embedding pipelines in favour of synchronous generation, the modularity constraint (NFR-03b) drove the *MediatR* dispatch model, and the reliability constraint (NFR-05a) motivated *Redis*-backed *Hangfire* @hangfire-docs over in-memory job storage. Verification against each target is presented in Chapter 3.

==== Performance

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*]),
    [NFR-01a], [Complete CBIR end-to-end search (upload, embedding, pgvector query, result assembly) within *1 second* per query.],
    [NFR-01b], [Respond to non-search API endpoints within *200 milliseconds* under normal load.],
    [NFR-01c], [Use asynchronous I/O to prevent thread blocking under concurrent requests.],
  ),
    kind: table,
  caption: [Performance: latency targets for CBIR search and general API responses],
) <nfr-performance>

==== Security

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*]),
    [NFR-02a], [Expire *JWT* access tokens @jones2015jwt after *15 minutes* and enforce single-use refresh token rotation with reuse detection.],
    [NFR-02b], [Enforce role-based access control with granular permission claims (`domain:category:action`) at endpoint middleware.],
    [NFR-02c], [Rate-limit login to *5 requests per minute* and registration to *3 requests per hour*.],
    [NFR-02d], [Validate file uploads by magic-byte signature, extension allowlist, and enforce a *10 MB* size limit.],
    [NFR-02e], [Emit security headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) on all HTTP responses.],
  ),
    kind: table,
  caption: [Security: authentication, authorisation, rate limiting, upload validation, and transport controls],
) <nfr-security>

==== Modularity

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*]),
    [NFR-03a], [Nine business modules reside in a single .NET assembly with *zero direct cross-references* enforced at compile time.],
    [NFR-03b], [All inter-module communication passes through *MediatR* in-process message dispatch only.],
    [NFR-03c], [Each module must be independently testable without loading other modules or their database contexts.],
  ),
    kind: table,
  caption: [Modularity: module isolation, communication boundary, and testability],
) <nfr-modularity>

==== Observability

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*]),
    [NFR-04a], [Propagate *OpenTelemetry* distributed tracing spans across .NET and Python services via HTTP headers.],
    [NFR-04b], [Every structured log entry must carry a *correlation identifier* to track requests across service boundaries.],
    [NFR-04c], [Each service must expose a health check endpoint reporting dependency connectivity for orchestration.],
  ),
    kind: table,
  caption: [Observability: distributed tracing, correlation logging, and health monitoring],
) <nfr-observability>

==== Reliability

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*]),
    [NFR-05a], [Background jobs use *Hangfire* @hangfire-docs with *Redis* @redis-docs persistence to survive application restarts.],
    [NFR-05b], [Remove carts inactive for *seven days* and release reserved inventory, running daily.],
    [NFR-05c], [Stripe webhook handlers enforce idempotency keys to prevent duplicate payment processing.],
    [NFR-05d], [Checkout inventory holds expire after *fifteen minutes* of inactivity.],
  ),
    kind: table,
  caption: [Reliability: job durability, cart expiry, idempotency, and timeout guarantees],
) <nfr-reliability>
