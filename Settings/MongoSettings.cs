namespace WildboarMonitor.FunctionApp.Settings;

public class MongoSettings
{
    public required string ConnectionString { get; set; } = string.Empty;
    public required string DatabaseName { get; set; } = string.Empty;
    public required string CollectionName { get; set; } = string.Empty;
}
