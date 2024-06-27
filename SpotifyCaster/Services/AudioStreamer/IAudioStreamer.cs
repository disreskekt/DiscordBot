using Discord.Audio;

namespace SpotifyCaster.Services.AudioStreamer;

public interface IAudioStreamer
{
    void Start(IAudioClient audioClient);
    void Stop();
}