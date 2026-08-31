using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Services;

public interface ICreateUserService
{
    Task<UserDto> CreateUserAsync(CreateUserCommand request, CancellationToken cancellationToken);
}
