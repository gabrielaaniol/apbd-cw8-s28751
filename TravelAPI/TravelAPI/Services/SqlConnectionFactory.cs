using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace TravelAPI.Services;

public class SqlConnectionFactory
{
    private readonly IConfiguration _config;

    public SqlConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public SqlConnection Create()
    {
        return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }
}