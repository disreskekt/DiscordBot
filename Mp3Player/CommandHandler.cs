using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Mp3Player.Commands;
using Mp3Player.Helpers;
using Mp3Player.Helpers.Extensions;
using Mp3Player.Services.Interfaces;
using Newtonsoft.Json;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace Mp3Player;

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
        _guilds = Array.Empty<ulong>();
    }
    
    public async Task InstallCommandsAsync(params ulong[] guilds)
    {
        _guilds = guilds;
        
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        
        _client.Ready += ClientReady;
        _client.InteractionCreated += HandleInteractionAsync;
        _client.ButtonExecuted += ButtonHandler;
        _client.SelectMenuExecuted += MenuHandler;
    }
    
    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        SocketInteractionContext context = new SocketInteractionContext(_client, interaction);
        
        await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
    }

    private async Task ClientReady()
    {
        SlashCommandProperties[] commands = ParseSlashCommands();
        
        try
        {
            foreach (ulong guildId in _guilds)
            {
                await _client.GetGuild(guildId).BulkOverwriteApplicationCommandAsync(commands); //it's ok
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
        using IServiceScope scope = _serviceProvider.CreateScope();
        IFileSystemService fileSystemService = scope.ServiceProvider.GetRequiredService<IFileSystemService>();
        IResponseService responseService = scope.ServiceProvider.GetRequiredService<IResponseService>();
        
        int indexOfSlash = component.Message.Content.IndexOf('/');
        string pageNumberString = component.Message.Content.Substring(0, indexOfSlash);
        int page = Convert.ToInt32(pageNumberString);
        
        int indexOfColon = component.Message.Content.IndexOf(':');
        string pageAmountString = component.Message.Content.Substring(indexOfSlash + 1, indexOfColon - indexOfSlash - 1);
        int pageAmout = Convert.ToInt32(pageAmountString);
        
        int? newPage = component.Data.CustomId switch
        {
            "left_arrow_page" => page - 1,
            "right_arrow_page" => page + 1,
            _ => null
        };
        
        if (newPage is null)
        {
            await component.Channel.SendMessageAsync("Ты как блять это сделал?");
            return;
        }
        
        string? newSongPage = NextPage(fileSystemService, responseService, newPage.Value, pageAmout);
        
        if (newSongPage is null)
        {
            await component.Channel.SendMessageAsync("Ты как блять это сделал?");
            return;
        }
        
        MessageComponent messageComponent = CommandHelper.BuildButtons(newPage.Value, pageAmout);
        
        //todo change from deleting to editing
        await component.Message.DeleteAsync();
        await component.Channel.SendMessageAsync(newSongPage, components: messageComponent);
        
        //todo пишет Ошибка взаимодействия, но в целом работает, хз
        // await component.Message.ModifyAsync(message =>
        // {
        //     message.Content = newSongPage;
        //     message.Components = messageComponent;
        // });
    }
    
    
    private async Task MenuHandler(SocketMessageComponent component)
    {
        Task.Run(async () =>
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IVoiceChannelManager voiceChannelManager = scope.ServiceProvider.GetRequiredService<IVoiceChannelManager>();
            IPlayingService playingService = scope.ServiceProvider.GetRequiredService<IPlayingService>();
            
            IVoiceChannel? voiceChannel = voiceChannelManager.GetVoiceChannel(component.User);
            
            if (voiceChannel is null)
            {
                await component.Channel.SendMessageAsync("Ты не в войсе");
                return;
            }
            
            if (!playingService.IsSessionExist(voiceChannel.Id))
            {
                IAudioClient audioClient = await voiceChannelManager.Enter(component.GuildId!.Value, voiceChannel);
                playingService.CreateSession(voiceChannel.Id, audioClient);
            }
            
            string songName = component.Data.Values.First();
            
            playingService.AddToQueue(voiceChannel.Id, songName);
            
            await component.Channel.SendMessageAsync("Поставил " + songName);
        });
    }
    
    private string? NextPage(IFileSystemService fileSystemService, IResponseService responseService, int nextPage, int pageAmount)
    {
        if (nextPage <= 0)
        {
            return null;
        }
        
        if (nextPage > pageAmount)
        {
            return null;
        }
        
        string[] songNames = fileSystemService.GetPage(nextPage, pageAmount);
        
        return responseService.BuildSongListMessageWithPages(songNames, nextPage, pageAmount);
    }
    
    private static SlashCommandProperties[] ParseSlashCommands()
    {
        MethodInfo[] methods = typeof(SlashCommands).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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
                bool required = parameter.CustomAttributes.Any(ca => ca.AttributeType.Equals(typeof(RequiredAttribute)));
                
                guildCommand.AddOption(parameter.Name?.ToLowerInvariant(), parameter.ParameterType.GetDiscordType(),
                    "Сами разбирайтесь, я генерю это автоматически", required);
            }
            
            SlashCommandProperties command = guildCommand.Build();
            commands[i] = command;
        }
        
        return commands;
    }
}