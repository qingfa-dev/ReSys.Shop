# Identity + Profile + Shipping Convention Remediation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix all feature convention violations in the Identity, Profile, and Shipping modules — unbased requests, unbased responses, and manual handler construction.

**Architecture:** Create missing Shared/Models/ base types for Identity auth features (Emails, Passwords, Register). Fix Response records to inherit from existing bases. Replace `new Response { ... }` with mapping methods where domain entities exist; mark non-entity responses (auth tokens, import results) with explicit exceptions.

**Tech Stack:** .NET 10, C# 13, MediatR, Carter, FluentValidation

## Global Constraints

- Warnings-as-errors global; any warning fails the build
- Result objects, not exceptions; all domain operations return `Result<T>` or `Result`
- Vertical slice feature files; follow static partial class pattern
- Forward-only dependency: Shared depends on nothing, Module depends on Shared
- Module-internal Shared/Models/ bases are correct — they need not move to the Shared assembly

---

## File Map

| File | Purpose | Action |
|---|---|---|
| **Identity** | | |
| `Identity/Features/Store/Emails/Shared/Models/Email.Model.Parameters.cs` | Create base EmailParameters | Create |
| `Identity/Features/Store/Emails/Shared/Models/Email.Model.Request.cs` | Create EmailRequest | Create |
| `Identity/Features/Store/Emails/Shared/Models/Email.Model.Response.cs` | Create EmailDetailResponse | Create |
| `Identity/.../Resend/ResendEmailVerification.Request.cs` | Inherit from EmailRequest | Modify |
| `Identity/.../Confirm/ConfirmEmail.Request.cs` | Inherit from EmailRequest | Modify |
| `Identity/.../Change/ChangeEmail.Request.cs` | Inherit from EmailRequest | Modify |
| `Identity/Features/Store/Passwords/Shared/Models/Password.Model.Parameters.cs` | Create base PasswordParameters | Create |
| `Identity/Features/Store/Passwords/Shared/Models/Password.Model.Request.cs` | Create PasswordRequest | Create |
| `Identity/.../Change/ChangePassword.Request.cs` | Inherit from PasswordRequest | Modify |
| `Identity/.../Forgot/RequestPasswordReset.Request.cs` | Inherit from PasswordRequest | Modify |
| `Identity/.../Reset/ResetPassword.Request.cs` | Inherit from PasswordRequest | Modify |
| `Identity/Features/Store/Auth/Shared/Models/Register.Model.Request.cs` | Create RegisterRequest | Create |
| `Identity/.../Register/EmailRegister.Request.cs` | Inherit from RegisterRequest | Modify |
| `Identity/.../Register/EmailRegister.Response.cs` | Inherit from RegisterResponse | Modify |
| `Identity/Features/Store/Auth/Shared/Models/Auth.Response.Model.cs` | Add RegisterResponse, SessionResponse | Modify |
| `Identity/.../Sessions/Get/GetSession.Response.cs` | Inherit from SessionResponse | Modify |
| `Identity/.../Users/Roles/Get/GetUserRoles.Response.cs` | Inherit from base | Modify |
| `Identity/.../Login/Password/PasswordLogin.cs` | Add EXCEPTION comment for manual construction | Modify |
| `Identity/.../Register/EmailRegister.cs` | Add mapping or EXCEPTION comment | Modify |
| `Identity/.../Sessions/Refresh/RefreshSession.cs` | Add EXCEPTION comment for manual construction | Modify |
| `Identity/.../Login/External/Authenticate/ExternalAuthenticate.cs` | Add EXCEPTION comment | Modify |
| `Identity/.../Roles/Delete/DeleteRole.cs` | Replace manual Response with mapping | Modify |
| `Identity/.../Users/Permissions/Get/GetUserPermissions.cs` | Add mapping or EXCEPTION comment | Modify |
| **Profile** | | |
| `Profile/.../Addresses/Delete/DeleteAddress.Response.cs` | Inherit from base | Modify |
| `Profile/.../NotificationPreferences/Update/UpdateNotificationPreferences.Response.cs` | Inherit from base | Modify |
| `Profile/.../NotificationPreferences/Get/GetNotificationPreferences.Response.cs` | Inherit from base | Modify |
| `Profile/.../Addresses/Delete/DeleteAddress.cs` | Replace manual Response with mapping | Modify |
| `Profile/.../NotificationPreferences handlers` | Replace manual Response with mapping | Modify |
| **Shipping** | | |
| `Shipping/.../Methods/GetShippingMethods.Response.cs` | Inherit from base | Modify |
| `Shipping/.../Rates/ListShippingRates.Response.cs` | Inherit from base | Modify |
| `Shipping/.../Calculate/CalculateShipping.Response.cs` | Inherit from base | Modify |
| `Shipping/.../Methods/GetShippingMethods.cs` | Replace manual Response with mapping | Modify |
| `Shipping/.../Calculate/CalculateShipping.cs` | Replace manual Response with mapping | Modify |

