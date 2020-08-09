using System;
using System.Collections.Generic;

namespace API
{
    using Newtonsoft.Json;

    public class Competitors
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "Competitors")]
        public List<Dictionary<string, string>> CompetitorList { get; set; }
    }
}