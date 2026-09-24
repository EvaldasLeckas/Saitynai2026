using Microsoft.EntityFrameworkCore;
using Saitynai.Models;

public class AppDbContext : DbContext
{
    private IConfiguration _configuration;

    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<UserResult> UserResults { get; set; }

    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString(("PostgreSQL")));
    }


}