using CSharp_Interactive_Learning_App.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharp_Interactive_Learning_App.API.DbContexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Battle> Battles { get; set; } 
        public DbSet<BattleState> BattleStates { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
