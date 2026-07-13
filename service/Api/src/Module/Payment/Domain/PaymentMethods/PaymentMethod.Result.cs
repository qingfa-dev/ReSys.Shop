namespace Module.Payment.Domain.PaymentMethods;

/// <summary>
/// Provides success messages and error definitions for payment method domain operations.
/// </summary>
public static class PaymentMethodResult
{
    /// <summary>
    /// Contains success message templates for payment method lifecycle events.
    /// </summary>
    public static class Success
    {
        /// <summary>Returns a success message for a created payment method.</summary>
        public static string Created(string name) => $"Payment method '{name}' was successfully created.";
        /// <summary>Returns a success message for an activated payment method.</summary>
        public static string Activated(string name) => $"Payment method '{name}' was successfully activated.";
        /// <summary>Returns a success message for a deactivated payment method.</summary>
        public static string Deactivated(string name) => $"Payment method '{name}' was successfully deactivated.";
        /// <summary>Returns a success message for an updated payment method.</summary>
        public static string Updated(string name) => $"Payment method '{name}' was successfully updated.";
    }

    /// <summary>
    /// Contains error definitions for payment method validation and business rule violations.
    /// </summary>
    public static class Errors
    {
        #region Validation
        /// <summary>Error indicating the payment method name is required.</summary>
        public static Error NameRequired => Error.Validation(
            code: "PaymentMethod.Name.Required",
            message: "Payment method name is required.");

        /// <summary>Error indicating the payment method name exceeds the maximum length.</summary>
        public static Error NameTooLong => Error.Validation(
            code: "PaymentMethod.Name.TooLong",
            message: $"Payment method name cannot exceed {PaymentMethodConstant.Constraints.MaxNameLength} characters.");

        /// <summary>Error indicating the payment method code exceeds the maximum length.</summary>
        public static Error CodeTooLong => Error.Validation(
            code: "PaymentMethod.Code.TooLong",
            message: $"Payment method code cannot exceed {PaymentMethodConstant.Constraints.MaxCodeLength} characters.");

        /// <summary>Error indicating the provider type is required.</summary>
        public static Error ProviderTypeRequired => Error.Validation(
            code: "PaymentMethod.ProviderType.Required",
            message: "Payment method provider type is required.");

        /// <summary>Error indicating the provider type exceeds the maximum length.</summary>
        public static Error ProviderTypeTooLong => Error.Validation(
            code: "PaymentMethod.ProviderType.TooLong",
            message: $"Payment method provider type cannot exceed {PaymentMethodConstant.Constraints.MaxProviderKeyLength} characters.");

        /// <summary>Error indicating the description exceeds the maximum length.</summary>
        public static Error DescriptionTooLong => Error.Validation(
            code: "PaymentMethod.Description.TooLong",
            message: $"Payment method description cannot exceed {PaymentMethodConstant.Constraints.MaxDescriptionLength} characters.");

        #endregion

        #region Code
        /// <summary>Error indicating the payment method code is required.</summary>
        public static Error CodeRequired => Error.Validation(
            code: "PaymentMethod.Code.Required",
            message: "Payment method code is required.");

        /// <summary>Error indicating the payment method code has an invalid format.</summary>
        public static Error CodeInvalid => Error.Validation(
            code: "PaymentMethod.Code.Invalid",
            message: "Payment method code must contain only alphanumeric characters, underscores, and hyphens.");
        #endregion

        #region Presentation
        /// <summary>Error indicating the presentation text exceeds the maximum length.</summary>
        public static Error PresentationTooLong => Error.Validation(
            code: "PaymentMethod.Presentation.TooLong",
            message: $"Payment method presentation cannot exceed {PaymentMethodConstant.Constraints.MaxPresentationLength} characters.");
        #endregion

        #region Position
        /// <summary>Error indicating the position is out of range.</summary>
        public static Error PositionOutOfRange => Error.Validation(
            code: "PaymentMethod.Position.OutOfRange",
            message: $"Payment method position must be between {PaymentMethodConstant.Constraints.MinPositionValue} and {PaymentMethodConstant.Constraints.MaxPositionValue}.");
        #endregion

        #region DisplayOn
        /// <summary>Error indicating the display target is invalid.</summary>
        public static Error DisplayOnInvalid => Error.Validation(
            code: "PaymentMethod.DisplayOn.Invalid",
            message: "Payment method display target is invalid.");
        #endregion

