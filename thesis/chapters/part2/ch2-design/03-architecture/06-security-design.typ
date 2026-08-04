=== Security Design

The ReSys.Shop security framework operates across three distinct operational layers: authentication, claim-based authorization, and defense-in-depth infrastructure.

==== Authentication and Session Management

- *Token Pair Generation:* Authenticated users (via email/password or Google OAuth) receive a 15-minute JWT access token and a server-stored refresh token in PostgreSQL.
- *Single-Use Rotation:* Exchanging an expired access token consumes the current refresh token and issues a new pair.
- *Breach Detection:* Re-submitting a previously consumed refresh token flags potential token interception, immediately revoking all active refresh tokens for that user ID and forcing full re-authentication.
- *Anonymous Guest Sessions:* Unauthenticated shoppers receive an HTTP-only cookie tracking an anonymous session ID. Upon login or registration, the guest cart automatically merges with the user's persistent cart.

==== Dynamic Authorization

- *Surface Isolation:* Role-Based Access Control (RBAC) separates administrative (`admin`) and customer (`storefront`) surfaces. Unprivileged accounts attempting to access `/api/*/admin/*` endpoints receive an immediate `403 Forbidden` response prior to command dispatch.
- *Granular Permissions:* Operations enforce fine-grained claims formatted as:

$ { "domain" } ":" { "category" } ":" { "action" } $

- *Runtime Resolution:* A custom `IAuthorizationPolicyProvider` maps claim strings (e.g., `catalog:products:create`) to policies dynamically. Permission assignments can be modified in the database without triggering application redeployments.

==== System Hardening and Defensive Controls

- *Rate Limiting:* Restricts IP traffic to 5 requests/min for authentication, 3 requests/hour for registration, and 30 requests/min for payment processing to mitigate brute-force attacks and abuse.
- *Security Headers:* Middleware injects `Content-Security-Policy`, `Strict-Transport-Security` (HSTS), `X-Frame-Options` (clickjacking protection), and `X-Content-Type-Options`.
- *File Upload Controls:* Visual search uploads enforce a 10 MB limit and inspect magic bytes directly to verify valid JPEG, PNG, or WebP image formats, bypassing spoofed client extensions.
- *Webhook Verification:* Stripe payment webhooks validate the `Stripe-Signature` header against the raw request body using HMAC signature verification before executing state transitions.

=== Chapter Summary

This chapter detailed the complete architectural framework of ReSys.Shop. The sections below summarize the core design decisions and their underlying technical rationale across all six architectural dimensions:

- *Service-Oriented Architecture:* The platform decouples concerns into three independent layers: a Vue 3 SPA presentation tier, a .NET 10 application engine, and a Python 3.12 FastAPI machine learning sidecar. This separation isolates heavy vector inference workloads from core transactional e-commerce workflows.
- *Domain-Driven Design (DDD):* Business logic is split across eight isolated bounded contexts (`Catalog`, `Ordering`, `Payment`, `Inventory`, `Identity`, `Profile`, `Shipping`, and `Location`). Communication relies on in-process MediatR CQRS dispatch, ensuring zero direct coupling between context databases while maintaining strong aggregate root invariants.
- *C4 Abstraction Modeling:* The system structure is defined across Context, Container, and Component levels, mapping deployable boundaries, asynchronous queue flows, and internal handler pipelines.
- *Database Architecture:* PostgreSQL manages both relational and vector data within per-context schemas. The platform uses `pgvector` for similarity queries, UUID primary keys for distributed generation, soft-deletion interceptors for data auditability, and PostgreSQL `xmin` columns for optimistic concurrency control.
- *RESTful API Contract:* Built on Carter minimal APIs, endpoints follow a uniform `/api/{module}/{surface}/{action}` route pattern. Pre-processor FluentValidation pipelines reject malformed inputs early, returning standard RFC 7807 Problem Details payloads.
- *Security Architecture:* System access relies on short-lived JWTs paired with refresh token rotation and reuse detection. Dynamic policy providers resolve string-based permissions at runtime, backed by defensive rate-limiting, magic-byte upload validation, and HMAC webhook verification.