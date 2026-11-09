# Chunk

A Slack bot that answers `!commands` in DMs and channels. It connects over Socket Mode, so it needs no public URL.

## Commands

- `!help`: list commands
- `!weather <zip or city>`: current conditions and a 3-day forecast from [Open-Meteo](https://open-meteo.com) (no API key needed)

You can also mention the bot instead of using `!`: `@Chunk weather 02025`.

## Slack app setup

Create the app from a manifest at api.slack.com/apps with Socket Mode turned on. It needs:

- Bot scopes: `chat:write`, `im:history`, `channels:history`, `groups:history`, `mpim:history`, `app_mentions:read`
- Bot events: `message.im`, `message.channels`, `message.groups`, `message.mpim`, `app_mention`
- App Home: the Messages tab turned on, with "Allow users to send messages" checked
- An app-level token with `connections:write`

To use commands in a channel, `/invite @Chunk` into it.

## Configuration

| Setting | Value |
|---|---|
| `SLACK_BOT_TOKEN` | Bot token (`xoxb-…`) |
| `SLACK_APP_TOKEN` | App-level token (`xapp-…`) |

For local development, store them as user secrets:

```
dotnet user-secrets --project Chunk.Worker set SLACK_BOT_TOKEN xoxb-...
dotnet user-secrets --project Chunk.Worker set SLACK_APP_TOKEN xapp-...
dotnet run --project Chunk.Worker
```

## Docker

```
cp .env.example .env   # fill in tokens
docker build -t chunk .
docker run -d --name chunk --env-file .env --restart unless-stopped chunk
```

Run only one instance per Slack app. Socket Mode spreads events across all open connections, so a second copy would take a share of the messages.

## Adding a command

1. Create a class in `Chunk.Core` that implements `IBotCommand` (`Name`, `Usage`, `Description`, `ExecuteAsync`).
2. Register it in `Chunk.Core/ServiceCollectionExtensions.cs`: `services.AddSingleton<IBotCommand, MyCommand>();`

`!help` picks it up automatically.
