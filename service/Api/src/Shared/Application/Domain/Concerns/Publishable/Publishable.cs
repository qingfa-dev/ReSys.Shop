namespace Shared.Application.Domain.Concerns.Publishable;

public interface IPublishable
{
    bool IsPublished { get; set; }
    DateTimeOffset? PublishedAtUtc { get; set; }
}
