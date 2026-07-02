namespace Shared.Application.Models.Results;

public static class ResultExtensions
{
    public static Optional<T> ToOptional<T>(this Result<T> result)
    {
        return result.IsSuccess ? Optional<T>.Some(result.Value) : Optional<T>.None;
    }
}
