# Workflow Integration Tests — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace smoke tests with 4 cross-module workflow integration tests exercising critical business paths through the full HTTP pipeline.

**Architecture:** New `WorkflowTestBase` extends `ApiIntegrationTestBase` with reusable step helpers. Four test classes in `Scenarios/Workflows/`, each one test method chaining 5-9 endpoint calls. Existing `ApiFixture`, Respawn, and HTTP helpers unchanged.

**Tech Stack:** xUnit v3, FluentAssertions, Testcontainers PostgreSQL, Respawn, `WebApplicationFactory<Program>`

## Global Constraints

- Extend `ApiIntegrationTestBase` — use `[Collection("ApiIntegration")]`, constructor-injected `ApiFixture`
- All tests go through the full HTTP pipeline via `Fixture.Client`
- Use `response.ReadApiResponseAsync()` → `ApiResponse` pattern for assertions
- Use `ApiResponse.StatusCode`, `ApiResponse.IsSuccess`, `ApiResponse.DeserializeValue<T>()`
- Admin auth: `IdentityTestHelper.CreateAdminRequest()` or `Client.PostAsAdminRawAsync()`
- User auth: `IdentityTestHelper.GenerateUserToken(userId, email)` with `AuthenticationHeaderValue("Bearer", token)`
- Test user creation: `IdentityTestHelper.CreateTestUserAsync(Client)` returns `(Guid Id, string Email, string UserName)`
- DB-scope access for seeding: `Fixture.Factory.Services.CreateScope()` + `IApplicationDbContext`
- Direct `UserManager<User>` resolution via scope for `GeneratePasswordResetTokenAsync`
- No new infrastructure — no new packages, no new fixtures
- Tests must pass with `dotnet test service/Api/tests/Api.Tests` (Docker required)

## File Map

| File | Create/Modify/Delete | Responsibility |
|------|---------------------|----------------|
| `Scenarios/HealthCheckTests.cs` | Delete | Smoke test — replaced by workflow tests |
| `Scenarios/Host/HealthCheckReadinessTests.cs` | Delete | Smoke test — replaced by workflow tests |
| `Api.Marker.Tests.cs` | Delete | Trivial preflight test — no longer needed |
| `Scenarios/Workflows/WorkflowTestBase.cs` | Create | Base class with reusable step helpers |
| `Scenarios/Workflows/GuestRegistrationWorkflowTests.cs` | Create | Register → Verify → Login → Profile |
| `Scenarios/Workflows/BrowseCatalogToCheckoutWorkflowTests.cs` | Create | Browse → Cart → Register → Address → Shipping → Checkout → Pay |
| `Scenarios/Workflows/AdminCreateCustomerBuyWorkflowTests.cs` | Create | Admin creates product → Customer browses → Customer buys → Admin views order |
| `Scenarios/Workflows/ForgotPasswordResetWorkflowTests.cs` | Create | Register → Forgot → Reset → Login new PW → Login old PW fails |

---

### Task 1: Drop smoke tests and create WorkflowTestBase

**Files:**
- Delete: `service/Api/tests/Api.Tests/Scenarios/HealthCheckTests.cs`
- Delete: `service/Api/tests/Api.Tests/Scenarios/Host/HealthCheckReadinessTests.cs`
- Delete: `service/Api/tests/Api.Tests/Api.Marker.Tests.cs`
- Create: `service/Api/tests/Api.Tests/Scenarios/Workflows/WorkflowTestBase.cs`

**Interfaces:**
- Consumes: `ApiIntegrationTestBase`, `ApiFixture`, `IdentityTestHelper`
- Produces: `WorkflowTestBase` with step helpers (listed below)

- [ ] **Step 1: Delete smoke test files**

```bash
rm service/Api/tests/Api.Tests/Scenarios/HealthCheckTests.cs
rm service/Api/tests/Api.Tests/Scenarios/Host/HealthCheckReadinessTests.cs
rm service/Api/tests/Api.Tests/Api.Marker.Tests.cs
```

- [ ] **Step 2: Create WorkflowTestBase.cs**

