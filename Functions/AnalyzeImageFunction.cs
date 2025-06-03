using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using System.Text.Json;
using WildboarMonitor.FunctionApp.Models;
using WildboarMonitor.FunctionApp.Services;
using static WildboarMonitor_FunctionApp.MLModel;

namespace WildboarMonitor.FunctionApp.Functions;

public class AnalyzeImageFunction
{
    private readonly ILogger<AnalyzeImageFunction> _logger;
    private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;
    private readonly IDatabaseService _db;

    public AnalyzeImageFunction(
        ILogger<AnalyzeImageFunction> logger, 
        PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, 
        IDatabaseService db)
    {
        _logger = logger;
        _predictionEnginePool = predictionEnginePool;
        _db = db;
    }

    [Function("AnalyzeImageFunction")]
    public async Task Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var attachments = JsonSerializer.Deserialize<List<ImageAttachment>>(requestBody);

        if (attachments is null)
        {
            return;
        }

        foreach (var attachment in attachments)
        {
            var input = new ModelInput
            {
                ImageSource = attachment.Data
            };

            var prediction = _predictionEnginePool.Predict(input);
            
            var wildBoarProbability = prediction.Score[0];
            
            attachment.WildBoarDetected =
              prediction.PredictedLabel == "wildboar" &&
              wildBoarProbability >= 0.7f;

            await _db.InsertResultAsync(attachment);
        }
    }
}