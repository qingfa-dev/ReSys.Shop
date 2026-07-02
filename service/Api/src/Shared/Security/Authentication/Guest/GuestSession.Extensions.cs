using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.Authentication.Guest.Options;

namespace Shared.Security.Authentication.Guest;

public static class GuestSessionExtensions
{
    public static WebApplicationBuilder AddGuestSession(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidator<GuestSessionSetting>, GuestSessionSettingValidator>();
        builder.Services.AddOptions<GuestSessionSetting>()
            .BindConfiguration(GuestSessionSetting.SectionName)
            .ValidateFluentValidation();

        return builder;
    }

    public static WebApplication UseGuestSession(this WebApplication app)
    {
        app.UseMiddleware<GuestSessionMiddleware>();

        return app;
    }
}
