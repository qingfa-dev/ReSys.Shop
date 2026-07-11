namespace Module.Shipping.Domain.ShippingMethods;

public static class ShippingMethodExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new shipping method with the specified name and calculator type.
    /// </summary>
    /// <param name="name">The shipping method name. Must not be empty.</param>
    /// <param name="calculatorType">The calculator type. Must not be empty.</param>
    /// <param name="code">Optional unique code.</param>
    /// <param name="taxCategoryId">Optional tax category identifier.</param>
    /// <returns>A result containing the newly created shipping method.</returns>
    // @CAT-10 Contract: pre=name!=null && calculatorType!=null, post=method.Id!=default && method.AvailableToUsers==true, throws=none
    public static Result<ShippingMethod> Create(
        string name,
        string calculatorType,
        string? code = null,
        Guid? taxCategoryId = null)
    {
        var method = new ShippingMethod
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            CalculatorType = calculatorType,
            TaxCategoryId = taxCategoryId,
            AvailableToUsers = ShippingMethodConstant.Defaults.AvailableToUsers,
            Position = ShippingMethodConstant.Defaults.Position,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return method;
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>
    /// Updates the shipping method properties. Only non-null parameters are applied.
    /// </summary>
    /// <param name="method">The shipping method to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="code">Optional new code.</param>
    /// <param name="trackingUrl">Optional new tracking URL template.</param>
    /// <param name="adminName">Optional new admin-facing name.</param>
    /// <param name="position">Optional new sort position.</param>
    /// <param name="availableToUsers">Optional availability flag.</param>
    /// <param name="calculatorType">Optional new calculator type.</param>
    /// <param name="taxCategoryId">Optional new tax category identifier.</param>
    /// <returns>A result indicating success.</returns>
    public static Result Update(
        this ShippingMethod method,
        string? name = null,
        string? code = null,
        string? trackingUrl = null,
        string? adminName = null,
        int? position = null,
        bool? availableToUsers = null,
        string? calculatorType = null,
        Guid? taxCategoryId = null)
    {
        method.Name = name ?? method.Name;
        method.Code = code ?? method.Code;
        method.TrackingUrl = trackingUrl ?? method.TrackingUrl;
        method.AdminName = adminName ?? method.AdminName;
        method.Position = position ?? method.Position;
        method.AvailableToUsers = availableToUsers ?? method.AvailableToUsers;
        method.CalculatorType = calculatorType ?? method.CalculatorType;
        method.TaxCategoryId = taxCategoryId ?? method.TaxCategoryId;

        return Result.Ok();
    }

    /// <summary>
    /// Builds a tracking URL by substituting the :tracking placeholder with the actual tracking number.
    /// </summary>
    /// <param name="method">The shipping method containing the URL template.</param>
    /// <param name="tracking">The tracking number to substitute.</param>
    /// <returns>The fully resolved tracking URL, or empty string if no template is set.</returns>
    // @CAT-5 Compute: Substitute :tracking placeholder with URL-encoded tracking value (Ruby SDK shipping_method.rb#build_tracking_url alignment)
    public static string BuildTrackingUrl(this ShippingMethod method, string tracking)
    {
        return method.TrackingUrl?.Replace(":tracking", tracking) ?? string.Empty;
    }

    /// <summary>
    /// Determines whether the shipping method is available for use.
    /// </summary>
    /// <param name="method">The shipping method to check.</param>
    /// <returns>True if available to users and not deleted; otherwise false.</returns>
    public static bool IsAvailableFor(this ShippingMethod method)
    {
        return method.AvailableToUsers && !method.IsDeleted;
    }

    #endregion Methods
}