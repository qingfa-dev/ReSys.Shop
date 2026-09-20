using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Pgvector;

namespace Shared.Operational.Persistence.Configurations.Vectors;

/// <summary>
/// Converts Pgvector Vector values to and from JSON strings for database storage.
/// Useful for providers that do not natively support vector types.
/// </summary>
public class VectorValueConverter()
    : ValueConverter<Vector, string>(
        // Transform: Serialize Vector to JSON array for storage
        v => JsonSerializer.Serialize(v.ToArray(), (JsonSerializerOptions?)null),
        // Transform: Deserialize JSON array back to Vector
        v => new Vector(JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<float>()));

/// <summary>
/// Converts nullable Pgvector Vector values to and from JSON strings for database storage.
/// </summary>
public class NullableVectorValueConverter()
    : ValueConverter<Vector?, string?>(
        // Transform: Serialize nullable Vector to JSON array
        v => v == null ? null : JsonSerializer.Serialize(v.ToArray(), (JsonSerializerOptions?)null),
        // Transform: Deserialize nullable JSON array back to Vector
        v => string.IsNullOrEmpty(v)
            ? null
            : new Vector(JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<float>()));
