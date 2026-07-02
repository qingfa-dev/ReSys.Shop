using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shared.Security.Identity.Domain.Shared;

namespace Shared.Security.Identity.Domain.Users;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table
        builder.ToTable(IdentitySchema.TableNames.Users, IdentitySchema.Name);
    }
}
