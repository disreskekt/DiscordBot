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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Mp3Player.Configs;
using Mp3Player.Data;
using Mp3Player.Services;
using Mp3Player.Services.Interfaces;

namespace Mp3Player
{
    class Program
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigurationRoot _config;
        
        public Program()
        {
            _config = BuildConfig();
            
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
                // .AddDbContext<DataContext>(options => options.UseNpgsql(_config.GetConnectionString("DiscordDb")!))
                // .AddSingleton<IDbContextAccessor, DbContextAccessor>()
                .AddSingleton<DiscordSocketConfig>(_ => config)
                .AddSingleton<CommandServiceConfig>(_ => commandConfig)
                .AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(config))
                .AddSingleton<CommandService>(_ => new CommandService(commandConfig))
                .AddSingleton<InteractionService>()
                .AddSingleton<CommandHandler>()
                .AddScoped<IPlayingService, PlayingService>()
                .AddScoped<IVoiceChannelManager, VoiceChannelManager>()
                .AddScoped<IFileSystemService, FileSystemService>()
                .AddScoped<IResponseService, ResponseService>()
                .AddScoped<ISearchService, SearchService>()
                .Configure<FileSystemConfig>(opt => _config.GetSection(nameof(FileSystemConfig)).Bind(opt))
                .BuildServiceProvider();
            
            // RefreshMusicFolder();
            
            CommandHandler commandHandler = _serviceProvider.GetRequiredService<CommandHandler>();
            ulong[]? guilds = _config.GetSection("Guilds").Get<ulong[]>();
            commandHandler.InstallCommandsAsync(guilds ?? Array.Empty<ulong>()).GetAwaiter();
        }
        
        // private void RefreshMusicFolder() //todo refactor
        // {
        //     DataContext db = _serviceProvider.GetRequiredService<DataContext>();
        //
        //     List<Content> contents = db.Contents.Where(c => c.ContentTypeId == 4).ToList();
        //
        //     string musicFolderPath = _config.GetSection("MusicFolderPath").Value!;
        //
        //     if (!Directory.Exists(musicFolderPath))
        //     {
        //         Directory.CreateDirectory(musicFolderPath);
        //     }
        //     
        //     string[] fileNames = Directory.GetFiles(musicFolderPath);
        //     
        //     foreach (string fileName in fileNames)
        //     {
        //         string shortFileName = Path.GetFileName(fileName);
        //
        //         if (!contents.Select(c => c.ContentSource).Contains(shortFileName))
        //         {
        //             db.Contents.Add(new Content
        //             {
        //                 ContentSource = shortFileName,
        //                 ContentTypeId = 4
        //             });
        //         }
        //     }
        //     
        //     db.SaveChanges();
        // }

        private IConfigurationRoot BuildConfig()
        {
            JsonConfigurationSource configurationSource = new JsonConfigurationSource
            {
                Path = Directory.GetCurrentDirectory() + "/appsettings.json",
                ReloadOnChange = true,
                OnLoadException = _ => Console.WriteLine("OnLoadException")
            };
            configurationSource.ResolveFileProvider();
            
            return new ConfigurationBuilder()
                .Add(configurationSource)
                .Build();
        }

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            DiscordSocketClient client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            
            client.Log += Log;
            
            await client.LoginAsync(TokenType.Bot, _config.GetSection("Token").Value);
            await client.StartAsync();
            
            await Task.Delay(Timeout.Infinite);
        }

        private async Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
        }
    }
}