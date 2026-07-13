namespace Module.Catalog.Domain.Taxonomies.Taxons.Rules;

public static class TaxonRuleResult
{
    public static class Success
    {
        /// <summary>Returns a success message for taxon rule creation.</summary>
        public static string Created(Guid id) => $"Taxon rule with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for taxon rule update.</summary>
        public static string Updated(Guid id) => $"Taxon rule with ID '{id}' was successfully updated.";
        /// <summary>Returns a success message for taxon rule deletion.</summary>
        public static string Deleted(Guid id) => $"Taxon rule with ID '{id}' was successfully deleted.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Rule type is invalid.</summary>
        public static Error InvalidType => Error.Validation(
            code: "TaxonRule.Type.Invalid",
            message: $"Rule type must be one of: {string.Join(", ", EnumExtensions.GetValues<TaxonRuleType>())}");

        /// <summary>Match policy is invalid.</summary>
        public static Error InvalidMatchPolicy => Error.Validation(
            code: "TaxonRule.MatchPolicy.Invalid",
            message: $"Match policy must be one of: {string.Join(", ", EnumExtensions.GetValues<TaxonRuleMatchPolicy>())}");

        /// <summary>Rule value is required.</summary>
        public static Error ValueRequired => Error.Validation(
            code: "TaxonRule.Value.Required",
            message: "Rule value is required.");

        /// <summary>Rule value exceeds the maximum length.</summary>
        public static Error ValueTooLong => Error.Validation(
            code: "TaxonRule.Value.TooLong",
            message: $"Rule value cannot exceed {TaxonRuleConstant.Constraints.ValueMaxLength} characters.");

        /// <summary>Taxonomy ID is required.</summary>
        public static Error TaxonomyIdRequired => Error.Validation(
            code: "TaxonRule.TaxonomyId.Required",
            message: "Taxonomy ID is required.");

        /// <summary>Taxon ID is required.</summary>
        public static Error TaxonIdRequired => Error.Validation(
            code: "TaxonRule.TaxonId.Required",
            message: "Taxon ID is required.");

        /// <summary>Rule ID is required.</summary>
        public static Error RuleIdRequired => Error.Validation(
            code: "TaxonRule.RuleId.Required",
            message: "Rule ID is required.");

        /// <summary>Rules list is required.</summary>
        public static Error RulesRequired => Error.Validation(
            code: "TaxonRule.Rules.Required",
            message: "Rules list is required.");
        #endregion

        #region Business
        /// <summary>Taxon rule was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "TaxonRule.NotFound",
            message: "Taxon rule was not found.");

        /// <summary>A similar taxon rule already exists for this taxon.</summary>
        public static Error Duplicate => Error.Conflict(
            code: "TaxonRule.Duplicate",
            message: "A similar taxon rule already exists for this taxon.");
        #endregion
    }
}