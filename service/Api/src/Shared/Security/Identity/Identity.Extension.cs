using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;
using Shared.Security.Identity.Options;

namespace Shared.Security.Identity;

/// <summary>
/// Provides extension methods for configuring ASP.NET Core Identity with enhanced security settings.
/// </summary>
public static class IdentityExtensions
{
    #region Service Registration

    /// <summary>
    /// Registers ASP.NET Core Identity with enhanced security settings using the primary ApplicationDbContext.
    /// Configures password policies, lockout settings, and sign-in requirements aligned with OWASP standards.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration for retrieving settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static WebApplicationBuilder AddApplicationIdentity(this WebApplicationBuilder builder)
    {
        #region Data Protection

        // Initialize: Data Protection for token stability across multiple instances
        builder.Services.AddDataProtection()
            .SetApplicationName(IdentitySettingConstant.DataProtection.ApplicationName);

        #endregion

        #region Identity Configuration

        // Configure: Identity Core with OWASP-aligned security settings
        builder.Services.AddIdentityCore<User>(options =>
        {
            // Password Policy (OWASP aligned)
            options.Password.RequireDigit = IdentitySettingConstant.Password.RequireDigit;
            options.Password.RequireLowercase = IdentitySettingConstant.Password.RequireLowercase;
            options.Password.RequireUppercase = IdentitySettingConstant.Password.RequireUppercase;
            options.Password.RequireNonAlphanumeric = IdentitySettingConstant.Password.RequireNonAlphanumeric;
            options.Password.RequiredLength = IdentitySettingConstant.Password.RequiredLength;
            options.Password.RequiredUniqueChars = IdentitySettingConstant.Password.RequiredUniqueChars;

            // User Policy
            options.User.RequireUniqueEmail = IdentitySettingConstant.User.RequireUniqueEmail;
            options.User.AllowedUserNameCharacters = IdentitySettingConstant.User.AllowedUserNameCharacters;

            // Lockout Policy
            options.Lockout.DefaultLockoutTimeSpan = IdentitySettingConstant.Lockout.DefaultLockoutTimeSpan;
            options.Lockout.MaxFailedAccessAttempts = IdentitySettingConstant.Lockout.MaxFailedAccessAttempts;
            options.Lockout.AllowedForNewUsers = IdentitySettingConstant.Lockout.AllowedForNewUsers;

            // Sign-In Settings
            options.SignIn.RequireConfirmedEmail = IdentitySettingConstant.SignIn.RequireConfirmedEmail;
            options.SignIn.RequireConfirmedAccount = IdentitySettingConstant.SignIn.RequireConfirmedAccount;

            // Schema / Stores
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version2;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        #endregion

        #region Security Validation

        // Configure: Security stamp validation interval for fresh identity checks
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = IdentitySettingConstant.SecurityStamp.ValidationInterval;
        });

        #endregion

        return builder;
    }

    #endregion
}
