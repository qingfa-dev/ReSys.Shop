# Feature: Api.Tests Integration Suite Speedup

## Goal

Reduce `service/Api/tests/Api.Tests/Api.Tests.csproj` test suite wall-clock time
from **5-15 minutes** to **≤3 minutes** on dev workstations and **≤5 minutes** on
CI (Linux + Podman), via low/medium-risk optimizations that preserve test
correctness.

## Non-Goals

- No change to production code (only test infrastructure + build properties).
- No change to unit tests in `Module.UnitTests` / `Shared.UnitTests` (out of
  scope for this iteration; can be tackled in a follow-up plan).
- No migration to SQLite / slice tests (Tier 3, future plan).

## Current Architecture (analysis)

- **Test runtime:** xUnit v3 3.2.2, FluentAssertions 8.x, Moq 4.20, coverlet 10
  (always-on via `Directory.Build.props`).
- **One** integration collection: `[Collection("ApiIntegration")]` →
  `ApiIntegrationCollection : ICollectionFixture<ApiFixture>`.
- `ApiFixture` spins up `PostgreSqlBuilder("pgvector/pgvector:pg17")` in
  `InitializeAsync`, runs `dbContext.Database.MigrateAsync()`, then iterates
  **all** `IDataSeeder`s.
- `ApiIntegrationTestBase.InitializeAsync` calls `ResetDatabaseAsync()` before
  every test class — Respawn truncates all 4 schemas, then re-runs all
  seeders.
- `ApiFactory : WebApplicationFactory<Program>` boots the full ASP.NET host
  (Carter, EF, JWT, OpenAPI, Scalar, governance, performance, security,
  Hangfire, observability).
- `AuthTokenHelper.GenerateAdminToken()` and
  `IdentityTestHelper.GenerateUserToken()` regenerate JWTs on every call.

## Phase 1 — Tier 1 quick wins (low risk, ~30-50% reduction)

### 1.1 Make coverlet opt-in
File: `Directory.Build.props` (lines 94-95)

- Change `<CollectCoverage>true</CollectCoverage>` →
  `<CollectCoverage>false</CollectCoverage>`.
- Document in test README that CI must pass
  `--property:CollectCoverage=true --property:CoverletOutput=...` to enable.

### 1.2 Cache admin JWT
File: `service/Api/tests/Api.Tests/Infrastructure/Auth/AuthTokenHelper.cs`

- Wrap `GenerateAdminToken()` body in a `Lazy<string>` (or
  `Volatile.Read`-backed static field).
- Same secret/issuer/audience for the whole run → one token is fine.

### 1.3 Cache user JWT by (userId, email)
File: `service/Api/tests/Api.Tests/Scenarios/Identity/Helpers/IdentityTestHelper.cs`

- Add `ConcurrentDictionary<(Guid,string), string>` cache.
- `GenerateUserToken(userId, email)` returns cached value when present.

### 1.4 Lighten test host
File: `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs`

- Already removes `IHostedService` — keep.
- Add `services.RemoveAll<EndpointDataSource>()`-friendly config OR
  explicitly remove the OpenAPI endpoint convention.
- Disable Scalar UI by stripping its services:
  `services.RemoveAll<IScalarConfiguration>();`
- Drop `app.MapDefaultEndpoints()` calls — already not in factory; verify.

### 1.5 Skip migration if no new migrations
File: `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs:43-47`

- Before calling `MigrateAsync()`, query
  `SELECT COUNT(*) FROM "__EFMigrationsHistory"`.
- Cache last-applied count in a static `ConcurrentDictionary<string, int>`
  keyed by the EF assembly version (e.g.,
  `typeof(ApplicationDbContext).Assembly.GetName().Version!.ToString()`).
- Skip migration when count matches the model's expected migration list size.

### 1.6 xunit.runner.json: aggressive parallel algorithm
File: `service/Api/tests/Api.Tests/xunit.runner.json`

- xUnit v3 supports `parallelAlgorithm` values: `Default`, `Aggressive`.
- Set to `Aggressive` for in-test parallelism (assumes each `[Fact]` is
  independent within a class — currently true).

## Phase 2 — Tier 2 architectural changes (medium risk, ~2-3x additional)

### 2.1 Split into 4 module collections
Files:
- `service/Api/tests/Api.Tests/Infrastructure/ApiCollection.cs` — add
  `CatalogIntegrationCollection`, `IdentityIntegrationCollection`,
  `LocationIntegrationCollection`, `ProfileIntegrationCollection`, all bound
  to the same `ApiFixture` type.
- All 116 `*.IntegrationTests.cs` files — change
  `[Collection("ApiIntegration")]` →
  `[Collection("Catalog")]` / `["Identity"]` / `["Location"]` / `["Profile"]`
  (or stay on `ApiIntegration` for cross-module tests).
- The module assignments follow the `Scenarios/{Module}/...` folder structure.

### 2.2 Schema-scoped reset
File: `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs`

- Replace the single `_respawner` with
  `ConcurrentDictionary<string, Respawner>` keyed by schema name.
- Add `ResetSchemasAsync(params string[] schemas)`.
- Keep `ResetDatabaseAsync()` as a thin wrapper that passes all 4 schemas
  (backward-compat for `ApiIntegrationTestBase`).

