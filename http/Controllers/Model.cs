using System;
using System.Text.Json.Serialization;

namespace http.Controllers
{
    public class TimeJson
    {
        [JsonPropertyName("hostId")]
        public string HostId { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("start")]
        public long Start { get; set; }
        
        [JsonPropertyName("dur")]
        public int Dur { get; set; }

        [JsonIgnore]
        public bool Valid => Guid.TryParse(HostId, out var _) && Dur > 0;
    }

    public class HostJson
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tz")]
        public string TimeZone { get; set; }
    }
}