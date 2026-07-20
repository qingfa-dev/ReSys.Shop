# API Review Remediation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 15 code review findings across 6 clusters: infrastructure security, identity encoding bugs, payment idempotency, inventory concurrency, image upload security, and polish.

**Architecture:** 6 independent clusters sequenced by dependency (Cluster 1 must land first). Each cluster targets one subsystem, has its own test file, and can be reviewed independently. Clusters 2–6 are parallelizable after Cluster 1.

**Tech Stack:** .NET 10, C# 14, xUnit, FluentValidation, EF Core + Npgsql, Carter minimal APIs, PostgreSQL.

## Global Constraints

- All domain operations return `Result<T>` or `Result` — no exceptions for domain failures
- Feature files follow `static partial class` split across Handler, Request, Response, Endpoint, Validator
- Tests required for every code change — follow existing test patterns in `Module.UnitTests` or `Shared.UnitTests`
- Warnings-as-errors globally — no build warnings tolerated
- Constant values go in `{Entity}.Constant.cs`, error messages in `{Entity}.Result.cs`, validation extensions in `{Entity}.Validation.cs`

---

## Part A: Cluster 1 — Infrastructure/Security (land first)

### Task A1: Remove health check production gate

**Files:**
- Modify: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:118`

**Interfaces:**
- Consumes: `MapDefaultEndpoints` extension method on `WebApplication`
- Produces: health checks available in all environments

- [ ] **Step 1: Write the failing test**

Create `service/Api/tests/Shared.UnitTests/Infra/Aspire/ServiceDefaults/ExtensionsTests.cs`:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared.UnitTests.Infra.Aspire.ServiceDefaults;

public class ExtensionsTests
{
    [Fact(DisplayName = "MapDefaultEndpoints: registers health checks in all environments")]
    public void MapDefaultEndpoints_ShouldMapHealthChecks_InAllEnvironments()
    {
        var builder = WebApplication.CreateBuilder([]);
        builder.Environment.EnvironmentName = Environments.Production;
        var app = builder.Build();

        app.MapDefaultEndpoints();

        var dataSources = app.Services.GetRequiredService<IEnumerable<EndpointDataSource>>();
        var endpoints = dataSources.SelectMany(ds => ds.Endpoints).ToList();

        endpoints.Should().Contain(e => e.DisplayName!.Contains("/health"));
        endpoints.Should().Contain(e => e.DisplayName!.Contains("/alive"));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Shared.UnitTests --filter "FullyQualifiedName~MapDefaultEndpoints_ShouldMapHealthChecks_InAllEnvironments"
```
Expected: FAIL — health check endpoints not registered in Production.

- [ ] **Step 3: Implement the fix**

In `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:118`, remove the `if (!app.Environment.IsProduction())` guard:

```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    app.MapHealthChecks(HealthEndpointPath);

    app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });

    return app;
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test service/Api/tests/Shared.UnitTests --filter "FullyQualifiedName~MapDefaultEndpoints_ShouldMapHealthChecks_InAllEnvironments"
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs service/Api/tests/Shared.UnitTests/Infra/Aspire/ServiceDefaults/ExtensionsTests.cs
git commit -m "fix: remove production gate on health check endpoints"
```

---

### Task A2: Fix HSTS documentation claim

**Files:**
- Modify: `service/Api/src/Shared/Security/Headers/SecurityHeadersMiddleware.cs:8`

**Interfaces:**
- Consumes: none
- Produces: accurate XML doc describing emitted headers

- [ ] **Step 1: Write the test**

In `service/Api/tests/Shared.UnitTests/Security/Headers/SecurityHeadersMiddlewareTests.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.Security.Headers;
using Shared.Security.Headers.Options;

namespace Shared.UnitTests.Security.Headers;

public class SecurityHeadersMiddlewareTests
{
    [Fact(DisplayName = "Middleware: emits X-Content-Type-Options but NOT Strict-Transport-Security")]
    public async Task InvokeAsync_EmitsExpectedHeaders_NotHSTS()
    {
        var settings = Options.Create(new SecurityHeadersSetting
        {
            IsEnabled = true,
            XContentTypeOptions = "nosniff",
            XFrameOptions = "DENY",
            ContentSecurityPolicy = "default-src 'self'",
            ReferrerPolicy = "strict-origin-when-cross-origin",
            PermissionsPolicy = "camera=()"
        });
        var middleware = new SecurityHeadersMiddleware(next: ctx => Task.CompletedTask, settings);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        context.Response.Headers.Should().ContainKey("X-Frame-Options");
        context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        context.Response.Headers.Should().ContainKey("Referrer-Policy");
        context.Response.Headers.Should().ContainKey("Permissions-Policy");
        context.Response.Headers.Should().NotContainKey("Strict-Transport-Security");
    }
}
```

- [ ] **Step 2: Run test to verify it passes** (behavior is already correct)

```bash
dotnet test service/Api/tests/Shared.UnitTests --filter "FullyQualifiedName~InvokeAsync_EmitsExpectedHeaders_NotHSTS"
```
Expected: PASS.

- [ ] **Step 3: Fix the XML doc**

In `service/Api/src/Shared/Security/Headers/SecurityHeadersMiddleware.cs:8`, replace:

```csharp
/// <summary>Adds security headers (CSP, HSTS, X-Frame-Options, etc.) to every HTTP response.</summary>
```

With:

```csharp
/// <summary>Adds security headers (X-Content-Type-Options, X-Frame-Options, Content-Security-Policy, Referrer-Policy, Permissions-Policy) to every HTTP response. HSTS should be handled by the reverse proxy (Aspire/nginx) in production.</summary>
```

- [ ] **Step 4: Verify build passes**

```bash
dotnet build service/Api/src/Shared/Shared.csproj --no-restore
```
Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Shared/Security/Headers/SecurityHeadersMiddleware.cs service/Api/tests/Shared.UnitTests/Security/Headers/SecurityHeadersMiddlewareTests.cs
git commit -m "docs: fix HSTS claim in SecurityHeadersMiddleware, add header emission test"
```

---

### Task A3: Apply rate limiting to endpoints

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.Endpoint.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.Endpoint.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.Endpoint.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.Endpoint.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Endpoint.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`
- Modify: `service/Api/src/Api/Program.cs` (add `app.UseRateLimiter()`)

**Interfaces:**
- Consumes: named policies defined in `RateLimit.Extensions.cs`
- Produces: rate-limited endpoints returning 429 on excess

- [ ] **Step 1: Write the test**

In `service/Api/tests/Shared.UnitTests/Security/RateLimiting/RateLimitExtensionTests.cs`:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Security.RateLimiting;

