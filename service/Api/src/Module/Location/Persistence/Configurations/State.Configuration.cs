using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Location.Domain.States;
using Module.Location.Persistence.Constants;

namespace Module.Location.Persistence.Configurations;

public sealed class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        // Table
        builder.ToTable(name: LocationSchema.TableNames.States, schema: LocationSchema.Name);

        // Key
        builder.HasKey(keyExpression: s => s.Id);

        // Properties
        builder.Property(propertyExpression: s => s.Name)
            .HasMaxLength(maxLength: StateConstant.Constraints.MaxNameLength)
            .IsRequired();

        builder.Property(propertyExpression: s => s.Abbreviation)
            .HasMaxLength(maxLength: StateConstant.Constraints.MaxAbbreviationLength)
            .IsRequired();

        builder.Property(propertyExpression: s => s.IsActive)
            .IsRequired();

        // Relationships
        builder.HasOne(navigationExpression: s => s.Country)
            .WithMany(navigationExpression: c => c.States)
            .HasForeignKey(foreignKeyExpression: s => s.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}
