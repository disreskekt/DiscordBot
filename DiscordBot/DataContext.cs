using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot;

public class DataContext : DbContext
{
    public DbSet<ContentType> ContentTypes { get; set; }
    public DbSet<Content> Contents { get; set; }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    // it uses for migrations
    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    // {
    //     options.UseNpgsql("Host=localhost;Port=5432;Database=DsDb;Username=postgres;Password=wronghousefool");
    // }
}