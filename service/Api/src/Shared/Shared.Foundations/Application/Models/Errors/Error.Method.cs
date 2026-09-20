namespace Shared.Application.Models.Errors;

public readonly partial struct Error
{
    public static Error Create(
       string code,
       string message,
       int type = ErrorConstant.DefaultValues.Type,
       params (string Key, object? Value)[] metadata)
    {
        return new Error(
            code,
            message,
            type,
            metadata.Length != 0 ? metadata.ToDictionary() : null);
    }

    public static Error BadRequest(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.BadRequest, metadata);

    public static Error Unauthorized(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.Unauthorized, metadata);

    public static Error Forbidden(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.Forbidden, metadata);

    public static Error NotFound(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.NotFound, metadata);

    public static Error Conflict(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.Conflict, metadata);

    public static Error Validation(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.Validation, metadata);

    public static Error Unexpected(
        string code,
        string message,
        params (string Key, object? Value)[] metadata)
        => Create(code, message, ErrorType.Unexpected, metadata);
}