```csharp
using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Workflows;

[Collection("ApiIntegration")]
public abstract class WorkflowTestBase : ApiIntegrationTestBase
{
    protected WorkflowTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuth()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected static (string Email, string Password, string UserName) TestCredentials()
    {
        string email = IdentityTestHelper.ValidEmail();
        string userName = IdentityTestHelper.ValidUserName();
        return (email, IdentityTestHelper.ValidPassword, userName);
    }
}
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/tests/Api.Tests
```
Expected: 0 warnings, 0 errors

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Api.Tests/
git commit -m "test: drop smoke tests, add WorkflowTestBase for cross-module workflow tests"
```

---

### Task 2: GuestRegistrationWorkflowTests

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Workflows/GuestRegistrationWorkflowTests.cs`

**Interfaces:**
- Consumes: `WorkflowTestBase`, `IdentityTestHelper`, `ApiResponse`, `ResponseHelperExtensions`
- Produces: `GuestRegistrationWorkflowTests` with `Guest_Register_VerifyEmail_Login_AccessProfile`

- [ ] **Step 1: Write the test**

```csharp
using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Workflows;

public sealed class GuestRegistrationWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    [Fact]
    public async Task Guest_Register_VerifyEmail_Login_AccessProfile()
    {
        var (email, password, userName) = TestCredentials();

        // Step 1: Register
        var registerBody = new
        {
            email,
            userName,
            password,
            firstName = "Workflow",
            lastName = "Tester"
        };

        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", registerBody);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse registerResult = await registerResponse.ReadApiResponseAsync();
        registerResult.IsSuccess.Should().BeTrue();

        // Step 2: Request email verification resend
        var resendBody = new { email };
        HttpResponseMessage resendResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/emails/resend", resendBody);
        resendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 3: Confirm email — resolve token from DB
        string token;
        Guid userId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Shared.Security.Identity.Domain.Users.User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            userId = user!.Id;
            token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        var confirmBody = new { userId = userId.ToString(), token };
        HttpResponseMessage confirmResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/emails/confirm", confirmBody);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 4: Login
        var loginBody = new { credential = email, password };
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/password", loginBody);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse loginResult = await loginResponse.ReadApiResponseAsync();
        loginResult.IsSuccess.Should().BeTrue();

        // Step 5: Access profile with user token
        string userToken = IdentityTestHelper.GenerateUserToken(userId, email);
        SetAuthToken(userToken);

        HttpResponseMessage profileResponse = await Client.GetAsync("/api/store/profiles");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse profileResult = await profileResponse.ReadApiResponseAsync();
        profileResult.IsSuccess.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/tests/Api.Tests
```
Expected: 0 warnings, 0 errors

- [ ] **Step 3: Run the test (requires Docker)**

```bash
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~GuestRegistrationWorkflowTests"
```
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Api.Tests/
git commit -m "test: add GuestRegistrationWorkflowTests — register, verify, login, access profile"
```

---

### Task 3: BrowseCatalogToCheckoutWorkflowTests

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Workflows/BrowseCatalogToCheckoutWorkflowTests.cs`

**Interfaces:**
- Consumes: `WorkflowTestBase`, `ApiFixture`, `IApplicationDbContext`, `StockLocationMethod`, `StockItemMethod`
- Produces: `BrowseCatalogToCheckoutWorkflowTests` with `Guest_Browse_AddToCart_Register_Address_Shipping_Checkout_Pay`

- [ ] **Step 1: Write the test**