---

### Task 1: Create Identity Email shared models

**Files:**
- Create dir: `service/Api/src/Module/Identity/Features/Store/Emails/Shared/Models/`
- Create: `service/Api/src/Module/Identity/Features/Store/Emails/Shared/Models/Email.Model.Parameters.cs`
- Create: `service/Api/src/Module/Identity/Features/Store/Emails/Shared/Models/Email.Model.Request.cs`
- Create: `service/Api/src/Module/Identity/Features/Store/Emails/Shared/Models/Email.Model.Response.cs`

**Interfaces:**
- Consumed by: ResendEmailVerification.Request, ConfirmEmail.Request, ChangeEmail.Request

- [ ] **Step 1: Create directory**

```bash
mkdir -p service/Api/src/Module/Identity/Features/Store/Emails/Shared/Models
```

- [ ] **Step 2: Create Email.Model.Parameters.cs**

```csharp
namespace Module.Identity.Features.Store.Emails.Shared.Models;

public abstract record EmailParameters
{
    public string Email { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Create Email.Model.Request.cs**

```csharp
namespace Module.Identity.Features.Store.Emails.Shared.Models;

public record EmailRequest : EmailParameters;
```

- [ ] **Step 4: Create Email.Model.Response.cs**

```csharp
namespace Module.Identity.Features.Store.Emails.Shared.Models;

public record EmailDetailResponse : EmailParameters
{
    public string? Message { get; init; }
}
```

- [ ] **Step 5: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Emails/Shared/
git commit -m "feat(Identity): create Email shared model hierarchy"

```

---

### Task 2: Fix Identity Email Request records — inherit from EmailRequest

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Resend/ResendEmailVerification.Request.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.Request.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Change/ChangeEmail.Request.cs`

- [ ] **Step 1: Fix ResendEmailVerification.Request.cs**

Add `using Module.Identity.Features.Store.Emails.Shared.Models;`. Change from:
```csharp
public record Request(string Email);
```
To:
```csharp
public record Request : EmailRequest;
```

- [ ] **Step 2: Fix ConfirmEmail.Request.cs** — same pattern (check current shape, inherit from `EmailRequest`).

- [ ] **Step 3: Fix ChangeEmail.Request.cs** — same pattern.

- [ ] **Step 4: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

Check for compilation errors (if the feature uses primary constructor fields from the old `record Request(string Email)`, they'll need to be accessed via `request.Email` from the base property instead).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Emails/
git commit -m "fix(Identity): inherit Email feature Requests from EmailRequest base"

```

---

### Task 3: Create Identity Password shared models + fix Request inheritance

**Files:**
- Create: `service/Api/src/Module/Identity/Features/Store/Passwords/Shared/Models/Password.Model.Parameters.cs`
- Create: `service/Api/src/Module/Identity/Features/Store/Passwords/Shared/Models/Password.Model.Request.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Change/ChangePassword.Request.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.Request.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Reset/ResetPassword.Request.cs`

- [ ] **Step 1: Create directory + base models**

```bash
mkdir -p service/Api/src/Module/Identity/Features/Store/Passwords/Shared/Models
```

Create `Password.Model.Parameters.cs`:
```csharp
namespace Module.Identity.Features.Store.Passwords.Shared.Models;

public abstract record PasswordParameters
{
    public string Email { get; init; } = string.Empty;
}
```

Create `Password.Model.Request.cs`:
```csharp
namespace Module.Identity.Features.Store.Passwords.Shared.Models;

public record PasswordRequest : PasswordParameters;
```

- [ ] **Step 2: Fix ChangePassword.Request.cs**

```csharp
using Module.Identity.Features.Store.Passwords.Shared.Models;

public static partial class ChangePassword
{
    public record Request : PasswordRequest
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
    }
}
```

- [ ] **Step 3: Fix RequestPasswordReset.Request.cs**

```csharp
using Module.Identity.Features.Store.Passwords.Shared.Models;

public static partial class RequestPasswordReset
{
    public record Request : PasswordRequest;
}
```

- [ ] **Step 4: Fix ResetPassword.Request.cs** — same pattern.

- [ ] **Step 5: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Identity/Features/Store/Passwords/
git commit -m "fix(Identity): create Password shared models, fix Request inheritance"

