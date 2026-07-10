---
title: MVP Architecture Hardening — Security, Integrity & Reliability Fixes
version: 1.0
date_created: 2026-07-11
last_updated: 2026-07-11
owner: ReSys.Shop Team
tags: [architecture, security, reliability, design, app]
---

# Introduction

This specification defines the requirements for resolving 82 findings from the full-codebase MVP review. Every fix must respect existing codebase patterns: `static partial class` vertical slices, `Result<T>` return types, Carter module endpoints, FluentValidation validators, and MediatR CQRS dispatch. No new frameworks, libraries, or architectural patterns are introduced.

## 1. Purpose & Scope

**Purpose:** Produce a single, machine-readable specification that an AI agent can use to implement all MVP hardening fixes without deviating from established patterns.

**Scope:** All 9 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping, Webhooks) plus Shared infrastructure. Covers security authorization gaps, race conditions, data integrity bugs, missing validation, and code quality nits.

**Audience:** AI coding agents implementing fixes; human reviewers validating completeness.

**Assumptions:**
- `.HasPermission(PermissionMetadata)` internally chains `.RequireAuthorization(new HasPermissionAttribute(permission))` — so an endpoint with `.HasPermission()` already requires authentication. The explicit `.RequireAuthorization()` before `.HasPermission()` is a codebase convention (used in Identity module) for readability, not a functional requirement.
- Stock reservations are "soft" — `CountOnHand` is NOT decremented on reserve. Available = `CountOnHand - SUM(active reservations)`. Only checkout/fulfillment decrements `CountOnHand`.
- The `Result<T>` pattern uses implicit conversions: `return error;` in a `Result<T>` method implicitly creates a failure result.

## 2. Definitions

| Term | Definition |
|------|-----------|
| **TOCTOU** | Time-of-Check-Time-of-Use — a race condition where a check and the subsequent use are not atomic. |
| **SSRF** | Server-Side Request Forgery — an attacker making the server issue requests to internal/private networks. |
| **Soft reservation** | A reservation that does not decrement `CountOnHand`. Availability is computed as `CountOnHand - SUM(active reservations)`. |
| **Vertical slice** | A feature organized as a set of files (Request, Command, Handler, Endpoint, Validator) in a single folder. |
| **HasPermission** | Extension method on `RouteHandlerBuilder` that chains `.RequireAuthorization(new HasPermissionAttribute(permission))`, making the permission string an ASP.NET Core authorization policy name. |
| **PermissionMetadata** | A sealed record with `Domain`, `Category`, `Resource`, `Action` fields, composing a dot-separated `Identifier` string. |
| **Result<T>** | A readonly record struct representing success (`Value`) or failure (`Errors`), with implicit conversions from `T`, `Error`, `Error[]`, and `List<Error>`. |
| **Carter** | A .NET minimal API module library. Each endpoint class implements `ICarterModule` and registers routes in `AddRoutes`. |
| **MediatR** | A mediator library for CQRS. Commands/queries are dispatched via `ISender.Send()`. |

## 3. Requirements, Constraints & Guidelines

### 3.1 Authorization (SEC-AUTH)

- **SEC-AUTH-001**: Every admin endpoint MUST have `.HasPermission(PermissionMetadata)` chained on its route handler. This is the canonical authorization mechanism. The `.RequireAuthorization()` call before `.HasPermission()` is optional but recommended per codebase convention (used in Identity module).

- **SEC-AUTH-002**: Every storefront endpoint that operates on user-specific data MUST have `.RequireAuthorization()` (authentication required). Endpoints that serve public data (product listing, taxonomy tree) MAY be anonymous.

- **SEC-AUTH-003**: The 6 Webhooks admin endpoints (`Create`, `Update`, `Delete`, `GetById`, `GetPaged`, `Test`) MUST add `.HasPermission(WebhooksFeature.Admin.Subscriptions.{Action}.Permission)`. Each endpoint must define its own `PermissionMetadata` in the feature metadata class, following the pattern: `WebhooksFeature.Admin.Subscriptions.Create.Permission = new PermissionMetadata("Webhooks", "Admin", "Subscriptions", "Create")`.

- **SEC-AUTH-004**: The Webhooks module must define a `WebhooksFeature` static class with route constants, permission metadata, tags, summaries, and descriptions for all 6 subscription endpoints — matching the pattern in `IdentityFeature`, `CatalogFeature`, etc.

- **SEC-AUTH-005**: Profile `GetProfilesPagedOrAll` (store-level) MUST be restricted to admin-only via `.HasPermission(ProfileFeature.Admin.Profiles.Get.Permission)` or removed entirely. A regular store user must not be able to enumerate all user profiles (PII leakage).

### 3.2 SSRF Protection (SEC-SSRF)

