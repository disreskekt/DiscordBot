using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DiscordBotCore.Helpers;

public static class ConfigurationHelper
{
    public static IConfigurationRoot BuildConfig()
    {
        JsonConfigurationSource configurationSource = new()
        {
            Path = Directory.GetCurrentDirectory() + "/appsettings.json",
            ReloadOnChange = true,
            OnLoadException = _ => Console.WriteLine("Cannot load config file")
        };

        configurationSource.ResolveFileProvider();

        return new ConfigurationBuilder()
            .Add(configurationSource)
            .Build();
    }
}