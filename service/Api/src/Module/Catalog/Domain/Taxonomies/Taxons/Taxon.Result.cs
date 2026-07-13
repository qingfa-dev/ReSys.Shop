namespace Module.Catalog.Domain.Taxonomies.Taxons;

public static class TaxonResult
{
    public static class Success
    {
        /// <summary>Taxon was successfully created.</summary>
        public static string Created => $"Taxon was successfully created.";
        /// <summary>Taxon was successfully updated.</summary>
        public static string Updated => $"Taxon was successfully updated.";
        /// <summary>Taxon was successfully deleted.</summary>
        public static string Deleted => $"Taxon was successfully deleted.";
        /// <summary>Taxon was successfully moved.</summary>
        public static string Moved => $"Taxon was successfully moved.";
    }

    public static class Errors
    {
        #region Business
        /// <summary>Taxon was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "Taxon.NotFound",
            message: "Taxon was not found.");

        /// <summary>A taxon with the same name already exists in this taxonomy.</summary>
        public static Error DuplicateName => Error.Conflict(
            code: "Taxon.DuplicateName",
            message: "A taxon with the same name already exists in this taxonomy.");

        /// <summary>A taxon with the same slug already exists in this taxonomy.</summary>
        public static Error DuplicateSlug => Error.Conflict(
            code: "Taxon.DuplicateSlug",
            message: "A taxon with the same slug already exists in this taxonomy.");

        /// <summary>The parent taxon must belong to the same taxonomy.</summary>
        public static Error ParentTaxonomyMismatch => Error.Validation(
            code: "Taxon.ParentTaxonomyMismatch",
            message: "The parent taxon must belong to the same taxonomy.");

        /// <summary>Root taxon cannot be moved, deleted, or re-parented.</summary>
        public static Error RootLock => Error.Validation(
            code: "Taxon.RootLock",
            message: "Root taxon cannot be moved, deleted, or re-parented.");

        /// <summary>Cycle detected in hierarchy. A taxon cannot be a descendant of itself.</summary>
        public static Error CycleDetected => Error.Validation(
            code: "Taxon.CycleDetected",
            message: "Cycle detected in hierarchy. A taxon cannot be a descendant of itself.");

        /// <summary>No root taxon found for the specified taxonomy.</summary>
        public static Error NoRoot => Error.NotFound(
            code: "Taxon.NoRoot",
            message: "No root taxon found for the specified taxonomy.");

        /// <summary>Taxonomy has multiple root taxons.</summary>
        public static Error RootConflict => Error.Conflict(
            code: "Taxon.RootConflict",
            message: "Taxonomy has multiple root taxons.");

        /// <summary>Failed to rebuild taxonomy hierarchy.</summary>
        public static Error HierarchyRebuildFailed => Error.Unexpected(
            code: "Taxon.HierarchyRebuildFailed",
            message: "Failed to rebuild taxonomy hierarchy.");

        /// <summary>Failed to regenerate products for taxon.</summary>
        public static Error RegenerationFailed => Error.Unexpected(
            code: "Taxon.RegenerationFailed",
            message: "Failed to regenerate products for taxon.");

        /// <summary>A taxon cannot be its own parent.</summary>
        public static Error SelfParenting => Error.Validation(
            code: "Taxon.SelfParenting",
            message: "A taxon cannot be its own parent.");

        /// <summary>Circular parenting detected. A taxon cannot be a descendant of itself.</summary>
        public static Error CircularParenting => Error.Validation(
            code: "Taxon.CircularParenting",
            message: "Circular parenting detected. A taxon cannot be a descendant of itself.");

        /// <summary>The specified parent taxon does not exist in the same taxonomy.</summary>
        public static Error InvalidParent => Error.Validation(
            code: "Taxon.InvalidParent",
            message: "The specified parent taxon does not exist in the same taxonomy.");

        /// <summary>Cannot delete a taxon that has children.</summary>
        public static Error HasChildren => Error.Conflict(
            code: "Taxon.HasChildren",
            message: "Cannot delete a taxon that has children.");

        /// <summary>Taxon has invalid nested set values.</summary>
        public static Error InvalidNestedSet(string name, Guid id, int lft, int rgt) => Error.Validation(
            code: "Taxon.Hierarchy.InvalidNestedSet",
            message: $"Taxon '{name}' ({id}) has invalid nested set values: Lft={lft}, Rgt={rgt}.");

        /// <summary>Taxon has overlapping nested set boundaries.</summary>
        public static Error OverlappingBoundaries(string name, Guid id) => Error.Validation(
            code: "Taxon.Hierarchy.OverlappingBoundaries",
            message: $"Taxon '{name}' ({id}) has overlapping nested set boundaries.");

        /// <summary>Child taxon is not contained within parent.</summary>
        public static Error BoundaryViolation(string childName, string parentName) => Error.Validation(
            code: "Taxon.Hierarchy.BoundaryViolation",
            message: $"Child taxon '{childName}' is not contained within parent '{parentName}'.");
        #endregion

