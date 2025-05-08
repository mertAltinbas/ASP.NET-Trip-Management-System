using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientService : IClientsService
{
    private readonly string _connectionString;

    // Constructor - get connection string from config
    public ClientService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Check if client exists by ID
    public async Task<bool> DoesClientExist(int clientId)
    {
        var query = "SELECT 1 FROM Client WHERE IdClient = @id";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", clientId);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    // Get trips of a specific client
    public async Task<List<ClientTripDTO>> GetTripsByClientId(int clientId)
    {
        var trips = new List<ClientTripDTO>();

        var query = @"
        SELECT 
            t.IdTrip,
            t.Name,
            t.Description,
            t.DateFrom,
            t.DateTo,
            t.MaxPeople,
            ct.RegisteredAt,
            ct.PaymentDate
        FROM Client c
        JOIN Client_Trip ct ON c.IdClient = ct.IdClient
        JOIN Trip t ON ct.IdTrip = t.IdTrip
        WHERE c.IdClient = @id";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", clientId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var registeredAtInt = reader.GetInt32(reader.GetOrdinal("RegisteredAt"));
            var paymentDateInt = reader.IsDBNull(reader.GetOrdinal("PaymentDate"))
                ? (int?)null
                : reader.GetInt32(reader.GetOrdinal("PaymentDate"));

            trips.Add(new ClientTripDTO
            {
                TripId = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                DateRangeFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                DateRangeTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                RegisteredAt = registeredAtInt,
                PaymentDate = paymentDateInt
            });
        }

        return trips;
    }

    // Create a new client and return new client ID
    public async Task<int> CreateClientAsync(ClientCreateDTO clientCreateDTO)
    {
        const string query = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@FirstName", clientCreateDTO.FirstName);
        cmd.Parameters.AddWithValue("@LastName", clientCreateDTO.LastName);
        cmd.Parameters.AddWithValue("@Email", clientCreateDTO.Email);
        cmd.Parameters.AddWithValue("@Telephone", clientCreateDTO.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", clientCreateDTO.Pesel);
        
        await conn.OpenAsync();
        var insertedId = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(insertedId);
    }

    // Register a client for a trip
    public async Task<bool> RegisterClientForTrip(int clientId, int tripId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        
        // Is Client exist?
        bool clientExists = await DoesClientExist(clientId);
        if (!clientExists)
            return false;
        
        // Is Trip exist?
        var checkTripCmd = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @id", conn);
        checkTripCmd.Parameters.AddWithValue("@id", tripId);
        var maxPeopleObj = await checkTripCmd.ExecuteScalarAsync();
        if (maxPeopleObj == null)
            return false;
        
        // Count how many person already registered
        var maxPeople = (int)maxPeopleObj;    
        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId", conn);
        countCmd.Parameters.AddWithValue("@tripId", tripId);
        var registeredCount = (int)await countCmd.ExecuteScalarAsync();
        
        if (registeredCount >= maxPeople)
            throw new InvalidOperationException("Trip is full.");
        
        // Is it already registered?
        var checkDuplicateCmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @cid AND IdTrip = @tid", conn);
        checkDuplicateCmd.Parameters.AddWithValue("@cid", clientId);
        checkDuplicateCmd.Parameters.AddWithValue("@tid", tripId);
        var alreadyRegistered = await checkDuplicateCmd.ExecuteScalarAsync();
        if (alreadyRegistered != null)
            throw new InvalidOperationException("Client is already registered for this trip.");
        
        // Insert
        var insertCmd = new SqlCommand(@"
        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
        VALUES (@cid, @tid, @registeredAt)", conn);
        insertCmd.Parameters.AddWithValue("@cid", clientId);
        insertCmd.Parameters.AddWithValue("@tid", tripId);
        insertCmd.Parameters.AddWithValue("@registeredAt", 08052025);

        await insertCmd.ExecuteNonQueryAsync();
        
        return true;
    }

    // Remove a client's registration from a trip
    public async Task<bool> RemoveClientFromTrip(int clientId, int tripId)
    {
        var queryCheck = @"SELECT 1 FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId";
        var queryDelete = @"DELETE FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId";
        
        using var conn = new SqlConnection(_connectionString);
        using var checkCmd = new SqlCommand(queryCheck, conn);
        checkCmd.Parameters.AddWithValue("@clientId", clientId);
        checkCmd.Parameters.AddWithValue("@tripId", tripId);
        
        await conn.OpenAsync();
        var exists = await checkCmd.ExecuteScalarAsync();
        
        if (exists == null)
            return false;
        
        using var deletedCmd = new SqlCommand(queryDelete, conn);
        deletedCmd.Parameters.AddWithValue("@clientId", clientId);
        deletedCmd.Parameters.AddWithValue("@tripId", tripId);
        
        await deletedCmd.ExecuteNonQueryAsync();

        return true;
    }

    // Check if a PESEL already exists in the database
    public async Task<bool> DoesPeselExist(string pesel)
    {
        var query = "SELECT 1 FROM Client WHERE Pesel = @pesel";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@pesel", pesel);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }
}