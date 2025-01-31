using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AcquisitionShared.Protocol
{
    public class Command
    {
        [JsonPropertyName("type")]
        public CommandTypes Type { get; set; }

        [JsonPropertyName("parameters")]
        public Dictionary<string, string>? Parameters { get; set; }

        public Command(CommandTypes type, Dictionary<string, string>? parameters = null)
        {
            Type = type;
            Parameters = parameters;
        }
    }
}