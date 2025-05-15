using Microsoft.AspNetCore.Mvc;
using TravelAPI.DTOs.Clients;
using TravelAPI.Services;

namespace TravelAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly SqlConnectionFactory _factory;

    public ClientsController(SqlConnectionFactory factory)
    {
        _factory = factory;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
    {
        using var connection = _factory.Create();
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT INSERTED.IdClient
            VALUES (@fn, @ln, @em, @tel, @pesel)";
        
        cmd.Parameters.AddWithValue("@fn", dto.FirstName);
        cmd.Parameters.AddWithValue("@ln", dto.LastName);
        cmd.Parameters.AddWithValue("@em", dto.Email);
        cmd.Parameters.AddWithValue("@tel", dto.Telephone);
        cmd.Parameters.AddWithValue("@pesel", dto.Pesel);

        var id = (int)await cmd.ExecuteScalarAsync();
        return CreatedAtAction(nameof(GetClientTrips), new { id }, new { IdClient = id });
    }

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        using var connection = _factory.Create();
        await connection.OpenAsync();

        var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(1) FROM Client WHERE IdClient = @id";
        check.Parameters.AddWithValue("@id", id);
        var exists = (int)await check.ExecuteScalarAsync();

        if (exists == 0)
            return NotFound("Client not found");

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo,
                   ct.RegisteredAt, ct.PaymentDate
            FROM Client_Trip ct
            JOIN Trip t ON t.IdTrip = ct.IdTrip
            WHERE ct.IdClient = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<ClientTripDto>();

        while (await reader.ReadAsync())
        {
            result.Add(new ClientTripDto
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                RegisteredAt = reader.GetDateTime(5),
                PaymentDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
            });
        }

        return Ok(result);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
    {
        using var connection = _factory.Create();
        await connection.OpenAsync();

        var check = connection.CreateCommand();
        check.CommandText = @"
            SELECT 
                (SELECT COUNT(*) FROM Client WHERE IdClient = @id),
                (SELECT COUNT(*) FROM Trip WHERE IdTrip = @tripId),
                (SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId),
                (SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId),
                (SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId)";
        check.Parameters.AddWithValue("@id", id);
        check.Parameters.AddWithValue("@tripId", tripId);

        using var reader = await check.ExecuteReaderAsync();
        await reader.ReadAsync();

        if (reader.GetInt32(0) == 0) return NotFound("Client not found");
        if (reader.GetInt32(1) == 0) return NotFound("Trip not found");
        if (reader.GetInt32(2) > 0) return Conflict("Client already registered");
        if (reader.GetInt32(3) >= reader.GetInt32(4)) return Conflict("Trip is full");

        await reader.CloseAsync();

        var insert = connection.CreateCommand();
        insert.CommandText = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@id, @tripId, GETDATE())";
        insert.Parameters.AddWithValue("@id", id);
        insert.Parameters.AddWithValue("@tripId", tripId);

        await insert.ExecuteNonQueryAsync();
        return Ok("Client registered");
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> UnregisterClientFromTrip(int id, int tripId)
    {
        using var connection = _factory.Create();
        await connection.OpenAsync();

        var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId";
        check.Parameters.AddWithValue("@id", id);
        check.Parameters.AddWithValue("@tripId", tripId);

        var exists = (int)await check.ExecuteScalarAsync();
        if (exists == 0) return NotFound("Not registered");

        var delete = connection.CreateCommand();
        delete.CommandText = "DELETE FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId";
        delete.Parameters.AddWithValue("@id", id);
        delete.Parameters.AddWithValue("@tripId", tripId);

        await delete.ExecuteNonQueryAsync();
        return Ok("Unregistered");
    }
}
