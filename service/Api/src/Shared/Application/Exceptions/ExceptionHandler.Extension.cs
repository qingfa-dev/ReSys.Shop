using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Exceptions.Handlers;

namespace Shared.Application.Exceptions;

public static class ExceptionHandlerExtension
{
    public static WebApplicationBuilder AddGlobalExceptionHandler(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }

    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
