using Carter;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Shared.Security.AntiForgery.Endpoints;

public sealed class AntiForgeryEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/antiforgery")
            .WithTags("AntiForgery");

        group.MapGet("/token", HandleGetToken)
            .WithName("GetAntiForgeryToken")
            .Produces<Result<TokenResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    internal static Microsoft.AspNetCore.Http.IResult HandleGetToken(IAntiforgery antiforgery, HttpContext context)
    {
        Result<TokenResponse> result = GetToken(antiforgery, context);

        return result.ToApiResult();
    }

    internal static Result<TokenResponse> GetToken(IAntiforgery antiforgery, HttpContext context)
    {
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);

        TokenResponse response = new() { Token = tokens.RequestToken!, HeaderName = tokens.HeaderName! };

        return Result<TokenResponse>.Ok(response);
    }
}
