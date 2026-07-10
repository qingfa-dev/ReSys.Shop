# Plan 6: Shared Infrastructure, Validation & Code Quality

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix shared infrastructure security headers, exception leak, validation completeness, and code quality nits.

**Architecture:** Fix ExceptionBehavior to not leak exception messages. Set CSP default. Add missing Id validation to 9 validators. Fix DateTime.UtcNow usage. Fix typos and HTTP semantics.

**Tech Stack:** .NET 10, FluentValidation, ASP.NET Core

## Global Constraints

- `TreatWarningsAsErrors=true` globally.
- All validators MUST use domain `Result.Failure` error codes/messages.
- Every command/query MUST have a corresponding validator class.

---

## File Structure

| Action | File | Responsibility |
|--------|------|---------------|
| Modify | `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs` | Remove exception message from response |
| Modify | `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.Constant.cs` | Set CSP default |
| Modify | `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.Validator.cs` | Validate more headers |
| Modify | `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs` | Fix dynamic cast |
| Modify | `service/Api/src/Shared/Operational/Webhooks/Backgrounds/WebhookDeliveryJob.cs` | Reduce log noise |
| Modify | `service/Api/src/Shared/Operational/Webhooks/Services/WebhookDispatcher.cs` | Remove double SaveChanges |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Revoke/RevokeUserRoles.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Sync/SyncUserRoles.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Users/Permissions/Assign/AssignUserPermissions.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Users/Permissions/Sync/SyncUserPermissions.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Roles/Permissions/Assign/AssignRolePermissions.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Roles/Permissions/Sync/SyncRolePermissions.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Roles/Permissions/Revoke/RevokeRolePermissions.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.Validator.cs` | Add Id.NotEmpty |
| Modify | `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.Validator.cs` | Add ShippingMethodId.NotEmpty |
| Modify | `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs` | Fix case-insensitive lookup + IsActive order |
| Modify | `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs` | Remove duplicate UserLogin |
| Modify | `service/Api/src/Module/Identity/Features/Store/Passwords/Change/ChangePassword.cs` | Replace DateTime.UtcNow |
| Modify | `service/Api/src/Module/Identity/Features/Store/Passwords/Reset/ResetPassword.cs` | Replace DateTime.UtcNow |
| Modify | `service/Api/src/Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.cs` | Replace DateTime.UtcNow |
| Rename | `service/Api/src/Shared/Application/Systems/SystemInfos/SystemInfo.Implementaion.cs` | Fix typo |
| Rename | `service/Api/src/Shared/Security/Identity/Domain/Roles/Role.EntityConfiugration.cs` | Fix typo |
| Modify | `service/Api/src/Api/Program.cs` | Fix comment typo |
| Modify | `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Revoke/RevokeUserRoles.Endpoint.cs` | Change MapDelete to MapPost |
| Modify | `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs` | Add 400 Produces |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.Endpoint.cs` | Remove unused 409 Produces |
| Modify | `service/Api/src/Module/Profile/Features/Store/Profiles/Delete/DeleteProfile.cs` | Fix error type |
| Modify | `service/Api/src/Module/Profile/Features/Store/Profiles/Delete/DeleteProfile.Endpoint.cs` | Fix Guid.Parse |
| Modify | `service/Api/src/Module/Profile/Features/Store/Profiles/Update/UpdateProfile.Endpoint.cs` | Fix Guid.Parse |
| Modify | `service/Api/src/Module/Profile/Features/Store/Profiles/Get/Detail/GetProfile.Endpoint.cs` | Fix Guid.Parse |
| Modify | `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.Validator.cs` | Add AcceptTerm validation |

---

### Task 1: Fix ExceptionBehavior — Remove Exception Message from Response

**Files:**
- Modify: `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current behavior**

Read `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs`.

- [ ] **Step 2: Remove exception message from response description**

Find the line that appends `$" Exception: {ex.Message}"` to the `description` string. Remove it. The exception details should only appear in the log, not in the API response.

Change:
```csharp
description += $" Exception: {ex.Message}";
```

To: Delete the line entirely.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Shared/Shared.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs
git commit -m "fix(shared): remove exception message from API error responses

Exception details were leaking to clients via the description field.
They should only appear in server logs."
```

---

### Task 2: Fix Security Headers — CSP Default + Validator

