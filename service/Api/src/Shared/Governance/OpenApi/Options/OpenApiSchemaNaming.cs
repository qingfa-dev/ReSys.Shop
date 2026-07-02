namespace Shared.Governance.OpenApi.Options;

internal static class OpenApiSchemaNaming
{
    internal static string GetSchemaReferenceId(Type type)
    {
        Type? underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType is not null)
        {
            return GetSchemaReferenceId(underlyingType);
        }

        if (type.Name.StartsWith("<>f", StringComparison.Ordinal))
        {
            string typeName = "AnonymousTypeOf";
            Type[] typeArgs = type.GetGenericArguments();
            string[] argNames = new string[typeArgs.Length];
            for (int i = 0; i < typeArgs.Length; i++)
            {
                argNames[i] = GetSchemaReferenceId(typeArgs[i]);
            }

            return $"{typeName}{string.Join("And", argNames)}";
        }

        if (type.IsArray)
        {
            Type elementType = type.GetElementType()!;
            return $"ArrayOf{GetSchemaReferenceId(elementType)}";
        }

        if (type.IsGenericType)
        {
            Type definition = type.GetGenericTypeDefinition();
            string name = ExtractSimpleName(definition);

            if (type.IsNested && type.DeclaringType is { } genericDeclaring)
            {
                name = GetSchemaReferenceId(genericDeclaring) + name;
            }

            Type[] args = type.GetGenericArguments();
            string[] argNames = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                argNames[i] = GetSchemaReferenceId(args[i]);
            }

            return $"{name}Of{string.Join("And", argNames)}";
        }

        if (type.IsNested)
        {
            return GetSchemaReferenceId(type.DeclaringType!) + type.Name;
        }

        return type.Name;
    }

    private static string ExtractSimpleName(Type type)
    {
        string name = type.Name;
        int index = name.IndexOf('`');
        return index > 0 ? name[..index] : name;
    }
}
