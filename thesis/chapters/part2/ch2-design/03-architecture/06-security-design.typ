=== Security Design

The security framework operates across three layers: authentication, authorization, and defense-in-depth infrastructure.

==== Authentication and Session Management

JWT authentication is configured via `JwtSettings` with HS256 algorithm, 15-minute access token expiration, and 30-day maximum token age. Single-use refresh token rotation is enforced: exchanging an expired access token consumes the current refresh token and issues a new pair. Re-submitting a previously consumed refresh token triggers breach detection, immediately revoking all active refresh tokens for that user and forcing full re-authentication. Unauthenticated shoppers receive an HTTP-only cookie tracking an anonymous session ID; on login, the guest cart automatically merges with the user's persistent cart.

```csharp
// JWT configuration (JwtSettings)
public sealed class JwtSettings
{
    public string Secret { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public int AccessTokenExpirationInMinutes { get; init; } = 15;
    public int RefreshTokenExpirationInDays { get; init; }
    public string Algorithm { get; init; } = "HS256";
    // Token security
    public bool RotationEnabled { get; init; } = true;
    public bool ReuseDetectionEnabled { get; init; } = true;
    public bool SlidingExpirationEnabled { get; init; } = true;
    public int MaxTokenAgeDays { get; init; } = 30;
}
```

==== Dynamic Authorization

Role-Based Access Control separates administrative and storefront surfaces. Unprivileged accounts accessing `/api/*/admin/*` endpoints receive an immediate #raw("403", lang: "http") prior to command dispatch. The built-in Administrator role bypasses all permission checks.

A three-layer permission architecture resolves claims at runtime:

```csharp
// Permission resolution pipeline
// L1: IPermissionCache      - in-memory cache of resolved permissions per user
// L2: IPermissionService    - merges role-derived + direct-grant permissions
// L3: IPermissionStore      - EF Core persistence to Identity claims tables

// Custom policy provider resolves permission names dynamically
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (PermissionContext.IsKnown(policyName))
            return Task.FromResult(new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build());
        return FallbackProvider.GetPolicyAsync(policyName);
    }
}
```

Operations enforce granular resource-action claims. Ten `FeatureMetadata` files define the permission registry across modules, each mapping to the `PermissionContext` static catalogue. Permissions use the format `Domain.Category.Resource.Action`:

```text
catalog.products.create
catalog.products.update
catalog.variants.delete
identity.roles.manage
ordering.orders.approve
payment.intents.capture
inventory.stock.transfer
```

A custom `IAuthorizationPolicyProvider` resolves claim strings to policies dynamically at runtime, allowing permission modifications in the database without application redeployments.

==== System Hardening

Rate limiting restricts authentication to 5 requests per minute, registration to 3 per hour, and payment processing to 30 per minute. Security middleware injects #raw("Content-Security-Policy", lang: "http"), #raw("Strict-Transport-Security", lang: "http"), #raw("X-Frame-Options", lang: "http"), and #raw("X-Content-Type-Options", lang: "http") headers. Visual search uploads enforce a 10 MB limit and validate magic bytes to verify valid JPEG, PNG, or WebP formats. Stripe payment webhooks validate the `Stripe-Signature` header using HMAC signature verification before executing state transitions.

The storage service includes a pre-upload virus scanning layer via ClamAV (nClam integration) and image format validation through SkiaSharp, preventing malicious payloads from reaching persistent storage.
