== MediatR Pipeline Registration

The MediatR pipeline is the system's cross-cutting concern backbone. Behaviors are registered in strict order (outermost first) in `Mediator.Extension.cs`:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionMappingBehavior<,>));
```

*Request lifecycle*:
```
HTTP Request
  → Carter Endpoint → sender.Send(new Command(request))
    → LoggingBehavior (log entry with CorrelationId)
      → ValidationBehavior (FluentValidation; short-circuit → Result.Validation)
        → ExceptionMappingBehavior (try/catch → Result.Unexpected)
          → Command/Query Handler
            → Domain logic → EF Core → Mapster → Response DTO
  → result.ToResult() → IResult with status code + JSON envelope
```

*Design rationale*: Validation is placed *inside* logging so that validation failures are still logged. Exception mapping is innermost to catch anything that leaks past validation.

*Evidence*: `Shared/Application/Mediators/Mediator.Extension.cs:46-50`, `Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:1-67`, `Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:1-42`

== Vertical Slice Anatomy

Every feature action is a `static partial class` split across files in `Features/{Admin|Storefront}/{Feature}/{Action}/`:

```
Module/Catalog/Features/Admin/Products/Create/
├── CreateProduct.cs            # Command + Handler (business logic)
├── CreateProduct.Endpoint.cs   # ICarterModule (route, auth, response types)
├── CreateProduct.Request.cs    # Request DTO
├── CreateProduct.Response.cs   # Response DTO
└── CreateProduct.Validator.cs  # FluentValidation rules
```

*Endpoint convention*:

```csharp
public static partial class CreateProduct
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Create.Route, ...)
               .HasPermission(CatalogPermission.AdminProductsCreate)
               .WithTags(CatalogFeature.Admin.Products.Create.Tags)
               .WithSummary(CatalogFeature.Admin.Products.Create.Summary)
               .Produces<Response>(StatusCodes.Status201Created)
               .ProducesValidationProblem();
        }
    }
}
```

Read-only queries may omit Request/Validator files. The `static partial class` pattern ensures all files share the same type while remaining individually navigable.

*Evidence*: `Module/Catalog/Features/Admin/Products/Create/CreateProduct.Endpoint.cs:14-32`, `Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78`

== Result Type Factory Methods

All domain operations return `Result<T>` or `Result`. Factory methods in `Result.Method.cs` provide semantic error creation:

```csharp
Result.NotFound("Product not found")
Result.Conflict("A product with this slug already exists")
Result.Validation(new Error("Slug.Unique", "Slug must be unique"))
Result.Unexpected("An unexpected error occurred")
```

The `Error` type carries a `Code`, `Message`, `Type`, and optional `Metadata` dictionary. The unified JSON envelope returned to clients:

```json
{
  "isSuccess": false,
  "statusCode": 400,
  "errors": [{ "code": "Slug.Unique", "message": "A product with this slug already exists." }],
  "message": "One or more validation errors occurred.",
  "metadata": null
}
```

*Evidence*: `Shared/Application/Models/Results/Result.cs:1-43`, `Shared/Application/Models/Results/Result.Method.cs:84-152`

== Python Sidecar: Model Registry

The embedding sidecar uses a Strategy pattern with runtime model selection via environment variable:

```python
# From embedding_service.py
MODEL_REGISTRY = {
    "fashion-clip": FashionCLIPModel,
    "resnet50": ResNet50Model,
    "efficientnet_b0": EfficientNetB0Model,
    "clip": CLIPGenericModel,
}

def get_model() -> BaseEmbeddingModel:
    model_name = os.getenv("EMBEDDING_MODEL", "fashion-clip")
    return MODEL_REGISTRY[model_name]()
```

Each model implements `BaseEmbeddingModel` with `encode_image(image: PIL.Image) → np.ndarray`. Vector dimensions vary per model (512, 2048, 1280). The sidecar exposes `POST /embeddings` with a `model` query parameter that overrides the env var, enabling per-request model switching during evaluation.

*Evidence*: `service/Embedding/src/services/embedding_service.py`, `service/Embedding/src/models/base_model.py`

== pgvector Multi-Model Configuration

A single `vector(2048)` column accommodates all models. Smaller vectors are right-padded with zeros (standard pgvector behavior):

```csharp
// From Vector.Configuration.cs
builder.Entity<VariantImage>()
    .Property(v => v.Embedding)
    .HasColumnType("vector(2048)");

builder.Entity<VariantImage>()
    .Property(v => v.ModelName)
    .HasMaxLength(50)
    .HasDefaultValue("fashion-clip");

builder.Entity<VariantImage>()
    .Property(v => v.VectorDim)
    .HasDefaultValue(512);
