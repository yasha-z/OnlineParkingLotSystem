using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineParkingLotSystem.Application.DTOs.Requests;
using OnlineParkingLotSystem.Application.Services;

namespace OnlineParkingLotSystem.Controllers;

[ApiController]
[Route("api/parking")]
public class ParkingController : ControllerBase
{
    private readonly IParkingService _parkingService;

    public ParkingController(IParkingService parkingService)
    {
        _parkingService = parkingService;
    }

    [HttpPost("park")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<object>> ParkVehicle(
        [FromBody] ParkVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await _parkingService.ParkVehicleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTicketById), new { ticketId = ticket.TicketId }, ticket);
    }

    [HttpPut("exit/{ticketId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> ExitVehicle(int ticketId, CancellationToken cancellationToken)
    {
        var result = await _parkingService.ExitVehicleAsync(ticketId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetActiveTickets(CancellationToken cancellationToken)
    {
        var tickets = await _parkingService.GetActiveTicketsAsync(cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("ticket/{ticketId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetTicketById(int ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _parkingService.GetTicketByIdAsync(ticketId, cancellationToken);
        return Ok(ticket);
    }
}
