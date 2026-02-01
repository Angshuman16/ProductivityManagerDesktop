using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityManager.Models
{
    public class UserSessionContext
    {

        public int UserId { get; set; }
        public int SessionId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public bool IsAuthenticated => UserId > 0 && SessionId > 0;

        public void Clear()
        {
            UserId = 0;
            SessionId = 0;
            Username = null;
            Role = null;
        }

    }
}
