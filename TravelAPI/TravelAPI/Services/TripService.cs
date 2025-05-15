using System.Data.SqlClient;
using TravelAPI.DTOs.Trips;

namespace TravelAPI.Services;

public class TripService
{
    private readonly SqlConnectionFactory _factory;

    public TripService(SqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<TripDto>> GetAllTripsAsync()
    {
        var result = new List<TripDto>();
        using var connection = _factory.Create();
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                   c.Name AS CountryName
            FROM Trip t
            LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
            ORDER BY t.IdTrip";

        using var reader = await cmd.ExecuteReaderAsync();
        var map = new Dictionary<int, TripDto>();

        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);

            if (!map.ContainsKey(id))
            {
                map[id] = new TripDto
                {
                    IdTrip = id,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Countries = new List<CountryDto>()
                };
            }

            if (!reader.IsDBNull(6))
            {
                map[id].Countries.Add(new CountryDto { Name = reader.GetString(6) });
            }
        }

        return map.Values;
    }
}