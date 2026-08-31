using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Services.Impl;

internal sealed class UpdateReservationService(IUnitOfWork unitOfWork,
    IReservationRepository reservationRepository,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    ILogger logger) : IUpdateReservationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IReservationRepository _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
    private readonly IEventRepository _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ReservationDto> UpdateReservationAsync(UpdateReservationCommand request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();
        try
        {
            _logger.Info(method!, "Start");

            var record = await _reservationRepository.GetByCodeAsync(request.Code, cancellationToken);

            record.Quantity = request.Quantity;
            record.Status = (ReservationStatus)request.Status.Code;

            await _reservationRepository.UpdateAsync(record, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            var users = await _userRepository.GetAllAsync(cancellationToken);
            var user = users.FirstOrDefault(u => u.Id == record.UserId);

            var events = await _eventRepository.GetAllAsync(cancellationToken);
            var @event = events.FirstOrDefault(u => u.Id == record.EventId);

            _logger.Info(method!, "End");

            return new ReservationDto
            {
                Code = record.ReservationCode,
                EventCode = @event.Code,
                UserName = user.Username,
                Quantity = record.Quantity,
                Channel = new OptionDto
                {
                    Code = (int)record.Channel,
                    Name = record.Channel.ToString()
                },
                Status = new OptionDto
                {
                    Code = (int)record.Status,
                    Name = record.Status.ToString()
                },
                CreatedAt = record.CreatedAt
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
