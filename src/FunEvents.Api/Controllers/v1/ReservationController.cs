using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.Queries;

namespace FunEvents.Api.Controllers.v1;

[ApiVersion("1.0")]
public class ReservationController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateReservationCommand command, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateReservationCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("getAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetAllQuery command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("getByCode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetByCodeQuery command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("getByEvent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetByEventQuery command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("getByUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetByUserQuery command)
    {
        return Ok(await Mediator.Send(command));
    }
}
