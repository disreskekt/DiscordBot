using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Mp3Player.Helpers;
using Mp3Player.Services.Interfaces;

namespace Mp3Player.Commands;

//todo трай кетчи мб везде добавить, в лив и скип уже есть
public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    // private readonly DataContext _db;
    private readonly IVoiceChannelManager _voiceChannelManager;
    private readonly IPlayingService _playingService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ISearchService _searchService;
    private readonly IResponseService _responseService;

    public SlashCommands(
        IVoiceChannelManager voiceChannelManager,
        IPlayingService playingService,
        IFileSystemService fileSystemService,
        ISearchService searchService,
        IResponseService responseService)
    {
        // _db = db;
        _voiceChannelManager = voiceChannelManager;
        _playingService = playingService;
        _fileSystemService = fileSystemService;
        _searchService = searchService;
        _responseService = responseService;
    }
    
    [SlashCommand("help", "Объясняет для тупых")]
    public async Task Help()
    {
        try
        {
            await Context.Interaction.RespondAsync("Впадлу помогать");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [SlashCommand("add", "Добавляет музыку в формате mp3")]
    public async Task Add([Required] IAttachment attachment, [Remainder] string? songname = null) //DO NOT USE CAMEL CASE
    {
        try
        {
            if (!attachment.Filename.EndsWith(".mp3"))
            {
                await Context.Interaction.RespondAsync("Ты че даун? mp3 кидай");
                return;
            }
            
            songname ??= attachment.Filename.Substring(0, attachment.Filename.LastIndexOf('.'));
            
            if (_fileSystemService.SongNameExists(songname))
            {
                await Context.Interaction.RespondAsync("Трек с таким именем уже существует");
                return;
            }
            
            using (HttpClient httpClient = new HttpClient())
            {
                await using Stream mp3Stream = await httpClient.GetStreamAsync(attachment.Url);
                await _fileSystemService.AddFile(mp3Stream, songname);
            }
            
            await Context.Interaction.RespondAsync("Сохранил");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [SlashCommand("play", "Добавляет песню в очередь")]
    public async Task Play([Remainder] [Required] string songname) //DO NOT USE CAMEL CASE
    {
        try
        {
            IVoiceChannel? voiceChannel = _voiceChannelManager.GetVoiceChannel(Context.User);
            
            if (voiceChannel is null)
            {
                await Context.Interaction.RespondAsync("Ты не в войсе");
                return;
            }
            
            string[] possibleSongs = _searchService.Search(songname);
            
            string songListMessage = _responseService.BuildSongListMessage(possibleSongs);
            
            SelectMenuBuilder selectMenuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Выберите песню")
                .WithCustomId("menu-1");
            
            foreach (string possibleSong in possibleSongs)
            {
                selectMenuBuilder.AddOption(possibleSong, possibleSong);
            }
            
            MessageComponent component = new ComponentBuilder()
                .WithSelectMenu(selectMenuBuilder)
                .Build();
            
            await Context.Interaction.RespondAsync(songListMessage, components: component);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [SlashCommand("skip", "Скипает песню")]
    public async Task Skip()
    {
        try
        {
            IVoiceChannel? voiceChannel = _voiceChannelManager.GetVoiceChannel(Context.User);
            
            if (voiceChannel is null)
            {
                await Context.Interaction.RespondAsync("Ты не в войсе");
                return;
            }
            
            _playingService.Skip(voiceChannel.Id);
            
            await Context.Interaction.RespondAsync("Скипнул");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [SlashCommand("leave", "Выходит из войса")]
    public async Task Leave()
    {
        try
        {
            IVoiceChannel? voiceChannel = _voiceChannelManager.GetVoiceChannel(Context.User);
            
            if (voiceChannel is null)
            {
                await Context.Interaction.RespondAsync("Ты не в войсе");
                return;
            }
            
            await _voiceChannelManager.Leave(Context.Guild.Id, voiceChannel);
            
            _playingService.Stop(voiceChannel.Id);
            
            await Context.Interaction.RespondAsync("Бб лохи");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [SlashCommand("queue", "Показывает текущую очередь")]
    public async Task Queue()
    {
        try
        {
            IVoiceChannel? voiceChannel = _voiceChannelManager.GetVoiceChannel(Context.User);
            
            if (voiceChannel is null)
            {
                await Context.Interaction.RespondAsync("Ты не в войсе");
                return;
            }
            
            string[] songs = _playingService.GetQueue(voiceChannel.Id);
            string songListMessage = _responseService.BuildSongListMessage(songs);
            
            await Context.Interaction.RespondAsync("Ща покажу");
            
            await Context.Channel.SendMessageAsync(songListMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [SlashCommand("songlist", "Выдает список доступных песен")]
    public async Task SongList(int page = 1)
    {
        try
        {
            if (page <= 0)
            {
                await Context.Interaction.RespondAsync("Дебил?");
                return;
            }
            
            int pageAmount = _fileSystemService.GetPageAmount();
            if (page > pageAmount)
            {
                await Context.Interaction.RespondAsync($"Всего {pageAmount} страниц");
                return;
            }
            
            await Context.Interaction.RespondAsync("Ща будет");
            
            string[] songNames = _fileSystemService.GetPage(page, pageAmount);
            
            string songListMessage = _responseService.BuildSongListMessageWithPages(songNames, page, pageAmount);
            
            MessageComponent messageComponent = CommandHelper.BuildButtons(page, pageAmount);
            
            await Context.Channel.SendMessageAsync(songListMessage, components: messageComponent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}