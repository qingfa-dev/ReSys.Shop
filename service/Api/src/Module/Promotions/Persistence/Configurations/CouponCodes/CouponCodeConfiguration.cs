using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Promotions.Persistence.Constants;
using Module.Promotions.Domain.CouponCodes;

namespace Module.Promotions.Persistence.Configurations.CouponCodes;

public class CouponCodeConfiguration : IEntityTypeConfiguration<CouponCode>
{
    public void Configure(EntityTypeBuilder<CouponCode> builder)
    {
        builder.ToTable(PromotionsSchema.TableNames.CouponCodes, PromotionsSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(CouponCodeConstant.Constraints.MaxCodeLength);

        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(CouponCodeConstant.Defaults.State);

        builder.Property(x => x.OrderId);
        builder.Property(x => x.RedeemedAtUtc);

        builder.Property(x => x.PromotionId);
        #endregion

        #region Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(p => p.CouponCodes)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Properties (Auditing)
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.ModifiedAtUtc);
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.ModifiedBy);
        #endregion

        #region Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        #endregion
    }
}
