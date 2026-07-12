# Security Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove hardcoded dev secrets from `appsettings.Development.json`, refuse to start in production with dev secret literals, fix the silent profile-creation failure in `ExternalAuthenticate`, and bind `SecurityHeadersSetting` from configuration.

**Architecture:** Two independent subsystems. (1) Configuration: replace dev secret values with a sentinel, add a `JwtSettings` validator that rejects the sentinel in non-development environments, and add a validator to `SecurityHeadersSetting`. (2) External auth: restructure `ExternalAuthenticate.CreateUserProfileAsync` to surface failures.

**Tech Stack:** .NET 10, xUnit v3, Moq, EF Core InMemory, FluentValidation (existing in repo).

## Global Constraints

- `TreatWarningsAsErrors=true`
- Test pattern: `[Fact(DisplayName = "...")]`, `[Trait("Category", "Unit")]`
- Configuration secrets in `appsettings.json` (production template) MUST be empty strings; dev values live in user-secrets or Aspire parameters
- Validators use FluentValidation, registered via `ValidateFluentValidation()` extension
- `BindConfiguration(SectionName)` MUST precede `ValidateFluentValidation()`

## File Structure

### Files to modify

| File | Change |
|------|--------|
| `service/Api/src/Api/appsettings.Development.json` | Remove hardcoded `Jwt:Secret` and `SettingsEncryptionKey`; add sentinel comments |
| `service/Api/src/Shared/Security/Authentication/Tokens/Options/JwtSettings.cs` (or wherever) | Add `IsProductionSafe` validator rule |
| `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.cs` | Add validator class |
| `service/Api/src/Shared/Security/Headers/SecurityHeaders.Extensions.cs` | Bind config + register validator |
| `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs` | Restructure `CreateUserProfileAsync` to surface failures |

### Files to create

| File | Purpose |
|------|---------|
| `service/Api/src/Shared/Security/Authentication/Tokens/Options/JwtSettingsValidator.cs` | Production secret rejection |
| `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSettingValidator.cs` | Headers config validation |
| `service/Api/tests/Module.UnitTests/Identity/JwtSettingsValidatorTests.cs` | Validator tests |
| `service/Api/tests/Module.UnitTests/Identity/ExternalAuthenticateProfileCreationTests.cs` | Profile-creation failure tests |
| `service/Api/tests/Shared.UnitTests/Security/SecurityHeadersSettingValidatorTests.cs` | Headers validator tests |

---

## Task 1: Add `JwtSettings` validator that rejects dev secret in production

**Files:**
- Create: `service/Api/src/Shared/Security/Authentication/Tokens/Options/JwtSettingsValidator.cs`
- Modify: `service/Api/src/Shared/Security/Authentication/Tokens/Tokens.Extensions.cs` (register validator)
- Test: `service/Api/tests/Module.UnitTests/Identity/JwtSettingsValidatorTests.cs`

**Context:** The dev JWT secret is `dev-jwt-secret-min-32-chars-for-hs256-algorithm!`. The validator MUST reject this literal in any environment other than `Development`.

- [ ] **Step 1: Locate the `JwtSettings` class and its current options registration**

Read the existing `JwtSettings` class and find `Tokens.Extensions.cs` (or wherever `AddOptions<JwtSettings>()` is called). Confirm the section name constant.

- [ ] **Step 2: Write the failing test**

Create file `service/Api/tests/Module.UnitTests/Identity/JwtSettingsValidatorTests.cs`:

