using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    class Program
    {
        private const string CONNECTION_STRING = "connection_string";
        private const string TOKEN = "token";

        private readonly IServiceProvider _serviceProvider;
        
        public Program()
        {
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
                .AddDbContext<DataContext>(options => options.UseNpgsql(CONNECTION_STRING))
                .AddSingleton<IDbContextAccessor, ServiceProviderDbContextAccessor>()
                .BuildServiceProvider();

            CommandHandler commandHandler = _serviceProvider.GetRequiredService<CommandHandler>();
            commandHandler.InstallCommandsAsync().GetAwaiter();
        }
        
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            DiscordSocketClient client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            
            client.Log += Log;
            
            await client.LoginAsync(TokenType.Bot, TOKEN);
            await client.StartAsync();

            PublicException.SetClient(client);

            await Task.Delay(Timeout.Infinite);
        }

        private async Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
        }
    }
}