- **SEC-SSRF-001**: Webhook URL validation MUST enforce:
  - Scheme is `https` only (reject `http`, `file`, `gopher`, `dict`, `ftp`).
  - Hostname does not resolve to private/reserved IP ranges: `127.0.0.0/8`, `10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`, `169.254.0.0/16`, `0.0.0.0`.
  - Hostname is not a metadata endpoint (`169.254.169.254`).
  - URL maximum length is 2048 characters.

- **SEC-SSRF-002**: The `WebhookSubscription.Method.Update()` domain method MUST apply the same URL validation as create. Direct property assignment (`subscription.Url = request.Url`) is forbidden.

### 3.3 Stock Integrity (BUS-STOCK)

- **BUS-STOCK-001**: All operations that read stock availability and then mutate stock (reserve, checkout, transfer, bulk adjust) MUST be atomic. Use one of:
  - `SELECT ... FOR UPDATE` within a serializable transaction, OR
  - `ExecuteUpdateAsync` with atomic arithmetic (`CountOnHand = CountOnHand - @qty`) and a `WHERE CountOnHand >= @qty` guard, OR
  - Optimistic concurrency via a `RowVersion` / `xmin` column on `StockItem`.

- **BUS-STOCK-002**: The `ReleaseCartReservation` handler MUST NOT increment `CountOnHand`. The reserve flow never decrements `CountOnHand` (soft reservation). The release must only update `StockReservation.State = Released`.

- **BUS-STOCK-003**: The `CancelStockReservation` (admin) handler MUST also NOT increment `CountOnHand`. If stock was never decremented during reserve, it must not be incremented during release. Remove lines 28-33 from `ReleaseCartReservation.cs` and lines 33-47 (the stock restoration block) from `CancelStockReservation.cs`.

- **BUS-STOCK-004**: The `TransferStockTransfer` handler MUST use atomic check-and-decrement. Replace the two-query pattern (check then decrement) with a single `ExecuteUpdateAsync` that atomically decrements `CountOnHand` only when `CountOnHand >= item.Quantity`.

- **BUS-STOCK-005**: The `BulkAdjustStockItems` handler MUST use `ExecuteUpdateAsync` with `CountOnHand = CountOnHand + quantity` to prevent concurrent adjustments from overwriting each other.

- **BUS-STOCK-006**: The `ReserveCartStock` handler MUST use a serializable transaction or `FOR UPDATE` lock to prevent concurrent reservations from over-reserving beyond physical stock.

### 3.4 Payment Integrity (BUS-PAY)

- **BUS-PAY-001**: `ConfirmPayment` handler MUST query the Stripe `PaymentIntent` status via the gateway before calling `payment.Complete()`. Only complete if `intent.Status == "succeeded"`.

- **BUS-PAY-002**: `StripeWebhook.HandlePaymentIntentSucceeded` MUST NOT directly assign `payment.RefundedAmount`. Use the domain method `payment.Refund(delta)` where `delta = newRefunded - payment.RefundedAmount`, and only if `delta > 0`.

- **BUS-PAY-003**: `CreatePaymentIntent` MUST filter payment methods: `.FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken)`.

- **BUS-PAY-004**: `CreateSetupIntent` MUST filter payment methods: `.FirstOrDefaultAsync(pm => pm.Id == id && pm.Active && !pm.IsDeleted, cancellationToken)`.

- **BUS-PAY-005**: `StripeGateway` amount conversions MUST use `Math.Round(amount * CentsMultiplier, MidpointRounding.AwayFromZero)` instead of `(long)(amount * CentsMultiplier)` to prevent truncation.

- **BUS-PAY-006**: `CreateSetupIntent` MUST NOT set `StripeConfiguration.ApiKey` globally. Remove the line; the `StripeGateway` constructor already sets it per scope.

- **BUS-PAY-007**: `StripeWebhook.HandlePaymentIntentFailed` MUST check the `payment.Fail()` return value before calling `SaveChangesAsync`.

- **BUS-PAY-008**: `RefundPayment` response MUST return `refundAmount` (the actual amount refunded), not `command.Request.Amount`.

- **BUS-PAY-009**: `RefundPayment` handler MUST use `command.Request.Amount` for partial refunds instead of hardcoded `payment.Amount`.

- **BUS-PAY-010**: `CapturePayment` for already-completed payments MUST return `PaymentResult.Failure.AlreadyCompleted` instead of silently succeeding.

- **BUS-PAY-011**: `GetPagedPayments` endpoint MUST accept `[AsParameters] QueryingParameters parameters` instead of hardcoding `new QueryingParameters()`.

- **BUS-PAY-012**: `DeletePaymentMethod` MUST check for active (non-terminal) payments before soft-deleting.

- **BUS-PAY-013**: `GetPaymentById` admin response MUST NOT include `IntentClientSecret`. Remove from response model.

