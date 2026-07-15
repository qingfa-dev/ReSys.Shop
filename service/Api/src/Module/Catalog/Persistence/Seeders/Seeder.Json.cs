using System.Text.Json;

namespace Module.Catalog.Persistence.Seeders;

public static class DemoJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static T[]? LoadIfExists<T>(string fileName)
    {
        var basePath = AppContext.BaseDirectory;
        var fullPath = Path.Combine(basePath, "Seeders", "Data", fileName);
        if (!File.Exists(fullPath))
            return null;

        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T[]>(json, JsonOptions);
    }

    public static string ResolveDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Seeders", "Data", fileName);
    }
}
