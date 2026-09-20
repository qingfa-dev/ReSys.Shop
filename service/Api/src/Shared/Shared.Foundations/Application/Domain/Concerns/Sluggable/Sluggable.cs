namespace Shared.Application.Domain.Concerns.Sluggable;

/// <summary>
/// Defines an entity that has a slug for SEO-friendly URLs.
/// </summary>
public interface ISluggable
{
    /// <summary>
    /// Gets or sets the unique slug for the entity.
    /// </summary>
    string Slug { get; set; }
}
