namespace API
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class CompetitorList
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "competitors")]
        public List<CompetitorData> CompetitorDatas { get; set; }
    }
}
