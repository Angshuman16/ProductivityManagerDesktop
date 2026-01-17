using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityManager.Models
{
    public class SignupResult
    {

        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        private SignupResult() { }

        public static SignupResult Success()
        {
            return new SignupResult { IsSuccess = true };
        }

        public static SignupResult Failed(string message)
        {
            return new SignupResult
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }
    }
}
