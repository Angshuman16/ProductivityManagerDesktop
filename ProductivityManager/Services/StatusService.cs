using System.Configuration;
using System.Data.SqlClient;

namespace ProductivityManager.Services
{
    public class StatusService
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        /// <summary>
        /// Sets a new status for the user within a session.
        /// Automatically closes any previously open status.
        /// </summary>
        public void SetStatus(
            int userId,
            int sessionId,
            string newStatus,
            string reason)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            CloseCurrentStatus(conn, userId, sessionId);
            InsertNewStatus(conn, userId, sessionId, newStatus, reason);
        }

        /// <summary>
        /// Closes the currently active status for the session (if any).
        /// </summary>
        private void CloseCurrentStatus(
            SqlConnection conn,
            int userId,
            int sessionId)
        {
            using var cmd = new SqlCommand(@"
                UPDATE UserStatusLog
                SET StatusEndTime = GETDATE()
                WHERE UserId = @UserId
                  AND SessionId = @SessionId
                  AND StatusEndTime IS NULL
            ", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts a new status entry for the session.
        /// </summary>
        private void InsertNewStatus(
            SqlConnection conn,
            int userId,
            int sessionId,
            string status,
            string reason)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO UserStatusLog
                (UserId, SessionId, Status, StatusTime, Reason)
                VALUES
                (@UserId, @SessionId, @Status, GETDATE(), @Reason)
            ", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Reason", reason);

            cmd.ExecuteNonQuery();
        }
    }
}
