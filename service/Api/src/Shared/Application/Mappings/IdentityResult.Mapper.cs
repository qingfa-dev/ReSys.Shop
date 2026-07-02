using Microsoft.AspNetCore.Identity;

namespace Shared.Application.Mappings;

/// <summary>
/// Extension methods for converting IdentityResult to Error/Result types.
/// </summary>
public static class IdentityResultMapper
{
    /// <summary>
    /// Converts an IdentityError to an Error.
    /// </summary>
    public static Error ToError(
        this IdentityError error,
        params (string Key, object? Value)[]? additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(error);

        var metadata = new List<(string Key, object? Value)>(capacity: additionalMetadata?.Length ?? 0);

        string code = error.Code!;
        string description = error.Description ?? "An identity error occurred";

        if (!(additionalMetadata?.Length > 0))
        {
            return Error.Validation(
                code,
                description,
                [.. metadata]
            );
        }
        metadata.AddRange(additionalMetadata);
        return Error.Validation(
            code,
            description,
            [.. metadata]
        );
    }

    /// <summary>
    /// Converts a collection of IdentityError to Errors.
    /// </summary>
    public static List<Error> ToErrors(
        this IEnumerable<IdentityError>? errors,
        params (string Key, object? Value)[] additionalMetadata)
    {
        if (errors is null)
            return [];

        var errorList = new List<Error>(capacity: 4);
        errorList.AddRange(errors.Select(error => error.ToError(additionalMetadata)));

        return errorList;
    }

    /// <summary>
    /// Converts IdentityResult to a list of Errors.
    /// </summary>
    public static List<Error> ToErrorList(
        this IdentityResult identityResult,
        params (string Key, object? Value)[] additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(identityResult);

        if (identityResult.Succeeded)
            return [];

        return identityResult.Errors.ToErrors(additionalMetadata);
    }

    /// <summary>
    /// Converts IdentityResult to a typed failure Result{T}. Throws if the result is successful.
    /// </summary>
    public static Result<T> ToResult<T>(
        this IdentityResult identityResult,
        params (string Key, object? Value)[] additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(identityResult);

        if (identityResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Cannot convert successful IdentityResult to failure.");
        }

        return Result<T>.Validation(
            errors: identityResult.Errors.ToErrors(additionalMetadata));
    }

    /// <summary>
    /// Converts IdentityResult to a Result. Returns Ok on success, Validation failure on failure.
    /// </summary>
    public static Result ToResult(
        this IdentityResult identityResult,
        params (string Key, object? Value)[] additionalMetadata)
    {
        ArgumentNullException.ThrowIfNull(identityResult);

        if (identityResult.Succeeded)
            return Result.Ok();

        return Result.Validation(
            errors: identityResult.Errors.ToErrors(additionalMetadata));
    }

    /// <summary>
    /// Checks if IdentityResult has errors.
    /// </summary>
    public static bool HasErrors(this IdentityResult? identityResult)
        => identityResult is not null &&
           !identityResult.Succeeded &&
           identityResult.Errors.Any();
}