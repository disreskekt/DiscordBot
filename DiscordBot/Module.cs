﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Helpers;

namespace DiscordBot;

public class ModuleDsContext : SocketCommandContext, IDsContext
{
    public ModuleDsContext(SocketCommandContext context) : base(context.Client, context.Message)
    {
        Message = new ModuleDsMessage(context.Message.Content, context.Channel, context.User,
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
        MessageText = messageText;
        Channel = channel;
        User = author;
        Attachments = attachments;
    }

}

public class Module : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("Объясняет для тупых")]
    public async Task Help()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string helpMessage = await Commands.Help(context);
        await Context.Channel.SendMessageAsync(helpMessage);
    }
    
    [Command("add")]
    [Summary("Добавляет контент в базу")]
    public async Task Add()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string? addMessage = await Commands.Add(context);
        if (addMessage is null)
        {
            return;
        }
        await Context.Channel.SendMessageAsync(addMessage);
    }
    
    [Command("any")]
    [Summary("Достает из базы рандомный контент")]
    public async Task Any()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string anyMessage = await Commands.Any(context);
        await Context.Channel.SendMessageAsync(anyMessage);
    }
    
    [Command("gif")]
    [Summary("Достает из базы рандомную гифку")]
    public async Task Gif()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string gifMessage = await Commands.Gif(context);
        await Context.Channel.SendMessageAsync(gifMessage);
    }
    
    [Command("image")]
    [Summary("Достает из базы рандомную картинку")]
    public async Task Image()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string imageMessage = await Commands.Image(context);
        await Context.Channel.SendMessageAsync(imageMessage);
    }
    
    [Command("song", RunMode = RunMode.Async)]
    [Summary("Включает рандомную песню из базы")]
    public async Task Song(int? songId = null)
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string audioMessage = await Commands.Song(context, songId);
        await Context.Channel.SendMessageAsync(audioMessage);
    }
    
    [Command("skip")]
    [Summary("Скипает песню")]
    public async Task Skip()
    {
        string skipMessage = Commands.Skip();
        await Context.Channel.SendMessageAsync(skipMessage);
    }
    
    [Command("leave", RunMode = RunMode.Async)]
    [Summary("Выходит из войса")]
    public async Task Leave()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.Leave(context);
        await Context.Channel.SendMessageAsync(message);
    }
    
    [Command("songlist")]
    [Summary("Выдает список доступных песен")]
    public async Task SongList(int page = 1)
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.SongList(context, page);
        
        MessageComponent messageComponent = CommandHelper.BuildButtons(message);
        
        await Context.Channel.SendMessageAsync(message, components: messageComponent);
    }
    
    [Command("харош", RunMode = RunMode.Async)]
    [Summary("Харош)")]
    public async Task Harosh()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.Harosh(context);
        // await Context.Channel.SendMessageAsync(message);
    }
    
    [Command("мегахарош", RunMode = RunMode.Async)]
    [Summary("Мегахарош)")]
    public async Task Megaharosh()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.Megaharosh(context);
        // await Context.Channel.SendMessageAsync(message);
    }

    [Command("челхарош", RunMode = RunMode.Async)]
    [Summary("Чел харош)")]
    public async Task ChelHarosh()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.ChelHarosh(context);
        // await Context.Channel.SendMessageAsync(message);
    }
    
    [Command("ахуителен", RunMode = RunMode.Async)]
    [Summary("Ахуителен)")]
    public async Task Ahuitelen()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.Ahuitelen(context);
        // await Context.Channel.SendMessageAsync(message);
    }
    
    [Command("плох", RunMode = RunMode.Async)]
    [Summary("Плох(")]
    public async Task Ploh()
    {
        ModuleDsContext context = new ModuleDsContext(Context);
        string message = await Commands.Ploh(context);
        // await Context.Channel.SendMessageAsync(message);
    }
}