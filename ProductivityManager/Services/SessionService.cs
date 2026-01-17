using System.Configuration;
using System.Data.SqlClient;

namespace ProductivityManager.Services
{
    public class SessionService
    {

        private readonly string _connectionString =
        ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        public int CreateSession(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
            INSERT INTO UserSessions (UserId, LoginTime)
            OUTPUT INSERTED.SessionId
            VALUES (@UserId, GETDATE())", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            return (int)cmd.ExecuteScalar();
        }
    }
}