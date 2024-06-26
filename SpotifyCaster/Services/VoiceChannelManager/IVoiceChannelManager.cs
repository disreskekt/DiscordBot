using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace SpotifyCaster.Services.VoiceChannelManager;

public interface IVoiceChannelManager
{
    public Task<IAudioClient> Enter(ulong guildId, IVoiceChannel voiceChannel);
    public Task Leave(ulong guildId, IVoiceChannel voiceChannel);
    public IVoiceChannel? GetVoiceChannel(SocketUser user);
}