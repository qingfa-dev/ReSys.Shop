using System.Text.Json.Serialization;

namespace Shared.Application.Models.Results;

public readonly partial record struct Result : IResultRecord
{
    #region Properties
    public bool IsSuccess { get; }

    public int StatusCode { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; }

    public List<Error> Errors { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object?>? Metadata { get; }
    #endregion

    #region Compute
    [JsonIgnore]
    public bool IsFailure => !IsSuccess;
    #endregion

    #region Constructors
    [JsonConstructor]
    internal Result(
        bool isSuccess = ResultConstant.DefaultValues.IsSuccess,
        int statusCode = ResultConstant.DefaultValues.StatusCode,
        string? message = null,
        List<Error>? errors = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        Errors = errors ?? [];
        Metadata = metadata;
    }
    #endregion
}