namespace Shared.UnitTests.Security.RateLimiting;

public class RateLimitExtensionTests
{
    [Fact(DisplayName = "AddRateLimiting: registers 5 named policies")]
    public void AddRateLimiting_RegistersFivePolicies()
    {
        var builder = WebApplication.CreateBuilder([]);
        builder.AddRateLimiting();
        var app = builder.Build();

        var policies = app.Services.GetRequiredService<RateLimitingOptions>();
        policies.PolicyMap.Should().ContainKey("auth");
        policies.PolicyMap.Should().ContainKey("register");
        policies.PolicyMap.Should().ContainKey("forgot-password");
        policies.PolicyMap.Should().ContainKey("payment");
        policies.PolicyMap.Should().ContainKey("default");
    }
}
```

- [ ] **Step 2: Run test**

```bash
dotnet test service/Api/tests/Shared.UnitTests --filter "FullyQualifiedName~RateLimitExtension"
```
Expected: PASS (extension already registers policies — test proves it).

- [ ] **Step 3: Apply RequireRateLimiting to each endpoint**

For each of the 6 endpoint files, add `.RequireRateLimiting("policyName")` to the `.MapPost()` chain. Example for `PasswordLogin.Endpoint.cs`:

```csharp
app.MapPost(IdentityFeature.Store.Auth.Login.Password.Route, ...)
    .AllowAnonymous()
    .RequireRateLimiting("auth")
    .WithName(...)
```

Policy assignments:
| Endpoint | Policy |
|----------|--------|
| `PasswordLogin.Endpoint.cs` | `"auth"` |
| `ExternalAuthenticate.Endpoint.cs` | `"auth"` |
| `EmailRegister.Endpoint.cs` | `"register"` |
| `RequestPasswordReset.Endpoint.cs` | `"forgot-password"` |
| `CreatePaymentIntent.Endpoint.cs` | `"payment"` |
| `ConfirmPayment.Endpoint.cs` | `"payment"` |

In `Program.cs`, add `app.UseRateLimiter()` after `app.UseSecurityHeaders()` in the pipeline.

- [ ] **Step 4: Build and run tests**

```bash
dotnet build service/Api/src/Api/Api.csproj
dotnet test service/Api/tests/Shared.UnitTests --filter "FullyQualifiedName~RateLimit"
```
Expected: Build passes, tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.Endpoint.cs
git add service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.Endpoint.cs
git add service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.Endpoint.cs
git add service/Api/src/Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.Endpoint.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Endpoint.cs
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs
git add service/Api/src/Api/Program.cs
git add service/Api/tests/Shared.UnitTests/Security/RateLimiting/RateLimitExtensionTests.cs
git commit -m "fix: apply RequireRateLimiting to auth, register, forgot-password, payment endpoints"
```

---

### Task A4: Add CORS documentation comment

**Files:**
- Modify: `service/Api/src/Api/appsettings.json`

- [ ] **Step 1: Add comment in appsettings.json**

Above the `"Cors"` section, add:

```json
// Add frontend origins per environment (e.g. Cors__Origins:0=https://shop.example.com).
// See appsettings.Development.json for local dev values.
"Cors": {
```

- [ ] **Step 2: Commit**

```bash
git add service/Api/src/Api/appsettings.json
git commit -m "docs: add CORS origins configuration guidance to appsettings.json"
```

---

## Part B: Cluster 2 — Identity/Auth Encoding Bugs

### Task B1: Verify and test Base64Url decoder (already correct)

**Files:**
- Create: `service/Api/tests/Shared.UnitTests/Governance/Conventions/Base64ConverterTests.cs`

**Interfaces:**
- Consumes: `ToBase64Url`, `FromBase64Url`, `TryFromBase64Url` from `Base64Converter`
- Produces: test coverage proving the decoder is correct (review finding was a false positive)

**Note:** The original review flagged `Base64.Conveter.cs:99` as having swapped `.Replace()` args. Inspection shows line 99 is `.Replace("_", "/")` which is correct — `_` becomes `/` for base64url → standard base64 conversion. No code change needed. This task adds tests to prove correctness and prevent regression.

- [ ] **Step 1: Write the failing test**

Create `service/Api/tests/Shared.UnitTests/Governance/Conventions/Base64ConverterTests.cs`:

```csharp
using Shared.Governance.Conventions;

namespace Shared.UnitTests.Governance.Conventions;

public class Base64ConverterTests
{
    [Fact(DisplayName = "FromBase64Url: round-trips correctly with underscore in input")]
    public void FromBase64Url_RoundTrip_WithComplexInput()
    {
        var original = "test-data_with/special+chars";
        var encoded = original.ToBase64Url();
        var decoded = encoded.FromBase64Url();

        decoded.Should().Be(original);
    }

    [Fact(DisplayName = "ToBase64Url: produces URL-safe output without +/=")]
    public void ToBase64Url_ProducesNoSpecialChars()
    {
        var result = "hello world".ToBase64Url();
        result.Should().NotContain("+");
        result.Should().NotContain("/");
        result.Should().NotContain("=");
    }

    [Fact(DisplayName = "TryFromBase64Url: returns true for valid base64url input")]
    public void TryFromBase64Url_ValidInput_ReturnsTrue()
    {
        var encoded = "test-data".ToBase64Url();
        var success = encoded.TryFromBase64Url(out var decoded);

        success.Should().BeTrue();
        decoded.Should().Be("test-data");
    }
}
```

- [ ] **Step 2: Run tests to verify the decoder is correct**

```bash
dotnet test service/Api/tests/Shared.UnitTests --filter "FullyQualifiedName~Base64"
```
Expected: All PASS. The `FromBase64Url` method is already correct — the review finding was a false positive.

- [ ] **Step 3: Commit**

```bash
git add service/Api/tests/Shared.UnitTests/Governance/Conventions/Base64ConverterTests.cs
git commit -m "test: add Base64Url round-trip and encoding regression tests"
```

---

### Task B2: Fix ResendEmailVerification encoding — use Base64Url

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Resend/ResendEmailVerification.cs:86`

**Interfaces:**
- Consumes: `token.ToBase64Url()` (from Shared.Governance.Conventions)
- Produces: token correctly encoded as base64url for URL query params

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Resend/ResendEmailVerificationTests.cs`:

