namespace Shared.Application.Models.Results;

public interface IResultRecord
{
    #region Interface Members
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public string? Message { get; }
    public List<Error> Errors { get; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; }
    #endregion
}