```

---

### Task 4: Create Identity Register base model + fix EmailRegister

**Files:**
- `Identity/Features/Store/Auth/Shared/Models/Register.Model.Request.cs` — Create
- `Identity/Features/Store/Auth/Shared/Models/Register.Model.Response.cs` — Create or extend existing
- Modify: `Identity/Features/Store/Auth/Register/EmailRegister.Request.cs`
- Modify: `Identity/Features/Store/Auth/Register/EmailRegister.Response.cs`

- [ ] **Step 1: Create Register.Model.Request.cs**

```csharp
namespace Module.Identity.Features.Store.Auth.Shared.Models;

public abstract record RegisterParameters
{
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public bool AcceptTerm { get; init; } = true;
}

public record RegisterRequest : RegisterParameters;
```

- [ ] **Step 2: Create Register.Model.Response.cs** or add to existing `Auth.Response.Model.cs`

Add to `Auth.Response.Model.cs`:
```csharp
public abstract record RegisterResponseModel
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Fix EmailRegister.Request.cs**

```csharp
using Module.Identity.Features.Store.Auth.Shared.Models;

public static partial class EmailRegister
{
    public record Request : RegisterRequest;
}
```

- [ ] **Step 4: Fix EmailRegister.Response.cs**

```csharp
using Module.Identity.Features.Store.Auth.Shared.Models;

public static partial class EmailRegister
{
    public sealed record Response : RegisterResponseModel;
}
```

- [ ] **Step 5: Update EmailRegister handler** — replace `new Response(user.Id, user.Email!, ...)` with mapping

Instead of constructing directly, create a mapping inside the handler class:
```csharp
private static Response MapToResponse(User user, string message)
{
    return new Response
    {
        UserId = user.Id,
        Email = user.Email ?? string.Empty,
        Message = message
    };
}
```

- [ ] **Step 6: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Identity/Features/Store/Auth/Register/
git add service/Api/src/Module/Identity/Features/Store/Auth/Shared/
git commit -m "fix(Identity): create Register base models, fix EmailRegister Request/Response inheritance"

