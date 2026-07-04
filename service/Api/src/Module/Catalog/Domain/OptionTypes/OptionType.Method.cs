namespace Module.Catalog.Domain.OptionTypes;

public static class OptionTypeMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new option type (e.g. Color, Size).
    /// </summary>
    /// <param name="name">The option type name. Must not be null or empty.</param>
    /// <param name="presentation">The display presentation text.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>A Result containing the created OptionType.</returns>
    // Contract: pre=name!=null, post=entity.Id!=null&&entity.Name==name, throws=ArgumentException
    public static Result<OptionType> Create(
        string name,
        string? presentation,
        int position = 0,
        bool filterable = false,
        Guid? id = null)
    {
        var optionType = new OptionType
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Presentation = presentation,
            Position = position,
            Filterable = filterable,
        };

        return optionType;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the option type with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="optionType">The option type to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="presentation">Optional new presentation text.</param>
    /// <param name="position">Optional new position.</param>
    /// <param name="filterable">Optional new filterable flag.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this OptionType optionType,
        string? name = null,
        string? presentation = null,
        int? position = null,
        bool? filterable = null)
    {
        optionType.Name = name ?? optionType.Name;
        optionType.Presentation = presentation ?? optionType.Presentation;
        optionType.Position = position ?? optionType.Position;
        optionType.Filterable = filterable ?? optionType.Filterable;

        return Result.Ok();
    }

    /// <summary>
    /// Soft-deletes the option type.
    /// </summary>
    /// <param name="optionType">The option type to delete.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this OptionType optionType)
    {
        optionType.IsDeleted = true;
        return Result.Ok();
    }
    #endregion
}