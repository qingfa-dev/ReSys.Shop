== .NET Aspire Orchestration

*.NET Aspire* coordinates the multi-container development and deployment environment.

- *Service discovery.* Components resolve each other by name: the .NET backend reaches the Python sidecar at `http://ml-service`, PostgreSQL at `postgres`, and Redis at `redis`. Configuration is injected via environment variables managed by the Aspire host.

- *Lifecycle.* Aspire enforces startup ordering (database before API, sidecar before backend health check) and gates each component behind a readiness probe. Containers restart on failure with configurable back-off.

- *Observability.* OpenTelemetry SDKs in both .NET and Python services export distributed traces, request metrics, and structured logs to the Aspire dashboard. Correlation IDs propagate across the HTTP boundary between backend and sidecar, linking a search request to its embedding generation span.
