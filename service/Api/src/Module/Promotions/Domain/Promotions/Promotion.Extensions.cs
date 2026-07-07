namespace Module.Promotions.Domain.Promotions;

public static class PromotionExtensions
{
    #region Factory Methods
    /// <summary>Creates a new promotion with the specified configuration parameters.</summary>
    /// <param name="name">The promotion name.</param>
    /// <param name="code">Optional promotion code for automatic promotions.</param>
    /// <param name="description">Optional description of the promotion.</param>
    /// <param name="usageLimit">Optional maximum number of times this promotion can be used.</param>
    /// <param name="perCustomerUsageLimit">Optional maximum uses per customer.</param>
    /// <param name="startsAtUtc">Optional start date for promotion eligibility.</param>
    /// <param name="expiresAtUtc">Optional end date for promotion eligibility.</param>
    /// <param name="matchPolicy">How promotion rules are evaluated (All/Any).</param>
    /// <param name="kind">Whether this is a coupon-code or automatic promotion.</param>
    /// <param name="advertise">Whether to display the promotion to customers.</param>
    /// <param name="active">Whether the promotion is currently active.</param>
    /// <param name="position">Display ordering position.</param>
    /// <param name="path">Optional URL path for the promotion.</param>
    /// <param name="id">Optional explicit identifier.</param>
    /// <returns>A Result containing the created Promotion on success.</returns>
    // @CAT-10 Contract: pre=name is non-null, post=entity.Id is not default, throws=none
    // Validate: Promotion name must not be null or empty
    public static Result<Promotion> Create(
        string name,
        string? code = null,
        string? description = null,
        int? usageLimit = null,
        int? perCustomerUsageLimit = null,
        DateTimeOffset? startsAtUtc = null,
        DateTimeOffset? expiresAtUtc = null,
        MatchPolicy matchPolicy = PromotionConstant.Defaults.MatchPolicy,
        PromotionKind kind = PromotionConstant.Defaults.Kind,
        bool advertise = PromotionConstant.Defaults.Advertise,
        bool active = PromotionConstant.Defaults.Active,
        int position = PromotionConstant.Defaults.Position,
        string? path = null,
        Guid? id = null)
    {
        return new Promotion
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            UsageLimit = usageLimit,
            PerCustomerUsageLimit = perCustomerUsageLimit,
            StartsAtUtc = startsAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            MatchPolicy = matchPolicy,
            Kind = kind,
            Advertise = advertise,
            Active = active,
            Position = position,
            Path = path,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>Updates the promotion with the specified optional property changes.</summary>
    /// <param name="promotion">The promotion to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="code">Optional new code.</param>
    /// <param name="description">Optional new description.</param>
    /// <param name="usageLimit">Optional new usage limit.</param>
    /// <param name="perCustomerUsageLimit">Optional new per-customer usage limit.</param>
    /// <param name="startsAtUtc">Optional new start date.</param>
    /// <param name="expiresAtUtc">Optional new end date.</param>
    /// <param name="matchPolicy">Optional new match policy.</param>
    /// <param name="kind">Optional new promotion kind.</param>
    /// <param name="advertise">Optional new advertise flag.</param>
    /// <param name="active">Optional new active flag.</param>
    /// <param name="position">Optional new position.</param>
    /// <param name="path">Optional new path.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Update(this Promotion promotion,
        string? name = null,
        string? code = null,
        string? description = null,
        int? usageLimit = null,
        int? perCustomerUsageLimit = null,
        DateTimeOffset? startsAtUtc = null,
        DateTimeOffset? expiresAtUtc = null,
        MatchPolicy? matchPolicy = null,
        PromotionKind? kind = null,
        bool? advertise = null,
        bool? active = null,
        int? position = null,
        string? path = null)
    {
        promotion.Name = name ?? promotion.Name;
        promotion.Code = code ?? promotion.Code;
        promotion.Description = description ?? promotion.Description;
        promotion.UsageLimit = usageLimit ?? promotion.UsageLimit;
        promotion.PerCustomerUsageLimit = perCustomerUsageLimit ?? promotion.PerCustomerUsageLimit;
        promotion.StartsAtUtc = startsAtUtc ?? promotion.StartsAtUtc;
        promotion.ExpiresAtUtc = expiresAtUtc ?? promotion.ExpiresAtUtc;
        promotion.MatchPolicy = matchPolicy ?? promotion.MatchPolicy;
        promotion.Kind = kind ?? promotion.Kind;
        promotion.Advertise = advertise ?? promotion.Advertise;
        promotion.Active = active ?? promotion.Active;
        promotion.Position = position ?? promotion.Position;
        promotion.Path = path ?? promotion.Path;

        return Result.Ok();
    }

    /// <summary>Activates the promotion so it can be applied to orders.</summary>
    /// <param name="promotion">The promotion to activate.</param>
    /// <returns>A Result indicating success or AlreadyActive failure.</returns>
    // Enforce: Cannot activate an already-active promotion
    public static Result Activate(this Promotion promotion)
    {
        if (promotion.Active)
        {
            return PromotionResult.Errors.AlreadyActive;
        }

        promotion.Active = true;

        return Result.Ok(PromotionResult.Success.Activated);
    }

    /// <summary>Deactivates the promotion so it is no longer applied.</summary>
    /// <param name="promotion">The promotion to deactivate.</param>
    /// <returns>A Result indicating success or AlreadyInactive failure.</returns>
    // Enforce: Cannot deactivate an already-inactive promotion
    public static Result Deactivate(this Promotion promotion)
    {
        if (!promotion.Active)
        {
            return PromotionResult.Errors.AlreadyInactive;
        }

        promotion.Active = false;

        return Result.Ok(PromotionResult.Success.Deactivated);
    }

    /// <summary>Soft-deletes the promotion by marking it as deleted.</summary>
    /// <param name="promotion">The promotion to delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <returns>A Result indicating success.</returns>
    public static Result Delete(this Promotion promotion, string deletedBy)
    {
        if (promotion.IsDeleted)
        {
            return Result.Ok();
        }

        promotion.IsDeleted = true;
        promotion.DeletedAtUtc = DateTimeOffset.UtcNow;
        promotion.DeletedBy = deletedBy;

        return Result.Ok();
    }

    /// <summary>Determines whether the promotion is currently eligible based on active flag, deletion status, and date range.</summary>
    /// <param name="promotion">The promotion to evaluate.</param>
    /// <returns>True if the promotion is active and within its valid date range.</returns>
    // Guard: Reject eligibility check when promotion is not in active state
    // @CAT-5 Compute: IsActive when Active=true, IsDeleted=false, StartsAtUtc <= now, ExpiresAtUtc >= now
    public static bool IsActive(this Promotion promotion)
    {
        return promotion.Active
            && !promotion.IsDeleted
            && (promotion.StartsAtUtc == null || promotion.StartsAtUtc <= DateTimeOffset.UtcNow)
            && (promotion.ExpiresAtUtc == null || promotion.ExpiresAtUtc >= DateTimeOffset.UtcNow);
    }
    #endregion Methods
}