# API Review Remediation — Design

**Date:** 2026-07-20
**Context:** Code review found 15 issues across infrastructure, identity, payment, ordering/inventory, image upload, and polish.
**Approach:** Cluster by subsystem. 6 independent PRs. Each cluster has its own tests. Infrastructure cluster must land first (other clusters may depend on it).

---

## Cluster 1: Infrastructure/Security

_Scope: `Shared/Security/`, `infra/Aspire/`, `appsettings.json`_

### 1.1 Rate Limiting — Apply policies to endpoints

**Problem:** Five named policies (`auth`, `register`, `forgot-password`, `payment`, `default`) defined in `RateLimit.Extensions.cs` but `RequireRateLimiting()` never called anywhere.

**Fix:** Add `.RequireRateLimiting("policyName")` to each target endpoint's `.MapPost()` chain.

**Endpoints to annotate:**

| Policy | Endpoints |
|--------|-----------|
| `auth` | `POST /api/store/identity/auth/login/password`, `POST /api/store/identity/auth/login/external/authenticate` |
| `register` | `POST /api/store/identity/auth/register` |
| `forgot-password` | `POST /api/store/identity/passwords/forgot` |
| `payment` | `POST /api/storefront/payment/create-intent`, `POST /api/storefront/payment/confirm` |
| `default` | Apply globally via `app.UseRateLimiter()` (opt-in fallback for unclassified endpoints) |

**Files changed:** ~8 Endpoint.cs files + `Program.cs` (for global `UseRateLimiter`).

**Tests:** `Shared.UnitTests` — verify extension method wiring. Smoke-test that rate-limited endpoint returns 429 after exceeding window limit.

### 1.2 Health Checks — Remove production gate

**Problem:** `Extensions.cs:118` in `infra/Aspire/src/ReSys.ServiceDefaults/` gates `/health` and `/alive` behind `!IsProduction()`. Orchestrators cannot probe in production.

**Fix:** Remove the `if (!app.Environment.IsProduction())` guard entirely. The default health check endpoint returns only `"Healthy"` — no sensitive data. If authentication is needed later, add it to the endpoint, don't disable it.

**Files changed:** 1 file: `ReSys.ServiceDefaults/Extensions.cs`.

**Tests:** `Shared.UnitTests` — verify `MapDefaultEndpoints` registers health checks regardless of environment.

### 1.3 HSTS Header — Fix documentation

**Problem:** `SecurityHeadersMiddleware.cs:8` XML doc claims it emits HSTS. `SecurityHeadersSetting` has no `HSTS` property. No `Strict-Transport-Security` header is emitted.

**Fix:** Remove the HSTS claim from the XML doc. Replace with accurate header list: `X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`, `Referrer-Policy`, `Permissions-Policy`. Add a comment noting that HSTS should be handled by the reverse proxy (Aspire/nginx) in production.

**Files changed:** 1 file: `SecurityHeadersMiddleware.cs`.

**Tests:** Verify middleware emits the 5 headers it claims to emit, and does NOT emit `Strict-Transport-Security`.

### 1.4 CORS Origins — Add documentation comment

**Problem:** Base `appsettings.json` has `Cors.Origins: []`. No production config file. Dev origins already set in `appsettings.Development.json`.

**Fix:** Add a comment in `appsettings.json` above the `Cors` section explaining that origins must be configured per environment.

**Files changed:** 1 file: `appsettings.json`.

---

## Cluster 2: Identity/Auth — Encoding Bugs

_Scope: `Shared/Governance/Conventions/`, `Module/Identity/Features/Store/Emails/`_

### 2.1 Base64Url Decoder — Fix swapped Replace args

**File:** `Shared/Governance/Conventions/Base64.Conveter.cs:99`

**Problem:** `FromBase64Url` does `.Replace("/", "_")` instead of `.Replace("_", "/")`. The method converts `/` chars already in the input to `_`, corrupting any base64url input. The correct transformation converts base64url (`-_`) to standard base64 (`+/`) — so `_` must become `/`, not vice versa.

**Fix:**
```csharp
.Replace("_", "/")   // base64url → standard base64
```

**Tests:** Add round-trip tests for `Encode → FromBase64Url → original` with inputs containing `_` and `/` characters.

### 2.2 Email Verification — Standardize on Base64Url

**Files:** `ResendEmailVerification.cs:86`, `ChangeEmail.cs:91`

