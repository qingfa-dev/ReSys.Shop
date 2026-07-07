using BuildingBlocks.Authorization.Attributes;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Get.ById;

public static partial class GetPaymentMethodById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PaymentFeature.Admin.PaymentMethods.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetPaymentMethodById))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.GetById.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.GetById.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
