using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Services;

namespace FunEvents.Application.Features.User.Handlers;

internal sealed class CreateUserHandler(ICreateUserService service) : IRequestHandler<CreateUserCommand, Response<UserDto>>
{
    private readonly ICreateUserService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = await _service.CreateUserAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<UserDto>.Success(dto);
    }
}