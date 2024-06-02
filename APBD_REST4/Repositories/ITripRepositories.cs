using APBD_REST4.Models;
using APBD_REST4.Models.DTOs;

namespace APBD_REST4.Repositories;

public interface ITripRepositories
{
    public Task<IEnumerable<TripsListDto>> GetTripsList(bool paging, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10);
    public Task<int> DeleteClient(int clientId, CancellationToken cancellationToken);
    public Task<int> AddClientToTrip(ClientAssignment clientAssignment, CancellationToken cancellationToken);
}