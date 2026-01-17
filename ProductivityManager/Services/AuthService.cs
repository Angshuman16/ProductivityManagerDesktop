using ProductivityManager.Models;
using System.Configuration;
using System.Data.SqlClient;


namespace ProductivityManager.Services
{
    public class AuthService
    {

        private readonly string _connectionString =
        ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        public User Login(string username, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
            SELECT UserId, Username, Role
            FROM Users
            WHERE Username = @Username
              AND PasswordHash = @Password", conn);

            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", password); // hashing later

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new User
            {
                UserId = (int)reader["UserId"],
                Username = reader["Username"].ToString(),
                Role = reader["Role"].ToString()
            };
        }



    }
}