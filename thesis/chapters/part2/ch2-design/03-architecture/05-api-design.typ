=== API Design

The API exposes a RESTful interface via *Carter* modules and the *MediatR* CQRS pattern @young2010cqrs, registering approximately 262 endpoints across eight modules.

==== API Architecture

Each request follows a standard MediatR pipeline: Carter endpoint → #emph[LoggingBehavior] → #emph[ValidationBehavior] → #emph[ExceptionMappingBehavior] → #emph("Handler.Execute()") → #emph("Result<T>.ToResult()") → HTTP Response (RFC 7807 Problem Details on error).

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

Handlers return #emph("Result<T>"); success maps to 200/201/204, domain errors to RFC 7807 Problem Details.

==== Endpoint Organisation

Endpoints follow the convention #emph("/api/{surface}/{module}/{resource}"), where #emph[surface] is #emph[storefront] or #emph[admin]. All #emph[admin] routes enforce administrator policies via #emph(".HasPermission()"). Eleven inter-module contract DTOs enable cross-module communication: #emph[ReserveCartStock], #emph[ReleaseCartStockReservations], #emph[ConsumeCartStockReservations], #emph[CheckVariantAvailability] (Inventory); #emph[GetCartForCheckout], #emph[GetCartForShipping], #emph[AdvanceCheckoutState] (Ordering); #emph[GetPaymentForCheckout], #emph[MarkPaymentPaid] (Payment); #emph[GetVariantDiscontinuedStatuses], #emph[GetVariantWeights] (Catalog).

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

All API endpoints conform to RFC 7807 Problem Details: #emph[400] for FluentValidation failures, #emph[401] for missing/expired JWT, #emph[403] for insufficient permissions, #emph[404] for entity/route resolution failure, #emph[409] for concurrency conflicts (PostgreSQL #emph[xmin]), and #emph[500] via global exception middleware preventing stack trace leakage.
