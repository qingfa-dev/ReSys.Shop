=== Non-Functional Requirements

Beyond feature completeness, the system must satisfy quantitative and qualitative constraints that determine its fitness for production use. Table @tbl-nfr summarises the non-functional requirements across five quality dimensions.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),

    table.header([*Quality*], [*Target*], [*Rationale*]),

    [Performance], [
      CBIR end-to-end search latency under 1 second (image upload through embedding generation, vector database query, and result assembly). Non-search API endpoints respond within 200 milliseconds under normal load. Asynchronous I/O handles concurrent requests without blocking threads.
    ], [
      Real-time visual search requires sub-second response to maintain user engagement. Studies show that search latency above one second measurably increases abandonment rates in e-commerce contexts @manning2008introduction.
    ],

    [Security], [
      JWT access tokens expire after 15 minutes; refresh tokens follow single-use rotation with reuse-detection invalidation. Role-based authorisation enforced per endpoint. Rate limiting on authentication endpoints (five requests per minute for login, three per hour for registration). Security response headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) on all HTTP responses. File upload validation: magic-byte verification, extension allowlist, and 10 megabyte size limit.
    ], [
      Browser-based single-page applications are exposed to the open internet and must defend against common web threats. Short-lived tokens and refresh rotation limit the window of token compromise. File upload validation prevents malicious payloads from entering the embedding pipeline.
    ],

    [Modularity], [
      Eight business modules in a single .NET assembly, separated by namespace convention with no direct cross-references. All inter-module communication occurs through MediatR in-process message dispatch. Each module independently testable without loading its neighbours.
    ], [
      The modular monolith pattern preserves the logical separation of microservices while avoiding distributed-system complexity. MediatR dispatch provides a clean integration point that can be replaced with an external message broker if modules are later extracted into separate services.
    ],

    [Observability], [
      OpenTelemetry distributed tracing across .NET API and Python ML sidecar, with trace context propagation through HTTP headers. Structured logging with correlation identifiers on every log entry. Health check endpoints for each service, consumed by .NET Aspire for container orchestration and restart decisions.
    ], [
      In a polyglot architecture spanning C\# and Python, end-to-end request tracing is essential for diagnosing latency bottlenecks and error propagation. Correlation identifiers enable a single request to be followed across service boundaries in log aggregators.
    ],

    [Reliability], [
      Background jobs (cart expiry, embedding generation retries, index maintenance) persist in Redis-backed Hangfire storage, surviving application restarts without data loss. Payment webhooks include idempotency keys that prevent duplicate processing on retry. Cart expiry triggers after fifteen minutes of inactivity, releasing reserved inventory automatically.
    ], [
      Many e-commerce operations are inherently long-running or time-delayed, cart expiry, payment confirmation, and index maintenance. A durable job queue ensures these operations complete reliably, even across process crashes or scheduled restarts.
    ],
  ),
  caption: [
    Non-functional requirements with concrete targets and design rationale.
  ],
) <tbl-nfr>

These non-functional requirements shaped architectural decisions throughout the system. The one-second CBIR latency target influenced the choice of a synchronous embedding pipeline rather than a queued approach; the modularity requirement led to the MediatR-based in-process dispatch model; and the reliability constraint motivated the choice of Hangfire with Redis-backed persistence for background jobs. Each target is revisited in the evaluation chapter, where the benchmark results confirm whether the implemented system meets these stated requirements.