```

*Per-model querying*:

```sql
SELECT vi.*, v.sku, p.name
FROM catalog.variant_images vi
JOIN catalog.variants v ON vi.variant_id = v.id
JOIN catalog.products p ON v.product_id = p.id
WHERE vi.model_name = 'fashion-clip'
ORDER BY vi.embedding <=> @query_embedding  -- cosine distance
LIMIT 20;
```

During evaluation, separate IVF flat indexes are created per `model_name` to prevent cross-model interference:

```sql
CREATE INDEX idx_embedding_fashion_clip ON catalog.variant_images
USING ivfflat (embedding vector_cosine_ops)
WHERE model_name = 'fashion-clip';
```

*Evidence*: `Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs`, `Module/Catalog/Domain/Products/Variants/Images/VariantImage.cs`

== EF Core Interceptors

Three interceptors provide automatic cross-cutting behavior on `SaveChanges`:

#figure(
  table(
    columns: (auto, auto, auto),
    align: (start, start, start),
    [*Interceptor*], [*Interface*], [*Behavior*],
    [`AuditableInterceptor`], [`IAuditable`], [Sets `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` timestamps and user IDs],
    [`SoftDeletableInterceptor`], [`ISoftDeletable`], [Converts `DELETE` to `UPDATE SET DeletedAt = now()`; filters soft-deleted rows from queries],
    [`VersionableInterceptor`], [`IVersionable`], [Checks optimistic concurrency via `RowVersion` column on update],
  ),
  caption: [EF Core interceptors for cross-cutting domain concerns],
)

All business entities implement `IAuditable` and `ISoftDeletable`. The interceptors are registered globally in `ApplicationDbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    // Interceptors registered in Di registration, not here
}
```

*Evidence*: `Shared/Operational/Persistence/Interceptors/Auditable.Interceptor.cs`, `Shared/Application/Domain/Concerns/Auditable/IAuditable.cs`, `Shared/Application/Domain/Concerns/SoftDeletable/ISoftDeletable.cs`

== Configuration Hierarchy

The application uses a layered configuration model per environment:

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: (start, start, start, start),
    [*Source*], [*Development*], [*Testing*], [*Production*],
    [`appsettings.json`], [Base], [Base], [Base],
    [`appsettings.Development.json`], [Override], [Ignored], [Ignored],
    [`appsettings.Testing.json`], [Ignored], [Override], [Ignored],
    [`dotnet user-secrets`], [Secrets], [Ignored], [Ignored],
    [Environment variables], [Optional], [Injected by test factory], [Primary secret source],
  ),
  caption: [Configuration source precedence per environment],
)

*Dev secret handling*: Dev JWT secrets live in `dotnet user-secrets` (id `resys.shop.api`). `JwtSettingsValidator` rejects known-dev literal secrets in non-Development environments. `setup-dev-secrets.sh` bootstraps dev secrets safely.

*Evidence*: `service/Api/src/Api/appsettings.json`, `service/Api/src/Api/appsettings.Development.json`, `service/Api/src/Api/.env.template:1-33`

== Permission-Based Authorization

The authorization model uses a custom `IAuthorizationPolicyProvider` rather than simple RBAC:

1. Permission descriptors defined in `PermissionContext` as `{Domain}:{Category}:{Action}`
2. Feature metadata types (e.g., `CatalogFeature.Admin.Products.Create`) bundle route, tags, summary, and permission descriptor
3. Endpoint registration calls `.HasPermission(CatalogPermission.AdminProductsCreate)` which creates policy string `catalog:products:create`
4. `PermissionPolicyProvider` resolves the string to a `PermissionRequirement`
5. Handler checks if the user's claims contain the required permission

```csharp
// PermissionContext.cs — permission registry
public static class CatalogPermission
{
    public static readonly PermissionContext AdminProductsCreate = new("catalog", "products", "create");
    public static readonly PermissionContext AdminProductsRead = new("catalog", "products", "read");
    // ...
}
```

*Design rationale*: RBAC is too coarse for e-commerce (an admin may edit products but not process refunds). Permission-based authorization gives granular control while keeping the policy provider centralized.

*Evidence*: `Shared/Security/Authorization/Registry/PermissionContext.cs:1-60`, `Shared/Security/Authorization/Policies/Permission.PolicyProvider.cs:1-31`

== Aspire Service Defaults

All Aspire-managed services share `ReSys.ServiceDefaults` which registers:

- OpenTelemetry (traces, metrics, logs)
- Health checks (`/health`, `/alive`)
- Service discovery
- HTTP client resilience (Polly pipeline)

```csharp
// Extensions.cs — service defaults registration
public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
{
    builder.AddOpenTelemetry();
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString)
        .AddRedis(redisConnection);
    builder.Services.AddServiceDiscovery();
    builder.Services.AddHttpClient().AddStandardResilienceHandler();
}
```

Health check endpoints (`/alive`, `/health`) are only exposed in non-production environments via `MapDefaultEndpoints`. In production, health checks should be on a separate port or sidecar.

*Evidence*: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:19-132`

