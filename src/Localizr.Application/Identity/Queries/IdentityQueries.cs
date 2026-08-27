using Localizr.Application.Common.Responses;
using Localizr.Application.Identity.Responses;
using Mediator;

namespace Localizr.Application.Identity.Queries;

/// <summary>Requests identity information.</summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetIdentityInfoQuery(
    string UserId)
    : IRequest<Response<IdentityInfoResponse>>;
