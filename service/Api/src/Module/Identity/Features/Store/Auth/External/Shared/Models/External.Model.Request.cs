namespace Module.Identity.Features.Store.Auth.External.Shared.Models;

public abstract record BaseExternalLoginRequest(string Provider = "", string IdToken = "");
