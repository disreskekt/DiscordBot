using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using SpotifyCaster.BackgroundServices;
using SpotifyCaster.Configs;
using SpotifyCaster.Services;
using SpotifyCaster.Services.VoiceChannelManager;
using DiscordConfig = SpotifyCaster.Configs.DiscordConfig;

namespace SpotifyCaster;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigs(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<DiscordConfig>(configuration.GetSection("Discord"));
        services.Configure<SpotifyConfig>(configuration.GetSection("Spotify"));

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<DiscordBotInitService>();
        // services.AddHostedService<SpotifyInitService>();

        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        DiscordSocketConfig discordSocketConfig = new()
        {
            GatewayIntents = GatewayIntents.All
        };

        services.AddSingleton<DiscordSocketConfig>(_ => discordSocketConfig);
        services.AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(discordSocketConfig));
        services.AddSingleton<InteractionService>(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<CommandService>();
        services.AddSingleton<CommandHandler>();

        services.AddSingleton<SpotifyService>();

        services.AddSingleton<IVoiceChannelManager, VoiceChannelManager>();

        return services;
    }
}