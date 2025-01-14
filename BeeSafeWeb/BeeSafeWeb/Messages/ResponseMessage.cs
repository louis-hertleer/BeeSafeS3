using System.Text.Json.Serialization;

namespace BeeSafeWeb.Messages;

public class ResponseMessage
{
    [JsonPropertyName("message_type")]
    public MessageType MessageType { get; set; }
    public dynamic? Data { get; set; }
}