using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        public DbSet<Responsible> Responsibles { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<Integration> Integrations { get; set; }
        public DbSet<ApplicationData> Applications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Repo> Repositories { get; set; }
        public DbSet<ServiceData> Services { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<DocumentData> Documents { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

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

