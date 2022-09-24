using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBot;

public class CommandHandler
{
    private const ulong OUR_GUILD = 663898503076118528;
    private const ulong MY_GUILD = 903390442979221565;
    
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, string> _commandToMethod;

    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
    {
        _client = client;
        _commands = commands;
        _serviceProvider = serviceProvider;
        _commandToMethod = new Dictionary<string, string>();
    }

    public async Task InstallCommandsAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        
        _client.MessageReceived += HandleCommandAsync;
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;
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
        
        IResult result = await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: _serviceProvider
        );
    }

    private async Task ClientReady()
    {
        SocketGuild? ourGuild = _client.GetGuild(OUR_GUILD);
        SocketGuild? myGuild = _client.GetGuild(MY_GUILD);
        
        MethodInfo[] methods = typeof(Module).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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
                    case CommandAttribute commandAttribute:
                        guildCommand.WithName(commandAttribute.Text);
                        _commandToMethod.Add(commandAttribute.Text, method.Name);
                        break;
                    case SummaryAttribute summaryAttribute:
                        guildCommand.WithDescription(summaryAttribute.Text);
                        break;
                }
            }

            SlashCommandProperties command = guildCommand.Build();
            commands[i] = command;
        }

        try
        {
            await ourGuild.BulkOverwriteApplicationCommandAsync(commands);
            await myGuild.BulkOverwriteApplicationCommandAsync(commands);
        }
        catch(HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            string json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            
            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }

        Commands.AddDbContextAccessor((_serviceProvider.GetService(typeof(IDbContextAccessor)) as IDbContextAccessor)!);
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        if (command.GuildId is null)
        {
            await command.RespondAsync("Пока только в гильдиях работает");

            return;
        }
        
        DsMessage dsMessage = new DsMessage(command.CommandName, command.Channel, command.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.Author, _client.GetGuild(command.GuildId.Value));
        
        string commandName = command.Data.Name;
        string methodName = _commandToMethod[commandName];
        
        Commands commands = new Commands();
        
        MethodInfo method = typeof(Commands).GetMethod(methodName)!;
        Task invoked = (Task) method.Invoke(commands, new object?[] {dsContext})!;
        
        await invoked.ConfigureAwait(false);
        
        PropertyInfo? resultProperty = invoked.GetType().GetProperty("Result");
        object? result = resultProperty?.GetValue(invoked);
        
        await command.RespondAsync((string) result!);
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
    public string Content { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser Author { get; }
    public IReadOnlyCollection<Attachment> Attachments { get; }
}

public class DsContext : IDsContext
{
    public DsContext(IDsMessage message, ISocketMessageChannel channel, SocketUser user, SocketGuild guild)
    {
        this.Message = message;
        this.Channel = channel;
        this.User = user;
        this.Guild = guild;
    }

    public IDsMessage Message { get; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public SocketGuild Guild { get; }
}

public class DsMessage : IDsMessage
{
    public string Content { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser Author { get; }
    public IReadOnlyCollection<Attachment> Attachments { get; }
    public DsMessage(string content, ISocketMessageChannel channel, SocketUser author, IReadOnlyCollection<Attachment> attachments)
    {
        this.Content = content;
        this.Channel = channel;
        this.Author = author;
        this.Attachments = attachments;
    }
    
}