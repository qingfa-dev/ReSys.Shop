using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Promotions.Persistence.Constants;
using Module.Promotions.Domain.Promotions;

namespace Module.Promotions.Persistence.Configurations.Promotions;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable(PromotionsSchema.TableNames.Promotions, PromotionsSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PromotionConstant.Constraints.MaxNameLength);

        builder.Property(x => x.Code)
            .HasMaxLength(PromotionConstant.Constraints.MaxCodeLength);

        builder.Property(x => x.Description)
            .HasMaxLength(PromotionConstant.Constraints.MaxDescriptionLength);

        builder.Property(x => x.UsageLimit);
        builder.Property(x => x.PerCustomerUsageLimit);
        builder.Property(x => x.StartsAtUtc);
        builder.Property(x => x.ExpiresAtUtc);

        builder.Property(x => x.MatchPolicy)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PromotionConstant.Defaults.MatchPolicy);

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PromotionConstant.Defaults.Kind);

        builder.Property(x => x.Advertise)
            .HasDefaultValue(PromotionConstant.Defaults.Advertise);

        builder.Property(x => x.Active)
            .HasDefaultValue(PromotionConstant.Defaults.Active);

        builder.Property(x => x.Position)
            .HasDefaultValue(PromotionConstant.Defaults.Position);

        builder.Property(x => x.Path)
            .HasMaxLength(PromotionConstant.Constraints.MaxPathLength);

        builder.Property(x => x.IsDeleted);
        builder.Property(x => x.DeletedAtUtc);
        builder.Property(x => x.DeletedBy);

        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.ModifiedAtUtc);
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.ModifiedBy);
        #endregion

        #region Relationships
        builder.HasOne(x => x.PromotionCategory)
            .WithMany(pc => pc.Promotions)
            .HasForeignKey(x => x.PromotionCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        #endregion
    }
}
