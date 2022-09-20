namespace DiscordBot.Models;

public class UsernameCondition
{
    public int Id { get; set; }
    
    public ulong UserId { get; set; }
    public User User { get; set; }
    
    public string Username { get; set; }
    public int Queue { get; set; }
}