=== Non-Functional Requirements

Beyond feature completeness, the system must satisfy quantitative and qualitative constraints that determine its fitness for production use. Five quality dimensions are specified below with atomic, measurable requirements.

==== Performance

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Requirement*]),
    [NFR-01a], [The system shall complete CBIR end-to-end search (image upload, embedding generation, pgvector query, result assembly) within *1 second* per query.],
    [NFR-01b], [The system shall respond to non-search API endpoints within *200 milliseconds* under normal load.],
    [NFR-01c], [The system shall use asynchronous I/O to prevent thread blocking under concurrent requests.],
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
    [NFR-02a], [The system shall expire JWT access tokens after *15 minutes* and enforce single-use refresh token rotation with reuse detection.],
    [NFR-02b], [The system shall enforce role-based access control with granular permission claims (`domain:category:action`) at endpoint middleware.],
    [NFR-02c], [The system shall rate-limit login to *5 requests per minute* and registration to *3 requests per hour*.],
    [NFR-02d], [The system shall validate file uploads by magic-byte signature, extension allowlist, and enforce a *10 MB* size limit.],
    [NFR-02e], [The system shall emit security headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) on all HTTP responses.],
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
    [NFR-03a], [Eight business modules shall reside in a single .NET assembly with *zero direct cross-references* enforced at compile time.],
    [NFR-03b], [All inter-module communication shall pass through *MediatR* in-process message dispatch only.],
    [NFR-03c], [Each module shall be independently testable without loading other modules or their database contexts.],
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
    [NFR-04a], [The system shall propagate *OpenTelemetry* distributed tracing spans across both .NET and Python services via HTTP headers.],
    [NFR-04b], [Every structured log entry shall carry a *correlation identifier* to track requests across service boundaries.],
    [NFR-04c], [Each service shall expose a health check endpoint reporting dependency connectivity for orchestration.],
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
    [NFR-05a], [Background jobs shall use *Hangfire* with Redis-backed persistence to survive application restarts.],
    [NFR-05b], [The system shall remove carts inactive for *seven days* and release reserved inventory, running daily.],
    [NFR-05c], [Stripe webhook handlers shall enforce idempotency keys to prevent duplicate payment processing.],
    [NFR-05d], [Checkout inventory holds shall expire after *fifteen minutes* of inactivity.],
  ),
    kind: table,
  caption: [Reliability: job durability, cart expiry, idempotency, and timeout guarantees],
) <nfr-reliability>

These constraints shaped architectural decisions throughout the system. The one-second CBIR latency target (NFR-01a) ruled out queued embedding pipelines in favour of synchronous generation. The modularity constraint (NFR-03b) drove the MediatR dispatch model over direct method calls. The reliability constraint (NFR-05a) motivated Redis-backed Hangfire over in-memory job storage. Each target is revisited in the evaluation chapter, where benchmark results confirm whether the implemented system meets these stated requirements.