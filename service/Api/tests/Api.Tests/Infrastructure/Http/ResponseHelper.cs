using System.Net;
using System.Text.Json;

namespace Api.Tests.Infrastructure.Http;

public readonly record struct ApiResponse
{
    public HttpStatusCode StatusCode { get; init; }
    public bool IsSuccess { get; init; }
    public string? ValueRaw { get; init; }
    public int StatusCodeInt { get; init; }

    public static async Task<ApiResponse> FromAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        int statusCode = (int)response.StatusCode;

        if (string.IsNullOrWhiteSpace(body))
        {
            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                StatusCodeInt = statusCode
            };
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            bool isSuccess = root.TryGetProperty("isSuccess", out JsonElement isSuccessEl)
                && isSuccessEl.GetBoolean();

            string? valueRaw = root.TryGetProperty("value", out JsonElement valueEl)
                ? valueEl.GetRawText()
                : null;

            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                IsSuccess = isSuccess,
                ValueRaw = valueRaw,
                StatusCodeInt = statusCode
            };
        }
        catch
        {
            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                StatusCodeInt = statusCode
            };
        }
    }
}

internal static class JsonOptionCache
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}

public static class ResponseHelperExtensions
{
    public static async Task<ApiResponse> ReadApiResponseAsync(this HttpResponseMessage response)
        => await ApiResponse.FromAsync(response);

    public static T? DeserializeValue<T>(this ApiResponse apiResponse)
        where T : class
    {
        if (apiResponse.ValueRaw is null)
            return null;

        return JsonSerializer.Deserialize<T>(apiResponse.ValueRaw, JsonOptionCache.Default);
    }
}
