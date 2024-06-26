using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotCore;

namespace SpotifyCaster;

public class CommandHandler : CommandHandlerBase
{
    public CommandHandler(
        DiscordSocketClient client,
        CommandService commands,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
        : base(client, commands, interactionService, serviceProvider)
    {
    }
}