using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using WildboarMonitor.FunctionApp.Models;
using WildboarMonitor.FunctionApp.Settings;

namespace WildboarMonitor.FunctionApp.Services;

public class MongoService : IDatabaseService
{
    private readonly IMongoCollection<ImageAttachment> _collection;

    public MongoService(MongoSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<ImageAttachment>(settings.CollectionName);
    }

    public async Task InsertResultAsync(ImageAttachment result)
    {
        await _collection.InsertOneAsync(result);
    }

    public async Task<DateTime?> GetLatestTimestampAsync()
    {
        var sort = Builders<ImageAttachment>.Sort.Descending(x => x.Timestamp);
        var latest = await _collection.Find(Builders<ImageAttachment>.Filter.Empty)
                                      .Sort(sort)
                                      .Limit(1)
                                      .FirstOrDefaultAsync();
        return latest?.Timestamp;
    }
}

