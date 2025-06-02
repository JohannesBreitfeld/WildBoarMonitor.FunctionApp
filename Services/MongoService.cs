using Microsoft.Extensions.Configuration;
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

    public async Task<List<ImageAttachment>> GetAllAsync()
    {
        return await _collection.Find(Builders<ImageAttachment>.Filter.Empty).ToListAsync();
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

    public async Task<List<ImageAttachment>> GetAllWithWildBoarAsync()
    {
        var filter = Builders<ImageAttachment>.Filter.Eq(x => x.WildBoarDetected, true);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<ImageAttachment>> GetWildBoarsFromLastDaysAsync(int days)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);

        var filter = Builders<ImageAttachment>.Filter.And(
            Builders<ImageAttachment>.Filter.Eq(x => x.WildBoarDetected, true),
            Builders<ImageAttachment>.Filter.Gte(x => x.Timestamp, fromDate)
        );

        return await _collection.Find(filter).ToListAsync();
    }
}
