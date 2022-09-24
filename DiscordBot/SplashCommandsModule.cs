using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using RunMode = Discord.Commands.RunMode;

namespace DiscordBot;

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
    public string Token { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public IReadOnlyCollection<Attachment> Attachments { get; }
    public DsMessage(string token, ISocketMessageChannel channel, SocketUser user, IReadOnlyCollection<Attachment> attachments)
    {
        this.Token = token;
        this.Channel = channel;
        this.User = user;
        this.Attachments = attachments;
    }
}

public class SplashCommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("help", "Объясняет для тупых")]
    public async Task Help()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string helpMessage = await Commands.Help(dsContext);
        await this.Context.Interaction.RespondAsync(helpMessage);
    }
    
    [SlashCommand("add", "Добавляет контент в базу")]
    public async Task Add()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string? addMessage = await Commands.Add(dsContext);
        if (addMessage is null)
        {
            return;
        }
        await this.Context.Interaction.RespondAsync(addMessage);
    }
    
    [SlashCommand("any", "Достает из базы рандомный контент")]
    public async Task Any()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string anyMessage = await Commands.Any(dsContext);
        await this.Context.Interaction.RespondAsync(anyMessage);
    }
    
    [SlashCommand("gif", "Достает из базы рандомную гифку")]
    public async Task Gif()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string gifMessage = await Commands.Gif(dsContext);
        await this.Context.Interaction.RespondAsync(gifMessage);
    }
    
    [SlashCommand("image", "Достает из базы рандомную картинку")]
    public async Task Image()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string imageMessage = await Commands.Image(dsContext);
        await this.Context.Interaction.RespondAsync(imageMessage);
    }
    
    [SlashCommand("харош", "Харош)", false, Discord.Interactions.RunMode.Async)]
    public async Task Harosh()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string message = await Commands.Harosh(dsContext);
        await this.Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("мегахарош", "Мегахарош)", false, Discord.Interactions.RunMode.Async)]
    public async Task Megaharosh()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string message = await Commands.Megaharosh(dsContext);
        await this.Context.Interaction.RespondAsync(message);
    }

    [SlashCommand("челхарош", "Чел харош)", false, Discord.Interactions.RunMode.Async)]
    public async Task ChelHarosh()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string message = await Commands.ChelHarosh(dsContext);
        await this.Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("ахуителен", "Ахуителен)", false, Discord.Interactions.RunMode.Async)]
    public async Task Ahuitelen()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string message = await Commands.Ahuitelen(dsContext);
        await this.Context.Interaction.RespondAsync(message);
    }
    
    [SlashCommand("плох", "Плох(", false, Discord.Interactions.RunMode.Async)]
    public async Task Ploh()
    {
        DsMessage dsMessage = new DsMessage(this.Context.Interaction.Token, this.Context.Channel, this.Context.User, Array.Empty<Attachment>());
        DsContext dsContext = new DsContext(dsMessage, dsMessage.Channel, dsMessage.User, this.Context.Guild);
        string message = await Commands.Ploh(dsContext);
        await this.Context.Interaction.RespondAsync(message);
    }
}