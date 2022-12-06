using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
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
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);
            DirectoryInfo targetDirectory = directoryInfo.Parent!.Parent!.Parent!; //todo fix

            _appConfig =  new ConfigurationBuilder()
                .SetBasePath(targetDirectory.ToString())
                .AddJsonFile("appsettings.json")
                .Build();
            
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

            CommandHandler commandHandler = _serviceProvider.GetRequiredService<CommandHandler>();
            ulong[]? guilds = _appConfig.GetSection("Guilds").Get<ulong[]>();
            commandHandler.InstallCommandsAsync(guilds ?? Array.Empty<ulong>()).GetAwaiter();
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