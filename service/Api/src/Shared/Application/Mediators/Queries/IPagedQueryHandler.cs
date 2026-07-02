using MediatR;

namespace Shared.Application.Mediators.Queries;

/// <summary>
/// Contract for handling paged queries that return a paginated result.
/// </summary>
/// <typeparam name="TQuery">The type of the paged query being handled.</typeparam>
/// <typeparam name="TResponse">The type of the items in the paged result.</typeparam>
public interface IPagedQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, PagedResult<TResponse>>
    where TQuery : IPagedQuery<TResponse>
{
}
