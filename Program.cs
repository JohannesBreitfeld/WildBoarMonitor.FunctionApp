using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WildboarMonitor.FunctionApp.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

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
