using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    class Program
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigurationRoot _appConfig;
        
        public Program()
        {
            _appConfig = BuildConfig();
            
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            DiscordSocketConfig config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };

            CommandServiceConfig commandConfig = new CommandServiceConfig
            {
                SeparatorChar = '-'
            };

            _serviceProvider = new ServiceCollection()
                .AddSingleton<DiscordSocketConfig>(_ => config)
                .AddSingleton<CommandServiceConfig>(_ => commandConfig)
                .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(config))
                .AddSingleton<CommandService>(_ => new CommandService(commandConfig))
                .AddSingleton<InteractionService>()
                .AddSingleton<CommandHandler>()
                .AddDbContext<DataContext>(options => options.UseNpgsql(_appConfig.GetConnectionString("DiscordDb")!))
                .AddSingleton<IDbContextAccessor, ServiceProviderDbContextAccessor>()
                .BuildServiceProvider();
            
            RefreshMusicFolder();
            
            CommandHandler commandHandler = _serviceProvider.GetRequiredService<CommandHandler>();
            ulong[]? guilds = _appConfig.GetSection("Guilds").Get<ulong[]>();
            commandHandler.InstallCommandsAsync(guilds ?? Array.Empty<ulong>()).GetAwaiter();
        }
        
        private void RefreshMusicFolder() //todo refactor
        {
            DataContext db = _serviceProvider.GetRequiredService<DataContext>();

            List<Content> contents = db.Contents.Where(c => c.ContentTypeId == 4).ToList();

            string musicFolderPath = _appConfig.GetSection("MusicFolderPath").Value!;

            if (!Directory.Exists(musicFolderPath))
            {
                Directory.CreateDirectory(musicFolderPath);
            }
            
            string[] fileNames = Directory.GetFiles(musicFolderPath);
            
            foreach (string fileName in fileNames)
            {
                string shortFileName = Path.GetFileName(fileName);

                if (!contents.Select(c => c.ContentSource).Contains(shortFileName))
                {
                    db.Contents.Add(new Content
                    {
                        ContentSource = shortFileName,
                        ContentTypeId = 4
                    });
                }
            }
            
            db.SaveChanges();
        }

        private IConfigurationRoot BuildConfig()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);
            DirectoryInfo targetDirectory = directoryInfo.Parent!.Parent!.Parent!; //todo fix

            return new ConfigurationBuilder()
                .SetBasePath(targetDirectory.ToString())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            DiscordSocketClient client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            
            client.Log += Log;
            
            await client.LoginAsync(TokenType.Bot, _appConfig.GetSection("Token").Value);
            await client.StartAsync();
            
            await Task.Delay(Timeout.Infinite);
        }

        private async Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
        }
    }
}