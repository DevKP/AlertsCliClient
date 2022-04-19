using System;
using System.Text.Json.Serialization;

namespace AlertUkrZen.Models
{
    public class Alert
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("location_title")]
        public string LocationTitle { get; set; }

        [JsonPropertyName("started_at")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("finished_at")]
        public object FinishedAt { get; set; }

        [JsonPropertyName("ended_at")]
        public DateTime EndedAt { get; set; }

        [JsonPropertyName("duration")]
        public string DurationStr { get; set; }

        [JsonIgnore]
        public TimeSpan Duration => TimeSpan.Parse(DurationStr);
    }
}