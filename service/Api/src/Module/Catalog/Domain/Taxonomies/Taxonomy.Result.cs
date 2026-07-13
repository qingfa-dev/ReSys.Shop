namespace Module.Catalog.Domain.Taxonomies;

public static class TaxonomyResult
{
    public static class Success
    {
        /// <summary>Taxonomy was created successfully.</summary>
        public static string Created => $"Taxonomy was created successfully.";
        /// <summary>Taxonomy was updated successfully.</summary>
        public static string Updated => $"Taxonomy was updated successfully.";
        /// <summary>Taxonomy was deleted successfully.</summary>
        public static string Deleted => $"Taxonomy was deleted successfully.";
        /// <summary>Taxonomy was restored successfully.</summary>
        public static string Restored => $"Taxonomy was restored successfully.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Taxonomy name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "Taxonomy.Name.Required",
            message: "Taxonomy name is required.");

        /// <summary>Taxonomy name exceeds the maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "Taxonomy.Name.TooLong",
            message: $"Taxonomy name cannot exceed {TaxonomyConstant.Constraints.NameMaxLength} characters.");

        /// <summary>Taxonomy presentation is required.</summary>
        public static Error PresentationRequired => Error.Validation(
            code: "Taxonomy.Presentation.Required",
            message: "Taxonomy presentation is required.");

        /// <summary>Taxonomy presentation exceeds the maximum length.</summary>
        public static Error PresentationTooLong => Error.Validation(
            code: "Taxonomy.Presentation.TooLong",
            message: $"Taxonomy presentation cannot exceed {TaxonomyConstant.Constraints.PresentationMaxLength} characters.");

        /// <summary>Position must be greater than or equal to the minimum value.</summary>
        public static Error InvalidPosition => Error.Validation(
            code: "Taxonomy.Position.Invalid",
            message: $"Position must be greater than or equal to {TaxonomyConstant.Constraints.MinPosition}.");
        #endregion

        #region Business
        /// <summary>Taxonomy was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Taxonomy.NotFound",
            message: "Taxonomy was not found.");

        /// <summary>A taxonomy with the same name already exists.</summary>
        public static Error DuplicateName => Error.Conflict(
            code: "Taxonomy.DuplicateName",
            message: "A taxonomy with the same name already exists.");

        /// <summary>Cannot delete a taxonomy that has associated taxons.</summary>
        public static Error HasTaxons => Error.Validation(
            code: "Taxonomy.HasTaxons",
            message: "Cannot delete a taxonomy with associated taxons. Delete or move all taxons first.");
        #endregion
    }
}