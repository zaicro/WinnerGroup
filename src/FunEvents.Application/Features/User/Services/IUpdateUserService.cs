using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Services;

public interface IUpdateUserService
{
    Task<UserDto> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellationToken);
}
