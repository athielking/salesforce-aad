using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private static HttpClient http = new HttpClient { BaseAddress = new Uri("https://ability-enterprise-9701-dev-ed.cs68.my.salesforce.com/services/") };

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType);
        }

        [Authorize]
        public ActionResult Contacts()
        {
            ViewBag.Message = "Your contact page.";

            var jwt = GenerateJwtToken(User as ClaimsPrincipal);
            var formUrlContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                {"assertion", jwt }
            });

            var tokenResp = http.PostAsync("oauth2/token", formUrlContent).GetAwaiter().GetResult();

            var contentStr = tokenResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var result = JsonConvert.DeserializeObject<JObject>(contentStr);

            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(result["access_token"]));
            var contactResp = http.GetAsync("apexrest/contacts").GetAwaiter().GetResult();

            var contacts = JsonConvert.DeserializeObject<List<Contact>>(contactResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var vm = new ContactsViewModel
            {
                Contacts = contacts
            };

            return View(vm);
        }

        private string GenerateJwtToken(ClaimsPrincipal user)
        {
            var userName = user.Claims.FirstOrDefault(c => c.Type == "preferred_username").Value;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName)
            };

            var cert = new X509Certificate2("C:/Users/athie/salesforce.pfx", String.Empty);
            var creds = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: "3MVG9U_dUptXGpYIIqvkU601Gn7R_E7hsRYvc8_YiCCuMdiIlNrO54zG7DzD9vovS6bcSmgst1PxE09qpaMSG",
                //issuer: "3MVG9l2zHsylwlpTGlLcDC3b.xHPxfs.Q6QyHhdWKE8_xgBvPze4ETY7VKGCOj30od6oXv1nRDQBzO3KiXmb8",
                audience: "https://test.salesforce.com",
                claims,
                expires: DateTime.UtcNow.AddMinutes(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}