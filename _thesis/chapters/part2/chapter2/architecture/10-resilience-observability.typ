=== System Reliability & Observability

==== Reliability & Resilience
- *Transactions:* All Commands execute within an atomic `BeginTransactionAsync` scope. If the ML service fails during an image upload, the database record is rolled back, ensuring consistency.
- *Concurrency:* `RowVersion` columns (Optimistic Concurrency) prevent "lost updates" on inventory. If two admins try to update the same stock simultaneously, the second one receives a `ConcurrencyException`.

==== Observability
The system is instrumented for comprehensive monitoring:
- *Tracing:* A unique `TraceId` follows requests from the Vue Frontend $->$ .NET API $->$ Python ML Service, simplifying debugging across languages.
- *Metrics:* Infrastructure metrics (CPU, Memory) and Application metrics (Search Latency, Order Rate) are captured for performance analysis.
