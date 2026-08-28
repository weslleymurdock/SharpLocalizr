using Localizr.Application.Common.Responses;
using Localizr.Application.Health.Abstractions;
using Localizr.Application.Health.Queries;
using Mediator;

namespace Localizr.Application.Health.Handlers;

public sealed class HealthHandlers(IHealthService service) : IRequestHandler<GetProviderNameQuery, Response<string>>,
    IRequestHandler<CanConnectAsyncQuery, Response<bool>>
{
    public async ValueTask<Response<bool>> Handle(CanConnectAsyncQuery request, CancellationToken cancellationToken) =>
        await service.CanConnectAsync(cancellationToken) ? Response.Success(true) : Response.Failure<bool>();

    public async ValueTask<Response<string>> Handle(GetProviderNameQuery request, CancellationToken cancellationToken) =>
        Response.Success(await service.GetProviderNameAsync());
}
