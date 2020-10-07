using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SsoIdentityServer4.Authentication
{
    public class CustomUserManager
    {
        public Task<User> FindByNameAsync(string username)
        {
            return Task.Run(() =>
            {
                return new User
                {
                    Id = 123456,
                    LoginName = username,
                    FullName = $"Nguyễn {username}",
                    Email = $"{username}@example.com",
                    IdDomain = 99999
                };
            });
        }

        public Task<string> GetUserIdAsync(string username)
        {
            return Task.Run<string>(() =>
            {
                return username;
            });
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int IdDomain { get; set; }
        public bool IsAdmin { get; set; }

        public string Role
        {
            get
            {
                return IsAdmin ? "Admin" : "User";
            }
        }

        public IEnumerable<Claim> GetSampleClaims()
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, LoginName),
                new Claim(ClaimTypes.Role, Role)
            };

            return claims;
        }
    }
}
