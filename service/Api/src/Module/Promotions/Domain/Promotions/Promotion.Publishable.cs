namespace Module.Promotions.Domain.Promotions;

// Invariant: Publishable promotion has valid StartAtUtc <= ExpiresAtUtc when both set
public sealed partial class Promotion
{
    #region Publishable
    // Compute: Promotion is published when Active=true, not deleted, and within valid date range
    // Compute: Promotion is published when Active=true, not deleted, and within valid date range
    public bool IsPublished => this.IsActive() && !IsDeleted;

    /// <summary>Publishes the promotion by activating it.</summary>
    // Create: Transition promotion to published state by activating
    #pragma warning disable CA1822
    public Result Publish()
    #pragma warning restore CA1822
    {
        return this.Activate();
    }

    /// <summary>Unpublishes the promotion by deactivating it.</summary>
    // Remove: Transition promotion to unpublished state by deactivating
    #pragma warning disable CA1822
    public Result Unpublish()
    #pragma warning restore CA1822
    {
        return this.Deactivate();
    }
    #endregion Publishable
}