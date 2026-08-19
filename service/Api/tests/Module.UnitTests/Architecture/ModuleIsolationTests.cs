using System.Reflection;

using Module.Catalog;

namespace Module.UnitTests.Architecture;

[Trait("Category", "Unit")]
[Trait("Module", "Architecture")]
public sealed class ModuleIsolationTests
{
    private static readonly Assembly ModuleAssembly = typeof(CatalogExtensions).Assembly;

    [Fact]
    public void SharedInfrastructure_ShouldNotReferenceModuleTypes()
    {
        var sharedAssembly = typeof(Result).Assembly;
        var moduleTypes = ModuleAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("Module.") == true)
            .ToHashSet();

        var violations = sharedAssembly.GetTypes()
            .AsParallel()
            .Where(t => t.Namespace?.StartsWith("Shared.") == true)
            .SelectMany(t => GetReferencedTypes(t))
            .Where(moduleTypes.Contains)
            .Select(r => $"Shared type references Module type: {r.FullName}")
            .Distinct()
            .ToList();

        violations.Should().BeEmpty(
            "Shared must not reference Module types. Dependencies flow forward only. " +
            $"Violations:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void FeatureHandlers_ShouldReturnResultTypes()
    {
        var handlerTypes = ModuleAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && (
                    i.GetGenericTypeDefinition().Name.Contains("ICommandHandler") ||
                    i.GetGenericTypeDefinition().Name.Contains("IQueryHandler"))))
            .ToList();

        var violations = handlerTypes
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.Name == "Handle")
            .Where(m => m.ReturnType.IsGenericType &&
                        m.ReturnType.GetGenericTypeDefinition() != typeof(Task<>) &&
                        !m.ReturnType.GetGenericArguments().Any(a =>
                            a.IsGenericType && a.GetGenericTypeDefinition().Name.Contains("Result")))
            .Select(m => $"{m.DeclaringType?.FullName}.Handle does not return Result<T>")
            .ToList();

        violations.Should().BeEmpty(
            "All feature handlers must return Result<T>. " +
            $"Violations:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void DomainNamespaces_ShouldNotDefineExceptionClasses()
    {
        var exceptionTypes = ModuleAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Domain") == true)
            .Where(t => t.IsSubclassOf(typeof(Exception)))
            .Select(t => t.FullName!)
            .ToList();

        exceptionTypes.Should().BeEmpty(
            "Domain entities should not define custom exception classes — use Result<T> pattern instead. " +
            $"Found:{Environment.NewLine}{string.Join(Environment.NewLine, exceptionTypes)}");
    }

    private static HashSet<Type> GetReferencedTypes(Type type)
    {
        var refs = new HashSet<Type>();

        refs.Add(type.BaseType!);
        refs.UnionWith(type.GetInterfaces());

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            if (field.FieldType.IsGenericType)
                refs.UnionWith(field.FieldType.GetGenericArguments());
            else
                refs.Add(field.FieldType);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            if (prop.PropertyType.IsGenericType)
                refs.UnionWith(prop.PropertyType.GetGenericArguments());
            else
                refs.Add(prop.PropertyType);

        foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            foreach (var param in ctor.GetParameters())
                if (param.ParameterType.IsGenericType)
                    refs.UnionWith(param.ParameterType.GetGenericArguments());
                else
                    refs.Add(param.ParameterType);

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            refs.Add(method.ReturnType);
            if (method.ReturnType.IsGenericType)
                refs.UnionWith(method.ReturnType.GetGenericArguments());

            foreach (var param in method.GetParameters())
                if (param.ParameterType.IsGenericType)
                    refs.UnionWith(param.ParameterType.GetGenericArguments());
                else
                    refs.Add(param.ParameterType);
        }

        refs.Remove(null!);
        return refs;
    }
}
