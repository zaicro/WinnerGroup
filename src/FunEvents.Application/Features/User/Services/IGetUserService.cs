using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Model.Queries;

namespace FunEvents.Application.Features.User.Services;

public interface IGetUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<UserDto> GetByEmailAsync(GetByEmailQuery request, CancellationToken cancellationToken);

    Task<UserDto> GetByUserAsync(GetByUserQuery request, CancellationToken cancellationToken);
}
