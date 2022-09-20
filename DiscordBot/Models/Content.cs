using System;

namespace DiscordBot.Models;

public class Content
{
    public int Id { get; set; }
    
    public ulong UserId { get; set; }
    public User User { get; set; }
    
    public int ContentTypeId { get; set; }
    public ContentType ContentType { get; set; }
    
    public string ContentSource { get; set; }
    public DateTime UploadingDate { get; set; }
}