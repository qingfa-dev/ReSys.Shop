using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Operational.Persistence.Configurations.Dictionaries;

public class DictionaryValueConverter<TKey, TValue> : ValueConverter<Dictionary<TKey, TValue>, string>
    where TKey : notnull
{
    public DictionaryValueConverter()
        : base(
            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null)!,
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<TKey, TValue>())
    {
    }
}
