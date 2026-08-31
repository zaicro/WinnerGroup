using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Model.Queries;
using FunEvents.Application.Features.User.Services;

namespace FunEvents.Application.Features.User.Handlers;

internal sealed class GetByEmailHandler(IGetUserService service) : IRequestHandler<GetByEmailQuery, Response<UserDto>>
{
    private readonly IGetUserService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<UserDto>> Handle(GetByEmailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByEmailAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<UserDto>.Success(dto);
    }
}