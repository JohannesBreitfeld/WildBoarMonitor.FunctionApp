using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using WildboarMonitor.FunctionApp.Services;
using WildboarMonitor_FunctionApp;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddPredictionEnginePool<MLModel.ModelInput, MLModel.ModelOutput>()
    .FromFile("MLModel.mlnet");

builder.Services.AddSingleton<IDatabaseService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var connectionString = config["MongoDbConnectionString"];
    var databaseName = config["MongoDbDatabaseName"];

    return new MongoService(connectionString!, databaseName!);
});

builder.Services.AddSingleton<IImageExtractionService>( sp=>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var clientId = config["client_id"];
    var clientSecret = config["client_secret"];

    return new ImageExtractionService(clientId!, clientSecret!);
});


builder.Build().Run();
