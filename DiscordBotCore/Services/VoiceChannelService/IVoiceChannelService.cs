using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace DiscordBotCore.Services;

public interface IVoiceChannelService
{
    public Task<IAudioClient> Enter(ulong guildId, IVoiceChannel voiceChannel);
    public Task Leave(ulong guildId, IVoiceChannel voiceChannel);
    public IVoiceChannel? GetVoiceChannel(SocketUser user);
}