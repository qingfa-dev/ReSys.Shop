using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Promotions.Persistence.Constants;
using Module.Promotions.Domain.PromotionCategories;

namespace Module.Promotions.Persistence.Configurations.PromotionCategories;

public class PromotionCategoryConfiguration : IEntityTypeConfiguration<PromotionCategory>
{
    public void Configure(EntityTypeBuilder<PromotionCategory> builder)
    {
        builder.ToTable(PromotionsSchema.TableNames.PromotionCategories, PromotionsSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PromotionCategoryConstant.Constraints.MaxNameLength);

        builder.Property(x => x.Code)
            .HasMaxLength(PromotionCategoryConstant.Constraints.MaxCodeLength);

        builder.Property(x => x.Presentation)
            .HasMaxLength(PromotionCategoryConstant.Constraints.MaxPresentationLength);

        builder.Property(x => x.IsDeleted);
        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedBy);

        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.ModifiedAtUtc);
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.ModifiedBy);
        #endregion
    }
}
