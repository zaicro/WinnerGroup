using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Application.Features.Event.Services.Impl;

internal sealed class UpdateEventService(IUnitOfWork unitOfWork, IEventRepository eventRepository, ILogger logger) : IUpdateEventService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IEventRepository _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<EventDto> UpdateEventAsync(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var record = await _eventRepository.GetByCodeAsync(request.Code, cancellationToken)
                ?? throw new KeyNotFoundException("Event is not registered.");

            record.Name = request.Name;
            record.EventDate = request.EventDate;
            record.Capacity = request.Capacity;

            await _eventRepository.UpdateAsync(record, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.Info(method!, "End");

            return new EventDto
            {
                Code = record.Code,
                Name = record.Name,
                EventDate = record.EventDate,
                Capacity = record.Capacity,
                Status = new OptionDto
                {
                    Code = (int)record.Status,
                    Name = record.Status.ToString()
                }
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
