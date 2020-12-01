using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaApi
{
    public class HerokuConfig
    {
        [JsonPropertyName("DATABASE_URL")]
        public string DatabaseUrl { get; set; }
    }
}
