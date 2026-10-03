using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.OptionTypes;

namespace Module.Catalog.Persistence.Configurations.OptionTypes;

public class OptionTypeConfiguration : IEntityTypeConfiguration<OptionType>
{
    public void Configure(EntityTypeBuilder<OptionType> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.OptionTypes, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(OptionTypeConstant.Constraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(OptionTypeConstant.Constraints.PresentationMaxLength);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(OptionTypeConstant.Default.Position);

        builder.Property(x => x.Filterable)
            .IsRequired()
            .HasDefaultValue(false);
        #endregion

        #region Relationships
        builder.HasMany(x => x.OptionValues)
            .WithOne(v => v.OptionType)
            .HasForeignKey(v => v.OptionTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}