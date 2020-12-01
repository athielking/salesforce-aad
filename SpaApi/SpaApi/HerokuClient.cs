using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpaApi
{
    public class HerokuClient
    {
        private readonly HerokuSettings _config;
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public HerokuClient(IOptions<HerokuSettings> configuration, IMemoryCache cache, HttpClient http)
        {
            _config = configuration.Value;
            _http = http;
            _cache = cache;
        }

        public async Task<PostgreSqlConnectionStringBuilder> GetConnectionString()
        {
            if(!_cache.TryGetValue("DATABASE_URL", out string databaseUrl)) 
            {
                var resp = await _http.GetAsync("config-vars");
                resp.EnsureSuccessStatusCode();

                var herokuConfig = await JsonSerializer.DeserializeAsync<HerokuConfig>(await resp.Content.ReadAsStreamAsync());
                databaseUrl = herokuConfig.DatabaseUrl;

                //Set for 24hrs
                var entryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.Now.AddDays(1));
                _cache.Set("DATABASE_URL", databaseUrl, entryOptions);
            }
            
            return new PostgreSqlConnectionStringBuilder(databaseUrl)
            {
                Pooling = true,
                TrustServerCertificate = true,
                SslMode = SslMode.Require
            };
        }


    }
}
