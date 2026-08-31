using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Models.Queries;
using FunEvents.Domain.Entities;

namespace FunEvents.Application.Features.Reservation.Services.Impl;

internal sealed class GetReservationService(IReservationRepository reservationRepository,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    ILogger logger) : IGetReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
    private readonly IEventRepository _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<ReservationDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _reservationRepository.GetAllAsync(cancellationToken);

            _logger.Info(method!, "End");

            return
            [
                .. result.Select(x => new ReservationDto
                {
                    Code = x.ReservationCode,
                    EventCode = GetUserAndEvent(x).@event.Code,
                    UserName = GetUserAndEvent(x).user.Username,
                    Quantity = x.Quantity,
                    Channel = new OptionDto
                    {
                        Code = (int)x.Channel,
                        Name = x.Channel.ToString()
                    },
                    Status = new OptionDto
                    {
                        Code = (int)x.Status,
                        Name = x.Status.ToString()
                    },
                    CreatedAt = x.CreatedAt
                })
            ];
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }

    public async Task<ReservationDto> GetByCodeAsync(GetByCodeQuery request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _reservationRepository.GetByCodeAsync(request.Code, cancellationToken);

            _logger.Info(method!, "End");

            return result is null
                ? throw new KeyNotFoundException("Reservation was not found.")
                : new ReservationDto
                {
                    Code = result.ReservationCode,
                    EventCode = GetUserAndEvent(result).@event.Code,
                    UserName = GetUserAndEvent(result).user.Username,
                    Quantity = result.Quantity,
                    Channel = new OptionDto
                    {
                        Code = (int)result.Channel,
                        Name = result.Channel.ToString()
                    },
                    Status = new OptionDto
                    {
                        Code = (int)result.Status,
                        Name = result.Status.ToString()
                    },
                    CreatedAt = result.CreatedAt
                };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }

    public async Task<List<ReservationDto>> GetByEventAsync(GetByEventQuery request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var @event = await _eventRepository.GetByCodeAsync(request.EventCode);

            if (@event == null) throw new KeyNotFoundException("Event without Reservation.");

            var result = await _reservationRepository.GetByEventAsync(@event.Id, cancellationToken);

            _logger.Info(method!, "End");

            return
            [
                .. result.Select(x => new ReservationDto
                {
                    Code = x.ReservationCode,
                    EventCode = GetUserAndEvent(x).@event.Code,
                    UserName = GetUserAndEvent(x).user.Username,
                    Quantity = x.Quantity,
                    Channel = new OptionDto
                    {
                        Code = (int)x.Channel,
                        Name = x.Channel.ToString()
                    },
                    Status = new OptionDto
                    {
                        Code = (int)x.Status,
                        Name = x.Status.ToString()
                    },
                    CreatedAt = x.CreatedAt
                })
            ];
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }

    public async Task<List<ReservationDto>> GetByUserAsync(GetByUserQuery request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var user = await _userRepository.GetByUsernameAsync(request.UserName);

            if (user == null) throw new KeyNotFoundException("User without Reservation.");

            var result = await _reservationRepository.GetByUserAsync(user.Id, cancellationToken);

            _logger.Info(method!, "End");

            return
            [
                .. result.Select(x => new ReservationDto
                {
                    Code = x.ReservationCode,
                    EventCode = GetUserAndEvent(x).@event.Code,
                    UserName = GetUserAndEvent(x).user.Username,
                    Quantity = x.Quantity,
                    Channel = new OptionDto
                    {
                        Code = (int)x.Channel,
                        Name = x.Channel.ToString()
                    },
                    Status = new OptionDto
                    {
                        Code = (int)x.Status,
                        Name = x.Status.ToString()
                    },
                    CreatedAt = x.CreatedAt
                })
            ];
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            throw;
        }
    }

    private (Domain.Entities.User user, Domain.Entities.Event @event) GetUserAndEvent(Domain.Entities.Reservation reservation)
    {
        var users = _userRepository.GetAllAsync().Result;
        var user = users.FirstOrDefault(u => u.Id == reservation.UserId);
        var events = _eventRepository.GetAllAsync().Result;
        var @event = events.FirstOrDefault(e => e.Id == reservation.EventId);
        return (user, @event);
    }
}
