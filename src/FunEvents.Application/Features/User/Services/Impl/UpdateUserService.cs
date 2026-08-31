using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Services.Impl;

internal sealed class UpdateUserService(IUnitOfWork unitOfWork, IUserRepository userRepository, ILogger logger) : IUpdateUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UserDto> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var record = await _userRepository.GetByUsernameAsync(request.UserName, cancellationToken) 
                ?? throw new KeyNotFoundException("Username is not registered.");

            record.Name = request.Name;
            record.Phone = request.Phone;

            await _userRepository.UpdateAsync(record, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.Info(method!, "End");

            return new UserDto
            {
                Username = record.Username,
                Name = record.Name,
                Email = record.Email,
                Phone = record.Phone
            };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);

            await _unitOfWork.RollBackAsync(cancellationToken);

            throw;
        }
    }
}
