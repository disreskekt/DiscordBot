using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Audio;
using Microsoft.Extensions.Options;
using Mp3Player.Configs;
using Mp3Player.Models;
using Mp3Player.Services.Interfaces;
using NAudio.Wave;

namespace Mp3Player.Services;

public class PlayingService : IPlayingService
{
    private readonly FileSystemConfig _fileSystemConfig;
    private static readonly Dictionary<ulong, PlayingSession> _playingSessions = new Dictionary<ulong, PlayingSession>();

    public PlayingService(IOptions<FileSystemConfig> fileSystemConfig)
    {
        _fileSystemConfig = fileSystemConfig.Value;
    }
    
    public void CreateSession(ulong channelId, IAudioClient audioClient)
    {
        if (_playingSessions.TryAdd(channelId, new PlayingSession(audioClient, channelId)))
        {
            return;
        }
        
        throw new Exception("Не получилось создать сессию");
    }
    
    public void AddToQueue(ulong channelId, string songName)
    {
        PlayingSession playingSession = GetPlayingSession(channelId);
        
        playingSession.Enqueue(playingSession, songName, async ps => await TryToPlay(ps));
    }
    
    public bool IsSessionExist(ulong channelId)
    {
        return _playingSessions.TryGetValue(channelId, out PlayingSession? _);
    }
    
    public void Skip(ulong channelId)
    {
        PlayingSession playingSession = GetPlayingSession(channelId);
        
        playingSession.Skip = true;
    }

    public void Stop(ulong channelId)
    {
        PlayingSession playingSession = GetPlayingSession(channelId);
        
        playingSession.Stop = true;
    }
    
    public string[] GetQueue(ulong channelId)
    {
        PlayingSession playingSession = GetPlayingSession(channelId);
        
        return playingSession.GetSongs();
    }
    
    private async Task TryToPlay(PlayingSession playingSession)
    {
        if (!playingSession.PlayingStatus)
        {
            await Play(playingSession);
        }
    }
    
    private PlayingSession GetPlayingSession(ulong channelId)
    {
        if (_playingSessions.TryGetValue(channelId, out PlayingSession? value))
        {
            return value;
        }
        
        throw new Exception("PlayingSession has not found");
    }
    
    private async Task Play(PlayingSession playingSession)
    {
        //todo hanlde when use old menu and song name changed
        string songSource = playingSession.Dequeue();
        string musicPath = _fileSystemConfig.MusicFolderPath + '\\' + songSource + ".mp3";
        
        WaveFormat waveFormat = new WaveFormat(48000, 16, 2);
        // MemoryStream memoryStream = await GetSongStream(songSource);
        Mp3FileReader mp3FileReader = new Mp3FileReader(musicPath);
        MediaFoundationResampler resampler = new MediaFoundationResampler(mp3FileReader, waveFormat);
        
        resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
        int blockSize = waveFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
        byte[] buffer = new byte[blockSize];
        
        AudioOutStream? targetStream = playingSession.AudioClient.CreatePCMStream(AudioApplication.Mixed);
        playingSession.PlayingStatus = true;

        try
        {
            int byteCount;
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

                if (playingSession.Skip)
                {
                    playingSession.Skip = false;
                    break;
                }

                if (playingSession.Stop)
                {
                    _playingSessions.Remove(playingSession.Id);
                    return;
                }
                //todo stop and pause
            }

            playingSession.PlayingStatus = false;
        }
        catch (Exception)
        {
            //to skip exception when /Leave is executed
        }
        finally
        {
            resampler.Dispose();
            await targetStream.DisposeAsync();
            await mp3FileReader.DisposeAsync();
            // await memoryStream.DisposeAsync();
        }
        
        if (playingSession.QueueAny())
        {
            await Play(playingSession);
        }
    }
}