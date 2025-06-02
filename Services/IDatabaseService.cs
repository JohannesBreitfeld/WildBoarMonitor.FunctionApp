using WildboarMonitor.FunctionApp.Models;

namespace WildboarMonitor.FunctionApp.Services
{
    public interface IDatabaseService
    {
        Task<List<ImageAttachment>> GetAllAsync();
        Task InsertResultAsync(ImageAttachment result);
    }
}