=== Non-Functional Requirements

Beyond feature completeness, the system must satisfy quantitative and qualitative constraints that determine its fitness for production use. Five quality dimensions define these constraints.

==== Performance

The CBIR end-to-end search latency must remain under one second, encompassing image upload, embedding generation by the Python ML sidecar, vector database query via pgvector HNSW index, and result assembly. Non-search API endpoints must respond within 200 milliseconds under normal load. Asynchronous I/O handles concurrent requests without blocking threads.

Real-time visual search requires sub-second response to maintain user engagement. Studies show that search latency above one second measurably increases abandonment rates in e-commerce contexts @manning2008introduction. The one-second target is treated as a hard constraint: a model that produces superior accuracy but cannot meet this latency budget is unsuitable for production deployment in this architecture regardless of its retrieval quality.

==== Security

JWT access tokens carry user claims and expire after 15 minutes. Refresh tokens follow single-use rotation: each refresh operation invalidates the previous token and issues a new pair (access token and refresh token). Presenting a previously consumed refresh token triggers revocation of all tokens for that user, containing potential credential theft.

Role-based authorisation is enforced at the endpoint level. Rate limiting protects authentication endpoints: five requests per minute for login attempts, three per hour for new account registration. Security response headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) are applied to all HTTP responses. File upload validation verifies the magic byte signature of incoming images, checks the file extension against an allowlist, and enforces a 10 megabyte size limit.

Browser-based single-page applications are exposed to the open internet and must defend against common web threats. Short-lived tokens and refresh rotation limit the window of token compromise. File upload validation prevents malicious payloads from entering the embedding pipeline.

==== Modularity

Eight business modules share a single .NET assembly, separated by namespace convention with no direct cross-references at compile time. All inter-module communication occurs through MediatR in-process message dispatch rather than direct method calls. Each module can be tested independently, with its dependencies mocked at the MediatR boundary, without loading neighbouring modules or their database contexts.

The modular monolith pattern preserves the logical separation of microservice-style bounded contexts while avoiding the operational complexity of distributed systems: no service discovery, no inter-service authentication, no network serialisation overhead for intra-module calls. The MediatR dispatch boundary provides a clean seam: if a module later warrants extraction into a separate service, the handler interface remains unchanged and only the dispatcher implementation must be replaced.

==== Observability

OpenTelemetry distributed tracing spans both the .NET API and the Python ML sidecar. Trace context propagates through HTTP headers, linking a storefront search request to its embedding generation span in the Python service. Structured logging includes correlation identifiers on every log entry, enabling a single request to be followed across service boundaries in log aggregation tools.

Each service exposes a health check endpoint consumed by .NET Aspire for container orchestration. The health check reports the service's ability to reach its dependencies: the .NET API verifies connectivity to PostgreSQL and Redis; the Python ML sidecar reports the currently loaded model and last inference latency.

==== Reliability

Background jobs (cart expiry, embedding generation retries, index maintenance) persist in Redis-backed Hangfire storage, surviving application restarts without data loss. Payment webhooks from Stripe include idempotency keys that prevent duplicate processing when the gateway retries a delivery. Cart reservations expire after fifteen minutes of checkout inactivity, releasing held inventory automatically and preventing indefinite stock locking.

Many e-commerce operations are inherently long-running or time-delayed: cart expiry, payment confirmation, and index maintenance cannot complete within an HTTP request-response cycle. A durable job queue ensures these operations complete reliably even across process crashes, scheduled restarts, or deployment rollouts.

#v(0.5cm)
These non-functional requirements shaped architectural decisions throughout the system. The one-second CBIR latency target influenced the choice of a synchronous embedding pipeline rather than a queued approach; the modularity requirement led to the MediatR-based in-process dispatch model; and the reliability constraint motivated the choice of Hangfire with Redis-backed persistence for background jobs. Each target is revisited in the evaluation chapter, where the benchmark results confirm whether the implemented system meets these stated requirements.