```csharp
using Shared.Governance.Conventions;
using Module.Identity.Features.Store.Emails.Resend;

namespace Module.UnitTests.Identity.Features.Store.Emails.Resend;

public class ResendEmailVerificationTests
{
    [Fact(DisplayName = "BuildVerificationPath: encodes token with base64url, decodable by ConfirmEmail")]
    public void BuildVerificationPath_EncodesBase64Url_Decodable()
    {
        var userId = Guid.NewGuid();
        var rawToken = "test-token-with/special+chars";

        var path = ResendEmailVerification.BuildVerificationPath(userId, rawToken);

        var tokenFromUrl = ExtractQueryParam(path, "token");
        var success = tokenFromUrl.TryFromBase64Url(out var decoded);
        success.Should().BeTrue();
        decoded.Should().Be(rawToken);
    }

    [Fact(DisplayName = "BuildVerificationPath: does not contain URL-unsafe characters")]
    public void BuildVerificationPath_NoUnsafeURlChars()
    {
        var path = ResendEmailVerification.BuildVerificationPath(Guid.NewGuid(), "test");

        path.Should().NotContain("+");
        path.Should().NotContain("/");
        path.Should().NotContain("=");
    }

    private static string ExtractQueryParam(string url, string param)
    {
        var query = url[(url.IndexOf('?') + 1)..];
        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts[0] == param) return parts[1];
        }
        return string.Empty;
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ResendEmailVerification"
```
Expected: `BuildVerificationPath_EncodesBase64Url_Decodable` FAILS — `.ToBase64()` produces `+/=` which `TryFromBase64Url` rejects.

- [ ] **Step 3: Fix the encoding**

In `service/Api/src/Module/Identity/Features/Store/Emails/Resend/ResendEmailVerification.cs:86`, replace:

```csharp
var encodedToken = token.ToBase64();
```

With:

```csharp
var encodedToken = token.ToBase64Url();
```

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ResendEmailVerification"
```
Expected: All PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Emails/Resend/ResendEmailVerification.cs
git add service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Resend/ResendEmailVerificationTests.cs
git commit -m "fix: use ToBase64Url in ResendEmailVerification to match ConfirmEmail decoder"
```

---

### Task B3: Fix ChangeEmail encoding — use Base64Url instead of Uri.EscapeDataString

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Change/ChangeEmail.cs:91`

**Interfaces:**
- Consumes: `token.ToBase64Url()`
- Produces: token and email correctly encoded as base64url

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Change/ChangeEmailTests.cs`:

```csharp
using Shared.Governance.Conventions;
using Module.Identity.Features.Store.Emails.Change;

namespace Module.UnitTests.Identity.Features.Store.Emails.Change;

public class ChangeEmailTests
{
    [Fact(DisplayName = "BuildConfirmPath: encodes token and email with base64url")]
    public void BuildConfirmPath_EncodesBase64Url_DecodableByConfirmEmail()
    {
        var userId = Guid.NewGuid();
        var rawToken = "changetoken+/=";
        var rawEmail = "user@example.com";

        var path = ChangeEmail.BuildConfirmPath(userId, rawToken, rawEmail);

        var tokenFromUrl = ExtractQueryParam(path, "token");
        var emailFromUrl = ExtractQueryParam(path, "newEmail");

        var tokenOk = tokenFromUrl.TryFromBase64Url(out var decodedToken);
        var emailOk = emailFromUrl.TryFromBase64Url(out var decodedEmail);

        tokenOk.Should().BeTrue();
        emailOk.Should().BeTrue();
        decodedToken.Should().Be(rawToken);
        decodedEmail.Should().Be(rawEmail);
    }

    private static string ExtractQueryParam(string url, string param)
    {
        var query = url[(url.IndexOf('?') + 1)..];
        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts[0] == param) return Uri.UnescapeDataString(parts[1]);
        }
        return string.Empty;
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ChangeEmailTests"
```
Expected: FAIL — `Uri.EscapeDataString` output is not decodable by `TryFromBase64Url`.

- [ ] **Step 3: Fix the encoding**

In `service/Api/src/Module/Identity/Features/Store/Emails/Change/ChangeEmail.cs:91`, replace:

```csharp
var encodedToken = Uri.EscapeDataString(token);
var encodedEmail = Uri.EscapeDataString(newEmail);
```

With:

```csharp
var encodedToken = token.ToBase64Url();
var encodedEmail = newEmail.ToBase64Url();
```

Also remove the `Uri.UnescapeDataString` call in the test's `ExtractQueryParam` (base64url doesn't need URL unescaping).

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ChangeEmail"
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Emails/Change/ChangeEmail.cs
git add service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Change/ChangeEmailTests.cs
git commit -m "fix: use ToBase64Url in ChangeEmail.BuildConfirmPath to match ConfirmEmail decoder"
```

---

### Task B4: Add UserId validation to ConfirmEmail Validator

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.Validator.cs`

**Interfaces:**
- Consumes: FluentValidation `RuleFor`
- Produces: `UserId` must not be empty

- [ ] **Step 1: Write the failing test**

In `service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Confirm/ConfirmEmailValidatorTests.cs`:

```csharp
using Module.Identity.Features.Store.Emails.Confirm;

namespace Module.UnitTests.Identity.Features.Store.Emails.Confirm;

public class ConfirmEmailValidatorTests
{
    [Fact(DisplayName = "Validator: rejects empty UserId")]
    public void Validator_EmptyUserId_ReturnsError()
    {
        var validator = new ConfirmEmail.Validator();
        var command = new ConfirmEmail.Command(new ConfirmEmail.Request
        {
            UserId = Guid.Empty,
            Token = "valid-token"
        });

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.UserId);
    }

    [Fact(DisplayName = "Validator: accepts valid UserId and Token")]
    public void Validator_ValidInput_Passes()
    {
        var validator = new ConfirmEmail.Validator();
        var command = new ConfirmEmail.Command(new ConfirmEmail.Request
        {
            UserId = Guid.NewGuid(),
            Token = "valid-token"
        });

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ConfirmEmailValidator"
```
Expected: `Validator_EmptyUserId_ReturnsError` FAILS — no `UserId` rule exists.

- [ ] **Step 3: Add the validation rule**

In `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.Validator.cs`, add:

```csharp
public Validator()
{
    RuleFor(x => x.Request.UserId).NotEmpty();
    RuleFor(x => x.Request.Token).ApplyUserTokenRules();
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ConfirmEmailValidator"
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.Validator.cs
git add service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Confirm/ConfirmEmailValidatorTests.cs
git commit -m "fix: add UserId NotEmpty validation to ConfirmEmail validator"
```

---

## Part C: Cluster 3 — Payment Idempotency & Ordering

### Task C1: Add terminal-state guards to webhook handlers

