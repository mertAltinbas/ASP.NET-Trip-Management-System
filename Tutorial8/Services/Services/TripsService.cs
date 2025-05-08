using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString;

    // We're taking connection string from .json file
    public TripsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new Dictionary<int, TripDTO>();

        var command = @"
        SELECT 
            t.IdTrip, 
            t.Name AS TripName, 
            t.Description, 
            t.DateFrom,
            t.DateTo,
            t.MaxPeople,
            c.Name AS CountryName
        FROM Trip t
        LEFT JOIN Country_Trip ct ON ct.IdTrip = t.IdTrip
        LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
        ORDER BY t.IdTrip;
        ";

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                string tripName = reader.GetString(reader.GetOrdinal("TripName"));
                string tripDescription = reader.GetString(reader.GetOrdinal("Description"));
                DateTime dateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom"));
                DateTime dateTo = reader.GetDateTime(reader.GetOrdinal("DateTo"));
                int maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));
                string countryName = reader.GetString(reader.GetOrdinal("CountryName"));

                // Converting dates
                int dateRangeFrom = int.Parse(dateFrom.ToString("ddMMyyyy"));
                int dateRangeTo = int.Parse(dateTo.ToString("ddMMyyyy"));
                
                if (!trips.ContainsKey(idTrip))
                {
                    trips[idTrip] = new TripDTO
                    {
                        Id = idTrip,
                        Name = tripName,
                        Description = tripDescription,
                        DateRangeFrom = dateRangeFrom,
                        DateRangeTo = dateRangeTo,
                        MaxPeople = maxPeople,
                        Countries = new List<CountryDTO>()
                    };
                }

                if (countryName != null && !trips[idTrip].Countries.Any(c => c.Name == countryName))
                {
                    trips[idTrip].Countries.Add(new CountryDTO { Name = countryName });
                }
            }
        }

        return trips.Values.ToList();
    }
}