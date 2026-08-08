---
goal: Configure IVFFlat and HNSW pgvector indexes, create EF Core migration, and fix embedding service connection config
version: 1.0
date_created: 2026-07-29
last_updated: 2026-08-08
status: Planned
tags: feature, pgvector, index, migration, configuration, embedding, infrastructure
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The `catalog.product_image_embeddings` table stores `VECTOR(512)` embeddings but has no vector index, causing sequential scans on all similarity searches via `VectorSearchService.NpgsqlSearchAsync`. This plan adds reusable IVFFlat/HNSW index configuration methods to the Shared persistence layer, applies an IVFFlat index to the `ImageEmbedding.Vector` column, creates the initial EF Core migration, and fixes the embedding service (Python FastAPI sidecar) connection config so it works both inside and outside Aspire orchestration.

## 1. Requirements & Constraints

- **REQ-001**: `catalog.product_image_embeddings` must have a vector index for cosine-distance similarity search (matching the `<=>` operator in `VectorSearchService.NpgsqlSearchAsync` at `VectorSearchService.cs:51`)
- **REQ-002**: IVFFlat index must use `vector_cosine_ops` operator class — same operator class used in benchmark `benchmarks/infra/postgres/init.sql:38`
- **REQ-003**: HNSW index configuration method must be available for future production adoption (higher recall, larger index build cost)
- **REQ-004**: Index configuration methods must be reusable across any entity with a `Vector` property (generic, on `EntityTypeBuilder<T>`)
- **REQ-005**: Embedding service (Python FastAPI at `service/Embedding/`) must be reachable from the .NET API: resolved endpoint injected via the AppHost (`embedding.GetEndpoint("http")`) with an `appsettings` URL (`http://embedding:8000`) as fallback for non-Aspire environments
- **SEC-001**: Vector index must not expose sensitive data through index metadata (index is on a `VECTOR(512)` column containing embeddings, not raw data)
- **CON-001**: Shared layer must not depend on Module — index config methods go in `Shared/Operational/Persistence/Configurations/Vectors/` (already depends only on `Pgvector` NuGet)
- **CON-002**: Extension method pattern must match existing `VectorConfiguration.cs` conventions: `public static` methods on `EntityTypeBuilder<T>`
- **CON-003**: Npgsql.EntityFrameworkCore 10.0.2 + Pgvector.EntityFrameworkCore 0.3.0 API surface must be used (`HasMethod`, `HasOperators`, `HasStorageParameter` — all available)
- **GUD-001**: IVFFlat `lists` parameter = 100 for initial deployment (<100K rows expected); tune later as data grows
- **GUD-002**: HNSW `m` = 16, `ef_construction` = 200 per pgvector recommended defaults for production-quality recall
- **GUD-003**: Index name convention: `IX_{TableName}_{ColumnName}_{Method}` using `HasDatabaseName`
- **GUD-004**: Connection config for embedding service: the AppHost injects the resolved endpoint URL directly (`embedding.GetEndpoint("http")`) into the Api as `Http:Clients:Inference:BaseAddress` so no DNS/hostname translation is needed; default and dev fallback both use `http://embedding:8000`

## 2. Implementation Steps

### Implementation Phase 1: Add IVFFlat and HNSW Index Configuration Methods to Shared Layer

- **GOAL-001**: Add reusable `ConfigureIVFFlatIndex` and `ConfigureHNSWIndex` extension methods on `EntityTypeBuilder<T>` in `Vector.Configuration.cs`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `ConfigureIVFFlatIndex<T>` extension method on `EntityTypeBuilder<T>` — calls `HasIndex`, sets `HasMethod("ivfflat")`, `HasOperators("vector_cosine_ops")`, `HasStorageParameter("lists", lists)`, optional `HasDatabaseName` | | |
| TASK-002 | Add `ConfigureHNSWIndex<T>` extension method on `EntityTypeBuilder<T>` — calls `HasIndex`, sets `HasMethod("hnsw")`, `HasOperators("vector_cosine_ops")`, `HasStorageParameter("m", m)`, `HasStorageParameter("ef_construction", efConstruction)`, optional `HasDatabaseName` | | |
| TASK-003 | Add `using Microsoft.EntityFrameworkCore.Metadata.Builders;` to `Vector.Configuration.cs` (for `EntityTypeBuilder<T>` and `IndexBuilder<T>`) | | |

**Implementation Details:**

File: `service/Api/src/Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs`

Add after the `ConfigureProperty` method (before the closing braces of the class):

