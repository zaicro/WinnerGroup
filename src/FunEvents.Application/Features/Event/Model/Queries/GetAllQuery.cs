using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Application.Features.Event.Model.Queries;

public class GetAllQuery : IRequest<Response<List<EventDto>>>
{
}
