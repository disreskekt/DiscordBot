using Discord.Audio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SpotifyCaster.Services.AudioStreamer;

public class AudioStreamer : IAudioStreamer, IDisposable
{
    private WasapiLoopbackCapture? _wasapi;
    private BufferedWaveProvider? _bufferedWaveProvider;
    private MediaFoundationResampler? _resampler;
    private CancellationTokenSource? _cancellationTokenSource;

    private Task? _streamingTask = Task.CompletedTask;
    private IAudioClient? _audioClient;

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _bufferedWaveProvider?.AddSamples(e.Buffer, 0, e.BytesRecorded);
    }

    public void Start(IAudioClient audioClient)
    {
        const string deviceName = "CABLE";
        MMDeviceCollection? devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
        MMDevice? device = devices.FirstOrDefault(d => d.FriendlyName.Contains(deviceName));
        if (device == null)
        {
            throw new ArgumentException($"Устройство с именем {deviceName} не найдено.");
        }

        _wasapi = new WasapiLoopbackCapture(device);

        _bufferedWaveProvider = new BufferedWaveProvider(_wasapi.WaveFormat);
        _resampler = new MediaFoundationResampler(_bufferedWaveProvider, new WaveFormat(48000, 16, 2))
        {
            ResamplerQuality = 60
        };

        _audioClient = audioClient;

        _wasapi.DataAvailable += OnDataAvailable;
        _wasapi.StartRecording();

        _cancellationTokenSource = new CancellationTokenSource();
        _streamingTask = Task.Run(StreamAudioAsync, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        _wasapi?.StopRecording();
        _cancellationTokenSource?.Cancel();
        _streamingTask?.Wait();
    }

    private async Task StreamAudioAsync()
    {
        AudioOutStream targetStream = _audioClient!.CreatePCMStream(AudioApplication.Mixed);

        const int blockSize = 8192;
        byte[] buffer = new byte[blockSize];
        while (!_cancellationTokenSource!.Token.IsCancellationRequested)
        {
            int bytesRead = _resampler!.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                if (bytesRead < blockSize)
                {
                    // Incomplete Frame
                    Array.Clear(buffer, bytesRead, blockSize - bytesRead);
                }
                
                await SendAudioToDiscordAsync(targetStream, buffer, bytesRead);
            }
            else
            {
                await Task.Delay(10);
            }
        }
    }

    private static async Task SendAudioToDiscordAsync(AudioOutStream targetStream, byte[] buffer, int bytesRead)
    {
        try
        {
            await targetStream.WriteAsync(buffer, 0, bytesRead); // Send the buffer to Discord
        }
        catch (Exception e)
        {
            Console.WriteLine($"Метод SendAudioToDiscordAsync отвалился по причине: {e.Message}");
            await targetStream.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _wasapi?.Dispose();
        _resampler?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}