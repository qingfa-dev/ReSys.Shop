---
goal: Add All Missing Configuration Settings to appsettings.json with Development-Safe Placeholder Values
version: 1.0
date_created: 2026-07-02
owner: ReSys Team
status: Planned
tags: data, configuration, settings
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The application throws `OptionsValidationException` at startup because 20+ options classes registered via `BindConfiguration()` have no corresponding JSON configuration sections. The most immediate error is `JwtSettings` validation failure (Secret, Issuer, Audience, AccessTokenExpirationInMinutes, RefreshTokenExpirationInDays). This plan adds all missing configuration sections to `appsettings.Development.json` with development-safe placeholder values.

## 1. Requirements & Constraints

- **REQ-001**: Add all settings sections that are registered via `.BindConfiguration()` in extension files to `appsettings.Development.json` with development-safe placeholder values
- **REQ-002**: `JwtSettings.Secret` must be at least 32 characters per `JwtSettingsConstant.Constraints.Secret.MinLength` and must not be in the `WeakSecrets` list
- **REQ-003**: All validation rules defined in each setting's `FluentValidation` validator must be satisfied by the placeholder values
- **REQ-004**: `appsettings.json` (shared/production) must contain the structure with empty/override-able values for security-sensitive fields (secrets, keys, passwords); actual values should come from environment variables or user secrets
- **REQ-005**: Do not modify any `.cs` files — only `appsettings.json` and `appsettings.Development.json`
- **REQ-006**: Development values must work out of the box for local development without external services (e.g., disable malware scanning, use local SMTP, disable SMS)
- **REQ-007**: The `SecurityHeadersSetting` registration does not call `.BindConfiguration()` so no section is needed for it
- **REQ-008**: `CachingSetting` validators inject `IConfiguration` to check for Redis connection strings — since no Redis is required in development, the `Distributed.Enabled` and `Hybrid.Enabled` should be set correctly to avoid validation failures

## 2. Implementation Steps

### Implementation Phase 1 — Add All Configuration Sections to appsettings.json

