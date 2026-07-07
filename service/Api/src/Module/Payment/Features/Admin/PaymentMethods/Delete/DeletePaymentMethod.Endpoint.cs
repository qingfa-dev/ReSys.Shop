using BuildingBlocks.Authorization.Attributes;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Module.Payment.Features.Shared;

namespace Module.Payment.Features.Admin.PaymentMethods.Delete;

public static partial class DeletePaymentMethod
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(PaymentFeature.Admin.PaymentMethods.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeletePaymentMethod))
            .WithTags(PaymentFeature.Tags.Payment)
            .HasPermission(PaymentFeature.Admin.PaymentMethods.Delete.Permission)
            .WithSummary(PaymentFeature.Admin.PaymentMethods.Delete.Summary)
            .WithDescription(PaymentFeature.Admin.PaymentMethods.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
