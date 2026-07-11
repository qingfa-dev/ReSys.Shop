using System.Data;
using System.Reflection;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Configurations.DateTimes;
using Shared.Operational.Persistence.Configurations.Enums;
using Shared.Operational.Persistence.Configurations.Models;
using Shared.Operational.Persistence.Configurations.Numbers;
using Shared.Operational.Persistence.Configurations.Vectors;
using Shared.Operational.Persistence.Transactions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Roles.Claims;
using Shared.Security.Identity.Domain.Users;
using Shared.Security.Identity.Domain.Users.Claims;
using Shared.Security.Identity.Domain.Users.Keys;
using Shared.Security.Identity.Domain.Users.Logins;
using Shared.Security.Identity.Domain.Users.Roles;
using Shared.Security.Identity.Domain.Users.Tokens;

namespace Shared.Operational.Persistence.Data;

/// <summary>
/// The primary database context for the application, combining Identity and application data.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken, UserPasskey>,
      IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public bool SupportsTransactions =>
        Database.ProviderName is not "Microsoft.EntityFrameworkCore.InMemory";

    public async Task<IDatabaseTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (!SupportsTransactions)
            return new NoOpTransaction();

        var transaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return new EfCoreTransaction(transaction);
    }

    /// <summary>
    /// Additional assemblies to scan for entity configurations.
    /// Set this static property before the DbContext is first used.
    /// </summary>
    public static Assembly[]? AdditionalConfigurationsAssemblies { get; set; }


    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Contract: pre=builder!=null, post=builder.Model!=null

        base.OnModelCreating(builder);

        // Update: Ignore non-essential Identity mappings to optimize schema
        builder.Ignore<IdentityPasskeyData>();

        // Check: Determine if current provider is Npgsql
        var isNpgsql = Database.IsNpgsql();

        if (isNpgsql)
        {
            // Initialize: Enable pgvector support for similarity searches
            builder.HasPostgresExtension("vector");
        }

        // Initialize: Discover and apply entity configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Initialize: Apply entity configurations from discovered assemblies
        if (AdditionalConfigurationsAssemblies != null)
        {
            foreach (Assembly? assembly in AdditionalConfigurationsAssemblies.Distinct().Where(a => a != typeof(ApplicationDbContext).Assembly))
            {
                builder.ApplyConfigurationsFromAssembly(assembly);
            }
        }

        // Initialize: Apply cross-cutting domain configurations for base entities
        EntityModelConfiguration.ConfigureModel(builder, isNpgsql);

        // Initialize: Apply global enum-to-string conversion convention (provider-aware)
        EnumConfiguration.ConfigureModel(builder);

        // Initialize: Apply provider-adaptable vector configurations (reads VectorDimensionsAttribute)
        VectorConfiguration.ConfigureModel(builder, isNpgsql);
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Contract: pre=configurationBuilder!=null

        var isNpgsql = Database.IsNpgsql();

        // Initialize: Apply global type-level conventions for consistency across schemas
        DateTimeConfiguration.ConfigureConvention(configurationBuilder, isNpgsql: isNpgsql);
        DecimalConfiguration.ConfigureConvention(configurationBuilder);
        EnumConfiguration.ConfigureConvention(configurationBuilder);
        VectorConfiguration.ConfigureConvention(configurationBuilder, isNpgsql: isNpgsql);
    }
}
