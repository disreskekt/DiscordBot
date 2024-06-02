using Microsoft.EntityFrameworkCore;

namespace Mp3Player.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    // uncomment it to apply migrations
    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    // {
    //     options.UseNpgsql("Host=localhost;Port=5432;Database=DsDb;Username=postgres;Password=password");
    // }
}