using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UsernameCondition> UsernameConditions { get; set; }
    public DbSet<ContentType> ContentTypes { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    // {
    //     options.UseNpgsql("Host=localhost;Port=5432;Database=DsDb;Username=postgres;Password=wronghousefool");
    // }
}