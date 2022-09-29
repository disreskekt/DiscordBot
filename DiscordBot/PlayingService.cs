using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
        TargetStream = audioClient.CreatePCMStream(AudioApplication.Music);
    }

    public static async Task ForcePlay()
    {
        if (!PlayingStatus)
        {
            await Play(Queue.Dequeue());
        }
    }
    
    public static async Task Play(string songSource)
    {
        (Mp3FileReader mp3FileReader, MemoryStream memoryStream) = await GetSongStream(songSource);
        
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
            await memoryStream.DisposeAsync();
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
    
    private static async Task<(Mp3FileReader, MemoryStream)> GetSongStream(string songSource)
    {
        using (WebClient client = new WebClient())
        {
            byte[] downloadData = client.DownloadData(songSource);

            MemoryStream memoryStream = new MemoryStream(downloadData);
            
            return (new Mp3FileReader(memoryStream), memoryStream);
        }
    }
}