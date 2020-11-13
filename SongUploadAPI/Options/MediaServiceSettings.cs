using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Options
{
    public class MediaServiceSettings
    {
        public string AadClientId { get; set; }
        public string AadSecret { get; set; }
        public Uri AadTenantDomain { get; set; }
        public string AadTenantId { get; set; }
        public string AccountName { get; set; }
        public string Location { get; set; }
        public string ResourceGroup { get; set; }
        public string SubscriptionId { get; set; }
        public Uri ArmAadAudience { get; set; }
        public Uri ArmEndpoint { get; set; }
        public string TransformName { get; set; }
    }
}
