using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordBotCore.Extensions;
using Newtonsoft.Json;

namespace DiscordBotCore;

public abstract class CommandHandlerBase
{
    protected readonly DiscordSocketClient Client;
    protected readonly CommandService Commands;
    protected readonly InteractionService InteractionService;
    protected readonly IServiceProvider ServiceProvider;
    protected ulong[] Guilds;

    protected CommandHandlerBase(
        DiscordSocketClient client,
        CommandService commands,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
    {
        Client = client;
        Commands = commands;
        InteractionService = interactionService;
        ServiceProvider = serviceProvider;
        Guilds = [];
    }
    
    public virtual async Task InstallCommandsAsync<TSlashCommands>(params ulong[] guilds)
    {
        Guilds = guilds;

        await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);

        Client.Ready += ClientReady<TSlashCommands>;
        Client.InteractionCreated += HandleInteractionAsync;
        Client.ButtonExecuted += ButtonHandler;
        Client.SelectMenuExecuted += MenuHandler;
    }

    protected virtual async Task ClientReady<TSlashCommands>()
    {
        SlashCommandProperties[] slashCommands = GetSlashCommands<TSlashCommands>();
        
        try
        {
            foreach (ulong guildId in Guilds)
            {
                await Client.GetGuild(guildId).BulkOverwriteApplicationCommandAsync(slashCommands); //it's ok
            }
        }
        catch(HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the
            // path of the error as well as the error message. You can serialize the Error field in the exception to get
            // a visual of where your error is.
            string json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to
            // print it.
            Console.WriteLine(json);
        }
        
        try
        {
            await InteractionService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    protected virtual async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        SocketInteractionContext context = new(Client, interaction);

        await InteractionService.ExecuteCommandAsync(context, ServiceProvider);
    }
    
    protected virtual async Task ButtonHandler(SocketMessageComponent component)
    {
    }

    protected virtual async Task MenuHandler(SocketMessageComponent component)
    {
    }

    protected virtual SlashCommandProperties[] GetSlashCommands<TSlashCommands>()
    {
        MethodInfo[] methods = typeof(TSlashCommands).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        SlashCommandProperties[] commands = new SlashCommandProperties[methods.Length];
        
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            SlashCommandBuilder guildCommand = new();
            
            IEnumerable<Attribute> commandAttributes = method.GetCustomAttributes();
            
            foreach (Attribute attribute in commandAttributes)
            {
                switch (attribute)
                {
                    case SlashCommandAttribute slashCommandAttribute:
                        guildCommand.WithName(slashCommandAttribute.Name);
                        guildCommand.WithDescription(slashCommandAttribute.Description);
                        break;
                }
            }

            System.Reflection.ParameterInfo[] parameters = method.GetParameters();

            foreach (System.Reflection.ParameterInfo parameter in parameters)
            {
                bool required = parameter.CustomAttributes.Any(ca => ca.AttributeType == typeof(RequiredAttribute));

                guildCommand.AddOption(parameter.Name?.ToLowerInvariant(), parameter.ParameterType.GetDiscordType(),
                    "Сами разбирайтесь, я генерю это автоматически", required);
            }

            SlashCommandProperties command = guildCommand.Build();
            commands[i] = command;
        }

        return commands;
    }
}