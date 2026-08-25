# Chapter 8 — Security Design

## 8.1 Threat Model and Design Principles

**Security design follows defense-in-depth** with multiple independent controls at each layer:

| Layer | Controls |
|-------|----------|
| **Network** | CORS allow-list, rate limiting, security headers |
| **Authentication** | JWT with short expiry, refresh-token rotation, guest sessions |
| **Authorization** | Permission-based claims, dynamic policy provider |
| **Transport** | HTTPS only (Aspire local dev uses http for simplicity) |
| **Input** | FluentValidation, anti-forgery tokens, file upload guards |
| **Data** | EF Core parameterization (SQL injection prevention), no secrets in config |
| **Observability** | Sensitive header redaction, correlation IDs without PII |

**Evidence**: `appsettings.json:30-155`, `Shared/Security/`

## 8.1a Threat Model Approach

**Decision**: The thesis documents a **defense-in-depth controls table** (§8.1) rather than an exhaustive per-element STRIDE analysis. The rationale is that the security contribution of this thesis is architectural — explicit error handling, permission-based authorization, and JWT token design — rather than a dedicated security research contribution. A full STRIDE threat model across all 18 entity types would add ~5 pages without advancing the primary thesis argument.

**If required by examiner**: A structured STRIDE analysis for the three most critical elements (Order, PaymentIntent, JWT Token) is prepared and can be appended as a supplementary document. It maps Spoofing, Tampering, Repudiation, Info Disclosure, DoS, and Elevation threats to the existing mitigations and identifies one gap (synchronous Stripe webhook processing, already tracked in `CONCERNS.md`).

**Evidence**: `Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:33-88`, `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:32-36`, `Shared/Security/Authorization/Policies/Permission.PolicyProvider.cs:1-31`, `CONCERNS.md:§2`

## 8.2 Authentication Design

### 8.2.1 JWT Token Design

**Access Token**:
- Type: JWT signed with HS256
- Expiry: 15 minutes (`appsettings.json:35`)
- Claims: `sub` (user id), `email`, `jti` (token id), `iat`, `exp`
- Issuer/Audience: `ReSys.Shop`

**Refresh Token**:
- Stored in database (`UserToken` table with `TokenType = Refresh`)
- Expiry: 7 days
- **Rotation**: New refresh token issued on every access-token renewal; old token invalidated
- **Reuse detection**: If a previously-used refresh token is presented again, all tokens for that user are revoked (theft detection)
- **Blacklist**: Revoked tokens stored in Redis for fast lookup

**Design rationale**: Short access-token lifetime limits the window of compromise. Refresh-token rotation + reuse detection prevents replay attacks if a token is stolen. The blacklist in Redis ensures O(1) revocation checks.

**Evidence**: `appsettings.json:30-43`, `Shared/Security/Authentication/Tokens/Services/Refresh/`

### 8.2.2 Dev Secret Handling

A historical vulnerability (commit `770b6a06`) allowed hardcoded dev JWT secrets to work in non-Development environments. This was mitigated by:

1. Moving dev secrets to `dotnet user-secrets` (id `resys.shop.api`)
2. `JwtSettingsValidator` rejects known-dev literal secrets in non-Development environments
3. `setup-dev-secrets.sh` bootstraps dev secrets safely

**Evidence**: `appsettings.Development.json:1-2`, `Tokens.Extensions.cs:38-43`

### 8.2.3 External Identity Providers

- **Google OAuth**: Implemented using `Google.Apis.Auth`
- **Facebook / Microsoft**: Config placeholders exist but are disabled (`Enabled=false`); not implemented

**Evidence**: `appsettings.json:45-57`, `Shared/Security/Authentication/External/ExternalLogin.Extensions.cs`

## 8.3 Authorization Design

### 8.3.1 Permission-Based Authorization

The system uses a custom authorization model rather than simple role-based access control (RBAC):

1. **Permission descriptors** are defined in `PermissionContext` as:
   - `Domain` (e.g., `catalog`, `ordering`)
   - `Category` (e.g., `products`, `orders`)
   - `Action` (e.g., `create`, `read`, `update`, `delete`, `cancel`)

2. **Feature metadata** types (e.g., `CatalogFeature.Admin.Products.Create`) bundle route, tags, summary, and permission descriptor.

3. **Endpoint registration** calls `.HasPermission(CatalogPermission.AdminProductsCreate)` which creates a policy string like `catalog:products:create`.

4. **Policy provider** (`PermissionPolicyProvider`) resolves the string to a `PermissionRequirement`.

5. **Handler** checks if the user's claims contain the required permission.

**Design rationale**: RBAC is too coarse for e-commerce (an admin may edit products but not process refunds). Permission-based authorization gives granular control while keeping the policy provider centralized.

**Evidence**: `Shared/Security/Authorization/Registry/PermissionContext.cs:1-60`, `Permission.PolicyProvider.cs:1-31`

### 8.3.2 Admin vs Storefront Surface Segregation

Features are physically separated into `Features/Admin/` and `Features/Storefront/` folders. Endpoints in `Admin/` typically require `catalog:products:create`-style permissions, while `Storefront/` endpoints rely on authentication state (or guest session) without explicit permission checks for browsing.

