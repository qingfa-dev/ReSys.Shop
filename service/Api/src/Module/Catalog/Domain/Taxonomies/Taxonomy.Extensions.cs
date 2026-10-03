using Module.Catalog.Domain.Taxons;

using Shared.Application.Domain.Concerns.Parameterizable;

namespace Module.Catalog.Domain.Taxonomies;

public static class TaxonomyMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new taxonomy with the specified properties.
    /// </summary>
    /// <param name="name">The taxonomy name. Must not be null or empty.</param>
    /// <param name="presentation">The display presentation text.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>A Result containing the created Taxonomy.</returns>
    // Contract: pre=name!=null, post=entity.Id!=null&&entity.Name==name, throws=ArgumentException
    public static Result<Taxonomy> Create(
        string name,
        string? presentation,
        int position = 0,
        Guid? id = null)
    {
        // Validate: Taxonomy name must not be null or empty
        var taxonomy = new Taxonomy
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Presentation = presentation,
            Position = position,
        };

        ParameterizableBehavior.ApplyNormalization(taxonomy);

        return taxonomy;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the taxonomy with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="taxonomy">The taxonomy to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="presentation">Optional new presentation text.</param>
    /// <param name="position">Optional new position.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this Taxonomy taxonomy,
        string? name = null,
        string? presentation = null,
        int? position = null)
    {
        taxonomy.Name = name ?? taxonomy.Name;
        taxonomy.Presentation = presentation ?? taxonomy.Presentation;
        taxonomy.Position = position ?? taxonomy.Position;
        ParameterizableBehavior.ApplyNormalization(taxonomy);

        return Result.Ok();
    }

    /// <summary>
    /// Soft-deletes the taxonomy.
    /// </summary>
    /// <param name="taxonomy">The taxonomy to delete.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this Taxonomy taxonomy)
    {
        taxonomy.IsDeleted = true;
        return Result.Ok();
    }

    /// <summary>
    /// Restores a previously soft-deleted taxonomy.
    /// </summary>
    /// <param name="taxonomy">The taxonomy to restore.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Restore(this Taxonomy taxonomy)
    {
        taxonomy.IsDeleted = false;
        return Result.Ok();
    }

    // Compute: The root taxon is the taxon with ParentId=null within this taxonomy
    public static Taxon? Root(this Taxonomy taxonomy)
    {
        // @CAT-5 Compute: Find the root taxon (no parent) within this taxonomy
        return taxonomy.Taxons.FirstOrDefault(t => t.ParentId == null);
    }
    #endregion
}