**Files:**
- Modify: `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.Constant.cs`
- Modify: `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.Validator.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current constants**

Read `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.Constant.cs`.

- [ ] **Step 2: Set restrictive CSP default**

Change `ContentSecurityPolicy = null` to:
```csharp
ContentSecurityPolicy = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'"
```

- [ ] **Step 3: Read the validator**

Read `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.Validator.cs`.

- [ ] **Step 4: Add validation for more headers**

Inside the `When(IsEnabled)` block, add:
```csharp
RuleFor(x => x.XFrameOptions).NotEmpty().WithErrorCode("SecurityHeaders.XFrameOptions.Required");
RuleFor(x => x.ReferrerPolicy).NotEmpty().WithErrorCode("SecurityHeaders.ReferrerPolicy.Required");
```

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Shared/Shared.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Shared/Security/Headers/Options/
git commit -m "fix(shared): set restrictive CSP default, validate more security headers"
```

---

### Task 3: Fix ValidationBehavior — Remove Dynamic Cast

**Files:**
- Modify: `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current behavior**

Read `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs`.

- [ ] **Step 2: Replace dynamic cast with Result.Failure**

Change:
```csharp
return (TResponse)(dynamic)validationFailures;
```

To:
```csharp
return (TResponse)(dynamic)Result.Failure(validationFailures);
```

This uses the explicit `Result.Failure(List<Error>)` factory and the implicit `Result -> TResponse` conversion, which is type-safe for the `Result` and `Result<T>` types used in this codebase.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Shared/Shared.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs
git commit -m "fix(shared): use Result.Failure factory instead of dynamic cast in ValidationBehavior"
```

---

### Task 4: Fix Webhook Logging — Reduce Noise + Remove Double Save

**Files:**
- Modify: `service/Api/src/Shared/Operational/Webhooks/Backgrounds/WebhookDeliveryJob.cs`
- Modify: `service/Api/src/Shared/Operational/Webhooks/Services/WebhookDispatcher.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read WebhookDeliveryJob**

Read `service/Api/src/Shared/Operational/Webhooks/Backgrounds/WebhookDeliveryJob.cs`.

- [ ] **Step 2: Reduce log level or conditional log**

Change `LogInformation` to `LogDebug`, or wrap in a condition:
```csharp
if (due.Count > 0)
    _logger.LogInformation("Webhook delivery job picked {Count} deliveries", due.Count);
```

- [ ] **Step 3: Read WebhookDispatcher**

Read `service/Api/src/Shared/Operational/Webhooks/Services/WebhookDispatcher.cs`.

- [ ] **Step 4: Remove SaveChangesAsync from DeliverAsync**

Find and remove the `await _dbContext.SaveChangesAsync(cancellationToken);` call in `DeliverAsync`. The caller (`WebhookDeliveryJob.RunAsync`) already calls `SaveChangesAsync` after the loop.

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Shared/Shared.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Shared/Operational/Webhooks/
git commit -m "fix(shared): reduce webhook log noise, remove double SaveChanges"
```

---

### Task 5: Add Id.NotEmpty to 9 Validators

**Files:**
- Modify: All 9 validator files listed in File Structure

**Interfaces:**
- N/A

- [ ] **Step 1: Add Id.NotEmpty to RevokeUserRoles.Validator**

Read `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Revoke/RevokeUserRoles.Validator.cs`.

Add inside the constructor:
```csharp
RuleFor(x => x.Id)
    .NotEmpty()
    .WithErrorCode(UserResult.Failure.IdRequired.Code)
    .WithMessage(UserResult.Failure.IdRequired.Message);
```

- [ ] **Step 2: Add Id.NotEmpty to SyncUserRoles.Validator**

Same pattern for `SyncUserRoles.Validator.cs`.

- [ ] **Step 3: Add Id.NotEmpty to AssignUserPermissions.Validator**

Same pattern for `AssignUserPermissions.Validator.cs`.

- [ ] **Step 4: Add Id.NotEmpty to SyncUserPermissions.Validator**

Same pattern for `SyncUserPermissions.Validator.cs`.

- [ ] **Step 5: Add Id.NotEmpty to AssignRolePermissions.Validator**

Same pattern for `AssignRolePermissions.Validator.cs`.

- [ ] **Step 6: Add Id.NotEmpty to SyncRolePermissions.Validator**

Same pattern for `SyncRolePermissions.Validator.cs`.

- [ ] **Step 7: Add Id.NotEmpty to RevokeRolePermissions.Validator**

Same pattern for `RevokeRolePermissions.Validator.cs`.

- [ ] **Step 8: Add Id.NotEmpty to UpdateOrderAdmin.Validator**

Read `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.Validator.cs`.

