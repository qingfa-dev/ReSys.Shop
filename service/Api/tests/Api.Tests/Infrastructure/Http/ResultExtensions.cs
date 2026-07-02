using System.Text.Json;

namespace Api.Tests.Infrastructure.Http;

public static class ResultExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<Result<T>> ReadAsResultAsync<T>(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        Result<T>? result = await response.Content
            .ReadFromJsonAsync<Result<T>>(JsonOptions, ct);

        if (!result.HasValue)
        {
            return new Result<T>(
                isSuccess: false,
                statusCode: (int)response.StatusCode,
                message: "Failed to deserialize response");
        }

        return result.Value;
    }

    public static async Task<PagedResult<T>> ReadAsPagedResultAsync<T>(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        PagedResult<T>? result = await response.Content
            .ReadFromJsonAsync<PagedResult<T>>(JsonOptions, ct);

        if (!result.HasValue)
        {
            return new PagedResult<T>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Message = "Failed to deserialize response"
            };
        }

        return result.Value;
    }
}
