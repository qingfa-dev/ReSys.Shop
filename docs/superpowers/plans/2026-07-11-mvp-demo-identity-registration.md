# MVP Demo Identity Registration Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the email verification and authentication flows so users can register, confirm email, and log in securely for the MVP demo.

**Architecture:** Correct the Base64 encoding mismatch between token generation and verification, replace in-memory user enumeration with store queries, and align username lookup with Identity's normalized username index.

**Tech Stack:** .NET 10, ASP.NET Core Identity, Carter minimal APIs, MediatR, FluentValidation, xUnit, FluentAssertions

## Global Constraints

- All domain operations return `Result<T>` or `Result`; exceptions only for unrecoverable infrastructure failures.
- Modules never reference each other; communication via MediatR `ISender` only.
- Every C# feature action is a `static partial class` split across files.
- `TreatWarningsAsErrors=true` globally.

---

### Task 1: Fix Base64 Encoding Mismatch in Email Verification

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.cs`
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs` (already uses URL-safe; verify)
- Test: `service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Confirm/ConfirmEmailTests.cs` (create or extend)

**Interfaces:**
- Consumes: `Base64Converter.ToBase64Url` and `Base64Converter.TryFromBase64Url`
- Produces: email verification tokens that round-trip correctly

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Identity.Features.Store.Auth.Register;
using Shared.Governance.Conventions;

namespace Module.UnitTests.Identity.Features.Store.Emails.Confirm;

public class VerificationTokenEncodingTests
{
    [Fact]
    public void EmailRegister_BuildVerificationPath_Produces_Token_Decodable_By_ConfirmEmail()
    {
        var userId = Guid.NewGuid();
        var token = "CfDJ8OKSJw..."; // any realistic Identity token shape

        var path = EmailRegister.CommandHandler.BuildVerificationPath(userId, token);
        var encodedToken = path.Split("token=")[1];

        Base64Converter.TryFromBase64Url(encodedToken, out var decoded).Should().BeTrue();
        decoded.Should().Be(token);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~VerificationTokenEncodingTests"`

Expected: FAIL — `TryFromBase64Url` cannot decode standard Base64

- [ ] **Step 3: Change token encoding to URL-safe Base64**

In `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.cs`:

```csharp
internal static string BuildVerificationPath(Guid userId, string token)
{
    var encodedToken = token.ToBase64Url();
    const string path = "verify-email";
    return $"{path}?userId={userId}&token={encodedToken}";
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~VerificationTokenEncodingTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.cs
git add service/Api/tests/Module.UnitTests/Identity/Features/Store/Emails/Confirm/VerificationTokenEncodingTests.cs
git commit -m "fix(identity): use URL-safe base64 for email verification tokens"
```

---

### Task 2: Fix Phone Number Lookup in Password Login

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs`
- Test: `service/Api/tests/Module.UnitTests/Identity/Features/Store/Auth/Login/Password/PasswordLoginTests.cs` (create or extend)

**Interfaces:**
- Consumes: `UserManager<User>` query APIs
- Produces: efficient phone-number-based user lookup

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Identity.Features.Store.Auth.Login.Password;
using Shared.Security.Identity.Domain.Users;
using Shared.Testing;

namespace Module.UnitTests.Identity.Features.Store.Auth.Login.Password;

public class PasswordLoginPhoneLookupTests : TestBase
{
    [Fact]
    public async Task FindUserByCredentialAsync_Should_Find_User_By_PhoneNumber()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PhoneNumber = "+15551234567",
            EmailConfirmed = true
        };
        await UserManager.CreateAsync(user, "Password123!");

        var handler = GetService<PasswordLogin.CommandHandler>();
        var found = await handler.FindUserByCredentialAsync("+15551234567");

        found.Should().NotBeNull();
        found!.PhoneNumber.Should().Be("+15551234567");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~PasswordLoginPhoneLookupTests"`

Expected: FAIL — current implementation enumerates `Users` instead of querying

- [ ] **Step 3: Replace enumeration with indexed query**

In `service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs`:

```csharp
internal async Task<User?> FindUserByCredentialAsync(string credential)
{
    var user = await userManager.FindByEmailAsync(credential);
    if (user is not null)
        return user;

    user = await userManager.FindByNameAsync(credential);
    if (user is not null)
        return user;

    return await userManager.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.PhoneNumber == credential);
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~PasswordLoginPhoneLookupTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Login/Password/PasswordLogin.cs
git add service/Api/tests/Module.UnitTests/Identity/Features/Store/Auth/Login/Password/PasswordLoginPhoneLookupTests.cs
git commit -m "fix(identity): replace phone lookup enumeration with query"
```

---

### Task 3: Fix Username Lookup Normalization

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.cs`
- Test: `service/Api/tests/Module.UnitTests/Identity/Features/Store/Auth/Register/EmailRegisterUsernameTests.cs` (create)

**Interfaces:**
- Consumes: `UserManager.FindByNameAsync`
- Produces: case-insensitive username duplicate check using Identity normalization

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Module.Identity.Features.Store.Auth.Register;
using Shared.Security.Identity.Domain.Users;
using Shared.Testing;

namespace Module.UnitTests.Identity.Features.Store.Auth.Register;

public class EmailRegisterUsernameTests : TestBase
{
    [Fact]
    public async Task Handle_Should_Reject_Duplicate_UserName_Different_Casing()
    {
        await UserManager.CreateAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            UserName = "ExistingUser",
            EmailConfirmed = true
        }, "Password123!");

        var handler = GetService<EmailRegister.CommandHandler>();
        var result = await handler.Handle(new EmailRegister.Command(
            new EmailRegister.Request
            {
                Email = "new@example.com",
                UserName = "existinguser",
                Password = "Password123!",
                FirstName = "New"
            }), default);

        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~EmailRegisterUsernameTests"`

Expected: FAIL — lowercase comparison may not match normalized storage

- [ ] **Step 3: Use `FindByNameAsync` without manual lowercasing**

In `service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.cs`:

```csharp
var trimmedUsername = request.UserName.Trim();
var existingByUsername = await userManager.FindByNameAsync(trimmedUsername);
if (existingByUsername is not null)
    return UserResult.Failure.UsernameDuplicate;
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~EmailRegisterUsernameTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Register/EmailRegister.cs
git add service/Api/tests/Module.UnitTests/Identity/Features/Store/Auth/Register/EmailRegisterUsernameTests.cs
git commit -m "fix(identity): use Identity normalized username lookup in registration"
```

---

### Task 4: Final Verification

- [ ] **Step 1: Run Module unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Identity"`

Expected: All identity tests pass

- [ ] **Step 2: Run build**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 3: Commit**

```bash
git commit -m "chore(identity): final verification for registration fixes" --allow-empty
```
