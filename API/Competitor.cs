﻿using System;
using System.Collections.Generic;

namespace API
{
    using Newtonsoft.Json;

    public class Competitor
    {
        [JsonProperty(PropertyName = "ok")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "competitor")]
        public Dictionary<string, string> CompetitorData { get; set; }
    }
}