using Google.Apis.Gmail.v1;
using WildboarMonitor.FunctionApp.Models;

namespace WildboarMonitor.FunctionApp.Services
{
    public interface IImageExtractionService
    {
        Task<HashSet<ImageAttachment>> ExtractAttachments(DateTime lastRetrievedMessageDate);
        Task<GmailService> StartService();
    }
}