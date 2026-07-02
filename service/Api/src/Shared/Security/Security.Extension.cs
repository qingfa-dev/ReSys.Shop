using Microsoft.AspNetCore.Builder;

using Shared.Security.AntiForgery;
using Shared.Security.Authentication;
using Shared.Security.Authorization;
using Shared.Security.Cors;
using Shared.Security.Headers;
using Shared.Security.Identity;

namespace Shared.Security;

public static class SecurityExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddSecurity(
        this WebApplicationBuilder builder)
    {
        builder.AddApplicationAuthorization();
        builder.AddApplicationAuthentication();
        builder.AddCors();
        builder.AddApplicationIdentity();
        builder.AddSecurityHeaders();
        builder.AddAntiForgery();

        return builder;
    }

    #endregion

    #region Pipeline Configuration

    public static WebApplication UseSecurity(this WebApplication app)
    {
        app.UseSecurityHeaders();
        CorsExtensions.UseCors(app);
        AuthenticationExtensions.UseApplicationAuthentication(app);
        app.UseApplicationAuthorization();

        return app;
    }

    #endregion
}
