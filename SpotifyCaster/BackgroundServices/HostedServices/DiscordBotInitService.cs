using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using SpotifyCaster.Commands;
using DiscordConfig = SpotifyCaster.Configs.DiscordConfig;

namespace SpotifyCaster.BackgroundServices;

public class DiscordBotInitService : BackgroundService
{
    private readonly CommandHandler _commandHandler;
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfig _config;

    public DiscordBotInitService(
        CommandHandler commandHandler,
        DiscordSocketClient discordSocketClient,
        IOptions<DiscordConfig> config)
    {
        _commandHandler = commandHandler;
        _client = discordSocketClient;
        _config = config.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _commandHandler.InstallCommandsAsync<SlashCommands>(_config.Guilds).GetAwaiter();

        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, _config.Token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private static Task Log(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}