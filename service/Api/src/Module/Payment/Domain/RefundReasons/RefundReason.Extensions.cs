namespace Module.Payment.Domain.RefundReasons;

public static class RefundReasonExtensions
{
    /// <summary>
    /// Creates a new refund reason with the specified name and optional code.
    /// </summary>
    /// <param name="name">The reason name. Must not be empty.</param>
    /// <param name="code">Optional unique code for the reason.</param>
    /// <returns>A result containing the created refund reason or a validation error.</returns>
    // Contract: pre=name!=null && name.Length>0, post=reason.Id!=default && reason.Active==true
    public static Result<RefundReason> Create(string name, string? code)
    {
        // Validate: Refund reason name is required and must not be empty
        if (string.IsNullOrWhiteSpace(name))
            return RefundReasonResult.Errors.NameRequired;

        var reason = new RefundReason
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Active = RefundReasonConstant.Defaults.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return reason;
    }

    /// <summary>
    /// Activates this refund reason so it can be selected for refunds.
    /// </summary>
    /// <param name="reason">The refund reason to activate.</param>
    /// <returns>A result indicating success.</returns>
    // Enforce: Activate enables the reason for selection in refund workflows
    public static Result Activate(this RefundReason reason)
    {
        reason.Active = true;
        reason.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(RefundReasonResult.Success.Activated(reason.Id));
    }

    /// <summary>
    /// Deactivates this refund reason so it cannot be selected for new refunds.
    /// </summary>
    /// <param name="reason">The refund reason to deactivate.</param>
    /// <returns>A result indicating success.</returns>
    // Enforce: Deactivate prevents selection of this reason in refund workflows
    public static Result Deactivate(this RefundReason reason)
    {
        reason.Active = false;
        reason.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(RefundReasonResult.Success.Deactivated(reason.Id));
    }
}