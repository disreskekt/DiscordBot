using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordBot.Helpers;
using DiscordBot.Helpers.Extensions;
using Newtonsoft.Json;
using IResult = Discord.Commands.IResult;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace DiscordBot;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;
    private ulong[] _guilds;

    public CommandHandler(DiscordSocketClient client, CommandService commands, InteractionService interactionService, IServiceProvider serviceProvider)
    {
        _client = client;
        _commands = commands;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
    }

    public async Task InstallCommandsAsync(params ulong[] guilds)
    {
        _guilds = guilds;
        
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        
        Commands.AddDbContextAccessor((_serviceProvider.GetService(typeof(IDbContextAccessor)) as IDbContextAccessor)!);
        
        _client.MessageReceived += HandleCommandAsync;
        _client.Ready += ClientReady;
        _client.InteractionCreated += HandleInteractionAsync;
        _client.ButtonExecuted += ButtonHandler;
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message)
        {
            return;
        }
        
        int argPos = 0;
        
        if (!(message.HasCharPrefix('-', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
        {
            return;
        }
        
        SocketCommandContext context = new SocketCommandContext(_client, message);
        
        IResult result = await _commands.ExecuteAsync(context, argPos, _serviceProvider);
    }
    
    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        SocketInteractionContext context = new SocketInteractionContext(_client, interaction);
        
        Discord.Interactions.IResult result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
    }

    private async Task ClientReady()
    {
        MethodInfo[] methods = typeof(SplashCommandsModule).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        SlashCommandProperties[] commands = new SlashCommandProperties[methods.Length];
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            SlashCommandBuilder guildCommand = new SlashCommandBuilder();

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
            
            ParameterInfo[] parameters = method.GetParameters();

            foreach (ParameterInfo parameter in parameters)
            {
                guildCommand.AddOption(parameter.Name?.ToLowerInvariant(), parameter.ParameterType.GetDiscordType(),
                    "Описание лол, я это автоматически добавляю, какие описания)", false);
            }

            SlashCommandProperties command = guildCommand.Build();
            commands[i] = command;
        }

        try
        {
            foreach (ulong guildId in _guilds)
            {
                await _client.GetGuild(guildId).BulkOverwriteApplicationCommandAsync(commands); //todo
            }
        }
        catch(HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            string json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            
            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }

        try
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task ButtonHandler(SocketMessageComponent component)
    {
        int indexOfSlash = component.Message.Content.IndexOf('/');
        string pageNumberString = component.Message.Content.Substring(0, indexOfSlash);
        int pageNumber = Convert.ToInt32(pageNumberString);

        string newSongPage = "";
        switch(component.Data.CustomId)
        {
            case "left_arrow_page":
                newSongPage = await Commands.SongListNextOrPreviousPage(pageNumber - 1);
                break;
            case "right_arrow_page":
                newSongPage = await Commands.SongListNextOrPreviousPage(pageNumber + 1);
                break;
        }
        
        MessageComponent messageComponent = CommandHelper.BuildButtons(newSongPage);

        await component.Message.DeleteAsync();
        await component.Channel.SendMessageAsync(newSongPage, components: messageComponent);
        
        // await component.Message.ModifyAsync(message =>
        // {
        //     message.Content = newSongPage;
        //     message.Components = messageComponent;
        // });
    }
}

public interface IDsContext
{
    public IDsMessage Message { get; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public SocketGuild Guild { get; }
}

public interface IDsMessage
{
    public string MessageText { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public IReadOnlyCollection<IAttachment> Attachments { get; }
}