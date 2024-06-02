using APBD_REST4.Models;
using APBD_REST4.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace APBD_REST4.Controllers;

[ApiController]
[Route("/api/trips")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTripsList(bool paging, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _tripService.GetTripsList(paging, cancellationToken, pageNumber, pageSize);
        
        return Ok(result);
    }

    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteClient(int clientId, CancellationToken cancellationToken)
    {
        var result = await _tripService.DeleteClient(clientId, cancellationToken);

        if (result == -1)
        {
            return BadRequest("This Client Id is assigned to trip!");
        }
        else
        {
            return Ok("Client removed successfully");
        }
        
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AddClientToTrip(ClientAssignment clientAssignment,
        CancellationToken cancellationToken, int idTrip)
    {
        var result = await _tripService.AddClientToTrip(clientAssignment, cancellationToken);

        if (idTrip != clientAssignment.IdTrip)
        {
            return BadRequest("Trip IDs are not the same");
        }
        
        if (result == 1)
        {
            return BadRequest("Client with this PESEL number does not exists");
        }
        
        if (result == 2)
        {
            return BadRequest("This client is already registered on this trip");
        }
        
        if (result == 3)
        {
            return BadRequest("This trip does not exists");
        }
        
        if (result == 4)
        {
            return BadRequest("Trip Date is invalid - trip has already started");
        }
        
        if (result == 5)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error while saving changes to database");
        }
        
        return Ok("Client added to trip successfully");
    }
}