=== Non-Functional Requirements

Beyond feature completeness, the system must satisfy quantitative and qualitative constraints that determine its fitness for production use. Five quality dimensions are specified in Table @tbl-nfr with concrete, measurable targets and the design rationale each constraint shaped.

#figure(
  table(
    columns: (auto, auto, 1fr, 1fr),
    stroke: 0.5pt,
    table.header([*ID*], [*Quality*], [*Target*], [*Rationale*]),

    [NFR-01], [Performance],
    [CBIR end-to-end search latency under *1 second* (image upload through embedding generation, pgvector query, and result assembly). Non-search API endpoints respond within *200 milliseconds* under normal load. Asynchronous I/O prevents thread blocking under concurrent requests.],
    [Real-time visual search requires sub-second response to maintain user engagement. Latency above one second measurably increases e-commerce abandonment. This target is treated as a *hard constraint*: a model exceeding the one-second budget is unsuitable for deployment regardless of retrieval quality. @manning2008introduction],

    [NFR-02], [Security],
    [*Authentication*: JWT access tokens expire after *15 minutes*. Refresh tokens follow *single-use rotation* with reuse detection; presenting a consumed token triggers *full revocation* of all tokens for that user. *Authorisation*: role-based access control with *granular permission claims* (`domain:category:action`) enforced at endpoint middleware. *Rate limiting*: *five requests per minute* for login, *three per hour* for registration. *Transport*: security headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) on all responses. *File upload*: *magic-byte verification*, extension allowlist, and *10 MB size limit*.],
    [Browser-based single-page applications are exposed to the open internet and must defend against common web threats. Short-lived tokens and refresh rotation limit the window of token compromise. File upload validation prevents malicious payloads from entering the embedding pipeline.],

    [NFR-03], [Modularity],
    [*Eight business modules* in a single .NET assembly, separated by namespace convention with *zero direct cross-references* at compile time. All inter-module communication occurs through *MediatR* in-process message dispatch. Each module is *independently testable* without loading neighbours or their database contexts.],
    [The modular monolith pattern preserves bounded-context separation without distributed-system overhead of service discovery, inter-service authentication, and network serialisation. The MediatR boundary provides a clean seam: extracting a module into a separate service requires replacing only the dispatcher, not the handlers or callers.],

    [NFR-04], [Observability],
    [*OpenTelemetry* distributed tracing spans both .NET and Python services. Trace context propagates through HTTP headers, linking a storefront search to its embedding generation span. *Correlation identifiers* on every structured log entry track requests across service boundaries. *Health check endpoints* report dependency connectivity for Aspire orchestration.],
    [In a polyglot architecture spanning C\# and Python, end-to-end request tracing is essential for diagnosing latency bottlenecks and error propagation. Correlation identifiers enable a single request to be followed across service boundaries in log aggregation tools.],

    [NFR-05], [Reliability],
    [*Hangfire* with *Redis-backed persistence* ensures background jobs survive application restarts. Cart expiry: daily job removes carts with *seven days* of inactivity, releasing reserved inventory. *Idempotency*: Stripe webhooks carry idempotency keys preventing duplicate payment processing. *Reservation timeout*: checkout inventory holds expire after *fifteen minutes* of inactivity.],
    [Many e-commerce operations are inherently long-running (cart expiry, payment confirmation, index maintenance) and cannot complete within an HTTP request-response cycle. A durable job queue ensures these operations complete reliably across process crashes, scheduled restarts, and deployment rollouts.],
  ),
  caption: [
    Non-functional requirements with measurable targets and design rationale. Each target is revisited in the evaluation chapter.
  ],
) <tbl-nfr>

These constraints shaped architectural decisions throughout the system. The one-second CBIR latency target ruled out queued embedding pipelines in favour of synchronous generation. The modularity requirement drove the MediatR dispatch model over direct method calls. The reliability constraint motivated Redis-backed Hangfire over in-memory job storage. Each target is revisited in the evaluation chapter, where benchmark results confirm whether the implemented system meets these stated requirements.