### 2.3 ModuleIntegrationTestBase
New file:
`service/Api/tests/Api.Tests/Infrastructure/ModuleIntegrationTestBase.cs`

```csharp
public abstract class ModuleIntegrationTestBase(ApiFixture fixture)
    : ApiIntegrationTestBase(fixture)
{
    protected abstract string[] Schemas { get; }
    public new async ValueTask InitializeAsync()
        => await fixture.ResetSchemasAsync(Schemas);
}
```

Each module test class overrides `Schemas` to declare which schemas to reset.

### 2.4 Group IDataSeeders by schema
File: `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs:81-89`

- Replace `RunSeedersAsync(IServiceScope scope)` with
  `RunSeedersAsync(IServiceScope scope, IEnumerable<string> schemas)`.
- Derive schema from seeder's namespace:
  - `Shared.Security.Identity.Seeders.*` → `Identity`
  - `Module.Catalog.Persistence.Seeders.*` → `Catalog`
  - `Module.Location.Persistence.Seeders.*` → `Location`
  - `Module.Profile.Persistence.Seeders.*` → `Profile`
- Cache the group-by in a `Lazy<Dictionary<string, IDataSeeder[]>>` resolved
  once per process.

### 2.5 Podman container reuse for dev
File: `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs:34-35`

- Guard `WithReuse(true)` with
  `Environment.GetEnvironmentVariable("TESTCONTAINERS_REUSE_ENABLE") == "true"`.
- Document in test README:
  - `export TESTCONTAINERS_REUSE_ENABLE=true` before running tests locally.
  - Requires Podman in rootless mode with `--userns=keep-id` (or rootful).
  - CI does **not** set this env var → fresh container per CI run.

### 2.6 Parallel migration in DatabaseInitializer
File:
`service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs:68-94`

- Replace `foreach` over `IEnumerable<IApplicationDbContext>` with
  `Task.WhenAll` (one task per context).
- Each context migration is independent (different schemas).

### 2.7 Per-test-class LogLevel filter
File: `service/Api/tests/Api.Tests/appsettings.Testing.json`

- Already sets `Microsoft.EntityFrameworkCore` to `Warning` — keep.
- Add `Microsoft.AspNetCore.HttpsPolicy` → `None` (it's noisy in test host).
- Add `Microsoft.AspNetCore.Routing` → `Warning` (skip info-level messages).

## Phase 3 — Verification

1. **Baseline** — record 3 cold runs of:
   ```bash
   dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj \
     -c Release --no-build
   ```
2. **Per-phase** — apply Phase 1, then Phase 2; measure after each.
3. **Correctness** — run tests 3x after each phase; watch for:
   - Flakiness from collection parallelism (likely on shared-seed tests).
   - Respawn per-schema FK violations (mitigation: existing
     `TablesToIgnore` includes `__EFMigrationsHistory`).
4. **Podman sanity** — verify reuse works with
   `TESTCONTAINERS_REUSE_ENABLE=true`.

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Schema isolation: Catalog tests assume seeded Identity users | Identity is seeded once at fixture init; per-test-class reset of `Identity` schema also re-runs `RoleSeeder` and `UserSeeder` (they're no-op when `HasDataAsync` returns true). |
| Respawn per-schema breaks FKs | `RespawnerOptions.TablesToIgnore = ["__EFMigrationsHistory"]` already set; FKs are dropped & recreated by Respawn — verified safe. |
| Testcontainer reuse in Podman rootless | Document `--userns=keep-id`; on failure, log warning and fall back to fresh container (don't throw). |
| Coverlet off breaks CI coverage | CI script in `.github/workflows/*.yml` (or equivalent) passes `--property:CollectCoverage=true --property:CoverletOutput=...`. |
| Splitting 116 files | Use a global regex `sed`-style replace per module: `[Collection("ApiIntegration")]` → `[Collection("<Module>")]`. |

## Estimated effort

- Phase 1: ~1 hour
- Phase 2: ~3-4 hours (incl. 116-file collection re-tagging)
- Phase 3: ~1 hour
- **Total: ~5-6 hours**, target **3-5x speedup**.

## Files touched

### Modified
- `Directory.Build.props` — coverlet opt-in
- `service/Api/tests/Api.Tests/Infrastructure/Auth/AuthTokenHelper.cs` — JWT cache
- `service/Api/tests/Api.Tests/Scenarios/Identity/Helpers/IdentityTestHelper.cs` — user JWT cache
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs` — light host
- `service/Api/tests/Api.Tests/Infrastructure/ApiFixture.cs` — schema-scoped reset, group seeders, podman reuse
- `service/Api/tests/Api.Tests/Infrastructure/ApiCollection.cs` — 4 module collections
- `service/Api/tests/Api.Tests/xunit.runner.json` — aggressive parallelism
- `service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs` — parallel migration
- `service/Api/tests/Api.Tests/appsettings.Testing.json` — log filters
- 116 × `service/Api/tests/Api.Tests/Scenarios/**/*.IntegrationTests.cs` — collection re-tag

### Added
- `service/Api/tests/Api.Tests/Infrastructure/ModuleIntegrationTestBase.cs` — schema-scoped base class
