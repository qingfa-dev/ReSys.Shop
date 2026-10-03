==== Service Orchestration & Observability

The backend operates as a distributed system where services are loosely coupled but highly observable.
- *Service Discovery:* Environment-based configuration manages connection strings for databases (PostgreSQL) and message brokers (Redis), ensuring seamless transitions between development and production environments.
- *Telemetry:* Comprehensive OpenTelemetry integration provides distributed tracing across the .NET API and Python ML Service, allowing full visibility into request latency and inter-service communication.

```cs
// Program.cs
// Service defaults configure OpenTelemetry, logging, and health checks
builder.AddServiceDefaults();
builder.AddPostgresHealthCheck("shopdb");
```
