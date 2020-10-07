using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AuthorizationContext = System.Web.Mvc.AuthorizationContext;

namespace NetFrameworkMvcClient.Common
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public const string RequiredClaimType = "identity.netframeworkmvcclient.permission";
        public const string RequiredScope = "identity.netframeworkmvcclient";

        private readonly Permission _permission;

        public CustomAuthorizeAttribute(string key, string value, PermissionType type)
        {
            _permission = new Permission(key, value, type);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.User.Identity as System.Security.Claims.ClaimsIdentity;

            try
            {
                Claim claim = user.Claims.Where(c => c.Type == RequiredClaimType).FirstOrDefault();
                if (claim == null)
                    throw new Exception($"Claim {RequiredClaimType} not found");

                Permission[] permissions = ParsePermission(claim.Value);

                return HasPermission(permissions, this._permission);
            }
            catch
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var viewResult = new ViewResult();
            viewResult.ViewName = "Error";
            viewResult.TempData.Add("Message", "No Permission");

            filterContext.Result = viewResult;
        }

        private Permission[] ParsePermission(string claimValue)
        {
            return JsonConvert.DeserializeObject<Permission[]>(claimValue);
        }

        private bool HasPermission(Permission[] granteds, Permission required)
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