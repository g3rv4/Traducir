#nullable enable
using System.Text.Json.Serialization;

namespace Traducir.Core.TransifexV3
{
    public static class ChromeI18N
    {
        public class Message
        {
            [JsonPropertyName("message")]
            public string? Value { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }
    }
}