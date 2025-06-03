using WildboarMonitor.FunctionApp.Models;

namespace WildboarMonitor.FunctionApp.Services
{
    public interface IDatabaseService
    {
        Task InsertResultAsync(ImageAttachment result);
        Task<DateTime?> GetLatestTimestampAsync();
    }
}