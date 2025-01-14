namespace BeeSafeWeb.Messages;

public enum MessageType
{
    /// <summary>
    /// This message is sent from the device to the server, to notify the server
    /// that the device is still working as intended.
    /// </summary>
    Ping,
    /// <summary>
    /// This message is a response to the ping message.
    /// </summary>
    Pong,
    /// <summary>
    /// This message is sent when a hornet detection occurs.
    /// </summary>
    DetectionEvent,
}