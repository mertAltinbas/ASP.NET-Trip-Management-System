using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<bool> DoesClientExist(int clientId);
    Task<List<ClientTripDTO>> GetTripsByClientId(int clientId);
    Task<int> CreateClientAsync(ClientCreateDTO clientCreateDTO);
    Task<bool> RegisterClientForTrip(int clientId, int tripId);
    Task<bool> RemoveClientFromTrip(int clientId, int tripId);
    Task<bool> DoesPeselExist(string pesel);
}