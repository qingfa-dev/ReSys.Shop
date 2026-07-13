namespace Module.Inventory.Domain.StockLocations;

/// <summary>
/// Defines success messages and error factories for stock location operations.
/// </summary>
public static class StockLocationResult
{
    /// <summary>
    /// Contains success message templates for stock location operations.
    /// </summary>
    public static class Success
    {
        /// <summary>Success message for stock location creation.</summary>
        public static string Created => "Stock location was successfully created.";
        /// <summary>Success message for stock location update.</summary>
        public static string Updated => "Stock location was successfully updated.";
        /// <summary>Success message for stock location activation.</summary>
        public static string Activated => "Stock location was successfully activated.";
        /// <summary>Success message for stock location deactivation.</summary>
        public static string Deactivated => "Stock location was successfully deactivated.";
        /// <summary>Success message for stock location deletion.</summary>
        public static string Deleted => "Stock location was successfully deleted.";
        /// <summary>Success message for stock location restoration.</summary>
        public static string Restored => "Stock location was successfully restored.";
        /// <summary>Success message when setting a stock location as default.</summary>
        public static string SetAsDefault => "Stock location was successfully set as default.";
        /// <summary>Success message for stock location list retrieval.</summary>
        public static string GetList => "Stock locations retrieved successfully.";
    }

    /// <summary>
    /// Contains error factory methods for stock location operations.
    /// </summary>
    public static class Failure
    {
        #region Validation
        /// <summary>Error when stock location name is not provided.</summary>
        public static Error NameRequired => Error.Validation(
            code: "StockLocation.Name.Required",
            message: "Stock location name is required.");

        /// <summary>Error when stock location name exceeds maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "StockLocation.Name.TooLong",
            message: $"Stock location name cannot exceed {StockLocationConstant.Constraints.NameMaxLength} characters.");

        /// <summary>Error when stock location code exceeds maximum length.</summary>
        public static Error CodeTooLong => Error.Validation(
            code: "StockLocation.Code.TooLong",
            message: $"Stock location code cannot exceed {StockLocationConstant.Constraints.CodeMaxLength} characters.");

        /// <summary>Error when a stock location with the same code already exists.</summary>
        public static Error CodeDuplicate => Error.Conflict(
            code: "StockLocation.Code.Duplicate",
            message: "A stock location with the same code already exists.");

        /// <summary>Error when stock location address exceeds maximum length.</summary>
        public static Error AddressTooLong => Error.Validation(
            code: "StockLocation.Address.TooLong",
            message: $"Stock location address cannot exceed {StockLocationConstant.Constraints.AddressMaxLength} characters.");

        /// <summary>Error when stock location city exceeds maximum length.</summary>
        public static Error CityTooLong => Error.Validation(
            code: "StockLocation.City.TooLong",
            message: $"Stock location city cannot exceed {StockLocationConstant.Constraints.CityMaxLength} characters.");

        /// <summary>Error when stock location phone exceeds maximum length.</summary>
        public static Error PhoneTooLong => Error.Validation(
            code: "StockLocation.Phone.TooLong",
            message: $"Stock location phone cannot exceed {StockLocationConstant.Constraints.PhoneMaxLength} characters.");

        /// <summary>Error when stock location postal code exceeds maximum length.</summary>
        public static Error PostalCodeTooLong => Error.Validation(
            code: "StockLocation.PostalCode.TooLong",
            message: $"Stock location postal code cannot exceed {StockLocationConstant.Constraints.PostalCodeMaxLength} characters.");

        /// <summary>Error when stock location admin name exceeds maximum length.</summary>
        public static Error AdminNameTooLong => Error.Validation(
            code: "StockLocation.AdminName.TooLong",
            message: $"Stock location admin name cannot exceed {StockLocationConstant.Constraints.AdminNameMaxLength} characters.");

        /// <summary>Error when stock location presentation exceeds maximum length.</summary>
        public static Error PresentationTooLong => Error.Validation(
            code: "StockLocation.Presentation.TooLong",
            message: $"Stock location presentation cannot exceed {StockLocationConstant.Constraints.PresentationMaxLength} characters.");
        #endregion

        #region Business
        /// <summary>Error when stock location is not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "StockLocation.NotFound",
            message: "Stock location was not found.");

        /// <summary>Error when attempting to create a duplicate stock location name.</summary>
        public static Error DuplicateName => Error.Conflict(
            code: "StockLocation.DuplicateName",
            message: "A stock location with the same name already exists.");

        /// <summary>Error when attempting to deactivate the default stock location.</summary>
        public static Error CannotDeactivateDefault => Error.Conflict(
            code: "StockLocation.CannotDeactivateDefault",
            message: "Cannot deactivate the default stock location. Set another location as default first.");

        /// <summary>Error when attempting to delete an active stock location.</summary>
        public static Error CannotDeleteActive => Error.Conflict(
            code: "StockLocation.CannotDeleteActive",
            message: "Cannot delete an active stock location. Deactivate it first.");

        /// <summary>Error when attempting to delete a stock location that has stock items.</summary>
        public static Error LocationHasStockItems => Error.Conflict(
            code: "StockLocation.HasStockItems",
            message: "Cannot delete a stock location that has associated stock items.");

        /// <summary>Error when attempting to set a second default stock location.</summary>
        public static Error OnlyOneDefaultAllowed => Error.Conflict(
            code: "StockLocation.OnlyOneDefaultAllowed",
            message: "Only one default stock location is allowed. Unset the current default first.");

        /// <summary>Error when attempting to change store association after creation.</summary>
        public static Error CannotChangeStore => Error.Conflict(
            code: "StockLocation.CannotChangeStore",
            message: "Cannot change the store association of an existing stock location.");
        #endregion
    }
}