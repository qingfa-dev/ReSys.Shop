=== System Overview

ReSys.Shop comprises three independently deployable *services*: a Vue 3 *TypeScript* frontend, a .NET 10 *ASP.NET Core* backend @microsoft-aspnet-core, and a Python *FastAPI* machine learning sidecar running *PyTorch* @paszke2019pytorch models. @tbl-system-services summarises their technology stacks and responsibilities.

#figure(
  table(
    columns: (auto, 1fr, 4fr),
    stroke: 0.5pt,
    align: (center, left, left),

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
  caption: [System services and their technology stacks. Each service communicates through well-defined HTTP contracts.],
) <tbl-system-services>

Internally, the backend is partitioned into eight *bounded contexts* following *Domain-Driven Design* (DDD) principles. Each context owns a dedicated database schema and communicates with others exclusively through *MediatR* in-process dispatch -- there are no direct namespace references between business modules. @tbl-contexts-overview lists each context, its aggregate root, and key domain entities.

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
  caption: [Bounded contexts with aggregate roots and key domain entities. Each context owns its database schema and communicates through MediatR dispatch only.],
) <tbl-contexts-overview>

This federated structure enables independent evolution of each domain while the *modular monolith* deployment model @newman2019monolith avoids the operational overhead of distributed microservices. Domain modelling is detailed in Section 2.3.2.