**Files:**
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:92,112,140,162`

**Interfaces:**
- Consumes: `payment.State` (PaymentRecordState enum)
- Produces: idempotent handlers that skip when payment is in a terminal state

- [ ] **Step 1: Write the tests**

In `service/Api/tests/Module.UnitTests/Payment/Backgrounds/ProcessStripeWebhookEventJobTests.cs`, add:

```csharp
[Fact(DisplayName = "HandlePaymentIntentFailed: skips when payment already Failed")]
public async Task HandlePaymentIntentFailed_AlreadyFailed_Skips()
{
    var payment = CreatePayment(PaymentRecordState.Failed);
    SetupPaymentLookup(payment);
    var job = CreateJob();

    await job.ExecuteAsync(BuildPayload(GatewayConstants.WebhookEvents.Stripe.PaymentIntentFailed));

    VerifyPaymentNotMutated(payment);
}

[Fact(DisplayName = "HandleChargeRefunded: skips when payment already Refunded")]
public async Task HandleChargeRefunded_AlreadyRefunded_Skips()
{
    var payment = CreatePayment(PaymentRecordState.Refunded);
    SetupPaymentLookup(payment);
    var job = CreateJob();

    await job.ExecuteAsync(BuildPayload(GatewayConstants.WebhookEvents.Stripe.ChargeRefunded));

    VerifyPaymentNotMutated(payment);
}

[Fact(DisplayName = "HandleChargeDisputeCreated: skips when payment already Disputed")]
public async Task HandleChargeDisputeCreated_AlreadyDisputed_Skips()
{
    var payment = CreatePayment(PaymentRecordState.Disputed);
    SetupPaymentLookup(payment);
    var job = CreateJob();

    await job.ExecuteAsync(BuildPayload(GatewayConstants.WebhookEvents.Stripe.ChargeDisputeCreated));

    VerifyPaymentNotMutated(payment);
}

[Fact(DisplayName = "HandlePaymentIntentCanceled: skips when payment already Voided")]
public async Task HandlePaymentIntentCanceled_AlreadyVoided_Skips()
{
    var payment = CreatePayment(PaymentRecordState.Voided);
    SetupPaymentLookup(payment);
    var job = CreateJob();

    await job.ExecuteAsync(BuildPayload(GatewayConstants.WebhookEvents.Stripe.PaymentIntentCanceled));

    VerifyPaymentNotMutated(payment);
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ProcessStripeWebhookEventJob"
```
Expected: The 4 new tests FAIL — handlers attempt state transitions on already-terminal payments.

- [ ] **Step 3: Add guards**

In `ProcessStripeWebhookEventJob.cs`:

At `HandlePaymentIntentFailed` (line 99, after the null check):
```csharp
if (payment.State is PaymentRecordState.Failed or PaymentRecordState.Voided) return;
```

At `HandleChargeRefunded` (line 119, after the null check):
```csharp
if (payment.State is PaymentRecordState.Refunded or PaymentRecordState.Voided) return;
```

At `HandleChargeDisputeCreated` (line 148, after the null check):
```csharp
if (payment.State is PaymentRecordState.Disputed) return;
```

At `HandlePaymentIntentCanceled` (line 169, after the null check):
```csharp
if (payment.State is PaymentRecordState.Canceled or PaymentRecordState.Voided) return;
```

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ProcessStripeWebhookEventJob"
```
Expected: All PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
git add service/Api/tests/Module.UnitTests/Payment/Backgrounds/ProcessStripeWebhookEventJobTests.cs
git commit -m "fix: add terminal-state guards to Stripe webhook handlers for idempotency"
```

---

### Task C2: Fix payment persist-before-gateway ordering

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:56-82`

**Interfaces:**
- Consumes: `IPaymentProcessingService.ProcessAsync`
- Produces: `PaymentCapture` persisted only after gateway success

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`, add:

```csharp
[Fact(DisplayName = "Handler: does NOT persist PaymentCapture when gateway call fails")]
public async Task Handle_GatewayFails_NoPaymentPersisted()
{
    var order = CreateOrder();
    var paymentMethod = CreatePaymentMethod();
    SetupOrderLookup(order);
    SetupPaymentMethodLookup(paymentMethod);
    SetupGatewayThatThrows();

    var handler = CreateHandler();
    var command = new CreatePaymentIntent.Command(order.Id, paymentMethod.Id);
    var result = await handler.Handle(command, CancellationToken.None);

    result.IsFailure.Should().BeTrue();
    VerifyPaymentNotAddedToContext();
}
```

- [ ] **Step 2: Run test to see it fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Handle_GatewayFails_NoPaymentPersisted"
```
Expected: FAIL — payment is added before the gateway call, so it persists even on failure.

- [ ] **Step 3: Reorder the code**

In `CreatePaymentIntent.cs`, move the `Add`+`SaveChangesAsync` pair from lines 57-58 to after the gateway success check at line 80:

```csharp
// REMOVE from lines 57-58:
// dbContext.Set<PaymentCapture>().Add(payment);
// await dbContext.SaveChangesAsync(cancellationToken);

// ADD after line 80 (processResult.IsFailure check):
// Persist now that the gateway call succeeded
dbContext.Set<PaymentCapture>().Add(payment);
await dbContext.SaveChangesAsync(cancellationToken);
```

But wait — the `payment` variable is used at line 79 (`processingService.ProcessAsync(payment, ...)`) — the gateway call needs the entity to be tracked for state updates. The entity must be added to the change tracker, just not yet saved.

Revised: keep `dbContext.Set<PaymentCapture>().Add(payment);` at line 57 but remove `await dbContext.SaveChangesAsync(cancellationToken);`. Then add `await dbContext.SaveChangesAsync(cancellationToken);` after the gateway call succeeds (line 82 is already there, which handles the gateway's changes — but the initial ADD needs to be saved too).

Actually, looking at the code more carefully: at line 57-58 the entity is added AND saved (so it has a database-generated ID). Then at line 79 the gateway mutates the entity. Then at line 82 it's saved again. The fix is to defer the FIRST SaveChangesAsync:

Replace lines 57-58:
```csharp
var payment = createResult.Value;
dbContext.Set<PaymentCapture>().Add(payment);
await dbContext.SaveChangesAsync(cancellationToken);
```

With:
```csharp
var payment = createResult.Value;
dbContext.Set<PaymentCapture>().Add(payment);
// Defer: only persist after gateway call succeeds to avoid orphaned records
```

Then move the first save to after line 82:
```csharp
await dbContext.SaveChangesAsync(cancellationToken);
```

Wait, line 82 already has `await dbContext.SaveChangesAsync(cancellationToken);`. If we add the entity at line 57 but don't save, then the gateway call at line 79 mutates payment, and line 82 saves everything together. That works — just remove line 58's `SaveChangesAsync`.

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreatePaymentIntent"
```
Expected: All PASS (including new test).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git add service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "fix: defer payment capture persistence until after gateway call succeeds"
```

---

### Task C3: Document CancellationToken.None in StripeWebhook

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:28`

- [ ] **Step 1: Add the comment**

Replace line 28:
```csharp
job => job.ExecuteAsync(command.Payload, CancellationToken.None));
```

With:
```csharp
// CancellationToken.None is a serialization placeholder — Hangfire injects the real token at execution time
job => job.ExecuteAsync(command.Payload, CancellationToken.None));
```

- [ ] **Step 2: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs
git commit -m "docs: explain CancellationToken.None in StripeWebhook Hangfire enqueue"
```

