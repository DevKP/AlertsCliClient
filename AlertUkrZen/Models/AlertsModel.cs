using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AlertUkrZen.Models
{
    public class AlertsModel
    {
        [JsonPropertyName("alerts")]
        public List<Alert> Alerts { get; set; }

        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }

        [JsonPropertyName("disclaimer")]
        public string Disclaimer { get; set; }
    }
}