```csharp
using FluentValidation.TestHelper;
using Shared.Security.Authentication.Tokens.Options;

namespace Module.UnitTests.Identity;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class JwtSettingsValidatorTests
{
    private const string DevSecret = "dev-jwt-secret-min-32-chars-for-hs256-algorithm!";
    private readonly JwtSettingsValidator _validator = new();

    [Fact(DisplayName = "Validator: rejects dev secret literal in Production")]
    public void Production_DevSecret_Fails()
    {
        var settings = ValidSettings() with { Secret = DevSecret };
        var result = _validator.TestValidate(settings);
        result.ShouldHaveValidationErrorFor(s => s.Secret);
    }

    [Fact(DisplayName = "Validator: accepts dev secret literal in Development")]
    public void Development_DevSecret_Allowed()
    {
        var settings = ValidSettings() with { Secret = DevSecret };
        var result = _validator.TestValidate(settings, "Development");
        result.ShouldNotHaveValidationErrorFor(s => s.Secret);
    }

    [Fact(DisplayName = "Validator: rejects empty secret")]
    public void Empty_Fails()
    {
        var settings = ValidSettings() with { Secret = "" };
        var result = _validator.TestValidate(settings);
        result.ShouldHaveValidationErrorFor(s => s.Secret);
    }

    private static JwtSettings ValidSettings() => new()
    {
        Secret = "real-32-character-or-longer-secret-here",
        Issuer = "ReSys.Shop",
        Audience = "ReSys.Shop",
        AccessTokenExpirationInMinutes = 15,
        RefreshTokenExpirationInDays = 7,
        Algorithm = "HS256"
    };
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~JwtSettingsValidatorTests" --no-restore`
Expected: FAIL — `JwtSettingsValidator` does not exist.

- [ ] **Step 4: Create the validator**

Create file `service/Api/src/Shared/Security/Authentication/Tokens/Options/JwtSettingsValidator.cs`:

```csharp
using FluentValidation;

namespace Shared.Security.Authentication.Tokens.Options;

public sealed class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
    public const string DevSecretLiteral = "dev-jwt-secret-min-32-chars-for-hs256-algorithm!";

    public JwtSettingsValidator()
    {
        RuleFor(s => s.Secret)
            .NotEmpty()
            .MinimumLength(32)
            .Must((settings, secret, ctx) =>
            {
                if (secret == DevSecretLiteral &&
                    !string.Equals(ctx.RootContextData["Environment"] as string, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            })
            .WithMessage("Jwt:Secret must not be the dev secret literal in non-Development environments.");
    }
}
```

- [ ] **Step 5: Register the validator with environment propagation**

In `Tokens.Extensions.cs` (or wherever `AddOptions<JwtSettings>()` is registered), add:

```csharp
builder.Services.AddSingleton<IValidator<JwtSettings>, JwtSettingsValidator>();
builder.Services.AddOptions<JwtSettings>()
    .BindConfiguration(JwtSettings.SectionName)
    .ValidateFluentValidation()
    .ValidateOnStart();

// After the host is built, propagate the environment to the validator:
var jwtDescriptor = builder.Services.Single(s => s.ServiceType == typeof(IValidator<JwtSettings>));
// (No code change needed if validator uses RootContextData — see Task 1 Step 6.)
```

If the environment is propagated via `IHostEnvironment` injection in the validator, change the validator constructor to:

```csharp
public sealed class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
    private readonly IHostEnvironment _env;
    public JwtSettingsValidator(IHostEnvironment env) { _env = env; }

    public JwtSettingsValidator() : this(new EmptyHostEnvironment()) { }

    // ...
    .Must((settings, secret, ctx) =>
    {
        if (secret == DevSecretLiteral && !_env.IsDevelopment()) return false;
        return true;
    })
```

Register a single instance bound to the host's `IHostEnvironment`.

- [ ] **Step 6: Re-run the test**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~JwtSettingsValidatorTests" --no-restore`
Expected: PASS.

- [ ] **Step 7: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 8: Commit**

```bash
git add service/Api/src/Shared/Security/Authentication/Tokens/Options/JwtSettingsValidator.cs service/Api/src/Shared/Security/Authentication/Tokens/Tokens.Extensions.cs service/Api/tests/Module.UnitTests/Identity/JwtSettingsValidatorTests.cs
git commit -m "feat(security): reject dev JWT secret in non-Development environments"
```

