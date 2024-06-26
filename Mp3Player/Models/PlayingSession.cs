using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Audio;

namespace Mp3Player.Models;

public class PlayingSession
{
    private readonly Queue<string> _queue;
    public ulong Id { get; set; }
    public bool Skip { get; set; }
    public bool Stop { get; set; }
    public bool PlayingStatus { get; set; }
    public IAudioClient AudioClient { get; set; }
    
    public PlayingSession(IAudioClient audioClient, ulong id)
    {
        _queue = new Queue<string>();
        Id = id;
        Skip = false;
        Stop = false;
        PlayingStatus = false;
        AudioClient = audioClient;
    }
    
    public void Enqueue(PlayingSession playingSession, string songName, Action<PlayingSession> tryToPlayAction)
    {
        _queue.Enqueue(songName);
        
        Task.Run(() => tryToPlayAction(playingSession));
    }
    
    public string Dequeue()
    {
        return _queue.Dequeue();
    }
    
    public bool QueueAny()
    {
        return _queue.Any();
    }
    
    public string[] GetSongs()
    {
        return _queue.ToArray();
    }
}