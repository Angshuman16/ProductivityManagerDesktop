using System.Configuration;
using System.Data.SqlClient;

namespace ProductivityManager.Services
{
    public class StatusService
    {
        private readonly string _connectionString =
        ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        public void LogStatus(int userId, int sessionId, string status, string reason)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
            INSERT INTO UserStatusLog
            (UserId, SessionId, Status, StatusTime, Reason)
            VALUES (@UserId, @SessionId, @Status, GETDATE(), @Reason)", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Reason", reason ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
}