using System.Diagnostics;
using NgrokGeneratedAddressExporterAgent.Models;
using NgrokGeneratedAddressExporterAgent.Services;
using Serilog;

await Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSerilog()
    .ConfigureHostConfiguration(c => c.AddYamlFile($"appsettings.yml"))
    .ConfigureLogging((context, logging) =>
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .CreateLogger();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(context.Configuration.Get<Settings>());
        services.AddHostedService<HostedService>();
    })
    .Build()
    .RunAsync();