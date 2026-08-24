== System Architecture & Design

The platform architecture covers six parts: how the services are organized, domain modelling, and the database, API, and security layers. The design follows a service-oriented approach: a Vue 3 frontend, a .NET 10 modular monolith backend, and a Python FastAPI machine learning sidecar, each independently deployable.

- *System Overview.* Three services, eight bounded contexts, technology stack summary.
- *Domain-Driven Design.* Context map, aggregate roots with invariants, state machines.
- *C4 Architecture.* Context, container, and component-level structural views.
- *Database Design.* Per-context schemas, pgvector integration, core design decisions.
- *API Design.* Carter minimal APIs, MediatR CQRS, endpoint conventions.
- *Security Design.* JWT authentication, permission-based authorisation, defensive hardening.

#include "01-system-overview.typ"
#include "02-domain-driven-design.typ"
#include "03-c4-architecture.typ"
#include "04-database-design.typ"
#include "05-api-design.typ"
#include "06-security-design.typ"
