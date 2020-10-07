using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SsoIdentityServer4.Authentication
{
    public class ProfileService : IProfileService
    {
        protected readonly CustomUserManager CustomUserManager;

        public ProfileService(CustomUserManager customUserManager)
        {
            CustomUserManager = customUserManager;
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null) throw new Exception("No sub claim present");

            await GetProfileDataAsync(context, sub);

            // request claims
            ICollection<IdentityResource> requestResources = context.RequestedResources.Resources.IdentityResources;
            IEnumerable<string> requestClaims = context.RequestedClaimTypes;

            // Add custom claims
            //var user = await _userManager.GetUserAsync(context.Subject);
            string permissionInJson = JsonConvert.SerializeObject(new Permission[] { new Permission("privacy", string.Empty) }, Formatting.None);

            var claims = new List<Claim>
            {
                new Claim("identity.mvcclient.permission", permissionInJson),
                new Claim("identity.netframeworkmvcclient.permission", permissionInJson)
            };
            context.IssuedClaims.AddRange(claims);
        }

        /// <summary>
        /// Called to get the claims for the subject based on the profile request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected virtual async Task GetProfileDataAsync(ProfileDataRequestContext context, string subjectId)
        {
            var user = await FindUserAsync(subjectId);
            if (user != null)
            {
                await GetProfileDataAsync(context, user);
            }
        }

        /// <summary>
        /// Called to get the claims for the user based on the profile request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual async Task GetProfileDataAsync(ProfileDataRequestContext context, User user)
        {
            var principal = await GetUserClaimsAsync(user);
            context.AddRequestedClaims(principal.Claims);
        }

        /// <summary>
        /// Gets the claims for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual async Task<ClaimsPrincipal> GetUserClaimsAsync(User user)
        {
            ClaimsPrincipal principal = await CreateAsync(user);
            if (principal == null) throw new Exception("ClaimsFactory failed to create a principal");

            return principal;
        }

        public Task<ClaimsPrincipal> CreateAsync(User user)
        {
            return Task.Run<ClaimsPrincipal>(() =>
            {
                ClaimsIdentity identity = new ClaimsIdentity(user.GetSampleClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                return principal;
            });
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null) throw new Exception("No subject Id claim present");

            await IsActiveAsync(context, sub);
        }

        /// <summary>
        /// Determines if the subject is active.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected virtual async Task IsActiveAsync(IsActiveContext context, string subjectId)
        {
            User user = await FindUserAsync(subjectId);
            if (user != null)
            {
                await IsActiveAsync(context, user);
            }
            else
            {
                context.IsActive = false;
            }
        }

        /// <summary>
        /// Determines if the user is active.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual async Task IsActiveAsync(IsActiveContext context, User user)
        {
            context.IsActive = await IsUserActiveAsync(user);
        }

        /// <summary>
        /// Returns if the user is active.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> IsUserActiveAsync(User user)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Loads the user by the subject id.
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected async Task<User> FindUserAsync(string subjectId)
        {
            return await CustomUserManager.FindByNameAsync(subjectId);
        }

    }

    public class Permission
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Permission(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
