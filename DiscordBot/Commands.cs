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
        await PreparingToExecuteCommand(context.Message, db);
        
        const string helpMessage = @"Ахахаха пашол нахуй пидар";
        
        await db.SaveChangesAsync();

        return helpMessage;
    }

    public static async Task<string?> Add(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        User? user = await PreparingToExecuteCommand(context.Message, db);

        if (user is null)
        {
            return null;
        }

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
            //     break;
            case "add" when context.Message.Attachments.Count > 0:
                await AddNewContentTypes(context.Message.Attachments, db);
                await AddAttachments(context.Message.Attachments, user, db);
                break;
            case "-add":
                return "Ну может ты что-нибудь прикрепишь?";
            default:
                await db.SaveChangesAsync();
                return "Неизвестная команда лол";
        }
        
        await db.SaveChangesAsync();
        return SAVED;
    }

    public static async Task<string> Any(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        
        string anyContentSource = await GetAnyContentSource(db);
        
        await db.SaveChangesAsync();

        return anyContentSource;
    }

    public static async Task<string> Gif(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        
        string gifContentSource = await GetContentSource(db, null, "image/gif");
        
        await db.SaveChangesAsync();

        return gifContentSource;
    }

    public static async Task<string> Image(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        
        string imageContentSource = await GetContentSource(db, null, "image/png", "image/jpeg");
        
        await db.SaveChangesAsync();

        return imageContentSource;
    }

    public static async Task<string> Song(IDsContext context, string? songName = null)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
        string audioContentSource = await GetContentSource(db, songName, "audio/mpeg");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Leave(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
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
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
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
        List<string> sourceList = db.Contents.Where(c => c.ContentTypeId == 4)
            .Skip(page * 10 - 10)
            .Take(10)
            .Select(c => c.ContentSource)
            .ToList();

        StringBuilder sb = new StringBuilder()
            .Append(page)
            .Append('/')
            .Append(pageAmount)
            .Append(':')
            .AppendLine();

        for (int i = 0; i < sourceList.Count; i++)
        {
            string source = sourceList[i];
            int lastIndexOfSlash = source.LastIndexOf('/');
            int lastIndexOfDot = source.LastIndexOf('.');

            sb.Append(i + 1)
                .Append('.')
                .Append(' ')
                .Append(source.AsSpan(lastIndexOfSlash + 1, lastIndexOfDot - lastIndexOfSlash - 1));

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
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
        string audioContentSource = await GetContentSource(db, "harosh", "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Megaharosh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
        string audioContentSource = await GetContentSource(db, "megaharosh", "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> ChelHarosh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
        string audioContentSource = await GetContentSource(db, "chelharosh", "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Ahuitelen(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
        string audioContentSource = await GetContentSource(db, "ahuitelen", "phrases");

        return await SetSong(context, audioContentSource);
    }
    
    public static async Task<string> Ploh(IDsContext context)
    {
        await using DataContext db = _dbContextAccessor!.ResolveContext<DataContext>();
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();
        
        string audioContentSource = await GetContentSource(db, "ploh", "phrases");

        return await SetSong(context, audioContentSource);
    }

    private static async Task<User?> PreparingToExecuteCommand(IDsMessage message, DataContext db)
    {
        User? user = await db.Users.FindAsync(message.User.Id);

        if (user is not null)
        {
            CheckUsernameCondition(message.User.Username, db, user);
        }
        else
        {
            user = CreateUser(message.User, db);
        }
                
        // if (user.Banned)
        // {
        //     await message.Channel.SendMessageAsync("Зачилься другалек"); //todo блять он пишет это но ставит все равно
        //             
        //     return null;
        // }
        
        await db.Entry(user).Collection(u => u.Messages).LoadAsync();
        if (user.Messages.Any() && user.Messages.Count > 4)
        {
            DateTime last = DateTime.Now + new TimeSpan(3, 0, 0);
            DateTime lastButFive = user.Messages.OrderByDescending(m => m.SentDate).Select(m => m.SentDate).Skip(4).First();
            // if (await BanIfSpamming(lastButFive, last, user, db))
            // {
            //     await message.Channel.SendMessageAsync("Зачилься другалек");
            //             
            //     return null;
            // }
        }
        
        db.Messages.Add(new Message
        {
            MessageText = message.MessageText,
            SentDate = DateTime.Now,
            UserId = message.User.Id
        });

        return user;
    }
    
    private static async Task<bool> BanIfSpamming(DateTime earlier, DateTime later, User user, DataContext db)
    {
        long ticks = later.Subtract(earlier).Ticks;
        double totalSeconds = TimeSpan.FromTicks(ticks).TotalSeconds;
        
        if (totalSeconds < 15)
        {
            user.BannedUntill = DateTime.Now.AddMinutes(5);
            
            await db.SaveChangesAsync();
                
            return true;
        }
        
        return false;
    }

    private static void CheckUsernameCondition(string newUsername, DataContext db, User user)
    {
        UsernameCondition actualCondition = db.UsernameConditions.Where(uc => uc.UserId == user.Id)
            .OrderByDescending(uc => uc.Queue)
            .First();

        if (newUsername != actualCondition.Username)
        {
            db.UsernameConditions.Add(new UsernameCondition
            {
                User = user,
                Username = newUsername,
                Queue = actualCondition.Queue + 1
            });
        }
    }
    
    private static User CreateUser(SocketUser author, DataContext db)
    {
        User user = new User
        {
            Id = author.Id,
            FirstMessageDate = DateTime.Now
        };

        db.Users.Add(user);

        db.UsernameConditions.Add(new UsernameCondition
        {
            User = user,
            Username = author.Username,
            Queue = 1
        });
            
        return user;
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

        await db.SaveChangesAsync();
    }

    // private static async Task AddTenorGif(string tenorLink, User user, DataContext db)
    // {
    //     await SetContent(db, user, tenorLink, "image/gif");
    // }

    private static async Task AddAttachments(IReadOnlyCollection<IAttachment> attachments, User user, DataContext db)
    {
        foreach (IAttachment attachment in attachments)
        {
            await SetContent(db, user, attachment.Url, attachment.ContentType);
        }
    }
    
    private static async Task SetContent(DataContext db, User user, string contentContent, string contentTypeName)
    {
        ContentType? contentType =
            await db.ContentTypes.FirstOrDefaultAsync(ct => ct.Name == contentTypeName);

        if (contentType is null)
        {
            throw new Exception("Content type has not been added yet");
        }

        db.Contents.Add(new Content
        {
            User = user,
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
        
    private static async Task<string> GetContentSource(DataContext db, string? contentName = null, params string[] contentNames)
    {
        List<int> contentTypeIds = db.ContentTypes.Where(ct => contentNames.Contains(ct.Name)).Select(ct => ct.Id).ToList();
        
        if (contentTypeIds is null || !contentTypeIds.Any())
        {
            throw new Exception("Нет таких контент тайпов лох");
        }
        
        IQueryable<Content> typedContents = db.Contents.Where(c => contentTypeIds.Contains(c.ContentTypeId));

        if (contentName is not null)
        {
            typedContents = typedContents.Where(c => c.ContentSource.EndsWith("/" + contentName + ".mp3"));
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
        await PreparingToExecuteCommand(context.Message, db);
        await db.SaveChangesAsync();

        // return await SetSong(db, context, audiContentSource);
        return "";
    }

    private static async Task<string> SetSong(IDsContext context, string audiContentSource)
    {
        IGuildUser? guildUser = context.User as IGuildUser;
        IAudioClient? audioClient;
        IVoiceChannel? voiceChannel = guildUser?.VoiceChannel;

        if (voiceChannel is null)
        {
            return "Ты не в войсе";
        }

        VoiceChannelStatus voiceChannelStatus = GuildsHelper.GetOrSet(context.Guild.Id);

        bool isChanged = voiceChannelStatus.FindAndChangeActualChannel(voiceChannel.Id);

        if (isChanged)
        {
            audioClient = await voiceChannel.ConnectAsync();
            PlayingService.ChangeAudioClient(audioClient);
            voiceChannelStatus.ChannelsClient.AddOrChangeValue(voiceChannel.Id, audioClient);
        }
        else
        {
            audioClient = voiceChannelStatus.ChannelsClient[voiceChannel.Id];
        }
        
        PlayingService.Queue.Enqueue(audiContentSource);
        
        Task.Run(() => PlayingService.ForcePlay());
        
        return POSTAVIL;
    }
}