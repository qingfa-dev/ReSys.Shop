using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.IdentityModel.Tokens;

using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Helpers;

public static class IdentityTestHelper
{
    public const string ValidPassword = "TestPass1234!";

    private const string TestSecret = "integration-test-secret-key-32-chars!!";
    private const string TestIssuer = "ReSys.Shop.Test";
    private const string TestAudience = "ReSys.Shop.Test";

    private static int _counter;
    private static readonly ConcurrentDictionary<(Guid UserId, string Email), string> _userTokenCache = new();

    public static string ValidUserName(string prefix = "testuser")
    {
        int count = Interlocked.Increment(ref _counter);
        return $"{prefix}{count}";
    }

    public static string ValidEmail(string prefix = "test")
    {
        int count = Interlocked.Increment(ref _counter);
        return $"{prefix}{count}@example.com";
    }

    public static string GenerateUserToken(Guid userId, string email)
    {
        return _userTokenCache.GetOrAdd((userId, email), BuildUserToken);
    }

    private static string BuildUserToken((Guid UserId, string Email) key)
    {
        SymmetricSecurityKey securityKey = new(
            Encoding.UTF8.GetBytes(TestSecret));
        SigningCredentials credentials = new(
            securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, key.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, key.Email),
            new Claim(JwtRegisteredClaimNames.Name, "Test User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        ];

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    public static async Task<(Guid Id, string Email, string UserName)> CreateTestUserAsync(HttpClient client)
    {
        string userName = ValidUserName();
        string email = ValidEmail();

        var request = new
        {
            email,
            userName,
            firstName = "Test",
            lastName = "User",
            emailConfirmed = true,
            phoneNumberConfirmed = true
        };

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "/api/identity/users")
        {
            Content = JsonContent.Create(request)
        };
        string token = Infrastructure.Auth.AuthTokenHelper.GenerateAdminToken();
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.SendAsync(httpRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        if (!result.IsSuccess)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to create test user. Status: {response.StatusCode}, Body: {body}");
        }

        UserDetailResponse? value = result.DeserializeValue<UserDetailResponse>();
        if (value is null)
            throw new InvalidOperationException("Failed to deserialize created user");

        return (value.Id, value.Email, value.UserName);
    }

    public static async Task<(Guid Id, string Name)> CreateTestRoleAsync(HttpClient client)
    {
        string roleName = $"testrole{Guid.NewGuid():N}"[..20];

        var request = new
        {
            name = roleName,
            description = "Test role for integration tests"
        };

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "/api/identity/roles")
        {
            Content = JsonContent.Create(request)
        };
        string token = Infrastructure.Auth.AuthTokenHelper.GenerateAdminToken();
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.SendAsync(httpRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        if (!result.IsSuccess)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to create test role. Status: {response.StatusCode}, Body: {body}");
        }

        using JsonDocument doc = JsonDocument.Parse(result.ValueRaw!);
        JsonElement root = doc.RootElement;
        Guid id = root.GetProperty("id").GetGuid();

        return (id, roleName);
    }

    public static async Task<Guid> GetFirstUserIdAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync("/api/identity/users?pageSize=1");
        PagedResult<UserListResponse> result = await response.ReadAsPagedResultAsync<UserListResponse>();

        if (!result.IsSuccess || !result.Items.Any())
            throw new InvalidOperationException("No users found");

        return result.Items.First().Id;
    }

    public static async Task<Guid> GetFirstRoleIdAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync("/api/identity/roles?pageSize=1");
        PagedResult<Module.Identity.Features.Admin.Roles.Shared.Models.RoleListResponse> result =
            await response.ReadAsPagedResultAsync<Module.Identity.Features.Admin.Roles.Shared.Models.RoleListResponse>();

        if (!result.IsSuccess || !result.Items.Any())
            throw new InvalidOperationException("No roles found");

        return result.Items.First().Id;
    }

    public static HttpRequestMessage CreateAdminRequest(HttpMethod method, string requestUri, HttpContent? content = null)
    {
        string token = Infrastructure.Auth.AuthTokenHelper.GenerateAdminToken();
        HttpRequestMessage request = new(method, requestUri)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public static HttpRequestMessage CreateUserRequest(HttpMethod method, string requestUri, Guid userId, string email, HttpContent? content = null)
    {
        string token = GenerateUserToken(userId, email);
        HttpRequestMessage request = new(method, requestUri)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
