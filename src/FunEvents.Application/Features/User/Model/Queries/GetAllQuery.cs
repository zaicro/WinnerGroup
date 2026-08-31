using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Model.Queries;

public class GetAllQuery : IRequest<Response<List<UserDto>>>
{
}
