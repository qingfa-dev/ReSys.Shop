---
goal: Scaffold EF Core Migration Project and Generate Initial Database Migration
version: 1.0
date_created: 2026-07-02
owner: ReSys Team
status: Planned
tags: infrastructure, migration, data
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create the missing ` Api.Migrations` class library project that the `ApplicationDbContext` references via `MigrationsAssembly(" Api.Migrations")`, then generate the initial EF Core migration (`InitialCreate`) capturing all entity configurations from both the `Shared` and `Module` assemblies.

## 1. Requirements & Constraints

- **REQ-001**: Create a class library project named ` Api.Migrations` at `service/Api/src/Migrations/ Api.Migrations.csproj`
- **REQ-002**: The new project must reference `Shared` and `Module` to access `ApplicationDbContext` and all module-level entity configurations
- **REQ-003**: Add the project to `ReSys.Shop.slnx` under folder `/service/Api/src/`
- **REQ-004**: Provide a `DesignTimeDbContextFactory` so `dotnet ef migrations add` can resolve `ApplicationDbContext` at design time with all entity configurations loaded
- **REQ-005**: The migration assembly name must be ` Api.Migrations` to match the existing `MigrationsAssembly` string in `PersistenceExtensions.cs:77`
- **REQ-006**: Generate the initial migration (`InitialCreate`) capturing all entity types: Identity tables (`identity.users`, `identity.roles`, `identity.user_roles`, `identity.user_claims`, `identity.user_logins`, `identity.user_tokens`, `identity.role_claims`, `identity.user_passkeys`) and Location tables (`Location.countries`, `Location.states`), plus all cross-cutting columns (audit, version, soft-delete, slug)
- **CON-001**: The `ApplicationDbContext` is sealed and uses `ApplyConfigurationsFromAssembly` scanning — the design-time factory must set `AdditionalConfigurationsAssemblies` to include the `Module` assembly before constructing the context
- **CON-002**: Use `dotnet ef` tool already configured at version `10.0.9` in `dotnet-tools.json`
- **CON-003**: All entity type configurations already exist — the initial migration must reflect the current model without manual schema changes
- **PAT-001**: Follow existing project conventions: `net10.0`, `Nullable` enabled, `ImplicitUsings` enabled, `LangVersion` preview
- **GUD-001**: Design-time factory must resolve a postgres connection string from configuration or use a fallback; the factory is for tooling only, not runtime

## 2. Implementation Steps

### Implementation Phase 1 — Create Migration Project Scaffold

- GOAL-001: Create the ` Api.Migrations` class library project with proper references

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create directory `service/Api/src/Migrations/` | |  |
| TASK-002 | Create `service/Api/src/Migrations/ Api.Migrations.csproj` — class library targeting `net10.0` with `Nullable`/`ImplicitUsings` enabled, project references to `../Shared/Shared.csproj` and `../Module/Module.csproj`, and a `DesignTime` package reference to `Microsoft.EntityFrameworkCore.Design` (version `$(EFCoreVersion)`) with `PrivateAssets=all` | |  |
| TASK-003 | Create `service/Api/src/Migrations/DesignTimeDbContextFactory.cs` — implement `IDesignTimeDbContextFactory<ApplicationDbContext>` that reads a connection string named `DefaultConnection` from `appsettings.json` (relative path `../../Api/appsettings.json`) or falls back to `Host=localhost;Database=resys_shop;Username=postgres;Password=postgres`, sets `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(IModuleMarker).Assembly]`, builds `DbContextOptionsBuilder<ApplicationDbContext>` with `UseNpgsql` + `UseVector` + snake_case naming, and returns a new `ApplicationDbContext` | |  |
| TASK-004 | Add project entry to `ReSys.Shop.slnx` under `<Folder Name="/service/Api/src/">` with `Project Path="service/Api/src/Migrations/ Api.Migrations.csproj"` | |  |
| TASK-005 | Run `dotnet restore` from the solution root to verify package resolution | |  |
| TASK-006 | Run `dotnet build` from the solution root to verify the solution compiles cleanly with the new project | |  |

### Implementation Phase 2 — Generate Initial Migration

- GOAL-002: Generate the initial database migration snapshot using `dotnet ef`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Run `dotnet ef migrations add InitialCreate --project service/Api/src/Migrations/ Api.Migrations.csproj --startup-project service/Api/src/Api/Api.csproj` from the solution root — verify it generates migration files (e.g., `20YYMMDDHHMMSS_InitialCreate.cs`, `20YYMMDDHHMMSS_InitialCreate.Designer.cs`, `ApplicationDbContextModelSnapshot.cs`) inside the `Migrations/` project directory | |  |
| TASK-008 | Run `dotnet build` from the solution root again to verify the generated migration code compiles | |  |

### Implementation Phase 3 — Verification

