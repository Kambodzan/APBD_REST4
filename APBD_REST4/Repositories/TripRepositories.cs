using APBD_REST4.Data;
using APBD_REST4.Models;
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

    public async Task<int> AddClientToTrip(ClientAssignment clientAssignment, CancellationToken cancellationToken)
    {
        if (await CheckIfPeselExists(clientAssignment, cancellationToken))
        {
            return 1;
        }

        if (await CheckIfClientIsNotAlreadyRegisteredOnTrip(clientAssignment, cancellationToken))
        {
            return 2;
        }

        if (await CheckIfTripExists(clientAssignment, cancellationToken))
        {
            return 3;
        }

        if (await CheckIfTripDateIsValid(clientAssignment, cancellationToken))
        {
            return 4;
        }

        using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                var newTripAssignment = new ClientTrip
                {
                    IdClient = await _context.Clients.Where(client => client.Pesel == clientAssignment.Pesel)
                        .Select(client => client.IdClient).FirstOrDefaultAsync(cancellationToken),
                    IdTrip = clientAssignment.IdTrip,
                    RegisteredAt = DateTime.Now,
                    PaymentDate = clientAssignment.PaymentDate

                };

                _context.ClientTrips.Add(newTripAssignment);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await transaction.RollbackAsync(cancellationToken);
                return 5;
            }
        }
    }

    public async Task<bool> CheckIfClientDoesNotHaveAssignedTrips(int clientId, CancellationToken cancellationToken)
    {
        var query = await _context.ClientTrips.Where(client => client.IdClient == clientId)
            .FirstOrDefaultAsync(cancellationToken);

        return query == null;
    }

    public async Task<bool> CheckIfPeselExists(ClientAssignment clientAssignment, CancellationToken cancellationToken)
    {
        var query = await _context.Clients.Where(client => client.Pesel == clientAssignment.Pesel)
            .FirstOrDefaultAsync(cancellationToken);

        return query == null;
    }

    public async Task<bool> CheckIfClientIsNotAlreadyRegisteredOnTrip(ClientAssignment clientAssignment,
        CancellationToken cancellationToken)
    {
        var isRegistered = await (from client in _context.Clients
                join clientTrip in _context.ClientTrips on client.IdClient equals clientTrip.IdClient
                where client.Pesel == clientAssignment.Pesel && clientTrip.IdTrip == clientAssignment.IdTrip
                select clientTrip)
            .AnyAsync(cancellationToken);

        // True jeśli nie jest zarejestrowany
        return isRegistered;
    }

    public async Task<bool> CheckIfTripExists(ClientAssignment clientAssignment, CancellationToken cancellationToken)
    {
        var doesExists = await _context.Trips.Where(trip => trip.IdTrip == clientAssignment.IdTrip)
            .FirstOrDefaultAsync(cancellationToken);

        return doesExists == null;
    }

    public async Task<bool> CheckIfTripDateIsValid(ClientAssignment clientAssignment,
        CancellationToken cancellationToken)
    {
        var TripDate = await _context.Trips.Where(trip => trip.IdTrip == clientAssignment.IdTrip)
            .Select(trip => trip.DateFrom).FirstOrDefaultAsync(cancellationToken);

        // True jeśli wycieczka już się odbyła
        return TripDate < DateTime.Now;
    }
}