using MediatR;

namespace Shared.Application.Mediators.Queries;

/// <summary>
/// Marker interface for paged queries that return a paginated result.
/// </summary>
/// <typeparam name="TResponse">The type of items in the paged result.</typeparam>
public interface IPagedQuery<TResponse> : IRequest<PagedResult<TResponse>>;