```

---

### Task 5: Fix Identity remaining unbased Response records

**Files:**
- Modify: `Identity/Features/Store/Auth/Sessions/Get/GetSession.Response.cs`
- Modify: `Identity/Features/Admin/Users/Roles/Get/GetUserRoles.Response.cs`

- [ ] **Step 1: Create SessionResponseModel base**

Add to `Auth.Response.Model.cs`:
```csharp
public abstract record SessionResponseModel
{
    public string Id { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
```

- [ ] **Step 2: Fix GetSession.Response.cs**

```csharp
using Module.Identity.Features.Store.Auth.Shared.Models;

public static partial class GetSession
{
    public sealed record Response : SessionResponseModel;
}
```

- [ ] **Step 3: Fix GetUserRoles.Response.cs**

Check existing `RoleListItemResponse` in `Identity/Features/Admin/Roles/Shared/Models/Role.Model.Response.cs`. If it matches, inherit from it:
```csharp
using Module.Identity.Features.Admin.Roles.Shared.Models;

public static partial class GetUserRoles
{
    public sealed record Response : RoleListItemResponse;
}
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Identity/
git commit -m "fix(Identity): fix unbased Response records in GetSession and GetUserRoles"

```

---

### Task 6: Identity auth handlers — mark exceptions or add mapping

**Files (modify each to add explicit EXCEPTION comment):**

- `Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs` — handler returns `new Response() { ... }` with token data from services. No domain entity to map from. **Legitimate exception.**

- `Identity/Features/Store/Auth/Sessions/Refresh/RefreshSession.cs` — similar token construction.

- `Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs` — similar.

- `Identity/Features/Store/Auth/Register/EmailRegister.cs` — fixed in Task 4.

- `Identity/Features/Admin/Users/Permissions/Get/GetUserPermissions.cs` — returns composite permission response. Either create mapping or mark exception.

- `Identity/Features/Admin/Roles/Delete/DeleteRole.cs` — replace with mapping.

- [ ] **Step 1: Add EXCEPTION comments to token-based handlers**

For `PasswordLogin.cs`, add before the manual Response construction:
```csharp
// EXCEPTION: no domain entity — response constructed from token service results
return new Response()
{
    AccessToken = tokenResult.Value.Token,
    // ...
};
```

Same for `RefreshSession.cs` and `ExternalAuthenticate.cs`.

- [ ] **Step 2: Fix DeleteRole handler** — replace `new Response { Id = ..., Name = ... }`

Check if `RoleDetailResponse` exists. If so:
```csharp
return role.MapToDetail<Response>();
```
If the feature needs a mapping method, add one or inline:
```csharp
return new Response { Id = role.Id, Name = role.Name ?? string.Empty };
```
With exception comment:
```csharp
// EXCEPTION: role is not a full domain entity for mapping
```

- [ ] **Step 3: Fix GetUserPermissions handler** — same pattern.

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Identity/
git commit -m "fix(Identity): mark auth handler exceptions, add mapping for role handlers"

```

---

### Task 7: Fix Profile unbased Response records

**Files:**
- Modify: `Profile/Features/Store/Addresses/Delete/DeleteAddress.Response.cs`
- Modify: `Profile/Features/Store/NotificationPreferences/Update/UpdateNotificationPreferences.Response.cs`
- Modify: `Profile/Features/Store/NotificationPreferences/Get/GetNotificationPreferences.Response.cs`

- [ ] **Step 1: Fix DeleteAddress.Response.cs**

Check existing `AddressDetailResponse` in `Profile/.../Addresses/Shared/Models/Address.Model.Response.cs`. If it exists and has compatible fields, inherit:
```csharp
using Module.Profile.Features.Store.Addresses.Shared.Models;

public static partial class DeleteAddress
{
    public sealed record Response : AddressDetailResponse;
}
```
If the property sets don't match, add minimal fields to `AddressDetailResponse` or use a separate base.

- [ ] **Step 2: Fix NotificationPreferences.Response files**

Check `ProfileNotificationPreferences` in `Profile/.../Profiles/Shared/Models/Profile.Model.NotificationPreferences.cs`. If it's already a suitable base:
```csharp
using Module.Profile.Features.Store.Profiles.Shared.Models;

public static partial class GetNotificationPreferences
{
    public sealed record Response : ProfileNotificationPreferences;
}
```
Do the same for `UpdateNotificationPreferences`.

- [ ] **Step 3: Update handlers** — replace `new Response(...)` with mapping

For `DeleteAddress`, use `address.MapToDetail<Response>()`.
For NotificationPreferences handlers, create a mapping or use implicit conversion:
```csharp
return new Response { EnableSms = prefs.EnableSms, EnableEmail = prefs.EnableEmail, EnableNewsfeeds = prefs.EnableNewsfeeds };
```
With exception comment if no domain entity exists.

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Profile/
git commit -m "fix(Profile): add base type inheritance to Response records, fix handlers"

```

---

### Task 8: Fix Shipping unbased Response records

**Files:**
- Modify: `Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Response.cs`
- Modify: `Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.Response.cs`
- Modify: `Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.Response.cs`

- [ ] **Step 1: Create shipping storefront Response bases if needed**

Check `Shipping/Features/Storefront/Shared/Models/ShippingMethod.Model.Response.cs`:
```bash
cat service/Api/src/Module/Shipping/Features/Storefront/Shared/Models/ShippingMethod.Model.Response.cs
```

If it has `ShippingMethodDetailResponse` with the right fields, use it:
```csharp
public sealed record Response : ShippingMethodDetailResponse;
```

For `ListShippingRates.Response.cs` and `CalculateShipping.Response.cs`, check if the existing `ShippingRateDetailResponse` can serve as the base. If the feature-specific Response has fields not in the base, add them to the base or create a feature-specific subclass.

- [ ] **Step 2: Fix GetShippingMethods handler**

Replace `new Response(methods.Select(...))` with mapping:
```csharp
return new Response { Items = methods.Select(m => new ShippingMethodDto { ... }).ToList() };
```
Or if a mapping method exists: `return methods.MapToList<Response>()`.

- [ ] **Step 3: Fix CalculateShipping handler**

Replace `new Response(method.Id, ...)` with mapping:
```csharp
return new Response { ShippingMethodId = method.Id, ... };
```
With exception comment if no domain entity exists.

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Shipping/
git commit -m "fix(Shipping): add base type inheritance to Response records, fix handlers"

```

---

### Task 9: Full build verification and final check

- [ ] **Step 1: Full solution build**

```bash
dotnet build
```

Expected: Build passes with zero warnings.

- [ ] **Step 2: Run convention checks**

```bash
bash scripts/check-feature-conventions.sh
```

Expected: All checks PASS (AC-001 through AC-005, AC-006 green for all modules).

- [ ] **Step 3: Run unit tests for affected modules**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "Identity|Profile|Shipping"
```

Expected: Tests pass.

- [ ] **Step 4: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Shared.UnitTests
```

Expected: All tests pass.
