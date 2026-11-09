using SlackNet;
using SlackNet.Extensions.DependencyInjection;
using SlackNet.SocketMode;

namespace Chunk.Worker.Slack;

public class SocketModeService : BackgroundService
{
    private readonly ISlackSocketModeClient _client;
    private readonly ILogger<SocketModeService> _logger;

    public SocketModeService(IServiceProvider serviceProvider, ILogger<SocketModeService> logger)
    {
        _client = serviceProvider.SlackServices().GetSocketModeClient();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client.Connect(new SocketModeConnectionOptions(), stoppingToken);
        _logger.LogInformation("Connected to Slack via Socket Mode");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.DisconnectAsync();
        await base.StopAsync(cancellationToken);
    }
}