---

## Task 2: Remove hardcoded dev secrets from `appsettings.Development.json`

**Files:**
- Modify: `service/Api/src/Api/appsettings.Development.json`
- Modify: `docs/codebase/CONCERNS.md` (or `.harness/`) — document the move to user-secrets

**Context:** The dev secret literal is convenient for local dev. The repo map already lists this as a security risk (AGENTS.md known issues). Move it to user-secrets with a setup script.

- [ ] **Step 1: Update `appsettings.Development.json`**

Replace lines 6 and 30-32:

```json
"SettingsEncryptionKey": "dev-encryption-key-32-chars-len!",
```

becomes:

```json
"SettingsEncryptionKey": "",
```

and:

```json
"Jwt": {
  "Secret": "dev-jwt-secret-min-32-chars-for-hs256-algorithm!"
}
```

becomes:

```json
"Jwt": {
  "Secret": ""
}
```

Add a top-of-file comment (JSON allows `_comment` keys by convention; this repo uses them — verify the existing pattern in the file or any other `appsettings.*.json`):

```json
"_dev_secrets_setup_comment": "JWT secret and SettingsEncryptionKey are loaded from dotnet user-secrets (id: resys.shop.api). Run: dotnet user-secrets set \"Authentication:Jwt:Secret\" \"<32+ char value>\"",
```

- [ ] **Step 2: Add a developer setup script**

Create file `service/Api/scripts/setup-dev-secrets.sh`:

```bash
#!/usr/bin/env bash
set -euo pipefail

PROJECT="service/Api/src/Api/Api.csproj"

if ! grep -q "UserSecretsId" "$PROJECT"; then
  echo "Api.csproj must declare <UserSecretsId>resys.shop.api</UserSecretsId> in the first <PropertyGroup>." >&2
  exit 1
fi

JWT_SECRET="${JWT_SECRET:-$(openssl rand -base64 48 | tr -d '\n' | head -c 64)}"
ENC_KEY="${SETTINGS_ENCRYPTION_KEY:-$(openssl rand -base64 32 | tr -d '\n' | head -c 44)}"

dotnet user-secrets set "Authentication:Jwt:Secret" "$JWT_SECRET" --project "$PROJECT"
dotnet user-secrets set "GatewayProviders:SettingsEncryptionKey" "$ENC_KEY" --project "$PROJECT"

echo "Dev secrets set. Verify with: dotnet user-secrets list --project $PROJECT"
```

Make it executable: `chmod +x service/Api/scripts/setup-dev-secrets.sh`

- [ ] **Step 3: Add `UserSecretsId` to `Api.csproj`**

In `service/Api/src/Api/Api.csproj`, add inside the first `<PropertyGroup>`:

```xml
<UserSecretsId>resys.shop.api</UserSecretsId>
```

- [ ] **Step 4: Document the setup in `docs/codebase/CONCERNS.md`**

Open `docs/codebase/CONCERNS.md` and replace the existing line about the dev JWT secret with:

```md
### Dev secrets
- `Authentication:Jwt:Secret` and `GatewayProviders:SettingsEncryptionKey` are stored in dotnet user-secrets (id `resys.shop.api`).
- Setup: `./service/Api/scripts/setup-dev-secrets.sh` (or set `JWT_SECRET` and `SETTINGS_ENCRYPTION_KEY` env vars first to override the auto-generated values).
- Production deploys MUST set these via environment variables (`Authentication__Jwt__Secret`, `GatewayProviders__SettingsEncryptionKey`) — the host refuses to start in `Production` if the dev literal is detected.
```

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Api/appsettings.Development.json service/Api/src/Api/Api.csproj service/Api/scripts/setup-dev-secrets.sh docs/codebase/CONCERNS.md
git commit -m "chore(security): move dev secrets to user-secrets, document setup"
```

---

## Task 3: Bind `SecurityHeadersSetting` from configuration

**Files:**
- Create: `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSettingValidator.cs`
- Modify: `service/Api/src/Shared/Security/Headers/SecurityHeaders.Extensions.cs`
- Test: `service/Api/tests/Shared.UnitTests/Security/SecurityHeadersSettingValidatorTests.cs`

**Context:** Today `AddOptions<SecurityHeadersSetting>()` does not call `BindConfiguration`. The middleware reads defaults only.

- [ ] **Step 1: Read the current `SecurityHeadersSetting` and its options registration**

Read `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSetting.cs` and `SecurityHeaders.Extensions.cs`.

- [ ] **Step 2: Write the failing test**

Create file `service/Api/tests/Shared.UnitTests/Security/SecurityHeadersSettingValidatorTests.cs`:

```csharp
using FluentValidation.TestHelper;
using Shared.Security.Headers.Options;