```csharp
public static IndexBuilder<T> ConfigureIVFFlatIndex<T>(
    this EntityTypeBuilder<T> builder,
    Expression<Func<T, Vector>> propertyExpression,
    int lists = 100,
    string? indexName = null) where T : class
{
    IndexBuilder<T> indexBuilder = builder.HasIndex(propertyExpression);
    indexBuilder.HasMethod("ivfflat");
    indexBuilder.HasOperators("vector_cosine_ops");
    indexBuilder.HasStorageParameter("lists", lists);
    if (indexName is not null)
        indexBuilder.HasDatabaseName(indexName);
    return indexBuilder;
}

public static IndexBuilder<T> ConfigureHNSWIndex<T>(
    this EntityTypeBuilder<T> builder,
    Expression<Func<T, Vector>> propertyExpression,
    int m = 16,
    int efConstruction = 200,
    string? indexName = null) where T : class
{
    IndexBuilder<T> indexBuilder = builder.HasIndex(propertyExpression);
    indexBuilder.HasMethod("hnsw");
    indexBuilder.HasOperators("vector_cosine_ops");
    indexBuilder.HasStorageParameter("m", m);
    indexBuilder.HasStorageParameter("ef_construction", efConstruction);
    if (indexName is not null)
        indexBuilder.HasDatabaseName(indexName);
    return indexBuilder;
}
```

Add the missing `using` at the top of the file:
```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
```

**Validation**: Build the project with `dotnet build service/Api/src/Api/Api.csproj` — must succeed with zero warnings.

---

### Implementation Phase 2: Apply IVFFlat Index to ImageEmbedding Entity

- **GOAL-002**: Wire `ConfigureIVFFlatIndex` into `ImageEmbeddingConfiguration` to produce a real IVFFlat index on `Vector`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Add `using Shared.Operational.Persistence.Configurations.Vectors;` to `ImageEmbeddingConfiguration.cs` | | |
| TASK-005 | Call `builder.ConfigureIVFFlatIndex(x => x.Vector, lists: 100, indexName: "ix_product_image_embeddings_vector_ivfflat")` after the `#endregion Properties` line (after line 29) | | |

**Implementation Details:**

File: `service/Api/src/Module/Catalog/Persistence/Configurations/Products/ImageEmbeddingConfiguration.cs`

Add after line 29 (`#endregion Properties`):

```csharp
builder.ConfigureIVFFlatIndex(
    x => x.Vector,
    lists: 100,
    indexName: "ix_product_image_embeddings_vector_ivfflat");
```

Add after the existing `using` directives (after line 3):

```csharp
using Shared.Operational.Persistence.Configurations.Vectors;
```

The `HasOperators("vector_cosine_ops")` call ensures the generated SQL is:
```sql
CREATE INDEX ix_product_image_embeddings_vector_ivfflat
    ON catalog.product_image_embeddings
    USING ivfflat (vector vector_cosine_ops)
    WITH (lists = 100);
```

**Validation**:
- `dotnet build service/Api/src/Api/Api.csproj` — must succeed with zero warnings
- `bash scripts/check-cross-module-refs.sh` — must not introduce new cross-module violations (Shared namespace import in Module is allowed: Module depends on Shared)

---

### Implementation Phase 3: Create Initial EF Core Migration

- **GOAL-003**: Generate and verify the initial migration that creates the vector extension, the `product_image_embeddings` table, and the IVFFlat index

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Run `dotnet ef migrations add InitialCreate` from repo root with correct project arguments | | |
| TASK-007 | Verify the generated migration `Migrations/Migrations/YYYYMMDDHHMMSS_InitialCreate.cs` contains: `HasPostgresExtension("vector")`, the IVFFlat index with `USING ivfflat`, and `vector_cosine_ops` | | |
| TASK-008 | Verify the migration SQL (`script-migration -p <file> -o <file>`) generates valid PostgreSQL: `CREATE EXTENSION IF NOT EXISTS vector`, `CREATE INDEX ... USING ivfflat (vector vector_cosine_ops) WITH (lists = 100)` | | |

**Implementation Details:**

Run from repo root:
```bash
dotnet ef migrations add InitialCreate \
    --project service/Api/src/Migrations/Api.Migrations.csproj \
    --startup-project service/Api/src/Api/Api.csproj
```

If PostgreSQL is not running locally, start it first:
```bash
# Using Aspire (pulls pgvector image automatically)
dotnet run --project infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj
```
Or via Docker directly:
```bash
docker run -d --name pgvector-test \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD=postgres \
    -e POSTGRES_DB=resys_shop \
    -p 5432:5432 \
    pgvector/pgvector:pg17-trixie
```

**Validation**:
- Migration file must be generated at `service/Api/src/Migrations/Migrations/<timestamp>_InitialCreate.cs`
- Search generated migration for `ivfflat`, `vector_cosine_ops`, and `lists = 100`

---

