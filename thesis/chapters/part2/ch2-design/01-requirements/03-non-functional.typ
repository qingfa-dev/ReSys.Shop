=== Non-Functional Requirements

Five quality dimensions constrain the system beyond feature completeness. Each is stated with a concrete, measurable target and the design rationale that it shaped.

==== Performance: Sub-Second CBIR Latency

The end-to-end CBIR search must complete in *under one second*, spanning image upload, embedding generation by the Python ML sidecar, pgvector HNSW-indexed cosine distance query, and result assembly. Non-search API endpoints respond within *200 milliseconds* under normal load. Asynchronous I/O prevents thread blocking under concurrent requests.

Real-time search requires sub-second response to maintain engagement; latency above one second measurably increases e-commerce abandonment @manning2008introduction. This target is treated as a *hard constraint*: a model delivering superior accuracy but exceeding the one-second budget is unsuitable for deployment regardless of retrieval quality.

==== Security: Defence in Depth

*Authentication.* JWT access tokens expire after *15 minutes*. Refresh tokens follow *single-use rotation*: each refresh invalidates the previous token and issues a new pair. Presenting a consumed token triggers *full revocation* of all tokens for that user, containing credential theft.

*Authorisation.* Role-based access control augmented with *granular permission claims* (`domain:category:action`), enforced at the endpoint middleware layer.

*Rate limiting.* *Five requests per minute* for login; *three per hour* for registration.

*Transport.* Security headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) on all responses.

*File upload.* *Magic-byte verification*, extension allowlist enforcement, and *10 MB size limit* prevent malicious payloads from entering the embedding pipeline.

==== Modularity: Compile-Time Isolation

*Eight business modules* in a single .NET assembly, separated by namespace convention with *zero direct cross-references*. All inter-module communication occurs through *MediatR* in-process message dispatch. Each module is *independently testable* without loading neighbours or their database contexts.

This pattern preserves bounded-context separation without distributed-system overhead. The MediatR boundary provides a clean seam: lifting a module into a separate service requires replacing only the dispatcher implementation, not the handler interface or the callers.

==== Observability: End-to-End Traceability

*OpenTelemetry* distributed tracing spans both .NET and Python services. Trace context propagates through HTTP headers, linking a storefront search to its embedding generation span. *Correlation identifiers* on every structured log entry track requests across service boundaries.

*Health check endpoints* consumed by .NET Aspire report dependency connectivity: the .NET API verifies PostgreSQL and Redis reachability; the Python sidecar reports the loaded model and last inference latency.

==== Reliability: Durable Background Processing

*Hangfire* with *Redis-backed persistence* ensures background jobs survive application restarts without data loss.

*Cart expiry.* Daily scheduled job removes carts with *seven days* of inactivity, releasing reserved inventory.

*Idempotency.* Stripe webhooks carry idempotency keys preventing duplicate payment processing on gateway retry.

*Reservation timeout.* Checkout inventory holds expire after *fifteen minutes* of inactivity, preventing indefinite stock locking.

#v(0.5cm)
These constraints shaped architectural decisions throughout the system. The one-second latency target ruled out queued embedding pipelines in favour of synchronous generation. The modularity requirement drove the MediatR dispatch model over direct method calls. The reliability constraint motivated Redis-backed Hangfire over in-memory job storage. Each target is revisited in the evaluation chapter, where benchmark results confirm whether the implemented system meets these stated requirements.