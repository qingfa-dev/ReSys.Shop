# Plan 1: Security Hardening — Authorization & SSRF

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add authorization to all unprotected admin endpoints and SSRF protection to webhook URLs.

**Architecture:** Add `.HasPermission(PermissionMetadata)` to 6 Webhooks admin endpoints. Create `WebhooksFeature` metadata class. Add URL validation to webhook subscriptions. Restrict Profile PII endpoint.

**Tech Stack:** .NET 10, Carter, ASP.NET Core Authorization, FluentValidation

## Global Constraints

- `.HasPermission(PermissionMetadata)` internally chains `.RequireAuthorization(new HasPermissionAttribute(permission))` — the canonical auth gate.
- All admin endpoints MUST have `.HasPermission()` chained on route handler.
- All fixes must respect existing `static partial class` vertical slice pattern.
- `TreatWarningsAsErrors=true` globally — any warning fails the build.

---

## File Structure

| Action | File | Responsibility |
|--------|------|---------------|
| Create | `service/Api/src/Module/Webhooks/Features/Admin/WebhooksFeature.cs` | Route constants, PermissionMetadata, tags for all 6 endpoints |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Create/CreateWebhookSubscription.Endpoint.cs` | Add `.HasPermission()` chain |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Update/UpdateWebhookSubscription.Endpoint.cs` | Add `.HasPermission()` chain |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Delete/DeleteWebhookSubscription.Endpoint.cs` | Add `.HasPermission()` chain |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Get/ById/GetWebhookSubscriptionById.Endpoint.cs` | Add `.HasPermission()` chain |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Get/Paged/GetWebhookSubscriptions.Endpoint.cs` | Add `.HasPermission()` chain |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Test/TestWebhookSubscription.Endpoint.cs` | Add `.HasPermission()` chain |
| Modify | `service/Api/src/Shared/Operational/Webhooks/Domain/WebhookSubscription.Method.cs` | Add URL validation (scheme, private IP) |
| Create | `service/Api/src/Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs` | URL validation helper |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Create/CreateWebhookSubscription.cs` | Call URL validation in handler |
| Modify | `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Update/UpdateWebhookSubscription.cs` | Call `subscription.Update()` instead of direct assignment |
| Modify | `service/Api/src/Module/Profile/Features/Store/Profiles/Get/PagedOrAll/GetProfilesPagedOrAll.Endpoint.cs` | Add admin-only restriction |

---

### Task 1: Create WebhooksFeature Metadata Class

**Files:**
- Create: `service/Api/src/Module/Webhooks/Features/Admin/WebhooksFeature.cs`
- Reference: `service/Api/src/Module/Identity/Features/IdentityFeature.cs` (pattern to follow)

**Interfaces:**
- Produces: `WebhooksFeature.Admin.Subscriptions.{Action}.Permission` (PermissionMetadata), `.Route` (string), `.Summary`, `.Description`, `.Tags.Subscription` (string)

- [ ] **Step 1: Read the IdentityFeature pattern**

Read `service/Api/src/Module/Identity/Features/IdentityFeature.cs` to understand the metadata class structure.

- [ ] **Step 2: Create WebhooksFeature.cs**

```csharp
namespace Module.Webhooks.Features.Admin;

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

            public static class GetById
            {
                public const string Route = "api/webhooks/subscriptions/{id}";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "GetById");
                public const string Summary = "Get a webhook subscription by ID";
                public const string Description = "Retrieves a specific webhook subscription.";
            }

            public static class GetPaged
            {
                public const string Route = "api/webhooks/subscriptions";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "GetPaged");
                public const string Summary = "List webhook subscriptions";
                public const string Description = "Retrieves a paged list of webhook subscriptions.";
            }

            public static class Update
            {
                public const string Route = "api/webhooks/subscriptions/{id}";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Update");
                public const string Summary = "Update a webhook subscription";
                public const string Description = "Updates an existing webhook subscription.";
            }

            public static class Delete
            {
                public const string Route = "api/webhooks/subscriptions/{id}";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Delete");
                public const string Summary = "Delete a webhook subscription";
                public const string Description = "Deletes a webhook subscription.";
            }

            public static class Test
            {
                public const string Route = "api/webhooks/subscriptions/{id}/test";
                public static readonly PermissionMetadata Permission = new("Webhooks", "Admin", "Subscriptions", "Test");
                public const string Summary = "Test a webhook subscription";
                public const string Description = "Sends a test event to the webhook subscription URL.";
            }
        }
    }
}
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Webhooks/Module.Webhooks.csproj`
Expected: Build succeeds (no warnings due to TreatWarningsAsErrors)

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Webhooks/Features/Admin/WebhooksFeature.cs
git commit -m "feat(webhooks): add WebhooksFeature metadata class for authorization"
```

---

### Task 2: Add Authorization to All 6 Webhooks Admin Endpoints

