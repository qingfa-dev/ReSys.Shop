using BuildingBlocks.Persistence.Configurations.Dictionaries;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Promotions.Persistence.Constants;
using Module.Promotions.Domain.PromotionRules;

namespace Module.Promotions.Persistence.Configurations.PromotionRules;

public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable(PromotionsSchema.TableNames.PromotionRules, PromotionsSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(PromotionRuleConstant.Constraints.MaxTypeLength);

        builder.Property(x => x.Preferences)
            .HasConversion<DictionaryValueConverter<string, string>>();

        builder.Property(x => x.PromotionId);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(p => p.PromotionRules)
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
