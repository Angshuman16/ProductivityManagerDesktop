using System.Configuration;
using System.Data.SqlClient;

namespace ProductivityManager.Services
{
    public class StatusService
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        public void ChangeStatus(
            int userId,
            int sessionId,
            string newStatus,
            string reason)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // 1️⃣ Get current open status
                string currentStatus = GetCurrentOpenStatus(conn, tran, userId, sessionId);

                // 2️⃣ If same status → do nothing
                if (currentStatus == newStatus)
                {
                    tran.Commit();
                    return;
                }

                // 3️⃣ Close existing open status
                CloseCurrentStatus(conn, tran, userId, sessionId);

                // 4️⃣ Insert new status
                InsertNewStatus(conn, tran, userId, sessionId, newStatus, reason);

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Used during logout to simply close the open status.
        /// No new status row is inserted.
        /// </summary>
        public void CloseStatusOnly(int userId, int sessionId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

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
        /// Closes the currently active status for the session (if any).
        /// </summary>
        private void CloseCurrentStatus(
            SqlConnection conn,
            SqlTransaction tran,
            int userId,
            int sessionId)
        {
            using var cmd = new SqlCommand(@"
                UPDATE UserStatusLog
                SET StatusEndTime = GETDATE()
                WHERE UserId = @UserId
                  AND SessionId = @SessionId
                  AND StatusEndTime IS NULL
            ", conn, tran);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts a new status entry.
        /// </summary>
        private void InsertNewStatus(
            SqlConnection conn,
            SqlTransaction tran,
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
            ", conn, tran);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Reason", reason);

            cmd.ExecuteNonQuery();
        }

        private string GetCurrentOpenStatus(
            SqlConnection conn,
            SqlTransaction tran,
            int userId,
            int sessionId)
        {
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 Status
                FROM UserStatusLog
                WHERE UserId = @UserId
                  AND SessionId = @SessionId
                  AND StatusEndTime IS NULL
                ORDER BY StatusTime DESC
            ", conn, tran);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }
}