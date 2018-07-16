using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusApiSettings
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }

        public OctopusApiSettings(string url, string apiKey)
        {
            Url = url;
            ApiKey = apiKey;
        }

        public bool IsEmpty => string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(ApiKey);
    }
}
