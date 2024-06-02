using APBD_REST4.Models.DTOs;
using APBD_REST4.Repositories;

namespace APBD_REST4.Services;

public class TripService : ITripService
{
    private ITripRepositories _tripRepositories;

    public TripService(ITripRepositories tripRepositories)
    {
        _tripRepositories = tripRepositories;
    }

    public async Task<IEnumerable<TripsListDto>> GetTripsList(bool paging, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
    {
        var tripsObject = await _tripRepositories.GetTripsList(paging, cancellationToken, pageNumber, pageSize);

        return tripsObject;
    }

    public async Task<int> DeleteClient(int clientId, CancellationToken cancellationToken)
    {
        var clientRemoval = await _tripRepositories.DeleteClient(clientId, cancellationToken);

        return clientRemoval;
    }
}