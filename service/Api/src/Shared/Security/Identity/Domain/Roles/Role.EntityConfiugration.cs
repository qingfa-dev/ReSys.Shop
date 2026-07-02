using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Security.Identity.Domain.Shared;

namespace Shared.Security.Identity.Domain.Roles;

public sealed class RolEntityConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Table
        builder.ToTable(IdentitySchema.TableNames.Roles, IdentitySchema.Name);

        // Key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Description)
            .HasMaxLength(RoleConstant.Constraints.Description.MaxLength);

        // Relationships
        // Each Role can have many entries in the UserRole join table
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        // Each Role can have many associated RoleClaims
        builder.HasMany(r => r.RoleClaims)
            .WithOne(rc => rc.Role)
            .HasForeignKey(rc => rc.RoleId)
            .IsRequired();
    }
}
