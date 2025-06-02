using MongoDB.Bson.Serialization.Attributes;

namespace WildboarMonitor.FunctionApp.Models;

public class ImageAttachment
{
    public string Id { get; set; } = string.Empty;

    [BsonIgnore]
    public byte[] Data { get; set; } = [];

    public DateTime Timestamp { get; set; }

    public bool WildBoarDetected { get; set; } = false;
}