---

## Part D: Cluster 4 — Ordering/Inventory Concurrency

### Task D1: Use domain Pick() instead of raw stock mutation + add retry

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs:90-157`

**Interfaces:**
- Consumes: `si.Pick(take)` from `StockItem.Method.Adjustment.cs`
- Produces: validated stock deduction via domain method, retry on concurrency conflict

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs`, add:

```csharp
[Fact(DisplayName = "Handler: retries on DbUpdateConcurrencyException up to 3 times")]
public async Task Handle_ConcurrencyConflict_RetriesThreeTimes()
{
    var attempts = 0;
    SetupCart();
    MockDbContextSaveChanges(() =>
    {
        attempts++;
        if (attempts < 3) throw new DbUpdateConcurrencyException();
    });

    var handler = CreateHandler();
    var result = await handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), CancellationToken.None);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Code.Should().Be("StockItem.ConcurrencyConflict");
    attempts.Should().Be(3);
}

[Fact(DisplayName = "Handler: uses Pick() domain method for stock deduction")]
public async Task Handle_StockDeduction_UsesPickDomainMethod()
{
    SetupCart();
    var mockStockItem = CreateMockStockItem(countOnHand: 10);
    SetupStockItemLookup(mockStockItem);

    var handler = CreateHandler();
    var result = await handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), CancellationToken.None);

    mockStockItem.CountOnHand.Should().BeLessThan(10); // Pick() was called
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateOrderFromCart"
```
Expected: New tests FAIL — no retry logic, no `Pick()` usage.

- [ ] **Step 3: Implement the fix**

Replace lines 116-118:
```csharp
si.CountOnHand -= take;
si.ModifiedAtUtc = DateTimeOffset.UtcNow;
```

With:
```csharp
var pickResult = si.Pick(take);
if (pickResult.IsFailure) return pickResult.Errors;
```

Wrap the transaction block (lines 90-157) in a retry loop:

```csharp
for (int attempt = 0; ; attempt++)
{
    await using var transaction = await dbContext.BeginTransactionAsync(
        IsolationLevel.RepeatableRead, cancellationToken);
    try
    {
        // ... existing transaction body (order number, place, stock loop, save) ...

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException) when (attempt < 2)
        {
            await transaction.RollbackAsync(cancellationToken);
            await Task.Delay(100 * (1 << attempt), cancellationToken);
            continue;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return StockItemResult.Errors.ConcurrencyConflict(
                cart.LineItems.First().VariantId);
        }

        await transaction.CommitAsync(cancellationToken);
        break;
    }
    catch (Exception) when (attempt < 2)
    {
        // Retry on other transient failures too
        continue;
    }
}
```

- [ ] **Step 4: Build and run tests**

```bash
dotnet build service/Api/src/Module/Module.csproj
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateOrderFromCart"
```
Expected: All PASS.

- [ ] **Step 5: Verify integration test still passes**

```bash
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~CheckoutConcurrency"
```
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs
git commit -m "fix: use domain Pick() for stock deduction, add retry on concurrency conflict"
```

---

### Task D2: Change cart reservation isolation from Serializable to RepeatableRead

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs:34`

**Interfaces:**
- Consumes: `IsolationLevel` enum
- Produces: lower contention for concurrent cart reservations

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Tests.cs`, add:

```csharp
[Fact(DisplayName = "Handler: uses RepeatableRead isolation for cart reservation transaction")]
public async Task Handle_UsesRepeatableRead()
{
    IsolationLevel? capturedLevel = null;
    MockDbContextBeginTransaction((level, _) => { capturedLevel = level; return CreateMockTransaction(); });

    var handler = CreateHandler();
    var command = new ReserveCartStock.Command(new ReserveCartStock.Request
    {
        VariantId = Guid.NewGuid(),
        Quantity = 3,
        StockLocationId = Guid.NewGuid(),
        CartToken = "test-cart",
        TtlMinutes = 15
    });

    await handler.Handle(command, CancellationToken.None);

    capturedLevel.Should().Be(IsolationLevel.RepeatableRead);
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Handle_UsesRepeatableRead"
```
Expected: FAIL — current code uses `IsolationLevel.Serializable`.

- [ ] **Step 3: Change the isolation level**

In `ReserveCartStock.cs:34`, replace:
```csharp
IsolationLevel.Serializable, cancellationToken);
```

With:
```csharp
IsolationLevel.RepeatableRead, cancellationToken);
```

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ReserveCartStock"
```
Expected: All PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs
git add service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Tests.cs
git commit -m "fix: use RepeatableRead instead of Serializable for cart stock reservations"
```

---

### Task D3: Add constants, Result errors, validation, and validator for ReserveCartStock

**Files:**
- Modify: `service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Constant.cs`
- Modify: `service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Result.cs`
- Modify: `service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Validation.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Validator.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs`

- [ ] **Step 1: Write the tests**

In `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Tests.cs`, add:

```csharp
[Fact(DisplayName = "Validator: rejects null StockLocationId")]
public void Validator_NullStockLocationId_ReturnsError()
{
    var validator = new ReserveCartStock.Validator();
    var command = new ReserveCartStock.Command(new ReserveCartStock.Request
    {
        VariantId = Guid.NewGuid(),
        Quantity = 1,
        StockLocationId = null,
        CartToken = "cart-1",
        TtlMinutes = 15
    });

    var result = validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Request.StockLocationId);
}

[Fact(DisplayName = "Validator: rejects zero Quantity")]
public void Validator_ZeroQuantity_ReturnsError()
{
    var validator = new ReserveCartStock.Validator();
    var command = new ReserveCartStock.Command(new ReserveCartStock.Request
    {
        VariantId = Guid.NewGuid(),
        Quantity = 0,
        StockLocationId = Guid.NewGuid(),
        CartToken = "cart-1",
        TtlMinutes = 15
    });

    var result = validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Request.Quantity);
}