== Security Headers Middleware

A custom middleware injects security headers on every response:

#figure(
  table(
    columns: (auto, auto, auto),
    align: (start, start, start),
    [*Header*], [*Value*], [*Purpose*],
    [`X-Content-Type-Options`], [`nosniff`], [Prevent MIME-type sniffing],
    [`X-Frame-Options`], [`DENY`], [Clickjacking protection],
    [`Content-Security-Policy`], [`default-src 'self'`], [XSS mitigation],
    [`Strict-Transport-Security`], [`max-age=31536000`], [HSTS],
    [`Referrer-Policy`], [`strict-origin-when-cross-origin`], [Privacy],
  ),
  caption: [Security response headers injected by middleware],
)

*Evidence*: `Shared/Security/Headers/SecurityHeadersMiddleware.cs`

== Rate Limiting Policies

Named rate-limit policies protect specific endpoint categories:

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: (start, start, start, start),
    [*Policy*], [*Permit Limit*], [*Window*], [*Protected Endpoints*],
    [`default`], [100], [60s], [All unclassified],
    [`auth`], [5], [60s], [Login, token refresh],
    [`register`], [3], [3600s], [Registration],
    [`forgot-password`], [3], [3600s], [Password reset],
    [`payment`], [30], [60s], [Payment intent creation],
  ),
  caption: [Rate limiting policies by endpoint category],
)

The `payment` policy is higher (30/min) than auth because legitimate checkout flows may involve multiple payment attempts.

*Evidence*: `appsettings.json:79-86`, `Shared/Security/RateLimiting/RateLimit.Extensions.cs`

== File Upload Security

Multi-layered defense for uploaded files:

#figure(
  table(
    columns: (auto, auto, auto),
    align: (start, start, start),
    [*Layer*], [*Check*], [*Implementation*],
    [1. Magic bytes], [File header matches extension], [`IStorageSecurityEnforcer`],
    [2. Extension allowlist], [Only `.jpg`, `.png`, `.webp`, `.pdf`, etc.], [`appsettings.json:135-138`],
    [3. Extension blocklist], [`.exe`, `.bat`, `.ps1`, `.jar` blocked], [`appsettings.json:139-142`],
    [4. Size limit], [Max 10 MB], [`appsettings.json:134`],
    [5. Anti-forgery guard], [Rate-limit consecutive failures], [`appsettings.json:146-149`],
    [6. Malware scan], [ClamAV TCP scan (opt-in, disabled by default)], [`appsettings.json:150-155`],
  ),
  caption: [Multi-layered file upload security controls],
)

*Evidence*: `Shared/Operational/Storages/Storage.Extensions.cs:35-74`, `appsettings.json:129-155`

== Test Infrastructure

*Unit tests*: `ApplicationDbContext` uses `UseInMemoryDatabase(Guid.NewGuid().ToString())` per test class. `Mock<ISender>` for nested command dispatch. `AdditionalConfigurationsAssemblies` must be set *before* first use to load module entity configs.

*Integration tests*: `ApiFactory : WebApplicationFactory<Program>` boots the real host with in-memory config overrides. `Testcontainers.PostgreSql` spins up a real PostgreSQL container. `Respawn` checkpoints database state between tests. `TestCurrentUser` uses `AsyncLocal<Guid?>` to simulate different users. All external integrations disabled (caching, background jobs, storage, malware scanner).

*Evidence*: `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189`, `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-92`

== Evidence

- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` — pipeline registration
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/` — vertical slice anatomy
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43`, `Result.Method.cs:1-191` — Result pattern
- `service/Embedding/src/services/embedding_service.py` — model registry
- `service/Embedding/src/models/base_model.py` — embedding model abstraction
- `service/Api/src/Shared/Operational/Persistence/Configurations/Vectors/Vector.Configuration.cs` — pgvector setup
- `service/Api/src/Shared/Operational/Persistence/Interceptors/` — EF Core interceptors
- `service/Api/src/Api/appsettings.json:1-237` — configuration hierarchy
- `service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs:1-60` — permission registry
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:1-132` — service defaults
- `service/Api/src/Shared/Security/Headers/SecurityHeadersMiddleware.cs` — security headers
- `service/Api/src/Shared/Security/RateLimiting/RateLimit.Extensions.cs` — rate limiting
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:35-74` — upload security
- `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:1-189` — test factory
