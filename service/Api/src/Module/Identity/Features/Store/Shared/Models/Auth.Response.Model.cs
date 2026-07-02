namespace Module.Identity.Features.Store.Shared.Models;

public abstract record BaseTokenResponseModel
{
    public string AccessToken { get; set; } = string.Empty;
    public long AccessTokenExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public long RefreshTokenExpiresIn { get; set; }
}
