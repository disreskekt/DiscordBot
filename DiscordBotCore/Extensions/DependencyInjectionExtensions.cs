using DiscordBotCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotCore.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDiscordBotCoreServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IVoiceChannelService, VoiceChannelService>();

        return serviceCollection;
    }
}