using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace TravelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DbTestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DbTestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult TestConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return Ok("✅ Połączenie z bazą danych działa!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Błąd połączenia z bazą: {ex.Message}");
            }
        }
    }
}