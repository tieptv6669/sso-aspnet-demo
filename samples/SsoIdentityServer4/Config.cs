using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace SsoIdentityServer4
{
    public static class Config
    {
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("admin", "provide admin permissions", new List<string>{ }),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
              {
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:44388/signin-oidc" }, // where to redirect to after login
                    PostLogoutRedirectUris = { "https://localhost:44388/signout-callback-oidc" }, // where to redirect to after logout
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "offline_access",
                        "identity.mvcclient"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true
                },

                new Client
                {
                    ClientId = "netframeworkmvcclient",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:44396/" },
                    //RedirectUris = { "https://localhost:44344/" },
                    PostLogoutRedirectUris = { "https://localhost:44396/" },
                    //PostLogoutRedirectUris = { "https://localhost:44344/" },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "offline_access",
                        "identity.netframeworkmvcclient"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true
                }
              };
        }

        public static List<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "identity.mvcclient",
                    Description = "User's permissions in MVC-Client app",
                    UserClaims = new[] { "identity.mvcclient.permission" }
                },
                new IdentityResource
                {
                    Name = "identity.netframeworkmvcclient",
                    Description = "User's permissions in .NET Framework MVC-Client app",
                    UserClaims = new[] { "identity.netframeworkmvcclient.permission" }
                }
            };
        }
    }
}
