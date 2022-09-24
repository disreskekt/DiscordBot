using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Helpers;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models;

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
    public string Content { get; set; }
    public ISocketMessageChannel Channel { get; }
    public SocketUser Author { get; }
    public IReadOnlyCollection<Attachment> Attachments { get; }
    public ModuleDsMessage(string content, ISocketMessageChannel channel, SocketUser author, IReadOnlyCollection<Attachment> attachments)
    {
        this.Content = content;
        this.Channel = channel;
        this.Author = author;
        this.Attachments = attachments;
    }

}

public class Module : ModuleBase<SocketCommandContext>
{
    private readonly IDbContextAccessor _dbContextAccessor;
    private const string SOUNDS_PATH = @"C:\Users\disre\Desktop\ds_bot\Sounds\";

    public Module(IDbContextAccessor dbContextAccessor)
    {
        _dbContextAccessor = dbContextAccessor;
    }
    
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
    
    [Command("харош", RunMode = RunMode.Async)]
    [Summary("Харош)")]
    public async Task Harosh()
    {
        await PlaySound();
    }
    
    [Command("мегахарош", RunMode = RunMode.Async)]
    [Summary("Мегахарош)")]
    public async Task Megaharosh()
    {
        await PlaySound();
    }

    [Command("челхарош", RunMode = RunMode.Async)]
    [Summary("Чел харош)")]
    public async Task ChelHarosh()
    {
        await PlaySound();
    }
    
    [Command("ахуителен", RunMode = RunMode.Async)]
    [Summary("Ахуителен)")]
    public async Task Ahuitelen()
    {
        await PlaySound();
    }
    
    [Command("плох", RunMode = RunMode.Async)]
    [Summary("Плох(")]
    public async Task Ploh()
    {
        await PlaySound();
    }

    private async Task PlaySound()
    {
        ModuleDsContext context = new ModuleDsContext(this.Context);
        string message = await Commands.Harosh(context);
        // await this.Context.Channel.SendMessageAsync(message);
    }
}