using Microsoft.Data.SqlClient;

namespace ServiceLayer.Services
{
    public class ConnectionManager
    {
        public static string? _connection;

        public ConnectionManager(string connection)
        {
            _connection = connection;
        }

        public static SqlConnection GetSqlConnection()
        {
            SqlConnection _sqlConnection = new(_connection);
            return _sqlConnection;
        }


    }
}