**Files:**
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Create/CreateWebhookSubscription.Endpoint.cs`
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Update/UpdateWebhookSubscription.Endpoint.cs`
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Delete/DeleteWebhookSubscription.Endpoint.cs`
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Get/ById/GetWebhookSubscriptionById.Endpoint.cs`
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Get/Paged/GetWebhookSubscriptions.Endpoint.cs`
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Test/TestWebhookSubscription.Endpoint.cs`

**Interfaces:**
- Consumes: `WebhooksFeature.Admin.Subscriptions.{Action}.Permission` from Task 1

- [ ] **Step 1: Read one existing endpoint to understand the pattern**

Read `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Assign/AssignUserRoles.Endpoint.cs` — note the `.RequireAuthorization().HasPermission(...)` chain.

- [ ] **Step 2: Add `.HasPermission()` to CreateWebhookSubscription.Endpoint.cs**

Find the `.WithName(nameof(CreateWebhookSubscription))` line. Before it, add:
```csharp
.HasPermission(WebhooksFeature.Admin.Subscriptions.Create.Permission)
```

The full chain should be:
```csharp
app.MapPost(WebhooksFeature.Admin.Subscriptions.Create.Route, async (...) =>
{
    ...
})
.WithName(nameof(CreateWebhookSubscription))
.WithTags(WebhooksFeature.Tags.Subscription)
.HasPermission(WebhooksFeature.Admin.Subscriptions.Create.Permission)
```

- [ ] **Step 3: Add `.HasPermission()` to UpdateWebhookSubscription.Endpoint.cs**

Same pattern — add `.HasPermission(WebhooksFeature.Admin.Subscriptions.Update.Permission)`.

- [ ] **Step 4: Add `.HasPermission()` to DeleteWebhookSubscription.Endpoint.cs**

Same pattern — add `.HasPermission(WebhooksFeature.Admin.Subscriptions.Delete.Permission)`.

- [ ] **Step 5: Add `.HasPermission()` to GetWebhookSubscriptionById.Endpoint.cs**

Same pattern — add `.HasPermission(WebhooksFeature.Admin.Subscriptions.GetById.Permission)`.

- [ ] **Step 6: Add `.HasPermission()` to GetWebhookSubscriptions.Endpoint.cs**

Same pattern — add `.HasPermission(WebhooksFeature.Admin.Subscriptions.GetPaged.Permission)`.

- [ ] **Step 7: Add `.HasPermission()` to TestWebhookSubscription.Endpoint.cs**

Same pattern — add `.HasPermission(WebhooksFeature.Admin.Subscriptions.Test.Permission)`.

- [ ] **Step 8: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Webhooks/Module.Webhooks.csproj`
Expected: Build succeeds

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/
git commit -m "feat(webhooks): add .HasPermission() to all 6 admin endpoints"
```

---

### Task 3: Create WebhookUrlValidator

**Files:**
- Create: `service/Api/src/Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs`
- Reference: `service/Api/src/Shared/Application/Models/Results/ValueResult.cs` (Result pattern)

**Interfaces:**
- Produces: `WebhookUrlValidator.ValidateUrl(string url)` → `Result`

- [ ] **Step 1: Create WebhookUrlValidator.cs**

```csharp
using System.Net;
using Shared.Application.Models.Results;

namespace Shared.Operational.Webhooks.Domain;

public static class WebhookUrlValidator
{
    private static readonly string[] AllowedSchemes = ["https"];
    private static readonly string[] BlockedHosts = ["127.0.0.1", "0.0.0.0", "169.254.169.254"];
    private static readonly (IPAddress Network, int PrefixLength)[] PrivateRanges =
    [
        (IPAddress.Parse("10.0.0.0"), 8),
        (IPAddress.Parse("172.16.0.0"), 12),
        (IPAddress.Parse("192.168.0.0"), 16),
        (IPAddress.Parse("127.0.0.0"), 8),
        (IPAddress.Parse("169.254.0.0"), 16),
    ];

    private const int MaxUrlLength = 2048;

    public static Result ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Error.Validation("Webhooks.Subscription.Url.Empty", "URL must not be empty.");

        if (url.Length > MaxUrlLength)
            return Error.Validation("Webhooks.Subscription.Url.TooLong", $"URL must not exceed {MaxUrlLength} characters.");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return Error.Validation("Webhooks.Subscription.Url.Invalid", "URL must be a valid absolute URI.");

        if (!AllowedSchemes.Contains(uri.Scheme.ToLowerInvariant()))
            return Error.Validation("Webhooks.Subscription.Url.Scheme", "Only HTTPS URLs are allowed.");

        if (BlockedHosts.Contains(uri.Host))
            return Error.Validation("Webhooks.Subscription.Url.Blocked", "This hostname is not allowed.");

        if (IPAddress.TryParse(uri.Host, out var ip))
        {
            foreach (var (network, prefixLength) in PrivateRanges)
            {
                if (IsInSubnet(ip, network, prefixLength))
                    return Error.Validation("Webhooks.Subscription.Url.Private", "Private network addresses are not allowed.");
            }
        }

        return Result.Ok();
    }

    private static bool IsInSubnet(IPAddress address, IPAddress network, int prefixLength)
    {
        var addressBytes = address.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        var maskBytes = new byte[addressBytes.Length];
        var fullBits = prefixLength;

        for (var i = 0; i < maskBytes.Length; i++)
        {
            if (fullBits >= 8)
            {
                maskBytes[i] = 255;
                fullBits -= 8;
            }
            else if (fullBits > 0)
            {
                maskBytes[i] = (byte)(255 << (8 - fullBits));
                fullBits = 0;
            }
        }

        for (var i = 0; i < addressBytes.Length; i++)
        {
            if ((addressBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                return false;
        }

        return true;
    }
}
```

