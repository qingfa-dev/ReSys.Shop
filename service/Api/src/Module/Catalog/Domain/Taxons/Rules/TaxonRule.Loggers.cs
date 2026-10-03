namespace Module.Catalog.Domain.Taxons.Rules;

public static partial class TaxonRuleLoggers
{
    [LoggerMessage(EventId = 4401, Level = LogLevel.Debug, Message = "Taxon rule created with ID '{RuleId}' for taxon '{TaxonId}'")]
    public static partial void Created(ILogger logger, Guid RuleId, Guid TaxonId);

    [LoggerMessage(EventId = 4402, Level = LogLevel.Debug, Message = "Taxon rule updated with ID '{RuleId}' for taxon '{TaxonId}'")]
    public static partial void Updated(ILogger logger, Guid RuleId, Guid TaxonId);

    [LoggerMessage(EventId = 4403, Level = LogLevel.Debug, Message = "Taxon rule deleted with ID '{RuleId}' for taxon '{TaxonId}'")]
    public static partial void Deleted(ILogger logger, Guid RuleId, Guid TaxonId);

    [LoggerMessage(EventId = 4404, Level = LogLevel.Debug, Message = "Classification regeneration started for taxon '{TaxonId}' after rule change")]
    public static partial void ClassificationStarted(ILogger logger, Guid TaxonId);

    [LoggerMessage(EventId = 4405, Level = LogLevel.Error, Message = "Classification regeneration failed for taxon '{TaxonId}' after rule change: {Error}")]
    public static partial void ClassificationFailed(ILogger logger, Guid TaxonId, string Error);
}