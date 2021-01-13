using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class AppDatabase : DbContext
    {
        public DbSet<DBSport> Sports { get; set; }
        public DbSet<DBLeague> Leagues { get; set; }
        public DbSet<DBEvent> Events { get; set; }
        public DbSet<DBLine> Lines { get; set; }
        public DbSet<DBSince> Sinces { get; set; }
        public DbSet<DBNotify> Notifies { get; set; }
        public DbSet<DBSubscribe> Subscribes { get; set; }

        public AppDatabase(DbContextOptions<AppDatabase> options) : base(options)
        {
            Database.Migrate();
        }

        public AppDatabase()
        {
            Database.Migrate();
        }
    }
}
