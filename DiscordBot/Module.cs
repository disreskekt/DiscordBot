using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot;

public class ModuleDsContext : SocketCommandContext, IDsContext
{
    public ModuleDsContext(SocketCommandContext context) : base(context.Client, context.Message)
    {
        this.Message = new ModuleDsMessage(context.Message.Content, context.Channel, context.User,
            context.Message.Attachments);
    }

    new public IDsMessage Message { get; }
}

public class ModuleDsMessage : IDsMessage
{
    public string MessageText { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser User { get; }
    public IReadOnlyCollection<IAttachment> Attachments { get; }
    public ModuleDsMessage(string messageText, ISocketMessageChannel channel, SocketUser author, IReadOnlyCollection<IAttachment> attachments)
    {
        this.MessageText = messageText;
        this.Channel = channel;
        this.User = author;
        this.Attachments = attachments;
    }

}

public class Module : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("Объясняет для тупых")]
    public async Task Help()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string helpMessage = await Commands.Help(context);
        await this.Context.Channel.SendMessageAsync(helpMessage);
    }
    
    [Command("add")]
    [Summary("Добавляет контент в базу")]
    public async Task Add()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string? addMessage = await Commands.Add(context);
        if (addMessage is null)
        {
            return;
        }
        await this.Context.Channel.SendMessageAsync(addMessage);
    }
    
    [Command("any")]
    [Summary("Достает из базы рандомный контент")]
    public async Task Any()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string anyMessage = await Commands.Any(context);
        await this.Context.Channel.SendMessageAsync(anyMessage);
    }
    
    [Command("gif")]
    [Summary("Достает из базы рандомную гифку")]
    public async Task Gif()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string gifMessage = await Commands.Gif(context);
        await this.Context.Channel.SendMessageAsync(gifMessage);
    }
    
    [Command("image")]
    [Summary("Достает из базы рандомную картинку")]
    public async Task Image()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string imageMessage = await Commands.Image(context);
        await this.Context.Channel.SendMessageAsync(imageMessage);
    }
    
    [Command("song", RunMode = RunMode.Async)]
    [Summary("Включает рандомную песню из базы")]
    public async Task Song(string? songName = null)
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string audioMessage = await Commands.Song(context, songName);
        await this.Context.Channel.SendMessageAsync(audioMessage);
    }
    
    [Command("leave", RunMode = RunMode.Async)]
    [Summary("Выходит из войса")]
    public async Task Leave()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.Leave(context);
        await this.Context.Channel.SendMessageAsync(message);
    }
    
    [Command("харош", RunMode = RunMode.Async)]
    [Summary("Харош)")]
    public async Task Harosh()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.Harosh(context);
        // await this.Context.Channel.SendMessageAsync(message);
    }
    
    [Command("мегахарош", RunMode = RunMode.Async)]
    [Summary("Мегахарош)")]
    public async Task Megaharosh()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.Megaharosh(context);
        // await this.Context.Channel.SendMessageAsync(message);
    }

    [Command("челхарош", RunMode = RunMode.Async)]
    [Summary("Чел харош)")]
    public async Task ChelHarosh()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.ChelHarosh(context);
        // await this.Context.Channel.SendMessageAsync(message);
    }
    
    [Command("ахуителен", RunMode = RunMode.Async)]
    [Summary("Ахуителен)")]
    public async Task Ahuitelen()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.Ahuitelen(context);
        // await this.Context.Channel.SendMessageAsync(message);
    }
    
    [Command("плох", RunMode = RunMode.Async)]
    [Summary("Плох(")]
    public async Task Ploh()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.Ploh(context);
        // await this.Context.Channel.SendMessageAsync(message);
    }
}