[Fact(DisplayName = "Validator: rejects TTL below minimum")]
public void Validator_TtlBelowMin_ReturnsError()
{
    var validator = new ReserveCartStock.Validator();
    var command = new ReserveCartStock.Command(new ReserveCartStock.Request
    {
        VariantId = Guid.NewGuid(),
        Quantity = 1,
        StockLocationId = Guid.NewGuid(),
        CartToken = "cart-1",
        TtlMinutes = 0
    });

    var result = validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Request.TtlMinutes);
}

[Fact(DisplayName = "Validator: accepts valid input")]
public void Validator_ValidInput_Passes()
{
    var validator = new ReserveCartStock.Validator();
    var command = new ReserveCartStock.Command(new ReserveCartStock.Request
    {
        VariantId = Guid.NewGuid(),
        Quantity = 3,
        StockLocationId = Guid.NewGuid(),
        CartToken = "cart-1",
        TtlMinutes = 15
    });

    var result = validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
}
```

In `service/Api/tests/Module.UnitTests/Inventory/Domain/StockReservations/StockReservationResultTests.cs`:

```csharp
namespace Module.UnitTests.Inventory.Domain.StockReservations;

public class StockReservationResultTests
{
    [Fact(DisplayName = "StockLocationRequired: returns validation error with correct code")]
    public void StockLocationRequired_HasCorrectCode()
    {
        var error = StockReservationResult.Errors.StockLocationRequired;
        error.Code.Should().Be("StockReservation.Cart.StockLocationRequired");
        error.Type.Should().Be(400);
    }

    [Fact(DisplayName = "CartTokenRequired: returns validation error")]
    public void CartTokenRequired_HasCorrectCode()
    {
        var error = StockReservationResult.Errors.CartTokenRequired;
        error.Code.Should().Be("StockReservation.Cart.CartTokenRequired");
    }

    [Fact(DisplayName = "TtlOutOfRange: returns validation error referencing constant values")]
    public void TtlOutOfRange_ReferencesConstantValues()
    {
        var error = StockReservationResult.Errors.TtlOutOfRange;
        error.Message.Should().Contain(StockReservationConstant.Defaults.MinTtlMinutes.ToString());
        error.Message.Should().Contain(StockReservationConstant.Defaults.MaxTtlMinutes.ToString());
    }
}
```

- [ ] **Step 2: Run tests to see them fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ReserveCartStock"
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~StockReservationResult"
```
Expected: All FAIL — errors and constants don't exist yet.

- [ ] **Step 3: Add constants**

In `StockReservation.Constant.cs`, add to `Defaults`:
```csharp
public const int MaxTtlMinutes = 10080;     // 7 days
public const int MinTtlMinutes = 1;
```

- [ ] **Step 4: Add Result errors**

In `StockReservation.Result.cs`, add to `Errors`:
```csharp
public static Error StockLocationRequired => Error.Validation(
    code: "StockReservation.Cart.StockLocationRequired",
    message: "Stock location is required for cart reservation.");

public static Error CartTokenRequired => Error.Validation(
    code: "StockReservation.Cart.CartTokenRequired",
    message: "Cart token is required for cart reservation.");

public static Error TtlOutOfRange => Error.Validation(
    code: "StockReservation.Cart.TtlOutOfRange",
    message: $"TTL minutes must be between {StockReservationConstant.Defaults.MinTtlMinutes} and {StockReservationConstant.Defaults.MaxTtlMinutes}.");
```

- [ ] **Step 5: Add validation extensions**

In `StockReservation.Validation.cs`, add:
```csharp
public static IRuleBuilderOptions<T, Guid?> ApplyStockLocationRequired<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
{
    return ruleBuilder
        .NotEmpty()
        .WithErrorCode(StockReservationResult.Errors.StockLocationRequired.Code)
        .WithMessage(StockReservationResult.Errors.StockLocationRequired.Message);
}

public static IRuleBuilderOptions<T, int> ApplyTtlRangeRules<T>(this IRuleBuilder<T, int> ruleBuilder)
{
    return ruleBuilder
        .InclusiveBetween(StockReservationConstant.Defaults.MinTtlMinutes, StockReservationConstant.Defaults.MaxTtlMinutes)
        .WithErrorCode(StockReservationResult.Errors.TtlOutOfRange.Code)
        .WithMessage(StockReservationResult.Errors.TtlOutOfRange.Message);
}
```

- [ ] **Step 6: Create the validator**

Create `ReserveCartStock.Validator.cs`:
```csharp
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.StockLocationId).ApplyStockLocationRequired();
            RuleFor(x => x.Request.CartToken).NotEmpty()
                .WithErrorCode(StockReservationResult.Errors.CartTokenRequired.Code)
                .WithMessage(StockReservationResult.Errors.CartTokenRequired.Message);
            RuleFor(x => x.Request.VariantId).NotEmpty();
            RuleFor(x => x.Request.Quantity).ApplyQuantityRules();
            RuleFor(x => x.Request.TtlMinutes).ApplyTtlRangeRules();
        }
    }
}
```

- [ ] **Step 7: Remove inline validation from handler**

In `ReserveCartStock.cs`, remove lines 30-31:
```csharp
if (quantity <= 0)
    return StockReservationResult.Errors.QuantityZero;
```

And replace line 26:
```csharp
var stockLocationId = command.Request.StockLocationId!.Value;
```

With:
```csharp
var stockLocationId = command.Request.StockLocationId!.Value; // guaranteed non-null by validator
```

Actually, since the validator guarantees `StockLocationId` is not null, we can use:
```csharp
var stockLocationId = command.Request.StockLocationId!.Value;
```

But the better approach is to keep `!.Value` with a comment since the validator is a pipeline behavior applied before the handler. Remove the null-forgiving if possible based on the Request type — since `StockLocationId` is `Guid?` in `StockReservationParameters`, we must use `.Value` but it's safe because the validator catches nulls.

- [ ] **Step 8: Run all tests**

```bash
dotnet build service/Api/src/Module/Module.csproj
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ReserveCartStock"
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~StockReservationResult"
```
Expected: All PASS.

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Constant.cs
git add service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Result.cs
git add service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Validation.cs
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Validator.cs
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs
git add service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Tests.cs
git add service/Api/tests/Module.UnitTests/Inventory/Domain/StockReservations/StockReservationResultTests.cs
git commit -m "fix: add validator, constants, and result errors for ReserveCartStock"
```

---

## Part E: Cluster 5 — Image Upload Security

### Task E1: Wire up AllowedImageExtensions in upload validation

**Files:**
- Modify: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.Result.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Shared/Validators/VariantImage.Validator.cs`

