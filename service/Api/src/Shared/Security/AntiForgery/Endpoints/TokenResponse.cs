namespace Shared.Security.AntiForgery.Endpoints;

public sealed record TokenResponse(string Token, string HeaderName);
