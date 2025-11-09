using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Pusula.Student.Automation.Blazor;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var serilogConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var loggerConfiguration = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Pusula.Student.Automation")
            .Enrich.WithProperty("Environment", environmentName)
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console());

        var elasticUri = serilogConfiguration["ElasticSearch:Uri"];
        if (!string.IsNullOrWhiteSpace(elasticUri))
        {
            var elasticOptions = new ElasticsearchSinkOptions(new Uri(elasticUri))
            {
                AutoRegisterTemplate = serilogConfiguration.GetValue<bool?>("ElasticSearch:AutoRegisterTemplate") ?? true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                IndexFormat = serilogConfiguration["ElasticSearch:IndexFormat"] ?? "pusula-app-log-{0:yyyy.MM}",
                NumberOfShards = serilogConfiguration.GetValue<int?>("ElasticSearch:NumberOfShards"),
                NumberOfReplicas = serilogConfiguration.GetValue<int?>("ElasticSearch:NumberOfReplicas"),
                FailureCallback = e => Console.WriteLine($"Failed to submit event to Elasticsearch: {e.Exception?.Message}"),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                   EmitEventFailureHandling.WriteToFailureSink |
                                   EmitEventFailureHandling.RaiseCallback
            };

            loggerConfiguration.WriteTo.Elasticsearch(elasticOptions);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        try
        {
            Log.Information("Starting web host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            await builder.AddApplicationAsync<AutomationBlazorModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
