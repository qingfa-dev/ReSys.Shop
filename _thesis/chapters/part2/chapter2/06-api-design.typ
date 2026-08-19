== API DESIGN

#import "../../../template/ctu-styles.typ": figure-placeholder

The system exposes a comprehensive RESTful API built on *ASP.NET Core Minimal APIs* and *Carter*. This approach reduces the boilerplate associated with traditional MVC controllers while maintaining full support for dependency injection, authorization, and OpenAPI specification generation.

=== Architecture

The API layer acts as a thin orchestration boundary. It does not contain business logic; instead, it delegates all processing to the *MediatR* pipeline described in the Application Core.

- *Routing:* Implemented via `ICarterModule` to group related endpoints (e.g., `ProductsModule`).
- *Serialization:* Uses `System.Text.Json` with snake_case naming policies to align with standard web practices.
- *Documentation:* Automatically generated OpenAPI v3 (Swagger) definitions.

=== Unified Response Structure

To ensure consistent consumption by the frontend (Vue 3) and mobile clients, all API responses adhere to a unified envelope structure or the *Problem Details for HTTP APIs* (RFC 7807) standard for errors.

The `ApiResponse<T>` wrapper enables consistent handling of metadata, such as pagination.

```json
{
  "data": {
    "items": [ ... ],
    "totalCount": 150
  },
  "success": true,
  "message": "Request processed successfully."
}
```

=== Implementation Pattern

Each feature exposes its own module. Below is the implementation of the *Product Management* API, demonstrating the rigorous security checks and command dispatching pattern.

```csharp
public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/catalog/products")
            .WithTags("Products")
            .RequireAuthorization();

        // 1. Query Handling (Read)
        group.MapGet("/",
            async ([AsParameters] GetProductsPagedList.Request request, ISender sender) =>
            {
                var result = await sender.Send(new GetProductsPagedList.Query(request));
                return Results.Ok(ApiResponse.Paginated(result));
            })
            .RequireAccessPermission(FeaturePermissions.Admin.Catalog.Product.List);

        // 2. Command Handling (Write)
        group.MapPost("/",
            async ([FromBody] CreateProduct.Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateProduct.Command(request));
                return result.ToApiCreatedResponse(x => $"/api/admin/catalog/products/{x.Id}");
            })
            .RequireAccessPermission(FeaturePermissions.Admin.Catalog.Product.Create);

        // 3. State Management (Patch)
        group.MapPatch("/{id:guid}/activate",
            async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new ActivateProduct.Command(id));
                return result.ToApiResponse();
            })
            .RequireAccessPermission(FeaturePermissions.Admin.Catalog.Product.ManageStatus);
    }
}
```

=== Security & Authorization
The API enforces a *Zero Trust* security model at the endpoint level.

1. *Authentication:* All non-public routes require a valid JWT Bearer token via `.RequireAuthorization()`.
2. *Fine-Grained Permissions:* The custom extension `.RequireAccessPermission(...)` validates that the user holds specific claims (e.g., `catalog.product.create`) before the handler is even invoked. This prevents unauthorized execution of expensive business logic.

=== Error Handling
Failures are converted from the Domain's `ErrorOr` result pattern into standardized HTTP responses.

- *Domain Validation Error:* Returns `400 Bad Request` with field-level details.
- *Result.NotFound:* Returns `404 Not Found`.
- *Result.Unauthorized:* Returns `403 Forbidden`.
- *WaitMsBeforeAsync:* Configured for background tasks to prevent client timeouts.

