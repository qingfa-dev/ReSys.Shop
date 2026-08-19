=== System Overview

ReSys.Shop comprises three services -- a Vue 3 frontend, a .NET 10 backend @microsoft-aspnet-core, and a Python *FastAPI* ML sidecar @paszke2019pytorch -- and eight *bounded contexts* using *Domain-Driven Design* with MediatR dispatch between modules.

#figure(
  table(
    columns: (auto, 1fr, 4fr),
    stroke: 0.5pt,
    align: (center, left, left),

    table.header([*Service*], [*Technology Stack*], [*Responsibilities*]),

    [Vue Frontend],
    [Vue 3 + TypeScript + Vite],
    [
      - Customer storefront and administrator dashboard\
      - Image upload and visual search UI
    ],

    [.NET Backend],
    [.NET 10 + ASP.NET Core + Carter + EF Core],
    [
      - REST API via Carter minimal APIs and MediatR CQRS\
      - PostgreSQL persistence with pgvector and Redis caching
    ],

    [Python ML],
    [Python 3.12 + FastAPI + PyTorch],
    [
      - Embedding model inference (Fashion-CLIP and others)\
      - Multi-model support with lazy-loading strategy
    ],
  ),
    kind: table,
  caption: [System services and their technology stacks. Each service communicates through well-defined HTTP contracts.],
) <tbl-system-services>

Internally, the backend is partitioned into nine bounded contexts, each owning a dedicated database schema. @tbl-contexts-overview lists each context, its aggregate root, and key domain entities.

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
