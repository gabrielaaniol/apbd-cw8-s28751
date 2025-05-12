using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace TravelAPI.Services;

//utworzylam pomocnicza klase do laczenia sie z baza
public class SqlConnectionFactory
{
    private readonly IConfiguration _configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SqlConnection Create()
    {
        return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
}