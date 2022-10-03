using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord.Audio;
using NAudio.Wave;

namespace DiscordBot;

public static class PlayingService
{
    public static Queue<string> Queue { get; } = new();
    private static IAudioClient? AudioClient { get; set; }
    public static bool PlayingStatus { get; set; }
    
    public static void ChangeAudioClient(IAudioClient audioClient)
    {
        AudioClient = audioClient;
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
        WaveFormat waveFormat = new WaveFormat(48000, 16, 2);
        MemoryStream memoryStream = await GetSongStream(songSource);
        Mp3FileReader mp3FileReader = new Mp3FileReader(memoryStream);
        MediaFoundationResampler resampler = new MediaFoundationResampler(mp3FileReader, waveFormat);
        
        resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
        int blockSize = waveFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
        byte[] buffer = new byte[blockSize];
        int byteCount;
        
        AudioOutStream? targetStream = AudioClient.CreatePCMStream(AudioApplication.Mixed);
        PlayingStatus = true;
        
        try
        {
            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
            {
                if (byteCount < blockSize)
                {
                    // Incomplete Frame
                    for (int i = byteCount; i < blockSize; i++)
                    {
                        buffer[i] = 0;
                    }
                }
                
                await targetStream.WriteAsync(buffer, 0, blockSize); // Send the buffer to Discord
            }

            PlayingStatus = false;
        }
        finally
        {
            await targetStream.DisposeAsync();
            await mp3FileReader.DisposeAsync();
            await memoryStream.DisposeAsync();
        }

        if (Queue.Count > 0)
        {
            string nextSongName = Queue.Dequeue();
            await Play(nextSongName);
        }
    }
    
    private static async Task<MemoryStream> GetSongStream(string songSource)
    {
        using (WebClient client = new WebClient())
        {
            byte[] downloadData = client.DownloadData(songSource);

            MemoryStream memoryStream = new MemoryStream(downloadData);
            
            return memoryStream;
        }
    }
}