- [ ] **Step 2: Verify build compiles**

Run: `dotnet build service/Api/src/Shared/Shared.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs
git commit -m "feat(webhooks): add WebhookUrlValidator with SSRF protection"
```

---

### Task 4: Wire URL Validation into CreateWebhookSubscription Handler

**Files:**
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Create/CreateWebhookSubscription.cs`

**Interfaces:**
- Consumes: `WebhookUrlValidator.ValidateUrl(url)` from Task 3

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Create/CreateWebhookSubscription.cs` to find where the subscription is created.

- [ ] **Step 2: Add URL validation before subscription creation**

In the `Handle` method, after parsing the request and before creating the subscription, add:

```csharp
var urlValidation = WebhookUrlValidator.ValidateUrl(request.Url);
if (urlValidation.IsFailure)
    return urlValidation.Errors;
```

Add the using at the top:
```csharp
using Shared.Operational.Webhooks.Domain;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Webhooks/Module.Webhooks.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Create/CreateWebhookSubscription.cs
git commit -m "feat(webhooks): validate webhook URL on creation (SSRF protection)"
```

---

### Task 5: Wire URL Validation into UpdateWebhookSubscription Handler

**Files:**
- Modify: `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Update/UpdateWebhookSubscription.cs`

**Interfaces:**
- Consumes: `WebhookUrlValidator.ValidateUrl(url)` from Task 3, `subscription.Update(url)` domain method

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Update/UpdateWebhookSubscription.cs`.

- [ ] **Step 2: Replace direct URL assignment with domain method + validation**

Find `subscription.Url = request.Url;` and replace with:

```csharp
var urlValidation = WebhookUrlValidator.ValidateUrl(request.Url);
if (urlValidation.IsFailure)
    return urlValidation.Errors;

subscription.Update(request.Url);
```

If `subscription.Update()` doesn't exist with a URL parameter, instead use:
```csharp
subscription.Url = request.Url;
```

Add the using at the top:
```csharp
using Shared.Operational.Webhooks.Domain;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Webhooks/Module.Webhooks.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Webhooks/Features/Admin/Subscriptions/Update/UpdateWebhookSubscription.cs
git commit -m "feat(webhooks): validate webhook URL on update (SSRF protection)"
```

---

### Task 6: Restrict Profile PII Endpoint

**Files:**
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Get/PagedOrAll/GetProfilesPagedOrAll.Endpoint.cs`

**Interfaces:**
- Consumes: `ProfileFeature.Admin.Profiles.Get.Permission` (or equivalent PermissionMetadata)

- [ ] **Step 1: Read the current endpoint**

Read `service/Api/src/Module/Profile/Features/Store/Profiles/Get/PagedOrAll/GetProfilesPagedOrAll.Endpoint.cs`.

- [ ] **Step 2: Add admin-only authorization**

If the endpoint currently only has `.RequireAuthorization()`, change to:
```csharp
.RequireAuthorization()
.HasPermission(ProfileFeature.Admin.Profiles.Get.Permission)
```

If no `ProfileFeature` metadata exists, create a minimal one in the Profile module:
```csharp
// service/Api/src/Module/Profile/Features/Admin/ProfileFeature.cs
namespace Module.Profile.Features.Admin;

public static class ProfileFeature
{
    public static class Admin
    {
        public static class Profiles
        {
            public static class Get
            {
                public static readonly PermissionMetadata Permission = new("Profile", "Admin", "Profiles", "Get");
            }
        }
    }
}
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Profile/Module.Profile.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Profile/
git commit -m "feat(profile): restrict GetProfilesPagedOrAll to admin-only"
```

---

### Task 7: Build and Verify All Changes

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeds with zero warnings (TreatWarningsAsErrors)

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All tests pass

- [ ] **Step 3: Verify authorization with .http tests**

Open `ApiTests/Subscriptions.http` and test:
- `POST api/webhooks/subscriptions` without auth header → expect 401
- `POST api/webhooks/subscriptions` with valid admin token → expect 201

- [ ] **Step 4: Commit (if any fixes needed)**

```bash
git commit -m "fix: address build warnings from security hardening"
```
