namespace Module.Catalog.Domain.Taxons;

public static partial class TaxonLoggers
{
    #region Create
    [LoggerMessage(
        EventId = 4101,
        Level = LogLevel.Debug,
        Message = "[Taxon.Created]: {Name} ({Id}) in Taxonomy {TaxonomyId} by {ActionBy}")]
    public static partial void Created(ILogger logger, Guid Id, string Name, Guid TaxonomyId, string? ActionBy = "System");
    #endregion

    #region Update
    [LoggerMessage(
        EventId = 4102,
        Level = LogLevel.Debug,
        Message = "[Taxon.Updated]: {Name} ({Id}) in Taxonomy {TaxonomyId} by {ActionBy}")]
    public static partial void Updated(ILogger logger, Guid Id, string Name, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4103,
        Level = LogLevel.Debug,
        Message = "[Taxon.HierarchyPropagationSucceeded]: Hierarchy propagation succeeded for Taxon {TaxonId} by {ActionBy}")]
    public static partial void HierarchyPropagationSucceeded(ILogger logger, Guid TaxonId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4104,
        Level = LogLevel.Error,
        Message = "[Taxon.HierarchyPropagationFailed]: Hierarchy propagation failed for Taxon {TaxonId} by {ActionBy}")]
    public static partial void HierarchyPropagationFailed(ILogger logger, Guid TaxonId, string? ActionBy = "System");
    #endregion

    #region Delete
    [LoggerMessage(
        EventId = 4105,
        Level = LogLevel.Debug,
        Message = "[Taxon.Deleted]: {Name} ({Id}) from Taxonomy {TaxonomyId} by {ActionBy}")]
    public static partial void Deleted(ILogger logger, Guid Id, string Name, Guid TaxonomyId, string? ActionBy = "System");
    #endregion

    #region Reposition
    [LoggerMessage(
        EventId = 4106,
        Level = LogLevel.Debug,
        Message = "[Taxon.Moved]: {Name} ({Id}) from Parent {OldParentId} to {NewParentId} at Position {Position} by {ActionBy}")]
    public static partial void Moved(
        ILogger logger,
        string Name,
        Guid Id,
        Guid? OldParentId,
        Guid? NewParentId,
        int Position,
        string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4107,
        Level = LogLevel.Debug,
        Message = "[Taxon.HierarchyRebuildStarted]: Taxon {TaxonId} moved within Taxonomy {TaxonomyId} by {ActionBy}")]
    public static partial void HierarchyRebuildStarted(ILogger logger, Guid TaxonId, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4108,
        Level = LogLevel.Debug,
        Message = "[Taxon.HierarchyRebuildFinished]: Successfully rebuilt hierarchy for Taxonomy {TaxonomyId} by {ActionBy}")]
    public static partial void HierarchyRebuildFinished(ILogger logger, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4109,
        Level = LogLevel.Error,
        Message = "[Taxon.HierarchyRebuildFailed]: Failed to rebuild hierarchy for Taxonomy {TaxonomyId} after Taxon {TaxonId} was moved. Errors: {Errors} by {ActionBy}")]
    public static partial void HierarchyRebuildFailed(ILogger logger, Guid TaxonomyId, Guid TaxonId, string Errors, string? ActionBy = "System");
    #endregion

    #region Restore
    [LoggerMessage(
        EventId = 4110,
        Level = LogLevel.Debug,
        Message = "[Taxon.Restored]: {Name} ({Id}) in Taxonomy {TaxonomyId} by {ActionBy}")]
    public static partial void Restored(ILogger logger, Guid Id, string Name, Guid TaxonomyId, string? ActionBy = "System");
    #endregion
}