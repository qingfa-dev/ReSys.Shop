using System.Reflection;
using System.Runtime.Serialization;

namespace Shared.Governance.Conventions;

public static class EnumExtensions
{
    public static IReadOnlyList<string> GetValues<TEnum>() where TEnum : Enum
    {
        return typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f =>
            {
                EnumMemberAttribute? attr = f.GetCustomAttribute<EnumMemberAttribute>();
                return attr?.Value ?? f.Name;
            })
            .ToList();
    }

    public static TEnum FromEnumMemberValue<TEnum>(string value) where TEnum : struct, Enum
    {
        foreach (FieldInfo field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            EnumMemberAttribute? attr = field.GetCustomAttribute<EnumMemberAttribute>();
            var enumName = attr?.Value ?? field.Name;
            if (enumName == value)
                return (TEnum)field.GetValue(null)!;
        }

        throw new ArgumentException($"Requested value '{value}' was not found in enum {typeof(TEnum).Name}.", nameof(value));
    }

    public static string ToEnumMemberValue<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        FieldInfo? field = typeof(TEnum).GetField(value.ToString()!);
        EnumMemberAttribute? attr = field?.GetCustomAttribute<EnumMemberAttribute>();
        return attr?.Value ?? value.ToString()!;
    }
}