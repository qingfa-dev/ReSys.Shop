using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Storefront.Cart.AdvanceCheckout;

public static partial class AdvanceCheckout
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Storefront.Cart.Advance.Route, async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(), ct);
                return result.ToResult();
            })
            .WithName(nameof(AdvanceCheckout))
            .WithTags(OrderingFeature.Tags.Cart)
            .WithSummary(OrderingFeature.Storefront.Cart.Advance.Summary)
            .WithDescription(OrderingFeature.Storefront.Cart.Advance.Description)
            .Produces<Result>();
        }
    }
}
