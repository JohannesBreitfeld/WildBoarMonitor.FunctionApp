using DnsClient.Internal;
using MongoDB.Driver;
using WildboarMonitor.FunctionApp.Models;
using WildboarMonitor.FunctionApp.Settings;

namespace WildboarMonitor.FunctionApp.Services;

public class MongoService : IDatabaseService
{
    private readonly IMongoCollection<ImageAttachment> _collection;
    private readonly ILogger _logger;

    public MongoService(MongoSettings settings, ILogger logger)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<ImageAttachment>(settings.CollectionName);
        _logger = logger;
    }

    public async Task InsertResultAsync(ImageAttachment result)
    {
        await _collection.InsertOneAsync(result);
    }

    public async Task<DateTime?> GetLatestTimestampAsync()
    {
        try
        {
            var sort = Builders<ImageAttachment>.Sort.Descending(x => x.Timestamp);
            var latest = await _collection.Find(Builders<ImageAttachment>.Filter.Empty)
                                          .Sort(sort)
                                          .Limit(1)
                                          .FirstOrDefaultAsync();
            return latest?.Timestamp;
        }
        catch(Exception ex)
        {
            _logger.LogError($"Error fetching timestamp from DB. Message: {ex.Message}");
            return null;
        }
    }
}

