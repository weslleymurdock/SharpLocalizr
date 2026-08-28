using Localizr.Application.Common.Responses;
using Mediator;

namespace Localizr.Application.Health.Queries;

public record GetProviderNameQuery() : IRequest<Response<string>>;

public record CanConnectAsyncQuery() : IRequest<Response<bool>>;