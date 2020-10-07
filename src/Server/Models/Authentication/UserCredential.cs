using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models.Authentication
{
    public class UserCredentialViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CallbackUrl { get; set; }
    }
}
