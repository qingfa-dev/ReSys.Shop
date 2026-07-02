using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Operational.Persistence.Configurations.Enums;

/// <summary>
/// Converts enum values to and from string values for database storage.
/// </summary>
/// <typeparam name="TEnum">The type of the enum.</typeparam>
public class CustomEnumToStringConverter<TEnum>() : ValueConverter<TEnum, string>(
    v => v.ToString(),
    v => Enum.Parse<TEnum>(v, true))
where TEnum : struct, Enum;

/// <summary>
/// Converts nullable enum values to and from nullable string values for database storage.
/// </summary>
/// <typeparam name="TEnum">The type of the enum.</typeparam>
public class CustomNullableEnumToStringConverter<TEnum>() : ValueConverter<TEnum?, string?>(
    v => v == null ? null : v.ToString(),
    v => string.IsNullOrEmpty(v) ? null : Enum.Parse<TEnum>(v, true))
where TEnum : struct, Enum;