**Problem:** Three senders use different encodings for the same token:
- `ResendEmailVerification.cs:86`: `token.ToBase64()` — standard base64 with `+/=`
- `ChangeEmail.cs:91`: `Uri.EscapeDataString(token)` — URL percent-encoding
- `ConfirmEmail.cs:42`: `TryFromBase64Url(token)` — expects base64url `-_` no padding

Tokens go in email links as URL query params. Base64Url is the correct choice — no chars that need URL escaping.

**Fix:** Standardize all senders on `.ToBase64Url()`:
```csharp
// ResendEmailVerification.cs:86
var encodedToken = token.ToBase64Url();   // was .ToBase64()

// ChangeEmail.cs:91
var encodedToken = token.ToBase64Url();   // was Uri.EscapeDataString(...)
```
`ConfirmEmail.cs:42` already correctly uses `TryFromBase64Url` — no change.

**Tests:** Round-trip test: `ToBase64Url()` → `TryFromBase64Url()` → original match. Negative test: standard-base64 encoded token (with `+` or `/`) correctly fails `TryFromBase64Url`.

### 2.3 ConfirmEmail — Add UserId validation

**File:** `ConfirmEmail.Validator.cs`

**Problem:** Only `Token` is validated. `UserId` can be `Guid.Empty`, passes validation, reaches `FindByIdAsync(Guid.Empty)` which returns null → `NotFound` error.

**Fix:** Add to existing validator:
```csharp
RuleFor(x => x.Request.UserId).NotEmpty();
```

**Tests:** Unit test that empty GUID returns validation error.

---

## Cluster 3: Payment — Idempotency & Ordering

_Scope: `Module/Payment/Features/`, `Module/Payment/Backgrounds/`_

### 3.1 Webhook Handlers — Add terminal-state guards

**File:** `ProcessStripeWebhookEventJob.cs`

**Problem:** `HandlePaymentIntentSucceeded` (line 79) checks `if (payment.State == Completed) return;`. The other four handlers (`PaymentIntentFailed`, `ChargeRefunded`, `ChargeDisputeCreated`, `PaymentIntentCanceled`) have no such guard. Duplicate webhook events can double-apply refunds or re-fail voided payments.

**Fix:** Add guards matching the pattern from line 79:

| Handler | Guard |
|---------|-------|
| `HandlePaymentIntentFailed` | `if (payment.State is Failed or Voided) return;` |
| `HandleChargeRefunded` | `if (payment.State is Refunded or Voided) return;` |
| `HandleChargeDisputeCreated` | `if (payment.State is Disputed) return;` |
| `HandlePaymentIntentCanceled` | `if (payment.State is Canceled or Voided) return;` |

**Tests:** Mock DB to return payment already in terminal state. Verify handler returns early without calling state-mutating methods.

### 3.2 Payment Intent — Persist after gateway call

**File:** `CreatePaymentIntent.cs:57-58`

**Problem:** `PaymentCapture` is `Add()`-ed and `SaveChangesAsync()`-ed before the Stripe call. If Stripe times out, orphaned `Pending` record stays in DB.

**Fix:** Swap order — call `processingService.CreateIntentAsync(...)` first. Only persist on success:
```csharp
var intentResult = await processingService.CreateIntentAsync(...);
if (intentResult.IsFailure) return intentResult.Errors;

dbContext.Set<PaymentCapture>().Add(payment);
await dbContext.SaveChangesAsync(cancellationToken);
```

**Tests:** Verify no entity added to change tracker when gateway call throws.

### 3.3 CancellationToken.None — Document (no code change)

**File:** `StripeWebhook.cs:28`

**Problem:** `CancellationToken.None` passed to Hangfire `Enqueue` expression. This is actually correct — Hangfire uses it as a serialization placeholder and injects its own token at execution time.

**Fix:** Add comment: `// CancellationToken.None is a serialization placeholder — Hangfire injects the real token at execution time.`

---

## Cluster 4: Ordering/Inventory — Concurrency

_Scope: `Module/Ordering/Features/Storefront/Cart/Checkout/`, `Module/Inventory/`_

### 4.1 Stock Deduction — Use domain Pick() + retry on conflict

**File:** `CreateOrderFromCart.cs:90-155`

**Problem:** Raw `si.CountOnHand -= take` bypasses domain validation. Concurrent checkouts under `RepeatableRead` both read same count, one hits `DbUpdateConcurrencyException` with no retry — user gets error and must retry manually.

**Fix:**

1. Replace raw mutation with domain method:
   ```csharp
   // Was: si.CountOnHand -= take; si.ModifiedAtUtc = ...;
   var pickResult = si.Pick(take);
   if (pickResult.IsFailure) return pickResult.Errors;
   ```