**Interfaces:**
- Consumes: `AllowedImageExtensions` from `UploadVariantImage.cs`
- Produces: file extension validation in the FluentValidation pipeline

- [ ] **Step 1: Write the tests**

In `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Shared/Validators/VariantImage.Validator.Tests.cs`, add:

```csharp
[Theory(DisplayName = "UploadImageRequestValidator: rejects disallowed extensions")]
[InlineData(".exe")]
[InlineData(".php")]
[InlineData(".html")]
public void UploadImageRequestValidator_DisallowedExtension_ReturnsError(string extension)
{
    var validator = new VariantImageValidator.UploadImageRequestValidator();
    var request = CreateUploadRequest(fileName: $"evil{extension}", contentType: "image/jpeg", fileSize: 1000);

    var result = validator.TestValidate(request);

    result.ShouldHaveValidationErrorFor(x => x.File);
}

[Theory(DisplayName = "UploadImageRequestValidator: accepts allowed extensions")]
[InlineData(".jpg")]
[InlineData(".jpeg")]
[InlineData(".png")]
[InlineData(".gif")]
[InlineData(".webp")]
public void UploadImageRequestValidator_AllowedExtension_Passes(string extension)
{
    var validator = new VariantImageValidator.UploadImageRequestValidator();
    var request = CreateUploadRequest(fileName: $"image{extension}", contentType: "image/jpeg", fileSize: 1000);

    var result = validator.TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
}

[Fact(DisplayName = "UnsupportedFileType: returns validation error with extension in message")]
public void UnsupportedFileType_ReturnsValidationError()
{
    var error = VariantImageResult.Failure.UnsupportedFileType(".exe");
    error.Code.Should().Be("VariantImage.UnsupportedFileType");
    error.Type.Should().Be(400);
    error.Message.Should().Contain(".exe");
}
```

- [ ] **Step 2: Run tests to see them fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~UploadImageRequestValidator"
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~UnsupportedFileType"
```
Expected: FAIL — no extension validation exists, no `UnsupportedFileType` error.

- [ ] **Step 3: Add the Result error**

In `VariantImage.Result.cs`, add to `Failure`:
```csharp
/// <summary>File extension is not in the allowed list.</summary>
public static Error UnsupportedFileType(string ext) => Error.Validation(
    code: "VariantImage.UnsupportedFileType",
    message: $"File extension '{ext}' is not supported.");
```

- [ ] **Step 4: Add extension validation to the validator**

In `VariantImage.Validator.cs`, in the `UploadImageRequestValidator` class, add after line 77 (after the ContentType check):

```csharp
// Validate: File extension must be in the allowed list
RuleFor(x => x.File.FileName)
    .Must(fileName =>
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(ext) && UploadVariantImage.AllowedImageExtensions.Contains(ext);
    })
    .WithErrorCode(VariantImageResult.Failure.UnsupportedFileType(".unknown").Code)
    .WithMessage(x => VariantImageResult.Failure.UnsupportedFileType(
        Path.GetExtension(x.File.FileName) ?? ".unknown").Message);
```

But `AllowedImageExtensions` is `private` in `UploadVariantImage.cs:33`. We need to either:
- Make it `internal` and add `InternalsVisibleTo` for the validator
- Or move it to `VariantImageConstant.Constraints.Upload`

Better: move to the domain constants file. In `VariantImage.Constant.cs`, add to `Constraints.Upload`:
```csharp
public static readonly string[] AllowedFileExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
```

Then reference it from both the validator and the upload handler. Remove the private array from `UploadVariantImage.cs:33`.

- [ ] **Step 5: Update UploadVariantImage.cs**

Remove line 33:
```csharp
private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
```

Update any reference to use `VariantImageConstant.Constraints.Upload.AllowedFileExtensions` instead.

- [ ] **Step 6: Run all tests**

```bash
dotnet build service/Api/src/Module/Module.csproj
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~VariantImage"
```
Expected: All PASS.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Shared/Validators/VariantImage.Validator.cs
git add service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.Result.cs
git add service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.Constant.cs
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Shared/Validators/VariantImage.Validator.Tests.cs
git commit -m "fix: add file extension validation for variant image uploads"
```

---

### Task E2: Sanitize filename for storage key

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs:67`

**Interfaces:**
- Consumes: `Path.GetFileName`
- Produces: safe storage key without path traversal

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.Tests.cs`, add:

```csharp
[Fact(DisplayName = "Handler: sanitizes filename with path traversal characters")]
public async Task Handle_PathTraversalFileName_SanitizesToLeaf()
{
    var request = CreateUploadRequest(fileName: "../../../etc/passwd.jpg", contentType: "image/jpeg");
    var handler = CreateHandler();
    SetupVariantExists();
    SetupStorageService(key => { /* capture key */ });

    await handler.Handle(new UploadVariantImage.Command(Guid.NewGuid(), request), CancellationToken.None);

    CapturedStorageKey.Should().EndWith("passwd.jpg");
    CapturedStorageKey.Should().NotContain("..");
}
```

- [ ] **Step 2: Run test to see it fail**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Handle_PathTraversalFileName"
```
Expected: FAIL — `../../../etc/passwd.jpg` is used directly as the storage key.

- [ ] **Step 3: Sanitize the filename**

In `UploadVariantImage.cs:67`, replace:
```csharp
Key = $"{subdirectory}/{request.File.FileName}",
```

With:
```csharp
Key = $"{subdirectory}/{Path.GetFileName(request.File.FileName)}",
```

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~UploadVariantImage"
```
Expected: All PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.Tests.cs
git commit -m "fix: sanitize filename with Path.GetFileName before constructing storage key"
```

---

## Part F: Cluster 6 — Nits & Polish

### Task F1: Add guard for .Value on Result<T> in checkout

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs:124`

**Interfaces:**
- Consumes: `StockReservationMethod.Reserve()` returning `Result<StockReservation>`
- Produces: safe unwrap with error propagation

- [ ] **Step 1: Write the test**

In `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs`, add:

```csharp
[Fact(DisplayName = "Handler: propagates Reserve() failure instead of throwing")]
public async Task Handle_ReserveStockFails_ReturnsError()
{
    SetupCart();
    var stockItem = CreateStockItem(countOnHand: 10);
    SetupStockItemLookup(stockItem);
    // Reserve() will fail because TTL is negative
    SetupStockReservationExpiry(-1);

    var handler = CreateHandler();
    var result = await handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), CancellationToken.None);

    result.IsFailure.Should().BeTrue();
    // Error propagated, not thrown as InvalidOperationException
}
```