        #region Settings
        /// <summary>Error indicating too many settings entries.</summary>
        public static Error SettingsTooMany => Error.Validation(
            code: "PaymentMethod.Settings.TooMany",
            message: $"Payment method settings cannot exceed {PaymentMethodConstant.Constraints.MaxSettingsItems} entries.");

        /// <summary>Error indicating a setting key exceeds the maximum length.</summary>
        public static Error SettingKeyTooLong => Error.Validation(
            code: "PaymentMethod.Settings.KeyTooLong",
            message: $"Payment method setting key cannot exceed {PaymentMethodConstant.Constraints.MaxSettingsKeyLength} characters.");

        /// <summary>Error indicating a setting value exceeds the maximum length.</summary>
        public static Error SettingValueTooLong => Error.Validation(
            code: "PaymentMethod.Settings.ValueTooLong",
            message: $"Payment method setting value cannot exceed {PaymentMethodConstant.Constraints.MaxSettingsValueLength} characters.");
        #endregion

        #region Preferences
        /// <summary>Error indicating too many preference entries.</summary>
        public static Error PreferencesTooMany => Error.Validation(
            code: "PaymentMethod.Preferences.TooMany",
            message: $"Payment method preferences cannot exceed {PaymentMethodConstant.Constraints.MaxPreferencesItems} entries.");

        /// <summary>Error indicating a preference key exceeds the maximum length.</summary>
        public static Error PreferenceKeyTooLong => Error.Validation(
            code: "PaymentMethod.Preferences.KeyTooLong",
            message: $"Payment method preference key cannot exceed {PaymentMethodConstant.Constraints.MaxPreferencesKeyLength} characters.");

        /// <summary>Error indicating a preference value exceeds the maximum length.</summary>
        public static Error PreferenceValueTooLong => Error.Validation(
            code: "PaymentMethod.Preferences.ValueTooLong",
            message: $"Payment method preference value cannot exceed {PaymentMethodConstant.Constraints.MaxPreferencesValueLength} characters.");
        #endregion

        #region Business
        /// <summary>Error indicating the payment method was not found.</summary>
        public static Error NotFound => Error.NotFound(
            code: "PaymentMethod.NotFound",
            message: "Payment method was not found.");

        /// <summary>Error indicating a payment method with the same code already exists.</summary>
        public static Error CodeDuplicate => Error.Conflict(
            code: "PaymentMethod.Code.Duplicate",
            message: "A payment method with the same code already exists.");

        /// <summary>Error indicating the payment method is already active.</summary>
        public static Error AlreadyActive => Error.Conflict(
            code: "PaymentMethod.AlreadyActive",
            message: "Payment method is already active.");

        /// <summary>Error indicating the payment method is already inactive.</summary>
        public static Error AlreadyInactive => Error.Conflict(
            code: "PaymentMethod.AlreadyInactive",
            message: "Payment method is already inactive.");

        public static Error ProviderNotRegistered(string providerKey) => Error.Validation(
            code: "PaymentMethod.ProviderKey.NotRegistered",
            message: $"Provider '{providerKey}' is not registered in the gateway registry.");

        /// <summary>Error indicating the payment method has active payments and cannot be deleted.</summary>
        public static Error HasActivePayments => Error.Conflict(
            code: "PaymentMethod.HasActivePayments",
            message: "Cannot delete the payment method because it has active payments.");

        /// <summary>Error indicating the payment method is referenced by existing orders and cannot be deleted.</summary>
        public static Error HasActiveOrders => Error.Conflict(
            code: "PaymentMethod.HasActiveOrders",
            message: "Cannot delete the payment method because it is referenced by active orders.");

        /// <summary>Error indicating the payment method name already exists.</summary>
        public static Error NameDuplicate => Error.Conflict(
            code: "PaymentMethod.Name.Duplicate",
            message: "A payment method with the same name already exists.");

        /// <summary>Error indicating the update failed for an unexpected reason.</summary>
        public static Error UpdateFailed(string reason) => Error.Unexpected(
            code: "PaymentMethod.UpdateFailed",
            message: $"Payment method update failed: {reason}.");

        /// <summary>Error indicating the delete failed for an unexpected reason.</summary>
        public static Error DeleteFailed(string reason) => Error.Unexpected(
            code: "PaymentMethod.DeleteFailed",
            message: $"Payment method delete failed: {reason}.");
        #endregion Business
    }
}