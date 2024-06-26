namespace SpotifyCaster.Configs;

public sealed record DiscordConfig
{
    public string Token { get; init; } = string.Empty;
    public ulong[] Guilds { get; init; } = [];
}