namespace Shared.UnitTests.Security;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class SecurityHeadersSettingValidatorTests
{
    private readonly SecurityHeadersSettingValidator _validator = new();

    [Fact(DisplayName = "Validator: passes when all values are non-empty")]
    public void Valid_Passes()
    {
        var settings = new SecurityHeadersSetting
        {
            ContentSecurityPolicy = "default-src 'self'",
            StrictTransportSecurity = "max-age=31536000",
            XFrameOptions = "DENY"
        };
        var result = _validator.TestValidate(settings);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Validator: warns on empty CSP")]
    public void EmptyCsp_Fails()
    {
        var settings = new SecurityHeadersSetting
        {
            ContentSecurityPolicy = "",
            StrictTransportSecurity = "max-age=31536000",
            XFrameOptions = "DENY"
        };
        var result = _validator.TestValidate(settings);
        result.ShouldHaveValidationErrorFor(s => s.ContentSecurityPolicy);
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --filter "FullyQualifiedName~SecurityHeadersSettingValidatorTests" --no-restore`
Expected: FAIL — `SecurityHeadersSettingValidator` does not exist.

- [ ] **Step 4: Create the validator**

Create file `service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSettingValidator.cs`:

```csharp
using FluentValidation;

namespace Shared.Security.Headers.Options;

public sealed class SecurityHeadersSettingValidator : AbstractValidator<SecurityHeadersSetting>
{
    public SecurityHeadersSettingValidator()
    {
        RuleFor(s => s.ContentSecurityPolicy)
            .NotEmpty()
            .WithMessage("SecurityHeaders:ContentSecurityPolicy is required.");
        RuleFor(s => s.StrictTransportSecurity)
            .NotEmpty()
            .WithMessage("SecurityHeaders:StrictTransportSecurity is required.");
        RuleFor(s => s.XFrameOptions)
            .NotEmpty()
            .WithMessage("SecurityHeaders:XFrameOptions is required.");
    }
}
```

- [ ] **Step 5: Wire `BindConfiguration` + validator + `ValidateOnStart`**

In `service/Api/src/Shared/Security/Headers/SecurityHeaders.Extensions.cs`, replace the `AddSecurityHeaders` body:

```csharp
public static WebApplicationBuilder AddSecurityHeaders(this WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<IValidator<SecurityHeadersSetting>, SecurityHeadersSettingValidator>();
    builder.Services.AddOptions<SecurityHeadersSetting>()
        .BindConfiguration(SecurityHeadersSetting.SectionName)
        .ValidateFluentValidation()
        .ValidateOnStart();
    return builder;
}
```

Add the imports:

```csharp
using FluentValidation;
using Shared.Application.Extensions.Validations;
```

- [ ] **Step 6: Re-run the test**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --filter "FullyQualifiedName~SecurityHeadersSettingValidatorTests" --no-restore`
Expected: PASS.

- [ ] **Step 7: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 8: Commit**

```bash
git add service/Api/src/Shared/Security/Headers/Options/SecurityHeadersSettingValidator.cs service/Api/src/Shared/Security/Headers/SecurityHeaders.Extensions.cs service/Api/tests/Shared.UnitTests/Security/SecurityHeadersSettingValidatorTests.cs
git commit -m "fix(security): bind SecurityHeadersSetting from configuration with fail-fast"
```

---

## Task 4: Fix `ExternalAuthenticate` silent profile-creation failure

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs`
- Test: `service/Api/tests/Module.UnitTests/Identity/ExternalAuthenticateProfileCreationTests.cs`

**Context:** Today `CreateUserProfileAsync` swallows exceptions and logs a warning. The user gets a JWT, but no profile exists.

- [ ] **Step 1: Read the current `CreateUserProfileAsync` method**

Read lines 155-180 of `ExternalAuthenticate.cs`.

- [ ] **Step 2: Write the failing test**

Create file `service/Api/tests/Module.UnitTests/Identity/ExternalAuthenticateProfileCreationTests.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Module.Identity.Features.Store.Auth.Login.External.Authenticate;
using Moq;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class ExternalAuthenticateProfileCreationTests
{
    [Fact(DisplayName = "ExternalAuthenticate: profile creation failure returns Result.Failure")]
    public async Task Handle_ProfileCreationThrows_ReturnsFailure()
    {
        var provider = new Mock<IExternalLoginProvider>();
        provider.Setup(x => x.Provider).Returns("google");
        provider.Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalUserInfo>.Success(new ExternalUserInfo
            {
                Provider = "google", ProviderSubjectId = "sub-1",
                Email = "new@user.com", FirstName = "New", LastName = "User"
            }));

        var userStore = new Mock<IUserStore<User>>();
        var userManager = MockUserManager(userStore.Object);
        var accessTokenService = new Mock<Shared.Security.Authentication.Tokens.Services.Access.IAccessTokenService>();
        accessTokenService.Setup(x => x.GenerateToken(It.IsAny<Shared.Security.Authentication.Tokens.Models.TokenRequestModel>()))
            .Returns(Result<Shared.Security.Authentication.Tokens.Models.AccessTokenResult>.Success(
                new Shared.Security.Authentication.Tokens.Models.AccessTokenResult("tok", DateTimeOffset.UtcNow.AddMinutes(15))));
        var refreshTokenService = new Mock<Shared.Security.Authentication.Tokens.Services.Refresh.IRefreshTokenService>();
        refreshTokenService.Setup(x => x.GenerateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Shared.Security.Authentication.Tokens.Models.RefreshTokenResult>.Success(
                new Shared.Security.Authentication.Tokens.Models.RefreshTokenResult("rt", DateTimeOffset.UtcNow.AddDays(7))));

        var dateTime = new Mock<Shared.Kernel.Time.ISystemDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);

        var currentUser = new Mock<Shared.Security.Identity.ICurrentUser>();
        currentUser.Setup(x => x.IpAddress).Returns("127.0.0.1");

        var mediator = new Mock<MediatR.ISender>();
        mediator.Setup(x => x.Send(It.IsAny<Module.Profile.Features.Store.Profiles.Create.CreateProfile.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("profile creation failed"));

        var handler = new ExternalAuthenticate.CommandHandler(
            new[] { provider.Object },
            userManager.Object,
            accessTokenService.Object,
            refreshTokenService.Object,
            dateTime.Object,
            currentUser.Object,
            new Mock<ILogger<ExternalAuthenticate.CommandHandler>>().Object,
            mediator.Object);

        var result = await handler.Handle(
            new ExternalAuthenticate.Command(new ExternalAuthenticate.Request { Provider = "google", IdToken = "tok" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Identity.ExternalLogin.ProfileCreationFailed");
    }

    private static Mock<UserManager<User>> MockUserManager(IUserStore<User> store)
    {
        var m = new Mock<UserManager<User>>(store, null, null, null, null, null, null, null, null);
        m.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        m.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        m.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>())).ReturnsAsync(IdentityResult.Success);
        m.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        return m;
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ExternalAuthenticateProfileCreationTests" --no-restore`
Expected: FAIL — currently the handler returns success despite the throw.

- [ ] **Step 4: Add a new `UserResult` error code**

In `service/Api/src/Module/Identity/.../UserResult.cs` (find the file), add:

```csharp
public static readonly Error ProfileCreationFailed = Error.Failure(
    "Identity.ExternalLogin.ProfileCreationFailed",
    "User profile could not be created. Please contact support.");
```

- [ ] **Step 5: Restructure `CreateUserProfileAsync` to return a result**

In `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs`, replace the entire `CreateUserProfileAsync` method (lines 155-180) with:

```csharp
private async Task<Result> CreateUserProfileAsync(User user, CancellationToken cancellationToken)
{
    try
    {
        var profileResult = await mediator.Send(
            new Module.Profile.Features.Store.Profiles.Create.CreateProfile.Command(user.Id, new Module.Profile.Features.Store.Profiles.Create.CreateProfile.Request
            {
                FirstName = user.FirstName,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email!
            }),
            cancellationToken);

        if (profileResult.IsFailure)
        {
            UserProfileLoggers.Management.ProfileCreationFailed(
                logger, user.Id, string.Join("; ", profileResult.Errors.Select(e => $"{e.Code}: {e.Message}")));
            return Result.Failure(UserResult.Failure.ProfileCreationFailed);
        }

        UserProfileLoggers.Management.ProfileCreated(logger, user.Id, profileResult.Value.Id);
        return Result.Ok();
    }
    catch (Exception ex)
    {
        UserProfileLoggers.Management.ProfileCreationFailed(logger, user.Id, ex.Message);
        return Result.Failure(UserResult.Failure.ProfileCreationFailed);
    }
}
```

- [ ] **Step 6: Update the call site to short-circuit on failure**

Replace line 95:

```csharp
await CreateUserProfileAsync(user, cancellationToken);
```

with:

```csharp
var profileResult = await CreateUserProfileAsync(user, cancellationToken);
if (profileResult.IsFailure)
    return profileResult.Errors;
```

- [ ] **Step 7: Re-run the test**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ExternalAuthenticateProfileCreationTests" --no-restore`
Expected: PASS.

- [ ] **Step 8: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Store/Auth/Login/External/Authenticate/ExternalAuthenticate.cs service/Api/src/Module/Identity/.../UserResult.cs service/Api/tests/Module.UnitTests/Identity/ExternalAuthenticateProfileCreationTests.cs
git commit -m "fix(security): surface external login profile-creation failures"
```

---

## Task 5: Build and full test suite

- [ ] **Step 1: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success, zero warnings.

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 3: Run Shared unit tests**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 4: Run integration tests (Docker required)**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "chore(security): post-security-plan cleanup" --allow-empty
```

---

## Self-Review

- **Spec coverage:** SEC-AUTH-001 ✓ Task 2. SEC-AUTH-002 ✓ Task 2. SEC-AUTH-003 ✓ Task 1. SEC-AUTH-004 (gateway encryption key in production) — deferred to a follow-up; not implemented in this plan. REQ-AUTH-001 ✓ Task 4. PAT-AUTH-001 ✓ Task 4 (atomic token issuance).
- **Placeholder scan:** Step 2 of Task 4 uses `MockUserManager` helper — that helper is defined in the same file. Step 2 of Task 4 imports `Module.Profile.Features.Store.Profiles.Create.CreateProfile.Command` directly — this is acceptable in a unit test (test only, not in the handler code).
- **Type consistency:** `UserResult.Failure.ProfileCreationFailed` referenced in Tasks 4 Steps 4, 5, 6. `SecurityHeadersSettingValidator` referenced in Tasks 3 Steps 4, 5. `JwtSettingsValidator` referenced in Tasks 1 Steps 4, 5. `SecurityHeadersSetting.SectionName` — verify the constant exists on the class; if not, add it.
