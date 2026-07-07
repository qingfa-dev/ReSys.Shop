using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Promotions.Persistence.Constants;
using Module.Promotions.Domain.OrderPromotions;

namespace Module.Promotions.Persistence.Configurations.OrderPromotions;

public class OrderPromotionConfiguration : IEntityTypeConfiguration<OrderPromotion>
{
    public void Configure(EntityTypeBuilder<OrderPromotion> builder)
    {
        builder.ToTable(PromotionsSchema.TableNames.OrderPromotions, PromotionsSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        builder.Property(x => x.OrderId);
        builder.Property(x => x.PromotionId);
        builder.Property(x => x.PromotionCodeId);
        #endregion
    }
}
