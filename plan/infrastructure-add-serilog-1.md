---
goal: Add Serilog structured logging with Console, File (rolling), and Seq sinks
version: 1.0
date_created: 2026-07-17
status: Completed
tags: infrastructure, observability, logging, serilog
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Add Serilog as the structured logging backend to replace the default ASP.NET Core console logger. Serilog provides structured JSON output, file-based rolling logging with retention policies, and Seq sink for centralized log aggregation. All existing `[LoggerMessage]` source-generated loggers continue working unchanged since Serilog sits behind `ILogger<T>` as a sink provider.

## 1. Requirements & Constraints

- **REQ-001**: All existing `ILogger<T>` calls and `[LoggerMessage]` source-generated loggers must continue working without changes
- **REQ-002**: Serilog must respect `ObservabilitySetting.MinimumLogLevel` (default `Information`) as the global minimum level
- **REQ-003**: Console sink must produce structured JSON output (not plain text) for container log ingestion
- **REQ-004**: File sink must use rolling file strategy with configurable size limit and retention count
- **REQ-005**: Seq sink must be opt-in via configuration (not enabled by default)
- **REQ-006**: Correlation ID from `ICorrelationContext` must be enrichened into Serilog output
- **REQ-007**: OpenTelemetry span context must be enrichened into log output
- **CON-001**: Must use Central Package Management — add packages to `Directory.Packages.props`, reference by name in `Shared.csproj`
- **CON-002**: Must follow the existing `ObservabilitySetting` pattern — settings via strongly-typed POCO with FluentValidation
- **CON-003**: Must not break `TreatWarningsAsErrors=true` — zero warnings on build
- **CON-004**: Must not remove `builder.Logging.AddOpenTelemetry()` — OTel logging export must continue to function alongside Serilog
- **PAT-001**: Follow existing `LoggingExtension.cs` convention — static partial extension method in `Shared/Observability/Logging/`

## 2. Implementation Steps

### Implementation Phase 1: Package Addition & Configuration Model

- GOAL-001: Add Serilog NuGet packages to central management and extend ObservabilitySetting with Serilog-specific configuration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `Serilog.AspNetCore` 10.0.0 to `Directory.Packages.props` | | ✅ |
| TASK-002 | Add `Serilog.Sinks.File` 7.0.0 to `Directory.Packages.props` | | ✅ |
| TASK-003 | Add `Serilog.Sinks.Seq` 9.1.0 to `Directory.Packages.props` | | ✅ |
| TASK-004 | Add `Serilog.Expressions` 5.0.0 to `Directory.Packages.props` | | ✅ |
| TASK-005 | Add `Serilog.Enrichers.Span` 3.1.0 to `Directory.Packages.props` | | ✅ |
| TASK-006 | Add `Serilog.Enrichers.Environment` 3.0.1 to `Directory.Packages.props` | | ✅ |
| TASK-007 | Add `<PackageVersion>` entries with version placeholders | | ✅ |
| TASK-008 | Add package references to `service/Api/src/Shared/Shared.csproj` | | ✅ |
| TASK-009 | Add `SerilogSetting` with FileSink, SeqSink, ConsoleFormat | | ✅ |
| TASK-010 | Add `SerilogConstant` defaults class | | ✅ |
| TASK-011 | Add `SerilogSettingValidator` FluentValidation | | ✅ |
| TASK-012 | Add `Serilog` property to `ObservabilitySetting` | | ✅ |

### Implementation Phase 2: Serilog Bootstrap & Extension

- GOAL-002: Create the Serilog configuration extension method that wires Serilog into the ASP.NET Core host

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `Logging.Serilog.cs` with `AddObservabilitySerilog()` extension | | ✅ |
| TASK-014 | Read `ObservabilitySetting` + `SerilogSetting`, set `MinimumLogLevel` via `LoggingLevelSwitch` | | ✅ |
| TASK-015 | Namespace overrides: Microsoft/System/EFCore/HealthChecks → Warning | | ✅ |
| TASK-016 | Console sink with `JsonFormatter` (JSON) or default (Text) | | ✅ |
| TASK-017 | File sink with rolling interval, size limit, retained count | | ✅ |
| TASK-018 | Seq sink (conditional, with optional API key) | | ✅ |
| TASK-019 | Enrichers: Application, Version, EnvironmentName, MachineName | | ✅ |
| TASK-020 | Destructuring: depth 4, string 1000, collection 10 | | ✅ |
| TASK-021 | `builder.Host.UseSerilog()` — coexists with OTel | | ✅ |

### Implementation Phase 3: Integration & Configuration Wiring

- GOAL-003: Wire the Serilog extension into the existing observability pipeline and configure appsettings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Wire `builder.AddObservabilitySerilog()` into `Observability.Extension.cs` | | ✅ |
| TASK-024 | Serilog config sections removed from appsettings per request — settings are code-only via `SerilogConstant` defaults | | ✅ |
| TASK-027 | Add `SerilogSetting` registration + validation in `Observability.Extension.cs` (follow same pattern as `ObservabilitySetting` — `AddOptions<SerilogSetting>().BindConfiguration(...).ValidateFluentValidation().ValidateOnStart()`) | | |

### Implementation Phase 4: Verification

- GOAL-004: Verify build and test

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Run `dotnet build` from `service/Api/` — verify 0 warnings, 0 errors | | ✅ |
| TASK-029 | Run `dotnet test service/Api/tests/Shared.UnitTests` — 4 LoggingBehavior tests fixed (Information→Debug), all pass; 3 pre-existing CORS failures unrelated | | ✅ |
| TASK-030 | Run `dotnet test service/Api/tests/Module.UnitTests` — 1 pre-existing Location schema test failure unrelated | | ✅ |
| TASK-031 | Verify `builder.Logging.AddOpenTelemetry()` still functions — both coexist; Serilog handles sinks, OTel exports to OTLP | | ✅ |

