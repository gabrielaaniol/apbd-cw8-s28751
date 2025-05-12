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

        var command = connection.CreateCommand();
        command.CommandText = "..."; 

        return result;
    }
}