public interface IIdentitySchemaProvider
{
    string SchemaName { get; }
    string GetTableName(string entityName);
}