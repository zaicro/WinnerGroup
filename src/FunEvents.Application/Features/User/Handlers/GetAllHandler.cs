using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Model.Queries;
using FunEvents.Application.Features.User.Services;

namespace FunEvents.Application.Features.User.Handlers;

internal sealed class GetAllHandler(IGetUserService service) : IRequestHandler<GetAllQuery, Response<List<UserDto>>>
{
    private readonly IGetUserService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<List<UserDto>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Response<List<UserDto>>.Success(result);
    }
}