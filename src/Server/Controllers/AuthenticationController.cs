using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Authentication;
using Server.Models.Authentication;

namespace Server.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly AuthenticationHandler _authHandler;

        public AuthenticationController(AuthenticationHandler authHanlder)
        {
            this._authHandler = authHanlder;
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login([FromForm] UserCredentialViewModel loginModel)
        {
            try
            {
                User user = await _authHandler.SignIn(this.HttpContext, loginModel.Username, loginModel.Password);
                if (user == null) throw new Exception("Invalid login.");

                // create security-token
                string token = await _authHandler.GetSecurityToken(user);

                // redirect user to callback url
                string callbackUrl = await _authHandler.BuildUrl(this.HttpContext, loginModel.CallbackUrl, token);

                return Redirect(callbackUrl);
            }
            catch (Exception e)
            {
                //ModelState.AddModelError("summary", e.Message);
                return RedirectToPage("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _authHandler.SignOut(this.HttpContext);
            }
            catch (Exception e)
            {
                // log
            }

            return RedirectToAction("Index", "Home", null);
        }
    }
}
