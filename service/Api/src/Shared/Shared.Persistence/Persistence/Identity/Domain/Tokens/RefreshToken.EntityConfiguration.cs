using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Security.Identity.Domain.Shared;

namespace Shared.Security.Identity.Domain.Tokens;

public sealed class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Tables
        builder.ToTable(IdentitySchema.TableNames.RefreshTokens, IdentitySchema.Name);

        // Keys
        builder.HasKey(t => t.Id);

        // Properites
        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(RefreshTokenConstant.Constraints.TokenHash.MaxLength);

        // Relationships
        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.TokenFamilyId);

        // Indexes
        builder.HasIndex(t => t.TokenFamilyId);
    }
}
