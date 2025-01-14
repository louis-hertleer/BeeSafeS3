using System.Text.Json.Serialization;

namespace BeeSafeWeb.Messages;

public class RequestMessage
{
    public string Device { get; set; }
    [JsonPropertyName("message_type")]
    public MessageType MessageType { get; set; }
    public dynamic? Data { get; set; }
}