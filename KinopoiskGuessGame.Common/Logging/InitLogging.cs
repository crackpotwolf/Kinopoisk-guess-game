using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace KinopoiskGuessGame.Common.Logging;

/// <summary>
/// Инициализация логирования
/// </summary>
public static class InitLogging
{
    /// <summary>
    /// Конфиг логирования
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    public static void ConfigureLogging(string indexName)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                optional: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, indexName))
            .Enrich.WithProperty("Environment", environment)
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sentry(o =>
            {
                // Debug and higher are stored as breadcrumbs (default is Information)
                o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                // Warning and higher is sent as event (default is Error)
                o.MinimumEventLevel = LogEventLevel.Warning;
            })
            .CreateLogger();
    }

    /// <summary>
    /// Конфиг Elasticsearch
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="indexName"></param>
    /// <returns></returns>
    private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string indexName)
    {
        return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
        {
            AutoRegisterTemplate = true,
            IndexFormat = indexName
        };
    }
}