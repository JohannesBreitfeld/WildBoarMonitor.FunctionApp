using MongoDB.Driver;
using WildboarMonitor.FunctionApp.Models;

namespace WildboarMonitor.FunctionApp.Services;

public class MongoService : IDatabaseService
{
    private readonly IMongoCollection<ImageAttachment> _collection;

    public MongoService(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        _collection = database.GetCollection<ImageAttachment>("Images");
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
