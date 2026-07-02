using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Application.Domain.Concerns.Versionable;

public static class VersionableConfiguration
{
    public static void Apply<T>(EntityTypeBuilder builder) where T : class, IVersionable
    {
        builder.Property(nameof(IVersionable.Version)).IsConcurrencyToken();
    }
}
