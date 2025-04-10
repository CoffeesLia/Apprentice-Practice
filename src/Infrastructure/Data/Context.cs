using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<Responsible> Responsibles { get; set; } 
        public DbSet<Area> Areas { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<Integration> Integrations { get; set; }
        public DbSet<ApplicationData> ApplicationDatas { get; set; }
        public DbSet<GitRepo> GitRepo { get; set; }
        public DbSet<DataService> Services { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}

