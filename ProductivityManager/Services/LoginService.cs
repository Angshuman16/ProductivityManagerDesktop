using ProductivityManager.Models;
using System;

namespace ProductivityManager.Services
{
    public class LoginService
    {
        private readonly AuthService _authService;
        private readonly SessionService _sessionService;
        private readonly StatusService _statusService;

        private const string STATUS_ACTIVE = "Active";

        public LoginService()
        {
            _authService = new AuthService();
            _sessionService = new SessionService();
            _statusService = new StatusService();
        }

        public LoginResult Login(string username, string password)
        {
            // 1️⃣ Basic validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return LoginResult.Failed("Username and password are required");
            }

            // 2️⃣ Authenticate
            var user = _authService.Login(username, password);
            if (user == null)
            {
                return LoginResult.Failed("Invalid username or password");
            }

            try
            {
                // 3️⃣ Create session
                int sessionId = _sessionService.CreateSession(user.UserId);

              //  4️⃣ Log initial Active status
                _statusService.SetStatus(
                    user.UserId,
                    sessionId,
                    "Active",
                    "Login");

                // 5️⃣ Success
                return LoginResult.Success(user, sessionId);
            }
            catch (Exception ex)
            {
                return LoginResult.Failed(ex.Message);
            }
        }
    }
}
