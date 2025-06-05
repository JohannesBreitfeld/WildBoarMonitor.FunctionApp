using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using WildboarMonitor.FunctionApp.Services;
using WildboarMonitor.FunctionApp.Settings;
using WildboarMonitor_FunctionApp;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddPredictionEnginePool<MLModel.ModelInput, MLModel.ModelOutput>()
    .FromFile("MLModel.mlnet");

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));


builder.Services.AddScoped<IDatabaseService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoService(settings);
});

builder.Services.Configure<GmailSettings>(
    builder.Configuration.GetSection("GmailSettings"));

builder.Services.AddScoped<IImageExtractionService>( sp=>
{
    var settings = sp.GetRequiredService<IOptions<GmailSettings>>().Value;
    return new ImageExtractionService(settings);
});

builder.Build().Run();
