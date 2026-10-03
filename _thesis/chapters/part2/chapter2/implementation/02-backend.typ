=== Backend Implementation

#import "../../../../template/ctu-styles.typ": figure-placeholder

The backend is a high-performance REST API built on *.NET 10* using a strict *Vertical Slice Architecture (VSA)*. Adopting VSA allows the system to modularize features by business capability rather than technical layers, ensuring maintainability and scalability.

- *Runtime:* *.NET 10* (ASP.NET Core).
- *Architecture:* *Vertical Slice Architecture* using *Carter* (Minimal APIs) for routing and *MediatR* for decoupling request handling (CQRS pattern).
- *Data Access:* *Entity Framework Core* with *PostgreSQL*.
  - *PgVector:* Utilizes the `pgvector` extension for storing and querying 512-dimensional embeddings.
- *Observability:* *OpenTelemetry* and *Serilog* for distributed tracing.
- *Documentation:* *Scalar.AspNetCore* for interactive OpenAPI documentation.

The fundamental structure of the data layer is designed around the *Vertical Slice* philosophy. As shown in the Entity Relationship Diagram (ERD) below, the database schema is partitioned into contexts like `Catalog`, `Ordering`, and `Identity`. This separation ensures that logic belonging to one slice does not inadvertently mutate data belonging to another, maintaining a "Shared-Nothing" approach at the application layer.

#figure(
  placement: none,
  ```cs
  // ReSys.Core/Data/ShopDbContext.cs
  public class ShopDbContext : DbContext, IApplicationDbContext
  {
      public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) {}

      // Domain Entities partitioned by Business Context
      public DbSet<Product> Products => Set<Product>();           // Catalog Context
      public DbSet<Order> Orders => Set<Order>();                 // Ordering Context
      public DbSet<StockItem> StockItems => Set<StockItem>();     // Inventory Context
      public DbSet<User> Users => Set<User>();                    // Identity Context

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          // Applies specific configurations (Keys, Indexes, Constraints)
          modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDbContext).Assembly);
          base.OnModelCreating(modelBuilder);
      }
  }
  ```,
  caption: [ShopDbContext Implementation: The single database context aggregates DbSets from all business domains, corresponding to the schema defined in @fig:data-01-erd.],
)

This implementation directly maps the architectural concepts defined in @sec:architecture-vsa to the project structure. As shown below, features are isolated into their own directories, containing all necessary logic (Commands, Handlers, Validators).

#figure(
  placement: none,
  ```text
  src/
    ReSys.Core/
      Features/                        // Vertical Slices
        Catalog/
          CreateProduct/
            CreateProductCommand.cs    // Request (DTO)
            CreateProductHandler.cs    // Logic (CQRS)
            CreateProductValidator.cs  // Validation
        Ordering/
          PlaceOrder/
            PlaceOrderCommand.cs
            PlaceOrderHandler.cs
  ```,
  caption: [Project Directory Structure: Implementation of Vertical Slice Architecture where code is co-located by feature.],
)

The internal structure of the API layer follows the principles of *Minimal APIs* in .NET 10, utilizing *Carter* for route registration and *MediatR* for executing business logic within the vertical slices.


#figure(
  placement: none,
  image("/images/diagrams/02-system-architecture/sys-03-api-structure.png", width: 30%),
  caption: [API Service Internal Structure: Orchestration of Minimal APIs, Carter modules, and MediatR handlers.],
)

#include "02-backend/01-orchestration-observability.typ"
#include "02-backend/02-middleware-pipelines.typ"
#include "02-backend/03-key-features-intro.typ"
#include "02-backend/04-product-images-vectorization.typ"
#include "02-backend/05-recommendations-api.typ"
#include "02-backend/06-atomic-stock-reservation.typ"
#include "02-backend/07-system-automation.typ"
