using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Model.Queries;
using FunEvents.Application.Features.User.Services;

namespace FunEvents.Application.Features.User.Handlers;

internal sealed class GetByUserHandler(IGetUserService service) : IRequestHandler<GetByUserQuery, Response<UserDto>>
{
    private readonly IGetUserService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<UserDto>> Handle(GetByUserQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByUserAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<UserDto>.Success(dto);
    }
}