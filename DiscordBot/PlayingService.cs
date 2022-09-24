using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Audio;
using NAudio.Wave;

namespace DiscordBot;

public static class PlayingService
{
    private const string SOUNDS_PATH = @"C:\Users\disre\Desktop\ds_bot\Sounds\";
    public static Queue<string> Queue { get; } = new();
    private static AudioOutStream? TargetStream { get; set; }
    public static bool PlayingStatus { get; set; }
    
    public static void ChangeChannel(IAudioClient audioClient)
    {
        TargetStream = audioClient.CreatePCMStream(AudioApplication.Mixed);
    }

    public static async Task ForcePlay()
    {
        if (!PlayingStatus)
        {
            await Play(Queue.Dequeue());
        }
    }
    
    public static async Task Play(string songName)
    {
        Mp3FileReader mp3FileReader = GetSongStream(songName);
        
        try
        {
            PlayingStatus = true;
            await mp3FileReader.CopyToAsync(TargetStream, 1024);
            PlayingStatus = false;
        }
        finally
        {
            await TargetStream.FlushAsync();
            await mp3FileReader.DisposeAsync();
        }

        try
        {
            string nextSongName = Queue.Dequeue();
            await Play(nextSongName);
        }
        catch
        {
            // ignored
        }
    }
    
    private static Mp3FileReader GetSongStream(string songName)
    {
        string soundFullPath = SOUNDS_PATH + songName.ToLowerInvariant() + ".mp3";

        return new Mp3FileReader(soundFullPath);
    }
}