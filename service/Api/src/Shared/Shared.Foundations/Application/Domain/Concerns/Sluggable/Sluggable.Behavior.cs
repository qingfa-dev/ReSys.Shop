using Slugify;

namespace Shared.Application.Domain.Concerns.Sluggable;

/// <summary>
/// Provides shared behaviors for sluggable entities.
/// </summary>
public static class SluggableBehavior
{
    private static readonly SlugHelper _slugHelper = new();

    /// <summary>
    /// Generates and assigns a slug to the entity based on the provided source text.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="source">The text to slugify (e.g., a Name or Title).</param>
    public static void ApplySlugging(ISluggable entity, string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        entity.Slug = _slugHelper.GenerateSlug(source);
    }

    /// <summary>
    /// Generates a slug from the given source text.
    /// </summary>
    /// <param name="source">The text to slugify.</param>
    /// <returns>A slugified version of the source text.</returns>
    public static string GenerateSlug(string source)
    {
        return _slugHelper.GenerateSlug(source);
    }
}