        #region Validation
        /// <summary>Taxonomy ID must be a valid GUID.</summary>
        public static Error InvalidTaxonomyId => Error.Validation(
            code: "Taxon.TaxonomyId.Invalid",
            message: "Taxonomy ID must be a valid GUID.");

        /// <summary>Parent ID must be a valid GUID if provided.</summary>
        public static Error InvalidParentId => Error.Validation(
            code: "Taxon.ParentId.Invalid",
            message: "Parent ID must be a valid GUID if provided.");

        /// <summary>Taxon name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "Taxon.Name.Required",
            message: "Taxon name is required.");

        /// <summary>Taxon name exceeds the maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "Taxon.Name.TooLong",
            message: $"Taxon name cannot exceed {TaxonConstant.Constraints.NameMaxLength} characters.");

        /// <summary>Taxon slug is required.</summary>
        public static Error SlugRequired => Error.Validation(
            code: "Taxon.Slug.Required",
            message: "Taxon slug is required.");

        /// <summary>Taxon slug exceeds the maximum length.</summary>
        public static Error SlugTooLong => Error.Validation(
            code: "Taxon.Slug.TooLong",
            message: $"Taxon slug cannot exceed {TaxonConstant.Constraints.SlugMaxLength} characters.");

        /// <summary>Taxon slug format is invalid.</summary>
        public static Error SlugInvalidFormat => Error.Validation(
            code: "Taxon.Slug.InvalidFormat",
            message: "Taxon slug format is invalid. Only lowercase letters, numbers, and hyphens are allowed, and it cannot start or end with a hyphen.");

        /// <summary>Taxon presentation exceeds the maximum length.</summary>
        public static Error PresentationTooLong => Error.Validation(
            code: "Taxon.Presentation.TooLong",
            message: $"Taxon presentation cannot exceed {TaxonConstant.Constraints.PresentationMaxLength} characters.");

        /// <summary>Taxon message exceeds the maximum length.</summary>
        public static Error DescriptionTooLong => Error.Validation(
            code: "Taxon.Description.TooLong",
            message: $"Taxon message cannot exceed {TaxonConstant.Constraints.DescriptionMaxLength} characters.");

        /// <summary>Taxon meta title exceeds the maximum length.</summary>
        public static Error MetaTitleTooLong => Error.Validation(
            code: "Taxon.MetaTitle.TooLong",
            message: $"Taxon meta title cannot exceed {TaxonConstant.Constraints.MetaTitleMaxLength} characters.");

        /// <summary>Taxon meta message exceeds the maximum length.</summary>
        public static Error MetaDescriptionTooLong => Error.Validation(
            code: "Taxon.MetaDescription.TooLong",
            message: $"Taxon meta message cannot exceed {TaxonConstant.Constraints.MetaDescriptionMaxLength} characters.");

        /// <summary>Taxon meta keywords exceeds the maximum length.</summary>
        public static Error MetaKeywordsTooLong => Error.Validation(
            code: "Taxon.MetaKeywords.TooLong",
            message: $"Taxon meta keywords cannot exceed {TaxonConstant.Constraints.MetaKeywordsMaxLength} characters.");

        /// <summary>Taxon image URL exceeds the maximum length.</summary>
        public static Error ImageUrlTooLong => Error.Validation(
            code: "Taxon.ImageUrl.TooLong",
            message: $"Taxon image URL cannot exceed {TaxonConstant.Constraints.UrlMaxLength} characters.");

        /// <summary>Taxon image URL format is invalid.</summary>
        public static Error ImageUrlInvalidFormat => Error.Validation(
            code: "Taxon.ImageUrl.InvalidFormat",
            message: "Taxon image URL format is invalid.");

        /// <summary>Taxon square image URL exceeds the maximum length.</summary>
        public static Error SquareImageUrlTooLong => Error.Validation(
            code: "Taxon.SquareImageUrl.TooLong",
            message: $"Taxon square image URL cannot exceed {TaxonConstant.Constraints.UrlMaxLength} characters.");

        /// <summary>Taxon square image URL format is invalid.</summary>
        public static Error SquareImageUrlInvalidFormat => Error.Validation(
            code: "Taxon.SquareImageUrl.InvalidFormat",
            message: "Taxon square image URL format is invalid.");

        /// <summary>Position must be at least the minimum value.</summary>
        public static Error InvalidPosition => Error.Validation(
            code: "Taxon.Position.Invalid",
            message: $"Position must be at least {TaxonConstant.Constraints.MinPosition}.");

        /// <summary>Rules match policy is invalid.</summary>
        public static Error InvalidRulesMatchPolicy => Error.Validation(
            code: "Taxon.RulesMatchPolicy.Invalid",
            message: $"Rules match policy is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<TaxonMatchPolicy>())}");

        /// <summary>Sort order is invalid.</summary>
        public static Error InvalidSortOrder => Error.Validation(
            code: "Taxon.SortOrder.Invalid",
            message: $"Sort order is invalid. Must be one of: {string.Join(", ", EnumExtensions.GetValues<TaxonSortOrder>())}");
        #endregion
    }
}