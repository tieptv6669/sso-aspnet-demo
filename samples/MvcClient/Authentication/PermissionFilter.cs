using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static MvcClient.Authentication.PermissionRequirementFilter;

namespace MvcClient.Authentication
{
    public class PermissionRequirementAttribute : TypeFilterAttribute
    {
        public PermissionRequirementAttribute(string key, string value, PermissionType type) : base(typeof(PermissionRequirementFilter))
        {
            Arguments = new object[] { new Permission(key, value, type) };
        }
    }

    public class PermissionRequirementFilter : IAuthorizationFilter
    {
        public const string RequiredClaimType = "identity.mvcclient.permission";
        public const string RequiredScope = "identity.mvcclient";

        private readonly Permission _permission;

        public PermissionRequirementFilter(Permission permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Claim claim = context.HttpContext.User.Claims.Where(c => c.Type == RequiredClaimType).FirstOrDefault();

            bool isAllowed = false;
            try
            {
                if (claim == null)
                    throw new Exception($"Claim {RequiredClaimType} not found");

                Permission[] permissions = ParsePermission(claim.Value);

                isAllowed = VerifyPermission(permissions, this._permission);
            }
            catch (Exception e)
            {
                // log e
            }

            if (!isAllowed)
            {
                context.Result = new ForbidResult();
            }
        }

        private Permission[] ParsePermission(string claimValue)
        {
            return JsonConvert.DeserializeObject<Permission[]>(claimValue);
        }

        private bool VerifyPermission(Permission[] granteds, Permission required)
        {
            switch (required.Type)
            {
                case PermissionType.Single:
                    return granteds.Any(p => p.Key == required.Key);

                case PermissionType.RestrictWithObjects:
                    return granteds.Any(p => p.Key == required.Key);

                default:
                    throw new Exception($"Unknown value type: {required.Type}");
            }
        }
    }

    public class Permission
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public PermissionType Type { get; set; }

        public Permission(string key, string value, PermissionType type)
        {
            this.Key = key;
            this.Value = value;
            this.Type = type;
        }
    }

    public enum PermissionType
    {
        Single = 1,
        RestrictWithObjects = 2,
    }

}