```csharp
using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Features.Admin.Products.Create;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Workflows;

public sealed class BrowseCatalogToCheckoutWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    private record CartItemResponse
    {
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
    }

    private record CartResponse
    {
        public List<CartItemResponse> Items { get; init; } = [];
    }

    [Fact]
    public async Task Guest_Browse_AddToCart_Register_Checkout()
    {
        ClearAuth();

        // Step 1: Admin creates product
        var slug = $"wf-checkout-{Guid.NewGuid():N}";
        var createBody = new { name = "Workflow Checkout Product", slug, description = "Test" };

        HttpResponseMessage createProductResp = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createBody);
        ApiResponse createProductResult = await createProductResp.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        var product = createProductResult.DeserializeValue<CreateProduct.Response>();
        product.Should().NotBeNull();

        // Step 2: Admin activates product
        HttpResponseMessage activateResp = await Client.PatchAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/activate");
        activateResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 3: Admin seeds stock via DB (no admin stock endpoint exists)
        Guid variantId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var hasLocation = await db.Set<StockLocation>().AnyAsync(sl => sl.Default);
            if (!hasLocation)
            {
                var locResult = StockLocationMethod.Create(
                    "Test Warehouse", "Test Warehouse", "TEST",
                    true, true, true);
                db.Set<StockLocation>().Add(locResult.Value);
                await db.SaveChangesAsync();
            }

            var location = await db.Set<StockLocation>().FirstAsync(sl => sl.Default);
            variantId = product.MasterVariantId;

            var hasStock = await db.Set<StockItem>().AnyAsync(si => si.VariantId == variantId);
            if (!hasStock)
            {
                var stockResult = StockItemMethod.Create(
                    location.Id, variantId, 100, true);
                db.Set<StockItem>().Add(stockResult.Value);
                await db.SaveChangesAsync();
            }
        }

        // Step 4: Guest browses products
        ClearAuth();
        var browseResp = await Client.GetAsync("/api/storefront/catalog/products?page=1&pageSize=10");
        browseResp.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse browseResult = await browseResp.ReadApiResponseAsync();
        browseResult.IsSuccess.Should().BeTrue();

        // Step 5: Guest adds item to cart
        var addItemBody = new { variantId, quantity = 2 };

        HttpResponseMessage addItemResp = await Client.PostAsJsonAsync(
            "/api/storefront/cart/items", addItemBody);
        addItemResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 6: Register as customer
        var (email, password, userName) = TestCredentials();
        var registerBody = new { email, userName, password, firstName = "Cart", lastName = "User" };

        HttpResponseMessage registerResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", registerBody);
        registerResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 7: Login
        var loginBody = new { credential = email, password };
        HttpResponseMessage loginResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/password", loginBody);
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 8: Get user ID and set auth
        Guid userId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Shared.Security.Identity.Domain.Users.User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            userId = user!.Id;
        }
        SetAuthToken(IdentityTestHelper.GenerateUserToken(userId, email));

        // Step 9: Set shipping address
        var addressBody = new
        {
            addressType = "Shipping",
            firstName = "Test",
            lastName = "User",
            address1 = "123 Test St",
            city = "TestCity",
            stateName = "California",
            countryName = "United States",
            postalCode = "90210",
            phone = "555-0100"
        };

        HttpResponseMessage addressResp = await Client.PostAsJsonAsync(
            "/api/store/profiles/addresses", addressBody);
        addressResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 10: Checkout
        var checkoutBody = new { paymentIntentId = (string?)null };
        HttpResponseMessage checkoutResp = await Client.PostAsJsonAsync(
            "/api/storefront/cart/checkout", checkoutBody);
        checkoutResp.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/tests/Api.Tests
```
Expected: 0 warnings, 0 errors

- [ ] **Step 3: Run the test (requires Docker)**

```bash
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~BrowseCatalogToCheckoutWorkflowTests"
```
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Api.Tests/
git commit -m "test: add BrowseCatalogToCheckoutWorkflowTests — browse, cart, register, address, checkout"
```

---

### Task 4: AdminCreateCustomerBuyWorkflowTests

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Workflows/AdminCreateCustomerBuyWorkflowTests.cs`

**Interfaces:**
- Consumes: `WorkflowTestBase`, `ApiFixture`, `IApplicationDbContext`, `StockLocationMethod`, `StockItemMethod`
- Produces: `AdminCreateCustomerBuyWorkflowTests`

- [ ] **Step 1: Write the test**

