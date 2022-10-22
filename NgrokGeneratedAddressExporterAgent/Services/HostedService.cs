using System.Diagnostics;
using System.Net.Http.Json;
using EnsureThat;
using NgrokGeneratedAddressExporterAgent.Api;
using NgrokGeneratedAddressExporterAgent.Models;
using Polly;

namespace NgrokGeneratedAddressExporterAgent.Services;

internal sealed class HostedService : BackgroundService
{
    private readonly Settings _settings;
    private readonly ILogger<HostedService> _logger;

    public HostedService(Settings settings, ILogger<HostedService> logger)
    {
        _settings = settings;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ngrokProcessStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = _settings.NgrokWorkingDirectory,
                    FileName = _settings.NgrokPath,
                    Arguments = _settings.NgrokArguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = ngrokProcessStartInfo };

                _logger.LogDebug($"Starting ngrok {ngrokProcessStartInfo.FileName} {ngrokProcessStartInfo.Arguments}");
                process.Start();

                var uri = await GetAddress(stoppingToken).ConfigureAwait(false);
                Ensure.That(uri).IsNotNull();

                await PutAddress(uri!, stoppingToken).ConfigureAwait(false);

                _logger.LogDebug($"Waiting for exit ngrok process");
                await process.WaitForExitAsync(stoppingToken).ConfigureAwait(false);

                Ensure.That(process.ExitCode).Is(0);

            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception.ToString());
            }
        }
    }


    private async Task<Uri?> GetAddress(CancellationToken stoppingToken)
    {
        _logger.LogDebug($"Retrieving Url of started ngrok process");
        using var httpClient = new HttpClient();
        
        return await Policy
            .HandleResult<Uri?>(r => r == null).Or<HttpRequestException>()
            .WaitAndRetryAsync(10, t => TimeSpan.FromSeconds(1))
            .ExecuteAsync(async () =>
            {
                var url = "http://localhost:4040/api/tunnels";
                
                _logger.LogDebug($"Requesting {url}");
                var response = await httpClient.GetAsync("http://localhost:4040/api/tunnels", stoppingToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var tunnelData = await response.Content.ReadFromJsonAsync<NgrokTunnelsResponse>(cancellationToken: stoppingToken).ConfigureAwait(false);
                Uri.TryCreate(tunnelData?.Tunnels.FirstOrDefault()?.PublicUrl, UriKind.Absolute, out var uri);
                
                _logger.LogDebug($"Received {url}");
                return uri;
            });
    }
    
    private async Task PutAddress(Uri uri, CancellationToken stoppingToken)
    {
        var curlProcessStartInfo = new ProcessStartInfo
        {
            WorkingDirectory = _settings.CurlWorkingDirectory,
            FileName = _settings.CurlPath,
            Arguments = _settings.CurlArguments.Replace("{{Address}}", uri.ToString()),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        _logger.LogDebug($"Starting curl {curlProcessStartInfo.FileName} {curlProcessStartInfo.Arguments}");
        using var process = new Process { StartInfo = curlProcessStartInfo };
        process.Start();
        
        _logger.LogDebug($"Waiting for exit curl process");
        await process.WaitForExitAsync(stoppingToken).ConfigureAwait(false);
        Ensure.That(process.ExitCode).Is(0);
        
        _logger.LogDebug($"Url put succeeded");
    }
}