- **BUS-PAY-014**: `GetPaymentById` MUST populate `OrderNumber` and `PaymentMethodName` from navigation properties, or remove from response model.

- **BUS-PAY-015**: `ConfirmPayment` endpoint MUST NOT require an empty `[FromBody] Request` — remove the body parameter.

### 3.5 Ordering Integrity (BUS-ORDER)

- **BUS-ORDER-001**: `UpdateOrderStatus` (admin) handler MUST release stock when canceling from Placed status. Add the same stock-release loop used in `CancelOrder.cs`.

- **BUS-ORDER-002**: `UpdateOrderStatus` (admin) handler MUST NOT allow Draft→Placed transition without stock availability check, stock reservation, and payment verification. Either remove the transition or add guards.

- **BUS-ORDER-003**: `UpdateOrderAdmin`, `UpdateOrderLineItem`, `UpdateOrderShipAddress`, `UpdateOrderBillAddress` MUST check `order.Uneditable()` or `order.Status == OrderStatus.Draft` before applying changes.

- **BUS-ORDER-004**: `UpdateOrderLineItem` MUST verify stock availability when increasing quantity on Placed orders.

- **BUS-ORDER-005**: `OrderMerger` MUST check `currentLineItem.Quantity + otherLineItem.Quantity <= LineItemConstant.MaxQuantity` before merging. Return error if exceeded.

- **BUS-ORDER-006**: `UpdateCheckout` and `SelectShippingRate` MUST compute a clean total (excluding old shipping adjustment) before passing to `ShippingRateCalculator.CalculateAsync`.

- **BUS-ORDER-007**: `EmptyCart` and `DeleteCart` handlers MUST support guest users via `currentUser.SessionId` fallback, consistent with `AddToCart` and `GetCart`.

- **BUS-ORDER-008**: `LineItem.FinalAmount()` MUST return `lineItem.Total` (which already includes `AdjustmentTotal`) or be fixed to `(Quantity * Price) + AdjustmentTotal` without double-counting.

- **BUS-ORDER-009**: `LineItem.RecalculateTotal` and `Order.RecalculateTotals` MUST be consistent: either line-level `AdjustmentTotal` is excluded from `LineItem.Total` (and Order handles all adjustments), or the Order formula excludes line-level adjustments from `ItemTotal`.

- **BUS-ORDER-010**: `UpdateOrderStatus` (admin cancel) MUST set `CanceledById` for audit trail, consistent with `CancelOrder.cs`.

### 3.6 Catalog Integrity (BUS-CAT)

- **BUS-CAT-001**: `GetSimilarProducts` raw SQL MUST use correct table names: `catalog.variants`, `catalog.product_images`, `catalog.product_image_embeddings` (matching `CatalogSchema.TableNames`).

- **BUS-CAT-002**: `GetSimilarProducts` happy-path return MUST wrap in `Result<Response>.Ok(...)` — not return raw `Response`.

- **BUS-CAT-003**: `CreateProduct` MUST check `addVariantResult.IsSuccess` before accessing `.Value.Id`.

- **BUS-CAT-004**: `SearchByImage` endpoint MUST validate file size (e.g., `image.Length <= 10_485_760`) and content type before allocating `MemoryStream`.

- **BUS-CAT-005**: `SetVariantPrice` MUST have a `Validator` class validating `Currency` (NotEmpty) and `Amount` (>= 0 when provided).

- **BUS-CAT-006**: `UpdateProduct.Request.cs` MUST NOT use `new` to shadow `TrackInventory`. Make the base property nullable or use `[JsonIgnore]` on the base.

- **BUS-CAT-007**: `DeleteProduct` MUST have a double-deletion guard: `if (entity.IsDeleted) return ProductResult.Errors.AlreadyDeleted`.

- **BUS-CAT-008**: `AddVariant` and `UpdateVariant` MUST enforce SKU uniqueness: `AnyAsync(x => x.Sku == sku && x.Id != currentVariantId)`.

- **BUS-CAT-009**: `DiscontinueProduct` MUST validate domain invariant `@CAT-10`: `AvailableOn <= DiscontinueOn` when both set.

- **BUS-CAT-010**: `RestoreTaxonomy` MUST cascade-restore all child taxons, not just the root.

- **BUS-CAT-011**: `CreateTaxonomy` slug generation MUST use the shared `ProductMethod.GenerateSlugFromName()` utility, not naive `ToLower().Replace(' ', '-')`.

- **BUS-CAT-012**: `GetSimilarProducts` MUST prioritize the primary (master) variant for embedding lookup: `OrderBy(v => v.Position).ThenBy(v => v.IsMaster ? 0 : 1)`.

### 3.7 Inventory Integrity (BUS-INV)

- **BUS-INV-001**: `CancelStockTransfer` (InTransit→Canceled) MUST create a `StockMovement` record for each restored item with `action: "transfer_canceled"`.

