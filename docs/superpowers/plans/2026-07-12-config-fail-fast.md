# Configuration Fail-Fast Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `.ValidateOnStart()` to every `AddOptions<T>().BindConfiguration(...).ValidateFluentValidation()` chain in `service/Api/src/Shared/`, so misconfiguration fails at host boot rather than at first request.

**Architecture:** Pure mechanical change. Each options-registration site gets a `.ValidateOnStart()` call appended. An integration test boots the host with a misconfigured value and asserts the host throws `OptionsValidationException`.

**Tech Stack:** .NET 10, xUnit v3, Moq, `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`).

## Global Constraints

- `TreatWarningsAsErrors=true`
- Pattern: `services.AddOptions<T>().BindConfiguration(Section).ValidateFluentValidation().ValidateOnStart()` — every new options registration MUST follow this
- Validator MUST be registered with `AddSingleton<IValidator<T>, TValidator>()` BEFORE the options call
- Test pattern: `[Fact(DisplayName = "...")]`, `[Trait("Category", "Integration")]`

## File Structure

### Files to modify

| File | Change |
|------|--------|
| `service/Api/src/Shared/Security/AntiForgery/AntiForgery.Extensions.cs` | Add `.ValidateOnStart()` |
| `service/Api/src/Shared/Security/Authentication/Guest/GuestSession.Extensions.cs` | Add `.ValidateOnStart()` |
| `service/Api/src/Shared/Performance/Caching/Caching.Extension.cs` | Add `.ValidateOnStart()` |
| `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs` | Add `.ValidateOnStart()` |
| `service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs` | Add `.ValidateOnStart()` to 6 settings |

### Files to create

| File | Purpose |
|------|---------|
| `service/Api/tests/Api.Tests/Scenarios/Shared/OptionsValidationOnStartTests.cs` | Integration test: host boot fails on misconfig |

---

## Task 1: AntiForgery `ValidateOnStart`

**Files:**
- Modify: `service/Api/src/Shared/Security/AntiForgery/AntiForgery.Extensions.cs:18-20`

- [ ] **Step 1: Read the current options registration**

Read lines 17-21 of `AntiForgery.Extensions.cs` and confirm the `BindConfiguration` + `ValidateFluentValidation` chain.

- [ ] **Step 2: Add `ValidateOnStart`**

Replace lines 18-20:

```csharp
builder.Services.AddOptions<AntiForgerySetting>()
    .BindConfiguration(AntiForgerySetting.SectionName)
    .ValidateFluentValidation();
```

with:

```csharp
builder.Services.AddOptions<AntiForgerySetting>()
    .BindConfiguration(AntiForgerySetting.SectionName)
    .ValidateFluentValidation()
    .ValidateOnStart();
```

- [ ] **Step 3: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Security/AntiForgery/AntiForgery.Extensions.cs
git commit -m "fix(shared): validate AntiForgerySetting on start"
```

---

## Task 2: GuestSession `ValidateOnStart`

**Files:**
- Modify: `service/Api/src/Shared/Security/Authentication/Guest/GuestSession.Extensions.cs:17-19`

- [ ] **Step 1: Read the current options registration**

Read lines 16-20 of `GuestSession.Extensions.cs`.

- [ ] **Step 2: Add `ValidateOnStart`**

Append `.ValidateOnStart()` to the existing `AddOptions<GuestSessionSetting>()` chain.

- [ ] **Step 3: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Security/Authentication/Guest/GuestSession.Extensions.cs
git commit -m "fix(shared): validate GuestSessionSetting on start"
```

---

## Task 3: Caching `ValidateOnStart`

**Files:**
- Modify: `service/Api/src/Shared/Performance/Caching/Caching.Extension.cs:40-42`

- [ ] **Step 1: Read the current options registration**

Read lines 38-44 of `Caching.Extension.cs`. There may be multiple `AddOptions` calls (memory, distributed, hybrid). Apply the fix to all of them.

- [ ] **Step 2: Add `ValidateOnStart` to each `AddOptions` chain**

For every `AddOptions<CachingSetting>()` (or any nested setting like `MemoryCachingSetting`, `DistributedCachingSetting`, `HybridCachingSetting`) with a `BindConfiguration` + `ValidateFluentValidation` chain, append `.ValidateOnStart()`.

- [ ] **Step 3: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Performance/Caching/Caching.Extension.cs
git commit -m "fix(shared): validate caching settings on start"
```

---

## Task 4: BackgroundJobs `ValidateOnStart`

**Files:**
- Modify: `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs:44-46`

- [ ] **Step 1: Read the current options registration**

Read lines 42-48 of `Background.Extension.cs`. Confirm the section name and the validator type.

- [ ] **Step 2: Add `ValidateOnStart`**

Append `.ValidateOnStart()` to the `AddOptions<BackgroundJobSetting>()` chain.

- [ ] **Step 3: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs
git commit -m "fix(shared): validate BackgroundJobSetting on start"
```

