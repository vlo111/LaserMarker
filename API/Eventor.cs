namespace API
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Eventor
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "events")]
        public List<EventorEvent> Events { get; set; }
    }
}
