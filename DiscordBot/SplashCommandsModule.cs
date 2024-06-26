using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Helpers;
using RunMode = Discord.Commands.RunMode;

namespace DiscordBot;

public class DsContext : IDsContext
{
    public DsContext(IDsMessage message, ISocketMessageChannel channel, SocketUser user, SocketGuild guild)
    {
        Message = message;
        Channel = channel;
        User = user;
        Guild = guild;
    }

    public IDsMessage Message { get; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public SocketGuild Guild { get; }
}

public class DsMessage : IDsMessage
{
    public string MessageText { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public IReadOnlyCollection<IAttachment> Attachments { get; }
    public DsMessage(string messageText, ISocketMessageChannel channel, SocketUser user, IReadOnlyCollection<IAttachment> attachments)
    {
        MessageText = messageText;
        Channel = channel;
        User = user;
        Attachments = attachments;
    }
}

public class SplashCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("help", "Объясняет для тупых")]
    public async Task Help()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string helpMessage = await Commands.Help(dsContext);
        await Context.Interaction.RespondAsync(helpMessage);
    }
    
    [SlashCommand("add", "Добавляет контент в базу")]
    public async Task Add([Remainder] IAttachment attachment)
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, new IAttachment[] {attachment});
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string? addMessage = await Commands.Add(dsContext);
        if (addMessage is null)
        {
            return;
        }
        await Context.Interaction.RespondAsync(addMessage);
    }
    
    [SlashCommand("any", "Достает из базы рандомный контент")]
    public async Task Any()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string anyMessage = await Commands.Any(dsContext);
        await Context.Interaction.RespondAsync(anyMessage);
    }
    
    [SlashCommand("gif", "Достает из базы рандомную гифку")]
    public async Task Gif()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string gifMessage = await Commands.Gif(dsContext);
        await Context.Interaction.RespondAsync(gifMessage);
    }
    
    [SlashCommand("image", "Достает из базы рандомную картинку")]
    public async Task Image()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string imageMessage = await Commands.Image(dsContext);
        await Context.Interaction.RespondAsync(imageMessage);
    }
    
    [SlashCommand("song", "Достает из базы рандомную песню")]
    public async Task Song([Remainder] int? songid = null) //DO NOT USE CAMEL CASE
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string audioMessage = await Commands.Song(dsContext, songid);
        await Context.Interaction.RespondAsync(audioMessage);
    }
    
    [SlashCommand("skip", "Скипает песню")]
    public async Task Skip()
    {
        string skipMessage = Commands.Skip();
        await Context.Interaction.RespondAsync(skipMessage);
    }
    
    [SlashCommand("leave", "Выходит из войса")]
    public async Task Leave()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.Leave(dsContext);
        await Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("songlist", "Выдает список доступных песен")]
    public async Task SongList([Remainder] int page = 1)
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.SongList(dsContext, page);
        
        MessageComponent messageComponent = CommandHelper.BuildButtons(message);
        await Context.Interaction.RespondAsync(message, components: messageComponent);
    }
    
    [SlashCommand("харош", "Харош)", false, Discord.Interactions.RunMode.Async)]
    public async Task Harosh()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.Harosh(dsContext);
        await Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("мегахарош", "Мегахарош)", false, Discord.Interactions.RunMode.Async)]
    public async Task Megaharosh()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.Megaharosh(dsContext);
        await Context.Interaction.RespondAsync(message);
    }

    [SlashCommand("челхарош", "Чел харош)", false, Discord.Interactions.RunMode.Async)]
    public async Task ChelHarosh()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.ChelHarosh(dsContext);
        await Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("ахуителен", "Ахуителен)", false, Discord.Interactions.RunMode.Async)]
    public async Task Ahuitelen()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.Ahuitelen(dsContext);
        await Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("плох", "Плох(", false, Discord.Interactions.RunMode.Async)]
    public async Task Ploh()
    {
        SocketSlashCommand socketSlashCommand = (Context.Interaction as SocketSlashCommand)!;
        DsMessage dsMessage = new DsMessage(socketSlashCommand.CommandName, Context.Channel, Context.User, Array.Empty<IAttachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, Context.Guild);
        string message = await Commands.Ploh(dsContext);
        await Context.Interaction.RespondAsync(message);
    }
}