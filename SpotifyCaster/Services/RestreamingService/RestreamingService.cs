using Discord.Audio;
using NAudio.Wave;

namespace SpotifyCaster.Services.RestreamingService;

[Obsolete]
public class RestreamingService : IRestreamingService
{
    private readonly IAudioClient _audioClient;

    public RestreamingService(IAudioClient audioClient)
    {
        _audioClient = audioClient;
    }

    public async Task StreamTrackToDiscordAsync(string trackUrl)
    {
        byte[] trackData = await DownloadTrackAsync(trackUrl);

        using MemoryStream memoryStream = new MemoryStream(trackData);
        using Mp3FileReader mp3FileReader = new Mp3FileReader(memoryStream);
        using MediaFoundationResampler resampler = new MediaFoundationResampler(mp3FileReader, new WaveFormat(48000, 16, 2))
        {
            ResamplerQuality = 60
        };

        int blockSize = 3840; // Размер буфера аудио
        byte[] buffer = new byte[blockSize];

        AudioOutStream targetStream = _audioClient.CreatePCMStream(AudioApplication.Mixed);

        try
        {
            int bytesRead;
            while ((bytesRead = resampler.Read(buffer, 0, blockSize)) > 0)
            {
                if (bytesRead < blockSize)
                {
                    for (int i = bytesRead; i < blockSize; i++)
                    {
                        buffer[i] = 0;
                    }
                }

                await targetStream.WriteAsync(buffer, 0, blockSize);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            resampler.Dispose();
            await targetStream.DisposeAsync();
            mp3FileReader.Dispose();
        }
    }

    private async Task<byte[]> DownloadTrackAsync(string trackUrl)
    {
        using HttpClient httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(trackUrl);
    }
}