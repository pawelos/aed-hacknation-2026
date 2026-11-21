using Mediator;
using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Handler of a query.
/// </summary>
/// <typeparam name="TQuery">Type of the query.</typeparam>
/// <typeparam name="TResponse">Type of the returned data.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
