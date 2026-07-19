namespace Shared.Application.Models.Responses;

public interface IResponse
{
    Guid Id { get; }
}

public abstract record Response : IResponse
{
    public Guid Id { get; init; }
}
