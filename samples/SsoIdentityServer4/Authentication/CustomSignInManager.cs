using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SsoIdentityServer4.Authentication
{
    public class CustomSignInManager
    {
        public Task<bool> CheckPasswordSignInAsync(string username, string password)
        {
            return Task.Run<bool>(() =>
            {
                return true;
            });
        }

        /// <summary>
        /// Attempts to sign in the specified <paramref name="user"/> and <paramref name="password"/> combination
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
        /// for the sign-in attempt.</returns>
        public async Task<bool> PasswordSignInAsync(User user, string password, bool isPersistent)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await PasswordSignInAsync(user.LoginName, password, isPersistent);
        }

        /// <summary>
        /// Attempts to sign in the specified <paramref name="userName"/> and <paramref name="password"/> combination
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="userName">The user name to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
        /// for the sign-in attempt.</returns>
        public async Task<bool> PasswordSignInAsync(string userName, string password, bool isPersistent)
        {
            return await CheckPasswordSignInAsync(userName, password);
        }

    }
}
