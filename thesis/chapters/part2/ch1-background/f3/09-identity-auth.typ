=== Identity, Authentication, and Authorisation

- *ASP.NET Identity* provides user account management: password hashing with salted PBKDF2, email confirmation workflows, and optional two-factor authentication via TOTP @microsoft-aspnet-core.

- *JWT tokens.* Access tokens carry claims (user ID, roles, permissions) and expire after 15 minutes @jones2015jwt. Refresh tokens enable silent renewal: the client exchanges a refresh token for a new access token, and each refresh token is single-use. Reuse of an already-consumed refresh token triggers revocation of all tokens for that user, mitigating token theft.

- *Guest sessions.* Anonymous users receive a signed session cookie. This cookie links to a server-side session backed by Redis, enabling cart operations and product browsing without authentication. On registration or login, the guest session is transferred to the authenticated user context.

- *Permission model.* Authorisation uses granular claims in the format `domain:category:action` (e.g., `catalog:products:create`). Roles aggregate commonly used permission sets. Endpoint-level attributes evaluate claims at the middleware layer, so handler code contains no authorisation logic.