- **BUS-INV-002**: `ReceiveStockTransfer` MUST NOT silently skip missing destination `StockItem`. Auto-create the stock item or return an error.

- **BUS-INV-003**: `RestockStockItem` MUST capture `previousCount` after backorder fulfillment loop (before adding remaining), not before.

- **BUS-INV-004**: `RestockStockItem` MUST return errors when movement creation fails, not proceed with `movementId = Guid.Empty`.

- **BUS-INV-005**: `RestockStockItem` handler MUST inject `ICurrentUser` and `ILogger` to set audit fields and log operations.

- **BUS-INV-006**: `ImportStockItems` MUST enforce a maximum file size before reading the stream.

- **BUS-INV-007**: `TransferStockTransfer` MUST load stock items once (not twice) for check and decrement.

- **BUS-INV-008**: `GenerateNumber()` for stock transfers MUST include a random suffix or database sequence to prevent second-level collisions.

### 3.8 Shipping & Location (BUS-SHIP, BUS-LOC)

- **BUS-SHIP-001**: `ShippingRateCalculator` weight matching MUST handle null bounds: `(r.MinWeight == null || r.MinWeight <= orderWeight) && (r.MaxWeight == null || r.MaxWeight >= orderWeight)`.

- **BUS-SHIP-002**: `DeactivateShippingMethod` MUST check for in-progress orders before deactivating.

- **BUS-SHIP-003**: `DeleteShippingMethod` MUST check for associated `ShippingRate` records and active orders before deleting.

- **BUS-SHIP-004**: `CalculateShipping` MUST derive currency from order/system config, not hardcode `"USD"`.

- **BUS-LOC-001**: Country ISO code duplicate check MUST be case-insensitive: `c.IsoCode.ToUpper() == request.IsoCode.ToUpper()`.

- **BUS-LOC-002**: ISO code validator MUST enforce format: `.Matches("^[A-Z]{2,3}$")`.

- **BUS-LOC-003**: `DeleteState` MUST check for referential integrity (addresses, orders) before hard-deleting.

### 3.9 Shared Infrastructure (INF-SHR)

- **INF-SHR-001**: `ExceptionBehavior` MUST NOT append `$" Exception: {ex.Message}"` to the API error response. Exception details go to logs only, not to clients.

- **INF-SHR-002**: `ContentSecurityPolicy` default MUST be a restrictive CSP string, not `null`.

- **INF-SHR-003**: `SecurityHeadersValidator` MUST validate `XFrameOptions` and `ReferrerPolicy` as non-empty when `IsEnabled=true`.

- **INF-SHR-004**: `ValidationBehavior` MUST use `Result.Failure()` factory instead of `(TResponse)(dynamic)validationFailures` dynamic cast.

- **INF-SHR-005**: `WebhookDispatcher.DeliverAsync` MUST NOT call `SaveChangesAsync` — let the caller (`WebhookDeliveryJob.RunAsync`) control persistence.

- **INF-SHR-006**: `WebhookSigner` — document that consumers must pre-compute `HMACSHA256(sha256(secret))` for verification, OR change to store raw secret and sign with it directly.

### 3.10 Validation Completeness (VAL)

- **VAL-001**: Every command/query handler MUST have a corresponding `CommandValidator`/`QueryValidator` class, even if empty (for consistency).

- **VAL-002**: Every admin endpoint that accepts a route `{id}` parameter MUST validate `Id.NotEmpty()` in the validator.

- **VAL-003**: Validators missing `Id.NotEmpty()`: `RevokeUserRoles`, `SyncUserRoles`, `AssignUserPermissions`, `SyncUserPermissions`, `AssignRolePermissions`, `SyncRolePermissions`, `RevokeRolePermissions`, `UpdateOrderAdmin`, `SelectShippingRate`. Add `RuleFor(x => x.Id).NotEmpty()` to each.

- **VAL-004**: `UpdateCheckout.Validator` MUST validate email format when provided: `When(x => x.Request.Email is not null, () => RuleFor(x => x.Request.Email).EmailAddress())`.

- **VAL-005**: `EmailRegister` validator MUST enforce `RuleFor(x => x.Request.AcceptTerm).Equal(true)`.

### 3.11 Code Quality (QAL)

- **QAL-001**: Replace all `DateTime.UtcNow` in log calls with the injected `ISystemDateTime.UtcNow` for testability: `ChangePassword.cs:L61`, `ResetPassword.cs:L53`, `RequestPasswordReset.cs:L44`.

- **QAL-002**: `PasswordLogin.FindUserByCredentialAsync` MUST use case-insensitive comparison on `NormalizedEmail`/`NormalizedUserName` or use `userManager.FindByEmailAsync`/`FindByNameAsync`.

- **QAL-003**: `ExternalAuthenticate` MUST remove duplicate `user.UserLogins.Add(new UserLogin { ... })` at lines 138-145 — login is already linked by `AddLoginAsync`.