---

## Task 5: Notification settings `ValidateOnStart` (6 settings)

**Files:**
- Modify: `service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs:74-106`

- [ ] **Step 1: Read the current options registrations**

Read lines 70-110 of `Notification.Extension.cs`. There are 6 `AddOptions` chains: `NotificationSetting`, `EmailChannelSetting`, `SmsChannelSetting`, `SendGridProviderSetting`, `SmtpProviderSetting`, `SinchProviderSetting`.

- [ ] **Step 2: Add `ValidateOnStart` to each chain**

For each of the 6 chains, append `.ValidateOnStart()` after `.ValidateFluentValidation()`.

- [ ] **Step 3: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs
git commit -m "fix(shared): validate notification settings on start (6 settings)"
```

---

## Task 6: Integration test for `ValidateOnStart` boot failure

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Shared/OptionsValidationOnStartTests.cs`

**Context:** Verify that misconfiguration now fails at host boot, not at first request.

- [ ] **Step 1: Read `ApiFactory` for in-memory config override pattern**

Open `service/Api/tests/Api.Tests/`. Find the `ApiFactory` (or `WebApplicationFactory<>` subclass) used by other integration tests. Identify how configuration overrides are applied (typically via `IConfigurationBuilder.AddInMemoryCollection` inside `ConfigureWebHost`).

- [ ] **Step 2: Write the failing test**

Create file `service/Api/tests/Api.Tests/Scenarios/Shared/OptionsValidationOnStartTests.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Api.Tests.Scenarios.Shared;

[Trait("Category", "Integration")]
public class OptionsValidationOnStartTests
{
    [Fact(DisplayName = "ValidateOnStart: empty SMTP host fails host build")]
    public void EmptySmtpHost_FailsHostBuild()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Notification:Channels:Email:Providers:Smtp:Host"] = "",
                        ["Notification:Channels:Email:Providers:Smtp:Port"] = "1025"
                    });
                });
            });

        var act = () =>
        {
            using var scope = factory.Services.CreateScope();
            var opts = scope.ServiceProvider.GetRequiredService<IOptions<NotificationSetting>>();
            _ = opts.Value; // force validation
        };

        act.Should().Throw<OptionsValidationException>()
           .WithMessage("*Smtp*Host*");
    }

    [Fact(DisplayName = "ValidateOnStart: missing anti-forgery cookie name fails host build")]
    public void EmptyAntiForgeryCookieName_FailsHostBuild()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AntiForgery:CookieName"] = ""
                    });
                });
            });

        var act = () =>
        {
            using var scope = factory.Services.CreateScope();
            var opts = scope.ServiceProvider.GetRequiredService<IOptions<AntiForgerySetting>>();
            _ = opts.Value;
        };

        act.Should().Throw<OptionsValidationException>()
           .WithMessage("*AntiForgery*CookieName*");
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~OptionsValidationOnStartTests" --no-restore`
Expected: FAIL — currently `IOptions.Value` returns the bound value without validating, so the test does not throw.

- [ ] **Step 4: Re-run after the Tasks 1-5 fixes are in place**

After completing Tasks 1-5, re-run the test:

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~OptionsValidationOnStartTests" --no-restore`
Expected: PASS — `IOptions.Value` now triggers validation eagerly.

- [ ] **Step 5: Commit**

```bash
git add service/Api/tests/Api.Tests/Scenarios/Shared/OptionsValidationOnStartTests.cs
git commit -m "test(shared): add integration test for ValidateOnStart boot failures"
```

---

## Task 7: Build and full test suite

- [ ] **Step 1: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 3: Run Shared unit tests**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 4: Run integration tests**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --no-restore`
Expected: PASS, including `OptionsValidationOnStartTests`.

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "chore(shared): post-fail-fast-plan cleanup" --allow-empty
```

---

## Self-Review

- **Spec coverage:** REQ-CFG-001 ✓ Tasks 1-5. REQ-CFG-002 ✓ Tasks 1-5 (covers all 6 listed settings). REQ-CFG-003 ✓ covered in the security plan, not duplicated here. AC-CFG-001 ✓ Task 6.
- **Placeholders:** none. Each task contains the exact `.ValidateOnStart()` call.
- **Type consistency:** `IOptions<T>` used consistently. `OptionsValidationException` referenced in Task 6 with matching import (`Microsoft.Extensions.Options`).
