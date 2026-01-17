using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityManager.Models
{
    public class LoginResult
    {

        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public User User { get; set; }
        public int SessionId { get; set; }

        public static LoginResult Success(User user, int sessionId)
        {
            return new LoginResult
            {
                IsSuccess = true,
                User = user,
                SessionId = sessionId
            };
        }

        public static LoginResult Failed(string error)
        {
            return new LoginResult
            {
                IsSuccess = false,
                ErrorMessage = error
            };
        }
    }
}
