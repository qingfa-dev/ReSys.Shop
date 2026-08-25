=== API Design

The API exposes a RESTful interface via *Carter* modules and the *MediatR* CQRS pattern @young2010cqrs, registering approximately 262 endpoints across eight modules.

==== API Architecture

Each request follows a standard MediatR pipeline: Carter endpoint → `LoggingBehavior` → `ValidationBehavior` → `ExceptionMappingBehavior` → `Handler.Execute()` → `Result<T>.ToResult()` → HTTP Response (RFC 7807 Problem Details on error).

A representative endpoint pattern:

```cs
public void AddRoutes(IEndpointRouteBuilder app)
{
    app.MapPost(CatalogFeature.Admin.Products.Create.Route,
        async ([FromBody] Request r, ISender s, CancellationToken ct) =>
        {
            var result = await s.Send(new Command(r), ct);
            return result.ToResult();
        })
        .HasPermission(CatalogFeature.Admin.Products.Create.Permission);
}
```

Handlers return `Result<T>`; success maps to 200/201/204, domain errors to RFC 7807 Problem Details.

==== Endpoint Organisation

Endpoints follow the convention `/api/{surface}/{module}/{resource}`, where `surface` is `storefront` or `admin`. All `admin` routes enforce administrator policies via `.HasPermission()`. Eleven inter-module contract DTOs enable cross-module communication: `ReserveCartStock`, `ReleaseCartStockReservations`, `ConsumeCartStockReservations`, `CheckVariantAvailability` (Inventory); `GetCartForCheckout`, `GetCartForShipping`, `AdvanceCheckoutState` (Ordering); `GetPaymentForCheckout`, `MarkPaymentPaid` (Payment); `GetVariantDiscontinuedStatuses`, `GetVariantWeights` (Catalog).

==== API Endpoint Contract

@tbl-api-contract summarises the registered Carter endpoints.

#figure(
  table(
    columns: (auto, 2fr, 2fr, auto),
    stroke: 0.5pt,
    align: (left + horizon, left, left, center + horizon),
    table.header([*Module*], [*Admin Routes*], [*Storefront Routes*], [*N*]),
    [Catalog],
    [Products, variants, images, option types/values, taxonomies, pricing, dashboard],
    [Product listing/search/detail, availability, similar products, CBIR search, taxonomy, taxon browsing],
    [80],
    [Identity],
    [Users, roles, permissions catalogue],
    [Login, register, logout, session refresh, email, password reset],
    [37],
    [Ordering],
    [Orders, line items, status transitions, shipping/billing address, dashboard],
    [Cart CRUD, cart items, checkout, shipping rate, customer orders],
    [35],
    [Inventory],
    [Stock locations, stock items, bulk adjust, low stock, reservations, transfers, dashboard],
    [Variant availability, cart reserve/release],
    [32],
    [Profile],
    [Profiles, addresses CRUD],
    [Profiles, addresses, notification preferences, wishlists],
    [27],
    [Location],
    [Countries, states CRUD],
    [Countries, states browse],
    [18],
    [Payment],
    [Payment methods, payments list/detail, capture/void/refund],
    [Payment intent create/confirm, methods, setup intent, Stripe webhook],
    [17],
    [Shipping],
    [Shipping methods, shipping rates CRUD],
    [Available methods, calculate cost, rates],
    [15],
    [Dashboard],
    [Aggregated metrics: sales, inventory, catalog, activity],
    [--],
    [1],
  ),
  kind: table,
  caption: [ReSys.Shop API contract: ~262 Carter endpoints across eight modules.],
) <tbl-api-contract>

==== Error Handling

All API endpoints conform to RFC 7807 Problem Details: #raw("400", lang: "http") for FluentValidation failures, #raw("401", lang: "http") for missing/expired JWT, #raw("403", lang: "http") for insufficient permissions, #raw("404", lang: "http") for entity/route resolution failure, #raw("409", lang: "http") for concurrency conflicts (PostgreSQL `xmin`), and #raw("500", lang: "http") via global exception middleware preventing stack trace leakage.
