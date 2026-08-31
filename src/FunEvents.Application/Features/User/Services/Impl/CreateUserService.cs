using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Services.Impl;

internal sealed class CreateUserService(IUnitOfWork unitOfWork, IUserRepository userRepository, ILogger logger) : ICreateUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UserDto> CreateUserAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var newRecord = new Domain.Entities.User(
                0,
                request.Username,
                request.Name,
                request.Email,
                request.Phone,
                request.Password);

            await EnsureUserDoesNotExistAsync(newRecord, cancellationToken);

            await _userRepository.AddAsync(newRecord, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.Info(method!, "End");

            return new UserDto
            {
                Username = newRecord.Username,
                Name = newRecord.Name,
                Email = newRecord.Email,
                Phone = newRecord.Phone
            };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);

            await _unitOfWork.RollBackAsync(cancellationToken);

            throw;
        }
    }

    private async Task EnsureUserDoesNotExistAsync(Domain.Entities.User user, CancellationToken cancellationToken)
    {
        var usernameExists = await _userRepository.ExistsByUsernameAsync(user.Username, cancellationToken);

        if (usernameExists) throw new ArgumentException("Username is already registered.", nameof(user.Username));

        var emailExists = await _userRepository.ExistsByEmailAsync(user.Email, cancellationToken);

        if (emailExists) throw new ArgumentException("Email is already registered.", nameof(user.Email));
    }
}