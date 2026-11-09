namespace Chunk.Core.Commands;

public interface ICommandService
{
    /// <returns>The reply to post, or null when the text is not a command.</returns>
    Task<string?> HandleAsync(string text, CancellationToken cancellationToken);
}
