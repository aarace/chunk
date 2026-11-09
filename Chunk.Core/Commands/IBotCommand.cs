namespace Chunk.Core.Commands;

public interface IBotCommand
{
    string Name { get; }
    string Usage { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string args, CancellationToken cancellationToken);
}
