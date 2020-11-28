﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpaApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private IConfiguration _config;
        private static HttpClient http = new HttpClient { BaseAddress = new Uri("https://ability-enterprise-9701-dev-ed.cs68.my.salesforce.com/services/") };
        public ContactController(IConfiguration config)
        {
            _config = config;
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    //get the JWT Sent to the api
        //    var authZhdr = Request.Headers.FirstOrDefault(h => h.Key.Equals("Authorization"));
        //    var token = authZhdr.Value.FirstOrDefault().Substring(7);

        //    IConfidentialClientApplication client = ConfidentialClientApplicationBuilder
        //        .Create(_config.GetValue<string>("AzureAd:ClientId"))
        //        .WithClientSecret(_config.GetValue<string>("AzureAd:ClientSecret"))
        //        .WithAuthority(AadAuthorityAudience.AzureAdMyOrg)
        //        .WithTenantId(_config.GetValue<string>("AzureAd:TenantId"))
        //        .WithLogging((LogLevel level, string message, bool containsPii) => 
        //        {
        //            Console.WriteLine($"[{level}] {message}");
        //        })
        //        .Build();

        //    var userAssertion = new UserAssertion(token);
        //    var resp = await client.AcquireTokenOnBehalfOf(
        //        new string[] { "https://login.salesforce.com/user_impersonation" }, userAssertion).ExecuteAsync();

        //    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resp.AccessToken);
        //    var sfResp = await http.GetAsync("sobjects/Account");

        //    return Ok(resp.AccessToken);
        //}

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var jwt = GenerateJwtToken(User);

            var formUrlContent = new FormUrlEncodedContent(new Dictionary<string, string> 
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                {"assertion", jwt }
            });
            
            var tokenResp = await http.PostAsync("oauth2/token", formUrlContent);

            var contentStr = await tokenResp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<JObject>(contentStr);

            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(result["access_token"]));
            var contactResp = await http.GetAsync("apexrest/contacts");

            return Ok(await contactResp.Content.ReadAsStringAsync());
        }

        private string GenerateJwtToken(ClaimsPrincipal user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Identity.Name),
            };

            var cert = new X509Certificate2("C:/Users/athie/salesforce.pfx", String.Empty);
            var creds = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256);
            var expires = DateTime.UtcNow.AddMinutes(3);

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