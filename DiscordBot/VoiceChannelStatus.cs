using System.Collections.Generic;
using System.Linq;
using Discord.Audio;

namespace DiscordBot;

public class VoiceChannelStatus
{
    public Dictionary<ulong, bool> IsConnectedTo { get; } = new();
    public Dictionary<ulong, IAudioClient> ChannelsClient { get; } = new();

    public bool FindAndChangeActualChannel(ulong channelId)
    {
        if (IsConnectedTo.TryGetValue(channelId, out bool isConnected))
        {
            if (isConnected)
            {
                return false;
            }
            else
            {
                KeyValuePair<ulong, bool> kvp = IsConnectedTo.FirstOrDefault(kvp => kvp.Value == true);

                if (!kvp.Equals(new KeyValuePair<ulong, bool>()))
                {
                    IsConnectedTo[kvp.Key] = false;
                }
                
                IsConnectedTo[channelId] = true;

                return true;
            }
        }
        else
        {
            KeyValuePair<ulong, bool> kvp = IsConnectedTo.FirstOrDefault(kvp => kvp.Value == true);
            
            if (!kvp.Equals(new KeyValuePair<ulong, bool>()))
            {
                IsConnectedTo[kvp.Key] = false;
            }
            
            IsConnectedTo.Add(channelId, true);

            return true;
        }
    }

    public ulong? FindActualChannel()
    {
        KeyValuePair<ulong, bool> kvp = IsConnectedTo.FirstOrDefault(kvp => kvp.Value == true);
        
        if (!kvp.Equals(new KeyValuePair<ulong, bool>()))
        {
            return kvp.Key;
        }

        return null;
    }

    public void LeaveChannel(ulong channelId)
    {
        IsConnectedTo[channelId] = false;
    }
}