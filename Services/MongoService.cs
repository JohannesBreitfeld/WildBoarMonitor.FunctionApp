using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WildboarMonitor.FunctionApp.Models;
using WildboarMonitor.FunctionApp.Settings;

namespace WildboarMonitor.FunctionApp.Services;

public class MongoService : IDatabaseService
{
    private readonly IMongoCollection<ImageAttachment> _collection;
    private readonly ILogger<MongoService> _logger;

    public MongoService(IOptions<MongoSettings> settings, ILogger<MongoService> logger)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<ImageAttachment>(settings.Value.CollectionName);
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

