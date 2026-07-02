using Module.Location.Features.Shared;

using Shared.Security.Authorization.Attributes;

namespace Module.Location.Features.Admin.Countries.Delete;

public static partial class DeleteCountry
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(pattern: LocationFeature.Admin.Countries.Delete.Route, handler: async (
                    [FromRoute] Guid id,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var command = new Command(Id: id);
                    var result = await sender.Send(request: command, cancellationToken: ct);
                    return result.ToResult();
                })
                .WithName(nameof(DeleteCountry))
                .WithTags(tags: LocationFeature.Admin.Countries.Tags)
                .HasPermission(permission: LocationFeature.Admin.Countries.Delete.Permission)
                .WithSummary(summary: LocationFeature.Admin.Countries.Delete.Summary)
                .WithDescription(description: LocationFeature.Admin.Countries.Delete.Description)
                .Produces<Result>()
                .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
                .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}