using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Persistence.Configurations.Adjustments;

// @CAT-10 Boundary: Persistence → Domain — reserved for EF Core materialization; do not add domain logic
public class AdjustmentConfiguration : IEntityTypeConfiguration<Adjustment>
{
    public void Configure(EntityTypeBuilder<Adjustment> builder)
    {
        builder.ToTable(OrderingSchema.TableNames.Adjustments, OrderingSchema.Name);

        builder.HasKey(x => x.Id);

        #region Properties
        // Initialize: Monetary precision matches AdjustmentConstant; type discriminators bounded to MaxTypeStrings
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(AdjustmentConstant.Constraints.MonetaryPrecision, AdjustmentConstant.Constraints.MonetaryScale);

        builder.Property(x => x.DisplayAmount)
            .HasMaxLength(AdjustmentConstant.Constraints.MaxDisplayAmountLength);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(AdjustmentConstant.Constraints.MaxLabelLength);

        builder.Property(x => x.Eligible)
            .HasDefaultValue(AdjustmentConstant.Defaults.Eligible);

        builder.Property(x => x.Included)
            .HasDefaultValue(AdjustmentConstant.Defaults.Included);

        builder.Property(x => x.Mandatory)
            .HasDefaultValue(AdjustmentConstant.Defaults.Mandatory);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(AdjustmentConstant.Constraints.MaxStateLength)
            .HasDefaultValue(AdjustmentConstant.Defaults.State);

        builder.Property(x => x.AdjustableType)
            .IsRequired()
            .HasMaxLength(AdjustmentConstant.Constraints.MaxTypeStrings);

        builder.Property(x => x.AdjustableId).IsRequired();

        builder.Property(x => x.SourceType)
            .HasMaxLength(AdjustmentConstant.Constraints.MaxTypeStrings);

        builder.Property(x => x.SourceId);

        builder.Property(x => x.OrderId).IsRequired();
        #endregion

        #region Relationships
        builder.HasOne(x => x.Order)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
    }
}