namespace Module.Identity.Features.Store.Shared.Models;

public abstract record BasePasswordLoginRequest(string Credential = "", string Password = "");

public abstract record BaseRefreshTokenRequestModel(string? RefreshToken = null);

public abstract record BaseLogOutRequest(string? RefreshToken = null, bool RevokeAll = false) : BaseRefreshTokenRequestModel(RefreshToken);