2. Wrap transaction block in retry loop (3 attempts, 100ms/200ms/400ms backoff). Only retry on `DbUpdateConcurrencyException`:
   ```csharp
   for (int attempt = 0; attempt < 3; attempt++)
   {
       try { /* transaction block */ break; }
       catch (DbUpdateConcurrencyException) when (attempt < 2)
       {
           await Task.Delay(100 * (1 << attempt));
           await transaction.RollbackAsync(ct);
       }
   }
   ```

3. Keep `RepeatableRead` + `RowVersion` — existing concurrency defense, already proven by `CheckoutConcurrencyTests.cs`.

**Notes:**
- The retry loop wraps the entire transaction block (lines 90–157), including stock reads, order creation, reservation, and save.
- `Pick()` is an extension method on `StockItem` — the handler already loads `StockItem` entities from the DB, so the method is available.
- Existing `RowVersion` optimistic lock in `StockItemConfiguration.cs:28` remains the primary concurrency defense.

**Tests:**
- Unit: retry loop — mock `dbContext.SaveChangesAsync` to throw `DbUpdateConcurrencyException` twice, succeed on third
- Unit: retry loop — throws all three times → returns `ConcurrencyConflict` error
- Unit: `Pick()` validates insufficient stock (domain-level, already covered by `StockItem.Method.Adjustment.Tests.cs`)
- Integration: `CheckoutConcurrencyTests.cs` must still pass

### 4.2 Cart Reservation — Serializable → RepeatableRead

**File:** `ReserveCartStock.cs:33-34`

**Problem:** `IsolationLevel.Serializable` causes predicate locks and serialization failures under concurrent cart reservations.

**Fix:** Change to `IsolationLevel.RepeatableRead`. Existing `RowVersion` optimistic lock on `StockItem` provides sufficient concurrency defense.

**Tests:** Unit test verifies the handler uses `IsolationLevel.RepeatableRead` when beginning the transaction (mock `IApplicationDbContext.BeginTransactionAsync`).

### 4.3 ReserveCartStock — Add Validator + Constants + Result errors

**Files to create/modify:**

**a) `StockReservation.Constant.cs`** — add:
```csharp
public static class Defaults
{
    public const int DefaultTtlMinutes = 15;
    public const int MaxTtlMinutes = 10080;     // 7 days
    public const int MinTtlMinutes = 1;
}
```

**b) `StockReservation.Result.cs`** — add:
```csharp
public static Error StockLocationRequired => Error.Validation(
    code: "StockReservation.Cart.StockLocationRequired",
    message: "Stock location is required for cart reservation.");
public static Error CartTokenRequired => Error.Validation(
    code: "StockReservation.Cart.CartTokenRequired",
    message: "Cart token is required.");
public static Error TtlOutOfRange => Error.Validation(
    code: "StockReservation.Cart.TtlOutOfRange",
    message: $"TTL minutes must be between {StockReservationConstant.Defaults.MinTtlMinutes} and {StockReservationConstant.Defaults.MaxTtlMinutes}.");
```

**c) `StockReservation.Validation.cs`** — add extension methods referencing the Result errors above.

**d) `ReserveCartStock.Validator.cs`** (new):
```csharp
public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Request.StockLocationId).ApplyStockLocationRequired();
        RuleFor(x => x.Request.CartToken).NotEmpty();
        RuleFor(x => x.Request.VariantId).NotEmpty();
        RuleFor(x => x.Request.Quantity).ApplyQuantityRules();
        RuleFor(x => x.Request.TtlMinutes).ApplyTtlRangeRules();
    }
}
```

**e) `ReserveCartStock.cs`** — remove inline `if (quantity <= 0)` and `StockLocationId!.Value` null-forgiving. Replace `!.Value` with `.Value` (validator now guarantees non-null).

**Tests:**
- Unit: validator rejects null `StockLocationId`, empty `CartToken`, zero `Quantity`, out-of-range `TtlMinutes`
- Unit: error codes match `StockReservationResult.Errors.*`

---

## Cluster 5: Image Upload — Security

_Scope: `Module/Catalog/Features/Admin/Products/Variants/Images/`_

### 5.1 Wire Up AllowedImageExtensions

**File:** `UploadVariantImage.cs:33`, `VariantImage.Validator.cs`, `VariantImage.Result.cs`

**Problem:** `AllowedImageExtensions` array declared but never used. Upload validation only checks MIME type (client-controlled).

