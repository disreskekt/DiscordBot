using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models;

public class User
{
    public ulong Id { get; set; }
    public DateTime FirstMessageDate { get; set; }
    
    [NotMapped]
    public bool Banned => DateTime.Now < this.BannedUntill;
    public DateTime BannedUntill { get; set; }
    
    public ICollection<Message> Messages { get; set; }
    public ICollection<UsernameCondition>  UsernameConditions { get; set; }
}