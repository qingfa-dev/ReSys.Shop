using BuildingBlocks.Persistence.Configurations.Dictionaries;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Promotions.Persistence.Constants;
using Module.Promotions.Domain.PromotionActions;

namespace Module.Promotions.Persistence.Configurations.PromotionActions;

public class PromotionActionConfiguration : IEntityTypeConfiguration<PromotionAction>
{
    public void Configure(EntityTypeBuilder<PromotionAction> builder)
    {
        builder.ToTable(PromotionsSchema.TableNames.PromotionActions, PromotionsSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(PromotionActionConstant.Constraints.MaxTypeLength);

        builder.Property(x => x.Preferences).HasConversion<DictionaryValueConverter<string, string>>();

        builder.Property(x => x.CalculatorType)
            .HasMaxLength(PromotionActionConstant.Constraints.MaxCalculatorTypeLength);

        builder.Property(x => x.PromotionId);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(p => p.PromotionActions)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Properties (Auditing)
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.ModifiedAtUtc);
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.ModifiedBy);
        #endregion
    }
}
