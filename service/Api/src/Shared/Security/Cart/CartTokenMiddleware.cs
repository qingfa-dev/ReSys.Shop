using Microsoft.AspNetCore.Http;

namespace Shared.Security.Cart;

public sealed class CartTokenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Cart-Token", out var values))
        {
            context.Items["CartToken"] = values.FirstOrDefault();
        }
        await next(context);
    }
}
