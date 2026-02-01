using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityManager.Services
{
    public class LogoutService
    {

        private readonly StatusService _statusService;
        private readonly SessionService _sessionService;

        public LogoutService()
        {
            _statusService = new StatusService();
            _sessionService = new SessionService();
        }

        public void Logout(int userId, int sessionId)
        {
            // 1️⃣ Close current status
            _statusService.SetStatus(
                userId,
                sessionId,
                "Inactive",
                "Logout"
            );

            // 2️⃣ End session
            _sessionService.EndSession(sessionId);
        }
    }
}
