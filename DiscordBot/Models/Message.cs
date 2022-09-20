using System;

namespace DiscordBot.Models;

public class Message
{
    public int Id { get; set; }
    
    public ulong UserId { get; set; }
    public User User { get; set; }
    
    public string MessageText { get; set; }
    public DateTime SentDate { get; set; }
}