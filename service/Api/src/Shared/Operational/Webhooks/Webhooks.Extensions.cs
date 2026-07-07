using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Shared.Operational.Webhooks.Backgrounds;
using Shared.Operational.Webhooks.Services;

namespace Shared.Operational.Webhooks;

public static class WebhooksExtensions
{
    public static WebApplicationBuilder AddWebhooks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("WebhookHttp", c =>
        {
            c.Timeout = TimeSpan.FromSeconds(10);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("ReSys.Shop-Webhooks/1.0");
        });

        builder.Services.AddSingleton<IWebhookSigner, WebhookSigner>();
        builder.Services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
        builder.Services.AddScoped<WebhookDeliveryJob>();
        builder.Services.AddHostedService<WebhookDeliveryBackgroundService>();

        return builder;
    }
}

public sealed class WebhookDeliveryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WebhookDeliveryBackgroundService> _logger;

    internal static TimeSpan DefaultSweepInterval = TimeSpan.FromMinutes(1);

    public WebhookDeliveryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WebhookDeliveryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook delivery background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(DefaultSweepInterval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<WebhookDeliveryJob>();
                await job.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during webhook delivery sweep");
            }
        }
    }
}
