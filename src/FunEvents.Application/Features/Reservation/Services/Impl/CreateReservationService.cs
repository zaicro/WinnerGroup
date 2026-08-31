using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Services.Impl;

internal sealed class CreateReservationService(IUnitOfWork unitOfWork, 
    IReservationRepository reservationRepository,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    ILogger logger) : ICreateReservationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IReservationRepository _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
    private readonly IEventRepository _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var user = await _userRepository.GetByUsernameAsync(request.UserName, cancellationToken);
            var @event = await _eventRepository.GetByCodeAsync(request.EventCode, cancellationToken);

            if (user == null) throw new ArgumentException("UserName is not already registered.", nameof(request.UserName));

            if (@event == null) throw new ArgumentException("Event is not already registered.", nameof(request.EventCode));

            var newRecord = new Domain.Entities.Reservation(
                request.Code,
                @event.Id,
                user.Id,
                request.Quantity,
                ReservationChannel.Internet,
                ReservationStatus.Reserved);

            await EnsureReservationDoesNotExistAsync(newRecord, cancellationToken);

            await _reservationRepository.AddAsync(newRecord, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.Info(method!, "End");

            return new ReservationDto
            {
                Code = newRecord.ReservationCode,
                EventCode = @event.Code,
                UserName = user.Username,
                Quantity = newRecord.Quantity,
                Channel = new OptionDto
                {
                    Code = (int)newRecord.Channel,
                    Name = newRecord.Channel.ToString()
                },
                Status = new OptionDto
                {
                    Code = (int)newRecord.Status,
                    Name = newRecord.Status.ToString()
                },
                CreatedAt = newRecord.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);

            await _unitOfWork.RollBackAsync(cancellationToken);

            throw;
        }
    }

    private async Task EnsureReservationDoesNotExistAsync(Domain.Entities.Reservation reservation, CancellationToken cancellationToken)
    {
        var usernameExists = await _reservationRepository.ExistsAsync(reservation.ReservationCode, cancellationToken);

        if (usernameExists) throw new ArgumentException("Reservation is already registered.", nameof(reservation.ReservationCode));
    }
}
