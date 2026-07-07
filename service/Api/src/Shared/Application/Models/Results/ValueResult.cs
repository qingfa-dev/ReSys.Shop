using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Shared.Application.Models.Results;

public readonly partial record struct Result<T> : IResultRecord
{
    #region Properties
    [AllowNull]
    public T Value
    {
        get => field!;
        init;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; init; }

    public int StatusCode { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Error> Errors { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
    #endregion

    #region Compute
    [JsonIgnore]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    [JsonIgnore]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsError => IsFailure;

    #endregion

    #region Constructors
    public Result(
       bool isSuccess = ResultConstant.DefaultValues.IsSuccess,
       int statusCode = ResultConstant.DefaultValues.StatusCode,
       T? value = default,
       string? message = null,
       List<Error>? errors = null,
       params (string Key, object? Value)[] metadata)
    {
        Value = value;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        Errors = errors ?? [];
        Metadata = metadata.Length != 0 ? metadata.ToDictionary() : null;
    }
    #endregion
}

public readonly partial record struct Result<T>
{
    public List<Error> Failures => Errors;
    public static Result<T> Failure(Error error) => error;
}

public readonly partial record struct Result<T>
{
    public Error FirstFailure => Errors?.FirstOrDefault() ?? default;
}
