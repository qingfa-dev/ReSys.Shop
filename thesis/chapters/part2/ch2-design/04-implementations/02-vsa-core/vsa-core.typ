=== Vertical Slice Architecture Core

The .NET backend implements Vertical Slice Architecture (VSA) combined with Command Query Responsibility Segregation (CQRS), organizing code by business capability rather than technical layer. Each feature co-locates its request DTOs, domain logic, validation, Carter endpoint, and response models within a single directory using `static partial class` definitions.

==== Feature Co-Location and Execution Mechanics

Every feature is structured across five files: #raw("{Feature}.cs", lang: "csharp"), #raw("{Feature}.Request.cs", lang: "csharp"), #raw("{Feature}.Response.cs", lang: "csharp"), #raw("{Feature}.Endpoint.cs", lang: "csharp"), and #raw("{Feature}.Validator.cs", lang: "csharp"). The handler pipeline (Validate → Build → Complete):

#figure(
  table(
    columns: (auto, 3fr),
    stroke: 0.5pt,
    align: left + horizon,
    inset: 6pt,
    table.header([*Step*], [*Action*]),
    [1. Validate], [Checks slug uniqueness; returns `DuplicateSlug` on collision.],
    [2. Build], [Constructs `Product` via domain factory, persists to database, dispatches cross-module `AddVariant` via `ISender`.],
    [3. Complete], [Assigns master variant ID, returns `Result<Response>.Created(...)`.],
  ),
  kind: table,
  caption: [Sequential execution flow within the CreateProduct command handler.],
) <tbl-handler-flow>

// [SCREENSHOT: implementation-vsa-feature-directory.png] IDE Solution Explorer showing the CreateProduct feature directory with five co-located files highlighted, illustrating the vertical slice file organization within the Catalog module.

Communication between modules uses only `ISender.Send()` messages. Bounded contexts never import another context's namespace. This keeps the modules isolated within a single assembly, checked using static analysis and build rules.

```cs
public sealed record Command(Request Request) : ICommand<Response>;

public sealed class CommandHandler(IApplicationDbContext db, ISender sender)
    : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
    {
        if (await db.Set<Product>().AnyAsync(x => x.Slug == c.Request.Slug, ct))
            return ProductResult.Errors.DuplicateSlug;

        var product = c.Request.MapToDomain().Value;
        db.Set<Product>().Add(product);
        await db.SaveChangesAsync(ct);

        var v = await sender.Send(new AddVariant.Command(product.Id, vr), ct);
        product.MasterVariantId = v.Value.Id;
        await db.SaveChangesAsync(ct);

        return Result<Response>.Created(product.MapToDetail<Response>(),
            ProductResult.Success.Created(product.Id));
    }
}
```

==== Request Pipeline Architecture

Each feature implements `ICarterModule` with thin endpoints that parse requests, construct MediatR commands, dispatch via `ISender`, and convert `Result<T>` to HTTP responses. Routes follow #raw("/api/{surface}/{module}/{resource}", lang: "http") with Carter auto-discovery at startup.

Three MediatR pipeline behaviors wrap every command: `LoggingBehavior` (outermost, captures request metadata and duration), `ValidationBehavior` (runs FluentValidation, short-circuits on failure), and `ExceptionMappingBehavior` (innermost, wraps unhandled exceptions).

// [SCREENSHOT: implementation-pipeline-debugging.png] Debugger call stack showing three nested MediatR behavior frames (Logging → Validation → ExceptionMapping) wrapping the handler, with breakpoint set in the handler's first line.

==== Functional Result Pattern

The application uses `Result<T>` (`readonly record struct`) instead of exception-driven control flow. Domain methods return explicitly typed results; `Error` structs carry machine-readable codes, human-readable messages, and optional metadata. The `ToResult()` extension maps domain results to HTTP status codes: 200 OK or 201 Created for success, 400 for validation failures, 404 for missing entities, and 409 for state conflicts.
