using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Services;

namespace FunEvents.Application.Features.User.Handlers;

internal sealed class UpdateUserHandler(IUpdateUserService service) : IRequestHandler<UpdateUserCommand, Response<UserDto>>
{
    private readonly IUpdateUserService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = await _service.UpdateUserAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<UserDto>.Success(dto);
    }
}