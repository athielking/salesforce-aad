using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        private SalesforceSettings _config;
        private AzureADSettings _adSettings;
        private static HttpClient http = new HttpClient { BaseAddress = new Uri("https://501software-dev-ed.my.salesforce.com/services/") };
        private readonly CertificateClient _kvClient;
        private readonly SecretClient _secretClient;
        private readonly SalesforceDbContext _dbContext;

        public ContactController(IOptions<SalesforceSettings> config, IOptions<AzureADSettings> adSettings, SalesforceDbContext dbContext)
        {
            _config = config.Value;
            _adSettings = adSettings.Value;

            _dbContext = dbContext;
            _kvClient = new CertificateClient(
                new Uri("https://astsalesforcekeyvault.vault.azure.net/"), 
                new ClientSecretCredential(_adSettings.TenantId, _adSettings.ClientId, _adSettings.ClientSecret));

            _secretClient = new SecretClient(
                new Uri("https://astsalesforcekeyvault.vault.azure.net/"),
                new ClientSecretCredential(_adSettings.TenantId, _adSettings.ClientId, _adSettings.ClientSecret));
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
            if (_config.UseApi)
            {
                var jwt = await GenerateJwtToken(User);

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

            var contacts = _dbContext.Contacts.ToList();
            return Ok(contacts);

        }

        private async Task<string> GenerateJwtToken(ClaimsPrincipal user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Identity.Name),
            };

            //Load private key from key vault
            var secret = await _secretClient.GetSecretAsync("Salesforce");
            var base64EncodedBytes = System.Convert.FromBase64String(secret.Value.Value);

            //var cert = new X509Certificate2("C:/Users/athie/salesforce.pfx", String.Empty);
            var cert = new X509Certificate2(base64EncodedBytes, String.Empty);
            var creds = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256);
            var expires = DateTime.UtcNow.AddMinutes(3);

            var token = new JwtSecurityToken(
                //issuer: "3MVG9U_dUptXGpYIIqvkU601Gn7R_E7hsRYvc8_YiCCuMdiIlNrO54zG7DzD9vovS6bcSmgst1PxE09qpaMSG",
                issuer: "3MVG9l2zHsylwlpTGlLcDC3b.xHPxfs.Q6QyHhdWKE8_xgBvPze4ETY7VKGCOj30od6oXv1nRDQBzO3KiXmb8",
                audience: "https://login.salesforce.com",
                claims,
                expires: DateTime.UtcNow.AddMinutes(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}