- GOAL-003: Verify the migration captures all expected entities and the DbContext initializer can discover the migration assembly

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Read the generated `InitialCreate.cs` migration file and verify it contains: `migrationBuilder.CreateTable` calls for `Identity.users`, `Identity.roles`, `Identity.user_roles`, `Identity.user_claims`, `Identity.user_logins`, `Identity.user_tokens`, `Identity.role_claims`, `Identity.user_passkeys`, `Location.countries`, `Location.states` — plus the `vector` extension enablement via `migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector")` and any `migrationBuilder.AlterDatabase().Annotation("Npgsql:PostgresExtension:vector", ...)` | |  |
| TASK-010 | Run `dotnet build` on the full solution one final time to confirm zero warnings/errors | |  |

## 3. Alternatives

- **ALT-001**: Embed migrations inside the `Api` project — rejected because `MigrationsAssembly` is explicitly set to ` Api.Migrations` in `PersistenceExtensions.cs`, and a separate project enforces clean separation of concerns
- **ALT-002**: Embed migrations inside the `Shared` project — rejected because `Shared` is a class library (not `Microsoft.NET.Sdk.Web`), and migrations would need the EF Core Design package at runtime rather than as a development dependency
- **ALT-003**: Use `DbContextFactory` already registered in DI rather than a design-time factory — rejected because `AddDbContextFactory` is not currently registered and the design-time factory pattern is the standard approach for separate migration assemblies

## 4. Dependencies

- **DEP-001**: `dotnet-ef` CLI tool version `10.0.9` (already configured in `dotnet-tools.json`)
- **DEP-002**: `Microsoft.EntityFrameworkCore.Design` NuGet package version `10.0.9` (already in `Directory.Packages.props` at line 37)
- **DEP-003**: `Microsoft.EntityFrameworkCore` version `10.0.9` (transitively available via `Shared` reference)
- **DEP-004**: `Npgsql.EntityFrameworkCore.PostgreSQL` version `10.0.2` (transitively available via `Shared` reference)
- **DEP-005**: Running PostgreSQL instance for `dotnet ef migrations add` to connect (design-time tool requires a live database connection)

## 5. Files

- **FILE-001**: `service/Api/src/Migrations/ Api.Migrations.csproj` — new migration project file
- **FILE-002**: `service/Api/src/Migrations/DesignTimeDbContextFactory.cs` — design-time factory for EF tooling
- **FILE-003**: `ReSys.Shop.slnx` — add new project reference entry
- **FILE-004**: `service/Api/src/Migrations/<timestamp>_InitialCreate.cs` — generated initial migration (Up/Down)
- **FILE-005**: `service/Api/src/Migrations/<timestamp>_InitialCreate.Designer.cs` — generated migration metadata
- **FILE-006**: `service/Api/src/Migrations/ApplicationDbContextModelSnapshot.cs` — generated model snapshot

## 6. Testing

- **TEST-001**: Run `dotnet build` — solution must build with zero errors and zero warnings after migration project is added
- **TEST-002**: Verify the generated migration `.cs` file compiles by running `dotnet build` after migration generation
- **TEST-003**: Manual inspection of `InitialCreate.cs` confirms all expected entity types are mapped (Identity + Location + cross-cutting columns)
- **TEST-004**: Confirm `dotnet ef migrations list --project service/Api/src/Migrations/ Api.Migrations.csproj --startup-project service/Api/src/Api/Api.csproj` returns the newly created migration

## 7. Risks & Assumptions

- **RISK-001**: `dotnet ef migrations add` requires a live PostgreSQL connection at design time. If no database is available, the command will fail. **Mitigation**: Provide a configurable connection string fallback in the `DesignTimeDbContextFactory`.
- **RISK-002**: Entity configurations in `Module` are discovered via `AdditionalConfigurationsAssemblies` at runtime. If the design-time factory does not set this static property before constructing the context, entity types from `Module` (Countries, States) will be missing from the generated migration. **Mitigation**: The `DesignTimeDbContextFactory` explicitly sets `ApplicationDbContext.AdditionalConfigurationsAssemblies` to include `typeof(IModuleMarker).Assembly`.
- **RISK-003**: The `MigrationsAssembly` string `" Api.Migrations"` must match the actual assembly name produced by the project. This is the default assembly name for a project named ` Api.Migrations`, so no explicit `<AssemblyName>` override is needed.
- **ASSUMPTION-001**: A PostgreSQL instance is available on localhost with default credentials, or the connection string in `Api/appsettings.json` is usable by the design-time factory.
- **ASSUMPTION-002**: No additional modules beyond `Location` currently contribute entity configurations — if more modules are added later, they must also be added to `DesignTimeDbContextFactory`'s `AdditionalConfigurationsAssemblies`.

## 8. Related Specifications / Further Reading

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Design-Time DbContext Creation](https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation/)
- [Using a Separate Migrations Project](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects)
