namespace Api.Tests.Infrastructure.Http;

public static class HttpClientExtensions
{
    public static async Task<Result<T>> GetAsResultAsync<T>(
        this HttpClient client,
        string requestUri,
        CancellationToken ct = default)
    {
        var response = await client.GetAsync(requestUri, ct);
        return await response.ReadAsResultAsync<T>(ct);
    }

    public static async Task<Result<T>> PostAsResultAsync<T>(
        this HttpClient client,
        string requestUri,
        object? body = null,
        CancellationToken ct = default)
    {
        var response = body is null
            ? await client.PostAsync(requestUri, null, ct)
            : await client.PostAsJsonAsync(requestUri, body, ct);
        return await response.ReadAsResultAsync<T>(ct);
    }

    public static async Task<Result<T>> PutAsResultAsync<T>(
        this HttpClient client,
        string requestUri,
        object? body = null,
        CancellationToken ct = default)
    {
        var response = body is null
            ? await client.PutAsync(requestUri, null, ct)
            : await client.PutAsJsonAsync(requestUri, body, ct);
        return await response.ReadAsResultAsync<T>(ct);
    }

    public static async Task<Result<T>> DeleteAsResultAsync<T>(
        this HttpClient client,
        string requestUri,
        CancellationToken ct = default)
    {
        var response = await client.DeleteAsync(requestUri, ct);
        return await response.ReadAsResultAsync<T>(ct);
    }
}