Add:
```csharp
RuleFor(x => x.Id)
    .NotEmpty()
    .WithErrorCode(OrderResult.Failure.IdRequired.Code)
    .WithMessage(OrderResult.Failure.IdRequired.Message);
```

- [ ] **Step 9: Add ShippingMethodId.NotEmpty to SelectShippingRate.Validator**

Read `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.Validator.cs`.

Add:
```csharp
RuleFor(x => x.Request.ShippingMethodId)
    .NotEmpty()
    .WithErrorCode("ShippingRate.Selection.MethodRequired")
    .WithMessage("Shipping method is required.");
```

- [ ] **Step 10: Verify build compiles**

Run: `dotnet build`
Expected: Build succeeds

- [ ] **Step 11: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Admin/Users/Roles/Revoke/RevokeUserRoles.Validator.cs
git add service/Api/src/Module/Identity/Features/Admin/Users/Roles/Sync/SyncUserRoles.Validator.cs
git add service/Api/src/Module/Identity/Features/Admin/Users/Permissions/Assign/AssignUserPermissions.Validator.cs
git add service/Api/src/Module/Identity/Features/Admin/Users/Permissions/Sync/SyncUserPermissions.Validator.cs
git add service/Api/src/Module/Identity/Features/Admin/Roles/Permissions/Assign/AssignRolePermissions.Validator.cs
git add service/Api/src/Module/Identity/Features/Admin/Roles/Permissions/Sync/SyncRolePermissions.Validator.cs
git add service/Api/src/Module/Identity/Features/Admin/Roles/Permissions/Revoke/RevokeRolePermissions.Validator.cs
git add service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.Validator.cs
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.Validator.cs
git commit -m "fix: add Id.NotEmpty validation to 9 validators missing it"
```

---

### Task 6: Fix Identity Auth Handlers

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read PasswordLogin handler**

Read `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs`.

- [ ] **Step 2: Move IsActive check before password verification**

Find where `user.IsActive` is checked. Move it before `signInManager.CheckPasswordSignInAsync`. This avoids wasted PBKDF2 hashing and lockout increments for inactive accounts.

- [ ] **Step 3: Fix case-insensitive credential lookup**

Read `FindUserByCredentialAsync`. Change the comparison to use `NormalizedEmail`/`NormalizedUserName` or use `userManager.FindByEmailAsync`/`FindByNameAsync`.

- [ ] **Step 4: Read ExternalAuthenticate handler**

Read `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs`.

- [ ] **Step 5: Remove duplicate UserLogin addition**

Delete lines 138-145 that do `user.UserLogins.Add(new UserLogin { ... })` — the login is already linked by `userManager.AddLoginAsync` above.

- [ ] **Step 6: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Identity/Module.Identity.csproj`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Login/
git commit -m "fix(identity): fix case-insensitive login, IsActive order, duplicate UserLogin"
```

---

### Task 7: Fix DateTime.UtcNow in Identity Handlers

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Change/ChangePassword.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Reset/ResetPassword.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.cs`

**Interfaces:**
- Consumes: `ISystemDateTime`

- [ ] **Step 1: Fix ChangePassword.cs**

Read the file. Find `DateTime.UtcNow` in log calls. Replace with `dateTime.UtcNow` (the injected `ISystemDateTime`).

- [ ] **Step 2: Fix ResetPassword.cs**

Read the file. If `ISystemDateTime` is not injected, add it to the constructor. Replace `DateTime.UtcNow` with `dateTime.UtcNow`.

- [ ] **Step 3: Fix RequestPasswordReset.cs**

Same pattern as Step 2.

- [ ] **Step 4: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Identity/Module.Identity.csproj`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Passwords/
git commit -m "fix(identity): replace DateTime.UtcNow with ISystemDateTime in log calls"
```

---

### Task 8: Fix File Typos

**Files:**
- Rename: `service/Api/src/Shared/Application/Systems/SystemInfos/SystemInfo.Implementaion.cs` → `SystemInfo.Implementation.cs`
- Rename: `service/Api/src/Shared/Security/Identity/Domain/Roles/Role.EntityConfiugration.cs` → `Role.EntityConfiguration.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Rename SystemInfo.Implementaion.cs**

```bash
git mv "service/Api/src/Shared/Application/Systems/SystemInfos/SystemInfo.Implementaion.cs" \
       "service/Api/src/Shared/Application/Systems/SystemInfos/SystemInfo.Implementation.cs"