### Implementation Phase 4: Correct Embedding Service Connection Configuration

- **GOAL-004**: Ensure the .NET API can reach the Python FastAPI embedding service. In Aspire, the endpoint is injected directly into the Api so it does not depend on service-discovery hostname translation (`embedding` is not a resolvable DNS name); for non-Aspire standalone dev, an explicit `appsettings` URL is used as fallback.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Add `Http:Clients:Inference` configuration section to `appsettings.Development.json` with `BaseAddress: "http://embedding:8000"` (standalone fallback; under Aspire the env-injected value from the AppHost always wins) | | |
| TASK-010 | Verify that `InferenceClientDependencyInjection.AddInferenceClient` correctly reads the config section (it already does via `InferenceClientSetting.SectionName = "Http:Clients:Inference"`) and falls back to default `"http://embedding:8000"` if section is absent | | |
| TASK-011 | In `infra/Aspire/src/ReSys.AppHost/AppHost.cs`, inject the resolved embedding endpoint into the Api: `.WithEnvironment("Http__Clients__Inference__BaseAddress", embedding.GetEndpoint("http"))`. DCP resolves the runtime-reachable URL (e.g. `http://localhost:8000`), so no gateway or service-discovery host rewrite is required. | | |

**Implementation Details:**

File: `service/Api/src/Api/appsettings.Development.json`

```json
  "Http": {
    "Clients": {
      "Inference": {
        "BaseAddress": "http://embedding:8000",
        "TimeoutSeconds": 60,
        "DefaultHeaders": {}
      }
    }
  },
```

File: `infra/Aspire/src/ReSys.AppHost/AppHost.cs`

```csharp
IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis)
    .WithReference(embedding)
    .WithEnvironment("Http__Clients__Inference__BaseAddress", embedding.GetEndpoint("http"))
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithOtlpExporter();
```

The `WithEnvironment(name, EndpointReference)` overload (Aspire.Hosting 13.4.6) materializes the endpoint's URL at runtime, overriding the appsettings value via the standard env-var config source. This avoids the earlier failure mode where the client configured `BaseAddress: "http://embedding"` fell through to port 80 and then to an unresolvable hostname because the reactive `.ResolvingHttpDelegatingHandler` had no `Services__Embedding__http__0` entry to rewrite.

**Validation**:
- `dotnet build infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj` — must succeed with zero warnings
- `dotnet build service/Api/src/Api/Api.csproj` — must succeed with zero warnings
- The JSON must be valid (no trailing commas, proper nesting)

## 3. Alternatives

- **ALT-001**: Configure the index directly in migration `Up()` method as raw SQL (`migrationBuilder.Sql("CREATE INDEX ...")`). Rejected because it bypasses EF Core model snapshot, making the index invisible to `dotnet ef migrations list` and causing diff drift on subsequent migrations. The `HasMethod`/`HasOperators`/`HasStorageParameter` approach preserves full snapshot integration.
- **ALT-002**: Hardcode index config in each entity configuration instead of reusable extension methods. Rejected because it duplicates logic — every entity with a `Vector` property would re-implement the same `HasMethod`/`HasOperators` calls. The extension method on `EntityTypeBuilder<T>` is the correct abstraction per the project's convention in `Vector.Configuration.cs`.
- **ALT-003**: Use HNSW instead of IVFFlat as the initial index. Rejected because IVFFlat is faster to build and sufficient for <1M rows; HNSW can be adopted later via a migration that drops IVFFlat and creates HNSW. Both methods are provided for future use.
- **ALT-004**: Rely solely on Aspire service discovery without explicit `appsettings` fallback and without an AppHost-injected endpoint. Rejected because the runtime exception showed the `Services__Embedding__http__0` discovery config was not rewriting the client base, so the Api attempted DNS resolution on the literal hostname `embedding` and failed. The AppHost endpoint injection (TASK-011) removes the need for that translation.
- **ALT-005**: Use `http://localhost:8000` in `InferenceClientSetting.DefaultBaseAddress` and dev config. Rejected in favor of `http://embedding:8000`, which stays semantically correct for the Aspire resource name and makes the fallback match the sidecar's declared `targetPort: 8000` in `AppHost.cs:20`; Aspire overrides the actual reachable URL via the injected environment variable.

## 4. Dependencies

- **DEP-001**: PostgreSQL with pgvector extension running on `localhost:5432` (or accessible via connection string) — required for migration generation
- **DEP-002**: `dotnet-ef` CLI tool — required for `dotnet ef migrations add`. Install: `dotnet tool install --global dotnet-ef`
- **DEP-003**: Python FastAPI embedding service (listening on port 8000) reachable via the AppHost-injected endpoint, or standalone at `localhost:8000` — required for end-to-end verification of the inference client connection

