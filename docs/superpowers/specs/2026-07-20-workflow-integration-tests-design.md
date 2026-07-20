# Workflow Integration Tests — Design Spec

## Goal

Replace smoke tests with cross-module workflow integration tests that exercise critical
business paths end-to-end through the real HTTP pipeline, PostgreSQL, and Respawn reset.

## Scope

Four workflow tests. No new infrastructure — reuse existing `ApiFixture`, Respawn,
HTTP helpers, and AuthTokenHelper from `Api.Tests`.

## What's Dropped

- `Scenarios/HealthCheckTests.cs`
- `Scenarios/Host/HealthCheckReadinessTests.cs`
- `Api.Marker.Tests.cs`

## What's Added

```
Scenarios/Workflows/
├── WorkflowTestBase.cs
├── GuestRegistrationWorkflowTests.cs
├── BrowseCatalogToCheckoutWorkflowTests.cs
├── AdminCreateCustomerBuyWorkflowTests.cs
└── ForgotPasswordResetWorkflowTests.cs
```

## Architecture

### WorkflowTestBase

Extends `ApiIntegrationTestBase`. Provides reusable step methods. Each method
returns `Result<T>` — tests compose steps explicitly. No hidden state.

```csharp
public abstract class WorkflowTestBase : ApiIntegrationTestBase
{
    protected WorkflowTestBase(ApiFixture fixture) : base(fixture) { }

    // Auth helpers
    protected async Task<Result<LoginResponse>> RegisterAsync(string email, string password);
    protected async Task<Result<LoginResponse>> LoginAsync(string email, string password);
    protected void SetAuthToken(string token);
    protected void ClearAuth();

    // Catalog helpers
    protected Task<Result<PagedResult<ProductSummary>>> BrowseProductsAsync(int page = 1);

    // Cart helpers
    protected Task<Result<CartResponse>> AddToCartAsync(Guid productId, int quantity);
    protected Task<Result<CartResponse>> GetCartAsync();

    // Checkout helpers
    protected Task<Result<AddressResponse>> SetAddressAsync(object address);
    protected Task<Result<ShippingMethodsResponse>> GetShippingMethodsAsync();
    protected Task<Result<OrderResponse>> CheckoutAsync(Guid addressId, Guid shippingMethodId);

    // Payment helpers
    protected Task<Result<PaymentIntentResponse>> CreateIntentAsync(Guid orderId);
    protected Task<Result<PaymentConfirmationResponse>> ConfirmPaymentAsync(Guid intentId);

    // Identity helpers
    protected Task<Result> RequestPasswordResetAsync(string email);
    protected Task<Result> ResetPasswordAsync(string token, string newPassword);
    protected Task<Result> VerifyEmailAsync(string token);
}
```

### Workflow 1: GuestRegistrationWorkflowTests

**Path:** Register → Verify Email → Login → Access Protected Resource

| Step | Endpoint | Assert |
|------|----------|--------|
| 1 | POST /api/v1/auth/register | 201, tokens returned |
| 2 | POST /api/v1/auth/emails/resend-verification | 204 |
| 3 | POST /api/v1/auth/emails/confirm {token} | 204 |
| 4 | POST /api/v1/auth/login | 200, tokens match |
| 5 | GET /api/v1/profile | 200, profile exists |

### Workflow 2: BrowseCatalogToCheckoutWorkflowTests

**Path:** Browse → Add to Cart → Register → Set Address → Select Shipping → Checkout → Pay

| Step | Endpoint | Assert |
|------|----------|--------|
| 1 | GET /api/v1/catalog/products | 200, non-empty |
| 2 | POST /api/v1/cart/items | 201, stock reserved |
| 3 | POST /api/v1/auth/register | 201, tokens |
| 4 | POST /api/v1/profile/addresses | 201 |
| 5 | GET /api/v1/shipping/methods/{addressId} | 200, methods available |
| 6 | POST /api/v1/cart/checkout | 201, order created |
| 7 | POST /api/v1/payment/intent | 200, intent created |
| 8 | POST /api/v1/payment/confirm | 200, order advances |
| 9 | GET /api/v1/cart | 200, cart empty |

### Workflow 3: AdminCreateCustomerBuyWorkflowTests

**Path:** Admin Creates Product → Customer Browses → Customer Buys → Admin Views Order

| Step | Endpoint | Assert |
|------|----------|--------|
| 1 | POST /api/v1/admin/catalog/products | 201 (as admin) |
| 2 | GET /api/v1/catalog/products | 200, product visible (as guest) |
| 3 | POST /api/v1/cart/items | 201 (as customer) |
| 4 | POST /api/v1/cart/checkout | 201, order created |
| 5 | GET /api/v1/admin/ordering/orders/{id} | 200, order found (as admin) |

### Workflow 4: ForgotPasswordResetWorkflowTests

**Path:** Request Reset → Reset → Login (New PW) → Login (Old PW Fails)

Precondition: User registered via `RegisterAsync`.

| Step | Endpoint | Assert |
|------|----------|--------|
| 1 | POST /api/v1/auth/passwords/forgot {email} | 204 (no info leak on success) |
| 2 | Query DB for reset token (notification is disabled in test config; token is persisted) |
| 3 | POST /api/v1/auth/passwords/reset {token, newPw} | 204 |
| 4 | POST /api/v1/auth/login {newPw} | 200 |
| 5 | POST /api/v1/auth/login {oldPw} | 401 |

## Error Handling

Each step is an independent HTTP call. If a step fails, the test fails with the HTTP
status and response body for diagnostics. No cleanup needed — Respawn resets the DB
between tests.

## Test Configuration Dependencies

- Email/SMS notifications disabled (existing `ApiFactory` config) — tokens persisted in DB, readable by tests
- Payment gateway in test mode / mocked (existing setup) — no real Stripe calls
- Background jobs disabled (existing) — any fire-and-forget work tested inline
- Anti-forgery disabled (existing) — all POST/PUT/DELETE calls work without tokens
- Caching disabled (existing) — fresh reads every time

## Testing

- All 4 tests run against the same `ApiFixture` (collection `"ApiIntegration"`)
- Respawn full-schema reset between each test ensures isolation
- Tests are run with `dotnet test service/Api/tests/Api.Tests` (requires Docker)