- **QAL-004**: `PasswordLogin` handler MUST check `IsActive` before `CheckPasswordSignInAsync` to avoid wasted hashing and lockout increments for inactive accounts.

- **QAL-005**: Fix filename typos: `SystemInfo.Implementaion.cs` → `SystemInfo.Implementation.cs`, `Role.EntityConfiugration.cs` → `Role.EntityConfiguration.cs`.

- **QAL-006**: Fix comment typo in `Program.cs:L36`: `moudular` → `modular`.

- **QAL-007**: `RevokeUserRoles.Endpoint.cs` MUST use `MapPost` with `/revoke` path instead of `MapDelete` with `[FromBody]` (DELETE body is stripped by proxies).

- **QAL-008**: `ResumeOrder.cs:L41` MUST use a dedicated `NotificationUseCase.OrderResumed` instead of `OrderConfirmed`.

- **QAL-009**: `GetCart.cs:L27-28` MUST return `OrderResult.Errors.UserNotAuthenticated` for unauthenticated users without session, not 200 OK with empty response.

- **QAL-010**: `RemoveCartItem.Endpoint.cs` MUST declare `.Produces<Result>(StatusCodes.Status400BadRequest)`.

- **QAL-011**: `DeleteProduct.Endpoint.cs` MUST remove `.Produces<Result>(StatusCodes.Status409Conflict)` if the handler never returns 409.

- **QAL-012**: `GetPaymentById.Response.cs` MUST NOT include `IntentClientSecret`.

- **QAL-013**: `StripeWebhook.HandleChargeDisputeCreated` MUST at minimum log the dispute event — currently a no-op.

- **QAL-014**: `BulkAdjustStockItems.Validator.cs` MUST use `.GreaterThan(0)` or `.NotEqual(0)` instead of `.NotEmpty()` on `int`.

- **QAL-015**: `WebhookDeliveryJob` log MUST use `LogDebug` instead of `LogInformation` for routine runs, or only log when `due.Count > 0`.

- **QAL-016**: Profile `DeleteProfile.cs:L38` MUST return `UserProfileResult.Failure.NotFound` not `UserResult.Failure.NotFound`.

- **QAL-017**: `DeleteProfile.Endpoint.cs:L17`, `UpdateProfile.Endpoint.cs:L26`, `GetProfile.Endpoint.cs:L29` MUST use `Guid.TryParse(currentUser.UserId, out var userId)` instead of `Guid.Parse(currentUser.UserId!)`.

- **QAL-018**: `Profile.Validator.cs` MUST validate `Email` and `PhoneNumber` fields.

- **QAL-019**: `Address.Validator.Parameters.cs` MUST validate `CountryName` (NotEmpty) and add length rules for `ZipCode` and `StateProvince`.

- **QAL-020**: `UpdateWishlist` request `IsDefault` field MUST document what "default wishlist" means, or remove if unsupported.

### 3.12 Conventions (CON)

- **CON-001**: All admin endpoints MUST follow the Carter module pattern: `static partial class` with nested `Endpoint : ICarterModule`, route from metadata class, `result.ToResult()` return.

- **CON-002**: All validators MUST use domain `Result.Failure` error codes/messages: `RuleFor(...).WithErrorCode(...).WithMessage(...)`.

- **CON-003**: All handlers MUST return `Result<T>` or `Result` — never throw exceptions for expected business failures.

- **CON-004**: New `WebhooksFeature` metadata class MUST define `Tags`, `Route`, `Permission`, `Summary`, `Description` for each endpoint — following `IdentityFeature` pattern.

- **CON-005**: `DeleteWishlist` MUST use the shared `SoftDeletable` behavior or equivalent domain method, not manual property assignment.

## 4. Interfaces & Data Contracts

### 4.1 Webhooks Authorization — New Feature Metadata

```csharp
// File: Webhooks/Features/Admin/WebhooksFeature.cs (new)
public static class WebhooksFeature
{
    public const string Module = "Webhooks";
    public static class Tags
    {
        public const string Subscription = "Webhooks-Subscriptions";
    }
    public static class Admin
    {
        public static class Subscriptions
        {
            public static class Create
            {
                public const string Route = "api/webhooks/subscriptions";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Create");
                public const string Summary = "Create a webhook subscription";
                public const string Description = "Creates a new webhook subscription for receiving events.";
            }
            // ... GetById, GetPaged, Update, Delete, Test following same pattern
        }
    }
}
```

### 4.2 SSRF URL Validation Extension

