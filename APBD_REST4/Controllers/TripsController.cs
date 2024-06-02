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
}