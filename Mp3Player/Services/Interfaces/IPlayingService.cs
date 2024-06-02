using Discord.Audio;

namespace Mp3Player.Services.Interfaces;

public interface IPlayingService
{
    public void CreateSession(ulong channelId, IAudioClient audioClient);
    public void AddToQueue(ulong channelId, string songName);
    public bool IsSessionExist(ulong channelId);
    public void Skip(ulong channelId);
    public void Stop(ulong channelId);
    public string[] GetQueue(ulong channelId);
}