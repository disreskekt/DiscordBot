using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Mp3Player.Services.Interfaces;

namespace Mp3Player.Services;

public class VoiceChannelManager : IVoiceChannelManager
{
    private static readonly Dictionary<ulong, ulong> _guildsToChannels = new();
    
    public async Task<IAudioClient> Enter(ulong guildId, IVoiceChannel voiceChannel)
    {
        if (!_guildsToChannels.TryAdd(guildId, voiceChannel.Id))
        {
            throw new Exception("Я уже в войсе чел");
        }
        
        try
        {
            return await voiceChannel.ConnectAsync();
        }
        catch (Exception e)
        {
            _guildsToChannels.Remove(guildId, out ulong _);
            
            Console.WriteLine(e);
            throw;
        }
    }
    
    //todo auto leave after a few minutes
    public async Task Leave(ulong guildId, IVoiceChannel voiceChannel)
    {
        if (!_guildsToChannels.Remove(guildId, out ulong channelId))
        {
            throw new Exception("Да вроде не в войсе");
        }
        
        try
        {
            await voiceChannel.DisconnectAsync();
        }
        catch (Exception e)
        {
            _guildsToChannels.Add(guildId, channelId);
            
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