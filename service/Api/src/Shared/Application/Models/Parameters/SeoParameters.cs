namespace Shared.Application.Models.Parameters;

public interface ISeoParameters
{
    string? MetaTitle { get; init; }
    string? MetaDescription { get; init; }
    string? MetaKeywords { get; init; }
}