```

- [ ] **Step 2: Rename Role.EntityConfiugration.cs**

```bash
git mv "service/Api/src/Shared/Security/Identity/Domain/Roles/Role.EntityConfiugration.cs" \
       "service/Api/src/Shared/Security/Identity/Domain/Roles/Role.EntityConfiguration.cs"
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "fix: rename misspelled files (Implementaion, EntityConfiugration)"
```

---

### Task 9: Fix Program.cs Comment Typo

**Files:**
- Modify: `service/Api/src/Api/Program.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Fix the comment**

Change `// Configure: Add moudular` to `// Configure: Add modular`.

- [ ] **Step 2: Commit**

```bash
git add service/Api/src/Api/Program.cs
git commit -m "fix: fix comment typo in Program.cs (moudular → modular)"
```

---

### Task 10: Fix RevokeUserRoles Endpoint HTTP Method

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Revoke/RevokeUserRoles.Endpoint.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current endpoint**

Read the file to find `MapDelete`.

- [ ] **Step 2: Change to MapPost**

Change `MapDelete` to `MapPost` and update the route to include `/revoke` sub-resource:
```csharp
app.MapPost($"{IdentityFeature.Admin.Users.Roles.Revoke.Route}/revoke", ...)
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Identity/Module.Identity.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Admin/Users/Roles/Revoke/RevokeUserRoles.Endpoint.cs
git commit -m "fix(identity): change RevokeUserRoles from MapDelete to MapPost

DELETE with request body is stripped by some proxies/LBs."
```

---

### Task 11: Fix Remaining Endpoint Produces Declarations

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.Endpoint.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Add 400 BadRequest to RemoveCartItem.Endpoint.cs**

Read the file. Add `.Produces<Result>(StatusCodes.Status400BadRequest)` to the endpoint chain.

- [ ] **Step 2: Remove unused 409 from DeleteProduct.Endpoint.cs**

Read the file. Remove `.Produces<Result>(StatusCodes.Status409Conflict)` if the handler never returns 409.

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs
git add service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.Endpoint.cs
git commit -m "fix: fix OpenAPI Produces declarations on endpoints"
```

---

### Task 12: Fix Profile Error Type and Guid.Parse

**Files:**
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Delete/DeleteProfile.cs`
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Delete/DeleteProfile.Endpoint.cs`
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Update/UpdateProfile.Endpoint.cs`
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Get/Detail/GetProfile.Endpoint.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Fix DeleteProfile.cs error type**

Change `UserResult.Failure.NotFound` to `UserProfileResult.Failure.NotFound`.

- [ ] **Step 2: Fix DeleteProfile.Endpoint.cs Guid.Parse**

Change `Guid.Parse(currentUser.UserId!)` to:
```csharp
if (!Guid.TryParse(currentUser.UserId, out var userId))
    return Results.Unauthorized();
```

- [ ] **Step 3: Fix UpdateProfile.Endpoint.cs Guid.Parse**

Same pattern as Step 2.

- [ ] **Step 4: Fix GetProfile.Endpoint.cs Guid.Parse**

Same pattern as Step 2.

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Profile/Module.Profile.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Profile/Features/Store/Profiles/
git commit -m "fix(profile): fix error type, replace Guid.Parse with TryParse"
```

---

### Task 13: Add AcceptTerm Validation to EmailRegister

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.Validator.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current validator**

Read the file.

- [ ] **Step 2: Add AcceptTerm rule**

Add inside the constructor:
```csharp
RuleFor(x => x.Request.AcceptTerm)
    .Equal(true)
    .WithErrorCode("Auth.Register.AcceptTerm.Required")
    .WithMessage("You must accept the terms and conditions.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Identity/Module.Identity.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.Validator.cs
git commit -m "fix(identity): enforce AcceptTerm=true in registration validator"
```

---

### Task 14: Build and Verify All Changes

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeds with zero warnings

- [ ] **Step 2: Run all unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All tests pass

- [ ] **Step 3: Run shared unit tests**

Run: `dotnet test service/Api/tests/Shared.UnitTests`
Expected: All tests pass

- [ ] **Step 4: Final grep verification**

Run: `grep -rn "DateTime.UtcNow" service/Api/src/Module/ --include="*.cs" | grep -v "//"`
Expected: No matches (all DateTime.UtcNow replaced with ISystemDateTime)

Run: `grep -rn "Exception: {ex.Message}" service/Api/src/ --include="*.cs"`
Expected: No matches (exception messages removed from responses)

- [ ] **Step 5: Commit (if any fixes needed)**

```bash
git commit -m "fix: final build fixes from infrastructure and quality hardening"
```
