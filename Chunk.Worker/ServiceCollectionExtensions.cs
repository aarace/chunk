using Chunk.Worker.Slack;
using SlackNet.Events;
using SlackNet.Extensions.DependencyInjection;

namespace Chunk.Worker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChunkWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var botToken = RequiredSetting(configuration, "SLACK_BOT_TOKEN");
        var appToken = RequiredSetting(configuration, "SLACK_APP_TOKEN");

        services.AddSlackNet(c => c
            .UseApiToken(botToken)
            .UseAppLevelToken(appToken)
            .RegisterEventHandler<MessageEvent, ChatEventHandler>()
            .RegisterEventHandler<AppMention, ChatEventHandler>());

        services.AddHostedService<SocketModeService>();

        return services;
    }

    private static string RequiredSetting(IConfiguration configuration, string key) =>
        configuration[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Missing required setting {key}.");
}
