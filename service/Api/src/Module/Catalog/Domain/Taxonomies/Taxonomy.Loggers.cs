namespace Module.Catalog.Domain.Taxonomies;

public static partial class TaxonomyLoggers
{
    #region Create
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.RootTaxonCreated]: {Name} ({TaxonomyId}) by {ActionBy}")]
    public static partial void RootTaxonCreated(ILogger logger, string Name, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "[Taxonomy.RootTaxonCreationFailed]: {Name} ({TaxonomyId}). Errors: {Errors} by {ActionBy}")]
    public static partial void RootTaxonCreationFailed(ILogger logger, string Name, Guid TaxonomyId, string Errors, string? ActionBy = "System");
    #endregion

    #region Update
    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, Guid Id, string Name, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Error,
        Message = "[Taxonomy.RootTaxonUpdateFailed]: {Name} ({TaxonomyId}) by {ActionBy}")]
    public static partial void RootTaxonUpdateFailed(ILogger logger, string Name, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4008,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.RootTaxonUpdated]: {Name} ({TaxonomyId}) by {ActionBy}")]
    public static partial void RootTaxonUpdated(ILogger logger, string Name, Guid TaxonomyId, string? ActionBy = "System");
    #endregion

    #region Delete
    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, Guid Id, string Name, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4009,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.RootTaxonDeleted]: {TaxonomyId} by {ActionBy}")]
    public static partial void RootTaxonDeleted(ILogger logger, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4010,
        Level = LogLevel.Error,
        Message = "[Taxonomy.RootTaxonDeletionFailed]: {TaxonomyId}. Errors: {Errors} by {ActionBy}")]
    public static partial void RootTaxonDeletionFailed(ILogger logger, Guid TaxonomyId, string Errors, string? ActionBy = "System");
    #endregion

    #region Restore
    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.Restored]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Restored(ILogger logger, Guid Id, string Name, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Debug,
        Message = "[Taxonomy.RootTaxonRestored]: {TaxonomyId} by {ActionBy}")]
    public static partial void RootTaxonRestored(ILogger logger, Guid TaxonomyId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Error,
        Message = "[Taxonomy.RootTaxonRestorationFailed]: {TaxonomyId}. Errors: {Errors} by {ActionBy}")]
    public static partial void RootTaxonRestorationFailed(ILogger logger, Guid TaxonomyId, string Errors, string? ActionBy = "System");
    #endregion
}
