using Discord;
using Discord.Audio;
using Discord.Interactions;
using DiscordBotCore.Services;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyCaster.Configs;
using SpotifyCaster.Services;
using SpotifyCaster.Services.VoiceChannelManager;

namespace SpotifyCaster.Commands;

//DO NOT USE CAMEL CASE IN PARAM NAMES
public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly SpotifyService _spotifyService;
    private readonly IVoiceChannelService _voiceChannelService;
    private readonly IVoiceChannelManager _voiceChannelManager;
    private readonly SpotifyConfig _spotifyConfig;

    public SlashCommands(
        SpotifyService spotifyService,
        IVoiceChannelService voiceChannelService,
        IOptions<SpotifyConfig> spotifyConfig,
        IVoiceChannelManager voiceChannelManager)
    {
        _spotifyService = spotifyService;
        _voiceChannelService = voiceChannelService;
        _voiceChannelManager = voiceChannelManager;
        _spotifyConfig = spotifyConfig.Value;
    }
    
    [SlashCommand("help", "Объясняет для тупых")]
    public async Task Help()
    {
        try
        {
            await Context.Interaction.RespondAsync("Впадлу помогать");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    [SlashCommand("authorize", "Авторизовывает")]
    public async Task Authorize() 
    {
        try
        {
            LoginRequest loginRequest = new(
                new Uri("https://localhost:7104/Spotify/Callback"), //todo
                _spotifyConfig.ClientId,
                LoginRequest.ResponseType.Code
            )
            {
                Scope = new[]
                {
                    Scopes.Streaming,
                    Scopes.UserReadPlaybackState,
                    Scopes.UserReadCurrentlyPlaying
                }
            };

            string uri = loginRequest.ToUri().ToString();

            await Context.Interaction.RespondAsync(uri);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    [SlashCommand("connect", "Подключает")]
    public async Task Connect()
    {
        try
        {
            IVoiceChannel? voiceChannel = _voiceChannelService.GetVoiceChannel(Context.User);

            if (voiceChannel is null)
            {
                await Context.Interaction.RespondAsync("Ты не в войсе чмошьнек");
                return;
            }

            IAudioClient audioClient = await _voiceChannelManager.Enter(Context.Guild.Id, voiceChannel);

            SpotifyClient? spotifyClient = _spotifyService.SpotifyClient;

            if (spotifyClient is null)
            {
                await Context.Interaction.RespondAsync("Надо авторизоваться блина");
                return;
            }

            CurrentlyPlayingContext playingContext = await spotifyClient.Player.GetCurrentPlayback();

            if (playingContext?.Item == null)
            {
                await Context.Interaction.RespondAsync("Не играет музыка в спотике щас");
                return;
            }

            string trackUri = ((FullTrack) playingContext.Item).Uri;
            // string trackUrl = GetTrackUrl(trackUri); // Создайте метод для получения URL трека по его URI
            //todo
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}