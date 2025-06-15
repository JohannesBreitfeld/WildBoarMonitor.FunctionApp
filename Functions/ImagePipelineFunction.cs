using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using WildboarMonitor.FunctionApp.Services;
using static WildboarMonitor_FunctionApp.MLModel;

namespace WildboarMonitor.FunctionApp.Functions;

public class ImagePipelineFunction
{
    private readonly ILogger _logger;
    private readonly IImageExtractionService _imageExtractionService;
    private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;
    private readonly IDatabaseService _db;

    public ImagePipelineFunction(
        ILoggerFactory loggerFactory,
        IImageExtractionService imageExtractionService,
        IDatabaseService db,
        PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool)
    {
        _logger = loggerFactory.CreateLogger<ImagePipelineFunction>();
        _imageExtractionService = imageExtractionService;
        _db = db;
        _predictionEnginePool = predictionEnginePool;
    }

    [Function("ImagePipelineFunction")]
    public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer)
    {
        try
        {
            var swedishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var locaStartlTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, swedishTimeZone);
            _logger.LogInformation($"Function started at {locaStartlTime:yyyy-MM-dd HH:mm:ss}");
            
            var latestTimeStamp = await _db.GetLatestTimestampAsync();
            var imageAttachments = await _imageExtractionService.ExtractAttachments(latestTimeStamp ?? DateTime.UtcNow.AddDays(-2000));

            if (imageAttachments is null || imageAttachments.Count == 0)
            {
                _logger.LogWarning($"No image attachments were found after timestamp: {latestTimeStamp}");
                return;
            }

            foreach (var attachment in imageAttachments)
            {
                var input = new ModelInput
                {
                    ImageSource = attachment.Data
                };

                var prediction = _predictionEnginePool.Predict(input);

                attachment.WildBoarDetected = prediction.PredictedLabel == "wildboar";

                await _db.InsertResultAsync(attachment);
            }


            var localEndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, swedishTimeZone);
            _logger.LogInformation($"Pipeline finished at {localEndTime:yyyy-MM-dd HH:mm:ss}, processed {imageAttachments.Count} images.");
        }
        catch(Exception ex)
        {
            _logger.LogError($"Error occured, message: {ex.Message}");
        }
    }
}