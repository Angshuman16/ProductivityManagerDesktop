using ProductivityManager.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityManager.Services
{
    public class SignupService
    {


        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        public SignupResult Signup(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return SignupResult.Failed("Username and password are required");
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // 1️⃣ Check if username already exists
            var checkCmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE Username = @Username",
                conn);

            checkCmd.Parameters.AddWithValue("@Username", username);

            int exists = (int)checkCmd.ExecuteScalar();
            if (exists > 0)
            {
                return SignupResult.Failed("Username already exists");
            }

            // 2️⃣ Insert user
            var insertCmd = new SqlCommand(@"
                INSERT INTO Users (Username, PasswordHash, Role)
                VALUES (@Username, @Password, 'User')", conn);

            insertCmd.Parameters.AddWithValue("@Username", username);
            insertCmd.Parameters.AddWithValue("@Password", password); // hashing later

            insertCmd.ExecuteNonQuery();

            return SignupResult.Success();
        }
    
}
}