```csharp
using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Features.Admin.Products.Create;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Workflows;

public sealed class AdminCreateCustomerBuyWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    [Fact]
    public async Task Admin_Creates_Product_Customer_Browses_And_Buys()
    {
        ClearAuth();

        // Step 1: Admin creates product
        var slug = $"wf-admin-{Guid.NewGuid():N}";
        var createBody = new { name = "Admin Workflow Product", slug, description = "Admin created" };

        HttpResponseMessage createResp = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createBody);
        ApiResponse createResult = await createResp.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<CreateProduct.Response>();
        product.Should().NotBeNull();

        // Step 2: Admin activates product
        HttpResponseMessage activateResp = await Client.PatchAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/activate");
        activateResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 3: Admin seeds stock
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            if (!await db.Set<StockLocation>().AnyAsync(sl => sl.Default))
            {
                var locResult = StockLocationMethod.Create(
                    "Warehouse", "Warehouse", "WH", true, true, true);
                db.Set<StockLocation>().Add(locResult.Value);
                await db.SaveChangesAsync();
            }
            var location = await db.Set<StockLocation>().FirstAsync(sl => sl.Default);
            if (!await db.Set<StockItem>().AnyAsync(si => si.VariantId == product.MasterVariantId))
            {
                var stockResult = StockItemMethod.Create(
                    location.Id, product.MasterVariantId, 50, true);
                db.Set<StockItem>().Add(stockResult.Value);
                await db.SaveChangesAsync();
            }
        }

        // Step 4: Guest browses catalog — product visible
        ClearAuth();
        var browseResp = await Client.GetAsync("/api/storefront/catalog/products?page=1&pageSize=20");
        browseResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Register customer
        var (email, password, userName) = TestCredentials();
        var registerBody = new { email, userName, password, firstName = "Buyer", lastName = "Smith" };

        HttpResponseMessage registerResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", registerBody);
        registerResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 6: Login as customer
        var loginBody = new { credential = email, password };
        HttpResponseMessage loginResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/password", loginBody);
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 7: Add product to cart
        Guid userId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Shared.Security.Identity.Domain.Users.User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            userId = user!.Id;
        }
        SetAuthToken(IdentityTestHelper.GenerateUserToken(userId, email));

        var addItemBody = new { variantId = product.MasterVariantId, quantity = 1 };
        HttpResponseMessage addResp = await Client.PostAsJsonAsync(
            "/api/storefront/cart/items", addItemBody);
        addResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 8: Checkout
        var checkoutBody = new { paymentIntentId = (string?)null };
        HttpResponseMessage checkoutResp = await Client.PostAsJsonAsync(
            "/api/storefront/cart/checkout", checkoutBody);
        checkoutResp.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/tests/Api.Tests
```
Expected: 0 warnings, 0 errors

- [ ] **Step 3: Run the test (requires Docker)**

```bash
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~AdminCreateCustomerBuyWorkflowTests"
```
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Api.Tests/
git commit -m "test: add AdminCreateCustomerBuyWorkflowTests — admin creates, customer browses and buys"
```

---

### Task 5: ForgotPasswordResetWorkflowTests

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Workflows/ForgotPasswordResetWorkflowTests.cs`

**Interfaces:**
- Consumes: `WorkflowTestBase`, `IdentityTestHelper`, `UserManager<User>`
- Produces: `ForgotPasswordResetWorkflowTests`

- [ ] **Step 1: Write the test**

```csharp
using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Identity.Domain.Users;

namespace Api.Tests.Scenarios.Workflows;

public sealed class ForgotPasswordResetWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    [Fact]
    public async Task ForgotPassword_Reset_LoginNewPassword_OldPasswordFails()
    {
        ClearAuth();

        // Step 1: Register a test user
        var (email, password, userName) = TestCredentials();
        var registerBody = new { email, userName, password, firstName = "Forgot", lastName = "Pwd" };

        HttpResponseMessage registerResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", registerBody);
        registerResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 2: Request password reset
        var forgotBody = new { email };
        HttpResponseMessage forgotResp = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/forgot", forgotBody);
        forgotResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 3: Generate reset token directly via UserManager
        Guid userId;
        string resetToken;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            userId = user!.Id;
            resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        }

        // Step 4: Reset password with the token
        string newPassword = "NewSecurePass1!";
        var resetBody = new { userId = userId.ToString(), token = resetToken, newPassword };

        HttpResponseMessage resetResp = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/reset", resetBody);
        resetResp.IsSuccessStatusCode.Should().BeTrue();

        // Step 5: Login with new password
        var loginNewBody = new { credential = email, password = newPassword };
        HttpResponseMessage loginNewResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/password", loginNewBody);
        loginNewResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Login with old password fails
        var loginOldBody = new { credential = email, password };
        HttpResponseMessage loginOldResp = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/password", loginOldBody);
        loginOldResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build service/Api/tests/Api.Tests
```
Expected: 0 warnings, 0 errors

- [ ] **Step 3: Run the test (requires Docker)**

```bash
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~ForgotPasswordResetWorkflowTests"
```
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Api.Tests/
git commit -m "test: add ForgotPasswordResetWorkflowTests — forgot, reset, login new, old fails"
```

---

### Task 6: Run full workflow test suite and verify

- [ ] **Step 1: Run all 4 workflow tests**

```bash
dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~Workflows"
```
Expected: PASS

- [ ] **Step 2: Run all integration tests to confirm no regressions**

```bash
dotnet test service/Api/tests/Api.Tests
```
Expected: PASS

- [ ] **Step 3: Commit if any remaining changes**

```bash
git status
```
