using System.Collections.Generic;

namespace DiscordBot.Helpers;

public static class GuildsHelper
{
    public static Dictionary<ulong, VoiceChannelStatus> GuildsChannels { get; } = new();

    public static VoiceChannelStatus GetOrSet(ulong guildId)
    {
        if (GuildsChannels.TryGetValue(guildId, out VoiceChannelStatus? channel))
        {
            return channel;
        }
        
        channel = new VoiceChannelStatus();
            
        GuildsChannels.Add(guildId, channel);

        return channel;
    }
}