```csharp
// File: Shared/Operational/Webhooks/Domain/WebhookSubscription.Validation.cs (new or extend existing)
public static class WebhookUrlValidator
{
    private static readonly string[] AllowedSchemes = ["https"];
    private static readonly string[] BlockedHosts = ["127.0.0.1", "0.0.0.0", "169.254.169.254"];
    private static readonly IPNetwork[] PrivateRanges = [
        new(IPAddress.Parse("10.0.0.0"), 8),
        new(IPAddress.Parse("172.16.0.0"), 12),
        new(IPAddress.Parse("192.168.0.0"), 16),
        new(IPAddress.Parse("127.0.0.0"), 8),
        new(IPAddress.Parse("169.254.0.0"), 16),
    ];

    public static Result ValidateUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return Error.Validation("Webhooks.Subscription.Url.Invalid", "URL must be a valid absolute URI.");
        if (!AllowedSchemes.Contains(uri.Scheme))
            return Error.Validation("Webhooks.Subscription.Url.Scheme", "Only HTTPS URLs are allowed.");
        if (BlockedHosts.Contains(uri.Host))
            return Error.Validation("Webhooks.Subscription.Url.Blocked", "This hostname is not allowed.");
        // DNS resolution + private range check at delivery time, not validation time
        return Result.Ok();
    }
}
```

### 4.3 Atomic Stock Operations — Example Pattern

```csharp
// Existing pattern to follow: ExecuteUpdateAsync with atomic arithmetic
// File: Inventory/Features/Admin/StockItems/BulkAdjust/BulkAdjustStockItems.cs (fix)
foreach (var item in command.Request.Items)
{
    var affected = await dbContext.Set<StockItem>()
        .Where(x => x.Id == item.StockItemId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.CountOnHand, x => x.CountOnHand + item.Quantity),
        cancellationToken);

    if (affected == 0)
        return StockItemResult.Errors.NotFound(item.StockItemId);
}
```

### 4.4 Payment Gateway Verification — Interface

```csharp
// StripeGateway must expose intent status retrieval
// File: Payment/Infrastructure/Gateways/Stripe/StripeGateway.cs (add)
public async Task<string> GetPaymentIntentStatusAsync(string paymentIntentId, CancellationToken ct)
{
    var intent = await new PaymentIntentService().GetAsync(paymentIntentId, null, null, ct);
    return intent.Status;
}
```

### 4.5 WebhookSigner — Consumer Contract

```csharp
// The HMAC signature header sent to consumers:
// X-Webhook-Signature: HMACSHA256(sha256(secret), payload)
//
// Consumer verification:
// 1. Pre-compute: expectedKey = SHA256(rawSecret)
// 2. Compute: expectedSig = HMAC-SHA256(expectedKey, requestBody)
// 3. Compare: constant-time compare(expectedSig, receivedSig)
//
// If changing to raw-secret signing:
// 1. Compute: expectedSig = HMAC-SHA256(rawSecret, requestBody)
// 2. Compare: constant-time compare(expectedSig, receivedSig)
```

## 5. Acceptance Criteria

### Authorization

- **AC-AUTH-001**: Given an unauthenticated request, When hitting any admin endpoint in Webhooks/Identity/Catalog/Inventory/Ordering/Payment/Shipping/Location, Then the response is 401 Unauthorized.
- **AC-AUTH-002**: Given an authenticated user without admin permissions, When hitting any admin endpoint with `.HasPermission()`, Then the response is 403 Forbidden.
- **AC-AUTH-003**: Given an anonymous request, When hitting `POST api/webhooks/subscriptions`, Then the response is 401 (not 200 or 400).

### SSRF

- **AC-SSRF-001**: Given a webhook URL `http://10.0.0.1:8080/hook`, When creating a subscription, Then the response is 400 with error code `Webhooks.Subscription.Url.Scheme`.
- **AC-SSRF-002**: Given a webhook URL `https://internal.company.com/admin`, When creating a subscription and the hostname resolves to `10.x.x.x`, Then the delivery is blocked and an error is logged.

### Stock Integrity

- **AC-STOCK-001**: Given 2 concurrent `AddToCart` requests for the same variant with 1 unit remaining, When both execute simultaneously, Then only 1 succeeds and the other returns `InsufficientStock`.
- **AC-STOCK-002**: Given a cart reservation is released, When `ReleaseCartReservation` completes, Then `StockItem.CountOnHand` is unchanged (only `StockReservation.State` changes to `Released`).
- **AC-STOCK-003**: Given an InTransit stock transfer is canceled, When `CancelStockTransfer` completes, Then a `StockMovement` record with `action: "transfer_canceled"` exists for each restored item.
- **AC-STOCK-004**: Given 2 concurrent bulk adjustments to the same stock item, When both execute, Then the final `CountOnHand` is the sum of both adjustments (no silent overwrite).

### Payment Integrity