**Fix:**
1. Add to `VariantImage.Result.cs`:
   ```csharp
   public static Error UnsupportedFileType(string ext) => Error.Validation(
       code: "VariantImage.UnsupportedFileType",
       message: $"File extension '{ext}' is not supported. Allowed: {string.Join(", ", AllowedImageExtensions)}.");
   ```
2. Add extension validation rule in `VariantImage.Validator.cs` checking `Path.GetExtension(request.File.FileName).ToLowerInvariant()` against `AllowedImageExtensions`.

**Tests:** `.exe`, `.php`, `.html` rejected; `.jpg`, `.png`, `.webp` accepted.

### 5.2 Sanitize Filename for Storage Key

**File:** `UploadVariantImage.cs:67`

**Problem:** `Key = $"{subdirectory}/{request.File.FileName}"` — client-controlled filename used directly as storage key. Path traversal possible.

**Fix:**
```csharp
var safeFileName = Path.GetFileName(request.File.FileName);
Key = $"{subdirectory}/{safeFileName}";
```

`Path.GetFileName()` strips directory components. Combined with extension whitelist from 5.1, provides defense in depth.

**Tests:** Filenames with `../` components sanitized to just the filename leaf.

---

## Cluster 6: Nits & Polish

_Scope: various files, small changes_

### 6.1 .Value on Result<T> — Add guard

**File:** `CreateOrderFromCart.cs:124`

**Fix:**
```csharp
var reserveResult = StockReservationMethod.Reserve(...);
if (reserveResult.IsFailure) return reserveResult.Errors;
var reservation = reserveResult.Value;
```

**Tests:** Handler propagates `Reserve()` failure instead of throwing.

### 6.2 Webhook Parse Failure — Log warning

**File:** `ProcessStripeWebhookEventJob.cs:39-41`

**Fix:** Add `logger.LogWarning("Failed to parse Stripe webhook event from payload.");` before the silent return.

**Tests:** Not needed.

### 6.3 Missing Produces Annotations

**Files:** `ReserveCartStock.Endpoint.cs`, `CreateOrderFromCart.Endpoint.cs`, `ConfirmEmail.Endpoint.cs`

**Fix:** Add missing OpenAPI annotations:
- `ReserveCartStock`: `.Produces<Result>(409)`, `.Produces<Result>(404)`
- `CreateOrderFromCart`: `.Produces<Result>(409)`, `.Produces<Result>(422)`
- `ConfirmEmail`: `.Produces(204)`

**Tests:** Not needed (existing integration tests exercise these status codes).

### 6.4 Guest Checkout — Add auth requirement to endpoint

**File:** `CreateOrderFromCart.Endpoint.cs`

**Problem:** Endpoint has `.AllowAnonymous()` but handler returns `UserNotAuthenticated` for guests. Inconsistent — the 401/403 should come from the framework, not a domain error.

**Fix:** Remove `.AllowAnonymous()`, add `[Authorize]` attribute. Guest checkout is a feature, not a bug fix; this just makes the API contract honest.

**Tests:** Verify 401 returned for unauthenticated requests (framework-level, not domain error).

### 6.5 Notification Fire-and-Forget — Move before response

**File:** `ConfirmEmail.cs:93-98`

**Fix:** Move `SendWelcomeNotificationAsync` and `CreateUserProfileAsync` before `return Result.NoContent()`:
```csharp
// Best-effort: profile creation and welcome notification fire after confirmation.
// Failures are logged but do not block the confirmation response.
await Task.WhenAll(
    SendWelcomeNotificationAsync(user),
    CreateUserProfileAsync(user, cancellationToken));
return Result.NoContent();
```

**Tests:** Not needed (behavior unchanged).

---

## Test Summary

| Cluster | Test Location | Test Count (est.) |
|---------|--------------|-------------------|
| 1. Infrastructure | `Shared.UnitTests` | ~6 |
| 2. Identity | `Module.UnitTests` + `Shared.UnitTests` | ~5 |
| 3. Payment | `Module.UnitTests` | ~5 |
| 4. Ordering/Inventory | `Module.UnitTests` | ~10 |
| 5. Image Upload | `Module.UnitTests` | ~5 |
| 6. Nits | `Module.UnitTests` | ~3 |
| **Total** | | **~34** |

## Execution Order

Clusters are independent except:
- Cluster 1 (infrastructure) touches `RateLimit.Extensions.cs` and `Extensions.cs` — other clusters' endpoints reference rate limiting. Land first.
- Clusters 2–6 can be parallel after Cluster 1 lands.