Wait — `StockReservationMethod.Reserve()` validates TTL and returns `StockReservationResult.Errors.TtlMustBePositive` if TTL <= 0. So this test passes a negative expiry constant and expects error propagation.

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Handle_ReserveStockFails"
```
Expected: FAIL — `.Value` on failed Result throws `InvalidOperationException`.

- [ ] **Step 3: Add the guard**

Replace lines 124-126:
```csharp
var reservation = StockReservationMethod.Reserve(
    si.VariantId, take, si.StockLocationId, cart.Id, StockReservationExpiryDays).Value;
dbContext.Set<StockReservation>().Add(reservation);
```

With:
```csharp
var reserveResult = StockReservationMethod.Reserve(
    si.VariantId, take, si.StockLocationId, cart.Id, StockReservationExpiryDays);
if (reserveResult.IsFailure) return reserveResult.Errors;
var reservation = reserveResult.Value;
dbContext.Set<StockReservation>().Add(reservation);
```

- [ ] **Step 4: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateOrderFromCart"
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs
git commit -m "fix: guard against Reserve() failure instead of accessing .Value unsafely"
```

---

### Task F2: Log warning on webhook parse failure

**Files:**
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:40-41`

- [ ] **Step 1: Add the log call**

Replace lines 39-41:
```csharp
var stripeEvent = _webhookService.ParseEvent(payload);
if (stripeEvent is null)
    return;
```

With:
```csharp
var stripeEvent = _webhookService.ParseEvent(payload);
if (stripeEvent is null)
{
    ProcessStripeWebhookEventJobLoggers.ParseFailure(_logger);
    return;
}
```

Add to the loggers class (`ProcessStripeWebhookEventJobLoggers`) if it doesn't exist, or use `_logger.LogWarning("Failed to parse Stripe webhook event from payload.")`.

- [ ] **Step 2: Build and verify**

```bash
dotnet build service/Api/src/Module/Module.csproj
```
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
git commit -m "fix: log warning when Stripe webhook event parse fails"
```

---

### Task F3: Add missing Produces annotations

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.Endpoint.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.Endpoint.cs`

- [ ] **Step 1: Add the annotations**

In `ReserveCartStock.Endpoint.cs`, add after `.Produces<Result>(StatusCodes.Status400BadRequest)`:
```csharp
.Produces<Result>(StatusCodes.Status409Conflict)
.Produces<Result>(StatusCodes.Status404NotFound)
```

In `CreateOrderFromCart.Endpoint.cs`, add after `.Produces<Result>(StatusCodes.Status404NotFound)`:
```csharp
.Produces<Result>(StatusCodes.Status409Conflict)
.Produces<Result>(StatusCodes.Status422UnprocessableEntity)
```

In `ConfirmEmail.Endpoint.cs`, add:
```csharp
.Produces(StatusCodes.Status204NoContent)
```

- [ ] **Step 2: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Endpoint.cs
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.Endpoint.cs
git add service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.Endpoint.cs
git commit -m "docs: add missing OpenAPI Produces annotations for conflict, validation, and no-content responses"
```

---

### Task F4: Remove AllowAnonymous, add Authorize to checkout endpoint

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.Endpoint.cs:22`

- [ ] **Step 1: Replace the attribute**

Remove `.AllowAnonymous()` from line 22 and ensure the endpoint requires authentication.

In the existing code, the endpoint has `.AllowAnonymous()` on line 22 but the handler at line 42-43 rejects unauthenticated users with a domain error. The fix: remove `.AllowAnonymous()`. The endpoint inherits whatever the default auth policy is (which should require authentication for Carter endpoints).

Actually, let me check — does the endpoint need an explicit `[Authorize]` attribute or does removal of `.AllowAnonymous()` suffice? In Carter, if no `.AllowAnonymous()` is present, the endpoint inherits the pipeline's default which typically requires authorization. To be safe, add `.RequireAuthorization()`.

Replace line 22:
```csharp
.AllowAnonymous()
```

With:
```csharp
.RequireAuthorization()
```

- [ ] **Step 2: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.Endpoint.cs
git commit -m "fix: require authorization on checkout endpoint instead of returning domain error"
```

---

### Task F5: Move fire-and-forget calls before response in ConfirmEmail

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs:86-98`

- [ ] **Step 1: Reorder the code**

Replace lines 86-98:
```csharp
if (isEmailChange)
{
    UserLoggers.Emails.EmailChangeConfirmed(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
}
else
{
    UserLoggers.Emails.EmailVerified(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
    await SendWelcomeNotificationAsync(user);
    await CreateUserProfileAsync(user, cancellationToken);
}

return Result.NoContent();
```

With:
```csharp
if (isEmailChange)
{
    UserLoggers.Emails.EmailChangeConfirmed(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
}
else
{
    UserLoggers.Emails.EmailVerified(logger, UserId: user.Id, Email: user.Email!, Timestamp: DateTime.UtcNow, ActionBy: currentUser.UserName);
}

// Best-effort: profile creation and welcome notification fire after confirmation.
// Failures are logged but do not block the confirmation response.
await Task.WhenAll(
    SendWelcomeNotificationAsync(user),
    CreateUserProfileAsync(user, cancellationToken));

return Result.NoContent();
```

Note: Move both notifications to always fire (not just for first-time verification). The `SendWelcomeNotificationAsync` only fires when `isEmailChange` is false (first-time verification) — keep that logic. Actually, let me keep the existing behavior: only fire for first-time verification.

- [ ] **Step 2: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```
Expected: 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs
git commit -m "fix: await profile creation and notification before returning NoContent in ConfirmEmail"
```

---

## Execution Order

```
A1 → A2 → A3 → A4  (Cluster 1 — land first)
                       ↓
    ┌──────────────────┼──────────────────┐
    ↓                  ↓                  ↓
B1→B2→B3→B4      C1→C2→C3          D1→D2→D3
(Cluster 2)       (Cluster 3)       (Cluster 4) → F1 (depends on D1 — same file)
    ↓                  ↓                            ↓
    └──────────────────┼────────────────────────────┘
                       ↓
                 E1→E2  F2→F3→F4→F5
              (Cluster 5)   (Cluster 6)

Total: 23 commits, ~34 tests

**Dependency note:** F1 modifies `CreateOrderFromCart.cs:124` — the same lines that D1 changes. Implement F1 after D1, or merge F1 into D1's final commit.
```

## Verification

After all clusters complete:
```bash
dotnet build                                                   # Full solution build
dotnet test service/Api/tests/Module.UnitTests                  # All unit tests
dotnet test service/Api/tests/Shared.UnitTests                  # All shared tests
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~CheckoutConcurrency"  # Integration test
```
