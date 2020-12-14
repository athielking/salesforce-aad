using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Odata.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Odata.Data
{
    public class AadSqlTokenProvider
    {
        private IMemoryCache _cache;
        private readonly IOptions<AadOptions> _aadOptions;
        public static readonly string[] _scopes = new[]
{
            "https://database.windows.net//.default"
        };

        public AadSqlTokenProvider(IOptions<AadOptions> aadOptions, IMemoryCache cache)
        {
            _cache = cache;
            _aadOptions = aadOptions;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrCreateAsync(nameof(AadSqlTokenProvider), async entry =>
            {
                var client = ConfidentialClientApplicationBuilder
                    .Create(_aadOptions.Value.ClientId)
                    .WithClientSecret(_aadOptions.Value.ClientSecret)
                    .WithAuthority(_aadOptions.Value.Authority)
                    .WithTenantId(_aadOptions.Value.TenantId)
                    .Build();

                
                var result = await client.AcquireTokenForClient(_scopes).ExecuteAsync();
                entry.SetAbsoluteExpiration(result.ExpiresOn.AddMinutes(-5));

                return result.AccessToken;
            });
        }

        public string GetAccessToken(CancellationToken cancellationToken = default)
        {
            return _cache.GetOrCreate(nameof(AadSqlTokenProvider), entry =>
            {
                var tokenRequestContext = new TokenRequestContext(_scopes);

                
                var token = new DefaultAzureCredential().GetToken(tokenRequestContext, cancellationToken);

                entry.SetAbsoluteExpiration(token.ExpiresOn.AddMinutes(-5));

                return token.Token;
            });
        }
    }
}