- GOAL-001: Add the full configuration structure with empty/placeholder values to the shared `appsettings.json` for production use

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `"Authentication:Jwt"` section to `appsettings.json` with `Secret` (empty string `""`), `Issuer` (`"ReSys.Shop"`), `Audience` (`"ReSys.Shop"`), `AccessTokenExpirationInMinutes` (`15`), `RefreshTokenExpirationInDays` (`7`), `Algorithm` (`"HS256"`), `TokenSecurity.RotationEnabled` (`false`), `TokenSecurity.ReuseDetectionEnabled` (`false`), `TokenSecurity.SlidingExpirationEnabled` (`false`), `TokenSecurity.MaxTokenAgeDays` (`30`) | | |
| TASK-002 | Add `"Authentication:Google"` section with `ClientId` (`""`) | | |
| TASK-003 | Add `"Authorization"` section with `PermissionCache.SlidingExpiration` (`"00:05:00"`), `PermissionCache.AbsoluteExpiration` (`"00:30:00"`) | | |
| TASK-004 | Add `"Cors"` section with `Origins` (`["*"]`), `AllowCredentials` (`false`) | | |
| TASK-005 | Add `"AntiForgery"` section with `IsEnabled` (`true`), `HeaderName` (`"X-CSRF-TOKEN"`), `Required` (`true`), `CookieName` (`".AspNetCore.Antiforgery"`), `CookieSameSite` (`"Strict"`), `CookieSecurePolicy` (`"Always"`), `CookieHttpOnly` (`true`), `CookieMaxAgeMinutes` (`null`) | | |
| TASK-006 | Add `"GuestSession"` section with `CookieName` (`"Guest"`), `CookieSameSite` (`"Lax"`), `CookieSecurePolicy` (`"Always"`), `CookieHttpOnly` (`true`), `ExpirationInDays` (`30`) | | |
| TASK-007 | Add `"Observability"` section with `ServiceName` (`"ReSys.Api"`), `ServiceVersion` (`"1.0.0"`), `UseAspireOTLPExporter` (`true`), `CorrelationHeader` (`"X-Correlation-Id"`), `MinimumLogLevel` (`"Information"`), `SensitiveHeaders` (`["Authorization","Cookie","X-Api-Key"]`), `ExposeDetailedReport` (`false`) | | |
| TASK-008 | Add `"Caching"` section with `Enabled` (`true`), `Memory.Enabled` (`true`), `Memory.DefaultExpirationMinutes` (`5`), `Memory.CompactionPercentage` (`5`), `Distributed.Enabled` (`false`), `Distributed.Type` (`"redis"`), `Distributed.DefaultExpirationMinutes` (`60`), `Hybrid.Enabled` (`true`), `Hybrid.DefaultExpirationMinutes` (`5`), `Hybrid.MaximumPayloadBytes` (`1048576`), `Hybrid.MaximumKeyLength` (`512`) | | |
| TASK-009 | Add `"Http"` section with `DefaultTimeoutSeconds` (`30`), `AttachResiliencePipelineByDefault` (`true`), `PropagateCorrelationId` (`true`), `Clients` (`{}`) | | |
| TASK-010 | Add `"Storage"` section with `DefaultProvider` (`"Local"`), `BaseUrl` (`""`), `Enabled` (`true`), `Security.MaxFileSizeBytes` (`10485760`), `Security.AllowedExtensions` (`[".jpg",".jpeg",".png",".gif",".webp",".svg",".pdf",".doc",".docx",".xls",".xlsx",".csv",".txt",".zip"]`), `Security.BlockedExtensions` (`[".exe",".bat",".cmd",".com",".msi",".scr",".ps1",".vbs",".jar",".war",".iso"]`), `Security.ValidateMagicBytes` (`true`), `Security.EncryptionKey` (`""`), `Providers.Local.IsEnabled` (`true`), `Providers.Local.LocalPath` (`"./uploads"`), `Providers.Local.BufferSize` (`4096`), `Providers.Azure.IsEnabled` (`false`), `Providers.Azure.ConnectionString` (`""`), `Providers.Azure.ContainerName` (`""`), `Providers.Azure.BufferSize` (`4096`), `Providers.S3.IsEnabled` (`false`), `Providers.S3.ServiceUrl` (`""`), `Providers.S3.AccessKey` (`""`), `Providers.S3.SecretKey` (`""`), `Providers.S3.BucketName` (`""`), `Providers.S3.Region` (`""`), `Providers.S3.ForcePathStyle` (`false`), `Providers.S3.BufferSize` (`4096`) | | |
| TASK-011 | Add `"Storage:AntiForgery"` section with `MaxConsecutiveFailures` (`5`), `BlockDuration` (`"00:15:00"`) | | |
| TASK-012 | Add `"Storage:MalwareScanner"` section with `Enabled` (`false`), `ClamAvHost` (`"localhost"`), `ClamAvPort` (`3310`), `DisableInDevelopment` (`true`) | | |
| TASK-013 | Add `"BackgroundJobs"` section with `Enabled` (`true`), `DashboardPath` (`"/jobs"`), `CachingEnabled` (`false`) | | |
| TASK-014 | Add `"Notification"` section with `EnableBackgroundJobs` (`false`), `ApplicationName` (`"ReSys Shop"`), `SupportEmail` (`"support@resys.shop"`), `SupportPhone` (`""`), `CustomerSupportLink` (`""`), `ApplicationUrl` (`""`), `UnsubscribeUrl` (`""`), `SurveyUrl` (`""`), `Channels.Email.Enabled` (`true`), `Channels.Email.FromEmail` (`""`), `Channels.Email.FromName` (`""`), `Channels.Email.Providers.SendGrids.Enabled` (`false`), `Channels.Email.Providers.SendGrids.Priority` (`2`), `Channels.Email.Providers.SendGrids.RetryCount` (`3`), `Channels.Email.Providers.SendGrids.Timeout` (`"00:00:30"`), `Channels.Email.Providers.SendGrids.ApiKey` (`""`), `Channels.Email.Providers.Smtp.Enabled` (`true`), `Channels.Email.Providers.Smtp.Priority` (`1`), `Channels.Email.Providers.Smtp.RetryCount` (`3`), `Channels.Email.Providers.Smtp.Timeout` (`"00:00:30"`), `Channels.Email.Providers.Smtp.Host` (`"localhost"`), `Channels.Email.Providers.Smtp.Port` (`25`), `Channels.Email.Providers.Smtp.EnableSsl` (`false`), `Channels.Email.Providers.Smtp.UseDefaultCredentials` (`true`), `Channels.Sms.Enabled` (`false`), `Channels.Sms.DefaultSenderNumber` (`""`), `Channels.Sms.Providers.Sinch.Enabled` (`false`), `Channels.Sms.Providers.Sinch.Priority` (`1`), `Channels.Sms.Providers.Sinch.RetryCount` (`3`), `Channels.Sms.Providers.Sinch.Timeout` (`"00:00:30"`), `Channels.Sms.Providers.Sinch.ProjectId` (`""`), `Channels.Sms.Providers.Sinch.KeyId` (`""`), `Channels.Sms.Providers.Sinch.KeySecret` (`""`), `Channels.Sms.Providers.Sinch.SenderPhoneNumber` (`""`) | | |

