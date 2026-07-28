namespace Module.Identity.Features.Store.Shared.Models;

public abstract record BaseTokenResponseModel
{
    public string AccessToken { get; set; } = string.Empty;
    public long AccessTokenExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public long RefreshTokenExpiresIn { get; set; }
}

public abstract record SessionResponseModel
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string[] Roles { get; init; } = [];
    public string[] Permissions { get; init; } = [];
}

public abstract record RegisterResponseModel
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}