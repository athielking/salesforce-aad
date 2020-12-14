using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Odata.Models
{
    public class AadOptions
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string Authority => $"{Instance}{TenantId}";
    }
}
