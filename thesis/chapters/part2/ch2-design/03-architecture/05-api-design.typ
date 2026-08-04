=== API Design

The ReSys.Shop API exposes a RESTful interface on .NET 10 minimal APIs via *Carter* modules, dispatched through the *MediatR* CQRS pattern @young2010cqrs. The platform registers 257 endpoints across nine business modules (184 admin, 73 storefront).

==== API Architecture

The API host acts as a thin orchestration boundary with zero business logic. Each request follows a standard path:

- *Endpoint Reception:* A Carter endpoint receives the HTTP request, extracts parameters, constructs a MediatR command or query, and dispatches via `ISender`.
- *Pipeline Validation:* FluentValidation behaviors execute before handler processing. Validation failures halt the pipeline and return HTTP 400 with field-level details.
- *Handler Execution:* Bounded context handlers process requests within domain isolation, operating on application models without HTTP dependencies.
- *Response Mapping:* Handlers return `Result<T>`. Endpoints map success to HTTP 200, 201, or 204, and domain errors to RFC 7807 Problem Details.

==== Endpoint Organisation and Route Structure

Endpoints follow a two-dimensional URL convention: `/api/{module}/{surface}/{resource}`.

- *`module`:* The owning bounded context (`catalog`, `ordering`, `payment`, `inventory`, `identity`, `profile`, `shipping`, `location`).
- *`surface`:* `storefront` (customer-facing) or `admin` (administrative operations).
- *`resource`:* The domain resource and sub-action (e.g., `products`, `cart/checkout`, `search-by-image`).

This structure provides self-documenting URLs, surface-level authorisation boundaries (`admin` routes enforce administrator policies globally), and independent module evolution without breaking adjacent contexts.

==== API Execution Layout

The repository follows the modular monolith pattern: a thin API host, eight bounded context modules, shared building blocks, and a Python ML sidecar.

- *Apps:* `ReSys.Shop.Api` (.NET 10 Carter host), `ReSys.Shop.Store` (Vue 3 storefront), `ReSys.Shop.Admin` (Vue 3 dashboard).
- *Modules:* Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location -- each with `Features/`, `Domain/`, and `Persistence/` layers.
- *BuildingBlocks:* `SharedKernel` (Result, Entity, domain events), `Persistence` (DbContext, interceptors, migrations), `Security` (authorisation policies, JWT).
- *Sidecar:* `ReSys.Shop.Ml` (Python FastAPI) with `api/` (embedding routes), `core/` (strategy registry), `models/` (Fashion-CLIP, DINOv2, ResNet, CLIP encoders).
- *Infrastructure:* `tests/` (unit, integration, benchmarks), `AppHost/` (.NET Aspire orchestrator).

==== Full API Endpoint Contract

@tbl-api-contract summarises the API surface verified against the 257 registered Carter endpoints in the codebase.

#figure(
  table(
    columns: (auto, 2fr, 2fr, auto),
    stroke: 0.5pt,
    align: (left + horizon, left, left, center + horizon),

    table.header([*Module*], [*Admin Routes*], [*Storefront Routes*], [*N*]),

    [Catalog],
    [Products CRUD, variants, variant images, option types/values, taxonomies, taxons, taxon rules, classifications, pricing, dashboard],
    [Product listing/search, product detail by slug, availability, related/similar products, CBIR search-by-image, taxonomy tree, taxon browsing, option types, image display],
    [77],

    [Identity],
    [Users CRUD, user roles/permissions, roles CRUD, role permissions, permissions catalogue],
    [Password/Google login, register, logout, session refresh, email confirm/change, password change/forgot/reset],
    [37],

    [Ordering],
    [Orders CRUD, line items, order status/cancel/complete/approve/resume, shipping/billing address, shipping method, dashboard],
    [Cart CRUD, cart items add/update/remove, cart associate, empty, checkout, validate, shipping rate, customer orders list/detail/cancel],
    [34],

    [Inventory],
    [Stock locations CRUD, stock items CRUD, bulk adjust/import, restock, low stock, summary, reservations, transfers, movements, dashboard],
    [Variant availability, cart reserve/release/list],
    [32],

    [Profile],
    [Profiles CRUD, addresses CRUD],
    [Profiles, addresses CRUD, notification preferences, wishlists CRUD with items],
    [26],

    [Location],
    [Countries CRUD (by ID or ISO code), states CRUD (by ID or ISO code)],
    [Countries browse, states browse (by ID or ISO code)],
    [18],

    [Payment],
    [Payment methods CRUD, activate/deactivate, payments list/detail, capture/void/refund],
    [Create payment intent, confirm payment, available methods, setup intent, Stripe webhook],
    [17],

    [Shipping],
    [Shipping methods CRUD, activate/deactivate, shipping rates CRUD],
    [Available methods, calculate cost, rates list],
    [15],

    [Dashboard],
    [Aggregated metrics: sales, inventory, catalog, activity],
    [--],
    [1],
  ),
  kind: table,
  caption: [ReSys.Shop API endpoint contract. The platform exposes 257 Carter endpoints across nine business modules (184 admin, 73 storefront). Admin routes provide full CRUD with activate/deactivate and sync/assign/revoke patterns. Storefront routes expose read-optimised public surfaces for product discovery, cart, checkout, and account management.],
) <tbl-api-contract>

==== Error Handling and Response Standards

All API endpoints conform to RFC 7807 Problem Details for error responses:

- *HTTP 400:* FluentValidation failures with field-level error mappings.
- *HTTP 401:* Missing or expired JWT in `Authorization: Bearer` header.
- *HTTP 403:* Insufficient role or permission scope (e.g., customer accessing admin route).
- *HTTP 404:* Domain entity or route resolution failure.
- *HTTP 409:* Optimistic concurrency control failure (e.g., inventory reservation race via PostgreSQL `xmin`).
- *HTTP 500:* Global exception middleware prevents stack trace leakage while logging full detail to telemetry.
