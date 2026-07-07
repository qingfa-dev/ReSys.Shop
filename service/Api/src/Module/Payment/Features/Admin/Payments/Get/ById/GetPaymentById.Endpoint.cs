using BuildingBlocks.Authorization.Attributes;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.Payments.Get.ById;

public static partial class GetPaymentById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(PaymentFeature.Admin.Payments.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetPaymentById))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.Payments.GetById.Permission)
            .WithSummary(PaymentFeature.Admin.Payments.GetById.Summary)
            .WithDescription(PaymentFeature.Admin.Payments.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
