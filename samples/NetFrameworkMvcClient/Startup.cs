using Microsoft.Owin;
using Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using IdentityModel;
using System;
using NetFrameworkMvcClient.Common;

[assembly: OwinStartupAttribute(typeof(NetFrameworkMvcClient.Startup))]
namespace NetFrameworkMvcClient
{
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "cookie"
            });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    SignInAsAuthenticationType = "cookie",

                    Authority = "https://localhost:44313", // issuer
                    ClientId = "netframeworkmvcclient",
                    ClientSecret = "secret",

                    RedirectUri = "https://localhost:44344/",

                    ResponseType = "code",
                    ResponseMode = "query",

                    RedeemCode = true,
                    SaveTokens = true,

                    RequireHttpsMetadata = false,

                    Scope = $"{OpenIdConnectScope.OpenIdProfile} {CustomAuthorizeAttribute.RequiredScope}",

                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        RedirectToIdentityProvider = n =>
                        {
                            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                            {
                                // generate code verifier and code challenge
                                var codeVerifier = CryptoRandom.CreateUniqueId(32);

                                string codeChallenge;
                                using (var sha256 = SHA256.Create())
                                {
                                    var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                                    codeChallenge = Base64Url.Encode(challengeBytes);
                                }

                                // set code_challenge parameter on authorization request
                                n.ProtocolMessage.SetParameter("code_challenge", codeChallenge);
                                n.ProtocolMessage.SetParameter("code_challenge_method", "S256");

                                // remember code verifier in cookie (adapted from OWIN nonce cookie)
                                RememberCodeVerifier(n, codeVerifier);
                            }

                            return Task.CompletedTask;
                        },
                        AuthorizationCodeReceived = n =>
                        {
                            // get code verifier from cookie
                            var codeVerifier = RetrieveCodeVerifier(n);

                            // attach code_verifier on token request
                            n.TokenEndpointRequest.SetParameter("code_verifier", codeVerifier);

                            return Task.CompletedTask;
                        }
                    }
                }
            );
        }

        private void RememberCodeVerifier(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> n, string codeVerifier)
        {
            var properties = new AuthenticationProperties();
            properties.Dictionary.Add("cv", codeVerifier);
            n.Options.CookieManager.AppendResponseCookie(
                n.OwinContext,
                GetCodeVerifierKey(n.ProtocolMessage.State),
                Convert.ToBase64String(Encoding.UTF8.GetBytes(n.Options.StateDataFormat.Protect(properties))),
                new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    HttpOnly = true,
                    Secure = n.Request.IsSecure,
                    Expires = DateTime.UtcNow + n.Options.ProtocolValidator.NonceLifetime
                });
        }

        private string RetrieveCodeVerifier(AuthorizationCodeReceivedNotification n)
        {
            string key = GetCodeVerifierKey(n.ProtocolMessage.State);

            string codeVerifierCookie = n.Options.CookieManager.GetRequestCookie(n.OwinContext, key);
            if (codeVerifierCookie != null)
            {
                var cookieOptions = new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    HttpOnly = true,
                    Secure = n.Request.IsSecure
                };

                n.Options.CookieManager.DeleteCookie(n.OwinContext, key, cookieOptions);
            }

            var cookieProperties = n.Options.StateDataFormat.Unprotect(Encoding.UTF8.GetString(Convert.FromBase64String(codeVerifierCookie)));
            cookieProperties.Dictionary.TryGetValue("cv", out var codeVerifier);

            return codeVerifier;
        }

        private string GetCodeVerifierKey(string state)
        {
            using (var hash = SHA256.Create())
            {
                return OpenIdConnectAuthenticationDefaults.CookiePrefix + "cv." + Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(state)));
            }
        }

    }

}