- **AC-PAY-001**: Given a Stripe PaymentIntent in `requires_action` state, When `ConfirmPayment` is called, Then the response is an error (not success).
- **AC-PAY-002**: Given a webhook `charge.refunded` with `AmountRefunded = 50` and current `RefundedAmount = 30`, When the handler completes, Then `RefundedAmount = 50` (delta of 20 applied via `payment.Refund(20)`).
- **AC-PAY-003**: Given `RefundPayment` with `Amount = 25` on a $100 payment, When the handler completes, Then `RefundedAmount = 25` in the response (not $100).
- **AC-PAY-004**: Given a payment with amount `19.995`, When sent to Stripe, Then the Stripe amount is `2000` (not `1999`).

### Ordering Integrity

- **AC-ORDER-001**: Given an admin cancels a Placed order via `UpdateOrderStatus`, When the handler completes, Then all line item quantities are released back to stock.
- **AC-ORDER-002**: Given a Draft order, When an admin calls `UpdateOrderAdmin`, Then the change succeeds. Given a Placed order, When the same call is made, Then the response is a validation error.
- **AC-ORDER-003**: Given a guest cart with 3 items, When the guest associates with an account, Then the cart is transferred. When the guest then calls `EmptyCart`, Then the session-based cart is emptied.

### Catalog Integrity

- **AC-CAT-001**: Given a product with slug "Men's Clothing", When `CreateTaxonomy` is called, Then the stored slug is normalized (special chars handled).
- **AC-CAT-002**: Given two variants with SKU "ABC-123", When `AddVariant` is called for a third product, Then the response is a conflict error.

### Validation

- **AC-VAL-001**: Given a `RevokeUserRoles` request with `Id = Guid.Empty`, When validation runs, Then the response is 400 with `IdRequired` error.
- **AC-VAL-002**: Given an `EmailRegister` request with `AcceptTerm = false`, When validation runs, Then the response is 400.

### Code Quality

- **AC-QAL-001**: Given an inactive user, When `PasswordLogin` is called, Then the `IsActive` check runs before the password hash comparison.
- **AC-QAL-002**: Given an existing external login, When `ExternalAuthenticate` completes, Then exactly 1 `UserLogin` record exists (no duplicates).

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for handlers and validators; Integration tests for endpoints (Docker-dependent); Manual `.http` tests in `ApiTests/`.
- **Frameworks**: xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing.
- **Test Data Management**: Use `Faker` libraries for test data; clean up via EF Core in-memory or testcontainers.
- **CI/CD Integration**: `dotnet build` (warnings-as-errors), `dotnet test service/Api/tests/Module.UnitTests` for fast feedback.
- **Coverage Requirements**: Minimum 80% line coverage on new/modified handlers.
- **Security Testing**: Manual penetration testing for SSRF, auth bypass, and race conditions before production.
- **Race Condition Testing**: Use `Task.WhenAll` with multiple concurrent requests in integration tests to validate atomicity.

## 7. Rationale & Context

The 82 findings were identified through a full-codebase review of 178 endpoint files across 9 modules. The specification is organized by domain area (Security, Stock, Payment, Order, etc.) rather than by module, because many issues span multiple modules (e.g., stock race conditions appear in Inventory, Ordering, and Catalog).

Key design decisions:
- **`.HasPermission()` is the canonical auth gate** — `.RequireAuthorization()` before it is a convention, not a requirement. The fix for Webhooks is to add `.HasPermission()` with proper `PermissionMetadata`.
- **Soft reservation model is preserved** — `CountOnHand` is not decremented during reserve. The bug was that release was incrementing it. The fix removes the increment.
- **Atomic operations use `ExecuteUpdateAsync`** — this is the simplest approach that works with EF Core without requiring raw SQL or distributed locks. For more complex scenarios (checkout with multiple items), `SELECT ... FOR UPDATE` within a serializable transaction is recommended.
- **No new dependencies** — all fixes use existing EF Core, FluentValidation, and ASP.NET Core primitives.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe API — Payment processing, webhook events. Required for `ConfirmPayment` gateway verification.
- **EXT-002**: ML Sidecar (Fashion-CLIP) — Image embedding generation. Required for `GetSimilarProducts` correctness.

### Third-Party Services
- **SVC-001**: Stripe — Idempotent payment operations. The `StripeConfiguration.ApiKey` race condition fix requires understanding of Stripe SDK thread safety.

### Infrastructure Dependencies
- **INF-001**: PostgreSQL with `FOR UPDATE` support — Required for atomic stock operations in checkout flow.
- **INF-002**: Redis — HybridCache for product data. Not directly affected by this spec.

### Data Dependencies
- **DAT-001**: `CatalogSchema.TableNames` constants — Must match raw SQL in `GetSimilarProducts`.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 (preview) — `ExecuteUpdateAsync` is available since EF Core 7. No version constraint added.
- **PLT-002**: EF Core + Npgsql — `FOR UPDATE` requires Npgsql-specific extensions.

