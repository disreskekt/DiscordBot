using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot
{
    class Program
    {
        private const string TOKEN = "token";

        private static DiscordSocketClient _client;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            });
            _client.MessageReceived += HandleMessage;
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, TOKEN);
            await _client.StartAsync();

            Console.ReadLine();
        }

        private async Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
        }

        private async Task HandleMessage(SocketMessage message)
        {
            if (!message.Author.IsBot && message.Content.StartsWith('-'))
            {
                await using Context db = new Context();
                
                User? user = await db.Users.FindAsync(message.Author.Id);

                if (user is not null)
                {
                    CheckUsernameCondition(message.Author.Username, db, user);
                }
                else
                {
                    user = CreateUser(message.Author, db);
                }
                
                if (user.Banned)
                {
                    await message.Channel.SendMessageAsync("Зачилься другалек");
                    
                    return;
                }
                
                await db.Entry(user).Collection(u => u.Messages).LoadAsync();
                if (user.Messages.Any() && user.Messages.Count > 4)
                {
                    DateTime last = user.Messages.OrderByDescending(m => m.SentDate).Select(m => m.SentDate).First();
                    DateTime lastButFive = user.Messages.OrderByDescending(m => m.SentDate).Select(m => m.SentDate).Last();
                    BanIfSpamming(lastButFive, last, user);
                }
                
                string trimmedMessage = message.Content.TrimStart('-');

                if (String.IsNullOrWhiteSpace(trimmedMessage))
                {
                    await message.Channel.SendMessageAsync("Ну может ты что-нибудь напишешь?");

                    return;
                }
                
                string text = trimmedMessage.ToLower();

                db.Messages.Add(new Message
                {
                    MessageText = trimmedMessage,
                    SentDate = DateTime.Now,
                    UserId = message.Author.Id
                });
                
                switch (text)
                {
                    case not null when text.StartsWith("add htttps://tenor.com/view/"):
                        int indexOfWhitespace = text.IndexOf(' ');
                        string tenorLink = text.Substring(indexOfWhitespace + 1);
                        await AddTenorGif(tenorLink, user, db);
                        await SavedAnswer(message.Channel);
                        break;
                    case "add" when message.Attachments.Count > 0:
                        await AddNewContentTypes(message.Attachments, db);
                        await AddAttachments(message.Attachments, user, db);
                        await SavedAnswer(message.Channel);
                        break;
                    case "add":
                        await message.Channel.SendMessageAsync("Ну может ты что-нибудь прикрепишь?");
                        break;
                }
                
                await db.SaveChangesAsync();
            }
        }

        private void BanIfSpamming(DateTime earlier, DateTime later, User user)
        {
            if (later.Subtract(earlier).Seconds < 15)
            {
                user.BannedUntill = DateTime.Now.AddMinutes(5);
            }
        }

        private static void CheckUsernameCondition(string newUsername, Context db, User user)
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

        private static async Task SavedAnswer(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync("Сохранил");
        }

        private async Task AddNewContentTypes(IReadOnlyCollection<Attachment> attachments, Context db)
        {
            foreach (Attachment attachment in attachments)
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

        private async Task AddTenorGif(string tenorLink, User user, Context db)
        {
            await SetContent(db, user, tenorLink, "tenorGif");
        }

        private async Task AddAttachments(IReadOnlyCollection<Attachment> attachments, User user, Context db)
        {
            foreach (Attachment attachment in attachments)
            {
                await SetContent(db, user, attachment.Url, attachment.ContentType);
            }
        }

        private async Task SetContent(Context db, User user, string contentContent, string contentTypeName)
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

        private static User CreateUser(SocketUser author, Context db)
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
    }
}