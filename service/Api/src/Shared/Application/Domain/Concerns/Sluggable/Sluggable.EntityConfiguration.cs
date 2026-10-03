using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Application.Domain.Concerns.Sluggable;

public static class SluggableConfiguration
{
    public static void Apply<T>(EntityTypeBuilder<T> builder)
        where T : class, ISluggable
    {
        builder.Property(m => m.Slug)
            .IsRequired()
            .HasMaxLength(SluggableConstant.Constraints.MaxSlugLength);
    }
}