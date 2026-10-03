using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Module.Catalog.Persistence.Seeders;

public class DemoJsonHelper
{
    private readonly string _basePath;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public DemoJsonHelper(IConfiguration configuration)
    {
        _basePath = configuration.GetValue<string>("Seeders:DemoDataPath") ?? string.Empty;
    }

    public T[]? LoadIfExists<T>(string fileName)
    {
        var fullPath = Path.Combine(_basePath, fileName);
        if (!File.Exists(fullPath))
            return null;

        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T[]>(json, JsonOptions);
    }

    public string GetBasePath() => _basePath;
}
