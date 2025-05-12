using Microsoft.AspNetCore.Mvc;
using TravelAPI.DTOs.Trips;
using TravelAPI.Services;

namespace TravelAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly SqlConnectionFactory _factory;

    public TripsController(SqlConnectionFactory factory)
    {
        _factory = factory;
    }

    //lacze sie z baza danych i pobieram dane, i tworze obiekty TripDto
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = new List<TripDto>();
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
        var tripMap = new Dictionary<int, TripDto>();

        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);

            if (!tripMap.ContainsKey(id))
            {
                tripMap[id] = new TripDto
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
                tripMap[id].Countries.Add(new CountryDto { Name = reader.GetString(6) });
            }
        }

        return Ok(tripMap.Values);
    }
}