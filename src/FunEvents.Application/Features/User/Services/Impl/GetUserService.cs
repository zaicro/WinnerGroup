using FunEvents.Application.Features.User.Model.DTOs;
using FunEvents.Application.Features.User.Model.Queries;

namespace FunEvents.Application.Features.User.Services.Impl;

internal sealed class GetUserService(IUserRepository userRepository, ILogger logger) : IGetUserService
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _userRepository.GetAllAsync(cancellationToken);

            _logger.Info(method!, "End");

            return
            [
                .. result.Select(x => new UserDto
                {
                    Username = x.Username,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone
                })
            ];
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }

    public async Task<UserDto> GetByEmailAsync(GetByEmailQuery request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            _logger.Info(method!, "End");

            return result is null
                ? throw new KeyNotFoundException("User was not found.")
                : new UserDto
                {
                    Username = result!.Username,
                    Name = result.Name,
                    Email = result.Email,
                    Phone = result.Phone
                };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }

    public async Task<UserDto> GetByUserAsync(GetByUserQuery request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _userRepository.GetByUsernameAsync(request.User, cancellationToken);

            if (result is null) throw new KeyNotFoundException("User was not found.");

            return new UserDto
            {
                Username = result!.Username,
                Name = result.Name,
                Email = result.Email,
                Phone = result.Phone
            };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }
}