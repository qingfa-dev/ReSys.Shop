using FluentValidation.Results;

namespace Shared.Application.Mappings;

/// <summary>
/// Extension methods for converting FluentValidation results to Error/Result types.
/// </summary>
public static class ValidationResultMapper
{
    /// <summary>
    /// Converts a ValidationFailure to an Error with optional additional metadata.
    /// </summary>
    public static Error ToError(
        this ValidationFailure failure,
        params (string Key, object? Value)[] additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(failure);

        var metadata = new List<(string Key, object? Value)>(capacity: 4 + (additionalMetadata?.Length ?? 0))
        {
            ("propertyName", failure.PropertyName),
            ("attemptedValue", failure.AttemptedValue)
        };

        if (failure.FormattedMessagePlaceholderValues?.Count > 0)
        {
            foreach (KeyValuePair<string, object> kvp in failure.FormattedMessagePlaceholderValues)
            {
                metadata.Add((kvp.Key, kvp.Value));
            }
        }

        if (additionalMetadata?.Length > 0)
        {
            foreach ((string Key, object? Value) item in additionalMetadata)
            {
                metadata.Add(item);
            }
        }

        return Error.Validation(
            failure.ErrorCode ?? "ValidationError",
            failure.ErrorMessage,
            metadata.ToArray());
    }

    /// <summary>
    /// Converts a collection of ValidationFailure to Errors with optional additional metadata.
    /// </summary>
    public static List<Error> ToErrors(
        this IEnumerable<ValidationFailure>? failures,
        params (string Key, object? Value)[] additionalMetadata)
    {
        if (failures is null)
            return [];

        var errors = new List<Error>(capacity: 4);
        errors.AddRange(failures.Select(failure => failure.ToError(additionalMetadata)));

        return errors;
    }

    /// <summary>
    /// Converts a ValidationResult to a list of Errors with optional additional metadata.
    /// </summary>
    public static List<Error> ToErrorList(
        this ValidationResult validationResult,
        params (string Key, object? Value)[] additionalMetadata)
    {
        if (validationResult is null)
            return [];

        return validationResult.Errors.ToErrors(additionalMetadata);
    }

    /// <summary>
    /// Converts a ValidationResult to a typed failure Result{T}. Throws if validation passed.
    /// </summary>
    public static Result<T> ToErrors<T>(
        this ValidationResult validationResult,
        params (string Key, object? Value)[] additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        if (validationResult.IsValid)
        {
            throw new InvalidOperationException(
                "Cannot convert valid ValidationResult to failure.");
        }

        return Result<T>.Validation(
            errors: validationResult.Errors.ToErrors(additionalMetadata));
    }

    /// <summary>
    /// Converts a ValidationResult to a Result. Returns Ok on valid, Validation failure on invalid.
    /// </summary>
    public static Result ToErrors(
        this ValidationResult validationResult,
        params (string Key, object? Value)[] additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        if (validationResult.IsValid)
            return Result.Ok();

        return Result.Validation(
            errors: validationResult.Errors.ToErrors(additionalMetadata));
    }

    /// <summary>
    /// Checks if ValidationResult has errors.
    /// </summary>
    public static bool HasErrors(this ValidationResult validationResult)
        => validationResult is not null && 
           !validationResult.IsValid && 
           validationResult.Errors.Count > 0;
}