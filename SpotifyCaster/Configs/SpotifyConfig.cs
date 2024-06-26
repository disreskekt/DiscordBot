namespace SpotifyCaster.Configs;

public sealed record SpotifyConfig
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}