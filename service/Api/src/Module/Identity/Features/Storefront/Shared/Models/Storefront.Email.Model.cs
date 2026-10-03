namespace Module.Identity.Features.Storefront.Shared.Models;

public abstract record EmailParameters
{
    public string Email { get; init; } = string.Empty;
}

public record EmailRequest : EmailParameters;

public record EmailDetailResponse : EmailParameters
{
    public string? Message { get; init; }
}
