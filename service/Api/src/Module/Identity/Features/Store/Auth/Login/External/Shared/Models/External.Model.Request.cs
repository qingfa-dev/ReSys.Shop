namespace Module.Identity.Features.Store.Auth.Login.External.Shared.Models;

public abstract record BaseExternalLoginRequest(string Provider = "", string IdToken = "");