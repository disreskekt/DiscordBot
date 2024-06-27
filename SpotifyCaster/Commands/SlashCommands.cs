using Discord;
using Discord.Audio;
using Discord.Interactions;
using DiscordBotCore.Services;
using SpotifyCaster.Services.AudioStreamer;
using SpotifyCaster.Services.VoiceChannelManager;

namespace SpotifyCaster.Commands;

//DO NOT USE CAMEL CASE IN PARAM NAMES
public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IVoiceChannelService _voiceChannelService;
    private readonly IVoiceChannelManager _voiceChannelManager;
    private readonly IAudioStreamer _audioStreamer;

    public SlashCommands(
        IVoiceChannelService voiceChannelService,
        IVoiceChannelManager voiceChannelManager,
        IAudioStreamer audioStreamer)
    {
        _voiceChannelService = voiceChannelService;
        _voiceChannelManager = voiceChannelManager;
        _audioStreamer = audioStreamer;
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

            _audioStreamer.Start(audioClient);

            await Context.Interaction.RespondAsync("Запустил");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    [SlashCommand("test", "Test")]
    public async Task Test()
    {
        try
        {
            _audioStreamer.Stop();

            await Context.Interaction.RespondAsync("Stopnul");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}