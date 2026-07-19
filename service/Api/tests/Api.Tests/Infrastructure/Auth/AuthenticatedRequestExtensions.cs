using System.Net.Http.Headers;

namespace Api.Tests.Infrastructure.Auth;

public static class AuthenticatedRequestExtensions
{
    private static HttpRequestMessage CreateAdminRequest(HttpMethod method, string requestUri, HttpContent? content = null)
    {
        string token = AuthTokenHelper.GenerateAdminToken();
        HttpRequestMessage request = new(method, requestUri)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public static async Task<Result<T>> PostAsAdminResultAsync<T>(
        this HttpClient client,
        string requestUri,
        object? body = null)
    {
        HttpContent? content = body is not null
            ? JsonContent.Create(body)
            : null;

        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Post, requestUri, content);
        HttpResponseMessage response = await client.SendAsync(request);
        return await response.ReadAsResultAsync<T>();
    }

    public static async Task<Result<T>> DeleteAsAdminResultAsync<T>(
        this HttpClient client,
        string requestUri)
    {
        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Delete, requestUri);
        HttpResponseMessage response = await client.SendAsync(request);
        return await response.ReadAsResultAsync<T>();
    }

    public static async Task<HttpResponseMessage> PostAsAdminRawAsync(
        this HttpClient client,
        string requestUri,
        object? body = null)
    {
        HttpContent? content = body is not null
            ? JsonContent.Create(body)
            : null;

        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Post, requestUri, content);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PatchAsAdminRawAsync(
        this HttpClient client,
        string requestUri,
        object? body = null)
    {
        HttpContent? content = body is not null
            ? JsonContent.Create(body)
            : null;

        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Patch, requestUri, content);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> DeleteAsAdminRawAsync(
        this HttpClient client,
        string requestUri)
    {
        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Delete, requestUri);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> GetAsAdminRawAsync(
        this HttpClient client,
        string requestUri)
    {
        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Get, requestUri);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PutAsAdminRawAsync(
        this HttpClient client,
        string requestUri,
        object? body = null)
    {
        HttpContent? content = body is not null
            ? JsonContent.Create(body)
            : null;

        using HttpRequestMessage request = CreateAdminRequest(HttpMethod.Put, requestUri, content);
        return await client.SendAsync(request);
    }
}
