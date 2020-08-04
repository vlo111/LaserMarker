namespace API
{
    using Newtonsoft.Json;
    public class Competitor
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "competitor")]
        public CompetitorData CompetitorData { get; set; }
    }
}