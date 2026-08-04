=== Vertical Slice Architecture Core

The .NET backend implements Vertical Slice Architecture (VSA) combined with Command Query Responsibility Segregation (CQRS), organizing application code by business capability rather than technical layer. Each feature co-locates its request DTOs, domain execution logic, validation constraints, Carter HTTP endpoints, and response models within a single directory using C\# `static partial class` definitions.

==== Feature Co-Location and Execution Mechanics

Every feature is structured as a `static partial class` split across five dedicated files. A representative feature, `CreateProduct` within the Catalog context, illustrates this organizational model:

#figure(
  ```text
  Admin/Products/Create/
  ├── CreateProduct.cs            # Command and CommandHandler logic
  ├── CreateProduct.Request.cs    # Input Request DTO
  ├── CreateProduct.Response.cs   # Output Response DTO
  ├── CreateProduct.Endpoint.cs   # Carter Minimal API route mapping
  └── CreateProduct.Validator.cs  # FluentValidation rule definitions
  ```,
  caption: [Co-located file layout for a single vertical slice feature.],
) <fig-vsa-layout>

The feature handler executes through a standardized six-step pipeline using MediatR's `ISender` for cross-module dispatch:

#figure(
  table(
    columns: (auto, 1.2fr, 2.8fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,

    table.header([*Step*], [*Phase*], [*Action Description*]),

    [Step 1], [Validation], [Checks product slug uniqueness against PostgreSQL; returns a `DuplicateSlug` domain error on collision.],
    [Step 2], [Instantiation], [Constructs the `Product` entity via a domain factory method returning `Result<Product>`.],
    [Step 3], [Persistence], [Appends the entity to `IApplicationDbContext` and saves changes to commit the transaction.],
    [Step 4], [Dispatch], [Issues a cross-module `AddVariant` command via `ISender` to initialize the master SKU.],
    [Step 5], [Association], [Assigns the generated master variant ID back to the `Product` aggregate and updates state.],
    [Step 6], [Completion], [Returns `Result<Response>.Created(...)` containing the mapped response payload.],
  ),
  kind: table,
  caption: [Sequential execution flow within the CreateProduct command handler.],
) <tbl-handler-flow>

*Compile-Time Module Isolation:* Inter-module communication relies exclusively on `ISender.Send()` messages. Bounded contexts never import foreign context namespaces, establishing strict boundaries within a single assembly enforced via static analysis and build policies.

```csharp
public sealed record Command(Request Request) : ICommand<Response>;

public sealed class CommandHandler(
    IApplicationDbContext dbContext, 
    ISender sender)
    : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(
        Command command, CancellationToken ct)
    {
        // Step 1: Validate slug uniqueness
        if (await dbContext.Set<Product>()
            .AnyAsync(x => x.Slug == command.Request.Slug, ct))
            return ProductResult.Errors.DuplicateSlug;

        // Step 2-3: Instantiate domain entity and persist
        var product = command.Request.MapToDomain().Value;
        dbContext.Set<Product>().Add(product);
        await dbContext.SaveChangesAsync(ct);

        // Step 4-5: Dispatch cross-module command for master variant
        var variantResult = await sender.Send(
            new AddVariant.Command(product.Id, variantRequest), ct);
        product.MasterVariantId = variantResult.Value.Id;
        await dbContext.SaveChangesAsync(ct);

        // Step 6: Return success response wrapper
        return Result<Response>.Created(
            product.MapToDetail<Response>(),
            ProductResult.Success.Created(product.Id));
    }
}
```
Each feature's endpoint participates in a shared request pipeline that applies cross-cutting concerns (validation, logging, and exception handling) uniformly across all features.

#pagebreak()

==== Request Pipeline Architecture
===== Carter Endpoint Registration

Each vertical slice implements `ICarterModule` to define its HTTP interface. Endpoints remain intentionally thin: parsing incoming requests, constructing MediatR command objects, dispatching via `ISender`, and converting functional `Result<T>` outputs into HTTP responses. All 257 API endpoints across the platform follow this pattern:

