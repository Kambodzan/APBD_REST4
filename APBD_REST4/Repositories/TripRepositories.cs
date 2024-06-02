using APBD_REST4.Data;
using APBD_REST4.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace APBD_REST4.Repositories;

public class TripRepositories : ITripRepositories
{
    private readonly MasterContext _context = new();

    public async Task<IEnumerable<TripsListDto>> GetTripsList(bool paging, CancellationToken cancellationToken,
        int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Trips.Include(trip => trip.ClientTrips)
            .ThenInclude(clientTrip => clientTrip.IdClientNavigation).Include(trip => trip.IdCountries)
            .OrderByDescending(trip => trip.DateFrom).AsQueryable();

        var tripsCount = await query.CountAsync(cancellationToken);
        var allPages = (int)Math.Ceiling((decimal)tripsCount / (decimal)pageSize);
        // var allPages = 213;

        if (paging)
        {
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        var trips = await query.Select(trip => new TripsListDto
        {
            PageNum = pageNumber,
            PageSize = pageSize,
            AllPages = allPages,
            Name = trip.Name,
            Description = trip.Description,
            DateFrom = trip.DateFrom,
            DateTo = trip.DateTo,
            MaxPeople = trip.MaxPeople,
            Countries = trip.IdCountries.Select(country => new CountryDto
            {
                Name = country.Name
            }).ToList(),
            Clients = trip.ClientTrips.Select(client => new ClientDto
            {
                FirstName = client.IdClientNavigation.FirstName,
                LastName = client.IdClientNavigation.LastName
            }).ToList()
        }).ToListAsync(cancellationToken);

        return trips;
    }

    public async Task<int> DeleteClient(int clientId, CancellationToken cancellationToken)
    {
        if (await CheckIfClientDoesNotHaveAssignedTrips(clientId, cancellationToken))
        {
            return -1;
        }

        var clientToRemove = await _context.Clients.Where(client => client.IdClient == clientId)
            .FirstAsync(cancellationToken);

        _context.Clients.Remove(clientToRemove);

        var queryResult = await _context.SaveChangesAsync(cancellationToken);

        return queryResult;
    }

    public async Task<bool> CheckIfClientDoesNotHaveAssignedTrips(int clientId, CancellationToken cancellationToken)
    {
        var query = await _context.ClientTrips.Where(client => client.IdClient == clientId)
            .FirstOrDefaultAsync(cancellationToken);

        return query != null;
    }
}