=== API Design

The ReSys.Shop API exposes a RESTful interface built on Carter minimal APIs and organised around the MediatR CQRS pattern. This section describes the API architecture, the endpoint organisation scheme, and a summary of the key endpoints that define the platform's external contract.

==== API Architecture

The API layer acts as a thin orchestration boundary. It contains no business logic; instead, it delegates all processing to the MediatR pipeline. Each request follows a consistent path: the Carter endpoint receives the HTTP request, extracts route and body parameters, constructs a MediatR command or query object, dispatches it through `ISender`, and maps the returned `Result<T>` to an HTTP response. This design keeps endpoints concise, typically six to twelve lines, and concentrates all domain logic in the handler layer, where it is testable without HTTP infrastructure.

Carter modules group related endpoints by module and surface. Each module (Catalog, Ordering, Payment, and so on) registers its own `ICarterModule` implementation, which defines the route groups, HTTP methods, and parameter bindings for that module's endpoints. This modular registration avoids a single monolithic route configuration file and enables each bounded context to own its API surface.

FluentValidation provides input validation through validator classes associated with each command and query. Validators run automatically as part of the MediatR pipeline behaviour, before the handler executes, ensuring that handlers never receive invalid input. Validation failures return standardised `400 Bad Request` responses with field-level error details.

==== Endpoint Organisation

Endpoints are organised by two dimensions: the business module that owns the operation, and the surface, Admin or Storefront, that serves as the entry point. The URL pattern follows the convention `/api/{module}/{surface}/{action}`, where module identifies the owning bounded context, surface distinguishes administrative from customer-facing operations, and action names the specific operation.

This two-dimensional organisation serves several purposes. It makes the API self-documenting: the URL alone communicates which business area and which user role the endpoint targets. It simplifies authorisation: Admin surface endpoints share a common authorisation policy requiring an administrator role, while Storefront endpoints apply corresponding customer-level policies. And it enables independent versioning: a module can evolve its endpoints without affecting other modules.

Table @tbl-key-endpoints summarises the most architecturally significant endpoints across the platform. These endpoints represent the primary user-facing capabilities, visual search, catalogue browsing, checkout, order history, authentication, and payment, that together define the complete customer and administrator experience.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, center + horizon, left),

    table.header([*Endpoint*], [*Module*], [*Surface*], [*Description*]),

    [`POST /api/catalog/storefront/search-by-image`], [Catalog], [Storefront], [
      Accepts an uploaded image file, sends it to the ML sidecar for embedding generation, queries pgvector for the nearest neighbour variant images by cosine similarity, and returns matching products ranked by similarity score with variant thumbnails, pricing, and product URLs.
    ],

    [`GET /api/catalog/storefront/products/{slug}`], [Catalog], [Storefront], [
      Returns a product with all its published variants, images, option configurations, and taxonomy classifications, identified by its URL slug. Supports guest access for anonymous browsing.
    ],

    [`POST /api/ordering/storefront/cart/checkout`], [Ordering], [Storefront], [
      Advances the cart through the checkout state machine: setting the shipping address, selecting the delivery method, and confirming the order. Each call transitions the checkout state forward one step.
    ],

    [`GET /api/ordering/storefront/orders/{id}`], [Ordering], [Storefront], [
      Returns the complete order with line items, payment state, shipment state, and status history. Requires authentication; customers may only access their own orders.
    ],

    [`POST /api/identity/store/auth/login`], [Identity], [Storefront], [
      Authenticates a user by email and password, returning a JWT access token (fifteen-minute lifetime) and a refresh token for token rotation. Supports Google OAuth as an alternative login method via a related endpoint.
    ],

    [`POST /api/payment/storefront/payment/create-intent`], [Payment], [Storefront], [
      Creates a payment intent for the specified order amount and currency, initialising the payment state machine.
    ],
  ),
  caption: [Key API endpoints representing the primary user-facing capabilities of the platform. All endpoints in the Storefront surface serve customer interactions; Admin surface endpoints (not shown) mirror these with full CRUD capabilities on all modules.],
) <tbl-key-endpoints>

The admin surface provides full CRUD operations on all module entities, products, variants, orders, inventory, users, shipping methods, and location data, following the same URL pattern with the Admin surface prefix. These endpoints are excluded from the table to maintain focus on the core platform capabilities, but they follow identical architectural patterns: minimal API route groups, MediatR dispatch, FluentValidation, and permission-based authorisation.
