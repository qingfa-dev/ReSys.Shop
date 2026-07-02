namespace Shared.Operational.Persistence.Specifications.Paging;

/// <summary>
/// Typed result factories for <see cref="PageModel"/> parse operations.
/// </summary>
public static class PageModelResult
{
    #region Success

    /// <summary>
    /// Success log messages produced by <see cref="PageModel"/> parsers.
    /// </summary>
    public static class Success
    {
        public const string Parsed = "Page parsed successfully.";
        public const string Empty = "No page input provided; empty model returned.";
    }

    #endregion Success

    #region Error

    /// <summary>
    /// Errors produced during parsing of a <see cref="PageModel"/>.
    /// </summary>
    /// <remarks>
    /// Paging has no field whitelist and no out-of-bounds errors — those are corrected
    /// silently via <see cref="PageBounds"/> clamping. The only hard Error is a
    /// structurally malformed JSON payload.
    /// </remarks>
    public static class Failure
    {
        /// <summary>
        /// The JSON pagination payload is missing a required property or has the wrong structure.
        /// </summary>
        public static Error InvalidJson(string detail) =>
            Error.Validation(
                "Paging.InvalidJson",
                $"The pagination JSON is malformed: {detail}"
            );

        /// <summary>
        /// A pagination property value in JSON is present but cannot be interpreted as an integer.
        /// </summary>
        public static Error InvalidNumber(string property, string value) =>
            Error.Validation(
                "Paging.InvalidNumber",
                $"The value '{value}' for '{property}' is not a valid integer."
            );
    }

    #endregion Error
}
