using System.Collections.Concurrent;
using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace SpotifyCaster.Services.VoiceChannelManager;

public class VoiceChannelManager : IVoiceChannelManager
{
    private static readonly ConcurrentDictionary<ulong, ulong> s_guildsToChannels = new();
    
    public async Task<IAudioClient> Enter(ulong guildId, IVoiceChannel voiceChannel)
    {
        if (!s_guildsToChannels.TryAdd(guildId, voiceChannel.Id))
        {
            throw new Exception("Я уже в войсе чел");
        }
        
        try
        {
            return await voiceChannel.ConnectAsync();
        }
        catch (Exception e)
        {
            s_guildsToChannels.Remove(guildId, out ulong _);
            
            Console.WriteLine(e);
            throw;
        }
    }
    
    //todo auto leave after a few minutes
    public async Task Leave(ulong guildId, IVoiceChannel voiceChannel)
    {
        if (!s_guildsToChannels.Remove(guildId, out ulong channelId))
        {
            throw new Exception("Да вроде не в войсе");
        }
        
        try
        {
            await voiceChannel.DisconnectAsync();
        }
        catch (Exception e)
        {
            s_guildsToChannels.TryAdd(guildId, channelId);
            
            Console.WriteLine(e);
            throw;
        }
    }
    
    public IVoiceChannel? GetVoiceChannel(SocketUser user)
    {
        IGuildUser? guildUser = user as IGuildUser;
        IVoiceChannel? voiceChannel = guildUser?.VoiceChannel;
        
        return voiceChannel;
    }
}