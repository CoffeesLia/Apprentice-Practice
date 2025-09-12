using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBefore();
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public override int SaveChanges()
        {
            OnBefore(); 

            var result = base.SaveChanges();
            return result;
        }

        public void OnBefore(string? usuario = null)
        {
            var entities = ChangeTracker.Entries()
                .Where(e => e.Entity is ApplicationData &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            foreach (var entity in entities)
            {
                string createdBy = usuario ?? "Unknown";
                if (entity.State == EntityState.Modified)
                {
                    var propriedadesAlteradas = entity.Properties.Where(p => p.IsModified).ToList();
                    var oldValues = propriedadesAlteradas.Select(p => $"{p.Metadata.Name}:{p.OriginalValue?.ToString() ?? ""}").ToList();
                    var newValues = propriedadesAlteradas.Select(p => $"{p.Metadata.Name}:{p.CurrentValue?.ToString() ?? ""}").ToList();

                    var audit = new Audit
                    {
                        Table = entity.Entity.GetType().Name,
                        Action = entity.State.ToString(),
                        OldValues = oldValues,
                        NewValues = newValues,
                        CreatedBy = createdBy,
                        DateTime = DateTime.UtcNow
                    };
                    Audits.Add(audit);
                }
                else if (entity.State == EntityState.Added)
                {
                    var newValues = entity.Properties.Select(p => $"{p.Metadata.Name}:{p.CurrentValue?.ToString() ?? ""}").ToList();

                    var audit = new Audit
                    {
                        Table = entity.Entity.GetType().Name,
                        Action = entity.State.ToString(),
                        OldValues = new List<string>(),
                        NewValues = newValues,
                        CreatedBy = createdBy,
                        DateTime = DateTime.UtcNow
                    };
                    Audits.Add(audit);
                }
                else if (entity.State == EntityState.Deleted)
                {
                    var oldValues = entity.OriginalValues.Properties.Select(p => $"{p.Name}:{entity.OriginalValues[p]}").ToList();

                    var audit = new Audit
                    {
                        Table = entity.Entity.GetType().Name,
                        Action = entity.State.ToString(),
                        OldValues = oldValues,
                        NewValues = new List<string>(),
                        CreatedBy = createdBy,
                        DateTime = DateTime.UtcNow
                    };
                    Audits.Add(audit);
                }
            }
        }


        public DbSet<Responsible> Responsibles { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Audit> Audits { get; set; }
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
        public DbSet<Chat> ChatMessages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Knowledge> Knowledges { get; set; } = null!;

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
