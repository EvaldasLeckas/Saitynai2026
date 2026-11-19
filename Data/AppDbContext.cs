using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Saitynai.Auth;
using Saitynai.Models;

public class AppDbContext : IdentityDbContext<AppUser>
//public class AppDbContext : DbContext

{
    private IConfiguration _configuration;

    //public DbSet<User> Users { get; set; }
    public DbSet<SessionParticipant> SessionParticipants { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<UserResult> UserResults { get; set; }

    public DbSet<SessionLogin> SessionLogin { get; set; }


    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSQL"));
    }
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);   // required, sets up the Identity tables
    //     modelBuilder.Entity<AppUser>().ToTable("Users");
    // }


}