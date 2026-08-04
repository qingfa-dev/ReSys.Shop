== BACKEND TECHNOLOGY STACK

This section describes the technologies used to build the backend of ReSys.Shop: .NET 10, the Vertical Slice Architecture, and supporting libraries.

=== .NET 10 Overview

The backend is built with *.NET 10*, Microsoft's cross-platform framework for building web applications. .NET was chosen because:

- Widely used in enterprise applications
- Strong typing with C\# helps catch errors at compile time
- Good performance for web APIs
- Leverages established patterns and prior familiarity from coursework

.NET 10 includes several improvements relevant to this project:
- *Native AOT:* Ahead-of-time compilation for faster startup
- *Improved HTTP/3:* Better performance for API requests
- *Enhanced minimal APIs:* Simpler code for defining endpoints

=== Minimal APIs with Carter

Traditional ASP.NET uses "Controllers", which are classes that define API endpoints. .NET 6+ introduced *Minimal APIs*, which allow defining endpoints more concisely.

This project uses *Carter*, a library that organizes minimal APIs into modules:

```csharp
// A simplified example of how endpoints are defined
public class SearchModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/search/image", async (
            IFormFile image,
            ISender sender) =>
        {
            var command = new SearchByImageCommand(image);
            var result = await sender.Send(command);
            return result.ToApiResponse();
        });
    }
}
```

Carter helps organize related endpoints together and integrates well with dependency injection.

=== MediatR for Request Handling

*MediatR* is a library that implements the mediator pattern. Instead of endpoints calling services directly, they send "requests" through a central dispatcher.

Benefits of this approach:
- *Decoupling:* Endpoints do not know about implementations
- *Cross-cutting concerns:* Logging, validation, and caching can be applied uniformly
- *Testability:* Handlers can be tested in isolation

=== Vertical Slice Architecture

Rather than organizing code by technical layer (Controllers, Services, Repositories), *Vertical Slice Architecture* organizes by feature @code-maze2024.

==== Traditional Layered Architecture (Not Used)

```
src/
├── Controllers/
│   ├── SearchController.cs
│   ├── CartController.cs
│   └── OrderController.cs
├── Services/
│   ├── SearchService.cs
│   ├── CartService.cs
│   └── OrderService.cs
└── Repositories/
    ├── ProductRepository.cs
    └── OrderRepository.cs
```

Problem: A single feature like "Search" is spread across multiple folders.

==== Vertical Slice Architecture (Used)

```
src/Features/
├── Search/
│   ├── SearchByImage/
│   │   ├── SearchByImageCommand.cs
│   │   ├── SearchByImageHandler.cs
│   │   └── SearchByImageValidator.cs
│   └── SearchByKeyword/
│       └── ...
├── Cart/
│   ├── AddToCart/
│   └── ...
└── Orders/
    └── ...
```

Advantage: Everything related to "Search by Image" is in one folder. Changes to this feature only affect files in that folder.

==== Request Flow Architecture and Component Interaction

When a user uploads an image for search:

1. *Endpoint* (Carter module) receives the HTTP request
2. Creates a *Command* object containing the image data
3. Sends the command through *MediatR*
4. MediatR routes to the *Handler* for that command type
5. Handler executes business logic and returns a result
6. Endpoint converts result to HTTP response

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (center, left),
    [*Component*], [*Responsibility*],
    [Carter Module], [Define HTTP endpoints, parse requests],
    [Command/Query], [Data transfer object for the request],
    [Validator], [Check that the request is valid (FluentValidation)],
    [Handler], [Execute business logic, access database],
    [Entity], [Domain model representing business concepts],
  ),
  caption: [Components in a vertical slice]
)

=== Entity Framework Core

For database access, the project uses *Entity Framework Core* (EF Core), an object-relational mapper (ORM) that lets C\# code work with database tables as objects.

Key features used:
- *Code-first migrations:* Database schema is defined in C\# and applied via migrations
- *LINQ queries:* Database queries written in C\# syntax
- *PostgreSQL provider:* Connects EF Core to PostgreSQL

Example of a database query:
```csharp
var products = await dbContext.Products
    .Where(p => p.IsActive)
    .OrderByDescending(p => p.CreatedAt)
    .Take(10)
    .ToListAsync();
```

=== Summary of Backend Libraries

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (center, left),
    [*Library*], [*Purpose*],
    [ASP.NET Core], [Web framework],
    [Carter], [Organize minimal API endpoints],
    [MediatR], [Request/handler dispatching],
    [FluentValidation], [Request validation],
    [Entity Framework Core], [Database access (ORM)],
    [ErrorOr], [Functional error handling],
    [Npgsql], [PostgreSQL database driver],
  ),
  caption: [Key backend libraries and their purposes]
)

These libraries work together to create a maintainable codebase where each feature is self-contained and easy to modify independently.


