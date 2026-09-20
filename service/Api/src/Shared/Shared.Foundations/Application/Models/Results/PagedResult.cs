using System.Text.Json.Serialization;

namespace Shared.Application.Models.Results;

public readonly partial record struct PagedResult<T>
{
    #region Properties
    public IEnumerable<T> Items { get; init; }
    [JsonPropertyName("page")]
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public long TotalCount { get; init; }
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
    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);
    #endregion
}