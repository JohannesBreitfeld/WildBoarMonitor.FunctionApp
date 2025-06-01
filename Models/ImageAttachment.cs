namespace WildboarMonitor.FunctionApp.Models;

public class ImageAttachment
{
    public string Id { get; set; } = string.Empty;

    public byte[] Data { get; set; } = [];

    public DateTime Timestamp { get; set; }

    public Status Status { get; set; } = Status.Pending;
}

public enum Status { Pending, Complete }