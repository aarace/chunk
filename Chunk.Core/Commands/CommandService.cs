using System.Text;
using Microsoft.Extensions.Logging;

namespace Chunk.Core.Commands;

public class CommandService : ICommandService
{
    public const char Prefix = '!';
    private const string HelpCommandName = "help";

    private readonly IReadOnlyDictionary<string, IBotCommand> _commands;
    private readonly ILogger<CommandService> _logger;

    public CommandService(IEnumerable<IBotCommand> commands, ILogger<CommandService> logger)
    {
        _commands = commands.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task<string?> HandleAsync(string text, CancellationToken cancellationToken)
    {
        var trimmed = text.Trim();
        if (trimmed.Length < 2 || trimmed[0] != Prefix || char.IsWhiteSpace(trimmed[1]))
        {
            return null;
        }

        var parts = trimmed[1..].Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0];
        var args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        if (name.Equals(HelpCommandName, StringComparison.OrdinalIgnoreCase))
        {
            return BuildHelp();
        }

        if (!_commands.TryGetValue(name, out var command))
        {
            return $"I don't know `{Prefix}{name}`. Try `{Prefix}{HelpCommandName}`.";
        }

        try
        {
            return await command.ExecuteAsync(args, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Command {Command} failed with args {Args}", command.Name, args);
            return $"Something went wrong running `{Prefix}{command.Name}`. Try again in a bit.";
        }
    }

    private string BuildHelp()
    {
        var sb = new StringBuilder("*Here's what I can do:*\n");
        foreach (var command in _commands.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
        {
            sb.Append($"• `{Prefix}{command.Usage}`: {command.Description}\n");
        }
        sb.Append($"• `{Prefix}{HelpCommandName}`: show this list");
        return sb.ToString();
    }
}
