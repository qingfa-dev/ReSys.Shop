using Shared.Security.Identity.Domain.Shared;

namespace Shared.Security.Identity;

public sealed class IdentitySchemaProvider : IIdentitySchemaProvider
{
    public string SchemaName => IdentitySchema.Name;

    public string GetTableName(string entityName) => entityName.ToLowerInvariant();
}