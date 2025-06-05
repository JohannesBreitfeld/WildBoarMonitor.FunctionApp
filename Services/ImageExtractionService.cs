using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using WildboarMonitor.FunctionApp.Models;
using WildboarMonitor.FunctionApp.Settings;

namespace WildboarMonitor.FunctionApp.Services;

public class ImageExtractionService : IImageExtractionService
{
    private readonly GmailSettings _settings;

    public ImageExtractionService(GmailSettings settings)
    {
        _settings = settings;
    }

    public async Task<GmailService> StartService()
    {
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync
            (
                new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret
                },
                new[] { GmailService.Scope.GmailReadonly },
                "user",
                CancellationToken.None
            );

        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });

        return service;
    }

    public async Task<HashSet<ImageAttachment>> ExtractAttachments(DateTime lastRetrievedMessageDate)
    {
        using var service = await StartService();

        HashSet<ImageAttachment> attachments = [];

        var dateFilter = lastRetrievedMessageDate.Subtract(TimeSpan.FromDays(1)).ToString("yyyy/MM/dd");
        var allThreads = new List<Google.Apis.Gmail.v1.Data.Thread>();
        string? nextPageToken = null;

        do
        {
            var request = service.Users.Threads.List("me");

            request.Q = $"after:{dateFilter}";
            request.MaxResults = 500;
            request.PageToken = nextPageToken;

            var response = request.Execute();
            allThreads.AddRange(response.Threads);
            nextPageToken = response.NextPageToken;

        } while (nextPageToken != null);

        foreach (var thread in allThreads)
        {
            var threadRequest = service.Users.Threads.Get("me", thread.Id);
            var threadData = threadRequest.Execute();

            foreach (var message in threadData.Messages)
            {
                foreach (var part in message.Payload.Parts)
                {
                    if (string.IsNullOrEmpty(part.Filename))
                    {
                        continue;
                    }
                    DateTime messageDate = UnixTimeToDateTime(message.InternalDate!.Value);

                    var attachId = part.Body.AttachmentId;
                    var attachRequest = service.Users.Messages.Attachments.Get("me", message.Id, attachId);
                    var attachData = attachRequest.Execute();
                    byte[] data = Convert.FromBase64String(attachData.Data.Replace('-', '+').Replace('_', '/'));

                    var fileName = message.Id + part.Filename;

                    var attachment = new ImageAttachment() { Id = fileName, Timestamp = messageDate, Data = data };
                    attachments.Add(attachment);
                }
            }
        }
        return attachments;
    }

    public DateTime UnixTimeToDateTime(long unixTime)
    {
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;
        return dateTime.ToLocalTime();
    }
}