### Compliance Dependencies
- **COM-001**: PCI DSS — Payment card data must not be logged. `StripeConfiguration.ApiKey` race condition is a compliance risk.

## 9. Examples & Edge Cases

### Race Condition — Concurrent Reserve

```csharp
// WRONG: Two reads, two writes (TOCTOU)
var available = stockItem.CountOnHand - reserved; // Thread A reads 1
// Thread B reads 1 (same stockItem, same reserved)
if (available < quantity) return InsufficientStock; // Both pass
dbContext.Add(reservation); // Both insert → over-reservation

// RIGHT: Atomic operation
var affected = await dbContext.Set<StockItem>()
    .Where(x => x.Id == stockItemId
        && (x.CountOnHand - dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == x.VariantId
                && r.State == ReservationState.Reserved
                && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .Sum(r => r.Quantity)) >= quantity)
    .ExecuteUpdateAsync(s => s.SetProperty(x => x.CountOnHand, x => x.CountOnHand));
if (affected == 0) return InsufficientStock;
```

### SSRF — Private IP Detection

```csharp
// WRONG: Only validates URL format
if (string.IsNullOrEmpty(url)) return Error.Validation(...);

// RIGHT: Validates scheme, blocks private IPs
var uri = new Uri(url);
if (uri.Scheme != "https") return Error.Validation("Scheme", "Only HTTPS allowed");
if (IPAddress.TryParse(uri.Host, out var ip))
{
    if (ip.IsPrivate()) return Error.Validation("Blocked", "Private IPs not allowed");
}
```

### Payment Confirmation — Gateway Check

```csharp
// WRONG: Marks complete without verification
var completeResult = payment.Complete();

// RIGHT: Verifies with Stripe first
var status = await _gateway.GetPaymentIntentStatusAsync(payment.ResponseCode, ct);
if (status != "succeeded")
    return PaymentResult.Failure.NotSucceeded;
var completeResult = payment.Complete();
```

### Stock Release — Correct Soft Reservation

```csharp
// WRONG: Increments CountOnHand (stock was never decremented)
stockItem.CountOnHand += reservation.Quantity;

// RIGHT: Only updates reservation state
reservation.State = ReservationState.Released;
reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
// No stockItem modification — CountOnHand was never decremented during reserve
```

### External Auth — Duplicate Login Prevention

```csharp
// WRONG: Adds login twice
await userManager.AddLoginAsync(user, info); // First add
user.UserLogins.Add(new UserLogin { ... });  // Duplicate!

// RIGHT: Single add
await userManager.AddLoginAsync(user, info);
// No manual UserLogins.Add
```

## 10. Validation Criteria

| ID | Criterion | Verification Method |
|----|-----------|-------------------|
| VC-001 | All admin endpoints have `.HasPermission()` | Grep for `MapPost\|MapGet\|MapPut\|MapDelete` in `Features/Admin/` and verify each has `.HasPermission()` |
| VC-002 | All Webhooks admin endpoints have authorization | Grep for `api/webhooks` in endpoint files, verify `.HasPermission()` chain |
| VC-003 | Stock operations are atomic | Unit test: concurrent requests to same variant, verify no oversell |
| VC-004 | `ReleaseCartReservation` does not modify `CountOnHand` | Unit test: reserve then release, verify `CountOnHand` unchanged |
| VC-005 | `ConfirmPayment` queries gateway | Code review: verify gateway call before `payment.Complete()` |
| VC-006 | All validators have `Id.NotEmpty()` | Grep for `AbstractValidator<Command>` in `Features/Admin/`, verify `Id` rule |
| VC-007 | No `DateTime.UtcNow` in log calls | Grep for `DateTime.UtcNow` in `*.cs` files |
| VC-008 | Webhook URLs validated for scheme and private IPs | Unit test: create subscription with `http://` and `127.0.0.1` URLs |
| VC-009 | `GetSimilarProducts` SQL uses correct table names | Integration test: call endpoint, verify no `PostgresException` |
| VC-010 | `RefundPayment` response matches actual refund amount | Unit test: refund $25 of $100, verify response `RefundedAmount = 25` |

## 11. Related Specifications / Further Reading

- `.harness/domains.yml` — Domain boundaries and layer maps
- `.harness/principles.yml` — Golden principles (Result objects, no module cross-references, warnings-as-errors)
- `.harness/enforcement.yml` — Naming, file limits, logging rules
- `docs/codebase/ARCHITECTURE.md` — Detailed architecture and data flow
- `docs/codebase/CONVENTIONS.md` — Coding conventions
- `docs/codebase/TESTING.md` — Testing strategy
- `service/Api/src/Shared/Security/Authorization/Attributes/HasPermission.Attribute.Extension.cs` — How `.HasPermission()` works
- `service/Api/src/Shared/Application/Models/Results/ValueResult.cs` — `Result<T>` definition
- `plan/` — 62 implementation plans for reference