## 5. Files

| File | Change Type | Description |
|------|-------------|-------------|
| FILE-001 | `service/Api/src/Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs` | Modify — add `ConfigureIVFFlatIndex<T>` and `ConfigureHNSWIndex<T>` extension methods; add `using Microsoft.EntityFrameworkCore.Metadata.Builders` and `using System.Linq.Expressions` |
| FILE-002 | `service/Api/src/Module/Catalog/Persistence/Configurations/Products/ImageEmbeddingConfiguration.cs` | Modify — add `using Shared.Operational.Persistence.Configurations.Vectors`; call `builder.ConfigureIVFFlatIndex(x => x.Vector, ...)` |
| FILE-003 | `service/Api/src/Migrations/Migrations/<timestamp>_InitialCreate.cs` | Create — auto-generated EF Core migration with `HasPostgresExtension("vector")` and IVFFlat index |
| FILE-004 | `service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs` | Create — auto-generated EF Core model snapshot reflecting the new index |
| FILE-005 | `service/Api/src/Api/appsettings.Development.json` | Modify — add `Http.Clients.Inference` section with `BaseAddress: "http://embedding:8000"` |
| FILE-006 | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` | Modify — inject resolved embedding endpoint into Api via `.WithEnvironment("Http__Clients__Inference__BaseAddress", embedding.GetEndpoint("http"))` |
| FILE-007 | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/Options/ImageEmbedding.Inference.Options.cs` | Modify — default `BaseAddress` updated to `"http://embedding:8000"` |

## 6. Testing

| Test | Description | Type | Verification Command |
|------|-------------|------|---------------------|
| TEST-001 | Build succeeds with zero warnings after all changes | Build | `dotnet build service/Api/src/Api/Api.csproj` |
| TEST-002 | Cross-module references not violated | Script | `bash scripts/check-cross-module-refs.sh` |
| TEST-003 | Migration generates valid IVFFlat SQL | Manual | Inspect generated migration and/or `dotnet ef migrations script -i -o /tmp/migration.sql` and verify `USING ivfflat (vector vector_cosine_ops)` appears |
| TEST-004 | No existing tests regress | Test | `dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests` |
| TEST-005 | Config JSON is valid | Manual | `python -m json.tool service/Api/src/Api/appsettings.Development.json` or IDE validation |
| TEST-006 | Migration applied successfully against PostgreSQL | Integration | `dotnet ef database update --project service/Api/src/Migrations/Api.Migrations.csproj --startup-project service/Api/src/Api/Api.csproj` |

## 7. Risks & Assumptions

- **RISK-001**: Migration generation requires a running PostgreSQL instance. Mitigation: run Docker pgvector container or use Aspire orchestration to spin up the database.
- **RISK-002**: IVFFlat `lists = 100` may be insufficient for >100K embeddings, causing degraded recall. Mitigation: monitor recall metrics and increase `lists` in a follow-up migration (or migrate to HNSW via `ConfigureHNSWIndex`).
- **RISK-003**: The `HasStorageParameter` API in Npgsql.EntityFrameworkCore 10.0.2 may have a different signature or not support all parameter types. Mitigation: if `HasStorageParameter("lists", 100)` generates invalid SQL, fall back to raw SQL in the migration `Up()` method.
- **ASSUMPTION-001**: The `pgvector` extension is available on the target PostgreSQL instance (pgvector/pgvector:pg17-trixie image in Aspire, or pgvector extension installed manually).
- **ASSUMPTION-002**: The embedding service declares `targetPort: 8000` in `AppHost.cs:20`; under Aspire the AppHost-injected endpoint env var carries the reachable URL, so no DNS entry for the `embedding` hostname is needed. Standalone (non-Aspire) dev must ensure the `embedding` name resolves or run the API via the AppHost.
- **ASSUMPTION-003**: The index should be an IVFFlat index (not HNSW) for initial deployment because the table is expected to have <100K rows and IVFFlat provides adequate recall with faster build time.

## 8. Related Specifications / Further Reading

- [pgvector IVFFlat documentation](https://github.com/pgvector/pgvector#ivfflat)
- [pgvector HNSW documentation](https://github.com/pgvector/pgvector#hnsw)
- [Npgsql EF Core index method API](https://www.npgsql.org/efcore/misc/IndexMethod.html)
- `benchmarks/infra/postgres/init.sql` — existing IVFFlat index pattern (lines 33-42)
- `benchmarks/src/benchmark/retrieval/pgvector.py` — `build_index` method (lines 394-442) for IVFFlat creation reference
- `docs/codebase/ARCHITECTURE.md` — data flow for vector search pipeline
- `docs/codebase/CONCERNS.md` — known tech debt and risks
