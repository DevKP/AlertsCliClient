using System;
using System.Text.Json.Serialization;

namespace AlertUkrZen.Models
{
    public class Meta
    {
        [JsonPropertyName("last_updated_at")]
        public DateTime LastUpdatedAt { get; set; }

        [JsonPropertyName("generated_at")]
        public string GeneratedAt { get; set; }
    }
}