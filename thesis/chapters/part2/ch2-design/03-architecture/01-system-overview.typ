=== System Overview

ReSys.Shop is built as a service-oriented system with three distinct services. The frontend is implemented in Vue 3 and TypeScript using the Vite build tool. The backend is a .NET 10 modular monolith using ASP.NET Core for HTTP handling, Entity Framework Core for data access, and Carter for minimal API endpoint registration. The machine learning service is a Python FastAPI application running PyTorch models that generates vector embeddings from product images for visual similarity search.

@tbl-system-services summarises the three services, their technology stacks, and their primary responsibilities within the platform.

#figure(
  table(
    columns: (auto, auto, auto),
    stroke: 0.5pt,
    align: (center + horizon, left, left),

    table.header([*Service*], [*Technology Stack*], [*Responsibilities*]),

    [Vue Frontend],
    [Vue 3 + TypeScript + Vite],
    [
      - Customer storefront (Nuxt UI)\
      - Administrator dashboard (PrimeVue)\
      - Pinia state management\
      - Image upload and visual search UI
    ],

    [.NET Backend],
    [.NET 10 + ASP.NET Core + Carter + EF Core],
    [
      - REST API endpoints via Carter minimal APIs\
      - Business logic via MediatR CQRS pattern\
      - PostgreSQL persistence with pgvector vector search\
      - JWT authentication and RBAC authorisation
    ],

    [Python ML],
    [Python 3.12 + FastAPI + PyTorch],
    [
      - Fashion-CLIP and other embedding model inference\
      - Vector embedding generation from product images\
      - Multi-model support with lazy-loading strategy
    ],
  ),
    kind: table,
  caption: [System services and their technology stacks. Each service is independently deployable and communicates through well-defined HTTP contracts.],
) <tbl-system-services>

The backend is internally organised into eight bounded contexts following the principles of Domain-Driven Design. Each context owns a distinct area of business logic and communicates with other contexts exclusively through MediatR in-process message dispatch, there are no direct namespace references between business modules. @tbl-contexts-overview lists each context, its aggregate root, and a representative sample of its domain entities.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center + horizon, center + horizon, left),

    table.header([*Bounded Context*], [*Aggregate Root*], [*Key Domain Entities*]),

    [Catalog], [Product], [
      Variant, VariantImage, OptionType, OptionValue,
      Classification, Taxonomy, Taxon
    ],

    [Ordering], [Order], [
      LineItem, Adjustment, Shipment
    ],

    [Payment], [PaymentIntent], [
      PaymentCapture
    ],

    [Inventory], [StockItem], [
      StockLocation, StockMovement, StockReservation
    ],

    [Identity], [User], [
      Role, UserRole, RefreshToken, UserLogin
    ],

    [Profile], [UserProfile], [
      Address, Wishlist
    ],

    [Shipping], [ShippingMethod], [
      ShippingRate, ShippingZone
    ],

    [Location], [Country], [
      State
    ],
  ),
    kind: table,
  caption: [Bounded contexts with aggregate roots and key domain entities. Each context owns its database schema and communicates with other contexts through MediatR dispatch only.],
) <tbl-contexts-overview>

The separation of concerns across these eight contexts enables independent evolution of each business domain while the modular monolith deployment model avoids the operational complexity of distributed microservices. The following section details the domain-driven design principles that govern these contexts.
