namespace Module.Catalog.Domain.OptionTypes.Values;

public static class OptionValueMethod
{
    #region Factory Methods
    /// <summary>
    /// Creates a new option value for an option type.
    /// </summary>
    /// <param name="optionTypeId">The parent option type identifier.</param>
    /// <param name="name">The option value name. Must not be null or empty.</param>
    /// <param name="presentation">The display presentation text.</param>
    /// <param name="position">Display order position. Defaults to 0.</param>
    /// <param name="id">Optional explicit identifier. Auto-generated if not provided.</param>
    /// <returns>A Result containing the created OptionValue.</returns>
    // Contract: pre=optionTypeId!=Guid.Empty&&name!=null,
    //           post=entity.Id!=null&&entity.Name==name&&entity.OptionTypeId==optionTypeId, throws=ArgumentException
    public static Result<OptionValue> Create(
        Guid optionTypeId,
        string name,
        string presentation,
        int position = 0,
        Guid? id = null)
    {
        var entity = new OptionValue
        {
            Id = id ?? Guid.NewGuid(),
            OptionTypeId = optionTypeId,
            Name = name,
            Presentation = presentation,
            Position = position
        };

        return entity;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Updates the option value with the specified properties. Only non-null values are applied.
    /// </summary>
    /// <param name="optionValue">The option value to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="presentation">Optional new presentation text.</param>
    /// <param name="position">Optional new position.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this OptionValue optionValue,
        string? name = null,
        string? presentation = null,
        int? position = null)
    {
        optionValue.Name = name ?? optionValue.Name;
        optionValue.Presentation = presentation ?? optionValue.Presentation;
        optionValue.Position = position ?? optionValue.Position;

        return Result.Ok();
    }
    #endregion
}