```csharp
public class Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(CatalogFeature.Admin.Products.Create.Route,
            async ([FromBody] Request request,
                   ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(request), ct);
            return result.ToResult();
        })
        .HasPermission(CatalogFeature.Admin.Products.Create.Permission)
        .Produces<Result<Response>>()
        .Produces<Result>(StatusCodes.Status400BadRequest);
    }
}
```

Routes follow a structural hierarchy: `/api/{module}/{surface}/{resource}` (where `surface` is designated as `admin` or `storefront`). Carter modules are auto-discovered at runtime via assembly scanning during application startup.

// [SCREENSHOT: vsa-feature-directory.png] Solution Explorer showing the CreateProduct feature directory with five co-located files.

===== MediatR Middleware Behaviors

Three cross-cutting behaviors wrap every command and query in a nested execution model. They are registered from outermost to innermost, forming a layered pipeline where each behavior delegates to the next:

```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionMappingBehavior<,>));
```

When a request is dispatched via `ISender`, it passes through each behavior in order:

- *LoggingBehavior* (outermost) captures the request type, then delegates inward.
- *ValidationBehavior* runs co-located FluentValidation rules. On failure, it returns `List<Error>` immediately, short-circuiting both the inner behavior and the handler.
- *ExceptionMappingBehavior* (innermost) delegates to the handler. It catches any unhandled exception, wraps it in `Error.FromException()`, and returns a structured failure instead of letting the exception propagate.

On the return path, `LoggingBehavior` inspects the response and records success or failure. The final `Result<T>` propagates back through Carter to the HTTP client. The handler never references logging, validation, or exception handling; these concerns are applied transparently by the pipeline.

#figure(
  table(
    columns: (1.5fr, 1fr, 3.5fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,

    table.header([*Pipeline Behavior*], [*Execution Order*], [*Operational Responsibility*]),

    [`LoggingBehavior`], [Outermost], [Logs request type metadata, execution duration, and completion status upon pipeline exit.],
    [`ValidationBehavior`], [Middle], [Executes co-located FluentValidation rules. Rejects invalid requests early by returning a `List<Error>` payload without invoking downstream handlers.],
    [`ExceptionMappingBehavior`], [Innermost], [Catches unhandled execution exceptions, wraps them in standard `Error.FromException()` models, and prevents internal stack trace leakage.],
  ),
  kind: table,
  caption: [MediatR pipeline behavior execution sequence and responsibilities.],
) <tbl-pipeline-behaviors>

// [SCREENSHOT: mediatr-pipeline-debugging.png] Debugger call stack showing three nested behavior frames wrapping the handler.

==== Functional Result Pattern and Error Handling

The application avoids exception-driven control flow in favor of a functional `Result<T>` pattern. Domain methods return explicitly typed `Result<T>` (for operations returning values) or `Result` (for void operations). Both structures are implemented as memory-efficient `readonly record struct` types:

```csharp
public readonly partial record struct Result<T> : IResultRecord
{
    [AllowNull] public T Value { get => field!; init; }
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public List<Error> Errors { get; init; }
    public bool IsFailure => !IsSuccess;
}
```

- *Domain Error Abstraction:* The `Error` struct encapsulates a machine-readable `Code`, human-readable `Message`, integer type discriminator, and an optional metadata dictionary. Specialized factory helpers generate standard error categories (`Error.BadRequest`, `Error.NotFound`, `Error.Conflict`, `Error.Validation`).
- *Implicit Conversions:* Returning a domain object implicitly wraps it in a successful `Result<T>` instance. Returning an `Error` or `List<Error>` automatically casts into a failed `Result<T>`.
- *HTTP Mapping:* The `ToResult()` extension translates domain results directly to standard HTTP status codes: *200 OK* or *201 Created* for success paths, *400 Bad Request* for validation failures, *404 Not Found* for missing entities, and *409 Conflict* for state violations.