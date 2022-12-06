using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordBot.Helpers;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;

namespace DiscordBot;

public class Commands
{
    private const string SAVED = "Сохранил";
    private const string POSTAVIL = "Поставил";
    private const string SKIPNUL = "Скипнул";
    private static IDbContextAccessor? _dbContextAccessor;

    public Commands()
    {
    }
    
    public static void AddDbContextAccessor(IDbContextAccessor dbContextAccessor)
    {
        _dbContextAccessor = dbContextAccessor;
    }
    
    public static async Task<string> Help(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        const string helpMessage = @"Ахахаха пашол нахуй пидар";

        return helpMessage;
    }

    public static async Task<string?> Add(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string text = context.Message.MessageText;
        text = text.TrimStart(' ')
            .TrimStart('-')
            .TrimEnd(' ');
        switch (text)
        {
            // case not null when text.StartsWith("add https://tenor.com/view/"):
            //     int indexOfWhitespace = text.IndexOf(' ');
            //     string tenorLink = text.Substring(indexOfWhitespace + 1);
            //     await AddTenorGif(tenorLink, user, db);
            //     await db.SaveChangesAsync();
            //     break;
            case "add" when context.Message.Attachments.Count > 0:
                await AddNewContentTypes(context.Message.Attachments, db);
                await AddAttachments(context.Message.Attachments, db);
                await db.SaveChangesAsync();
                break;
            case "-add":
                return "Ну может ты что-нибудь прикрепишь?";
            default:
                return "Неизвестная команда лол";
        }
        
        return SAVED;
    }

    public static async Task<string> Any(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string anyContentSource = await GetAnyContentSource(db);

        return anyContentSource;
    }

    public static async Task<string> Gif(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string gifContentSource = await GetContentSource(db, null, "image/gif");

        return gifContentSource;
    }

    public static async Task<string> Image(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string imageContentSource = await GetContentSource(db, null, "image/png", "image/jpeg");

        return imageContentSource;
    }

    public static async Task<string> Song(IDsContext context, int? songId = null)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string audioContentSource = await GetContentSource(db, songId, "audio/mpeg");
        
