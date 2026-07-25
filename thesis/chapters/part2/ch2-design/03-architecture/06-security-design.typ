=== Security Design

The security architecture of ReSys.Shop addresses three layers: authentication, verifying the identity of callers, authorisation, controlling what authenticated callers may do, and hardening, defensive measures against common attack vectors. This section describes each layer in turn.

==== Authentication

The platform uses JSON Web Tokens (JWT) for bearer token authentication. Upon successful login, via email and password or Google OAuth, the server issues two tokens: an access token with a fifteen-minute lifetime and a refresh token with a longer lifetime. The access token carries the user's identifier, email, and permission claims in a compact signed payload. All authenticated API requests include the access token in the `Authorization` header as a Bearer token.

The refresh token is a long-lived credential stored server-side in the database. When the access token expires, the client presents the refresh token to obtain a new access token and a new refresh token, a pattern known as refresh token rotation. Each refresh token is single-use: upon successful rotation, the consumed token is marked as used and a replacement is issued. If a previously consumed refresh token is presented again, indicating a potential token theft scenario, the system revokes all tokens for that user, forcing re-authentication. This rotation-with-reuse-detection pattern limits the damage window of a compromised refresh token to the interval between rotations.

Guest users, customers who have not yet authenticated, are assigned a session identifier stored in a browser cookie. This session identifier links their anonymous cart to their browsing context and persists across page navigations. Upon registration or login, the anonymous cart is merged with the authenticated user's cart, preserving the shopping intent built during the guest session.

==== Authorisation

Authorisation is implemented through two complementary mechanisms: role-based access control (RBAC) for broad category restrictions and permission-based claims for fine-grained control.

Roles, such as Customer and Administrator, segregate the Admin and Storefront surfaces. An endpoint in the Admin surface requires the Administrator role; a customer presenting valid credentials without that role receives a `403 Forbidden` response. This coarse check prevents unauthorised access to administrative functions at the infrastructure level, before any business logic executes.

Permissions use a structured claim format: `{domain}:{category}:{action}`. For example, `catalog:products:create` grants permission to create products in the Catalog domain. A dynamic permission provider, `IAuthorizationPolicyProvider`, resolves these claim strings to ASP.NET Core authorisation policies at runtime, eliminating the need for static policy registration for every endpoint. This dynamic resolution enables permission configuration through the database without redeployment: an administrator may create a new role, assign it a set of permission claims, and those permissions take effect across all authorised endpoints immediately.

==== Security Measures

Several defensive measures harden the platform against common web application attack vectors.

*Rate Limiting.* Authentication endpoints are rate-limited to five requests per minute per IP address to prevent credential brute-forcing. Registration endpoints are limited to three requests per hour per IP address to deter automated account creation. Payment endpoints are limited to thirty requests per minute to maintain availability during high-traffic checkout events.

*Security Headers.* All HTTP responses include security headers configured through ASP.NET Core middleware: Content-Security-Policy restricts script and style sources to the application's own domains, HTTP Strict-Transport-Security enforces HTTPS-only connections for a configurable duration, X-Frame-Options prevents the application from being embedded in iframes to block clickjacking, and X-Content-Type-Options prevents MIME-type sniffing by browsers.

*File Upload Validation.* The visual search and product image upload endpoints enforce strict file validation. Uploaded files undergo magic-byte verification, inspecting the file header bytes rather than trusting the file extension, to confirm they are valid JPEG, PNG, or WebP images. A ten-megabyte size limit prevents resource exhaustion from oversized uploads. Server-side validation repeats the client-side checks, as client-side validation is a convenience that an attacker can bypass.

*Payment Webhook Verification.* The Stripe webhook endpoint, which receives payment event notifications, validates each incoming request using Stripe's signature verification algorithm. The webhook payload is hashed with a shared signing secret; if the computed signature does not match the one provided in the Stripe-Signature header, the request is discarded before any business logic processes it. This verification prevents spoofed webhook payloads from injecting fraudulent payment state into the system.

==== Token Flow

The authentication token lifecycle operates as follows. A client authenticates with email and password, receiving an access token and a refresh token. The access token is short-lived and not stored server-side; it is validated by signature verification and expiration check on each request. When the access token expires, the client sends the refresh token to the refresh endpoint. The server validates the refresh token against the database: if it is valid and has not been used before, the server marks it as consumed, issues a new access token and a new refresh token, and returns both to the client. If the presented refresh token has already been consumed, flagged as used from a previous rotation, the server assumes token theft and revokes all refresh tokens associated with that user, logging the security event. The user must then re-authenticate, which invalidates the compromised token chain and issues fresh credentials. This model provides a self-healing defence against refresh token interception without requiring the user to detect or report the compromise.

=== Summary

This section has presented the architectural design of ReSys.Shop across six dimensions. The service-oriented system architecture separates presentation (Vue 3), business logic (.NET 10), and machine learning (Python sidecar) into independently deployable services. Domain-Driven Design partitions the business domain into eight bounded contexts communicating through MediatR in-process dispatch, with four architecturally significant aggregate roots enforcing explicit invariants. The C4 model describes the system at context, container, and component levels of abstraction, revealing the communication paths between deployable units and the internal composition of the .NET backend. The PostgreSQL database uses per-context schemas, pgvector for vector similarity search, and a set of consistent design decisions, GUIDs, soft deletion, audit columns, applied across all contexts. The API layer follows the URL convention `/api/{module}/{surface}/{action}` with Carter minimal APIs and MediatR CQRS. The security architecture covers JWT authentication with refresh token rotation, permission-based authorisation with dynamic policy resolution, and layered defensive measures against common attack vectors. Together, these architectural decisions provide the foundation on which the implementation described in the following section is built.
