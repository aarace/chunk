using System.Text.RegularExpressions;
using Chunk.Core.Commands;
using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace Chunk.Worker.Slack;

public partial class ChatEventHandler : IEventHandler<MessageEvent>, IEventHandler<AppMention>
{
    private readonly ICommandService _commandService;
    private readonly ISlackApiClient _slack;

    public ChatEventHandler(ICommandService commandService, ISlackApiClient slack)
    {
        _commandService = commandService;
        _slack = slack;
    }

    public async Task Handle(MessageEvent message)
    {
        // Subtypes are edits, joins, bot posts, etc.; skipping them (and bot_id) keeps Chunk from answering itself.
        if (message.Subtype is not null || message.BotId is not null || string.IsNullOrWhiteSpace(message.Text))
        {
            return;
        }

        await ReplyAsync(message.Text, message.Channel, message.ThreadTs);
    }

    public async Task Handle(AppMention mention)
    {
        // Only "@Chunk weather 02025" style mentions; "!weather 02025 @Chunk" already arrives as a MessageEvent.
        var match = LeadingMention().Match(mention.Text ?? string.Empty);
        if (!match.Success)
        {
            return;
        }

        var rest = mention.Text![match.Length..].Trim();
        if (rest.Length == 0)
        {
            rest = "help";
        }
        if (rest[0] != CommandService.Prefix)
        {
            rest = CommandService.Prefix + rest;
        }

        await ReplyAsync(rest, mention.Channel, mention.ThreadTs);
    }

    private async Task ReplyAsync(string text, string channel, string? threadTs)
    {
        var reply = await _commandService.HandleAsync(text, CancellationToken.None);
        if (reply is null)
        {
            return;
        }

        await _slack.Chat.PostMessage(new Message
        {
            Channel = channel,
            Text = reply,
            ThreadTs = threadTs
        });
    }

    [GeneratedRegex(@"^\s*<@[A-Z0-9]+>")]
    private static partial Regex LeadingMention();
}