### Implementation Phase 2 — Add Development Overrides to appsettings.Development.json

- GOAL-002: Add development-safe placeholder values that satisfy all validators, including a valid JWT secret

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Replace the current `appsettings.Development.json` content with a full development configuration that includes all sections from Phase 1, overriding only values that differ from production defaults. Key development-specific overrides: `Authentication.Jwt.Secret` must be `"ThisIsADevelopmentJwtSecretKeyThatIsLongEnough32!"` (exactly 49 chars, satisfies `MinLength(32)`), `Authentication.Google.ClientId` set to `""`, `Cors.Origins` set to `["http://localhost:5173","http://localhost:4173","http://localhost:3000"]`, `Cors.AllowCredentials` (`true`), `Storage.Providers.Local.LocalPath` (`"./uploads"`), `Notification.Channels.Email.Providers.Smtp.Host` (`"localhost"`), `Notification.Channels.Email.Providers.Smtp.Port` (`1025`) for Mailpit/MailHog, `BackgroundJobs.CachingEnabled` (`false`) | | |

### Implementation Phase 3 — Verification

- GOAL-003: Verify the application starts without `OptionsValidationException`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Run `dotnet build service/Api/src/Api/Api.csproj` and confirm 0 errors, 0 warnings | | |
| TASK-017 | Attempt to start the application with `ASPNETCORE_ENVIRONMENT=Development dotnet run --project service/Api/src/Api/Api.csproj --no-launch-profile` and verify it initializes past the options validation phase (will hit PostgreSQL connection error next — that is expected and separate) | | |

## 3. Alternatives

- **ALT-001**: Use `UserSecrets` instead of `appsettings.Development.json` for secrets — not chosen because `appsettings.Development.json` is more transparent for onboarding new developers and can be `.gitignore`-d if needed
- **ALT-002**: Set all values via environment variables — not chosen because it's impractical for 50+ settings; environment variables should only override specific values in production
- **ALT-003**: Disable options validation in development — rejected because validation catches configuration errors early and should be active in all environments

## 4. Dependencies

- **DEP-001**: The `appsettings.json` file at `service/Api/src/Api/appsettings.json`
- **DEP-002**: The `appsettings.Development.json` file at `service/Api/src/Api/appsettings.Development.json`
- **DEP-003**: All settings classes and their validators listed in the exploration output

## 5. Files

- **FILE-001**: `service/Api/src/Api/appsettings.json` — add all configuration sections with production-suitable defaults
- **FILE-002**: `service/Api/src/Api/appsettings.Development.json` — override with development-safe values

## 6. Testing

- **TEST-001**: `dotnet build` passes with 0 errors and 0 warnings
- **TEST-002**: Application starts past options validation (confirmed by the absence of `OptionsValidationException` in startup logs)
- **TEST-003**: `dotnet run` with `ASPNETCORE_ENVIRONMENT=Development` does not throw `OptionsValidationException` for any settings class

## 7. Risks & Assumptions

- **RISK-001**: If a setting has a validator that injects `IConfiguration` and checks for connection strings (e.g., `CachingSettingValidator`, `BackgroundJobSettingValidator`), the app may still fail at options access time if Redis connection strings are missing. **Mitigation**: Set `Distributed.Enabled` and `BackgroundJobs.CachingEnabled` to `false` in development.
- **RISK-002**: The `JwtSettings.Secret` placeholder value `"ThisIsADevelopmentJwtSecretKeyThatIsLongEnough32!"` is in the `WeakSecrets` list check? Let me verify: the weak secrets list contains `"SuperSecretKeyForTestingPurposesOnly123!"`, `"secret"`, `"123456"`, `"password"`, `"admin"`, `"test"`, `"default"`. The chosen value is NOT in this list. **Safe.**
- **RISK-003**: SMTP on port 1025 assumes Mailpit/MailHog is running locally — the app will log email errors silently if not, so this is development-safe.
- **ASSUMPTION-001**: All section names are derived from the `SectionName` or `Section` constant on each settings class and match exactly what `BindConfiguration()` expects.
- **ASSUMPTION-002**: The `SecurityHeadersSetting` registration (`AddOptions<SecurityHeadersSetting>()` without `.BindConfiguration()`) does not need a configuration section.

## 8. Related Specifications / Further Reading

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/options/)
- [FluentValidation integration with IOptions](https://docs.fluentvalidation.net/en/latest/aspnet.html)