**Evidence**: `Module/*/Features/Admin/` and `Module/*/Features/Storefront/` directory structures

## 8.4 Input Validation and Anti-Forgery

### 8.4.1 FluentValidation Pipeline

Every request DTO has a corresponding `Validator.cs` class. The `ValidationBehavior` runs all validators before the handler executes. Validation failures short-circuit the pipeline and return `Result.Validation` with field-level error codes.

**Evidence**: `Validation.Behavior.cs:1-67`, `CreateProduct.Validator.cs`

### 8.4.2 Anti-Forgery Tokens

Enabled by default (`appsettings.json:69-78`):
- Header name: `X-CSRF-TOKEN`
- Cookie: `.AspNetCore.Antiforgery`
- SameSite: `Strict`
- Secure: `Always` (requires HTTPS)
- Required: `true` (all mutating endpoints must include token)

**Design rationale**: The Storefront SPA uses JWT Bearer auth (stateless), so CSRF protection is less critical for API endpoints. The anti-forgery system primarily protects cookie-based flows (admin login, guest sessions).

**Evidence**: `appsettings.json:69-78`, `Shared/Security/AntiForgery/AntiForgery.Extensions.cs`

### 8.4.3 File Upload Security

Multi-layered defense for uploaded files:

| Layer | Check | Implementation |
|-------|-------|----------------|
| 1. Magic bytes | File header matches extension | `IStorageSecurityEnforcer` |
| 2. Extension allowlist | Only `.jpg`, `.png`, `.webp`, `.pdf`, etc. | `appsettings.json:135-138` |
| 3. Extension blocklist | `.exe`, `.bat`, `.ps1`, `.jar` blocked | `appsettings.json:139-142` |
| 4. Size limit | Max 10 MB | `appsettings.json:134` |
| 5. Anti-forgery guard | Rate-limit consecutive failures | `appsettings.json:146-149` |
| 6. Malware scan | ClamAV TCP scan (opt-in, disabled by default) | `appsettings.json:150-155` |

**Evidence**: `Shared/Operational/Storages/Storage.Extensions.cs:35-74`, `appsettings.json:129-155`

## 8.5 Rate Limiting

Named policies protect specific endpoint categories:

| Policy | Permit Limit | Window | Protected Endpoints |
|--------|-------------|--------|---------------------|
| `default` | 100 | 60s | All unclassified |
| `auth` | 5 | 60s | Login, token refresh |
| `register` | 3 | 3600s | Registration |
| `forgot-password` | 3 | 3600s | Password reset |
| `payment` | 30 | 60s | Payment intent creation |

**Design rationale**: Rate limiting prevents brute-force attacks on auth endpoints and reduces fraud risk on payment endpoints. The `payment` policy is higher (30/min) than auth because legitimate checkout flows may involve multiple payment attempts.

**Evidence**: `appsettings.json:79-86`, `Shared/Security/RateLimiting/RateLimit.Extensions.cs`

## 8.6 Security Headers

A custom middleware injects security headers on every response:

| Header | Value | Purpose |
|--------|-------|---------|
| `X-Content-Type-Options` | `nosniff` | Prevent MIME-type sniffing |
| `X-Frame-Options` | `DENY` | Clickjacking protection |
| `Content-Security-Policy` | `default-src 'self'` | XSS mitigation |
| `Strict-Transport-Security` | `max-age=31536000` | HSTS |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Privacy |

**Evidence**: `Shared/Security/Headers/SecurityHeadersMiddleware.cs`

## 8.7 Secrets Management

| Environment | Secret Store | Evidence |
|-------------|-------------|----------|
| Development | `dotnet user-secrets` (id `resys.shop.api`) | `Api.csproj:7`, `setup-dev-secrets.sh` |
| Testing | Hardcoded in `ApiFactory.cs` (self-contained) | `ApiFactory.cs:53,81-83` |
| Production | Environment variables (`__` delimiter) | `.env.template:5-33` |

**No secrets are committed** to the repository. All `appsettings.json` secret fields are empty strings (`""`).

**Evidence**: `appsettings.json` (empty secrets), `.env.template`

## 8.8 Evidence

- `service/Api/src/Api/appsettings.json:30-155` — security configuration
- `service/Api/src/Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:33-88` — JWT setup
- `service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs:1-60` — permission registry
- `service/Api/src/Shared/Security/RateLimiting/RateLimit.Extensions.cs` — rate limit policies
- `service/Api/src/Shared/Security/AntiForgery/AntiForgery.Extensions.cs` — anti-forgery setup
- `service/Api/src/Shared/Security/Headers/SecurityHeadersMiddleware.cs` — response headers
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:35-74` — upload security
- `service/Api/src/Api/.env.template:1-33` — required environment variables

---

## [ASK USER] Items

15. Should this chapter include a formal threat model (e.g., STRIDE-per-element), or is the layered controls table sufficient?
16. Is there a requirement for GDPR / privacy-by-design documentation (e.g., data retention, right to erasure via soft deletion)?
