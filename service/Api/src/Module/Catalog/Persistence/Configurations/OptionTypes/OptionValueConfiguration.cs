using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Configurations.OptionTypes;

public class OptionValueConfiguration : IEntityTypeConfiguration<OptionValue>
{
    public void Configure(EntityTypeBuilder<OptionValue> builder)
    {
        builder.ToTable(CatalogSchema.TableNames.OptionValues, CatalogSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(OptionValueConstant.Constraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(OptionValueConstant.Constraints.PresentationMaxLength);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasDefaultValue(OptionValueConstant.Default.Position);
        #endregion

        #region Relationships
        builder.HasOne(x => x.OptionType)
            .WithMany(t => t.OptionValues)
            .HasForeignKey(x => x.OptionTypeId);
        #endregion
    }
}
