using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaApi
{
    public class AzureADSettings
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
