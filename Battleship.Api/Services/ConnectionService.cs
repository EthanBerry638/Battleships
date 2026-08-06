using Battleship.Api.Repositories;
using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public class ConnectionService (IConnectionRepository connectionRepository)
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    
    public bool AddConnection(AddConnectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ConnectionId) || request.PlayerId == Guid.Empty)
            throw new ArgumentException("ConnectionId and/or Guid cannot be null or empty.");

        return _connectionRepository.TryAddConnection(request);
    }
}