        return await SetSong(context, audioContentSource);
    }

    public static string Skip()
    {
        PlayingService.Skip();

        return SKIPNUL;
    }
    
    public static async Task<string> Leave(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        VoiceChannelStatus voiceChannelStatus = GuildsHelper.GetOrSet(context.Guild.Id);

        ulong? actualChannel = voiceChannelStatus.FindActualChannel();
        
        if (actualChannel is null)
        {
            return "Да вроде не в войсе";
        }

        voiceChannelStatus.LeaveChannel(actualChannel.Value);

        SocketGuildChannel channelToLeave = context.Guild.GetChannel(actualChannel.Value);

        IVoiceChannel voiceChannelToLeave = (channelToLeave as IVoiceChannel)!;
        
        await voiceChannelToLeave.DisconnectAsync();
        
        PlayingService.Queue.Clear();
        PlayingService.PlayingStatus = false;
        
        return "Бб лохи";
    }
    
    public static async Task<string> SongList(IDsContext context, int page = 1)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        if (page <= 0)
        {
            return "Дебил?";
        }
        
        int pageAmount = await GetPageAmount(db);
        if (page > pageAmount)
        {
            return $"Всего {pageAmount} страниц";
        }
        
        return GetSongPage(page, db, pageAmount);
    }

    public static async Task<string> SongListNextOrPreviousPage(int page)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        if (page <= 0)
        {
            throw new Exception("Блять как ты это сделал?");
        }
        
        int pageAmount = await GetPageAmount(db);
        if (page > pageAmount)
        {
            throw new Exception("Блять как ты это сделал?");
        } 
        
        return GetSongPage(page, db, pageAmount);
    }

    private static string GetSongPage(int page, DataContext db, int pageAmount)
    {
        var sourceList = db.Contents.Where(c => c.ContentTypeId == 4)
            .OrderBy(c => c.Id)
            .Skip(page * 10 - 10)
            .Take(10)
            .Select(c => new
            {
                c.Id,
                c.ContentSource
            })
            .ToList();

        StringBuilder sb = new StringBuilder()
            .Append(page)
            .Append('/')
            .Append(pageAmount)
            .Append(':')
            .AppendLine();

        for (int i = 0; i < sourceList.Count; i++)
        {
            string source = sourceList[i].ContentSource;
            // int lastIndexOfSlash = source.LastIndexOf('/');
            int lastIndexOfDot = source.LastIndexOf('.');

            sb.Append(sourceList[i].Id)
                .Append('.')
                .Append(' ')
                .Append(source.AsSpan(0, lastIndexOfDot - 1));

            if (i < sourceList.Count - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static async Task<int> GetPageAmount(DataContext db)
    {
        int count = await db.Contents.Where(c => c.ContentTypeId == 4).CountAsync();

        int countDividedByTen = count / 10;
        int countPercentOften = count % 10;

        int pageAmount;
        if (countPercentOften == 0)
        {
            pageAmount = countDividedByTen;
        }
        else
        {
            pageAmount = countDividedByTen + 1;
        }

        return pageAmount;
    }
    
    public static async Task<string> Harosh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string audioContentSource = await GetContentSource(db, 22, "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Megaharosh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string audioContentSource = await GetContentSource(db, 23, "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> ChelHarosh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string audioContentSource = await GetContentSource(db, 21, "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Ahuitelen(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string audioContentSource = await GetContentSource(db, 20, "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Ploh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        
        string audioContentSource = await GetContentSource(db, 24, "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    private static async Task AddNewContentTypes(IReadOnlyCollection<IAttachment> attachments, DataContext db)
    {
        foreach (IAttachment attachment in attachments)
        {
            ContentType? dbContentType =
                await db.ContentTypes.FirstOrDefaultAsync(ct => ct.Name == attachment.ContentType);

            if (dbContentType is null)
            {
                db.ContentTypes.Add(new ContentType
                {
                    Name = attachment.ContentType
                });
            }
        }
    }
    
    private static async Task AddAttachments(IReadOnlyCollection<IAttachment> attachments, DataContext db)
    {
        foreach (IAttachment attachment in attachments)
        {
            await SetContent(db, attachment.Url, attachment.ContentType);
        }
    }
    
    private static async Task SetContent(DataContext db, string contentContent, string contentTypeName)
    {
        ContentType? contentType =
            await db.ContentTypes.FirstOrDefaultAsync(ct => ct.Name == contentTypeName);

        if (contentType is null)
        {
            throw new Exception("Content type has not been added yet");
        }

        db.Contents.Add(new Content
        {
            ContentType = contentType,
            ContentSource = contentContent,
            UploadingDate = DateTime.Now
        });
    }
    
    private static async Task<string> GetAnyContentSource(DataContext db)
    {
        int сontentCount = await db.Contents.CountAsync();

        if (сontentCount < 1)
        {
            throw new Exception("Сука нет контента");
        }
            
        Random random = new Random();
        int next = random.Next(1, сontentCount + 1);
            
        return db.Contents.Select(c => c.ContentSource).Skip(next - 1).First();
    }
        
    private static async Task<string> GetContentSource(DataContext db, int? contentId = null, params string[] contentNames)
    {
        List<int> contentTypeIds = db.ContentTypes.Where(ct => contentNames.Contains(ct.Name)).Select(ct => ct.Id).ToList();
        
        if (contentTypeIds is null || !contentTypeIds.Any())
        {
            throw new Exception("Нет таких контент тайпов лох");
        }
        
        IQueryable<Content> typedContents = db.Contents.Where(c => contentTypeIds.Contains(c.ContentTypeId));

        if (contentId is not null)
        {
            typedContents = typedContents.Where(c => c.Id == contentId);
        }
        
        int typedContentCount = await typedContents.CountAsync();
        
        if (typedContentCount < 1)
        {
            throw new Exception("Сука нет контента");
        }
        
        Random random = new Random();
        int next = random.Next(1, typedContentCount + 1);
        
        return typedContents.Select(c => c.ContentSource).Skip(next - 1).First();
    }
    
    private static async Task<string> PlaySound(string songName, IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();

        // return await SetSong(db, context, audiContentSource);
        return "";
    }

    private static async Task<string> SetSong(IDsContext context, string audiContentSource)
    {
        IGuildUser? guildUser = context.User as IGuildUser;
        IVoiceChannel? voiceChannel = guildUser?.VoiceChannel;

        if (voiceChannel is null)
        {
            return "Ты не в войсе";
        }

        VoiceChannelStatus voiceChannelStatus = GuildsHelper.GetOrSet(context.Guild.Id);

        bool isChanged = voiceChannelStatus.FindAndChangeActualChannel(voiceChannel.Id);

        if (isChanged)
        {
            IAudioClient? audioClient = await voiceChannel.ConnectAsync();
            PlayingService.ChangeAudioClient(audioClient);
            voiceChannelStatus.ChannelsClient.AddOrChangeValue(voiceChannel.Id, audioClient);
        }
        
        PlayingService.Queue.Enqueue(audiContentSource);
        
        Task.Run(() => PlayingService.ForcePlay());
        
        return POSTAVIL;
    }
}