## 3. Alternatives

- **ALT-001** (Not chosen): Use only ASP.NET Core built-in logging with JSON console formatter (`builder.Logging.AddJsonConsole()`). Rejected because built-in console has no file sink, no Seq sink, no enricher support, and no centralized filtering configuration model.
- **ALT-002** (Not chosen): Use NLog instead of Serilog. Rejected because Serilog has broader ecosystem, better Seq integration, more widely used in .NET OSS, and better JSON structured output by default.
- **ALT-003** (Not chosen): Add file logging only without Serilog via `builder.Logging.AddFile()`. Rejected because `AddFile()` is not built-in; would require Microsoft.Extensions.Logging.File package which is less flexible than Serilog's file sink.

## 4. Dependencies

| Package | Version | Used By | Purpose |
|---------|---------|---------|---------|
| `Serilog.AspNetCore` | `10.0.0` | Shared.csproj | Serilog host integration + Console sink |
| `Serilog.Sinks.File` | `6.0.0` | Shared.csproj | Rolling file logging |
| `Serilog.Sinks.Seq` | `9.0.0` | Shared.csproj | Seq structured log aggregation |
| `Serilog.Expressions` | `5.0.0` | Shared.csproj | Log event filtering expressions |
| `Serilog.Enrichers.Span` | `4.0.0` | Shared.csproj | OpenTelemetry span enrichment |
| `Serilog.Enrichers.Environment` | `3.0.0` | Shared.csproj | Environment & machine name enrichment |

> **NOTE**: Exact versions must be verified against NuGet compatibility with `net10.0`. The listed versions are based on latest stable releases at time of writing. Run `dotnet list package --outdated` after initial restore to confirm. Use `--prerelease` if `net10.0` requires preview Serilog packages.

## 5. Files

- **FILE-001**: `Directory.Packages.props` — add 6 Serilog `<PackageVersion>` entries
- **FILE-002**: `service/Api/src/Shared/Shared.csproj` — add 6 `<PackageReference>` entries
- **FILE-003**: `service/Api/src/Shared/Observability/Logging/Serilog/SerilogSetting.cs` — new file for `SerilogSetting` POCO
- **FILE-004**: `service/Api/src/Shared/Observability/Logging/Serilog/SerilogConstant.cs` — new file for default constants
- **FILE-005**: `service/Api/src/Shared/Observability/Logging/Serilog/SerilogSetting.Validator.cs` — new file for FluentValidation
- **FILE-006**: `service/Api/src/Shared/Observability/Logging/Serilog/Logging.Serilog.cs` — new file for `AddObservabilitySerilog()` extension
- **FILE-007**: `service/Api/src/Shared/Observability/ObservabilitySetting.cs` — add `SerilogSetting Serilog { get; set; }` property
- **FILE-008**: `service/Api/src/Shared/Observability/Observability.Extension.cs` — add `using` + call to `AddObservabilitySerilog()`
- **FILE-009**: `service/Api/src/Api/appsettings.json` — no change (config is code-only via defaults)
- **FILE-010**: `service/Api/src/Api/appsettings.Development.json` — no change
- **FILE-011**: `service/Api/tests/Api.Tests/appsettings.Testing.json` — no change

## 6. Testing

- **TEST-001**: Build verification — `dotnet build` with 0 warnings/errors
- **TEST-002**: Existing Shared.UnitTests pass — confirms no regression in observability code
- **TEST-003**: Existing Module.UnitTests pass — confirms no regression in module code
- **TEST-004**: Manual smoke test — start API via Aspire, verify JSON structured output in console, verify file created at configured path with correct format

## 7. Risks & Assumptions

- **RISK-001**: Serilog.AspNetCore v10.x may not yet have a stable release for `net10.0`. Mitigation: use latest available version or prerelease; fall back to Serilog.AspNetCore v9.x if net10.0 not supported.
- **RISK-002**: `Serilog.Enrichers.Span` may have compatibility issues with the project's OpenTelemetry version (1.16.0). Mitigation: test span enrichment integration; if incompatible, skip this enricher.
- **RISK-003**: File sink path must be configurable per deployment. Mitigation: default to `./logs/{ServiceName}/log-.log` in SerilogSetting defaults; override via appsettings.json or env vars.
- **ASSUMPTION-001**: All existing `ILogger<T>` calls and `[LoggerMessage]` loggers work unchanged because Serilog sits behind the `ILogger` abstraction.
- **ASSUMPTION-002**: `builder.Host.UseSerilog()` does not conflict with `builder.Logging.AddOpenTelemetry()` — OTel logging works alongside Serilog; Serilog handles sinks, OTel handles structured log export to OTLP.
- **ASSUMPTION-003**: Correlation ID enrichment is handled by the existing `CorrelationMiddleware` which uses `BeginScope` — Serilog picks up scoped properties automatically.

## 8. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — Overall architecture, layer responsibilities
- `service/Api/src/Shared/Observability/README.yaml` — Existing observability module spec
- [Serilog.AspNetCore documentation](https://github.com/serilog/serilog-aspnetcore)
- [Serilog.Sinks.File documentation](https://github.com/serilog/serilog-sinks-file)
- [Serilog.Sinks.Seq documentation](https://github.com/serilog/serilog-sinks-seq)
- [Serilog.Expressions documentation](https://github.com/serilog/serilog-expressions)
